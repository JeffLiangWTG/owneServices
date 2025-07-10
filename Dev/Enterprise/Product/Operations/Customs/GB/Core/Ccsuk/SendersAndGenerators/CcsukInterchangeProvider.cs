using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.Generic;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.Declaration
{
	class CcsukInterchangeProvider : GbInterchangeProvider
	{
		public CcsukInterchangeProvider(NonDependentEDIMessageCollection messages, ILogger serviceLogger)
			: base(messages, serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		protected override void UpdateQueuedMessageToPendingAndPerformAndOtherUpdatesAfterSpoolingMessageIntoInterchange(EDIMessage message)
		{
			base.UpdateQueuedMessageToPendingAndPerformAndOtherUpdatesAfterSpoolingMessageIntoInterchange(message);
			var hawb = message.EM_LinkedObject as CusHAWB;
			if (hawb != null)
			{
				using (hawb?.SuspendCalculateNprFromReceipts())
				using (hawb?.MAWB?.SuspendCalculateNprFromReceipts())
				{
					CcsukUtilities.UpdatePresenceToPendingIfRelevant(hawb);
				}
			}
		}

		public override string ApplicationCode
		{
			get { return ApplicationCodeList.Codes.GbCcsuk; }
		}

		public override void PrepareInterchangeProperties(EDIInterchange interchange, EDIMessage message)
		{
			bool failed = false;
			// We want all CCSUK messages to share a fountain, not have one per recipient. Otherwise the first Genral to recipient ABC will have number 1, then the first to (bad) recipient XXX will have 1, and the bounce back for that later message will refer to interchange "1".  Ambiguous. 
			interchange.NumberStrategy = new GbInterchangeNumberStrategy(interchange.Factory, ApplicationCode);
			interchange.EI_ApplicationCode = ApplicationCode;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_To = GetRecipientPima(message);
			interchange.EI_From = GetPimaForAgentOrShed(message, ref failed).Left(interchange.EI_FromInfo.MaxLength);
			bool isTraining = interchange.ContainedMessages[0].EM_IsTestMessage;  // Only one message per interchange, as set by the CcsukInterchangeSender

			if (failed)
			{
				interchange.ContainedMessages.Remove(message);
				interchange.Delete();
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				return;
			}

			// Should look like this:  UNB+UNOA:2+CUKFFW98000DVL:IATA:1489+CUKCTM98CHFIMP:IATA+041118:1120+1489
			UNBSegment unb = GetUNB(ZDateTime.Now, interchange.EI_To, "IATA", interchange.EI_From, "IATA", "", "UNOA", "2", "", false, isTraining, "");
			interchange.EI_HeaderText = IsCDSMessage(message) ? string.Empty : unb.ToString(CurrentUNCharacterSet);
			interchange.EI_BodyText = message.EM_MessageText; // Only one message per interchange, as set by the CcsukInterchangeSender
		}

		ZBool IsCDSMessage(EDIMessage message) => string.Equals(message?.EM_ApplicationCode ?? "", ApplicationCodeList.Codes.GbCDSViaCCSUK, StringComparison.OrdinalIgnoreCase);

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			var result = ZString.Empty;
			var message = messages.FirstOrDefault() as EDIMessage;
			if (message != null)
			{
				result = IsCDSMessage(message) ? (ZString)(GBCustomsDataRegistry.Instance.GetCdsCcsukProcessingInstruction((message.EM_LinkedObject as Customs.Business.CusEntryHeader)?.IsImport ?? true, interchange.EI_From, interchange.PK.ToAlphanumericOnlyString())) : base.GetFooterText(interchange, messages);
			}

			return result;
		}

		ZString GetPimaForAgentOrShed(EDIMessage message, ref bool failed)
		{
			var result = GetTargetRecipient(message); // naked sender PIMA
			if (result.Length == 3)
			{
				var cred = CredentialsSetting.GetCredentialsForBadge(result, message.Branch.Company.PK); // Badge code - old style
				if (cred == null)
				{
					failed = true;
					result = ZString.Empty;

					var errorMessageToLog = FormattableString.Invariant($@"Message {message.EM_MessageNum} could not be packaged into an interchange for devlivery via CCSUK because EM_MessageOwner '{message.EM_MessageOwner}' was invalid. The interchange will be removed and this message marked as failed.");
					serviceLogger.Log(LogType.Warning, errorMessageToLog);
				}
				else
				{
					result = cred.Printer;  // e.g. CUKFFW98000CAR or CUKAIR98LHRCAX
				}
			}
			return result;
		}

		ZString GetRecipientPima(EDIMessage outboundMessage)
		{
			if (IsCDSMessage(outboundMessage))
			{
				ZString pima = string.Empty;
				var entry = outboundMessage.EM_LinkedObject as Customs.Business.CusEntryHeader;
				if (entry != null)
				{
					pima = entry.IsImport ? GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdImport.Value : GBCustomsDataRegistry.Instance.CdsCcsukRecipientIdExport.Value;
				}

				return pima;
			}
			else
			{
				switch (outboundMessage.EM_MessageType)
				{
					case ChiefConstants.CusDecTypeDLU:
						return ChiefConstants.ChiefCcsukPimaPrefix + "IMP";
					case CcsukTransmissionMessageFunction.CUSCAR.Code:
					case CcsukTransmissionMessageFunction.CIM.Code:
					case CcsukTransmissionMessageFunction.CUSDEC.Code:
					case CcsukTransmissionMessageFunction.CONTRL.Code:
					case GenralMessageGenerator.GenralMessageCodeShortForMessageType:   // GENRAL messages can go to various recipients
						return outboundMessage.EM_ApplicationReference;  // e.g. CUK98FFW000ABC

					case CcsukTransmissionMessageFunction.CUKG2G.Code:
						return CcsukEdiMessageDiverter.PimaForCcsukNes;

					case "EAA":
					case "EAL":
					case "EAC":
					case "EDL":
					case "DEC": // for consol-level cusdec/DEC message
						return ChiefConstants.ChiefCcsukPimaPrefix + "EXP";

					case CcsukTransmissionMessageFunction.CUKFSR.Code:
						return (outboundMessage.EM_MessageSubType == CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode)
									? (ZString)CcsukEdiMessageDiverter.PimaForCommunityDatabase
									: outboundMessage.EM_ApplicationReference;

					default:
						return GetPimaForChief(outboundMessage);
				}
			}
		}

		ZString GetPimaForChief(EDIMessage message)
		{
			var entry = message.EM_LinkedObject as EU.Business.Declaration.CusEntryHeader;
			var errorMessageToLog = ZString.Empty;
			if (entry != null)
			{
				var importOrExport = entry.Declaration.JE_MessageType;
				if (importOrExport != MessageTypeList.Codes.Import && importOrExport != MessageTypeList.Codes.Export)
				{
					errorMessageToLog = string.Format("CCSUK package only knows how to send to CHIEF for imports or exports. This message's interchange cannot addressed properly. JE_MessageType={0}; EM_MessageNum={2}; PK={1}", importOrExport, message.PK, message.EM_MessageNum);
				}
				else
				{
					return ChiefConstants.ChiefCcsukPimaPrefix + importOrExport;
				}
			}
			else
			{
				errorMessageToLog = string.Format("CcsukInterchangeProvider doesn't know how to send to the recipient defined for this message type. This message's interchange cannot addressed properly. EM_MessageType={0};EM_MessageNum={3}; EM_Applicationreference={1}; PK={2}", message.EM_MessageType, message.EM_ApplicationReference, message.PK, message.EM_MessageNum);
			}
			ErrorReporter.ReportOnce("BGB-CUK-IntProviderCannotGetRecipientPima", errorMessageToLog);
			serviceLogger.Log(LogType.Warning, errorMessageToLog);
			return ZString.Empty;  // EI_TO will be empty and will be bounced...
		}

		readonly ILogger serviceLogger;
	}
}

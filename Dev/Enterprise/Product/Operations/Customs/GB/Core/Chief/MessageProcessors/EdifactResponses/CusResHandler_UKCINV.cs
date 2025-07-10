using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.Auto;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	/// <summary>
	/// A processor that can understand a UN:4:1 CUSRES message the we received. Pulls out the values and saves them to the CusEntry(Header|Line)
	/// </summary>
	public class CusResHandler_UKCINV : CusResHandler_BASE
	{
		public CusResHandler_UKCINV(ILogger iLogger)
			: base(iLogger)
		{ }

		UkCinvMessage ukCinv;

		protected override SegmentGroup GetEdifactMessage()
		{
			ukCinv = (UkCinvMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(GbEdiMessageFactory.Factory);
			return ukCinv;
		}

		protected override UkResponseMsgIdNumber GetUniqueReferenceNumberFromInboundMessagePreferablySysCar()
		{
			ZString car = ukCinv.UNH[0].CommonAccessReference;
			if (!car.IsEmpty)
			{
				return new UkResponseMsgIdNumber(car, UkResponseMsgIdNumberType.CommonAccessReference);
			}
			else
			{
				if (Understander.HeaderLevelDucr.IsEmpty && !Understander.HeaderLevelMucr.IsEmpty)
				{
					return new UkResponseMsgIdNumber(Understander.HeaderLevelMucr, UkResponseMsgIdNumberType.MasterUCR);
				}
				else if (!Understander.HeaderLevelDucr.IsEmpty)
				{
					return new UkResponseMsgIdNumber(Understander.HeaderLevelDucr, UkResponseMsgIdNumberType.UniqueConsignmentNumberWithPart);
				}
				else if (Understander.DUCRs.Length == 1 && !Understander.DUCRs[0].IsEmpty)  // An ERS without a header-level MUCR will have exactly one DUCR in the body
				{
					return new UkResponseMsgIdNumber(Understander.DUCRs[0], UkResponseMsgIdNumberType.UniqueConsignmentNumberWithPart);
				}
			}
			return null;
		}

		protected override bool IsSuccessfulAcknowledgement
		{
			get
			{
				return true;  // Bad UCR/MUCR results in a CUSRES/27, if we get a UKCINV it means success.  See DES 222 v2.1 page 3-2.
			}
		}

		UkcinvUnderstander understander;
		UkcinvUnderstander Understander
		{
			get
			{
				if (understander == null)
				{
					understander = new UkcinvUnderstander(ukCinv);
				}
				return understander;
			}
		}

		protected override void UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications()
		{
			string emailBody = Understander.GetInterpretation();
			Understander.ParseEmrErsEaaEal(incomingMessage, entry);
			incomingMessage.EM_Status = EDIMessage.Status.Received;
			originalOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			SendEmail(emailBody);
		}

		void SendEmail(string emailBody)
		{
			ZString subject = string.Format("{0} / {1} - CHIEF response ({2})", GetReferenceNumberForEmailAndHtml(), entry.CH_EntryStatus, Understander.Class);
			base.SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, emailBody, gbDeclaration,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates, MessageCode, Guid.Empty, entry.RegistryBranchPK, Guid.Empty),
				GBCustomsDataRegistry.Instance.GetNotificationItem(MessageCode, "", GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates));
		}

		protected override ZString ClassOfInboundMessage
		{
			get { return "UKCINV"; }
		}

		protected override ZString MessageFunction  // Will return M for master or D for declaration
		{
			get { return Understander.LevelOfMessage; }
		}

		protected override ZString MessageCode
		{
			get { return Understander.Class.ToString(); }
		}

		protected override void DoAllProcessingCore(EDIMessage inboundMessage)
		{
			PerformPreProcessingInitialisation(inboundMessage);
			switch (Understander.Class)
			{
				// Unsolicited messages without SYS-CAR
				case UkcinvUnderstander.MessageClasses.EMR:
				case UkcinvUnderstander.MessageClasses.ERS:
					Understander.ParseEmrErsEaaEal(inboundMessage);
					break;

				// Messages might be responses to messages sent from a declaration or a consol
				case UkcinvUnderstander.MessageClasses.EAA:
				case UkcinvUnderstander.MessageClasses.EAL:
				case UkcinvUnderstander.MessageClasses.EDL:
					var pkOfLinkedObject = new ZGuid(new Guid(responseMsgIdNumber.Reference));
					originalOutgoingMessage = factory.Load<EDIMessage>(pkOfLinkedObject);
					if (originalOutgoingMessage != null)
					{
						ProcessReceivedEaaEalEdlMessageFromChiefOnConsol(inboundMessage);
					}
					else
					{
						base.DoAllProcessingCore(inboundMessage);
					}
					break;

				case UkcinvUnderstander.MessageClasses.DEC:
					DoConsolOrEntryDecResponseProcessingForDEC(inboundMessage);
					break;

				default:
					base.DoAllProcessingCore(inboundMessage);
					break;
			}
		}

		void DoConsolOrEntryDecResponseProcessingForDEC(EDIMessage inboundMessage)
		{
			var pkOfLinkedObject = new ZGuid(new Guid(responseMsgIdNumber.Reference));
			originalOutgoingMessage = factory.Load<EDIMessage>(pkOfLinkedObject);
			if (originalOutgoingMessage != null)
			{
				var bizO = originalOutgoingMessage.EM_LinkedObject;
				var consol = bizO as ForwardingConsol;
				if (consol != null)
				{
					ProcessDecResponseOnConsol(consol, inboundMessage);
					return;
				}
			}
			base.DoAllProcessingCore(inboundMessage); // for messages linked to CusEntryHeader
		}

		void ProcessDecResponseOnConsol(ForwardingConsol consol, EDIMessage inboundMessage)
		{
			var wrapper = new ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper(consol, new SendsMessagesToCustomsShutterUpperer(false));
			ProcessConsolLevelDecResponseAndStateWhetherAdsCanNowBeProduced(Understander, inboundMessage, wrapper);
			inboundMessage.EM_Status = EDIMessage.Status.Received;
			originalOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			ChangeDefaultValuesOnIncomingMessageToSomethingUseful();
		}

		// For when the request message was sent at CONSOL level rather than entry level
		void ProcessReceivedEaaEalEdlMessageFromChiefOnConsol(EDIMessage inboundMessage)
		{
			inboundMessage.EM_LinkedObject = originalOutgoingMessage.EM_LinkedObject;
			Understander.ParseEmrErsEaaEal(inboundMessage);
			originalOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			var subject = MessageCode + " response from CHIEF for " + inboundMessage.EM_LinkedObject.HumanReadableName;
			if (inboundMessage.EM_LinkedObject is ForwardingConsol)
			{
				SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, Understander.GetInterpretation(), inboundMessage.EM_LinkedObject, ControllerIDs.JobConsol,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates, MessageCode, inboundMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(MessageCode, "", GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates));
			}
			else if (inboundMessage.EM_LinkedObject is JobDeclaration)
			{
				var dec = ((EU.Business.Declaration.JobDeclaration)inboundMessage.EM_LinkedObject);
				SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, Understander.GetInterpretation(), inboundMessage.EM_LinkedObject, ControllerIDs.Customs.JobDeclaration,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates, MessageCode, Guid.Empty, dec.RegistryBranchPK, Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(MessageCode, "", GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates));
			}
			else if (inboundMessage.EM_LinkedObject is EU.Business.Declaration.CusEntryHeader)
			{
				var dec = ((EU.Business.Declaration.CusEntryHeader)inboundMessage.EM_LinkedObject).Declaration;
				((EU.Business.Declaration.CusEntryHeader)inboundMessage.EM_LinkedObject).CH_Status = IsSuccessfulAcknowledgement ? MessageStatusList.Codes.OK : MessageStatusList.Codes.SentAndRejected;
				SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, Understander.GetInterpretation(), dec, ControllerIDs.Customs.JobDeclaration,
					GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates, MessageCode, Guid.Empty, dec.RegistryBranchPK, Guid.Empty),
					GBCustomsDataRegistry.Instance.GetNotificationItem(MessageCode, "", GBCustomsDataRegistry.Instance.NotificationChiefStatusUpdates));
			}
		}

		void ProcessConsolLevelDecResponseAndStateWhetherAdsCanNowBeProduced(UkcinvUnderstander understander, EDIMessage inboundMessage, ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper wrapper)
		{
			var masterIsNowClosed = !understander.IsMasterOpen;
			var masterWasClosed = wrapper.MawbExportHelper.ME_ChiefConsolIsClosed;
			if (masterIsNowClosed && !masterWasClosed)
			{
				wrapper.MawbExportHelper.ME_Queried = ZDateTime.BrettsBirthday;
				wrapper.MawbExportHelper.ME_ChiefConsolIsClosed = masterIsNowClosed;
			}

			var reasonsWhyAdsSuppressed = new ZStringBuilder();
			var ucrAndPartsThatChiefKNowsAbout = new List<ZString>();
			ucrAndPartsThatChiefKNowsAbout.AddRange(understander.DUCRs);
			ucrAndPartsThatChiefKNowsAbout.AddRange(understander.IntermediateMucrs);
			var nonCDucrsKnownOnConsolLocally = new List<ZString>();
			var nonCStatusShipmentsOnConsol = (from ForwardingShipment s in wrapper.ForwardingConsol.Shipments where s.IsExport() && s.JS_CommunityTransitStatus != ExportCommunityTransitStatusList.Codes.C select s);
			foreach (var shipment in nonCStatusShipmentsOnConsol)
			{
				var gbExportDeclarations = (from BaseJobDeclaration dec in shipment.Declarations where dec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom && dec.IsExport select dec);
				if (!gbExportDeclarations.Any())
				{
					var externalDucrs = (from CusEntryNumber cen in shipment.Numbers where cen.CE_EntryType == CusEntryNumberTypes.Standard.UniqueConsignementReference select cen.CE_EntryNum);
					if (!externalDucrs.Any())
					{
						reasonsWhyAdsSuppressed.Append(string.Format("{0} is not C-status but has no internal or external DUCR", shipment.HumanReadableName));
					}
					else
					{
						nonCDucrsKnownOnConsolLocally.AddRange(externalDucrs.ToArray());
					}
				}
				else
				{
					foreach (var dec in gbExportDeclarations)
					{
						if (dec.ActiveEntryHeaders.Count == 0)
						{
							reasonsWhyAdsSuppressed.Append(string.Format("Declaration {0} for shipment {1} is not C-status but has no entry header. Ensre that entries have been sent to CHIEF.", dec.JE_UCR, dec.JE_DeclarationReference));
						}
						foreach (Customs.Business.CusEntryHeader entry in dec.ActiveEntryHeaders)
						{
							nonCDucrsKnownOnConsolLocally.Add(entry.CH_BGMReference);
						}
					}
				}
			}

			foreach (var chiefDucr in ucrAndPartsThatChiefKNowsAbout)
			{
				if (!nonCDucrsKnownOnConsolLocally.Contains(chiefDucr))
				{
					reasonsWhyAdsSuppressed.Append(string.Format("Declaration {0} is known to CHIEF but not locally.", chiefDucr));
				}
			}

			foreach (var localDucr in nonCDucrsKnownOnConsolLocally)
			{
				if (!ucrAndPartsThatChiefKNowsAbout.Contains(localDucr))
				{
					reasonsWhyAdsSuppressed.Append(string.Format("Declaration {0} is locally tied to the consol but it is not associated properly on CHIEF.", localDucr));
				}
			}

			if (!masterIsNowClosed)
			{
				reasonsWhyAdsSuppressed.Append(string.Format("CHIEF reports that MUCR '{0}' is not closed.", wrapper.MawbExportHelper.ME_MasterUCR));
			}

			var messageInterpretation = understander.GetInterpretation();

			if (reasonsWhyAdsSuppressed.Length == 0)
			{
				wrapper.MawbExportHelper.ME_Queried = ZDateTime.Now;
				messageInterpretation = "<p><b><font color='green'>The ADS can now be printed (if no changes are made to the consol)</font></b></p>" + messageInterpretation;
			}
			else
			{
				wrapper.MawbExportHelper.ME_Queried = ZDateTime.Empty;
				var noAdsAllowedMessage = "<p><b>The ADS cannot be produced for the following reasons:</b></P>";
				noAdsAllowedMessage += "<pre><font color='red'>";
				noAdsAllowedMessage += reasonsWhyAdsSuppressed.ToStringWithNewLineBetweenAppends();
				noAdsAllowedMessage += "</font></pre>";
				messageInterpretation = noAdsAllowedMessage + messageInterpretation;
			}

			inboundMessage.EM_MessageInterpretation = MessagePrettierCss.CSS + messageInterpretation;
			wrapper.ForwardingConsol.Messages.Add(inboundMessage);
		}
	}
}



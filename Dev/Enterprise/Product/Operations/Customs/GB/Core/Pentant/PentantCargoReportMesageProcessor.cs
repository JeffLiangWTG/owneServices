using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Pentant
{
	internal class PentantCargoReportMesageProcessor
	{
		public PentantCargoReportMesageProcessor(ILogger serviceLogger, EDIMessage ediMessage)
		{
			this.serviceLogger = serviceLogger;
			this.ediMessage = ediMessage;
		}

		internal void Process()
		{
			ediMessage.EM_Status = EDIMessage.Status.Received;
			if (ediMessage.Interchange != null)
			{
				ediMessage.Interchange.EI_Status = EDIMessage.Status.Received;
			}
			var elements = ediMessage.EM_MessageText.Split(PentantConstants.SeparatorChar);
			if (elements.Length > 14)
			{
				var messageNumber = elements[1];
				var messageSender = elements[2];
				var importExport = elements[6];
				var aca = elements[11];
				var successOrError = elements[13];
				var mucrOrErrorText = elements[14];

				ediMessage.EM_MessageInterpretation = GetInterpretation(messageNumber, aca, successOrError, mucrOrErrorText);

				var outgoingMessage = FindOutgoingMessageFromMessageNumber(messageNumber, messageSender);
				var entry = outgoingMessage?.EM_LinkedObject as CusEntryHeader;
				if (entry == null)
				{
					outgoingMessage = FindOutgoingMessageFromMessageNumberAndLinkedBusinessObject(messageNumber, ediMessage.EM_LinkUniqueID);
					entry = outgoingMessage?.EM_LinkedObject as CusEntryHeader;
				}
				if (entry == null)
				{
					serviceLogger.Log(LogType.Warning, "Could not find entry for cargo report reply #" + messageNumber);
					PentantReportApplicationTypeMessageProcessor.SendEmailForProblem(ediMessage, "CargoReport", "Entry not found");
					return;
				}
				else
				{
					outgoingMessage.EM_Status = EDIMessage.Status.Rejected;
					var declaration = entry.Declaration;
					ediMessage.EM_GB = declaration.JE_GB;
					AttachMessageToEntry(entry);
					if (DeclarationHasMatchingACA(declaration, aca))
					{
						if (successOrError == PentantConstants.CargoReportSuccessCode)
						{
							if (declaration.JE_MasterUCR.IsEmpty)
							{
								UpdateDeclarationOrDeclarationsToNewMucr(messageNumber, aca, mucrOrErrorText, outgoingMessage, declaration, importExport);
							}
							else
							{
								serviceLogger.Log(LogType.Warning, "Cargo report reply #" + messageNumber + " with ACA " + aca + ", but the declaration (" + declaration.JE_DeclarationReference + ") linked to this message already has a MUCR");
								PentantReportApplicationTypeMessageProcessor.SendEmailForProblem(ediMessage, "CargoReport", "MUCR already set");
								return;
							}
						}
						else
						{
							serviceLogger.Log(LogType.Warning, "Cargo report reply #" + messageNumber + " with ACA " + aca + " for declaration " + declaration.JE_DeclarationReference + " advises of a failure: " + mucrOrErrorText);
							PentantReportApplicationTypeMessageProcessor.SendEmailForProblem(ediMessage, "CargoReport", "Business rule failure");
							return;
						}
					}
					else
					{
						serviceLogger.Log(LogType.Warning, "Cargo report reply #" + messageNumber + " advised of ACA " + aca + ", but the entry linked to this message has ACA " + declaration.JE_ACAReference);
						PentantReportApplicationTypeMessageProcessor.SendEmailForProblem(ediMessage, "CargoReport", "ACA mismatch");
						return;
					}
				}
			}
		}

		void UpdateDeclarationOrDeclarationsToNewMucr(ZString messageNumber, ZString aca, ZString mucrOrErrorText, EDIMessage outgoingMessage, JobDeclaration declaration, string importExport)
		{
			if (importExport == PentantConstants.Import || declaration.RelevantConsol == null)
			{
				serviceLogger.Log(LogType.Information, "Cargo report reply #" + messageNumber + " with ACA " + aca + " for declaration " + declaration.JE_DeclarationReference + "; new MUCR set to " + mucrOrErrorText);
				declaration.JE_MasterUCR = mucrOrErrorText;
			}
			else if (importExport == PentantConstants.Export)
			{
				var consol = declaration.RelevantConsol;
				foreach (var shipment in consol.Shipments.OfType<ForwardingShipment>().Where(s => s.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, System.StringComparison.OrdinalIgnoreCase)))
				{
					var brotherDeclaration = declaration.Factory.Load<Customs.Business.BaseJobDeclaration>(new ZQuery(JobDeclarationSchema.JE_JS, shipment.PK)).OfType<JobDeclaration>().FirstOrDefault();
					if (brotherDeclaration != null)
					{
						serviceLogger.Log(LogType.Information, "Cargo report reply #" + messageNumber + " with ACA " + aca + " for declaration " + declaration.JE_DeclarationReference + "; new MUCR set to " + mucrOrErrorText + " on declaration " + brotherDeclaration.JE_DeclarationReference + " also on consol " + consol.JK_UniqueConsignRef);
						brotherDeclaration.JE_MasterUCR = mucrOrErrorText;
					}
				}
			}
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			PentantReportApplicationTypeMessageProcessor.SendSuccessEmail(declaration, "CargoReport");
		}

		ZString GetInterpretation(ZString messageNumber, ZString aca, ZString successOrError, ZString mucrOrErrorText)
		{
			return successOrError == PentantConstants.CargoReportSuccessCode
					? string.Format(CultureInfo.InvariantCulture, "<h3>Cargo Report Reply</h3><P>Message number: {0}</P> <P>ACA: {1}</P> <P>Result: success</P> <P>MUCR Allocated: {2}</P> ", messageNumber, aca, mucrOrErrorText)
					: string.Format(CultureInfo.InvariantCulture, "<h3>Cargo Report Reply</h3><P>Message number: {0}</P> <P>ACA: {1}</P> <P>Result: failure</P> <P>Error reason: {2}</P> <P>Please correct any data problems or contact Pentant for assistance.</P>", messageNumber, aca, mucrOrErrorText);
		}

		void AttachMessageToEntry(CusEntryHeader entry)
		{
			entry.Messages.Add(ediMessage);
		}

		bool DeclarationHasMatchingACA(JobDeclaration declaration, ZString aca)
		{
			return declaration.JE_ACAReference == aca;
		}

		EDIMessage FindOutgoingMessageFromMessageNumber(string messageNumber, string messageSender)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNumber);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.Pentant);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, new ZString[] { ApplicationCodeList.Codes.GbCustomsDeclarationServices, PentantConstants.CargoMessageType });
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, messageSender);
			var messages = ediMessage.Factory.Load<EDIMessage>(query);
			if (messages.Length == 0)
			{
				return null;
			}
			else if (messages.Length == 1)
			{
				return messages[0];
			}
			else
			{
				serviceLogger.Log(LogType.Warning, "Cargo report reply #" + messageNumber + " for message sender " + messageSender + " has too many outgoing messages found");
				return null;
			}
		}

		EDIMessage FindOutgoingMessageFromMessageNumberAndLinkedBusinessObject(string messageNumber, ZGuid linkUniqueID)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNumber);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCustomsDeclarationServices);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, PentantConstants.CargoMessageType);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, linkUniqueID);
			var messages = ediMessage.Factory.Load<EDIMessage>(query);
			if (messages.Length == 0)
			{
				return null;
			}
			else if (messages.Length == 1)
			{
				return messages[0];
			}
			else
			{
				serviceLogger.Log(LogType.Warning, "Cargo report reply #" + messageNumber + " for business object " + linkUniqueID + " has too many outgoing messages found");
				return null;
			}
		}

		readonly ILogger serviceLogger;
		readonly EDIMessage ediMessage;
	}
}

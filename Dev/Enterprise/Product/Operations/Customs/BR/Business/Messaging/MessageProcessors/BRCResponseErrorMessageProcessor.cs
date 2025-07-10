using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class BRCResponseErrorMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCResponseErrorMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C51A70C1-06CA-485D-96FF-1C6662BCA34F", "Common Service Error");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.XER };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var outgoingMessages = BRMessageHelper.GetOutgoingMessages(message);
			var universalEvent = new UniversalEventWrapper(message.EM_MessageText);

			if (universalEvent.MessageType == MessageTypeList.Codes.XER)
			{
				var linkedObject = message.EM_LinkedObject;
				var outgoingMessage = outgoingMessages.FirstOrDefault();
				var subject = string.Empty;
				var heading = string.Empty;

				if (linkedObject is CusEntryHeader entryHeader)
				{
					if (outgoingMessage.EM_MessageType == MessageTypeList.Codes.CIH)
					{
						BRCDuimpHeaderErrorResponseMessageProcessor.ProcessMessage(message, outgoingMessage, Logger);
					}
					else
					{
						entryHeader.CH_Status = BRMessageStatusList.Codes.Rejected;
					}

					subject = $"Service error response has been received for job {entryHeader.Declaration.JE_DeclarationReference}";
					heading = $"Entry reference number {entryHeader.CH_BGMReference} failed";
				}
				else if (linkedObject is GlbExternalPassword_BRS externalPassword)
				{
					BRCSubscriptionErrorResponseMessageProcessor.ProcessMessage(message, outgoingMessage);

					subject = $"Service error response has been received for subscription {externalPassword.GP_UserID} for staff {externalPassword.Staff.GS_FullName}";
					heading = $"Subscription {externalPassword.GP_UserID}" + (NoResString)" failed";
				}
				else if (linkedObject is CusGoodsCatalog)
				{
					BRCCatalogErrorResponseMessageProcessor.ProcessMessages(message, outgoingMessages);
				}
				else if (linkedObject is OrgHeader owner)
				{
					if (outgoingMessage.EM_MessageType == MessageTypeList.Codes.CAT && outgoingMessage.EM_MessageSubType == EDIMessageSubTypeList.Codes.CatalogZipFile)
					{
						owner.Logs.AddNew(Events.MessageRejected, outgoingMessage.PK.ToString());
					}
				}
				else if (linkedObject is CusLPCOHeader)
				{
					BRCLPCOErrorResponseMessageProcessor.ProcessMessages(message, outgoingMessages);
				}
				else if (outgoingMessage.EM_LinkedObject is CusBRForeignOperator)
				{
					BRCForeignOperatorErrorResponseMessageProcessor.ProcessMessages(message, outgoingMessages);
				}

				if (!string.IsNullOrEmpty(subject))
				{
					message.EM_MessageInterpretation = new UniversalEventMessagePrettyFormatter(universalEvent).GetFormattedMessageText();
					SendErrorNotification(message, outgoingMessage, subject, heading);
				}
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: Message Text is not a XER Universal Event.");
			}
		}
	}
}

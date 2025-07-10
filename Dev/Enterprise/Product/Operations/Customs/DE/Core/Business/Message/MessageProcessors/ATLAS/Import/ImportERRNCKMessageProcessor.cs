using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportERRNCKMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public ImportERRNCKMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6D295A58-88C5-41F9-89D6-5687EB953BE5", "Import ERRNCK Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IERRNCK> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.ERR;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			entryHeader.CH_Status = EDIMessageStatusList.Codes.Rejected;

			var logbookRegistrationNumber = !message.DataProvider.ReferenceNumber.IsNullOrEmpty() ? message.DataProvider.ReferenceNumber : message.DataProvider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);

			DeleteCusReconEntryIfRequired(entryHeader);

			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				SkipSnapshotUpdate(declaration);
				SendEmail();
			}

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("1CE635EF-1C5E-47F1-8F4A-B978E301CD29", "Import Declaration Status")
					, GetEmailBody(message.DataProvider.Errors)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}

			ZString GetEmailBody(IEnumerable<IERRNCKError> errors)
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("D72AB525-9D81-47AF-8F97-FA9018E061FD", @"Your Import Declaration for Job {0} has been rejected. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
				htmlBody.Append("<br /><br />");
				htmlBody.Append(GetErrorsEmailTable(errors));
				return htmlBody.ToString();
			}
		}

		void DeleteCusReconEntryIfRequired(CusEntryHeader entryHeader)
		{
			if (entryHeader.CusEntryNumber == null)
			{
				entryHeader.GetCusReconEntry()?.Delete();
			}
		}
	}
}

using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportFINTAXMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<IFINTAX>, IFINTAX>
	{
		public ImportFINTAXMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0863CC87-20A0-4119-B0C4-A547FFE1C165", "Import FINTAX Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IFINTAX> message)
			=> GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IFINTAX> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(new ZString[] { message.DataProvider.ReferenceNumber, message.DataProvider.MRN });
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TXF;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
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
					, Res.GetString("C9F0F768-1C23-49A3-9BB8-80DEFD809855", "Import FINTAX – Final Tax Assessment")
					, GetEmailBody(message.DataProvider, declaration)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(IFINTAX provider, JobDeclaration declaration)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("AEBCC5EC-F6CC-47C0-AF54-810E95F9AEFE", "Your Import Declaration for Job {0} received a Final Tax Assessment. For details please follow the link to the job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator();
			if (!string.IsNullOrEmpty(provider.MRN))
			{
				tableCreator.WriteRow(Res.GetString("4fa329ad-f3a3-41a0-a2dd-382e7058c4d0", "MRN"), provider.MRN);
			}
			if (!string.IsNullOrEmpty(provider.ReferenceNumber))
			{
				tableCreator.WriteRow(Res.GetString("9A9129FC-CE5E-4A2F-9BF4-6FBAC43C4E61", "Registration Number"), provider.ReferenceNumber);
			}
			if (!string.IsNullOrEmpty(provider.LocalReferenceNumber))
			{
				tableCreator.WriteRow(Res.GetString("3ABF6E6A-AFB8-4261-B18E-567366C083C6", "Local Reference Number"), provider.LocalReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}
	}
}

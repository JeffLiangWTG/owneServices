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
	public class ImportCUSTRAMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICUSTRA>, ICUSTRA>
	{
		public ImportCUSTRAMessageProcessor(LoggingInformation logger) : base(logger)
		{ }

		protected override string MessageFriendlyNameCore => Res.GetString("3FF20DA7-F77C-4DC8-BA18-A4F6109E9711", "Import CUSTRA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSTRA> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSTRA> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TRA;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			var referenceNumber = message.DataProvider?.ReferenceNumber ?? ZString.Empty;
			var mrn = message.DataProvider?.MRN ?? string.Empty;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });

			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("DBD4D37D-F649-4D58-ADE6-90C3B234133B", "Import CUSTRA – Customs Transmission Message")
					, GetEmailBody(declaration, referenceNumber, mrn, message.DataProvider)
					, isFailure: false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ZString referenceNumber, ZString mrn, ICUSTRA provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("7BC7368C-FD78-45C0-ADA9-97606C9E1BD1", "Your Import Declaration for Job {0} received a Customs Transmission Message. For details please follow the link to the job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var transmissionDate = provider.ForwardedDate;
			var reason = provider.Reason;

			var tableCreator = new HtmlTableCreator();
			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("E9D47285-5D85-464F-BC28-492C9182B764", "Registration Number"), referenceNumber);
			}
			if (!mrn.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("C39B1EFD-C8D9-429B-B1E2-E28E09BB3AF7", "MRN"), mrn);
			}
			if (!transmissionDate.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("4E90854C-233B-41DE-AE8E-4408FC91C0A6", "Transmission Date"), transmissionDate.ToString("dd.MM.yyyy"));
			}
			if (!reason.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("F2D341E3-0B26-47BC-B147-966B09F707F0", "Transmission Reason"), reason);
			}
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}

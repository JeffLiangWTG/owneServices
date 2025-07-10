using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPREJMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPREJ>, IEXPREJ>
	{
		public ExportEXPREJMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("27B53C0B-4AA0-476E-8A20-2BE796470356", "Export EXPREJ Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPREJ> message) => GetLinkedObjectFromOriginalMessageOrMrn(message.Factory, message.DataProvider?.ReferencedMessageIdentifier, message.DataProvider?.MovementReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPREJ> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK;
			var dataProvider = message.DataProvider;
			entryHeader.MovementReferenceNumberSetter(dataProvider.MovementReferenceNumber);
			entryHeader.CH_EntryStatus = dataProvider.ExportStatus;
			entryHeader.AddCustomsEntryStatusLog(entryHeader.CH_EntryStatus);

			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			SendEmailIfNeeded(message, entryHeader, dataProvider.ReferencedMessageIdentifier);
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPREJ> message, CusEntryHeader entryHeader, ZString referencedMessageIdentifier)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);

				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory,
					declaration
					, Res.GetString("F404F6B8-09A5-488B-B46A-0262D8B43D37", "AES EXP Rejection Message")
					, GetEmailBody(entryHeader, declaration)
					, false
					, message.Branch
					, declaration
					, referencedMessageIdentifier);
			}
		}

		ZString GetEmailBody(CusEntryHeader entryHeader, JobDeclaration declaration)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("D98D5CA1-FA60-4A20-BDF4-E4A26835F251",
				"Your Export Declaration Message for {0} has a Rejection Message. For details please follow the Link to the Job",
				declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("6A258D0B-9DB9-4603-9102-848E4BBF4DF5", "MRN:"), entryHeader.MovementReferenceNumber);
			tableCreator.WriteRow(Res.GetString("A14487D0-9BCB-43B0-8A75-B740385895B2", "Status:"), entryHeader.CH_EntryStatus);
			tableCreator.WriteRow(Res.GetString("DB1F6FA9-B91A-44E0-9877-2AF39FDFDB7F", "Status Text:"), entryHeader.EntryHeaderStatusDescription);
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}

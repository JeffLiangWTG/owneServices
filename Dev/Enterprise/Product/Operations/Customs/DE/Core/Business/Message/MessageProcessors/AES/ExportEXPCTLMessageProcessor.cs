using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPCTLMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPCTL>, IEXPCTL>
	{
		public ExportEXPCTLMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B8739D65-F0F0-4DEE-AE84-313B08E7CC2C", "Export EXPCTL Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPCTL> message)
			=> GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MovementReferenceNumber)
			?? GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPCTL> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			var declaration = entryHeader.Declaration;
			var lastSentOutgoingMessage = entryHeader.Messages.LastSentOutgoingMessage;
			message.EM_Status = AesEDIMessage.Status.ProcessedOK;

			var dataProvider = message.DataProvider;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			entryHeader.AddCustomsEntryStatusLog(UniversalReferenceConstants.EntryStatus.CTL);

			SendEmailIfNeeded(message, message.DataProvider, entryHeader, lastSentOutgoingMessage, declaration);
			declaration.ControlMessageUnreadEDocStatus = YesNoList.Codes.Yes;
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPCTL> message, IEXPCTL dataProvider, CusEntryHeader entryHeader, EDIMessage lastSentOutgoingMessage, JobDeclaration declaration)
		{
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, declaration
					, Res.GetString("E395F913-A1A6-4ED3-8DE7-38E0CD137F49", "AES EXP Control measure Message")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, () => lastSentOutgoingMessage);
			}
		}

		ZString GetEmailBody(JobDeclaration declaration, IEXPCTL dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("4FB9DE84-9ADE-4164-AAC1-4B5B3FEAC937", "Your Export Declaration Message for Job {0} has a control measure request. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("bd542d7a-179d-408f-8a6a-de7b78a870a8", "MRN: {0}", dataProvider.MovementReferenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var emailTable = new HtmlTableCreator(new string[]
			{
				Res.GetString("3AE63594-52D0-4536-8D9B-4788BCB2BF3F", "Line No."), Res.GetString("A47E4B45-A5B5-4EFD-815D-118543B33083", "Control Type"), Res.GetString("3A64B657-2F0C-4911-8889-21242923BAA7", "Annotation")
			});
			var exportTypeOfControlsList = new ExportTypeOfControlsCodeList();
			foreach (var line in dataProvider.Lines)
			{
				var type = line.ControlMeasureType;
				emailTable.WriteRow(new string[] { line.SequenceNumber, type + " - " + exportTypeOfControlsList.GetDescriptionFromCode(type), line.Annotation });
			}
			htmlBody.Append(emailTable.ToHtml());
			return htmlBody.ToString();
		}
	}
}

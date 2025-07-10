using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPNOTMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPNOT>, IEXPNOT>
	{
		public ExportEXPNOTMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0c3d27fd-c7d0-4e96-98f5-aef11b4d62b1", "Export EXPNOT Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPNOT> message) => GetLinkedObjectFromOriginalMessageOrMrn(message.Factory, message.DataProvider?.ReferencedMessageIdentifier, message.DataProvider?.MovementReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPNOT> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			var status = EDIMessage.Status.ProcessedOK;
			if (entryHeader.CH_EntryStatus == "501")
			{
				entryHeader.CH_EntryStatus = "541";
				entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;
				entryHeader.AddCustomsEntryStatusLog(entryHeader.CH_EntryStatus);
				SendEmailIfNeeded(message, entryHeader);
			}
			else if (entryHeader.CH_EntryStatus == "502")
			{
				entryHeader.CH_EntryStatus = "570";
				entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;
				entryHeader.AddCustomsEntryStatusLog(entryHeader.CH_EntryStatus);
				SendEmailIfNeeded(message, entryHeader);
			}
			else
			{
				status = EDIMessage.Status.Discarded;
				factory.CreateStmNoteForEdiMessage(message.PK, Res.GetString("64287CE6-847E-42F0-B19E-33A4FBEF9BC3", "Message discarded: Entry Status '501' or '502' expected."));
			}
			message.EM_Status = status;

			var dataProvider = message.DataProvider;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure,
			IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPNOT> message, CusEntryHeader entryHeader)
		{
			attachedDocumentsCached = message.AttachedDocuments;

			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				var dataProvider = message.DataProvider;
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, declaration
					, Res.GetString("6FE361E8-25DD-49FD-808E-FFC8DDB0ABDC", "AES EXP")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, dataProvider.ReferencedMessageIdentifier);
			}
		}

		ZString GetEmailBody(JobDeclaration declaration, IEXPNOT dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("EB6A52A8-8801-47F1-A8B8-A919DAEA5BA8", @"Your Export Declaration Message for Job {0} has a Notification for Export. For details please follow the Link to the Job.", declaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("856F38B5-FE02-4C22-B172-F48EB2B9236A", "MRN"), dataProvider.MovementReferenceNumber);
			var exitDateTime = dataProvider.ExitDateTime;
			if (exitDateTime != null)
			{
				tableCreator.WriteRow(Res.GetString("07C27C87-7C26-4339-9B08-73E4481653EB", "Exit Date - Time"), exitDateTime?.ToString((NoResString)"dd.MM.yyyy - HH:mm", CultureInfo.InvariantCulture));
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}

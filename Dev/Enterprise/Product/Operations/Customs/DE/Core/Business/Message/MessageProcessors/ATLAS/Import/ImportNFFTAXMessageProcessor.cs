using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportNFFTAXMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<INFFTAX>, INFFTAX>
	{
		public ImportNFFTAXMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("70B625EA-627E-4940-AC9E-5A77AE6E4695", "Import NFFTAX Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<INFFTAX> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			return email;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<INFFTAX> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var referenceNumber = message.DataProvider?.ReferenceNumber ?? ZString.Empty;
			var mrn = message.DataProvider?.MRN ?? ZString.Empty;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TXR;
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			attachedDocumentsCached = message.AttachedDocuments;
			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("FCE4FBDB-C05E-44DA-BD51-250637BD38C9", "Import NFFTAX – Reasons for not finally fixed Import Taxes")
					, GetEmailBody(declaration, referenceNumber, mrn, message.DataProvider)
					, isFailure: false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ZString referenceNumber, ZString mrn, INFFTAX provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("C43154DC-BD06-4D05-8D3F-6A38B8573111", "Your Import Declaration for Job {0} received Reasons for not finally fixed Import Taxes. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();

			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("1DEC605E-D1E2-4697-950A-1DC365555BDA", "Registration Number"), referenceNumber);
			}
			if (!mrn.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("7B873864-E866-49BE-BC76-C8A6363ADAC2", "MRN"), mrn);
			}
			var localReferenceNumber = provider.LocalReferenceNumber;
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("858E49F1-483E-4A94-8B99-8CBA145012FE", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());

			var goodsItems = provider.GoodsItems;
			if (goodsItems.Any())
			{
				var goodsItemsTableCreator = new HtmlTableCreator();
				goodsItemsTableCreator.WriteRow(Res.GetString("17DB763F-74A9-4B1D-A826-363D307EC562", "Affected Lines"));

				foreach (var goodsItem in goodsItems)
				{
					goodsItemsTableCreator.WriteRow(goodsItem.SequenceNumber);
				}
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(goodsItemsTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}

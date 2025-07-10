using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportCUSNOAMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICUSNOA>, ICUSNOA>
	{
		public ImportCUSNOAMessageProcessor(LoggingInformation logger) : base(logger)
		{ }

		protected override string MessageFriendlyNameCore => Res.GetString("15A295C5-07D0-45F5-8520-42D298776E2E", "Import CUSNOA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSNOA> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.ReferenceNumber ?? ZString.Empty);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSNOA> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(message.DataProvider.ReferenceNumber);
			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("C9D1DD4C-E158-4B86-8834-C183005E1050", "Import CUSNOA – Customs Notification of Attribution")
					, GetEmailBody(declaration, message.DataProvider)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ICUSNOA provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("4BD3C7BF-DB70-46FD-A639-FEF856C536B7", "Your Import Declaration for Job {0} received a Customs Notification of Attribution. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("57A02842-4DC1-4F5F-9865-5B55B1367A44", "Registration Number"), referenceNumber);
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("DF890C69-9505-49DF-AC24-C8DB85DF7144", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());

			if (!provider.GoodsItems.IsNullOrEmpty())
			{
				var goodsItemsTableCreator = new HtmlTableCreator();
				goodsItemsTableCreator.WriteRow(Res.GetString("BB432826-1D52-4716-932C-865123B592BA", "Line Number"),
					Res.GetString("9FCF6E46-EEEE-4F59-9AFE-1E9FBA4B384B", "Document Type"),
					Res.GetString("DA0D584B-243F-4F33-B949-55AF82FB0907", "Document Reference"),
					Res.GetString("70A970F7-2C8A-4A4D-AA29-7E0611052165", "Cancellation?"));

				foreach (var goodsItem in provider.GoodsItems)
				{
					goodsItemsTableCreator.WriteRow(goodsItem.SequenceNumber, goodsItem.DocumentType, goodsItem.DocumentReference, goodsItem.CancellationWriteOffFlag);
				}

				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(goodsItemsTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}

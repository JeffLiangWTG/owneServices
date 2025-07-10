using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingCUSNOAMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<ICUSNOA>, ICUSNOA>
	{
		public MonthlyClosingCUSNOAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B77A3ECB-FF20-49A6-9AF4-27F998774117", "Monthly Closing CUSNOA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSNOA> message) => GetCusReconDeclarationFromMRN(message.Factory, message.Branch.GB_GC, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSNOA> message)
		{
			var reconDeclaration = (CusReconDeclaration)message.EM_LinkedObject;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(message.DataProvider.ReferenceNumber);

			SendEmail(message, reconDeclaration);
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSNOA> message, CusReconDeclaration reconDeclaration)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, reconDeclaration
				, Res.GetString("FEC381E2-683C-4CCF-BA0D-8178B8628B42", "Monthly Closing CUSNOA – Customs Notification of Attribution")
				, GetEmailBody(reconDeclaration, message.DataProvider)
				, false
				, message.Branch
				, reconDeclaration
				, () => reconDeclaration.Messages.LastSentOutgoingMessage);
		}

		static ZString GetEmailBody(CusReconDeclaration declaration, ICUSNOA provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("739C1B7C-BF83-4330-BDF0-CDDA55CD9E51", "Your Monthly Closing Declaration for Job {0} received a Customs Notification of Attribution. For details please follow the link to the job.", declaration.CRD_JobReferenceNumber));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("AFC296E4-C4CB-41CC-8F79-CA443C746573", "Registration Number"), referenceNumber);
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("D6A2456A-57E1-4518-951F-7F9069AB842F", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());

			if (!provider.GoodsItems.IsNullOrEmpty())
			{
				var goodsItemsTableCreator = new HtmlTableCreator();
				goodsItemsTableCreator.WriteRow(Res.GetString("FAB721BC-2CDD-40D8-9F6E-DE6EB665719E", "Line Number"),
					Res.GetString("8C6B845D-FB7A-4602-99B0-751D1BC2D932", "Document Type"),
					Res.GetString("1C3D2FAA-A6D2-4608-A873-DF8DACD43D2E", "Document Reference"),
					Res.GetString("C47E18E8-F9F7-4566-879B-809DDD9BEAE4", "Cancellation?"));

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

using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingNFFTAXMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<INFFTAX>, INFFTAX>
	{
		public MonthlyClosingNFFTAXMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("7CAD6F95-6960-4A3C-85B5-5BCE642111F4", "Monthly Closing NFFTAX Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<INFFTAX> message) => GetCusReconDeclarationFromMRN(message.Factory, message.Branch.GB_GC, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<INFFTAX> message)
		{
			var declaration = (CusReconDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var referenceNumber = message.DataProvider.ReferenceNumber;
			var mrn = message.DataProvider.MRN;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory
					, declaration
					, Res.GetString("C727400C-C9D5-4440-B19B-9DB88CF650C6", "Monthly Closing NFFTAX – Reasons for not finally fixed Import Taxes")
					, GetEmailBody(declaration, referenceNumber, mrn, message.DataProvider)
					, isFailure: false
					, message.Branch
					, declaration
					, () => declaration.Messages.LastSentOutgoingMessage);
			}
		}

		static ZString GetEmailBody(CusReconDeclaration declaration, string referenceNumber, string mrn, INFFTAX provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("BB3E5A47-0B06-4C7B-9C14-EFCA3F9AB782", "Your Monthly Closing Declaration for Job {0} received Reasons for not finally fixed Import Taxes. For details please follow the link to the job.", declaration.CRD_JobReferenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			if (!string.IsNullOrWhiteSpace(referenceNumber))
			{
				tableCreator.WriteRow(Res.GetString("19CB10E0-5A29-4002-9D9F-75EB51EF7D23", "Registration Number"), provider.ReferenceNumber);
			}
			if (!string.IsNullOrWhiteSpace(mrn))
			{
				tableCreator.WriteRow(Res.GetString("744DA617-4D7D-4162-8736-521863B75746", "MRN"), provider.MRN);
			}
			var localReferenceNumber = provider.LocalReferenceNumber;
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("47562130-94DA-40CD-96BB-7EC0B961F0C5", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());

			var goodsItems = provider.GoodsItems;
			if (goodsItems.Any())
			{
				var goodsItemsTableCreator = new HtmlTableCreator();
				goodsItemsTableCreator.WriteRow(Res.GetString("F160AFEC-CCCE-455D-BF9C-226D274221EA", "Affected Lines"));
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
	}
}

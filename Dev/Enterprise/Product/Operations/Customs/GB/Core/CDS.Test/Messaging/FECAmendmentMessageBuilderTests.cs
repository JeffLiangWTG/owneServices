using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class FECAmendmentMessageBuilderTests : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			var invoice = entryHeader.Declaration.Invoices[0];
			var invoiceLine1 = invoice.InvoiceLines[0];
			invoiceLine1.JI_Weight = 5;
			invoiceLine1.JI_WeightUQ = "KG";

			var newInvoiceLine = entryHeader.MergedLines[0].InvoiceLines.AddNew();
			newInvoiceLine.JI_Weight = 10;
			newInvoiceLine.JI_WeightUQ = "T";
			newInvoiceLine.JI_CL = entryHeader.MergedLines[0].PK;

			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
				messageSendingObject.VOCReason = "FEC Amendment";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedFECAmendment.xml"), amendmentMessage);
		}
	}
}

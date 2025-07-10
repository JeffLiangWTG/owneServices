using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class ArrivalAmendmentMessageBuilderTests : TestCaseWithFactory
	{
		public void TestBuildWithoutChanges()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entryHeader.LRN = entryHeader.DeclarationUCR;
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Arrival Notification";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
				messageSendingObject.VOCReason = "Arrival Notification";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedArrivalAmendmentWithoutChanges.xml"), amendmentMessage);
		}

		public void TestBuildWithChanges()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entryHeader.LRN = entryHeader.DeclarationUCR;
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.Declaration.InvoiceLines[0].JI_Weight = 25;

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Arrival Notification";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
				messageSendingObject.VOCReason = "Arrival Notification";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedArrivalAmendmentWithChanges.xml"), amendmentMessage);
		}
	}
}

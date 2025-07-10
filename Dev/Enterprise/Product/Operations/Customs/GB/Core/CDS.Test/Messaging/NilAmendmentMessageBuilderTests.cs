using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class NilAmendmentMessageBuilderTests : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Nil Amendment";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			entryHeader.CH_MasterUCR = "123456";
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
				messageSendingObject.VOCReason = "Nil Amendment";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedNilAmendment.xml"), amendmentMessage);
		}

		public void TestBuildMUCROrder()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			var prevDoc1 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "DCR";
			prevDoc1.CSI_SubType = "1";
			prevDoc1.CSI_ReferenceNumber = "123";
			var prevDoc2 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "MCR";
			prevDoc2.CSI_SubType = "2";
			prevDoc2.CSI_ReferenceNumber = "456";

			Factory.Save();

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.Declaration.PreviousDocuments.RemoveAndDelete(prevDoc1);

			MessageSendingTestHelper.CreateNewAmendmentMessage(entryHeader);

			entryHeader.Declaration.PreviousDocuments.RemoveAndDeleteAll();

			prevDoc1 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc1.CSI_Code = "DCR";
			prevDoc1.CSI_SubType = "1";
			prevDoc1.CSI_ReferenceNumber = "123";

			prevDoc2 = entryHeader.Declaration.PreviousDocuments.AddNew();
			prevDoc2.CSI_Code = "MCR";
			prevDoc2.CSI_SubType = "2";
			prevDoc2.CSI_ReferenceNumber = "456";

			Factory.Save();

			MessageSendingTestHelper.CreateNewAmendmentMessage(entryHeader, EDIMessageStatusList.Codes.Discarded);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Nil Amendment";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			entryHeader.CH_MasterUCR = "123456";
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
				messageSendingObject.VOCReason = "Nil Amendment";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new EU.Business.ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedNilAmendmentMucrOrder.xml"), amendmentMessage);
		}
	}
}

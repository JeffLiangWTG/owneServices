using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSGBJobDeclarationMessageManagerTests : TestCaseWithFactory
	{
		public void TestPopulateAmendmentReasonIfNeeded()
		{
			var entry1 = Factory.New<CusEntryHeader>();
			var sendingObject1 = new JobDeclarationMessageSendingObject(entry1);
			sendingObject1.MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			sendingObject1.ChangeAcknowledgementIndicator = "ABC";
			sendingObject1.VOCReason = "123";

			var entry2 = Factory.New<CusEntryHeader>();
			var sendingObject2 = new JobDeclarationMessageSendingObject(entry2);
			sendingObject2.MessageType = CDSEDIMessageTypeList.Codes.AmendDeclaration;
			sendingObject2.ChangeAcknowledgementIndicator = "DEF";
			sendingObject2.VOCReason = "456";

			var entry3 = Factory.New<CusEntryHeader>();
			var sendingObject3 = new JobDeclarationMessageSendingObject(entry3);
			sendingObject3.MessageType = CDSEDIMessageTypeList.Codes.CancelDeclaration;
			sendingObject3.ChangeAcknowledgementIndicator = "GHI";
			sendingObject3.VOCReason = "789";

			var objectsToSend = new JobDeclarationMessageSendingObject[]
			{
				sendingObject1,
				sendingObject2,
				sendingObject3
			};

			var cdsTransmissionGenerator = new CDSTransmissionMessageGenerator(objectsToSend);
			var cdsGBJobDeclarationMessageManager = new CDSGBJobDeclarationMessageManagerForTest(null, cdsTransmissionGenerator);
			//cdsGBJobDeclarationMessageManager.SetObjectsToSend(objectsToSend);
			cdsGBJobDeclarationMessageManager.PopulateAmendmentReasonIfNeeded();

			CombineAssertions(delegate
			{
				AssertNullOrEmpty(entry1.ZG_AmendmentReasonCode);
				AssertNullOrEmpty(entry1.CH_CustomsMessageRemarks);
			});

			CombineAssertions(delegate
			{
				AssertEquals("DEF", entry2.ZG_AmendmentReasonCode);
				AssertEquals("456", entry2.CH_CustomsMessageRemarks);
			});

			CombineAssertions(delegate
			{
				AssertEquals("GHI", entry3.ZG_AmendmentReasonCode);
				AssertEquals("789", entry3.CH_CustomsMessageRemarks);
			});
		}

		public class CDSGBJobDeclarationMessageManagerForTest : CDSGBJobDeclarationMessageManager
		{
			public CDSGBJobDeclarationMessageManagerForTest(EU.Business.Declaration.JobDeclaration declaration, CDSTransmissionMessageGenerator generator)
			: base(declaration, generator)
			{
			}

			public new void PopulateAmendmentReasonIfNeeded()
			{
				base.PopulateAmendmentReasonIfNeeded();
			}
		}
	}
}

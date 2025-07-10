using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportAmendmentDetailsCollection))]
	sealed class LocalExportAmendmentDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LocalExportAmendmentDetailsCollection>
	{
		protected override LocalExportAmendmentDetailsCollection GetCollectionToTest() => new LocalExportAmendmentDetailsCollection(Factory.New<CusEntryHeader>());
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new LocalExportAmendmentDetails(new LocalExportAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory);
		}

		public void TestGetLocalExportAmendmentDetailsCollection_5DR()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DPSnapshot();
			Assert(entry, ElectronicDocumentTypeList.Codes._5DR);
		}

		public void TestGetLocalExportAmendmentDetailsCollection_5DS()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DQSnapshot();
			Assert(entry, ElectronicDocumentTypeList.Codes._5DS);
		}

		void Assert(CusEntryHeader entry, ZString messageType)
		{
			entry.CH_VersionID = 1;

			var amendMessage1 = SetAmendmentSnapShotData(entry, messageType, LocalExportAmendmentReasonCodeList.Codes._1);
			var collection = new LocalExportAmendmentDetailsCollection(entry);
			AssertEquals(1, collection.Count);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._1, collection[0].ReasonCode);

			var amendMessage2 = SetAmendmentSnapShotData(entry, messageType, LocalExportAmendmentReasonCodeList.Codes._2);
			collection = new LocalExportAmendmentDetailsCollection(entry);
			AssertEquals(1, collection.Count);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._2, collection[0].ReasonCode);

			var amendMessage3 = SetAmendmentSnapShotData(entry, messageType, LocalExportAmendmentReasonCodeList.Codes._9);
			collection = new LocalExportAmendmentDetailsCollection(entry);
			AssertEquals(1, collection.Count);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._9, collection[0].ReasonCode);
			entry.CH_VersionID = 2;

			var amendMessage4 = SetAmendmentSnapShotData(entry, messageType, LocalExportAmendmentReasonCodeList.Codes._1);
			collection = new LocalExportAmendmentDetailsCollection(entry);
			AssertEquals(2, collection.Count);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._9, collection[0].ReasonCode);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._1, collection[1].ReasonCode);

			var amendMessage5 = SetAmendmentSnapShotData(entry, messageType, LocalExportAmendmentReasonCodeList.Codes._2);
			collection = new LocalExportAmendmentDetailsCollection(entry);
			AssertEquals(2, collection.Count);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._9, collection[0].ReasonCode);
			AssertEquals(LocalExportAmendmentReasonCodeList.Codes._2, collection[1].ReasonCode);
		}

		EDIMessage SetAmendmentSnapShotData(CusEntryHeader entry, ZString messageType, ZString reason)
		{
			var declaration = Factory.Load<JobDeclaration>(entry.Declaration.PK);
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, messageType);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ReasonCode = reason;

			new GOVCBR5DS5DRAmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, messageType, Factory).Send();
			Factory.Save();

			return entry.Messages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == messageType);
		}
	}
}

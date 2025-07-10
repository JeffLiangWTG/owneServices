using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(MessageSendingActionCollection))]
	class MessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingActionCollection>
	{
		public void TestElementsArePopulatedCorrectly()
		{
			jobHeader.CHGOFFCusTempStorageDecs.AddNew();
			var coll = GetCollectionToTest();
			AssertEquals(jobHeader.CHGOFFCusTempStorageDecs.Count, coll.Count);
			AssertEquals("REF", coll[0].Details);
		}

		public void TestAllowNewAndInvalidExceptionOnAddNew()
		{
			var coll = GetCollectionToTest();
			CombineAssertions(() =>
			{
				AssertEquals("System constructs this collection from dbo.JobHeader's relevant Dec collection", false, coll.AllowNew);
				AssertExceptionThrown<InvalidOperationException>("AddNew Exception", "Users cannot add a new member", () => coll.AddNew());
			});
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		protected override Type GetExpectedCollectionType() => typeof(MessageSendingActionCollection);

		protected override MessageSendingActionCollection GetCollectionToTest()
		{
			jobHeader.CHGOFFCusTempStorageDecs.AddNew();
			var result = new MessageSendingActionCollection(jobHeader.CHGOFFCusTempStorageDecs.Cast<CusTempStorageDec>(), x => "REF", Factory);
			result.PopulateElements();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = jobHeader.CHGOFFCusTempStorageDecs.AddNew();
			return new MessageSendingAction(dec, x => "REF");
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobHeader = Factory.New<CusTempStorageJobHeader>();
		}
		CusTempStorageJobHeader jobHeader;
	}
}

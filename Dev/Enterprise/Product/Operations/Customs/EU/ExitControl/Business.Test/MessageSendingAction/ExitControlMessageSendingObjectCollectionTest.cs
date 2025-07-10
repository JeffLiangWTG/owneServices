using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObjectCollection))]
	class ExitControlMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitControlMessageSendingObjectCollection>
	{
		public void TestElementsArePopulatedCorrectly()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "TRANS123" }, Collection.Cast<ExitControlMessageSendingObject>().Select(x => x.TransportID));
		}

		public void TestAllowNewAndInvalidExceptionOnAddNew()
		{
			var coll = GetCollectionToTest();
			CombineAssertions(() =>
			{
				Assert("Adding records not allowed", !coll.AllowNew);
				AssertExceptionThrown<InvalidOperationException>("AddNew Exception", "Users cannot add a new member", () => coll.AddNew());
			});
		}

		public void TestAllowRemove()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowRemove);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExitControlMessageSendingObjectCollection);

		protected override ExitControlMessageSendingObjectCollection GetCollectionToTest()
		{
			return new ExitControlMessageSendingObjectCollection(exitHeader.CusExitReports, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var report = exitHeader.CusExitReports.AddNew();
			return new ExitControlMessageSendingObject(report);
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitHeader>();
			var report = exitHeader.CusExitReports.AddNew();
			report.CER_TransportID = "TRANS123";
		}
		CusExitHeader exitHeader;
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationItemCollection))]
	class ExitNotificationItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitNotificationItemCollection>
	{
		public void TestConstructor()
		{
			var item1 = exitDetail.CusExitItems.AddNew();
			var item2 = exitDetail.CusExitItems.AddNew();
			item1.Packages.AddNew();
			item1.Packages.AddNew();
			item2.Packages.AddNew();
			var coll = new ExitNotificationPackageCollection(exitDetail);

			AssertEquals(3, coll.Count);
		}

		public void TestConstructorNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ExitNotificationItemCollection(null));
		}

		public void TestAllowNew()
		{
			AssertEquals(false, new ExitNotificationItemCollection(exitDetail).AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, new ExitNotificationItemCollection(exitDetail).AllowRemove);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExitNotificationItemCollection);

		protected override ExitNotificationItemCollection GetCollectionToTest()
		{
			return new ExitNotificationItemCollection(exitDetail);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitDetail = Factory.New<CusExitDetail>();
		}
		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExitNotificationItem(Factory.New<CusExitItem>());

		CusExitDetail exitDetail;
	}
}

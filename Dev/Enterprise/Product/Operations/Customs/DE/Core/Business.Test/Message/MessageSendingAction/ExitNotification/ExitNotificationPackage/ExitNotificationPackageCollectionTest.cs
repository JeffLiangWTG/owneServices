using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationPackageCollection))]
	class ExitNotificationPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitNotificationPackageCollection>
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
			AssertExceptionThrown<ArgumentNullException>(() => new ExitNotificationPackageCollection(null));
		}

		public void TestAllowNew()
		{
			AssertEquals(false, new ExitNotificationPackageCollection(exitDetail).AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, new ExitNotificationPackageCollection(exitDetail).AllowRemove);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExitNotificationPackageCollection);

		protected override ExitNotificationPackageCollection GetCollectionToTest() => new ExitNotificationPackageCollection(exitDetail);

		protected override void SetUp()
		{
			base.SetUp();

			exitDetail = Factory.New<CusExitDetail>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExitNotificationPackage(Factory.New<CusExitItemPackage>());

		CusExitDetail exitDetail;
	}
}

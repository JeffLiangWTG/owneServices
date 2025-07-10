using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>))]
	sealed class GuaranteeAccessCodesSendingObjectCollectionBaseOnlyTest : NonPersistentBusinessObjectCollectionTestCase<GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(null));
		}

		public void TestAddSendingObjectOnConstruction()
		{
			var collection = new GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(sendingObjectParent);
			AssertEquals(1, collection.Count);
			AssertSame(cusGuaranteeHeader, collection[0].CusGuaranteeHeader);
		}

		public override void TestDelete()
		{
			Assert("Not supporting deleting, this check is not required.", true);
		}

		public override void TestAdd()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Not supporting removing, this check is not required.", true);
		}

		protected override GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject> GetCollectionToTest()
			=> new GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>(sendingObjectParent);

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendingObjectParent = new GuaranteeAccessCodesSendingObjectParent(cusGuaranteeHeader);
		}
		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingObjectParent sendingObjectParent;
	}
}

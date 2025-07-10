using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentItemCollection<CusExitConsignmentItem>))]
sealed class CusExitConsignmentItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitConsignmentItemCollection<CusExitConsignmentItem>>
{
	public void TestMaxCountValidation()
	{
		var collection = (ISupportMaxCountValidation)GetCollectionToTest();
		CombineAssertions(() =>
		{
			var validator = collection.MaxCountValidator;
			var notification = validator.Notification;
			AssertEquals("MaxCount", 99, validator.MaxCount);
			AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
			AssertEquals("Notification Type", NotificationType.Error, notification.Type);
			AssertEquals("Notification Message", "Maximum number of items is 99.", notification.Message);
		});
	}

	public void TestSuspendAllowNew()
	{
		var master = Factory.New<CusExitConsignment>();
		var collection = master.CusExitConsignmentItems;
		var consignmentItem = collection.AddNew();
		var pivotCollection = consignmentItem.CusExitConsignmentPackagePivots;
		AssertEquals("CusExitConsignmentItems, allowNew as default.", true, collection.AllowNew);
		AssertEquals("CusExitConsignmentPackagePivots, allowNew as default.", true, pivotCollection.AllowNew);

		var suspender = collection.SuspendAllowNew();
		AssertEquals("CusExitConsignmentItems, not allowNew when SuspendAllowNew.", false, collection.AllowNew);
		AssertEquals("CusExitConsignmentPackagePivots, not allowNew  when SuspendAllowNew.", false, pivotCollection.AllowNew);
		suspender.Dispose();

		AssertEquals("CusExitConsignmentItems, allowNew when SuspendAllowNew disposed.", true, collection.AllowNew);
		AssertEquals("CusExitConsignmentPackagePivots, allowNew when SuspendAllowNew disposed.", true, pivotCollection.AllowNew);
	}

	protected override CusExitConsignmentItemCollection<CusExitConsignmentItem> GetCollectionToTest()
	{
		var master = Factory.New<CusExitConsignment>();
		return new CusExitConsignmentItemCollection<CusExitConsignmentItem>(master);
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415MessageSendingObjectParent))]
	sealed class RF415MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "321";

			var messageSendingObjectParent = new RF415MessageSendingObjectParent(header);
			AssertEquals("Should create message sending object collection from bills", 2, messageSendingObjectParent.SendingObjectsCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new RF415MessageSendingObjectParent(header);
		}
	}
}

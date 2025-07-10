using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePackCollection))]
	public class TemporaryStoragePackCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			return new TemporaryStoragePackCollection(bill);
		}

		public void TestAllowNew()
		{
			var maxCount = 9;
			var cusSupportingInfoCollection = GetCollectionToTest();
			cusSupportingInfoCollection.MaxCountValidationEnable(maxCount);
			for (var i = 0; i < maxCount; i++)
			{
				Assertion.AssertEquals("It is allowed to add more packs", true, cusSupportingInfoCollection.AllowNew);
				cusSupportingInfoCollection.AddNew();
			}
			Assertion.AssertEquals("No more packs allowed", false, cusSupportingInfoCollection.AllowNew);
		}
	}
}

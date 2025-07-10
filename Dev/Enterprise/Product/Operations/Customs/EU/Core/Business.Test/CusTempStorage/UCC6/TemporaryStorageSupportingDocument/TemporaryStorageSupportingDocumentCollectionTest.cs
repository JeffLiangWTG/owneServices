using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>))]
	public class TemporaryStorageSupportingDocumentCollectionTest : CusSupportingInfoCollectionTest<TemporaryStorageSupportingDocument>
	{
		protected override CusSupportingInfoCollection<TemporaryStorageSupportingDocument> GetCusSupportingInfoCollection()
		{
			var bill = Factory.New<TemporaryStorageBill>();
			return new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>(bill);
		}

		public void TestAllowNew()
		{
			var maxCount = 9;
			var cusSupportingInfoCollection = GetCusSupportingInfoCollection();
			cusSupportingInfoCollection.MaxCountValidationEnable(maxCount);
			for (var i = 0; i < maxCount; i++)
			{
				Assertion.AssertEquals("It is allowed to add more supporting documents", true, cusSupportingInfoCollection.AllowNew);
				cusSupportingInfoCollection.AddNew();
			}
			Assertion.AssertEquals("No more supporting documents allowed", false, cusSupportingInfoCollection.AllowNew);
		}

		public void TestAllowNew_FromBillItem()
		{
			var maxCount = 9;
			var billItem = Factory.New<TemporaryStoragePackedItem>();
			var cusSupportingInfoCollection = new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>(billItem);
			cusSupportingInfoCollection.MaxCountValidationEnable(maxCount);
			for (var i = 0; i < maxCount; i++)
			{
				AssertEquals("It is allowed to add more supporting documents", true, cusSupportingInfoCollection.AllowNew);
				cusSupportingInfoCollection.AddNew();
			}
			AssertEquals("No more supporting documents allowed", false, cusSupportingInfoCollection.AllowNew);
		}
	}
}

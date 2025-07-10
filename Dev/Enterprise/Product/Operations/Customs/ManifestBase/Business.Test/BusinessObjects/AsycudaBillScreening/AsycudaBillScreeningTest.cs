using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestedType(typeof(AsycudaBillScreening))]
	class AsycudaBillScreeningTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var billScreening = Factory.New<AsycudaBillScreening>();
			billScreening.ASR_ABL = bill.PK;
			AssertEquals(bill, billScreening.Bill);
		}

		public void TestClusterKey()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = Factory.New<AsycudaBillWithIAsycudaBillScreeningTypeSupporter>();
			bill.ABL_AMA = manifestHeader.PK;
			var collection = new AsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBillWithIAsycudaBillScreeningTypeSupporter>(bill);
			var billScreening = collection.AddNew();
			AssertEquals(0, billScreening.ASR_ClusterKey);

			Factory.Save();
			AssertEquals(1, billScreening.ASR_ClusterKey);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var result = factory.New<AsycudaBillScreening>();
			result.ASR_ABL = bill.PK;
			return result;
		}
	}
}

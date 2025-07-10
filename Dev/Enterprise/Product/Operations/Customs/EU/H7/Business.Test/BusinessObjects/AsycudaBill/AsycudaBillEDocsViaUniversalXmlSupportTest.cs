using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaBillEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObjectFromCode()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "Header0001";
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillNumber = "Bill1";

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "Header0002";
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill1";

			Factory.Save();

			var loader = new AsycudaBillEDocsViaUniversalXmlSupport();
			var bill1InNewFactory = NewFactory().Load<AsycudaBill>(bill1.PK);
			AssertEquals("Bill1", bill1InNewFactory.ABL_BillNumber);
			AssertEquals("Header0001", bill1InNewFactory.Header.AMA_JobReference);
			AssertEquals(bill1.PK, loader.LoadBusinessObjectFromCode(Factory, "Header0001|Bill1")?.PK);
			AssertEquals(bill2.PK, loader.LoadBusinessObjectFromCode(Factory, "Header0002|Bill1")?.PK);
		}
	}
}

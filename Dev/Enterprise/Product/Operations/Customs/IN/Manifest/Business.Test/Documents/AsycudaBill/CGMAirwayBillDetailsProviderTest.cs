using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAirwayBillDetailsProvider))]
sealed class CGMAirwayBillDetailsProviderTest : TestCaseWithFactory
{
	[TestDate(2024, 05, 15)]
	public void TestProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("BillNumber", provider.BillNumber, "BILL001");
			AssertEquals("BillDate", provider.BillDate, ZDate.Today);
			AssertEquals("PortOfOrigin", provider.PortOfOrigin, "BLR");
			AssertEquals("PortOfDestination", provider.PortOfDestination, "DEL");
			AssertEquals("NumberOfPackages", provider.NumberOfPackages, 1);
			AssertEquals("GrossWeight", provider.GrossWeightInKilos, 10m);
			AssertEquals("CargoDescription", provider.CargoDescription, "Desc");
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		var bill = header.Bills.AddNew();
		bill.ABL_BillNumber = "BILL001";
		bill.ABL_BillIssueDate = ZDate.Today;
		bill.ABL_RL_NKOrigin = "INBLR";
		bill.ABL_RL_NKFinalDestination = "INDEL";
		bill.ABL_ManifestQty = 1;
		bill.ABL_GrossWeight = 10.0;
		bill.ABL_GrossWeightUQ = "KG";
		bill.ABL_GoodsDescription = "Desc";

		provider = new CGMAirwayBillDetailsProvider(bill);
	}
	ICGMAirwayBillDetails provider;
}

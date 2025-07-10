using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAirAsycudaBillDocWrapper))]
sealed class CGMAirAsycudaBillDocWrapperTest : TestCaseWithFactory
{
	[TestDate(2024, 05, 15)]
	public void TestProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("HAWBNumber", billDocWrapper.HAWBNumber, "BILL001");
			AssertEquals("HAWBDate", billDocWrapper.HAWBDate, ZDate.Today);
			AssertEquals("PortOfOrigin", billDocWrapper.PortOfOrigin, "BLR");
			AssertEquals("PortOfDestination", billDocWrapper.PortOfDestination, "DEL");
			AssertEquals("NumberOfPackages", billDocWrapper.NumberOfPackages, 1);
			AssertEquals("GrossWeightInKilos", billDocWrapper.GrossWeightInKilos, 10m);
			AssertEquals("CargoDescription", billDocWrapper.CargoDescription, "Desc");
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

		billDocWrapper = new CGMAirAsycudaBillDocWrapper(bill);
	}
	CGMAirAsycudaBillDocWrapper billDocWrapper;
}

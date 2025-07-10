using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAirAsycudaManifestHeaderDocWrapper))]
sealed class CGMAirAsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
{
	[TestDate(2024, 05, 15)]
	public void TestProperties()
	{
		var now = ZDateTime.Now;
		var today = ZDate.Today;
		header.MasterBill.ABL_E_ARV = now;
		using (INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Agent01"))
		{
			CombineAssertions(() =>
			{
				AssertEquals("AgentCode", "Agent01", manifestHeaderDocWrapper.AgentCode);
				AssertEquals("AgentName", GlbCompany.CurrentCompany.CompanyName, manifestHeaderDocWrapper.AgentName);
				AssertEquals("IGMNumber", "MAN001", manifestHeaderDocWrapper.IGMNumber);
				AssertEquals("IGMDate", today, manifestHeaderDocWrapper.IGMDate);
				AssertEquals("FlightNumber", "Voyage", manifestHeaderDocWrapper.FlightNumber);
				AssertEquals("FlightDateTime", now, manifestHeaderDocWrapper.FlightDateTime);
				AssertEquals("MAWBNumber", "BILL001", manifestHeaderDocWrapper.MAWBNumber);
				AssertEquals("MAWBDate", today, manifestHeaderDocWrapper.MAWBDate);
				AssertEquals("PortOfOrigin", "BLR", manifestHeaderDocWrapper.PortOfOrigin);
				AssertEquals("PortOfDestination", "DEL", manifestHeaderDocWrapper.PortOfDestination);
				AssertEquals("NumberOfPackages", 1, manifestHeaderDocWrapper.NumberOfPackages);
				AssertEquals("GrossWeightInKilos", 10m, manifestHeaderDocWrapper.GrossWeightInKilos);
				AssertEquals("CargoDescription", "Desc", manifestHeaderDocWrapper.CargoDescription);
				AssertEquals("AsycudaBillDocWrappers Count", 2, manifestHeaderDocWrapper.BillsWrapper.Count);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		var branch = GlbBranch.CurrentBranch;
		header.AMA_GB = branch.PK;
		Factory.Save();

		header.ImportGeneralManifestNumber = "MAN001";
		header.ImportGeneralManifestDate = ZDate.Today;
		header.AMA_Voyage = "Voyage";

		var masterBill = header.MasterBill;
		masterBill.ABL_BillNumber = "BILL001";
		masterBill.ABL_BillIssueDate = ZDate.Today;
		masterBill.ABL_RL_NKOrigin = "INBLR";
		masterBill.ABL_RL_NKFinalDestination = "INDEL";
		masterBill.ABL_ManifestQty = 1;
		masterBill.ABL_GrossWeight = 10.0;
		masterBill.ABL_GrossWeightUQ = "KG";
		masterBill.ABL_GoodsDescription = "Desc";

		header.Bills.AddNew();
		header.Bills.AddNew();

		manifestHeaderDocWrapper = new CGMAirAsycudaManifestHeaderDocWrapper(header);
	}

	CGMAirAsycudaManifestHeaderDocWrapper manifestHeaderDocWrapper;
	CGMAsycudaManifestHeader header;
}

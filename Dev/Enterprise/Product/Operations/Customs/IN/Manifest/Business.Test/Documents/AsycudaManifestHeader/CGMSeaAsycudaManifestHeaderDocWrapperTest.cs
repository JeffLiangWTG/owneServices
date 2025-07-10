using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMSeaAsycudaManifestHeaderDocWrapper))]
sealed class CGMSeaAsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
{
	public void TestCARNNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Without Registry setup", ZString.Empty, Wrapper.CARNNumber);
			using (INCustomsDataRegistry.Instance.INConsolAgentRegistrationNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Carn001"))
			{
				AssertEquals("With Registry setup", "Carn001", Wrapper.CARNNumber);
			}
		});
	}

	public void TestConsolAgentName()
	{
		GlbCompany.CurrentCompany.OrgProxy.OH_FullName = "Wisetech";
		AssertEquals("ConsolAgentName", "Wisetech", Wrapper.ConsolAgentName);
	}

	public void TestJobReference()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.JobReference);
			Header.AMA_JobReference = "M1234";
			AssertEquals("Header assigned", "M1234", Wrapper.JobReference);
		});
	}

	public void TestCustomHouse()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.CustomHouse);
			Header.AMA_CustomsOffice = "INBLR";
			AssertEquals("Header assigned", "INBLR", Wrapper.CustomHouse);
		});
	}

	public void TestIGMNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.IGMNumber);
			Header.ImportGeneralManifestNumber = "1234567";
			AssertEquals("Header assigned", "1234567", Wrapper.IGMNumber);
		});
	}

	public void TestIGMDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZDate.Empty, Wrapper.IGMDate);
			Header.ImportGeneralManifestDate = new ZDate(2024, 6, 13);
			AssertEquals("Header assigned", new ZDate(2024, 6, 13), Wrapper.IGMDate);
		});
	}

	public void TestVesselCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.VesselCode);
			Header.AMA_VesselName = "V1234";
			AssertEquals("Header assigned", "V1234", Wrapper.VesselCode);
		});
	}

	public void TestIMOCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.IMOCode);
			Header.AMA_LloydsNumber = "IM1234";
			AssertEquals("Header assigned", "IM1234", Wrapper.IMOCode);
		});
	}

	public void TestVoyageNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.VoyageNumber);
			Header.AMA_Voyage = "VY1234";
			AssertEquals("Header assigned", "VY1234", Wrapper.VoyageNumber);
		});
	}

	public void TestShippingLineName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ShippingLineName);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "BlueDart";
			var orgAddress = orgHeader.MainAddress;
			Header.AMA_OA_Carrier = orgAddress.PK;
			AssertEquals("Header assigned", "BlueDart", Wrapper.ShippingLineName);

			orgAddress.OA_CompanyNameOverride = "DHL";
			AssertEquals("CompanyNameOverride", "DHL", Wrapper.ShippingLineName);
		});
	}

	public void TestLineNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.LineNumber);
			Header.AMA_CarrierReference = "1234";
			AssertEquals("Header assigned", "1234", Wrapper.LineNumber);
		});
	}

	public void TestMBLNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.MBLNumber);
			Header.AMA_MasterBill = "MB1234";
			AssertEquals("Header assigned", "MB1234", Wrapper.MBLNumber);
		});
	}

	public void TestMBLDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZDate.Empty, Wrapper.MBLDate);
			Header.AMA_MasterBillIssueDate = new ZDate(2024, 6, 13);
			AssertEquals("Header assigned", new ZDate(2024, 6, 13), Wrapper.MBLDate);
		});
	}

	public void TestBillsWrapper()
	{
		Header.Bills.AddNew();
		Header.Bills.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, Wrapper.BillsWrapper.Count);
			AssertType<CGMSeaAsycudaBillDocWrapper>("Type", Wrapper.BillsWrapper[0]);
		});
	}

	public void TestContainersWrapper()
	{
		var container = Header.Containers.AddNew();
		Header.Containers.AddNew();

		var bill = Header.Bills.AddNew();
		var pack = bill.Packs.AddNew();
		bill.Packs.AddNew();
		pack.ContainerPK = container.PK;
		CombineAssertions(() =>
		{
			AssertEquals("Count", 1, Wrapper.ContainersWrapper.Count);
			AssertType<CGMAsycudaContainerDocWrapper>("Type", Wrapper.ContainersWrapper[0]);
		});
	}

	CGMSeaAsycudaManifestHeaderDocWrapper Wrapper => wrapper ??= new CGMSeaAsycudaManifestHeaderDocWrapper(Header);
	CGMSeaAsycudaManifestHeaderDocWrapper wrapper;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}

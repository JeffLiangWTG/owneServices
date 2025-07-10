using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaContainerDocWrapper))]
sealed class CGMAsycudaContainerDocWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When pack is null", () => new CGMAsycudaContainerDocWrapper(null));

			var pack = Factory.New<CGMAsycudaPack>();
			AssertExceptionThrown<ArgumentNullException>("When Container is null", () => new CGMAsycudaContainerDocWrapper(pack));

			pack.ContainerPK = Container.PK;
			AssertExceptionThrown<ArgumentNullException>("When Bill is null", () => new CGMAsycudaContainerDocWrapper(pack));
			AssertNoExceptionThrown("When not null", () => new CGMAsycudaContainerDocWrapper(Pack));
		});
	}

	public void TestLineNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.LineNumber);
			Header.AMA_CarrierReference = "C123";
			AssertEquals("Header Assigned", "C123", Wrapper.LineNumber);
		});
	}

	public void TestSubLineNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Auto sequence", 1, Wrapper.SubLineNumber);
		});
	}

	public void TestContainerNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ContainerNumber);
			Container.ACN_ContainerNumber = "CN123";
			AssertEquals("Container Assigned", "CN123", Wrapper.ContainerNumber);
		});
	}

	public void TestContainerSealNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ContainerSealNumber);
			Container.ACN_Seal1 = "S123";
			AssertEquals("Container Assigned", "S123", Wrapper.ContainerSealNumber);
		});
	}

	public void TestContainerAgentCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ContainerAgentCode);
			var orgHeader = Factory.New<OrgHeader>();
			Container.ContainerAgentCodeOrgPK = orgHeader.PK;
			AssertEquals("PAN not added", ZString.Empty, Wrapper.ContainerAgentCode);
			orgHeader.CustomsCodes.AddNew("PAN", "PAN1234");
			AssertEquals("PAN added", "PAN1234", Wrapper.ContainerAgentCode);
		});
	}

	public void TestContainerStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ContainerStatus);
			Container.ACN_EmptyFullIndicator = "FUL";
			AssertEquals("Container Assigned", "FUL", Wrapper.ContainerStatus);
		});
	}

	public void TestTotalPackages()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", 0, Wrapper.TotalPackages);
			Container.ACN_NumberOfPackages = 5;
			AssertEquals("Container Assigned", 5, Wrapper.TotalPackages);
		});
	}

	public void TestContainerWeight()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", 0m, Wrapper.ContainerWeight);
			Container.ACN_GoodsWeight = 3.5m;
			AssertEquals("Container Assigned", 3.5m, Wrapper.ContainerWeight);
		});
	}

	public void TestISOCode()
	{
		AssertEquals("Default", "TBA", Wrapper.ISOCode);
	}

	public void TestSOCFlag()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", "No", Wrapper.SOCFlag);
			Container.ACN_IsShipperOwned = true;
			AssertEquals("Container Assigned", "Yes, Its Shippers own container", Wrapper.SOCFlag);
		});
	}

	CGMAsycudaContainer Container => container ??= Header.Containers.AddNew();
	CGMAsycudaContainer container;

	CGMAsycudaPack Pack => pack ??= GetPack();
	CGMAsycudaPack pack;

	CGMAsycudaPack GetPack()
	{
		var pack = Bill.Packs.AddNew();
		pack.ContainerPK = Container.PK;
		return pack;
	}

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;

	CGMAsycudaContainerDocWrapper Wrapper => wrapper ??= new CGMAsycudaContainerDocWrapper(Pack);
	CGMAsycudaContainerDocWrapper wrapper;
}

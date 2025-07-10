using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaPackCollection))]
sealed class CGMAsycudaPackCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAPA_WeightUQDefault()
	{
		var pack = Factory.New<CGMAsycudaPack>();
		AssertEquals("Standalone Pack", ZString.Empty, pack.APA_WeightUQ);
		var packFromBill = Bill.Packs.AddNew();
		AssertEquals("Pack from Bill, ABL_GrossWeightUQ empty", ZString.Empty, packFromBill.APA_WeightUQ);

		Bill.ABL_GrossWeightUQ = "T";
		packFromBill = Bill.Packs.AddNew();
		AssertEquals("Pack from Bill, ABL_GrossWeightUQ set", "T", packFromBill.APA_WeightUQ);
	}

	public void TestShouldDefaultContainerPK()
	{
		Header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
		var cont = Header.Containers.AddNew();
		var pack = Bill.Packs.AddNew();
		AssertEquals(ZGuid.Empty, pack.ContainerPK);
	}

	protected override Type GetExpectedCollectionType() => typeof(CGMAsycudaPackCollection);

	protected override BusinessObjectCollection GetCollectionToTest() => (BusinessObjectCollection)Bill.Packs;

	CGMAsycudaManifestHeader Header => header ??= GetHeader();
	CGMAsycudaManifestHeader header;

	CGMAsycudaManifestHeader GetHeader()
	{
		var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
		return manifestHeader;
	}

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;
}

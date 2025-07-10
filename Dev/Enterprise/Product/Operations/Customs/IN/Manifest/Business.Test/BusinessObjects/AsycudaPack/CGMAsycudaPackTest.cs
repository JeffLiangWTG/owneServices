using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaPack))]
sealed class CGMAsycudaPackTest : EnterpriseBusinessObjectTestCase
{
	public void TestIAsycudaPack()
	{
		var bizObj = GetNewBusinessObject();
		Factory.Save();
		AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaPack>(bizObj.PK).GetType());
	}
	public void TestAPA_WeightUQ()
	{
		AssertEquals(true, Pack.APA_WeightUQInfo.ReadOnly);
	}

	public void TestContainerPKSetQuantityAndWeight()
	{
		Bill.ABL_ManifestQty = 3;
		Bill.ABL_GrossWeight = 5m;

		var pack1 = Bill.Packs.AddNew();
		pack1.ContainerPK = Container.PK;
		AssertEquals("Container Count", 1, Header.Containers.Count);
		AssertEquals("APA_PackQty, when Container count 1", 3, pack1.APA_PackQty);
		AssertEquals("APA_Weight, when Container count 1", 5m, pack1.APA_Weight);

		Header.Containers.AddNew();
		var pack2 = Bill.Packs.AddNew();
		pack2.ContainerPK = Container.PK;
		AssertEquals("Container Count", 2, Header.Containers.Count);
		AssertEquals("APA_PackQty, when Container count 2", 0, pack2.APA_PackQty);
		AssertEquals("APA_Weight, when Container count 2", 0m, pack2.APA_Weight);
	}

	protected override BusinessObject GetNewBusinessObject() => Pack;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Pack;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => Pack;

	CGMAsycudaPack Pack => pack ??= GetPack();
	CGMAsycudaPack pack;

	CGMAsycudaContainer Container => container ??= Header.Containers.AddNew();
	CGMAsycudaContainer container;

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;

	CGMAsycudaPack GetPack()
	{
		var pack = Bill.Packs.AddNew();
		pack.ContainerPK = Container.PK;
		return pack;
	}
}

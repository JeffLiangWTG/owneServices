using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaPack))]
sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
{
	[ExpectNoExceptions]
	public void TestGetPackedItemTypeCore()
	{
		var pack = (AsycudaPack)GetNewBusinessObject();
		NUnit.Framework.Assert.That(pack.GetPackedItemType(), Is.EqualTo(typeof(AsycudaPackedItem)));
	}

	[ExpectNoExceptions]
	public void TestPackedItem()
	{
		var pack = (AsycudaPack)GetNewBusinessObject();
		NUnit.Framework.Assert.That(pack.PackedItem, Is.TypeOf<AsycudaPackedItem>());
	}

	[ExpectNoExceptions]
	public void TestOverridePropertiesTypes()
	{
		var pack = (AsycudaPack)GetNewBusinessObject();
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(pack.Bill, Is.TypeOf<AsycudaBill>(), "AsycudaBill");
			NUnit.Framework.Assert.That(pack.Validation, Is.TypeOf<AsycudaPackValidation>(), "AsycudaPackValidation");
			NUnit.Framework.Assert.That(pack.Container, Is.TypeOf<AsycudaContainer>(), "AsycudaContainer");
		});
	}

	[ExpectNoExceptions]
	public void TestLookups()
	{
		var pack = (AsycudaPack)GetNewBusinessObject();
		NUnit.Framework.Assert.That(pack.Lookups, Is.TypeOf<AsycudaPackLookups>());
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var container = header.Containers.AddNew();
		var pack = bill.Packs.AddNew();
		pack.ContainerPK = container.PK;
		return pack;
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
}

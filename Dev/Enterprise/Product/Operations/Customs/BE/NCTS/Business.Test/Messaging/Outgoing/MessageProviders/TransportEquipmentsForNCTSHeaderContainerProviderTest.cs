using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(TransportEquipmentsForNCTSHeaderContainerProvider))]
sealed class TransportEquipmentsForNCTSHeaderContainerProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForNCTSHeaderContainerProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentsForNCTSHeaderContainerProvider(null, 1));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(3, provider.SequenceNumber);
	}

	public void TestContainerIdentificationNumber()
	{
		container.BC_ContainerNum = "value";
		AssertEquals("value", provider.ContainerIdentificationNumber);
	}

	public void TestNumberOfSeals()
	{
		container.BC_Seal1 = "one";
		container.BC_Seal2 = "two";
		container.AdditionalSeals.AddNew();

		AssertEquals(3, provider.Seals.Count);
	}

	public void TestSeals() => CombineAssertions(() =>
	{
		container.BC_Seal1 = "one";
		container.BC_Seal2 = "two";
		var seal = container.AdditionalSeals.AddNew();
		seal.BK_SealNumber = "three";

		AssertEquals("one", true, provider.Seals.Any(s => s.Identifier == "one"));
		AssertEquals("two", true, provider.Seals.Any(s => s.Identifier == "two"));
		AssertEquals("three", true, provider.Seals.Any(s => s.Identifier == "three"));
	});

	public void TestGoodsReferences()
	{
		var bill = container.Header.Bills.AddNew();
		var item = bill.GoodsItems.AddNew();
		var package = item.Packages.AddNew();
		package.ContainersPivot.Clear();
		var pivot = package.ContainersPivot.AddNew();
		pivot.XX_Relation2ID = container.PK;
		item.BY_DeclarationGoodsItemNumber = 10;

		AssertEquals("count", 1, provider.GoodsReferences.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		container = nctsHeader.DepartureHeaderContainers.AddNew();
		provider = new TransportEquipmentsForNCTSHeaderContainerProvider(container, 3);
	}
	NctsDepartureHeaderContainer container;
	TransportEquipmentsForNCTSHeaderContainerProvider provider;

	protected override TransportEquipmentsForNCTSHeaderContainerProvider GetProvider() => provider;
}

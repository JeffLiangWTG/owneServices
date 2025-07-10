using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(TransportEquipmentsForNCTSContainerProvider))]
sealed class TransportEquipmentsForNCTSContainerProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForNCTSContainerProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentsForNCTSContainerProvider(null, 1));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(3, provider.SequenceNumber);
	}

	public void TestContainerIdentificationNumber()
	{
		container.ContainerNumber = "value";
		AssertEquals("value", provider.ContainerIdentificationNumber);
	}

	public void TestNumberOfSeals()
	{
		container.BC_Seal1 = "one";
		container.BC_Seal2 = "two";
		container.Seals.AddNew();
		var sealDAM = container.Seals.AddNew();
		sealDAM.BK_UnloadingState = "DAM";

		AssertEquals(3, provider.Seals.Count);
	}

	public void TestSeals() => CombineAssertions(() =>
	{
		container.BC_Seal1 = "one";
		container.BC_Seal2 = "two";
		var seal = container.Seals.AddNew();
		seal.BK_SealNumber = "three";
		var sealDAM = container.Seals.AddNew();
		sealDAM.BK_SealNumber = "four";
		sealDAM.BK_UnloadingState = "DAM";

		AssertEquals("one", true, provider.Seals.Any(s => s.Identifier == "one"));
		AssertEquals("two", true, provider.Seals.Any(s => s.Identifier == "two"));
		AssertEquals("three", true, provider.Seals.Any(s => s.Identifier == "three"));
		AssertEquals("four", false, provider.Seals.Any(s => s.Identifier == "four"));
	});

	public void TestGoodsReferences() => CombineAssertions(() =>
	{
		var validGoodsReference = container.ItemNumbers.AddNew();
		validGoodsReference.CY_Code = Constants.CusCodeDataTypes.ITEM;
		validGoodsReference.CY_DataNumeric = 10;
		var invalidGoodsReference = container.ItemNumbers.AddNew();
		invalidGoodsReference.CY_Code = Constants.CusCodeDataTypes.LOC;
		invalidGoodsReference.CY_DataNumeric = 20;

		AssertEquals("count", 1, provider.GoodsReferences.Count);
		AssertEquals("correct", 10, provider.GoodsReferences.First().DeclarationGoodsItemNumber);
	});

	protected override void SetUp()
	{
		base.SetUp();
		container = Factory.New<NctsContainer>();
		provider = new TransportEquipmentsForNCTSContainerProvider(container, 3);
	}
	NctsContainer container;
	TransportEquipmentsForNCTSContainerProvider provider;

	protected override TransportEquipmentsForNCTSContainerProvider GetProvider() => provider;
}

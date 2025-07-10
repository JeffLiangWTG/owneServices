using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class TransportEquipmentDataProviderTest : TestCaseWithFactory
{
	public void TestNewCollection() => AssertNull("null", TransportEquipmentDataProvider.NewCollection(null));

	public void TestSequenceNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CusContainers.AddNew();
		declaration.CusContainers.AddNew();

		var transportEquipments = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).ToArray();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, transportEquipments.Length);
			AssertEquals("Sequence at 1 index", 1, transportEquipments.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, transportEquipments.ElementAt(1).SequenceNumber);
		});
	}

	public void TestContainerIdentificationNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "123";

		var transportEquipment = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).Single();

		AssertEquals("123", transportEquipment.ContainerIdentificationNumber);
	}

	public void TestSeals()
	{
		var declaration = Factory.New<JobDeclaration>();
		var container = declaration.CusContainers.AddNew();

		container.CO_Seal = ZString.Empty;
		var transportEquipment1 = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).Single();
		AssertEquals(0, transportEquipment1.Seals.Count);
		AssertEquals(0, transportEquipment1.NumberOfSeals);

		container.CO_Seal = "123";
		var transportEquipment2 = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).Single();
		CombineAssertions(() =>
		{
			AssertEquals("NumberOfSeals", 1, transportEquipment2.NumberOfSeals);
			AssertEquals("Count", 1, transportEquipment2.Seals.Count);
			Assert("Type", transportEquipment2.Seals.All(te => te is SealDataProvider));
			AssertEquals("Cached", transportEquipment2.Seals, transportEquipment2.Seals);
		});
	}

	public void TestGoodsReferences()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = Factory.New<CusEntryLine>().PK;
		var container = declaration.CusContainers.AddNew();

		var transportEquipment1 = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).Single();
		AssertEquals(0, transportEquipment1.GoodsReferences.Count);

		container.InvoiceLinePivotCollection.AddNew(invoiceLine);
		var transportEquipment2 = TransportEquipmentDataProvider.NewCollection(declaration.CusContainers).Single();
		CombineAssertions(() =>
		{
			AssertEquals("Count", 1, transportEquipment2.GoodsReferences.Count);
			Assert("Type", transportEquipment2.GoodsReferences.All(te => te is GoodsReferenceDataProvider));
			AssertEquals("Cached", transportEquipment2.GoodsReferences, transportEquipment2.GoodsReferences);
		});
	}
}

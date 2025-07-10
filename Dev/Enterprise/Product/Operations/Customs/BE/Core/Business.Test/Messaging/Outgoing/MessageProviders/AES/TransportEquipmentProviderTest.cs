using System;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class TransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentProvider(null, 1));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestContainerIdentificationNumber()
	{
		AssertEquals("CONTAINERID", Provider.ContainerIdentificationNumber);
	}

	public void TestNumberOfSeals()
	{
		AssertEquals(3, Provider.NumberOfSeals);
	}

	public void TestSeals()
	{
		AssertEquals(3, Provider.Seals.Count);
	}

	public void TestGoodsReferences()
	{
		AssertEquals(1, Provider.GoodsReferences.Count);
	}

	public void TestAdditionalSeals()
	{
		AssertEquals(1, Provider.AdditionalSeals.Count);
	}

	protected override TransportEquipmentProvider GetProvider()
	{
		var container = Factory.New<CusContainer>();
		container.CO_ContainerNumber = "containerID";
		var seal1 = container.AdditionalSeals.AddNew();
		container.AdditionalSeals.AddNew();
		seal1.BK_SealNumber = "notempty";
		container.InvoiceLinePivotCollection.AddNew();
		container.JobContainer.JC_SealNum = "SEALNUMBER";
		container.JobContainer.JC_Additional2SealNum = "SECONDSEALNUMBER";
		container.CO_Seal = "SEAL1";
		container.CO_SecondSeal = "SEAL12";
		return new TransportEquipmentProvider(container, 1);
	}
}

using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class TransportEquipmentWrapperTest : DataProviderTestCase<TransportEquipmentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(2, wrapper.SequenceNumeric);
	}

	public void TestId()
	{
		container.CO_ContainerNumber = "APLU8521458";
		AssertEquals("APLU8521458", wrapper.Id);
	}

	public void TestSealsAffixedQuantity()
	{
		container.CO_Seal = "one";
		container.CO_SecondSeal = "two";
		container.AdditionalSeals.AddNew();
		AssertEquals(3, wrapper.SealsAffixedQuantity);
	}

	public void TestGoodsReferences()
	{
		container.InvoiceLinePivotCollection.AddNew();
		AssertEquals("Number of GoodsReferences", 1, wrapper.GoodsReferences.Count);
	}

	public void TestSeals()
	{
		container.CO_Seal = "one";
		container.CO_SecondSeal = "two";
		container.AdditionalSeals.AddNew();

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.Seals.FirstOrDefault());
			AssertType<SealWrapper>(wrapper.Seals.FirstOrDefault());
			AssertEquals("Number of Seals", 3, wrapper.Seals.Count);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		container = Factory.New<CusContainer>();
		wrapper = new TransportEquipmentWrapper(container, 2);
	}
	CusContainer container;
	TransportEquipmentWrapper wrapper;

	protected override TransportEquipmentWrapper GetProvider() => wrapper;
}

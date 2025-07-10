using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItem))]
sealed class CusTempStorageRegLineItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestSRI_GoodsItemNumber() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegLineItem>()
			.HasProperty(i => i.SRI_GoodsItemNumber)
			.WithCaption("TSD Item Number")
			.WithMediumCaption("TSD Item No.")
			.WithShortCaption("TSD It. No.")
			.WithFullDescription("Goods Item Number in TSD")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void TestFormattedTariff_Attributes() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegLineItem>()
			.HasProperty(i => i.FormattedTariff)
			.WithCaption("Commodity Code")
			.WithMediumCaption("Commodity")
			.WithShortCaption("Cmdty.")
			.WithFullDescription("Tariff Commodity Code for the Goods Item");
	});

	public void TestFormattedTariff()  => CombineAssertions(() =>
	{
		var lineItem = Factory.New<CusTempStorageRegLineItem>();
		lineItem.SRI_Tariff = "49019990";
		AssertEquals("Formatted Tariff", "4901.99.90", lineItem.FormattedTariff);
	});

	public void TestSRI_CusC4Number() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegLineItem>()
			.HasProperty(i => i.SRI_CusC4Number)
			.WithCaption("CUS Code")
			.WithMediumCaption("CUS Code")
			.WithShortCaption("CUS Code")
			.WithFullDescription("CUS Code for chemical substances")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void TestSRI_GoodsDescription() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegLineItem>()
			.HasProperty(i => i.SRI_GoodsDescription)
			.WithCaption("Goods Description")
			.WithMediumCaption("Goods Description")
			.WithShortCaption("Description")
			.WithFullDescription("Goods Description Text")
			.WithAttribute<ReadOnlyAttribute>(r => r.IsReadOnly);
	});

	public void TestGetStorageRegLineItemPivotType()
	{
		var lineItem = Factory.New<CusTempStorageRegLineItem>();
		AssertEquals(typeof(CusTempStorageRegLineItemPivot), lineItem.GetStorageRegLineItemPivotType());
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var lineItem = factory.NewWithValidTestData<CusTempStorageRegLineItem>();
		return lineItem;
	}
}

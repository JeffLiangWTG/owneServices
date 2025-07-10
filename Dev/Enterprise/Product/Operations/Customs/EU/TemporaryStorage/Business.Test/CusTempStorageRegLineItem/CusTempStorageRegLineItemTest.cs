using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItem))]
sealed class CusTempStorageRegLineItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestFormattedTariff()
	{
		var lineItem = Factory.New<CusTempStorageRegLineItem>();
		lineItem.SRI_Tariff = "49019990";
		AssertEquals("Formatted Tariff", "4901.99.90", lineItem.FormattedTariff);
	}

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

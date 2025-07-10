using CargoWise.EntityFramework;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivot))]
sealed class CusTempStorageRegLineItemPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestDelete()
	{
		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var pivot = Factory.New<CusTempStorageRegLineItemPivot>();
		pivot.SRV_SRI_Item = regLineItem.PK;

		pivot.Delete();
		CombineAssertions(() =>
		{
			Assert("Pivot is delete", pivot.IsDeleted);
			Assert("Item is delete", regLineItem.IsDeleted);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		var lineItem = factory.New<CusTempStorageRegLineItem>();
		var lineItemPivot = factory.New<CusTempStorageRegLineItemPivot>();
		lineItemPivot.SRV_SRL_Line = line.PK;
		lineItemPivot.SRV_SRI_Item = lineItem.PK;
		return lineItemPivot;
	}
}

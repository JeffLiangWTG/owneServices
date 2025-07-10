using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivot))]
sealed class CusTempStorageRegLineItemPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestRegLine()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var regLineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRL_Line = regLine.PK;
		AssertType<CusTempStorageRegLine>(regLineItemPivot.RegLine);
	}

	public void TestRegLineItem()
	{
		var regLineItem = Factory.New<CusTempStorageRegLineItem>();
		var regLineItemPivot = Factory.New<CusTempStorageRegLineItemPivot>();
		regLineItemPivot.SRV_SRI_Item = regLineItem.PK;
		AssertType<CusTempStorageRegLineItem>(regLineItemPivot.RegLineItem);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<CusTempStorageRegHeader>();
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 20;
		var lineItem = factory.NewWithValidTestData<CusTempStorageRegLineItem>();
		var lineItemPivot = factory.New<CusTempStorageRegLineItemPivot>();
		lineItemPivot.SRV_SRL_Line = line.PK;
		lineItemPivot.SRV_SRI_Item = lineItem.PK;

		return lineItemPivot;
	}
}

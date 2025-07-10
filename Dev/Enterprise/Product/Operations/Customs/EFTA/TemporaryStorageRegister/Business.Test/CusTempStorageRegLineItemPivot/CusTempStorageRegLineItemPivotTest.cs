using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineItemPivot))]
sealed class CusTempStorageRegLineItemPivotTest : EnterpriseBusinessObjectTestCase
{
	public void TestSRV_GrossWeight() => CombineAssertions(() =>
	{
		AssertEntity<CusTempStorageRegLineItemPivot>()
			.HasProperty(i => i.SRV_GrossWeight)
			.WithCaption("Gross Weight")
			.WithMediumCaption("Gross Weight")
			.WithShortCaption("GWT")
			.WithFullDescription("Goods Gross Weight");
	});

	public void TestIPivotBusinessObject()
	{
		CombineAssertions(() =>
		{
			var regLine = Factory.New<CusTempStorageRegLine>();
			var regItem = (BusinessObject)Factory.New<CusTempStorageRegLineItem>();
			var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
			var pivot = collection.AddChild(regItem);

			AssertEquals("Implements interface", expected: true, typeof(ZArchitecture.Integration.IPivotBusinessObject).IsAssignableFrom(pivot.GetType()));
			AssertEquals("Relation1ID", pivot.Relation1ID, regLine.PK);
			AssertEquals("SRV_SRL_Line", pivot.SRV_SRL_Line, regLine.PK);

			AssertEquals("Relation2ID", pivot.Relation2ID, regItem.PK);
			AssertEquals("SRV_SRI_Item", pivot.SRV_SRI_Item, regItem.PK);

			AssertSame("RegLine", pivot.Relation1Object, pivot.RegLine);
			AssertSame("RegLineItem", pivot.Relation2Object, pivot.RegLineItem);
		});
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

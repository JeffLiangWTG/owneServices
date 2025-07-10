using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(RegLineItemQuantity))]
sealed class RegLineItemQuantityTest : NonPersistentBusinessObjectTestCase
{
	public void TestLoadNew()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when invoiceLine.Declaration is null", () => RegLineItemQuantity.LoadNew(null));
			var regLine = Factory.New<CusTempStorageRegLine>();
			var regItem = Factory.New<CusTempStorageRegLineItem>();
			var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
			var pivot = collection.AddChild(regItem);
			pivot.SRV_GrossWeight = 1;
			pivot.RegLineItem.SRI_GoodsDescription = "1234";
			AssertNoExceptionThrown("No exception expected", () => RegLineItemQuantity.LoadNew(pivot));
			var regLineItemQuantity = RegLineItemQuantity.LoadNew(pivot);
			AssertEquals("RegLineItem.SRI_GoodsDescription is SRI_GoodsDescription from CusTempStorageRegLineItem", "1234", regLineItemQuantity.RegLineItem.SRI_GoodsDescription);
			AssertEquals("SRV_GrossWeight is SRV_GrossWeight From CusTempStorageRegLineItemPivot", (ZDecimal)1, regLineItemQuantity.SRV_GrossWeight);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var regLine = Factory.New<CusTempStorageRegLine>();
		var regItem = Factory.New<CusTempStorageRegLineItem>();
		var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
		var pivot = collection.AddChild(regItem);
		return RegLineItemQuantity.LoadNew(pivot);
	}
}

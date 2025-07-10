using System;
using Enterprise.Registry.Business.BillCustomisationStrategies;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class VariableLengthElementStrategyTest : TestCase
	{
		public void TestProperties()
		{
			var strategy1 = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseCode, "Warehouse Code", 3, NumberCustomisationElementCategories.WarehouseOrder | NumberCustomisationElementCategories.WarehouseReceive);
			var strategy2 = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseClientCode, "Client Code", 9, NumberCustomisationElementCategories.WarehouseOrder | NumberCustomisationElementCategories.WarehouseReceive);
			var strategy3 = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseSalesChannelCode, "Sales Channel Code", 3, NumberCustomisationElementCategories.WarehouseOrder);
			var strategy4 = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseSupplierCode, "Supplier Code", 9, NumberCustomisationElementCategories.WarehouseReceive);
			var strategy5 = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseReceiveCategoryCode, "Receive Category Code", 3, NumberCustomisationElementCategories.WarehouseReceive);

			var customisation = new BillOfLadingNumberCustomisation();
			var element1 = new BillOfLadingNumberCustomisationElement(customisation, strategy1);
			var element2 = new BillOfLadingNumberCustomisationElement(customisation, strategy2);
			var element3 = new BillOfLadingNumberCustomisationElement(customisation, strategy3);
			var element4 = new BillOfLadingNumberCustomisationElement(customisation, strategy4);
			var element5 = new BillOfLadingNumberCustomisationElement(customisation, strategy5);

			AssertEquals("strategy1.Key", "WarehouseCode", strategy1.Key);
			AssertEquals("strategy1.Name", "Warehouse Code", strategy1.Name);
			AssertEquals("strategy1.Description", "Warehouse Code - the maximum length is 3, if you set the length less than the maximum length, the string on the left will be preserved.", strategy1.Description);
			AssertEquals("strategy1.Category", NumberCustomisationElementCategories.WarehouseOrder | NumberCustomisationElementCategories.WarehouseReceive, strategy1.Categories);
			AssertEquals("strategy1.MaxLength", 3, strategy1.CalcMaxGeneratedLength(element1));
			AssertEquals("strategy1.useDetail", expected: true, strategy1.UseDetail);

			AssertEquals("strategy2.Key", "WarehouseClientCode", strategy2.Key);
			AssertEquals("strategy2.Name", "Client Code", strategy2.Name);
			AssertEquals("strategy2.Description", "Client Code - the maximum length is 9, if you set the length less than the maximum length, the string on the left will be preserved.", strategy2.Description);
			AssertEquals("strategy2.Category", NumberCustomisationElementCategories.WarehouseOrder | NumberCustomisationElementCategories.WarehouseReceive, strategy2.Categories);
			AssertEquals("strategy2.MaxLength", 9, strategy2.CalcMaxGeneratedLength(element2));
			AssertEquals("strategy2.useDetail", expected: true, strategy1.UseDetail);

			AssertEquals("strategy3.Key", "WarehouseSalesChannelCode", strategy3.Key);
			AssertEquals("strategy3.Name", "Sales Channel Code", strategy3.Name);
			AssertEquals("strategy3.Description", "Sales Channel Code - the maximum length is 3, if you set the length less than the maximum length, the string on the left will be preserved.", strategy3.Description);
			AssertEquals("strategy3.Category", NumberCustomisationElementCategories.WarehouseOrder, strategy3.Categories);
			AssertEquals("strategy3.MaxLength", 3, strategy3.CalcMaxGeneratedLength(element3));
			AssertEquals("strategy3.useDetail", expected: true, strategy3.UseDetail);

			AssertEquals("strategy4.Key", "WarehouseSupplierCode", strategy4.Key);
			AssertEquals("strategy4.Name", "Supplier Code", strategy4.Name);
			AssertEquals("strategy4.Description", "Supplier Code - the maximum length is 9, if you set the length less than the maximum length, the string on the left will be preserved.", strategy4.Description);
			AssertEquals("strategy4.Category", NumberCustomisationElementCategories.WarehouseReceive, strategy4.Categories);
			AssertEquals("strategy4.MaxLength", 9, strategy4.CalcMaxGeneratedLength(element4));
			AssertEquals("strategy4.useDetail", expected: true, strategy4.UseDetail);

			AssertEquals("strategy5.Key", "WarehouseReceiveCategoryCode", strategy5.Key);
			AssertEquals("strategy5.Name", "Receive Category Code", strategy5.Name);
			AssertEquals("strategy5.Description", "Receive Category Code - the maximum length is 3, if you set the length less than the maximum length, the string on the left will be preserved.", strategy5.Description);
			AssertEquals("strategy5.Category", NumberCustomisationElementCategories.WarehouseReceive, strategy5.Categories);
			AssertEquals("strategy5.MaxLength", 3, strategy5.CalcMaxGeneratedLength(element5));
			AssertEquals("strategy5.useDetail", expected: true, strategy5.UseDetail);
		}

		public void TestGetRegExForDataType()
		{
			var strategy = new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseCode, "Warehouse Code", 3, NumberCustomisationElementCategories.WarehouseOrder | NumberCustomisationElementCategories.WarehouseReceive);

			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, strategy);
			AssertExceptionThrown<NotImplementedException>(() => strategy.GetRegExForDataType(element));
		}
	}
}

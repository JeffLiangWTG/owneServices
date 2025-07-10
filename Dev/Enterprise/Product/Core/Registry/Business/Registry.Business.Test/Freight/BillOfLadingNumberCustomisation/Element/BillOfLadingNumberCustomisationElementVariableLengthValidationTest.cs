using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Registry.Business.Testing
{
	sealed class BillOfLadingNumberCustomisationElementVariableLengthValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetail()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			var element = new BillOfLadingNumberCustomisationElement(customisation, new VariableLengthElementStrategy(BillOfLadingNumberCustomisationElement.Keys.WarehouseCode, "WarehouseCode", 3, NumberCustomisationElementCategories.WarehouseJob));
			element.Include = true;
			element.Order = 1;

			element.Detail = "x";
			AssertHasError(element.DetailInfo, "The WarehouseCode length should be in the range of 1-3.");

			element.Detail = "0";
			AssertHasError(element.DetailInfo, "The WarehouseCode length should be in the range of 1-3.");

			element.Detail = "1";
			AssertNoErrors(element.DetailInfo);

			element.Detail = "2";
			AssertNoErrors(element.DetailInfo);

			element.Detail = "3";
			AssertNoErrors(element.DetailInfo);

			element.Detail = "4";
			AssertHasError(element.DetailInfo, "The WarehouseCode length should be in the range of 1-3.");
		}
	}
}

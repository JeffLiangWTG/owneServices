using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Registry.Business.Testing
{
	sealed class BillOfLadingNumberCustomisationElementYearAsDigitValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetail()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element = new BillOfLadingNumberCustomisationElement(customisation, new YearAsDigitElementStrategy());
			element.Include = true;
			element.Order = 1;

			element.Detail = "x";
			AssertHasError(element.DetailInfo, "The year as digit length should be either 1, 2 or 4.");

			element.Detail = "4";
			AssertNoNotifications(element.DetailInfo);

			element.Detail = "9";
			AssertHasError(element.DetailInfo, "The year as digit length should be either 1, 2 or 4.");

			element.Detail = "2";
			AssertNoErrors(element.DetailInfo);

			element.Detail = "1";
			AssertNoErrors(element.DetailInfo);
		}
	}
}

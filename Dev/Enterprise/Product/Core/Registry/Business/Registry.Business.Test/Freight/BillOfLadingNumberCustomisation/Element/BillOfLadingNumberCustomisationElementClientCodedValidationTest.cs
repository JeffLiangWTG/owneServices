using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Registry.Business.Testing
{
	sealed class BillOfLadingNumberCustomisationElementClientCodedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetail()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element = new BillOfLadingNumberCustomisationElement(customisation, new ClientCodedElementStrategy(BillOfLadingNumberCustomisationElement.Keys.ClientCoded1));
			element.Include = true;
			element.Order = 1;

			element.Detail = "";
			AssertNoNotifications(element.DetailInfo);

			element.Detail = "[]";
			AssertHasError(element.DetailInfo, "Only letters and digits are valid here.");

			customisation.AllowNonAlphanumericCharacters = true;
			element.Detail = "(-:";
			AssertNoError(element.DetailInfo, "Only letters and digits are valid here.");

			customisation.AllowNonAlphanumericCharacters = false;
			element.Detail = "snth";
			AssertNoErrors(element.DetailInfo);
		}
	}
}

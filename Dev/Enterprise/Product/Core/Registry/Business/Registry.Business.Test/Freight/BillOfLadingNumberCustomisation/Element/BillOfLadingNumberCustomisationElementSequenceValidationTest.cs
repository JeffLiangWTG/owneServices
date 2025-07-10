using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.BillCustomisationStrategies;

namespace Enterprise.Registry.Business.Testing
{
	sealed class BillOfLadingNumberCustomisationElementSequenceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetail()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			BillOfLadingNumberCustomisationElement element = new BillOfLadingNumberCustomisationElement(customisation, new SequenceElementStrategy());
			element.Include = true;
			element.Order = 1;

			element.Detail = "xxx";
			AssertHasError(element.DetailInfo, "The sequence length should be in the range of 3-19.");

			element.Detail = "19";
			AssertNoNotifications(element.DetailInfo);

			element.Detail = "29";
			AssertHasError(element.DetailInfo, "The sequence length should be in the range of 3-19.");

			element.Detail = "3";
			AssertNoErrors(element.DetailInfo);

			element.Detail = "2";
			AssertHasError(element.DetailInfo, "The sequence length should be in the range of 3-19.");
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionWithEnabledAndDefaultValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDescription()
		{
			var codeDescription = new CodeDescriptionWithEnabledAndDefault();

			codeDescription.Description = (NoResString)"";
			AssertMandatoryValidationError(codeDescription.DescriptionInfo, true);

			codeDescription.Description = (NoResString)"Skype";
			AssertMandatoryValidationError(codeDescription.DescriptionInfo, false);
		}

		public void TestValidateIsDefault()
		{
			var collection = new CodeDescriptionWithEnabledAndDefaultCollection();
			var item1 = collection.AddNew();

			item1.IsEnabled = true;
			item1.IsDefault = false;
			AssertNoErrors(item1.IsDefaultInfo);
			item1.IsDefault = true;
			AssertNoErrors(item1.IsDefaultInfo);

			item1.IsEnabled = false;
			item1.IsDefault = false;
			AssertNoErrors(item1.IsDefaultInfo);
			item1.IsDefault = true;
			AssertHasError(item1.IsDefaultInfo, "Only enabled items can be the default.");

			item1.IsDefault = true;
			var item2 = collection.AddNew();
			item2.IsDefault = true;
			AssertHasError(item2.IsDefaultInfo, "There can only be one default.");
		}
	}
}

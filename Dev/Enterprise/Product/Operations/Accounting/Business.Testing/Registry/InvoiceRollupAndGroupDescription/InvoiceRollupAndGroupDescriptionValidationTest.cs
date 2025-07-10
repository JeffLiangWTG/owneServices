using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class InvoiceRollupAndGroupDescriptionValidationTest : TestCaseWithFactory
	{
		public void TestValidateEnglishDescription()
		{
			var item = new InvoiceRollupAndGroupDescription();

			item.EnglishDescription = ZString.Empty;
			AssertHasError(item.EnglishDescriptionInfo, "Please enter a value.");

			item.EnglishDescription = "description";
			AssertNoErrors(item.EnglishDescriptionInfo);
		}
	}
}
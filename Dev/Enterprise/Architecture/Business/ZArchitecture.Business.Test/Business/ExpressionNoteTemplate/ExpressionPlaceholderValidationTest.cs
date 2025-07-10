using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ExpressionPlaceholderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDescription()
		{
			var placeholder = new ExpressionPlaceholder(1);
			placeholder.Description = "Hello";
			AssertNoErrors(placeholder.DescriptionInfo);

			placeholder.Description = ZString.Empty;
			AssertHasErrors(placeholder.DescriptionInfo);
		}
	}
}

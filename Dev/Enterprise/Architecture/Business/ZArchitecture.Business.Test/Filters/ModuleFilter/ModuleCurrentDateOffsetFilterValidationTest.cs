using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ModuleCurrentDateOffsetFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOffset()
		{
			var filter = new ModuleCurrentDateOffsetFilter("Dummy", DummyBizoSchema.Z0_Date);
			var validation = new ModuleCurrentDateOffsetFilterValidation(filter);

			filter.Offset = -1;
			validation.ValidateOffset();
			AssertHasErrors(filter.OffsetInfo);

			filter.Offset = 0;
			validation.ValidateOffset();
			AssertNoErrors(filter.OffsetInfo);

			filter.Offset = 3;
			validation.ValidateOffset();
			AssertNoErrors(filter.OffsetInfo);
		}
	}
}

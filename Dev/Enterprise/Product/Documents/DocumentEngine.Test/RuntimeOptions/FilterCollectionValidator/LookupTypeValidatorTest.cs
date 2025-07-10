using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class LookupTypeValidatorTest : TestCaseWithFactory
	{
		public void TestLookupTypeValidator()
		{
			OrgHeader validOrg = Factory.NewWithValidTestData<OrgHeader>();

			LookupField field = new LookupField(Factory);
			field.ZValue = validOrg.PK;
			WarehouseClientCollectionProvider provider = new WarehouseClientCollectionProvider(Factory);

			LookupTypeValidator validator = new LookupTypeValidator(provider.GetFilterDescription());
			validator.Filters.Add(field);
			AssertEquals(true, validator.IsValid(field));

			field.ZValue = ZGuid.Missing;
			AssertEquals(false, validator.IsValid(field));
			AssertEquals(provider.GetFilterDescription(), validator.GetErrorMessage(field));

			LookupTypeValidator validator2 = new LookupTypeValidator(string.Empty);
			validator2.Filters.Add(field);
			AssertEquals(false, validator2.IsValid(field));
			AssertEquals("Value doesn't match lookup criteria.", validator2.GetErrorMessage(field));
		}
	}
}

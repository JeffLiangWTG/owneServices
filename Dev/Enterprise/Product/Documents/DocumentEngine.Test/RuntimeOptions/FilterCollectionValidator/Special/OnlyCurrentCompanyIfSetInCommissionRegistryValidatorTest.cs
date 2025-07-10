using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OnlyCurrentCompanyIfSetInCommissionRegistryValidatorTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var anotherCompany = Factory.New<GlbCompany>();
			var validator = new OnlyCurrentCompanyIfSetInCommissionRegistryValidator();
			var companyField = new LookupField(Factory);

			validator.AddFilterField(companyField);
			companyField.Validators.Add(validator);
			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var exepctedErrorMessage = string.Format("Must be the current login company ({0}) as the registry item '{1}' is currently set to true.", GlbCompany.CurrentCompany.GC_Code, OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.Caption);

			companyField.ZValue = GlbCompany.CurrentCompany.PK;
			AssertEquals(true, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertNoErrors(companyField.ZValueInfo);

			companyField.ZValue = anotherCompany.PK;
			AssertEquals(false, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertHasError(companyField.ZValueInfo, exepctedErrorMessage);

			companyField.ZValue = ZGuid.Empty;
			AssertEquals(false, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertHasError(companyField.ZValueInfo, exepctedErrorMessage);

			OrganisationsDataRegistry.Instance.OnlyShowCommissionsForCurrentLoginCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			companyField.ZValue = GlbCompany.CurrentCompany.PK;
			AssertEquals(true, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertNoErrors(companyField.ZValueInfo);

			companyField.ZValue = anotherCompany.PK;
			AssertEquals(true, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertNoErrors(companyField.ZValueInfo);

			companyField.ZValue = ZGuid.Empty;
			AssertEquals(true, validator.IsValid(companyField));
			companyField.ValidateZValue();
			AssertNoErrors(companyField.ZValueInfo);
		}
	}
}

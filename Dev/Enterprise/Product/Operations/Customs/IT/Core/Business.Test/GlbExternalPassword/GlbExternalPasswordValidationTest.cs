using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class GlbExternalPasswordValidationTest<T, TValidation> : MasterFiles.Business.Testing.GlbExternalPasswordWithCertificateValidationTest<T, TValidation>
	where T : GlbExternalPassword
	where TValidation : GlbExternalPasswordValidation
{
	public void TestCheckGP_ExpiryDateIsValidZDateTimeRange()
	{
		var externalPassword = GetExternalPassword();

		var currentYear = ZDateTime.Today.Year;
		externalPassword.GP_ExpiryDate = new ZDateTime(currentYear + 2, 01, 01);
		AssertNoWarnings(externalPassword.GP_ExpiryDateInfo);
	}

	protected override bool IsCertificateMandatory => true;

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("INTCODE1", "DEC1")
			.Build();
	}

	protected abstract T GetExternalPassword();
}

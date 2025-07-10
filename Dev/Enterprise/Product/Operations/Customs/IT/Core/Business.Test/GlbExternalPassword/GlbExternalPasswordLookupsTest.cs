using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class GlbExternalPasswordLookupsTest<T> : BusinessObjectLookupsTestCase
	where T : GlbExternalPassword
{
	public void TestAllAccountsWithCompany()
	{
		var externalPassword = GetExternalPassword();

		AssertArrayEqualsByElements(System.Array.Empty<AccountAndCompanyPair>(), externalPassword.Lookups.AllAccountsWithCompany.ToArray());
		Factory.ClearCachedValue<List<AccountAndCompanyPair>>("GlbExternalPasswordLookups_IT.AllAccountsWithCompany");

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		company.CompanyName = "FIRST";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

		var companyAccountCollection = new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-DEC1", "DEC1")
			.Build();

		var newCompany = Factory.NewWithValidTestData<GlbCompany>();
		newCompany.CompanyName = "SECOND";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		newCompany.Branches.AddNew();

		var newCompanyAccountCollection = new AccountCollectionTestBuilder(newCompany.PK)
			.AppendAccount("22222222222-001", "2345").AppendAccountDetail("2345-DEC1", "DEC1")
			.AppendAccount("22222222222-002", "6789").AppendAccountDetail("6789-DEC1", "DEC1")
			.Build();

		CombineAssertions(() =>
		{
			var accountWithCompanyList = externalPassword.Lookups.AllAccountsWithCompany.ToArray();
			AssertEquals(4, accountWithCompanyList.Length);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"1234 - FIRST",
					"5678 - FIRST",
					"2345 - SECOND",
					"6789 - SECOND",
				},
				accountWithCompanyList.Select(a => $"{a.Account.AccountNode} - {a.Company.CompanyName}"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
	}

	protected abstract T GetExternalPassword();
}

using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GlbBrokerExternalPasswordLookupsTest : GlbExternalPasswordLookupsTest<GlbBrokerExternalPassword>
{
	public void TestNodes()
	{
		var externalPassword = GetExternalPassword();
		AssertEquals("No nodes", "", externalPassword.Lookups.Nodes.CodesAsString);
		Factory.ClearCachedValue<List<AccountAndCompanyPair>>("GlbExternalPasswordLookups_IT.AllAccountsWithCompany");

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		company.CompanyName = "FIRST";
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-DEC1", "DEC1")
			.AppendAccount("11111111111-002", "5678").AppendAccountDetail("5678-DEC1", "DEC1")
			.Build();

		var newCompany = Factory.NewWithValidTestData<GlbCompany>();
		newCompany.CompanyName = "SECOND";
		newCompany.Branches.AddNew();
		new AccountCollectionTestBuilder(newCompany.PK)
			.AppendAccount("22222222222-001", "2345").AppendAccountDetail("2345-DEC1", "DEC1")
			.AppendAccount("22222222222-002", "6789").AppendAccountDetail("6789-DEC1", "DEC1")
			.Build();

		AssertContainsExactElementsInAnyOrder(
			new[]
			{
				"1234 - FIRST",
				"5678 - FIRST",
				"2345 - SECOND",
				"6789 - SECOND",
			},
			externalPassword.Lookups.Nodes.ToArray().Select(n => $"{n.Code} - {n.Description}"));
	}

	protected override GlbBrokerExternalPassword GetExternalPassword()
	{
		var staff = Factory.NewWithValidTestData<GlbStaff>();
		var staffWrapper = GlbStaffWrapper.Get(staff);
		return staffWrapper.PasswordCollection.AddNew();
	}
}

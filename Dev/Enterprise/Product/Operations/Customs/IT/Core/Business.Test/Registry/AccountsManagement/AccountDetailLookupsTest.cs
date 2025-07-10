using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class AccountDetailLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestContructor()
	{
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new AccountDetailLookups(null));
	}

	public void TestAccountDeclarants()
	{
		var declarants = accountDetailLookups.Declarants;
		AssertNotNull("Not null", declarants);
		AssertType<OrgHeaderCollection>("Expected type", declarants);
	}

	protected override void SetUp()
	{
		base.SetUp();

		accountDetailLookups = new AccountDetailLookups(Factory);
	}
	AccountDetailLookups accountDetailLookups;
}

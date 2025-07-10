using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class AccountLookupTest : BusinessObjectLookupsTestCase
{
	public void TestAccountStatusList()
	{
		var statusList = accountLookups.AccountStatusList;
		AssertNotNull("Not null", statusList);
		AssertType<AccountStatusList>("Expected type", statusList);
		AssertSame("Expected cached", statusList, accountLookups.AccountStatusList);
	}

	public void TestAccountCertificateStatusList()
	{
		var certificateStatusList = accountLookups.AccountCertificateStatusList;
		AssertNotNull("Not null", certificateStatusList);
		AssertType<AccountCertificateStatusList>("Expected type", certificateStatusList);
		AssertSame("Expected cached", certificateStatusList, accountLookups.AccountCertificateStatusList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		accountLookups = new AccountLookups(Factory);
	}

	AccountLookups accountLookups;
}

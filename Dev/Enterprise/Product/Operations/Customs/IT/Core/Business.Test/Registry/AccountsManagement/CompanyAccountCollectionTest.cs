using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class CompanyAccountCollectionTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var accountCollection = new AccountCollection();
		CombineAssertions("Test constructor", () =>
		{
			AssertNoExceptionThrown("No exception expected", () => new CompanyAccountCollection(companyPK: ZGuid.Empty, accounts: null));
			AssertNoExceptionThrown("No exception expected", () => new CompanyAccountCollection(companyPK: ZGuid.NewZGuid(), accountCollection));
		});
	}

	public void TestProperties()
	{
		var accountCollection = new AccountCollection();
		accountCollection.AddNew();
		var companyPk = ZGuid.NewZGuid();
		var companyAccountCollection = new CompanyAccountCollection(companyPk, accountCollection);

		AssertEquals("CompanyPK", companyPk, companyAccountCollection.CompanyPK);
		AssertNotNull("Accounts", companyAccountCollection.Accounts);
		AssertEquals("Accounts.Count", 1, companyAccountCollection.Accounts.Count);
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class AccountAndCompanyPairTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AccountAndCompanyPair(account: null, company: null));
		AssertExceptionThrown<ArgumentNullException>(() => new AccountAndCompanyPair(account: account, company: null));
		AssertExceptionThrown<ArgumentNullException>(() => new AccountAndCompanyPair(account: null, company: company));
		AssertNoExceptionThrown(() => new AccountAndCompanyPair(account: account, company: company));
	}

	public void TestAccountAndCompany()
	{
		var accountWithCompany = new AccountAndCompanyPair(account: account, company: company);
		AssertSame(account, accountWithCompany.Account);
		AssertSame(company, accountWithCompany.Company);
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

		var accountCollection = new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		account = accountCollection.Cast<Account>().First();
	}

	Account account;
	GlbCompany company;
}

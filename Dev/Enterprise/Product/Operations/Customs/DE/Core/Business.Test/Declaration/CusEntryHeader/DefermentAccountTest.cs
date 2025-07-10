using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DefermentAccountTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new DefermentAccount(null, null));
		}

		public void TestType()
		{
			AssertEquals("E", Provider.Type);
		}

		public void TestType_NoValidAccount()
		{
			AssertNull(ProviderWithoutValidAccount.Type);
		}

		public void TestApplicationType()
		{
			AssertEquals("10", Provider.ApplicationType);
		}

		public void TestApplicationType_NoValidAccount()
		{
			AssertNull(ProviderWithoutValidAccount.ApplicationType);
		}

		public void TestAccountPrefix()
		{
			AssertEquals("F", Provider.AccountPrefix);
		}

		public void TestAccountPrefix_NoValidAccount()
		{
			AssertNull(ProviderWithoutValidAccount.AccountPrefix);
		}

		public void TestAccountNumber()
		{
			AssertEquals(AccountNumberTest, Provider.AccountNumber);
		}

		public void TestAccountNumber_NoValidAccount()
		{
			AssertNull(ProviderWithoutValidAccount.AccountNumber);
		}

		public void TestAuthorisationNumber()
		{
			AssertEquals("BINBINBIN", Provider.AuthorisationNumber);
		}

		public void TestAuthorisationNumber_NoValidAccount()
		{
			AssertNull(ProviderWithoutValidAccount.AuthorisationNumber);
		}

		public void TestApplicant()
		{
			account.CZ_Type = "";
			TestHelper.CreateCL010CoutryList(Factory);
			AssertEquals("GR1234567", Provider.Applicant);
		}

		public void TestAccountHolder()
		{
			account.CZ_Type = "";
			AssertEquals("Account Holder", Provider.AccountHolder);
		}

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1234567", Core.Constants.CountryCodes.Greece);
			organisation.OH_FullName = "Account Holder";
			account = organisation.AddDefermentAccountNumber("E", AccountNumberTest);
			account.CZ_Type = "10"; // Application type
			account.CZ_Issuer = "F"; // prefix
			account.DecryptedPassword = "BINBINBIN"; // bin
		}

		OrgHeader organisation;
		OrgCusAccount account;

		DefermentAccount Provider => provider ??= new DefermentAccount(organisation, AccountNumberTest);
		DefermentAccount provider;

		DefermentAccount ProviderWithoutValidAccount => providerWithoutValidAccount ??= new DefermentAccount(organisation, "XXX");
		DefermentAccount providerWithoutValidAccount;

		const string AccountNumberTest = "123456";
	}
}

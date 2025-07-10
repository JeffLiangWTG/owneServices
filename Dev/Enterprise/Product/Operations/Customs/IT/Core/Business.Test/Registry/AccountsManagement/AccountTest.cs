using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(Account))]
sealed class AccountTest : RegistryBusinessObjectTemplateTestCase<Account>
{
	public void TestAccountStatusDisplay()
	{
		var accounts = GetNewCollection();
		var account = accounts.AddNew();

		account.AccountStatus = ZString.Empty;
		AssertEquals("Empty AccountStatus", ZString.Empty, account.AccountStatusDisplay);

		account.AccountStatus = AccountStatusList.Codes.Valid;
		AssertEquals("VAL AccountStatus", AccountStatusList.Descriptions.Valid, account.AccountStatusDisplay);

		account.AccountStatus = AccountStatusList.Codes.Invalid;
		AssertEquals("INV AccountStatus", AccountStatusList.Descriptions.Invalid, account.AccountStatusDisplay);

		account.AccountStatus = "XXX";
		AssertEquals("XXX AccountStatus", ZString.Empty, account.AccountStatusDisplay);
	}

	[TestDate(2020, 01, 01)]
	public void TestCheckAccountCertificateStatus()
	{
		var accounts = GetNewCollection();
		var account = accounts.AddNew();

		account.AccountCertificate = null;
		account.AccountCertificatePassword = "test";

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Valid, account.AccountCertificateStatus);

		account.AccountCertificatePassword = "";

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Valid, account.AccountCertificateStatus);

		account.AccountCertificate = new byte[] { 241, 40 };

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Invalid, account.AccountCertificateStatus);

		account.AccountCertificatePassword = validPassword;
		account.AccountCertificate = validCertificate;

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Valid, account.AccountCertificateStatus);

		account.AccountCertificateExpirationDate = ZDate.Today.AddDays(-2);

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Expired, account.AccountCertificateStatus);

		account.AccountCertificateExpirationDate = ZDate.Today.AddDays(2);

		Factory.Save();
		AssertEquals("Account Certificate Status should be", AccountCertificateStatusList.Codes.Valid, account.AccountCertificateStatus);
	}

	public void TestAccountCertificateStatusDisplay()
	{
		var accounts = GetNewCollection();
		var account = accounts.AddNew();

		account.AccountCertificateStatus = ZString.Empty;
		AssertEquals("Empty AccountCertificateStatus", ZString.Empty, account.AccountCertificateStatusDisplay);

		account.AccountCertificateStatus = AccountCertificateStatusList.Codes.Valid;
		AssertEquals("VAL AccountCertificateStatus", AccountCertificateStatusList.Descriptions.Valid, account.AccountCertificateStatusDisplay);

		account.AccountCertificateStatus = AccountCertificateStatusList.Codes.Invalid;
		AssertEquals("INV AccountCertificateStatus", AccountCertificateStatusList.Descriptions.Invalid, account.AccountCertificateStatusDisplay);

		account.AccountCertificateStatus = AccountCertificateStatusList.Codes.Expired;
		AssertEquals("EXP AccountCertificateStatus", AccountCertificateStatusList.Descriptions.Expired, account.AccountCertificateStatusDisplay);

		account.AccountCertificateStatus = "XXX";
		AssertEquals("XXX AccountCertificateStatus", ZString.Empty, account.AccountCertificateStatusDisplay);
	}

	public void TestDeclarantTaxNumber()
	{
		var account = GetNewCollection().AddNew();
		AssertEquals("[PRE-CONDITION] DeclarantTaxNumber", ZString.Empty, account.DeclarantTaxNumber);

		AssertDeclarantTaxNumberPart(account, "12345678901-123", expectedTaxNumber: "12345678901", validationErrorExpected: false);
		AssertDeclarantTaxNumberPart(account, "12", expectedTaxNumber: "12", validationErrorExpected: true);
		AssertDeclarantTaxNumberPart(account, "A-", expectedTaxNumber: "A", validationErrorExpected: true);
		AssertDeclarantTaxNumberPart(account, "-01", expectedTaxNumber: ZString.Empty, validationErrorExpected: true);
		AssertDeclarantTaxNumberPart(account, "-", expectedTaxNumber: ZString.Empty, validationErrorExpected: true);
	}

	void AssertDeclarantTaxNumberPart(Account account, string accountNumber, ZString expectedTaxNumber, bool validationErrorExpected)
	{
		AssertAccountNumberPart(account, accountNumber, "DeclarantTaxNumber", () => account.DeclarantTaxNumber, expectedTaxNumber, validationErrorExpected);
	}

	public void TestWorkstationSequentialNumber()
	{
		var account = GetNewCollection().AddNew();
		AssertEquals("[PRE-CONDITION] WorkstationSequentialNumber", 0, account.WorkstationSequentialNumber);

		AssertSequentialNumberPart(account, "12345678901-123", expectedSeqNumber: 123, validationErrorExpected: false);
		AssertSequentialNumberPart(account, "12345678901-A11", expectedSeqNumber: 0, validationErrorExpected: false);
		AssertSequentialNumberPart(account, "12", expectedSeqNumber: 0, validationErrorExpected: true);
		AssertSequentialNumberPart(account, "A-", expectedSeqNumber: 0, validationErrorExpected: true);
		AssertSequentialNumberPart(account, "-01", expectedSeqNumber: 1, validationErrorExpected: true);
		AssertSequentialNumberPart(account, "-", expectedSeqNumber: 0, validationErrorExpected: true);
	}

	void AssertSequentialNumberPart(Account account, string accountNumber, ZInt expectedSeqNumber, bool validationErrorExpected)
	{
		AssertAccountNumberPart(account, accountNumber, "WorkstationSequentialNumber", () => account.WorkstationSequentialNumber, expectedSeqNumber, validationErrorExpected);
	}

	void AssertAccountNumberPart<T>(Account account, string accountNumber, string ptyName, Func<T> getActualValue, T expectedValue, bool validationErrorExpected) where T : IZType
	{
		account.AccountNumber = accountNumber;

		CombineAssertions($"AccountNumber = {accountNumber}", () =>
		{
			AssertEquals(ptyName, expectedValue, getActualValue());
			AssertEquals("HasErrors", validationErrorExpected, account.AccountNumberInfo.HasErrors());
		});
	}

	public void TestAccountNumberMaxLength()
	{
		var account = GetNewCollection().AddNew();
		AssertEquals(20, account.AccountNumberInfo.MaxLength);
	}

	public void TestAccountRangeStart()
	{
		var account = GetNewCollection().AddNew();

		account.AccountRangeStart = "1";
		AssertEquals("AccountRangeStart", "01", account.AccountRangeStart);

		account.AccountRangeStart = "A";
		AssertEquals("AccountRangeStart", "0A", account.AccountRangeStart);

		account.AccountRangeStart = "a";
		AssertEquals("AccountRangeStart", "0a", account.AccountRangeStart);

		account.AccountRangeStart = "";
		AssertEquals("AccountRangeStart", "", account.AccountRangeStart);
	}

	public void TestAccountRangeEnd()
	{
		var account = GetNewCollection().AddNew();

		account.AccountRangeEnd = "2";
		AssertEquals("AccountRangeEnd", "02", account.AccountRangeEnd);

		account.AccountRangeEnd = "B";
		AssertEquals("AccountRangeStart", "0B", account.AccountRangeEnd);

		account.AccountRangeEnd = "b";
		AssertEquals("AccountRangeEnd", "0b", account.AccountRangeEnd);

		account.AccountRangeEnd = "";
		AssertEquals("AccountRangeEnd", "", account.AccountRangeEnd);
	}

	public void TestLookups()
	{
		var account = new Account();

		AssertNotNull("Account lookups should not be null", account.Lookups);
		AssertType<AccountLookups>("Account lookups type should be", account.Lookups);
	}

	public void TestAccountCertificateExpirationDateDefaulting()
	{
		var account = GetNewCollection().AddNew();
		var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);

		void SetValidCertificate()
		{
			account.AccountCertificate = X509Certificate2TestHelper.ValidCertificate;
			account.AccountCertificatePassword = X509Certificate2TestHelper.ValidPassword;
		}

		CombineAssertions("Case 1: first certificate info setup run", () =>
		{
			AssertEquals(ZDate.Empty, account.AccountCertificateExpirationDate);

			account.AccountCertificate = X509Certificate2TestHelper.ValidCertificate;
			AssertEquals(ZDate.Empty, account.AccountCertificateExpirationDate);

			account.AccountCertificatePassword = X509Certificate2TestHelper.ValidPassword;
			AssertEquals((ZDate)validCertificate.NotAfter, account.AccountCertificateExpirationDate);
		});

		CombineAssertions("Case 2: once setup, by setting invalid certificate binary data the expiration date will be cleared", () =>
		{
			SetValidCertificate();
			AssertEquals("PRE-CONDITION", (ZDate)validCertificate.NotAfter, account.AccountCertificateExpirationDate);

			account.AccountCertificate = new byte[1];
			AssertEquals(ZDate.Empty, account.AccountCertificateExpirationDate);
		});

		CombineAssertions("Case 3: once setup, by setting invalid certificate password the expiration date will be cleared", () =>
		{
			SetValidCertificate();
			AssertEquals("PRE-CONDITION", (ZDate)validCertificate.NotAfter, account.AccountCertificateExpirationDate);

			account.AccountCertificatePassword = "invalid password";
			AssertEquals(ZDate.Empty, account.AccountCertificateExpirationDate);
		});
	}

	public void TestAccountCertificateExpirationDateReadOnly()
	{
		var account = GetNewCollection().AddNew();
		Assert(account.AccountCertificateExpirationDateInfo.ReadOnly);
	}

	public void TestEmcsNotificationEnabled()
	{
		var account = GetNewCollection().AddNew();
		account.EmcsNotificationEnabled = true;
		account.ExciseNumbers.AddNew().Number = "XXX";
		account.ExciseNumbers.AddNew().Number = "YYY";
		AssertEquals("Expected filled excise numbers collection", 2, account.ExciseNumbers.Count);

		account.EmcsNotificationEnabled = false;
		AssertEquals("Expected empty excise numbers collection", 0, account.ExciseNumbers.Count);

		account.ExciseNumbers.AddNew().Number = "ZZZ";
		AssertEquals("Just for test purposes (not real world example), adding excise numbers even if EmcsNotificationEnabled = false is allowed", 1, account.ExciseNumbers.Count);
	}

	public void TestCloneValidExciseNumbers()
	{
		var account = GetNewCollection().AddNew();
		account.EmcsNotificationEnabled = true;
		var exciseNumber1 = account.ExciseNumbers.AddNew();
		exciseNumber1.Number = "XXX";
		var exciseNumber2 = account.ExciseNumbers.AddNew();
		exciseNumber2.Number = "YYY";
		exciseNumber2.Delete();
		var exciseNumber3 = account.ExciseNumbers.AddNew();
		exciseNumber3.Number = "ZZZ";

		var clonedAccount = (Account)account.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		AssertEquals("Expected count", 2, clonedAccount.ExciseNumbers.Count);
		AssertEquals("Expected first excise number", "XXX", clonedAccount.ExciseNumbers[0].Number);
		AssertEquals("Expected third excise number", "ZZZ", clonedAccount.ExciseNumbers[1].Number);
	}

	public void TestExciseNumbersReadOnly()
	{
		var account = GetNewCollection().AddNew();
		account.EmcsNotificationEnabled = true;
		Assert("EMCS notification enabled", !account.ExciseNumbers.ReadOnly);

		account.EmcsNotificationEnabled = false;
		Assert("EMCS notification disabled", account.ExciseNumbers.ReadOnly);
	}

	public void TestSetCustomDefaultValues()
	{
		var account = GetNewCollection().AddNew();
		Assert("Default value for EmcsNotificationEnabled", !account.EmcsNotificationEnabled);
		AssertEquals("Default account status", AccountStatusList.Codes.Valid, account.AccountStatus);
	}

	public void TestAccountMustHaveAtLeastOneAccountDetail()
	{
		const string expectedRowError = "At least one row of Account Details must be filled";

		var account = GetNewCollection().AddNew();

		AssertEquals("[PRE-CONDITION], AccountDetails count", 0, account.AccountDetails.Count);
		account.RunPreSaveValidation();
		AssertHasRowErrorContaining(account, expectedRowError);

		account.AccountDetails.AddNew();
		AssertNoRowErrorContaining(account, expectedRowError);

		account.AccountDetails.RemoveAndDeleteAll();
		AssertHasRowErrorContaining(account, expectedRowError);
	}

	protected override bool RequiresFactory
	{
		get { return true; }
	}

	protected override bool RequiresFallbackLevel
	{
		get { return true; }
	}

	AccountCollection GetNewCollection()
	{
		return new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		AccountCollection accountCollection = GetNewCollection();
		Account result = accountCollection.AddNew();
		return result;
	}

	protected override Account GetBusinessObjectToClone()
	{
		return (Account)GetNewBusinessObject();
	}

	protected override Account GetBusinessObjectToSerialise()
	{
		return GetBusinessObjectToClone();
	}

	#region Certificate

	readonly byte[] validCertificate = X509Certificate2TestHelper.ValidCertificate;
	readonly string validPassword = X509Certificate2TestHelper.ValidPassword;

	#endregion
}

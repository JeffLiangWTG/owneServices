using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(Account))]
sealed class AccountValidationTest : RegistryBusinessObjectTemplateTestCase<Account>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when Account parameter is null", () => new AccountValidation(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Should be exception when Factory parameter is null", () => new AccountValidation(account, null));
		AssertNoExceptionThrown("Should not thrown any exception.", () => new AccountValidation(account, Factory));
	}

	public void TestValidateAccountNumber()
	{
		account.AccountNumber = "12345678901234";
		AssertEquals("VAT case - wrong format", true, account.AccountNumberInfo.HasError(ValidationCaptions.Account.AccountLengthMustBe15Or20));

		account.AccountNumber = "";
		AssertEquals("empty number", true, account.AccountNumberInfo.HasError(ValidationCaptions.Account.EnterAccountNumber));

		account.AccountNumber = "ABCDEFGHIJK-LMN";
		AssertEquals("VAT case - valid", false, account.AccountNumberInfo.HasErrors());

		account.AccountNumber = "ABCDEFGHIJKL-MN";
		AssertEquals("VAT case - invalid", true, account.AccountNumberInfo.HasError(ValidationCaptions.Account.AccountFormat));

		account.AccountNumber = "ABCDEF70A21F205Q-001";
		AssertEquals("Fiscal code case - valid", false, account.AccountNumberInfo.HasErrors());
	}

	public void TestValidateDuplicatedAccountNumber()
	{
		var duplicatedAccountInTheSameCompany = accounts.AddNew();
		account.AccountNumber = "ABCDEFGHIJK-LMN";
		account.AccountPassword = "abc123456";

		duplicatedAccountInTheSameCompany.AccountNumber = "ABCDEFGHIJK-LMN";
		AssertEquals("AccountNumber has errors", true, duplicatedAccountInTheSameCompany.AccountNumberInfo.HasError("Account Number must be unique"));

		duplicatedAccountInTheSameCompany.AccountNumber = "ABCDEFGHIJK-000";
		AssertEquals("AccountNumber has errors", false, duplicatedAccountInTheSameCompany.AccountNumberInfo.HasError("Account Number must be unique"));

		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.GC_Name = "C01";
		otherCompany.Branches.AddNew();

		var accountsForOtherCompany = accounts.Clone(new FallbackLevel(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		var accountLineForOtherCompany1 = accountsForOtherCompany.AddNew();
		var expectedAccountNumberErrorMessage = "This Account Number already exists in company C01, it is not possible to have duplicated Account Numbers";

		accountLineForOtherCompany1.AccountNumber = "09876543210-321";

		account.AccountNumber = "09876543210-321";
		AssertEquals("AccountNumber has errors", true, account.AccountNumberInfo.HasError(expectedAccountNumberErrorMessage));

		account.AccountNumber = "01234567890-123";
		AssertEquals("AccountNumber has errors", false, account.AccountNumberInfo.HasError(expectedAccountNumberErrorMessage));
	}

	public void TestValidateAccountPassword()
	{
		account.AccountPassword = "1234567";
		AssertEquals(true, account.AccountPasswordInfo.HasError(PasswordLengthMustBeBetween8and15));

		account.AccountPassword = "GoodPassword";
		AssertEquals(false, account.AccountPasswordInfo.HasErrors());

		account.AccountPassword = "";
		AssertEquals(true, account.AccountPasswordInfo.HasError(EnterPassword));

		account.AccountPassword = "SpecialChars!";
		AssertEquals(true, account.AccountPasswordInfo.HasError(PasswordFormat));
	}

	internal const string EnterPassword = "Enter a password.";
	internal const string PasswordLengthMustBeBetween8and15 = "Password must be between 8 and 15 characters.";
	internal const string PasswordFormat = "Only letters and numbers can be used for the password.";

	public void TestValidateAccountCertificate()
	{
		account.AccountCertificatePassword = "";
		account.AccountCertificate = new byte[] { 241, 40 };
		AssertEquals(true, account.AccountCertificateInfo.HasError(CertificateOrPasswordInvalid));

		account.AccountCertificate = null;
		account.AccountCertificatePassword = "password";
		AssertEquals(false, account.AccountCertificateInfo.HasError(CertificateOrPasswordInvalid));

		account.AccountCertificate = validCert;
		AssertEquals(true, account.AccountCertificateInfo.HasError(CertificateOrPasswordInvalid));

		account.AccountCertificatePassword = "password";
		AssertEquals(true, account.AccountCertificateInfo.HasError(CertificateOrPasswordInvalid));

		account.AccountCertificatePassword = "CaRgOw1sE";
		AssertEquals(false, account.AccountCertificateInfo.HasError(CertificateOrPasswordInvalid));
	}

	internal const string CertificateOrPasswordInvalid = "The Certificate or accompanying password is invalid.";

	public void TestValidateCertificatePassword()
	{
		account.AccountCertificate = new byte[] { 120, 200 };
		account.AccountCertificatePassword = "";
		AssertEquals(true, account.AccountCertificatePasswordInfo.HasError(PasswordRequiredForCertificate));

		account.AccountCertificatePassword = "password";
		AssertEquals(false, account.AccountCertificatePasswordInfo.HasError(PasswordRequiredForCertificate));
	}

	public void TestValidateNode()
	{
		var expectedErrorMessage = "Node field must be filled with the customs user code consisting of 4 characters.";

		account.AccountNode = ZString.Empty;
		AssertHasErrorContaining("When Account Node is empty", account.AccountNodeInfo, expectedErrorMessage);

		account.AccountNode = "12";
		AssertHasErrorContaining("When Account Node length less than 4 chars", account.AccountNodeInfo, expectedErrorMessage);

		AssertExceptionThrown("When Account Node length more than 4 chars", typeof(MaxLengthExceededException), "The maximum length of 'AccountNode' has been exceeded", () => account.AccountNode = "12345", true);
		ClearExceptionReporter();

		account.AccountNode = "1234";
		AssertNoErrorContaining("When Account Node length equal to 4 chars", account.AccountNodeInfo, expectedErrorMessage);
	}

	public void TestValidateDuplicatedNode()
	{
		account.AccountNode = "XXXX";

		var duplicatedAccountInTheSameCompany = accounts.AddNew();
		duplicatedAccountInTheSameCompany.AccountNode = "XXXX";
		AssertEquals("AccountNode has errors", true, duplicatedAccountInTheSameCompany.AccountNodeInfo.HasError("Account Node must be unique"));

		duplicatedAccountInTheSameCompany.AccountNode = "YYYY";
		AssertEquals("AccountNode has errors", false, duplicatedAccountInTheSameCompany.AccountNodeInfo.HasError("Account Node must be unique"));

		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.GC_Name = "C01";
		otherCompany.Branches.AddNew();

		var accountsForOtherCompany = accounts.Clone(new FallbackLevel(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		var accountLineForOtherCompany1 = accountsForOtherCompany.AddNew();

		var expectedAccountNumberErrorMessage = "This Account Node already exists in company C01, it is not possible to have duplicated Account Nodes";
		accountLineForOtherCompany1.AccountNode = "ZZZZ";

		account.AccountNode = "ZZZZ";
		AssertEquals("AccountNode has errors", true, account.AccountNodeInfo.HasError(expectedAccountNumberErrorMessage));

		account.AccountNode = "GGGG";
		AssertEquals("AccountNode has errors", false, account.AccountNodeInfo.HasError(expectedAccountNumberErrorMessage));
	}

	public void TestValidateRangeStart()
	{
		var expectedInvalidFormatErrorMessage = "Account Range must be in the format [0-9][A-Z][a-z]";

		account.AccountRangeStart = "--";
		AssertEquals("[RangeStart: --] AccountRangeStart should contain error", true, account.AccountRangeStartInfo.HasError(expectedInvalidFormatErrorMessage));

		account.AccountRangeStart = "00";
		AssertEquals("[RangeStart: 00] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedInvalidFormatErrorMessage));

		account.AccountRangeStart = "zz";
		AssertEquals("[RangeStart: zz] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedInvalidFormatErrorMessage));

		var expectedRangeStartMustBeLessThanRangeEnd = "Account Range Start must be less than Account Range End";

		account.AccountRangeEnd = "aa";
		account.AccountRangeStart = "b0";
		AssertEquals("[RangeStart: b0, RangeEnd: aa] AccountRangeStart should contain error", true, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeStart = "aa";
		AssertEquals("[RangeStart: aa, RangeEnd: aa] AccountRangeStart should contain error", true, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeEnd = "AA";
		account.AccountRangeStart = "aa";
		AssertEquals("[RangeStart: aa, RangeEnd: AA] WI00290107 Handle duplicated response messages v4 AccountRangeStart should contain error", true, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeEnd = "aa";
		account.AccountRangeStart = "AA";
		AssertEquals("[RangeStart: AA, RangeEnd: aa] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeEnd = "";
		account.AccountRangeStart = "aa";
		AssertEquals("[RangeStart: aa, RangeEnd: Empty] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeEnd = "--";
		account.AccountRangeStart = "aa";
		AssertEquals("[RangeStart: aa, RangeEnd: --] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));

		account.AccountRangeEnd = "aa";
		account.AccountRangeStart = "01";
		AssertEquals("[RangeStart: 01, RangeEnd: aa] AccountRangeStart should not contain error", false, account.AccountRangeStartInfo.HasError(expectedRangeStartMustBeLessThanRangeEnd));
	}

	public void TestValidateRangeEnd()
	{
		var expectedInvalidFormatErrorMessage = "Account Range must be in the format [0-9][A-Z][a-z]";

		account.AccountRangeEnd = "--";
		AssertEquals("[RangeEnd: --] AccountRangeEnd should contain error", true, account.AccountRangeEndInfo.HasError(expectedInvalidFormatErrorMessage));

		account.AccountRangeEnd = "00";
		AssertEquals("[RangeEnd: 00] AccountRangeEnd should not contain error", false, account.AccountRangeEndInfo.HasError(expectedInvalidFormatErrorMessage));

		account.AccountRangeEnd = "xx";
		AssertEquals("[RangeEnd: xx] AccountRangeEnd should not contain error", false, account.AccountRangeEndInfo.HasError(expectedInvalidFormatErrorMessage));

		var expectedRangeEndMustBeGreaterThanRangeStart = "Account Range End must be greater than Account Range Start";

		account.AccountRangeStart = "dd";
		account.AccountRangeEnd = "cc";
		AssertEquals("[RangeStart: dd, RangeEnd: cc] AccountRangeEnd should contain error", true, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));

		account.AccountRangeEnd = "dd";
		AssertEquals("[RangeStart: dd, RangeEnd: dd] AccountRangeEnd should contain error", true, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));

		account.AccountRangeStart = "cc";
		account.AccountRangeEnd = "CC";
		AssertEquals("[RangeStart: cc, RangeEnd: CC] AccountRangeEnd should contain error", true, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));

		account.AccountRangeStart = "CC";
		account.AccountRangeEnd = "cc";
		AssertEquals("[RangeStart: CC, RangeEnd: cc] AccountRangeEnd should not contain error", false, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));

		account.AccountRangeStart = "";
		account.AccountRangeEnd = "aa";
		AssertEquals("[RangeStart: Empty, RangeEnd: aa] AccountRangeEnd should not contain error", false, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));

		account.AccountRangeStart = "01";
		account.AccountRangeEnd = "aa";
		AssertEquals("[RangeStart: 01, RangeEnd: aa] AccountRangeEnd should not contain error", false, account.AccountRangeEndInfo.HasError(expectedRangeEndMustBeGreaterThanRangeStart));
	}

	public void TestValidateEmcsNotificationEnabled()
	{
		account.EmcsNotificationEnabled = false;
		AssertNoErrorContaining("No excise numbers and disabled EMCS notification", account.EmcsNotificationEnabledInfo, ValidationCaptions.Account.AccountMustHaveAtLeastOneExciseNumber);

		account.EmcsNotificationEnabled = true;
		account.ExciseNumbers.AddNew();
		AssertNoErrorContaining("One excise number and enabled EMCS notification", account.EmcsNotificationEnabledInfo, ValidationCaptions.Account.AccountMustHaveAtLeastOneExciseNumber);

		account.ExciseNumbers.RemoveAndDeleteAll();
		AssertHasErrorContaining("No excise numbers and enabled EMCS notification", account.EmcsNotificationEnabledInfo, ValidationCaptions.Account.AccountMustHaveAtLeastOneExciseNumber);

		account.ExciseNumbers.AddNew();
		account.EmcsNotificationEnabled = false;
		AssertNoErrorContaining("No excise numbers (collection cleared) and disabled EMCS notification", account.EmcsNotificationEnabledInfo, ValidationCaptions.Account.AccountMustHaveAtLeastOneExciseNumber);
	}

	public void TestValidateAccountStatus()
	{
		account.AccountStatus = ZString.Empty;
		CombineAssertions("Empty AccountStatus", () =>
		{
			AssertHasErrorContaining(account.AccountStatusInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(account.AccountStatusInfo, ListValidation.InvalidCodeError);
		});

		account.AccountStatus = "XXX";
		CombineAssertions("XXX AccountStatus", () =>
		{
			AssertNoErrorContaining(account.AccountStatusInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(account.AccountStatusInfo, ListValidation.InvalidCodeError);
		});

		account.AccountStatus = AccountStatusList.Codes.Invalid;
		AssertNoErrors("INV AccountStatus", account.AccountStatusInfo);

		account.AccountStatus = AccountStatusList.Codes.Valid;
		AssertNoErrors("VAL AccountStatus", account.AccountStatusInfo);
	}

	internal const string PasswordRequiredForCertificate = "A Certificate Password is required if a Certificate is loaded.";

	#region Implementation

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override BusinessObject GetNewBusinessObject() => account;

	protected override Account GetBusinessObjectToClone() => (Account)GetNewBusinessObject();

	protected override Account GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

	protected override void SetUp()
	{
		accounts = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		account = accounts.AddNew();
		base.SetUp();
	}

	AccountCollection accounts;
	Account account;

	void ClearExceptionReporter() => ExceptionReporterTestListener.Instance.Clear();

	#endregion Implementation

	#region Certificate Sample

	readonly byte[] validCert = new byte[]
		{ 48, 130, 8, 50, 2, 1, 3, 48, 130, 7, 236, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 7, 221, 4, 130, 7, 217, 48, 130, 7, 213, 48,
			130, 3, 26, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 160, 130, 3, 11, 4, 130, 3, 7, 48, 130, 3, 3, 48, 130, 2, 255, 6, 11, 42, 134, 72,
			134, 247, 13, 1, 12, 10, 1, 2, 160, 130, 2, 178, 48, 130, 2, 174, 48, 40, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 3, 48, 26, 4, 20, 3,
			201, 53, 132, 254, 125, 104, 210, 117, 214, 94, 59, 92, 119, 146, 241, 28, 192, 223, 218, 2, 2, 4, 0, 4, 130, 2, 128, 193, 8, 229,
			243, 255, 114, 100, 3, 49, 196, 12, 225, 34, 75, 208, 74, 147, 224, 211, 120, 110, 214, 57, 117, 191, 242, 23, 218, 199, 80, 207, 10,
			107, 209, 177, 156, 176, 248, 43, 130, 127, 83, 149, 50, 161, 6, 133, 70, 236, 228, 69, 59, 2, 9, 176, 223, 127, 233, 66, 217, 27, 217,
			74, 203, 86, 0, 233, 240, 40, 57, 16, 75, 40, 47, 121, 60, 239, 32, 234, 137, 160, 30, 197, 86, 203, 216, 181, 140, 9, 50, 21, 143, 5,
			59, 218, 110, 137, 45, 138, 235, 121, 40, 96, 234, 4, 165, 76, 167, 91, 205, 82, 167, 49, 95, 251, 24, 134, 34, 7, 251, 212, 216, 238,
			213, 71, 65, 29, 129, 201, 224, 146, 8, 135, 185, 217, 58, 232, 87, 208, 74, 224, 148, 150, 170, 100, 188, 99, 42, 167, 167, 82, 170, 23,
			92, 144, 203, 90, 218, 241, 134, 238, 92, 122, 79, 51, 97, 186, 192, 9, 18, 207, 7, 66, 231, 87, 117, 30, 212, 186, 98, 174, 112, 240, 84,
			154, 224, 65, 208, 247, 185, 70, 5, 151, 151, 55, 181, 208, 96, 50, 146, 52, 254, 50, 131, 246, 102, 103, 207, 250, 170, 163, 71, 18, 81,
			227, 232, 85, 65, 229, 35, 90, 16, 23, 44, 237, 222, 74, 198, 213, 158, 207, 50, 8, 197, 205, 254, 133, 147, 36, 1, 240, 200, 49, 186,
			187, 81, 177, 128, 154, 220, 114, 248, 219, 123, 255, 120, 145, 173, 117, 193, 45, 62, 252, 4, 253, 45, 63, 130, 151, 182, 148, 128, 144,
			197, 106, 114, 139, 237, 85, 110, 129, 239, 180, 29, 69, 238, 249, 4, 226, 21, 48, 91, 122, 204, 86, 200, 189, 254, 227, 60, 244, 50, 125,
			202, 94, 37, 110, 24, 242, 245, 1, 74, 16, 38, 25, 247, 53, 251, 40, 45, 118, 0, 13, 158, 197, 62, 159, 195, 163, 235, 71, 132, 47, 200,
			183, 195, 230, 204, 33, 136, 204, 103, 11, 83, 68, 171, 93, 51, 147, 240, 1, 21, 94, 183, 43, 200, 208, 135, 29, 248, 184, 43, 62, 249, 241,
			188, 74, 46, 161, 80, 29, 15, 31, 95, 43, 140, 108, 163, 129, 55, 144, 133, 225, 57, 24, 123, 95, 169, 120, 250, 236, 34, 44, 151, 162, 31,
			115, 12, 103, 74, 104, 215, 246, 116, 106, 3, 87, 200, 201, 152, 144, 67, 151, 154, 36, 104, 139, 91, 8, 43, 156, 13, 187, 87, 50, 98, 184, 41,
			235, 39, 167, 18, 64, 250, 107, 141, 158, 192, 176, 237, 221, 191, 67, 29, 105, 13, 91, 182, 133, 21, 221, 213, 51, 24, 30, 2, 12, 109, 82, 56,
			233, 137, 172, 165, 191, 86, 80, 38, 37, 182, 230, 100, 124, 86, 55, 65, 55, 220, 110, 66, 244, 174, 182, 16, 63, 138, 110, 90, 184, 91, 31, 10,
			238, 71, 61, 125, 198, 244, 231, 122, 179, 134, 172, 62, 216, 4, 62, 238, 234, 49, 52, 255, 4, 241, 93, 62, 178, 154, 170, 99, 76, 41, 72, 196,
			144, 160, 3, 186, 197, 221, 92, 72, 154, 164, 118, 192, 20, 105, 10, 0, 159, 83, 80, 238, 70, 120, 240, 9, 21, 238, 143, 74, 83, 41, 204, 84, 112,
			231, 75, 49, 241, 78, 56, 58, 108, 196, 95, 83, 148, 78, 149, 194, 39, 99, 162, 45, 217, 32, 208, 108, 12, 0, 71, 38, 27, 226, 183, 55, 154, 205,
			102, 86, 225, 45, 9, 109, 42, 164, 125, 107, 168, 152, 37, 206, 189, 141, 178, 154, 26, 30, 240, 63, 223, 161, 98, 69, 94, 55, 61, 153, 219, 60,
			107, 141, 229, 253, 233, 61, 144, 221, 129, 5, 217, 230, 142, 114, 152, 238, 214, 192, 49, 58, 48, 21, 6, 9, 42, 134, 72, 134, 247, 13, 1, 9, 20,
			49, 8, 30, 6, 0, 115, 0, 115, 0, 108, 48, 33, 6, 9, 42, 134, 72, 134, 247, 13, 1, 9, 21, 49, 20, 4, 18, 84, 105, 109, 101, 32, 49, 53, 48, 48, 54,
			50, 53, 53, 54, 57, 53, 56, 49, 48, 130, 4, 179, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 6, 160, 130, 4, 164, 48, 130, 4, 160, 2, 1, 0, 48, 130, 4,
			153, 6, 9, 42, 134, 72, 134, 247, 13, 1, 7, 1, 48, 40, 6, 10, 42, 134, 72, 134, 247, 13, 1, 12, 1, 6, 48, 26, 4, 20, 234, 207, 97, 241, 214, 134,
			128, 105, 23, 115, 99, 96, 119, 67, 69, 148, 76, 159, 190, 28, 2, 2, 4, 0, 128, 130, 4, 96, 153, 80, 254, 57, 80, 169, 35, 66, 207, 142, 31, 56,
			40, 132, 110, 177, 214, 116, 131, 219, 73, 122, 115, 40, 201, 21, 98, 192, 249, 135, 45, 20, 224, 8, 250, 82, 88, 173, 208, 2, 182, 227, 100, 8,
			203, 4, 49, 233, 77, 182, 11, 173, 161, 0, 115, 172, 77, 159, 119, 139, 107, 103, 208, 28, 211, 24, 234, 139, 85, 85, 247, 128, 75, 25, 13, 180,
			120, 38, 166, 53, 96, 194, 230, 27, 86, 5, 175, 22, 153, 102, 123, 2, 90, 62, 29, 113, 203, 63, 229, 21, 135, 242, 242, 233, 65, 182, 126, 52, 64,
			158, 146, 175, 43, 70, 66, 134, 109, 175, 144, 117, 121, 34, 181, 4, 49, 201, 127, 70, 154, 95, 8, 160, 135, 151, 211, 46, 12, 138, 129, 40, 169,
			87, 189, 34, 116, 183, 216, 228, 140, 17, 27, 73, 202, 200, 146, 210, 109, 67, 162, 1, 54, 58, 99, 44, 115, 34, 41, 7, 37, 255, 191, 180, 77, 170,
			86, 72, 236, 63, 95, 113, 73, 95, 97, 203, 239, 100, 170, 130, 182, 9, 79, 240, 72, 81, 46, 226, 72, 126, 191, 194, 199, 188, 110, 161, 32, 106,
			39, 217, 198, 79, 22, 195, 185, 126, 75, 240, 93, 177, 124, 127, 136, 98, 124, 2, 82, 98, 125, 57, 185, 131, 213, 208, 106, 54, 191, 103, 21, 33,
			153, 150, 254, 80, 201, 224, 243, 67, 49, 77, 239, 106, 43, 110, 209, 104, 245, 6, 243, 51, 82, 181, 239, 212, 228, 145, 45, 14, 83, 59, 246, 191,
			42, 65, 89, 123, 179, 251, 179, 10, 103, 238, 119, 96, 164, 187, 172, 150, 202, 159, 245, 78, 33, 51, 60, 105, 96, 119, 28, 168, 231, 169, 91, 109,
			5, 212, 204, 9, 154, 109, 84, 203, 198, 51, 102, 117, 119, 15, 105, 253, 74, 143, 27, 156, 4, 85, 190, 123, 110, 183, 100, 193, 136, 8, 23, 200, 72,
			48, 48, 65, 31, 30, 14, 164, 191, 15, 250, 1, 202, 120, 44, 210, 129, 127, 131, 197, 148, 229, 103, 178, 147, 184, 39, 0, 53, 64, 172, 233, 232, 31,
			119, 53, 104, 241, 101, 4, 216, 8, 195, 25, 29, 49, 51, 168, 85, 111, 47, 181, 219, 232, 157, 221, 250, 17, 244, 183, 201, 140, 99, 229, 91, 141,
			247, 43, 143, 241, 146, 163, 111, 31, 157, 159, 218, 17, 14, 223, 42, 18, 75, 172, 223, 174, 37, 1, 39, 228, 25, 250, 240, 136, 39, 79, 253, 167,
			209, 254, 39, 118, 207, 246, 181, 68, 32, 255, 151, 147, 197, 195, 45, 73, 56, 181, 56, 122, 131, 78, 13, 105, 136, 137, 97, 212, 7, 13, 198, 90, 52,
			66, 64, 126, 24, 120, 225, 63, 176, 27, 104, 34, 67, 176, 133, 48, 59, 164, 79, 67, 139, 152, 143, 15, 45, 97, 105, 109, 50, 245, 91, 207, 110, 206,
			56, 96, 201, 195, 38, 118, 185, 44, 52, 230, 215, 161, 101, 88, 41, 113, 218, 245, 169, 47, 111, 3, 204, 185, 196, 166, 171, 178, 39, 240, 90, 232,
			90, 23, 39, 60, 16, 255, 10, 93, 174, 248, 171, 51, 88, 152, 15, 52, 245, 110, 243, 103, 7, 80, 112, 128, 51, 40, 154, 125, 224, 159, 54, 222, 21, 222,
			121, 227, 159, 6, 170, 3, 19, 159, 174, 86, 38, 149, 84, 250, 21, 89, 173, 61, 192, 152, 106, 197, 233, 178, 158, 175, 131, 231, 238, 82, 85, 178, 204,
			100, 43, 40, 204, 18, 72, 129, 81, 125, 51, 238, 170, 100, 251, 39, 164, 75, 83, 100, 229, 27, 53, 130, 212, 179, 6, 138, 32, 196, 108, 102, 154, 114,
			59, 18, 72, 192, 77, 29, 246, 201, 169, 223, 45, 60, 160, 105, 228, 201, 27, 249, 252, 160, 10, 231, 249, 115, 203, 106, 249, 147, 108, 81, 85, 113, 121,
			165, 157, 92, 84, 15, 122, 166, 94, 212, 182, 86, 239, 6, 217, 238, 148, 49, 36, 116, 177, 209, 155, 16, 186, 192, 59, 232, 64, 18, 171, 50, 245, 170, 14,
			104, 33, 87, 43, 115, 236, 66, 126, 226, 203, 131, 223, 160, 121, 190, 58, 15, 179, 185, 160, 8, 191, 214, 16, 108, 12, 90, 58, 33, 116, 137, 145, 170, 91,
			243, 12, 229, 178, 59, 71, 95, 199, 175, 152, 233, 81, 30, 54, 59, 184, 125, 202, 107, 63, 134, 73, 96, 255, 234, 85, 241, 13, 255, 174, 38, 20, 187, 180,
			43, 213, 134, 180, 119, 184, 89, 183, 205, 98, 141, 238, 219, 13, 201, 99, 64, 250, 202, 234, 231, 92, 121, 72, 195, 16, 142, 252, 175, 25, 223, 19, 137,
			221, 59, 157, 124, 0, 218, 144, 221, 174, 41, 122, 146, 169, 147, 94, 230, 175, 143, 245, 203, 205, 204, 96, 122, 254, 91, 202, 60, 136, 218, 171, 172, 96,
			8, 161, 142, 140, 198, 148, 75, 62, 177, 174, 233, 81, 65, 35, 20, 92, 62, 10, 191, 207, 174, 44, 51, 245, 247, 59, 175, 178, 89, 77, 223, 58, 169, 145, 202,
			126, 241, 93, 211, 120, 161, 216, 51, 249, 16, 118, 158, 228, 224, 161, 85, 193, 236, 199, 189, 115, 11, 83, 186, 167, 15, 179, 172, 233, 173, 139, 214,
			220, 168, 232, 149, 91, 218, 244, 30, 127, 25, 100, 122, 138, 248, 81, 116, 4, 140, 235, 70, 143, 18, 191, 83, 134, 137, 39, 66, 220, 116, 243, 126, 60, 205,
			193, 150, 71, 38, 177, 219, 130, 98, 187, 54, 115, 44, 27, 37, 4, 246, 53, 44, 208, 33, 91, 67, 50, 174, 131, 172, 6, 199, 120, 145, 154, 246, 66, 213, 237,
			36, 254, 205, 117, 160, 226, 100, 106, 202, 84, 59, 190, 193, 92, 146, 41, 134, 84, 233, 237, 139, 24, 16, 171, 38, 194, 103, 222, 25, 134, 183, 28, 17, 100,
			189, 46, 71, 137, 220, 207, 187, 39, 156, 133, 91, 250, 66, 17, 41, 146, 41, 200, 173, 101, 44, 204, 47, 237, 46, 58, 75, 213, 223, 226, 252, 213, 226, 117,
			89, 205, 234, 179, 179, 136, 187, 205, 132, 254, 126, 232, 166, 247, 55, 71, 172, 182, 78, 83, 78, 157, 59, 209, 46, 48, 123, 192, 20, 90, 127, 7, 242, 179,
			123, 46, 131, 65, 211, 159, 242, 189, 97, 189, 114, 148, 77, 58, 77, 191, 127, 218, 17, 16, 27, 3, 71, 120, 255, 180, 65, 196, 169, 237, 7, 52, 76, 243, 247,
			213, 83, 237, 117, 38, 223, 105, 226, 246, 53, 51, 212, 50, 235, 150, 185, 48, 61, 48, 33, 48, 9, 6, 5, 43, 14, 3, 2, 26, 5, 0, 4, 20, 165, 236, 46, 49, 120,
			101, 103, 122, 101, 135, 254, 168, 124, 35, 161, 176, 197, 228, 148, 112, 4, 20, 94, 152, 178, 64, 157, 196, 49, 13, 135, 220, 30, 60, 140, 118, 42, 41, 144,
			244, 15, 196, 2, 2, 4, 0 };

	#endregion
}

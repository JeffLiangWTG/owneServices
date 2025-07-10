using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IT.Registry.Testing;

sealed class AccountDetailValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("accountDetail is required", () => new AccountDetailValidation(null));
	}

	public void TestCheckInternalCode()
	{
		const string expectedFormatErrorMessage = "Allow only chars [A-Z][0-9][-_]";

		accountDetail.Validation.ValidateInternalCode();
		AssertHasErrorContaining("Empty Internal Code", accountDetail.InternalCodeInfo, MandatoryValidation.MustBeEntered);

		accountDetail.InternalCode = "AA3";
		AssertNoErrorContaining("Internal Code", accountDetail.InternalCodeInfo, MandatoryValidation.MustBeEntered);

		CombineAssertions(() =>
		{
			accountDetail.InternalCode = "aaA";
			AssertHasErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "?<>";
			AssertHasErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "A G";
			AssertHasErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "AAAA-01";
			AssertNoErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "_AAA-90";
			AssertHasErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "1LEOPARDO";
			AssertNoErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);

			accountDetail.InternalCode = "A";
			AssertNoErrorContaining(accountDetail.InternalCodeInfo, expectedFormatErrorMessage);
		});
	}

	public void TestCheckInternalCodeMustBeUnique()
	{
		const string InternalCodeMustBeUnique = "Internal Code must be unique";
		const string InternalCodeExistInAnotherCompany = "This Account Internal Code already exists in company C01, it is not possible to have duplicated Account Internal Codes";

		accountDetail.InternalCode = "CODE1";
		var accountDetail2 = account.AccountDetails.AddNew();
		accountDetail2.InternalCode = "CODE1";
		AssertHasErrorContaining("Duplicated Internal Code", accountDetail2.InternalCodeInfo, InternalCodeMustBeUnique);

		accountDetail2.InternalCode = "CODE2";
		AssertNoErrorContaining("Valid Unique Internal Code", accountDetail2.InternalCodeInfo, InternalCodeMustBeUnique);

		var newAccount = accountCollection.AddNew();
		var accountDetail3 = newAccount.AccountDetails.AddNew();
		accountDetail3.InternalCode = "CODE1";
		AssertHasErrorContaining("Duplicated Internal Code", accountDetail3.InternalCodeInfo, InternalCodeMustBeUnique);

		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.GC_Name = "C01";
		otherCompany.Branches.AddNew();

		var accountsForOtherCompany = accountCollection.Clone(new FallbackLevel(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		var accountLineForOtherCompany1 = accountsForOtherCompany.AddNew();

		accountLineForOtherCompany1.AccountDetails.AddNew().InternalCode = "CODE3";

		accountDetail2.InternalCode = "CODE3";
		AssertHasErrorContaining("Duplicated Internal Code", accountDetail2.InternalCodeInfo, InternalCodeExistInAnotherCompany);

		accountDetail2.InternalCode = "GGGG";
		AssertNoErrorContaining("Valid Unique Internal Code", accountDetail2.InternalCodeInfo, InternalCodeExistInAnotherCompany);
	}

	public void TestCheckInternalCodeMinLenghtValidation()
	{
		const string expectedError = "Internal Code must be at least 5 chars length.";

		CombineAssertions(() =>
		{
			accountDetail.InternalCode = "ABCD";
			AssertHasErrorContaining(accountDetail.InternalCodeInfo, expectedError);

			accountDetail.InternalCode = "ABCDE";
			AssertNoErrorContaining(accountDetail.InternalCodeInfo, expectedError);
		});
	}

	public void TestCheckAccountDeclarantCode()
	{
		var declarant = Factory.New<OrgHeader>();
		declarant.OH_Code = "DEC";

		accountDetail.DeclarantCode = ZString.Empty;
		accountDetail.Validation.ValidateDeclarantCode();
		AssertHasErrorContaining("DeclarantCode Not Entered", accountDetail.DeclarantCodeInfo, MandatoryValidation.MustBeEntered);

		accountDetail.DeclarantCode = "INV";
		AssertHasErrorContaining("DeclarantCode Invalid Code", accountDetail.DeclarantCodeInfo, ListValidation.InvalidCodeError);

		accountDetail.DeclarantCode = "DEC";
		AssertNoErrors("DeclarantCode Valid Code", accountDetail.DeclarantCodeInfo);
	}

	public void TestAuthorizedUser()
	{
		accountDetail.AuthorizedUser = ZString.Empty;
		accountDetail.Validation.ValidateAuthorizedUser();
		AssertHasErrorContaining("AuthorizedUser Not Entered", accountDetail.AuthorizedUserInfo, MandatoryValidation.MustBeEntered);

		const string expectedWarning = "The value should be in the form X-NNN, EG: 11111111111-001";

		accountDetail.AuthorizedUser = "AAA ee 2";
		AssertHasWarningContaining(accountDetail.AuthorizedUserInfo, expectedWarning);

		accountDetail.AuthorizedUser = "999";
		AssertHasWarningContaining(accountDetail.AuthorizedUserInfo, expectedWarning);

		accountDetail.AuthorizedUser = "11111-999";
		AssertNoWarnings(accountDetail.AuthorizedUserInfo);

		accountDetail.AuthorizedUser = "AAABCD-999";
		AssertNoWarningContaining(accountDetail.AuthorizedUserInfo, expectedWarning);

		accountDetail.AuthorizedUser = "AAABCD";
		AssertHasWarningContaining(accountDetail.AuthorizedUserInfo, expectedWarning);
	}

	public void TestDeclarantAndAuthorizedUserMustBeUnique()
	{
		const string expectedError = "A row with the same Declarant and Authorized User already exists for this Account";

		var accountDetail2 = account.AccountDetails.AddNew();

		CombineAssertions(() =>
		{
			accountDetail.AuthorizedUser = ZString.Empty;
			accountDetail.DeclarantCode = ZString.Empty;
			accountDetail2.AuthorizedUser = ZString.Empty;
			accountDetail2.DeclarantCode = ZString.Empty;
			AssertNoErrorContaining(accountDetail2.AuthorizedUserInfo, expectedError);
			AssertNoErrorContaining(accountDetail2.DeclarantCodeInfo, expectedError);

			accountDetail.AuthorizedUser = "02028530281-001";
			accountDetail.DeclarantCode = "ACOXMLMIL";
			accountDetail2.AuthorizedUser = "02028530281-001";
			accountDetail2.DeclarantCode = "ACOXMLMIL";

			AssertHasErrorContaining(accountDetail2.DeclarantCodeInfo, expectedError);
			accountDetail2.Validation.ValidateAuthorizedUser();
			AssertHasErrorContaining(accountDetail2.AuthorizedUserInfo, expectedError);
			accountDetail.Validation.ValidateDeclarantCode();
			AssertHasErrorContaining(accountDetail.DeclarantCodeInfo, expectedError);
			accountDetail.Validation.ValidateAuthorizedUser();
			AssertHasErrorContaining(accountDetail.AuthorizedUserInfo, expectedError);

			accountDetail2.AuthorizedUser = "02028530281-002";
			AssertNoErrorContaining(accountDetail2.AuthorizedUserInfo, expectedError);
			accountDetail.Validation.ValidateDeclarantCode();
			AssertNoErrorContaining(accountDetail.DeclarantCodeInfo, expectedError);

			accountDetail2.AuthorizedUser = "02028530281-001";
			accountDetail.DeclarantCode = "DECLARANT";
			AssertNoErrorContaining(accountDetail.DeclarantCodeInfo, expectedError);
			accountDetail2.Validation.ValidateAuthorizedUser();
			AssertNoErrorContaining(accountDetail2.AuthorizedUserInfo, expectedError);

			accountDetail.AuthorizedUser = ZString.Empty;
			accountDetail2.AuthorizedUser = ZString.Empty;
			AssertNoErrorContaining(accountDetail.AuthorizedUserInfo, expectedError);
			AssertNoErrorContaining(accountDetail2.AuthorizedUserInfo, expectedError);
			accountDetail.Validation.ValidateDeclarantCode();
			accountDetail2.Validation.ValidateDeclarantCode();
			AssertNoErrorContaining(accountDetail.DeclarantCodeInfo, expectedError);
			AssertNoErrorContaining(accountDetail2.DeclarantCodeInfo, expectedError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		accountCollection = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		account = accountCollection.AddNew();
		accountDetail = account.AccountDetails.AddNew();
	}
	AccountCollection accountCollection;
	Account account;
	AccountDetail accountDetail;
}

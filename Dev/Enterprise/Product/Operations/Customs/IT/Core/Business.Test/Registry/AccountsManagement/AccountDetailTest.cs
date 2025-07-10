using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(AccountDetail))]
sealed class AccountDetailTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("account is required", () => new AccountDetail(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("factory is required", () => new AccountDetail(account, null));
	}

	public void TestInternalCode()
	{
		var resourceStringData = accountDetail.InternalCodeInfo.GetAttribute<ResourceStringDataAttribute>();

		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(AccountDetail.InternalCode)} MaxLenght", 20, accountDetail.InternalCodeInfo.MaxLength);

			AssertEquals("Caption", "Internal Code", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Int. Code", resourceStringData.ShortCaption);
		});
	}

	public void TestDeclarantCode()
	{
		var resourceStringData = accountDetail.DeclarantCodeInfo.GetAttribute<ResourceStringDataAttribute>();

		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Declarant", resourceStringData.Caption);
			AssertEquals("FullDescription", "Declarant Organization Code", resourceStringData.FullDescription);
		});
	}

	public void TestAuthorizedUser()
	{
		var resourceStringData = accountDetail.AuthorizedUserInfo.GetAttribute<ResourceStringDataAttribute>();

		CombineAssertions(() =>
		{
			AssertEquals($"{nameof(AccountDetail.AuthorizedUserInfo)} MaxLenght", 20, accountDetail.AuthorizedUserInfo.MaxLength);

			AssertEquals("Caption", "Authorized User", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Auth. User", resourceStringData.ShortCaption);
		});
	}

	public void TestDeclarant()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_Code = "DECL";
		declarant.Addresses[0].OA_RN_NKCountryCode = "IT";

		Factory.Save();

		accountDetail.DeclarantCode = "DECL";
		AssertNotNull("Declarant should not be null", accountDetail.Declarant);
		AssertEquals("Declarant PK", declarant.PK, accountDetail.Declarant.PK);

		accountDetail.DeclarantCode = "";
		AssertNull("Declarant should be null", accountDetail.Declarant);

		accountDetail.DeclarantCode = "XXYYZZ";
		AssertNull("Declarant should be null", accountDetail.Declarant);
	}

	public void TestDeclarantTaxNumber()
	{
		AssertEquals("[PRE-CONDITION] DeclarantTaxNumber", ZString.Empty, accountDetail.DeclarantTaxNumber);

		AssertDeclarantTaxNumberPart(accountDetail, "12345678901-123", expectedTaxNumber: "12345678901");
		AssertDeclarantTaxNumberPart(accountDetail, "12", expectedTaxNumber: "12");
		AssertDeclarantTaxNumberPart(accountDetail, "A-", expectedTaxNumber: "A");
		AssertDeclarantTaxNumberPart(accountDetail, "-01", expectedTaxNumber: ZString.Empty);
		AssertDeclarantTaxNumberPart(accountDetail, "-", expectedTaxNumber: ZString.Empty);
	}

	public void TestWorkstationSequentialNumber()
	{
		AssertEquals("[PRE-CONDITION] WorkstationSequentialNumber", 0, accountDetail.WorkstationSequentialNumber);

		AssertSequentialNumberPart(accountDetail, "12345678901-123", expectedSeqNumber: 123);
		AssertSequentialNumberPart(accountDetail, "12345678901-A11", expectedSeqNumber: 0);
		AssertSequentialNumberPart(accountDetail, "12", expectedSeqNumber: 0);
		AssertSequentialNumberPart(accountDetail, "A-", expectedSeqNumber: 0);
		AssertSequentialNumberPart(accountDetail, "-01", expectedSeqNumber: 1);
		AssertSequentialNumberPart(accountDetail, "-", expectedSeqNumber: 0);
	}

	void AssertDeclarantTaxNumberPart(AccountDetail accountDetail, string authorizedUser, ZString expectedTaxNumber)
	{
		AssertAccountNumberPart(accountDetail, authorizedUser, "DeclarantTaxNumber", () => accountDetail.DeclarantTaxNumber, expectedTaxNumber);
	}

	void AssertSequentialNumberPart(AccountDetail accountDetail, string authorizedUser, ZInt expectedSeqNumber)
	{
		AssertAccountNumberPart(accountDetail, authorizedUser, "WorkstationSequentialNumber", () => accountDetail.WorkstationSequentialNumber, expectedSeqNumber);
	}

	void AssertAccountNumberPart<T>(AccountDetail accountDetail, string authorizedUser, string ptyName, Func<T> getActualValue, T expectedValue)
		where T : IZType
	{
		accountDetail.AuthorizedUser = authorizedUser;
		AssertEquals($"AuthorizedUser = {authorizedUser}, {ptyName}", expectedValue, getActualValue());
	}

	protected override void SetUp()
	{
		base.SetUp();

		account = new AccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory).AddNew();
		accountDetail = new AccountDetail(account, Factory);
	}
	Account account;
	AccountDetail accountDetail;

	protected override BusinessObject GetNewBusinessObject()
	{
		return new AccountDetail(account, Factory);
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsCredentialHelperTest : TestCaseWithFactory
{
	public void TestGetAccountFromNode()
	{
		var acc1234 = CustomsCredentialHelper.GetAccountFromNode("1234");
		AssertNotNull("Account from node = 1234", acc1234);
		AssertEquals("11111111111-001", acc1234.AccountNumber);

		var accXXXX = CustomsCredentialHelper.GetAccountFromNode("XXXX");
		AssertNotNull("Account from node = XXXX", accXXXX);
		AssertEquals("22222222222-001", accXXXX.AccountNumber);

		AssertNull("Account from empty node string", CustomsCredentialHelper.GetAccountFromNode(""));
		AssertNull("Account from node not in the registry", CustomsCredentialHelper.GetAccountFromNode("12A1"));
	}

	public void TestGetAccountDetailFromInternalCode()
	{
		var acc1234 = CustomsCredentialHelper.GetAccountDetailFromInternalCode("1234-CODE1");
		AssertNotNull("Account from Internal Code = 1234", acc1234);
		AssertEquals("AuthorizedUser", "AUTHUSER1", acc1234.AuthorizedUser);

		var accXXXX = CustomsCredentialHelper.GetAccountDetailFromInternalCode("XXXX-CODE1");
		AssertNotNull("Account from Internal Code = XXXX", accXXXX);
		AssertEquals("AuthorizedUser", "AUTHUSER2", accXXXX.AuthorizedUser);

		AssertNull("Account from empty Internal Code string", CustomsCredentialHelper.GetAccountDetailFromInternalCode(""));
		AssertNull("Account from node not in the registry", CustomsCredentialHelper.GetAccountDetailFromInternalCode("12A1"));
	}

	public void TestGetAccountFromInternalCode()
	{
		var acc1234 = CustomsCredentialHelper.GetAccountFromInternalCode("1234-CODE1");
		AssertNotNull("Account from Internal Code = 1234", acc1234);
		AssertEquals("11111111111-001", acc1234.AccountNumber);

		var accXXXX = CustomsCredentialHelper.GetAccountFromInternalCode("XXXX-CODE1");
		AssertNotNull("Account from Internal Code = XXXX", accXXXX);
		AssertEquals("22222222222-001", accXXXX.AccountNumber);

		AssertNull("Account from empty Internal Code string", CustomsCredentialHelper.GetAccountFromInternalCode(""));
		AssertNull("Account from node not in the registry", CustomsCredentialHelper.GetAccountFromInternalCode("12A1"));
	}

	public void TestGetAccounts()
	{
		var accounts = CustomsCredentialHelper.GetAccounts();
		AssertNotNull(accounts);
		AssertNotNull(accounts.SingleOrDefault(x => x.AccountNode == "1234"));
		AssertNotNull(accounts.SingleOrDefault(x => x.AccountNode == "XXXX"));
	}

	public void TestGetAccountDetailsForDeclarant()
	{
		var accountDetails = CustomsCredentialHelper.GetAccountDetailsForDeclarant("");
		AssertEquals("No accounts for empty declarant", 0, accountDetails.Count());

		accountDetails = CustomsCredentialHelper.GetAccountDetailsForDeclarant("NOTEXISTING");
		AssertEquals("No accounts for non existing declarant", 0, accountDetails.Count());

		var internalCodes = CustomsCredentialHelper.GetAccountDetailsForDeclarant("CODE1").Select(x => x.InternalCode);
		AssertEquals("Two accounts available for declarant", 2, internalCodes.Count());
		AssertEquals("1234-CODE1 is available", true, internalCodes.Contains("1234-CODE1"));
		AssertEquals("XXXX-CODE1 is available", true, internalCodes.Contains("XXXX-CODE1"));
	}

	public void TestGetAllAccountDetailsForCompany()
	{
		var companyAccounts = CustomsCredentialHelper.GetAllAccountDetailsForCompany().Select(x => x.InternalCode);
		AssertEquals("Available list of nodes for company", 2, companyAccounts.Count());
		AssertEquals("1234-CODE1 is available", true, companyAccounts.Contains("1234-CODE1"));
		AssertEquals("XXXX-CODE1 is available", true, companyAccounts.Contains("XXXX-CODE1"));
	}

	public void TestGetDeclarantFromInternalCode()
	{
		AssertNull(CustomsCredentialHelper.GetDeclarantFromInternalCode(""));
		AssertNull(CustomsCredentialHelper.GetDeclarantFromInternalCode("1"));
		var declarantFromNode = CustomsCredentialHelper.GetDeclarantFromInternalCode("1234-CODE1");
		AssertNotNull(declarantFromNode);
		AssertEquals(organizationHeader.PK, declarantFromNode.PK);
	}

	public void TestGetNodeFromInternalCode()
	{
		AssertEquals("GetNodeFromInternalCode()", "", CustomsCredentialHelper.GetNodeFromInternalCode("<INTCODE>"));
		AssertEquals("GetNodeFromInternalCode()", "XXXX", CustomsCredentialHelper.GetNodeFromInternalCode("XXXX-CODE1"));
	}

	public void TestInternalCodeIsValid()
	{
		CombineAssertions("InternalCodeIsValid", () =>
		{
			AssertEquals(true, CustomsCredentialHelper.InternalCodeIsValid("1234-CODE1"));
			AssertEquals(false, CustomsCredentialHelper.InternalCodeIsValid(""));
			AssertEquals(false, CustomsCredentialHelper.InternalCodeIsValid("XXXX"));
			AssertEquals(true, CustomsCredentialHelper.InternalCodeIsValid("XXXX-CODE1"));
		});
	}

	public void TestCurrentUserHasFiscalCode()
	{
		var temporaryStaff = Factory.NewWithValidTestData<GlbStaff>();
		Factory.Save();

		using (Env.Instance.SetTemporaryUserContext(temporaryStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			AssertEquals(false, CustomsCredentialHelper.CurrentUserHasFiscalCode());

			var codCertificate = temporaryStaff.Certificates.AddNew();
			codCertificate.XZ_Type = ItalyOrgCusCodeInfo.OrgCusCodes.CodiceFiscale;
			codCertificate.XZ_RefNumber = "00891230153";
			Factory.Save();

			AssertEquals(true, CustomsCredentialHelper.CurrentUserHasFiscalCode());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		organizationHeader = Factory.New<OrgHeader>();
		organizationHeader.OH_Code = "CODE1";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-CODE1", "CODE1", "AUTHUSER1")
			.AppendAccount("22222222222-001", "XXXX").AppendAccountDetail("XXXX-CODE1", "CODE1", "AUTHUSER2")
			.Build();
	}

	OrgHeader organizationHeader;
}

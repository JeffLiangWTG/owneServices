using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Registry.Testing;

[TestedType(typeof(ITAccountsManagementRegistry))]
sealed class ITAccountsManagementRegistryTest : RegistryItemSetTestCaseWithFactory<ITAccountsManagementRegistry>
{
	[TestDate(2019, 11, 14)]
	public void TestOnAllValuesSavedAction()
	{
		Factory.New<OrgHeader>().OH_Code = "AA";
		Factory.New<OrgHeader>().OH_Code = "ZZ";
		Factory.Save();

		var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();
		var registry = ITAccountsManagementRegistry.Instance.AccountsManagement.GetValueWithoutFallback(currentCompanyPk, Guid.Empty, Guid.Empty);
		AssertEquals(0, registry.Count);
		var accounts = new AccountCollection();
		var accountLine1 = accounts.AddNew();
		accountLine1.AccountNumber = "01234567890-123";
		accountLine1.AccountPassword = "abc123456";
		accountLine1.AccountPasswordExpirationDate = new ZDate(2020, 01, 01);
		accountLine1.AccountCertificatePassword = "def123456";
		accountLine1.AccountCertificateExpirationDate = new ZDate(2020, 01, 01);
		var account1Detail1 = accountLine1.AccountDetails.AddNew();
		accountLine1.AccountNode = "1111";
		account1Detail1.InternalCode = "1111-AA";
		account1Detail1.DeclarantCode = "AA";
		account1Detail1.AuthorizedUser = "00000-001";
		var accountDetail2 = accountLine1.AccountDetails.AddNew();
		accountDetail2.InternalCode = "1111-ZZ";
		accountDetail2.DeclarantCode = "ZZ";
		accountDetail2.AuthorizedUser = "00000-002";

		accountLine1.EmcsNotificationEnabled = true;
		accountLine1.ExciseNumbers.AddNew().Number = "IT00PD0000000";
		accountLine1.ExciseNumbers.AddNew().Number = "IT00MI0000000";
		accountLine1.ExciseNumbers.AddNew().Number = "IT00VE0000000";

		var accountLine2 = accounts.AddNew();
		accountLine2.AccountNumber = "01234567890-456";
		accountLine2.AccountPassword = "abc654321";
		accountLine2.AccountStatus = AccountStatusList.Codes.Invalid;
		accountLine2.AccountPasswordExpirationDate = new ZDate(2019, 01, 01);
		accountLine2.AccountCertificatePassword = "def654321";
		accountLine2.AccountCertificateExpirationDate = new ZDate(2019, 01, 01);
		accountLine2.AccountNode = "4444";
		var account2Detail = accountLine2.AccountDetails.AddNew();
		account2Detail.InternalCode = "4444-AA";
		account2Detail.DeclarantCode = "AA";
		account2Detail.AuthorizedUser = "0001-1";
		accountLine2.AccountRangeStart = "50";
		accountLine2.AccountRangeEnd = "dd";
		accountLine2.EmcsNotificationEnabled = false;
		accountLine2.ExciseNumbers.AddNew().Number = "IT00RM0000000";
		ITAccountsManagementRegistry.Instance.AccountsManagement.SetValue(currentCompanyPk, Guid.Empty, Guid.Empty, accounts);

		var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
		otherCompany.Branches.AddNew();
		Factory.Save();

		var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy);
		country.States.AddNew().RW_Code = "PD";
		country.States.AddNew().RW_Code = "MI";
		country.States.AddNew().RW_Code = "VE";
		country.States.AddNew().RW_Code = "RM";

		var accountsForOtherCompany = new AccountCollection();
		var accountLineForOtherCompany1 = accountsForOtherCompany.AddNew();
		accountLineForOtherCompany1.AccountNumber = "09876543210-321";
		accountLineForOtherCompany1.AccountPassword = "654321cba";
		accountLineForOtherCompany1.AccountCertificateExpirationDate = ZDate.Today.AddMonths(1);
		accountLineForOtherCompany1.AccountCertificatePassword = "psw1234";
		accountLineForOtherCompany1.AccountCertificateExpirationDate = ZDate.Today.AddMonths(1);
		accountLineForOtherCompany1.AccountNode = "5555";
		var accountLineForOtherCompany1Detail = accountLineForOtherCompany1.AccountDetails.AddNew();
		accountLineForOtherCompany1Detail.InternalCode = "5555-AA";
		accountLineForOtherCompany1Detail.DeclarantCode = "ZZ";
		accountLineForOtherCompany1Detail.AuthorizedUser = "001-2";
		accountLineForOtherCompany1.EmcsNotificationEnabled = false;
		ITAccountsManagementRegistry.Instance.AccountsManagement.SetValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, accountsForOtherCompany);

		var currentCompanyAccountList = ITAccountsManagementRegistry.Instance.AccountsManagement.GetValueWithoutFallback(currentCompanyPk, Guid.Empty, Guid.Empty).Cast<Account>().OrderBy(x => x.AccountNumber).ToArray();
		AssertEquals(2, currentCompanyAccountList.Length);
		CombineAssertions("Check Account property is saved correctly", () =>
		{
			var account = currentCompanyAccountList[0];
			AssertEquals("Account Number should be", "01234567890-123", account.AccountNumber);
			AssertEquals("Account Password should be", "abc123456", account.AccountPassword);
			AssertEquals("Account Password Expiration Date should be", new ZDate(2020, 01, 01), account.AccountPasswordExpirationDate);
			AssertEquals("Account Certificate Password should be", "def123456", account.AccountCertificatePassword);
			AssertEquals("Account Certificate Password Expiration Date should be", new ZDate(2020, 01, 01), account.AccountCertificateExpirationDate);
			AssertEquals("Account Node should be", "1111", account.AccountNode);
			AssertEquals("Account Details count", 2, account.AccountDetails.Count);
			var accountDetail1 = account.AccountDetails[0];
			AssertEquals("Internal Code", "1111-AA", accountDetail1.InternalCode);
			AssertEquals("Declarant Code", "AA", accountDetail1.DeclarantCode);
			AssertEquals("Authorized User", "00000-001", accountDetail1.AuthorizedUser);
		});

		ITAccountsManagementRegistry.Instance.AccountsManagement.OnUpdateAction(currentCompanyPk, Guid.Empty, Guid.Empty, accounts);
		ITAccountsManagementRegistry.Instance.AccountsManagement.OnUpdateAction(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, accountsForOtherCompany);
		ITAccountsManagementRegistry.Instance.AccountsManagement.OnAllValuesSavedAction();

		var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
		zQuery.OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + " DESC";
		var xml = Factory.LoadTop1<IEDIInterchange>(zQuery);
		#region Pieces of XML

		var piecesOfXML = new List<ZString>();
#if NETFRAMEWORK
		piecesOfXML.Add("<Configuration xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" Name=\"ITCustomsCredentials\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\">\r\n" +
"  <Group Type=\"System\" Reference=\"");
#else
		piecesOfXML.Add("<Configuration xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" Name=\"ITCustomsCredentials\" Version=\"1.0\" xmlns=\"http://www.wisetechglobal.com/Schemas/Configuration\">\r\n" +
"  <Group Type=\"System\" Reference=\"");
#endif
		piecesOfXML.Add("<Group Type=\"System\" Reference=\"");
		piecesOfXML.Add("<Group Type=\"MailboxID\" Reference=\"01234567890-123\" Status=\"VAL\">\r\n");
		piecesOfXML.Add("<Item Name=\"Node\">1111</Item>\r\n");
		piecesOfXML.Add("<Item Name=\"ExciseNumber\">IT00PD0000000</Item>\r\n");
		piecesOfXML.Add("<Item Name=\"ExciseNumber\">IT00MI0000000</Item>\r\n");
		piecesOfXML.Add("<Item Name=\"ExciseNumber\">IT00VE0000000</Item>\r\n");
		piecesOfXML.Add("<Credential Name=\"MailboxID\">\r\n");
		piecesOfXML.Add("<Password>");
		piecesOfXML.Add("</Password>\r\n");
		piecesOfXML.Add("</Credential>\r\n");
		piecesOfXML.Add("<Group Type=\"XMLWebService\" Status=\"VAL\">\r\n");
		piecesOfXML.Add("<Certificate Name=\"Certificate\">\r\n");
		piecesOfXML.Add("<File />\r\n");
		piecesOfXML.Add("<Passphrase>");
		piecesOfXML.Add("</Passphrase>\r\n");
		piecesOfXML.Add("</Certificate>\r\n");
		piecesOfXML.Add("</Group>\r\n");
		piecesOfXML.Add("</Group>\r\n");
		piecesOfXML.Add("<Group Type=\"MailboxID\" Reference=\"01234567890-456\" Status=\"INV\">\r\n");
		piecesOfXML.Add("<Item Name=\"Node\">4444</Item>\r\n");
		piecesOfXML.Add("<Credential Name=\"MailboxID\">\r\n");
		piecesOfXML.Add("<Password>");
		piecesOfXML.Add("</Password>\r\n");
		piecesOfXML.Add("</Credential>\r\n");
		piecesOfXML.Add("<Group Type=\"XMLWebService\" Status=\"INV\">\r\n");
		piecesOfXML.Add("<Certificate Name=\"Certificate\">\r\n");
		piecesOfXML.Add("<File />\r\n");
		piecesOfXML.Add("<Passphrase>");
		piecesOfXML.Add("</Passphrase>\r\n");
		piecesOfXML.Add("</Certificate>\r\n");
		piecesOfXML.Add("</Group>\r\n");
		piecesOfXML.Add("</Group>\r\n");
		piecesOfXML.Add("<Group Type=\"MailboxID\" Reference=\"09876543210-321\" Status=\"VAL\">\r\n");
		piecesOfXML.Add("<Item Name=\"Node\">5555</Item>\r\n");
		piecesOfXML.Add("<Credential Name=\"MailboxID\">\r\n");
		piecesOfXML.Add("<Password>");
		piecesOfXML.Add("</Password>\r\n");
		piecesOfXML.Add("</Credential>\r\n");
		piecesOfXML.Add("<Group Type=\"XMLWebService\" Status=\"VAL\">\r\n");
		piecesOfXML.Add("<Certificate Name=\"Certificate\">\r\n");
		piecesOfXML.Add("<File />\r\n");
		piecesOfXML.Add("<Passphrase>");
		piecesOfXML.Add("</Passphrase>\r\n");
		piecesOfXML.Add("</Certificate>\r\n");
		piecesOfXML.Add("</Group>\r\n");
		piecesOfXML.Add("</Group>\r\n");

#endregion

#if NET
		if (string.IsNullOrEmpty(xml.EI_BodyText))
		{
			throw new Exception("xml.EI_BodyText is null or empty");
		}
#endif
		piecesOfXML.ForEach(x =>
		{
			AssertContains("The XML should be properly generated with the specific elements", x, xml.EI_BodyText);
		});

		accountLineForOtherCompany1.AccountNode = "9999";
		ITAccountsManagementRegistry.Instance.AccountsManagement.OnUpdateAction(currentCompanyPk, Guid.Empty, Guid.Empty, accounts);
		ITAccountsManagementRegistry.Instance.AccountsManagement.OnUpdateAction(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, accountsForOtherCompany);
		ITAccountsManagementRegistry.Instance.AccountsManagement.OnAllValuesSavedAction();

		var newInterchange = Factory.LoadTop1<IEDIInterchange>(zQuery);
		AssertNotContains("Old AccountNode data for 'accountLineForOtherCompany1' is not contained in the interchange", "<Item Name=\"Node\">5555</Item>", newInterchange.EI_BodyText);
		AssertContains("New AccountNode data for 'accountLineForOtherCompany1' is contained in the interchange", "<Item Name=\"Node\">9999</Item>", newInterchange.EI_BodyText);
	}

	public void TestAccountCollectionRegistryItem()
	{
		TestGenericRegistryItem(
			ItemSet.AccountsManagement,
			"ITAccountsManagementRegistryItem",
			CustomsDataRegistry.Categories.Customs_Italy,
			"Accounts Management",
			"Capture Italian Customs Accounts and Nodes. These credentials are used by the system in order to communicate with Italian Customs FTP web service.",
			RegistryStorageFlags.Company);
	}

	public void TestIsForProductivityWise()
	{
		AssertEquals(false, ITAccountsManagementRegistry.Instance.IsForProductivityWise);
	}
}

using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EdiCommissionAgreementCustomizationTreeNodeTest : TestCaseWithFactory
	{
		public void TestSelected()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var licDatabase = licEnterprise.Databases.AddNew();
			var licCompany = licEnterprise.Companies.AddNew();
			licCompany.LC_LE = licEnterprise.PK;
			var org = Factory.New<EDIOrgHeader>();
			licCompany.LC_OH = org.PK;
			var clientCompanyAu1 = Factory.New<ClientCompany>();
			clientCompanyAu1.LCC_Code = "AU1";
			clientCompanyAu1.LCC_LD = licDatabase.PK;
			clientCompanyAu1.LCC_RN_NKCountryCode = "AU";
			var clientCompanyAu2 = Factory.New<ClientCompany>();
			clientCompanyAu2.LCC_Code = "AU2";
			clientCompanyAu2.LCC_LD = licDatabase.PK;
			clientCompanyAu2.LCC_RN_NKCountryCode = "AU";
			var clientCompanyUs1 = Factory.New<ClientCompany>();
			clientCompanyUs1.LCC_Code = "US1";
			clientCompanyUs1.LCC_LD = licDatabase.PK;
			clientCompanyUs1.LCC_RN_NKCountryCode = "US";
			var clientCompanyUs2 = Factory.New<ClientCompany>();
			clientCompanyUs2.LCC_Code = "US2";
			clientCompanyUs2.LCC_LD = licDatabase.PK;
			clientCompanyUs2.LCC_RN_NKCountryCode = "US";
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllCompanies = false;
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			var databaseNode = (EdiCommissionAgreementCustomizationTreeNode)model.RootNodes.Single(x => ((EdiCommissionAgreementDatabaseWrapper)x.BizObj).LicenceDatabase == licDatabase);
			var auNode = (EdiCommissionAgreementCustomizationTreeNode)databaseNode.ChildNodes.Single(x => ((EdiCommissionAgreementCountryWrapper)x.BizObj).CountryCode == "AU");
			var clientCompanyAu1Node = (EdiCommissionAgreementCustomizationTreeNode)auNode.ChildNodes.Single(x => ((EdiCommissionAgreementCompanyWrapper)x.BizObj).ClientCompany == clientCompanyAu1);
			var clientCompanyAu2Node = (EdiCommissionAgreementCustomizationTreeNode)auNode.ChildNodes.Single(x => ((EdiCommissionAgreementCompanyWrapper)x.BizObj).ClientCompany == clientCompanyAu2);
			var usNode = (EdiCommissionAgreementCustomizationTreeNode)databaseNode.ChildNodes.Single(x => ((EdiCommissionAgreementCountryWrapper)x.BizObj).CountryCode == "US");
			var clientCompanyUs1Node = (EdiCommissionAgreementCustomizationTreeNode)usNode.ChildNodes.Single(x => ((EdiCommissionAgreementCompanyWrapper)x.BizObj).ClientCompany == clientCompanyUs1);
			var clientCompanyUs2Node = (EdiCommissionAgreementCustomizationTreeNode)usNode.ChildNodes.Single(x => ((EdiCommissionAgreementCompanyWrapper)x.BizObj).ClientCompany == clientCompanyUs2);
			AssertEquals(CheckState.Unchecked, databaseNode.Selected);
			AssertEquals(CheckState.Unchecked, auNode.Selected);
			AssertEquals(CheckState.Unchecked, clientCompanyAu1Node.Selected);
			AssertEquals(CheckState.Unchecked, clientCompanyAu2Node.Selected);
			AssertEquals(CheckState.Unchecked, usNode.Selected);
			AssertEquals(CheckState.Unchecked, clientCompanyUs1Node.Selected);
			AssertEquals(CheckState.Unchecked, clientCompanyUs2Node.Selected);
			clientCompanyAu1Node.Selected = CheckState.Checked;
			AssertEquals(CheckState.Unchecked, clientCompanyAu2Node.Selected);
			AssertEquals(CheckState.Indeterminate, auNode.Selected);
			AssertEquals(CheckState.Indeterminate, databaseNode.Selected);
			clientCompanyAu2Node.Selected = CheckState.Checked;
			AssertEquals(CheckState.Checked, auNode.Selected);
			AssertEquals(CheckState.Indeterminate, databaseNode.Selected);
			usNode.Selected = CheckState.Checked;
			AssertEquals(CheckState.Checked, clientCompanyUs1Node.Selected);
			AssertEquals(CheckState.Checked, clientCompanyUs2Node.Selected);
			AssertEquals(CheckState.Checked, databaseNode.Selected);
			usNode.Selected = CheckState.Unchecked;
			AssertEquals(CheckState.Unchecked, clientCompanyUs1Node.Selected);
			AssertEquals(CheckState.Unchecked, clientCompanyUs2Node.Selected);
			AssertEquals(CheckState.Indeterminate, databaseNode.Selected);
		}

		public void TestShouldAutoAdd()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var licDatabase = licEnterprise.Databases.AddNew();
			var licCompany = licEnterprise.Companies.AddNew();
			licCompany.LC_LE = licEnterprise.PK;
			var org = Factory.New<EDIOrgHeader>();
			licCompany.LC_OH = org.PK;
			var clientCompanyAu1 = Factory.New<ClientCompany>();
			clientCompanyAu1.LCC_Code = "AU1";
			clientCompanyAu1.LCC_LD = licDatabase.PK;
			clientCompanyAu1.LCC_RN_NKCountryCode = "AU";
			var clientCompanyAu2 = Factory.New<ClientCompany>();
			clientCompanyAu2.LCC_Code = "AU2";
			clientCompanyAu2.LCC_LD = licDatabase.PK;
			clientCompanyAu2.LCC_RN_NKCountryCode = "AU";
			var clientCompanyUs1 = Factory.New<ClientCompany>();
			clientCompanyUs1.LCC_Code = "US1";
			clientCompanyUs1.LCC_LD = licDatabase.PK;
			clientCompanyUs1.LCC_RN_NKCountryCode = "US";
			var clientCompanyUs2 = Factory.New<ClientCompany>();
			clientCompanyUs2.LCC_Code = "US2";
			clientCompanyUs2.LCC_LD = licDatabase.PK;
			clientCompanyUs2.LCC_RN_NKCountryCode = "US";
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllCompanies = false;
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			var databaseNode = (EdiCommissionAgreementCustomizationTreeNode)model.RootNodes.Single(x => x.BizObj is EdiCommissionAgreementDatabaseWrapper && ((EdiCommissionAgreementDatabaseWrapper)x.BizObj).LicenceDatabase == licDatabase);
			var auNode = (EdiCommissionAgreementCustomizationTreeNode)databaseNode.ChildNodes.Single(x => ((EdiCommissionAgreementCountryWrapper)x.BizObj).CountryCode == "AU");
			var usNode = (EdiCommissionAgreementCustomizationTreeNode)databaseNode.ChildNodes.Single(x => ((EdiCommissionAgreementCountryWrapper)x.BizObj).CountryCode == "US");
			AssertEquals(CheckState.Unchecked, databaseNode.ShouldAutoAdd);
			AssertEquals(CheckState.Unchecked, auNode.ShouldAutoAdd);
			AssertEquals(CheckState.Unchecked, usNode.ShouldAutoAdd);
			usNode.ShouldAutoAdd = CheckState.Checked;
			AssertEquals(CheckState.Indeterminate, databaseNode.ShouldAutoAdd);
		}

		public void TestIncludeDatabaseUsage()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var licDatabase = licEnterprise.Databases.AddNew();
			var licCompany = licEnterprise.Companies.AddNew();
			licCompany.LC_LE = licEnterprise.PK;
			var org = Factory.New<EDIOrgHeader>();
			licCompany.LC_OH = org.PK;
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			customization.EZN_IsAllDatabases = false;
			var model = new EdiCommissionAgreementCustomizationTreeModel(customization);
			var databaseNode = (EdiCommissionAgreementCustomizationTreeNode)model.RootNodes.Single(x => x.BizObj is EdiCommissionAgreementDatabaseWrapper && ((EdiCommissionAgreementDatabaseWrapper)x.BizObj).LicenceDatabase == licDatabase);
			var newDatabaseNode = (EdiCommissionAgreementCustomizationTreeNode)model.RootNodes.Single(x => x.BizObj is EdiCommissionAgreementDatabaseWrapper && ((EdiCommissionAgreementDatabaseWrapper)x.BizObj).LicenceDatabase == null);
			AssertEquals(false, databaseNode.IncludeDatabaseUsage);
			AssertEquals(false, newDatabaseNode.IncludeDatabaseUsage);
			customization.EZN_IsAllDatabases = true;
			AssertEquals(true, databaseNode.IncludeDatabaseUsage);
			AssertEquals(true, newDatabaseNode.IncludeDatabaseUsage);
		}
	}
}

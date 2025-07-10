using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.Business.Organisation.Helpers;
using Enterprise.DataTransfer.SystemMerge.XmlDefinition;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeCompanyDataValueObjectHelperTest : TestCaseWithFactory
	{
		#region Import

		public void TestImportAccAPAccountDetails()
		{
			SysMergeOrgCompanyDataCollection xsdOrgCompanyDataCollection = new SysMergeOrgCompanyDataCollection();
			SysMergeOrgCompanyData xsdOrgCompanyData = xsdOrgCompanyDataCollection.AddNew();
			xsdOrgCompanyData.GC_Code = "EDI";
			SysMergeAccAPAccountDetails xsdAccAPAccountDetails = xsdOrgCompanyData.AccAPAccountDetails.AddNew();

			xsdAccAPAccountDetails.AccountName = "TestAccount";
			xsdAccAPAccountDetails.BankAccount = "10018743";
			xsdAccAPAccountDetails.BankAddress1 = "BankAddress1";
			xsdAccAPAccountDetails.BankAddress2 = "BankAddress2";
			xsdAccAPAccountDetails.BankAddress3 = "BankAddress3";
			xsdAccAPAccountDetails.BankBranchName = "TestBranch";
			xsdAccAPAccountDetails.BankBsb = "062200";
			xsdAccAPAccountDetails.BankName = "TestBank";
			xsdAccAPAccountDetails.BankSwift = "('')";
			xsdAccAPAccountDetails.IsDefaultAccount = true;
			xsdAccAPAccountDetails.PaymentMethod = "DEF";
			xsdAccAPAccountDetails.RX_AccountCurrency_NK = "AUD";

			OrgHeaderForDataTransfer orgHeader = Factory.New<OrgHeaderForDataTransfer>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());
			SysMergeCompanyDataValueObjectHelper testHelper = new SysMergeCompanyDataValueObjectHelper("");
			testHelper.ImportFromValueObjectCollection(xsdOrgCompanyDataCollection, orgHeader, context, new SysMergeCompanyMapper());

			OrgCompanyData[] orgCompanyData = orgHeader.Factory.Load<OrgCompanyData>(new ZQuery(OrgCompanyDataSchema.OB_OH, orgHeader.PK));
			AccAPAccountDetails[] accountDetails = orgHeader.Factory.Load<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_OB, orgCompanyData[0].PK));
			AssertEquals("AccountName", xsdAccAPAccountDetails.AccountName, accountDetails[0].A1_AccountName);
			AssertEquals("BankAccount", xsdAccAPAccountDetails.BankAccount, accountDetails[0].A1_BankAccount);
			AssertEquals("BankAddress1", xsdAccAPAccountDetails.BankAddress1, accountDetails[0].A1_BankAddress1);
			AssertEquals("BankAddress2", xsdAccAPAccountDetails.BankAddress2, accountDetails[0].A1_BankAddress2);
			AssertEquals("BankAddress3", xsdAccAPAccountDetails.BankAddress3, accountDetails[0].A1_BankAddress3);
			AssertEquals("BankBranchName", xsdAccAPAccountDetails.BankBranchName, accountDetails[0].A1_BankBranchName);
			AssertEquals("BankBsb", xsdAccAPAccountDetails.BankBsb, accountDetails[0].A1_BankBsb);
			AssertEquals("BankName", xsdAccAPAccountDetails.BankName, accountDetails[0].A1_BankName);
			AssertEquals("BankSwift", xsdAccAPAccountDetails.BankSwift, accountDetails[0].A1_BankSwift);
			AssertEquals("IsDefaultAccount", xsdAccAPAccountDetails.IsDefaultAccount, accountDetails[0].A1_IsDefaultAccount);
			AssertEquals("PaymentMethod", xsdAccAPAccountDetails.PaymentMethod, accountDetails[0].A1_PaymentMethod);
			AssertEquals("RX_AccountCurrency_NK", xsdAccAPAccountDetails.RX_AccountCurrency_NK, accountDetails[0].A1_RX_NKAccountCurrency);
		}

		public void TestImportARTermsAndInvoiceCycle()
		{
			SysMergeOrgARTermCollection xsdARTermCollection = new SysMergeOrgARTermCollection();
			SysMergeOrgARTermInvoiceCycleCollection xsdARTermsCycleCollection = new SysMergeOrgARTermInvoiceCycleCollection();

			SysMergeOrgARTerm arTerm1 = xsdARTermCollection.AddNew();
			arTerm1.ARInvoiceClass = "ALL";
			arTerm1.ARInvoiceTerm = "COD";
			arTerm1.ARInvoiceTermDays = 10;

			SysMergeOrgARTerm arTerm2 = xsdARTermCollection.AddNew();
			arTerm2.ARInvoiceClass = "DSB";
			arTerm2.ARInvoiceTerm = "MIC";
			arTerm2.ARInvoiceTermDays = 20;

			SysMergeOrgARTermInvoiceCycle arTermCycle = arTerm2.ARTermsCycles.AddNew();
			arTermCycle.ToDay = 10;
			arTermCycle.PaymentDay = 12;

			OrgCompanyData companyData = Factory.New<OrgCompanyData>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());
			SysMergeARTermsValueObjectHelper arTermHelper = new SysMergeARTermsValueObjectHelper("");
			arTermHelper.ImportFromValueObjectCollection(xsdARTermCollection, companyData, context);

			ZQuery query = new ZQuery(OrgARTermsSchema.PY_OB, companyData.PK);
			OrgARTerms[] arTerms = companyData.Factory.Load<OrgARTerms>(query);

			AssertEquals("2 imported AR Terms should exist", 2, arTerms.Length);

			AssertEquals("ALL", arTerms[0].PY_InvoiceClass);
			AssertEquals("COD", arTerms[0].PY_InvoiceTerm);
			AssertEquals(10, arTerms[0].PY_InvoiceDays.ToZInt());

			AssertEquals("DSB", arTerms[1].PY_InvoiceClass);
			AssertEquals("MIC", arTerms[1].PY_InvoiceTerm);
			AssertEquals(20, arTerms[1].PY_InvoiceDays.ToZInt());
			AssertEquals(10, arTerms[1].ARTermsCycles[0].P5_ToDay.ToZInt());
			AssertEquals(12, arTerms[1].ARTermsCycles[0].P5_PaymentDay.ToZInt());
		}

		#endregion

		#region Export

		public void TestExportOrgCompanyDataVATConfig()
		{
			OrgHeaderForDataTransfer orgHeader = Factory.New<OrgHeaderForDataTransfer>();
			OrgCompanyData orgCompanyData = orgHeader.Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_IsDebtor = true;
			orgCompanyData.OB_IsCreditor = true;
			orgCompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			orgCompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			orgCompanyData.OB_OH = orgHeader.PK;

			var xsdOrgCompanyDataCollection = new SysMergeOrgCompanyDataCollection();
			var testHelper = new SysMergeCompanyDataValueObjectHelper("");
			testHelper.ExportToValueObjectCollection(orgHeader, xsdOrgCompanyDataCollection);
			var xsdOrgCompanyData = xsdOrgCompanyDataCollection[0];

			AssertEquals("IsDebtor", orgCompanyData.OB_IsDebtor, xsdOrgCompanyData.IsDebtor);
			AssertEquals("IsDebtorSpecified", true, xsdOrgCompanyData.IsDebtorSpecified);
			AssertEquals("IsCreditor", orgCompanyData.OB_IsCreditor, xsdOrgCompanyData.IsCreditor);
			AssertEquals("IsCreditorSpecified", true, xsdOrgCompanyData.IsCreditorSpecified);

			AssertEquals("APTaxApplicable", orgCompanyData.IsAPTaxApplicable, xsdOrgCompanyData.APTaxApplicable);
			AssertEquals("APTaxApplicableSpecified", true, xsdOrgCompanyData.APTaxApplicableSpecified);
			AssertEquals("ARTaxApplicable", orgCompanyData.IsARTaxApplicable, xsdOrgCompanyData.ARTaxApplicable);
			AssertEquals("ARTaxApplicableSpecified", true, xsdOrgCompanyData.ARTaxApplicableSpecified);
		}

		public void TestExportAccAPAccountDetails()
		{
			OrgHeaderForDataTransfer orgHeader = Factory.New<OrgHeaderForDataTransfer>();
			OrgCompanyData orgCompanyData = orgHeader.Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_OH = orgHeader.PK;
			AccAPAccountDetails accountDetails = orgCompanyData.Factory.NewWithValidTestData<AccAPAccountDetails>();
			accountDetails.A1_OB = orgCompanyData.PK;

			SysMergeOrgCompanyDataCollection xsdOrgCompanyDataCollection = new SysMergeOrgCompanyDataCollection();
			SysMergeCompanyDataValueObjectHelper testHelper = new SysMergeCompanyDataValueObjectHelper("");
			testHelper.ExportToValueObjectCollection(orgHeader, xsdOrgCompanyDataCollection);
			SysMergeAccAPAccountDetails xsdAccAPAccountDetail = xsdOrgCompanyDataCollection[0].AccAPAccountDetails[0];

			AssertEquals("AccountName", xsdAccAPAccountDetail.AccountName, accountDetails.A1_AccountName);
			AssertEquals("BankAccount", xsdAccAPAccountDetail.BankAccount, accountDetails.A1_BankAccount);
			AssertEquals("BankAddress1", xsdAccAPAccountDetail.BankAddress1, accountDetails.A1_BankAddress1);
			AssertEquals("BankAddress2", xsdAccAPAccountDetail.BankAddress2, accountDetails.A1_BankAddress2);
			AssertEquals("BankAddress3", xsdAccAPAccountDetail.BankAddress3, accountDetails.A1_BankAddress3);
			AssertEquals("BankBranchName", xsdAccAPAccountDetail.BankBranchName, accountDetails.A1_BankBranchName);
			AssertEquals("BankBsb", xsdAccAPAccountDetail.BankBsb, accountDetails.A1_BankBsb);
			AssertEquals("BankName", xsdAccAPAccountDetail.BankName, accountDetails.A1_BankName);
			AssertEquals("BankSwift", xsdAccAPAccountDetail.BankSwift, accountDetails.A1_BankSwift);
			AssertEquals("IsDefaultAccount", xsdAccAPAccountDetail.IsDefaultAccount, accountDetails.A1_IsDefaultAccount);
			AssertEquals("PaymentMethod", xsdAccAPAccountDetail.PaymentMethod, accountDetails.A1_PaymentMethod);
			AssertEquals("RX_AccountCurrency_NK", xsdAccAPAccountDetail.RX_AccountCurrency_NK, accountDetails.A1_RX_NKAccountCurrency);
		}

		public void TestExportARTermsAndInvoiceCycle()
		{
			OrgHeaderForDataTransfer orgHeader = Factory.New<OrgHeaderForDataTransfer>();
			OrgCompanyData companyData = orgHeader.Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_OH = orgHeader.PK;
			OrgARTerms arTerm = Factory.New<OrgARTerms>();
			arTerm.PY_OB = companyData.PK;
			arTerm.PY_InvoiceClass = "ALL";
			arTerm.PY_InvoiceTerm = "COD";
			arTerm.PY_InvoiceDays = 10;

			OrgARTerms arTerm2 = Factory.New<OrgARTerms>();
			arTerm2.PY_OB = companyData.PK;
			arTerm2.PY_InvoiceClass = "DSB";
			arTerm2.PY_InvoiceTerm = "MIC";
			arTerm2.PY_InvoiceDays = 20;

			OrgARTermsCycle arTermCycle = Factory.New<OrgARTermsCycle>();
			arTermCycle.P5_PY = arTerm2.PK;
			arTermCycle.P5_ToDay = 10;
			arTermCycle.P5_PaymentDay = 12;

			NotificationBuffer notify = new NotificationBuffer();
			SysMergeOrgCompanyDataCollection xsdOrgCompanyDataCollection = new SysMergeOrgCompanyDataCollection();
			SysMergeCompanyDataValueObjectHelper testHelper = new SysMergeCompanyDataValueObjectHelper("");
			testHelper.ExportToValueObjectCollection(orgHeader, xsdOrgCompanyDataCollection);

			AssertEquals("There should be 2 ARTerms exported", 2, xsdOrgCompanyDataCollection[0].OrgARTerms.Count);

			SysMergeOrgARTerm xsdArTerm1 = xsdOrgCompanyDataCollection[0].OrgARTerms[0];
			AssertEquals(arTerm.PY_InvoiceClass, xsdArTerm1.ARInvoiceClass);
			AssertEquals(arTerm.PY_InvoiceTerm, xsdArTerm1.ARInvoiceTerm);
			AssertEquals(arTerm.PY_InvoiceDays, xsdArTerm1.ARInvoiceTermDays);

			SysMergeOrgARTerm xsdArTerm2 = xsdOrgCompanyDataCollection[0].OrgARTerms[1];
			AssertEquals(arTerm2.PY_InvoiceClass, xsdArTerm2.ARInvoiceClass);
			AssertEquals(arTerm2.PY_InvoiceTerm, xsdArTerm2.ARInvoiceTerm);
			AssertEquals(arTerm2.PY_InvoiceDays, xsdArTerm2.ARInvoiceTermDays);
			AssertEquals(1, xsdArTerm2.ARTermsCycles.Count);
			AssertEquals(arTermCycle.P5_PaymentDay, xsdArTerm2.ARTermsCycles[0].PaymentDay);
			AssertEquals(arTermCycle.P5_ToDay, xsdArTerm2.ARTermsCycles[0].ToDay);
		}

		#endregion
	}
}

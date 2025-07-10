using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	sealed class MasterFiles1_10Test : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 09, 26)]
		public void TestBuildXml()
		{
			var debtorAddress = Creator.Debtor1.MainAddress;
			debtorAddress.Address2 = "Debtor Address 2";
			debtorAddress.OA_PostCode = "AU";

			var creditorAddress = Creator.Creditor1.MainAddress;
			creditorAddress.Address2 = "Creditor Address 2";
			creditorAddress.OA_PostCode = "AU";

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry("NO"))
			{
				var dateInLastMonth = ZDateTime.Today.AddMonths(-1);
				Creator.CreateTestPeriods(ZDateTime.Today.AddYears(-1));
				Creator.CreateTestPeriods(ZDateTime.Today);

				var glAccount = Creator.GetGLAccountFromDB();
				var aRinvoice1 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV1", Creator.AUD, 1M, 10M, 0M, 10M, 0M, Creator.Debtor1, glAccount.PK);
				aRinvoice1.AH_PostDate = dateInLastMonth;
				var aRinvoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV2", Creator.AUD, 1M, 20M, 0M, 20M, 0M, Creator.Debtor1, glAccount.PK);
				aRinvoice2.AH_PostDate = dateInLastMonth;
				var aRinvoice3 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV3", Creator.AUD, 1M, 30M, 0M, 30M, 0M, Creator.Debtor1, glAccount.PK);
				aRinvoice3.AH_PostDate = dateInLastMonth;

				var receipt1 = Creator.CreateAndMatchARReceiptForARInvoice(aRinvoice1, partPaidAmount: 5m);
				receipt1.AH_PostDate = dateInLastMonth;
				var receipt2 = Creator.CreateAndMatchARReceiptForARInvoice(aRinvoice2);
				receipt2.AH_PostDate = dateInLastMonth;

				aRinvoice1 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV4", Creator.AUD, 1M, 10M, 0M, 10M, 0M, Creator.Debtor1, glAccount.PK);
				aRinvoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV5", Creator.AUD, 1M, 20M, 0M, 20M, 0M, Creator.Debtor1, glAccount.PK);
				aRinvoice3 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV6", Creator.AUD, 1M, 30M, 0M, 30M, 0M, Creator.Debtor1, glAccount.PK);

				Creator.CreateAndMatchARReceiptForARInvoice(aRinvoice1, partPaidAmount: 5m);
				Creator.CreateAndMatchARReceiptForARInvoice(aRinvoice2);

				var aPinvoice1 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV1", Creator.AUD, 1M, 10M, 0M, 10M, 0M, Creator.Creditor1, glAccount.PK);
				aPinvoice1.AH_PostDate = dateInLastMonth;
				var aPinvoice2 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV2", Creator.AUD, 1M, 20M, 0M, 20M, 0M, Creator.Creditor1, glAccount.PK);
				aPinvoice2.AH_PostDate = dateInLastMonth;
				var aPinvoice3 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV3", Creator.AUD, 1M, 30M, 0M, 30M, 0M, Creator.Creditor1, glAccount.PK);
				aPinvoice3.AH_PostDate = dateInLastMonth;

				var payment1 = Creator.CreateAndMatchAPPaymentForAPInvoice(aPinvoice1, partPaidAmount: -5m);
				payment1.AH_PostDate = dateInLastMonth;
				var payment2 = Creator.CreateAndMatchAPPaymentForAPInvoice(aPinvoice2);
				payment2.AH_PostDate = dateInLastMonth;

				aPinvoice1 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV4", Creator.AUD, 1M, 10M, 0M, 10M, 0M, Creator.Creditor1, glAccount.PK);
				aPinvoice2 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV5", Creator.AUD, 1M, 20M, 0M, 20M, 0M, Creator.Creditor1, glAccount.PK);
				aPinvoice3 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "APINV6", Creator.AUD, 1M, 30M, 0M, 30M, 0M, Creator.Creditor1, glAccount.PK);

				Creator.CreateAndMatchAPPaymentForAPInvoice(aPinvoice1, partPaidAmount: -5m);
				Creator.CreateAndMatchAPPaymentForAPInvoice(aPinvoice2);
				Factory.Save();

				var contactForDebtor1 = Creator.Debtor1.Contacts.AddNew();
				contactForDebtor1.OC_ContactName = "Debtor1_1";
				contactForDebtor1.OC_Phone = "1";
				contactForDebtor1.OC_Email = "E1";

				contactForDebtor1 = Creator.Debtor1.Contacts.AddNew();
				contactForDebtor1.OC_ContactName = "Debtor1_2";
				contactForDebtor1.OC_Phone = "2";
				contactForDebtor1.OC_Email = "E2";
				var documentForDebtorContact = contactForDebtor1.Documents.AddNew();
				documentForDebtorContact.OD_DocumentGroup = ContactType.Receivables.Code;
				documentForDebtorContact.OD_DefaultContact = true;

				var contactForCreditor1 = Creator.Creditor1.Contacts.AddNew();
				contactForCreditor1.OC_ContactName = "Creditor1_1";
				contactForCreditor1.OC_Phone = "3";
				contactForCreditor1.OC_Email = "E3";

				contactForCreditor1 = Creator.Creditor1.Contacts.AddNew();
				contactForCreditor1.OC_ContactName = "Creditor1_2";
				contactForCreditor1.OC_Phone = "4";
				contactForCreditor1.OC_Email = "E4";
				var documentForCreditorContact = contactForCreditor1.Documents.AddNew();
				documentForCreditorContact.OD_DocumentGroup = ContactType.Payables.Code;
				documentForCreditorContact.OD_DefaultContact = true;

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_DateFrom = ZDateTime.Today.Date;
				report.ACR_DateTo = ZDateTime.Today.Date;
				report.Company.GC_OH_OrgProxy = report.Company.GetNewOrgProxy(Factory).PK;
				Factory.Save();
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var transactionNum = "ARINV001";
				var invoice1 = Creator.CreateInvoice(typeof(ARInvoice), transactionNum, Creator.USD, 1m, Creator.Debtor1);
				invoice1.AH_TransactionReference = "ref" + transactionNum;
				invoice1.AH_ComplianceSubType = "TXI";
				invoice1.SourceReference = $"FTM a/{transactionNum}";

				transactionNum = "APINV001";
				var invoice2 = Creator.CreateInvoice(typeof(APInvoice), transactionNum, Creator.USD, 1m, Creator.Creditor1);
				invoice1.AH_TransactionReference = "ref" + transactionNum;
				invoice1.AH_ComplianceSubType = "TXI";
				invoice1.SourceReference = $"FTM a/{transactionNum}";

				var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxRate = taxRateCollection.AddNew();
				validTaxRate.AT_Code = "TGST";
				validTaxRate.AT_Type = "RAT";
				validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate.SetRateNumerator_ForTestOnly(10);
				validTaxRate.AT_ExtraTaxRateType = "QST";
				validTaxRate.SetExtraRate_ForTestOnly(4, 2);
				var validTaxRate2 = taxRateCollection.AddNew();
				validTaxRate2.AT_Code = "TST2";
				validTaxRate2.AT_Type = "RAT";
				validTaxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate2.SetRateNumerator_ForTestOnly(20);
				validTaxRate2.AT_ExtraTaxRateType = "QST";
				validTaxRate2.SetExtraRate_ForTestOnly(8, 2);
				var validTaxRate3 = taxRateCollection.AddNew();
				validTaxRate3.AT_Code = "TST3";
				validTaxRate3.AT_Type = "RAT";
				validTaxRate3.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate3.SetRateNumerator_ForTestOnly(30);
				validTaxRate3.AT_ExtraTaxRateType = "QST";
				validTaxRate3.SetExtraRate_ForTestOnly(8, 3);
				var validTaxRate4 = taxRateCollection.AddNew();
				validTaxRate4.AT_Code = "TST4";
				validTaxRate4.AT_Type = "RAT";
				validTaxRate4.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate4.SetRateNumerator_ForTestOnly(20);
				validTaxRate4.AT_ExtraTaxRateType = "QST";
				validTaxRate4.SetExtraRate_ForTestOnly(8, 2);

				var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxMessage = taxMessageCollection.AddNew();
				validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage.A9_TaxGroupCode = "N1";
				var validTaxMessage2 = taxMessageCollection.AddNew();
				validTaxMessage2.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage2.A9_TaxGroupCode = "N2";
				var validTaxMessage3 = taxMessageCollection.AddNew();
				validTaxMessage3.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage3.A9_TaxGroupCode = "N3";

				Factory.Save();

				var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection();
				taxMessageGroupsManagement.Add("N1", (NoResString)"Description N1", true, "N1.0");
				taxMessageGroupsManagement.Add("N2", (NoResString)"Description N2", true, "N2.0");
				taxMessageGroupsManagement.Add("N3", (NoResString)"Description N2", true, "");
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

				Factory.Save();

				var config = Creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration(
					(TransactionLineTypes.Revenue, validTaxRate, validTaxMessage),
					(TransactionLineTypes.Revenue, validTaxRate2, validTaxMessage2),
					(TransactionLineTypes.Revenue, validTaxRate3, validTaxMessage3),
					(TransactionLineTypes.Revenue, validTaxRate4, validTaxMessage2));
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, config);

				Factory.Save();

				Creator.Debtor1.CustomsCodes.AddNew("GBR", "000000001", "NO");
				Creator.Debtor1.CustomsCodes.AddNew("MVA", "000000002", "NO");
				Creator.Creditor1.CustomsCodes.AddNew("GBR", "000000003", "NO");
				Creator.Creditor1.CustomsCodes.AddNew("MVA", "000000004", "NO");

				Creator.CreateComplianceReportTransactionPivot(report, invoice1, 1);
				Creator.CreateComplianceReportTransactionPivot(report, invoice2, 2);

				var additionalData = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10);
				additionalData.SetOrgBalance(new[] { report });

				var writer = new MasterFiles1_10();
				var actualXml = writer.BuildXml(report, null, additionalData);

				var expectedXml = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\SAFT1_10\SAFT-NO Master.xml");
				expectedXml = string.Join(string.Empty, expectedXml.Split('\r', '\n').ToList().Select(line => line.Trim()));

				AssertNotNull(actualXml.ToString());
				this.AssertXMLEqualsByDiff(expectedXml, actualXml.ToString(), XmlDiffEquals.XmlCompareOptions.IgnoreXmlDecl);

				Creator.Creditor1.CustomsCodes.RemoveAndDeleteAll();
				additionalData = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10);
				additionalData.SetOrgBalance(new[] { report });
				writer = new MasterFiles1_10();
				actualXml = writer.BuildXml(report, null, additionalData);
				Assert("No TaxRegistrationNumber, it should be NA", actualXml.ToString().Contains("<TaxRegistrationNumber>NA</TaxRegistrationNumber>"));
			}
		}

		[TestDate(2022, 10, 01)]
		public void TestGeneralLedgerAccounts()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var glHeaderPLAppropriation = Creator.CreateAccGLHeader("A300.00.33", "TS", "PLAppropriationAccount", "BSH", Core.Constants.DebitCredit.Debit);
				var glHeaderPAndL = Creator.CreateAccGLHeader("A400.00.33", "TS", "P&L", "P&L", Core.Constants.DebitCredit.Debit);
				var glHeaderNTE = Creator.CreateAccGLHeader("A500.00.33", "TS", "P&L", Core.Constants.AccountType.Note, Core.Constants.DebitCredit.Debit);
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeaderPLAppropriation.PK.ToGuid());

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
				report.FillWithValidTestData();
				Factory.Save();

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				var lastYearPeriod = new AccountingPeriodTestHelper().SetupSinglePeriod(202212, new ZDateTime(2021, 12, 1), new ZDateTime(2021, 12, 30));
				report.ACR_Periodicity = "PER";
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
				Factory.Save();

				var debitGLAccount = Creator.GLHeader1;
				debitGLAccount.AG_Description = "debit GL account";
				debitGLAccount.AG_AccountNum = "A100.00.00";
				debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;
				var creditGLAccount = Creator.GLHeader2;
				creditGLAccount.AG_Description = "credit GL Account";
				creditGLAccount.AG_AccountNum = "A200.00.00";
				creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

				var debitDescriptor = Creator.CreateAccountDescriptor(Creator.GLHeader1, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.English, "", "NO", Core.Constants.DebitCredit.Debit);
				debitDescriptor.AJ_AG = debitGLAccount.PK;

				var debitDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader1, "71000.95", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Debit);
				debitDescriptor_Norway.AJ_AG = debitGLAccount.PK;

				var creditDescriptor = Creator.CreateAccountDescriptor(Creator.GLHeader2, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.English, "", "NO", Core.Constants.DebitCredit.Credit);
				creditDescriptor.AJ_AG = creditGLAccount.PK;

				var creditDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader2, "71000.96", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Credit);
				creditDescriptor_Norway.AJ_AG = creditGLAccount.PK;

				Creator.CreateGLDescriptorPivot(debitDescriptor, Creator.GLHeader1, "P&L", "D11");
				Creator.CreateGLDescriptorPivot(debitDescriptor_Norway, Creator.GLHeader1, "P&L", "D11");
				Creator.CreateGLDescriptorPivot(creditDescriptor, Creator.GLHeader2, "P&L", "D11");
				Creator.CreateGLDescriptorPivot(creditDescriptor_Norway, Creator.GLHeader2, "P&L", "D11");

				Factory.Save();

				var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

				var openingBalance1 = Factory.New<AccGLAggregate>();
				openingBalance1.AA_AG = debitGLAccount.PK;
				openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance1.AA_Period = previousPeriod;
				openingBalance1.AA_Amount = 123m;

				var openingBalance2 = Factory.New<AccGLAggregate>();
				openingBalance2.AA_AG = creditGLAccount.PK;

				openingBalance2.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance2.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance2.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance2.AA_Period = previousPeriod;
				openingBalance2.AA_Amount = -123m;

				var openingBalance3 = Factory.New<AccGLAggregate>();
				openingBalance3.AA_AG = glHeaderPLAppropriation.PK;

				openingBalance3.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance3.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance3.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance3.AA_Period = previousPeriod;
				openingBalance3.AA_Amount = 10m;

				var openingBalance4 = Factory.New<AccGLAggregate>();
				openingBalance4.AA_AG = glHeaderPAndL.PK;

				openingBalance4.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance4.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance4.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance4.AA_Period = lastYearPeriod.AM_Period;
				openingBalance4.AA_Amount = 500;

				var openingBalance5 = Factory.New<AccGLAggregate>();
				openingBalance5.AA_AG = glHeaderNTE.PK;
				openingBalance5.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance5.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance5.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance5.AA_Period = lastYearPeriod.AM_Period;
				openingBalance5.AA_Amount = 600;

				Factory.Save();

				report.ClearReportLines_ForTestOnly();

				var masterFiles = new MasterFiles1_10();

				var collector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10);
				var result = masterFiles.BuildXml(report, null, collector);

				AssertMultilineASCIIEquals(@"<MasterFiles>
  <GeneralLedgerAccounts>
    <Account>
      <AccountID>A100.00.00</AccountID>
      <AccountDescription>debit GL account</AccountDescription>
      <StandardAccountID>71</StandardAccountID>
      <AccountType>GL</AccountType>
      <AccountCreationDate>2022-10-01</AccountCreationDate>
      <OpeningDebitBalance>123.00</OpeningDebitBalance>
      <ClosingDebitBalance>123.00</ClosingDebitBalance>
    </Account>
    <Account>
      <AccountID>A200.00.00</AccountID>
      <AccountDescription>credit GL Account</AccountDescription>
      <StandardAccountID>71</StandardAccountID>
      <AccountType>GL</AccountType>
      <AccountCreationDate>2022-10-01</AccountCreationDate>
      <OpeningCreditBalance>123.00</OpeningCreditBalance>
      <ClosingCreditBalance>123.00</ClosingCreditBalance>
    </Account>
    <Account>
      <AccountID>A300.00.33</AccountID>
      <AccountDescription>PLAppropriationAccount</AccountDescription>
      <StandardAccountID></StandardAccountID>
      <AccountType>GL</AccountType>
      <AccountCreationDate>2022-10-01</AccountCreationDate>
      <OpeningDebitBalance>510.00</OpeningDebitBalance>
      <ClosingDebitBalance>510.00</ClosingDebitBalance>
    </Account>
  </GeneralLedgerAccounts>
  <TaxTable>
    <TaxTableEntry>
      <TaxType>MVA</TaxType>
      <Description>Merverdiavgift</Description>
    </TaxTableEntry>
  </TaxTable>
</MasterFiles>", result.ToString());
			}
		}

		[TestDate(2022, 10, 10)]
		public void TestStandardAccountIDWithLocalGLAccountNumberOnlyHasTwoDigits()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
				report.FillWithValidTestData();
				Factory.Save();

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_Periodicity = "PER";
				report.AccountingPeriod = currentPeriod.AM_Period;
				Creator.CreateConfigurationForComplianceReport(report, "**", "DBW");
				Factory.Save();

				var creditGLAccount = Creator.GLHeader2;
				creditGLAccount.AG_Description = "credit GL Account";
				creditGLAccount.AG_AccountNum = "A200.00.00";
				creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

				var creditDescriptor_Norway = Creator.CreateAccountDescriptor(Creator.GLHeader2, "71", "COA", "P&L", Core.SharedConstants.Languages.Norwegian, "", "NO", Core.Constants.DebitCredit.Credit);
				creditDescriptor_Norway.AJ_AG = creditGLAccount.PK;

				Creator.CreateGLDescriptorPivot(creditDescriptor_Norway, Creator.GLHeader2, "P&L", "D11");

				Factory.Save();

				var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

				var openingBalance = Factory.New<AccGLAggregate>();
				openingBalance.AA_AG = creditGLAccount.PK;

				openingBalance.AA_GE = GlbDepartment.CurrentDepartment.PK;
				openingBalance.AA_GB = GlbBranch.CurrentBranch.PK;
				openingBalance.AA_GC = GlbCompany.CurrentCompany.PK;
				openingBalance.AA_Period = previousPeriod;
				openingBalance.AA_Amount = -123m;

				Factory.Save();

				report.ClearReportLines_ForTestOnly();

				var masterFiles = new MasterFiles1_10();

				var collector = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT1_10);
				var result = masterFiles.BuildXml(report, null, collector);

				Assert(result.ToString().Contains("<StandardAccountID>71</StandardAccountID>"));
			}
		}

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;
	}
}

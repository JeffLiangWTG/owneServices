using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.SAFT;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	public abstract class SAFTXMLWriterBaseTest : TestCaseWithFactory
	{
		#region ISAFTXMLWriter

		public abstract void TestWriteSingleReportXmlToStream();

		public abstract void TestWriteAnnualReportXmlToStream();

		protected abstract ISAFTXMLWriter GetWriter(AccComplianceReport report, AccComplianceReport[] reports, ReportModeAndCreditorSelector reportModeAndCreditorSelector);

		#endregion

		protected string VersionNumber = string.Join(".", ReleaseInfo.Instance.VersionNumber.ToString().Split('.').Take(3)); // deliberately creating the version number a different way to the production code

		protected AccComplianceReport CreateWriteSAFTReportData(string reportType, string reportTablePrefix, string reportLineGrouping)
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.Company.GC_Phone = "012345678";
			report.Company.GC_Email = "email@company.com";

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			report.ACR_ReportType = reportType;
			report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(report, reportTablePrefix, reportLineGrouping);
			Factory.Save();

			var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_TaxGroupCode = "GRP";

			var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
			var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

			var openingPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today.AddMonths(-2));
			var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();

			var shipment = Creator.CreateShipment("S001001", true);
			var job = Creator.CreateJob(shipment);
			var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "*JC*ACR*ACRCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "**JC*ACR**");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "*JC*WIP*WIPCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "**JC*WIP**");

			var originalInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
			originalInvoice.AH_TransactionReference = "originalRef1";
			originalInvoice.SourceReference = "originalRef1_SourceRef";
			var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
			Creator.ABIGAS.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
			invoice.AH_TransactionReference = "ref1";
			invoice.AuthorizationNumberReference = "TEST-ATCUD-01";
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AC = Creator.CC10.PK;
			line.AL_AT = taxRate.PK;
			line.AL_A9_VATClass = taxMessage.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
			Creator.GLHeader1.AG_AccountNum = "8888.10.00";
			Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
			Creator.GLHeader1.AG_Description = "Test GL Account";
			Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT123123123");
			(invoice as IAmending).AmendingReason = "AmendingReason1";

			var receipt = Creator.CreateARReceipt(1m, 150m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
			Factory.Save();

			Creator.AALSHI.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
			Creator.AALSHI.OH_IsGlobalAccount = true;
			Creator.AALSHI.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Mexico;
			Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Mexico, OrgCusCode.CodeTypes.IVA, "MX231231231");

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line, 3);
				Creator.CreateComplianceReportTransactionPivot(report, line2, 4);
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice, 3, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*GSTOut*-");

				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*Bank*-");
				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*ARCtrl*");

				AssertEquals("GLOpeningBalanceDR is set for test", 123m, report.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR is set for test", 123m, report.GLOpeningBalanceCR);
			}

			var invoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice2.AH_OH = Creator.AALSHI.PK;
			invoice2.AH_InvoiceDate = invoice2.AH_PostDate.AddDays(2);
			invoice2.AH_ComplianceSubType = "TXI";
			invoice2.AH_TransactionReference = "ref2";
			(invoice2 as IAmending).AmendingReason = "AmendingReason2";
			invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			invoice2.AuthorizationNumberReference = "TEST-ATCUD-02";

			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AC = Creator.CC10.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_AG = Creator.GLHeader1.PK;
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.CC1.PK, 100m, Creator.USD);

			var invoice3 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0003", Creator.AUD, 1m, 200m, 0m, 200m, 0m);
			invoice3.AH_OH = Creator.AALSHI.PK;
			invoice3.AH_ComplianceSubType = "TXM";
			invoice3.AH_TransactionReference = "ref3";
			invoice3.AH_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
			invoice3.AH_InvoiceTermDays = 10;
			invoice3.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
			invoice3.SourceReference = "FTM a/189";
			invoice3.AuthorizationNumberReference = "TEST-ATCUD-03";

			Assert("Has Lines", invoice3.Lines.Count > 0);
			var line3_1 = invoice3.Lines[0];
			line3_1.AL_AC = ZGuid.Empty;
			line3_1.AL_AT = ZGuid.Empty;
			line3_1.AL_AG = Creator.GLHeader1.PK;

			var adjNote = Creator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "A0001", Creator.AUD, 1m, 100m, 10m, 100m, 10m);
			adjNote.AH_OH = Creator.ABIGAS.PK;
			adjNote.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			adjNote.AuthorizationNumberReference = "TEST-ATCUD-04";
			Assert("No Compliance Sub Type on AR ADJ", adjNote.AH_ComplianceSubType.IsEmpty);
			Assert("Has Lines", adjNote.Lines.Count > 0);
			var adjLine = adjNote.Lines[0];

			Creator.ABIGAS.MainAddress.Postcode = "";

			Factory.Save();

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 5);
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 6);
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 7);
				Creator.CreateComplianceReportTransactionPivot(report, adjLine, 8);
			}
			else
			{
				// The following SubCodes are related to Lines AL_ReverseDate which could be earlier than AL_PostDate/AH_PostDate
				// That's why they could be placed with lesser Sequence than the TransactionHeader Report Line 
				// *AR*INV**Rev-
				// *AR*INV*ARSusp*Rev
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 7, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 7, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 8, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 8, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 9, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 9, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, invoice2, 10, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 11, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 11, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 12, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 12, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, invoice3, 13, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 14, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 14, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, adjNote, 15, "*AR*ADJ*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, adjLine, 16, "*AR*ADJ**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, adjLine, 16, "*AR*ADJ*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, adjLine, 16, "*AR*ADJ*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, adjLine, 16, "*AR*ADJ*GSTOut*-");
			}

			return report;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Assert contains BaseSourcePath")]
		protected void TestWriteSAFTAnnualReportCore(Dictionary<string, string> replaceTags, string expectedFilePath,
			string reportType = "SAF", string reportTablePrefix = "**", string reportLineGrouping = "DBW",
			IEnumerable<ZGuid> orgHeadersWithoutTaxNumber = null, bool shouldAddTaxNumber = true)
		{
			Creator.ABIGAS.OH_RL_NKClosestPort = "PTLIS";
			Creator.ABIGAS.OH_Code = "ABIGAS";
			AssertEquals("Precond: org is from Portugal", CountryCodes.Portugal, Creator.ABIGAS.CountryCode);

			Creator.CC10.AC_LocalLanguageDescription = "Local " + Creator.CC10.AC_Desc;
			AssertNotEquals("preCond to test ChargeCode ProductDescription", Creator.CC10.AC_Desc, Creator.CC10.AC_LocalLanguageDescription);
			Factory.Save();

			var startOfFinancialYear = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			Creator.CreateTestPeriods(startOfFinancialYear); // Need to set up Financial year to pass balance from report to report inside Financial Year
			var periodCalculator = new AccountingPeriodCalculator(Factory);

			var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
			var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

			Creator.GLHeader1.AG_AccountNum = "8888.10.00";
			Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
			Creator.GLHeader1.AG_Description = "Test GL Account1";

			Creator.GLHeader2.AG_AccountNum = "8888.20.00";
			Creator.GLHeader2.AG_DebitCredit = Constants.DebitCredit.Debit;
			Creator.GLHeader2.AG_Description = "Test GL Account2";

			var openingPeriod = periodCalculator.GetPeriodFromDate(startOfFinancialYear);
			var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			if (shouldAddTaxNumber)
			{
				Creator.ABIGAS.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				if (orgHeadersWithoutTaxNumber == null || !orgHeadersWithoutTaxNumber.Contains(Creator.ABIGAS.PK))
				{
					Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT123123123");
				}

				Creator.AALSHI.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				Creator.AALSHI.OH_IsGlobalAccount = true;
				Creator.AALSHI.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;
				if (orgHeadersWithoutTaxNumber == null || !orgHeadersWithoutTaxNumber.Contains(Creator.AALSHI.PK))
				{
					Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Spain, OrgCusCode.SpainCodeTypes.NIF, "ES231231231");
				}

				Creator.ZECTRA.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				Creator.ZECTRA.OH_IsGlobalAccount = true;
				Creator.ZECTRA.MainAddress.OA_RN_NKCountryCode = CountryCodes.Italy;
				if (orgHeadersWithoutTaxNumber == null || !orgHeadersWithoutTaxNumber.Contains(Creator.ZECTRA.PK))
				{
					creator.CreateCustomsCodes(Creator.ZECTRA, ZString.Empty, OrgCusCode.CodeTypes.IVA, "IT321321321");
				}
			}
			Factory.Save();

			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_ReportType = reportType;
			report1.ACR_DateFrom = new ZDate(ZDateTime.Now.Year, 2, 1);
			report1.ACR_DateTo = new ZDate(ZDateTime.Now.Year, 2, 28);

			report1.Company.GC_Phone = "012345678";
			report1.Company.GC_Email = "email@company.com";
			report1.Company.Postcode = "";

			var reportPeriod = periodCalculator.GetPeriodManagementFromDate(report1.ACR_DateFrom, report1.ACR_GC_Company);
			report1.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report1.AccountingPeriod = reportPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report1.ACR_DateFrom);
			AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report1.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(report1, reportTablePrefix, reportLineGrouping);
			Factory.Save();

			CreateTestData(report1, Creator.ABIGAS, Creator.CC10, Creator.GLHeader1, "Report1-ATCUD");

			if (report1.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				AssertEquals("GLOpeningBalanceDR is set for test", 123m, report1.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR is set for test", 123m, report1.GLOpeningBalanceCR);
			}

			// Simulate GL TakeUp for the report1
			Creator.CreateAccGLAggregate(250m, reportPeriod.AM_Period, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(-210m, reportPeriod.AM_Period, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(510m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6210.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(-60m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8310.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(-600m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8888.10.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(150m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "ZUSDHeader").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = report1.ACR_ReportType;
			report2.ACR_DateFrom = new ZDate(ZDateTime.Now.Year, 3, 1);
			report2.ACR_DateTo = new ZDate(ZDateTime.Now.Year, 3, 31);
			reportPeriod = periodCalculator.GetPeriodManagementFromDate(report2.ACR_DateFrom, report2.ACR_GC_Company);
			report2.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report2.AccountingPeriod = reportPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report2.ACR_DateFrom);
			AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report2.ACR_DateTo);
			Factory.Save();

			CreateTestData(report2, Creator.AALSHI, Creator.CC6, Creator.GLHeader2, "Report2-ATCUD");

			if (report2.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				AssertEquals("GLOpeningBalanceDR", 1033m, report2.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR", 1033m, report2.GLOpeningBalanceCR);
			}

			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report3.ACR_ReportType = report1.ACR_ReportType;
			report3.ACR_DateFrom = new ZDate(ZDateTime.Now.Year, 4, 1);
			report3.ACR_DateTo = new ZDate(ZDateTime.Now.Year, 4, 30);

			reportPeriod = periodCalculator.GetPeriodManagementFromDate(report3.ACR_DateFrom, report3.ACR_GC_Company);
			report3.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report3.AccountingPeriod = reportPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report3.ACR_DateFrom);
			AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report3.ACR_DateTo);
			Factory.Save();

			CreateTestData(report3, Creator.ZECTRA, creator.CC6, creator.GLHeader2, "Report3-ATCUD");

			if (report3.ACR_ReportType != AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				AssertEquals("GLOpeningBalanceDR", 1033m, report3.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR", 1033m, report3.GLOpeningBalanceCR);
			}

			replaceTags.Add("<SoftwareVersion>20.8.20.0</SoftwareVersion>", $"<SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion>");
			replaceTags.Add("<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>");
			var selector = new ReportModeAndCreditorSelector(Factory);
			AssertSAFTXml(GetWriter(null, new AccComplianceReport[] { report1, report2, report3 }, selector), BaseSourcePath + expectedFilePath, replaceTags, true);
		}

		[TestDate(2019, 6, 2)]
		public void TestBuildDocumentStatusXml()
		{
			const string userCodeXXX = "XXX";
			const string userCodeYYY = "YYY";
			const string userNameXXX = "User Named " + userCodeXXX;
			const string userNameYYY = "User Named " + userCodeYYY;
			const string invoiceStatusN = "<InvoiceStatus>N</InvoiceStatus><InvoiceStatusDate>2019-06-01T00:00:00</InvoiceStatusDate><SourceID>" + userCodeXXX + "</SourceID>";
			const string invoiceStatusS = "<InvoiceStatus>S</InvoiceStatus><InvoiceStatusDate>2019-06-01T00:00:00</InvoiceStatusDate><SourceID>" + userCodeXXX + "</SourceID>";
			const string sourceBillingM = "<SourceBilling>M</SourceBilling>";
			const string sourceBillingP = "<SourceBilling>P</SourceBilling>";
			const string irrelevantCategory = "XXX";

			var options = new[]
			{
				// reversal (parent transaction is reversed by this transaction)
				new { IsCancelled = true,   HasParent=true,     IsParentCancelled = true,   IsReversedAmendment = false,  SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = true,   HasParent=true,     IsParentCancelled = true,   IsReversedAmendment = false,  SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// reversed amendment (parent transaction is amended but not reversed)
				new { IsCancelled = true,   HasParent=true,     IsParentCancelled = false,  IsReversedAmendment = true,   SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = true,   HasParent=true,     IsParentCancelled = false,  IsReversedAmendment = true,   SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// reversed original (no parent transaction, some child transaction is the reversal)
				new { IsCancelled = true,   HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = true,   HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// amendment when DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed is false and the parent transaction is reversed (by some other child transaction)
				new { IsCancelled = false,  HasParent=true,     IsParentCancelled = true,   IsReversedAmendment = false,  SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = false,  HasParent=true,     IsParentCancelled = true,   IsReversedAmendment = false,  SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// amendment (parent transaction is amended by this transaction)
				new { IsCancelled = false,  HasParent=true,     IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = false,  HasParent=true,     IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// normal transaction (may have amendments, but that is not known in this transaction)
				new { IsCancelled = false,  HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "M", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingM + "</DocumentStatus>" },
				new { IsCancelled = false,  HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "P", Category = irrelevantCategory, ExpectedXML = "<DocumentStatus>" + invoiceStatusN + sourceBillingP + "</DocumentStatus>" },
				// normal self billing transaction (M source is not aplicable)
				new { IsCancelled = false,  HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "P", Category = TransactionCategory.Codes.SelfBilling, ExpectedXML = "<DocumentStatus>" + invoiceStatusS + sourceBillingP + "</DocumentStatus>" },
				// reversed self billing transaction (no amendments for AP ledger, M source is not aplicable)
				new { IsCancelled = true,  HasParent=false,    IsParentCancelled = false,  IsReversedAmendment = false,  SourceBilling = "P", Category = TransactionCategory.Codes.SelfBilling, ExpectedXML = "<DocumentStatus>" + invoiceStatusS + sourceBillingP + "</DocumentStatus>" },
				// reversal self billing transaction (no amendments for AP ledger, M source is not aplicable)
				new { IsCancelled = true,  HasParent=true,    IsParentCancelled = true,  IsReversedAmendment = false,  SourceBilling = "P", Category = TransactionCategory.Codes.SelfBilling, ExpectedXML = "<DocumentStatus>" + invoiceStatusS + sourceBillingP + "</DocumentStatus>" },
			};

			var actual = options.Select(option =>
			{
				var invoice = Creator.CreateInvoice(typeof(ARInvoice), Creator.USD);

				var transactionHeaderDetails = new TransactionHeaderDetails
				{
					IsCancelled = option.IsCancelled,
					IsOriginalTransactionCancelled = option.HasParent ? new ZBool?(option.IsParentCancelled) : null,
					OriginalTransactionReference = option.HasParent ? "something" : string.Empty,
					IsReversedAmendment = option.IsReversedAmendment,
					TransactionCategory = option.Category,
					CreateTime = ZDateTime.Now.AddDays(-1),
					CreateUserCode = userCodeXXX,
					CreateUserName = userNameXXX,
					LastEditUserCode = userCodeYYY,
					LastEditUserName = userNameYYY,
					LastEditTime = ZDateTime.Now,
				};

				return SalesInvoices.BuildDocumentStatusXml(transactionHeaderDetails, option.SourceBilling)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			}).ToArray();

			AssertSequencesEqual(options.Select(option => option.ExpectedXML), actual);
		}

		public void TestBuildReferencesXml()
		{
			const string inv = "INVOICE";
			const string invX = "REVERSED INVOICE";
			const string amd = "AMENDMENT";
			const string amdX = "REVERSED AMENDMENT";
			const string canX = "REVERSAL";
			const string ide = "IDE";
			const string iam = "IAM";
			const string txt = "TXT";
			const string wor = "WOR";
			const string nul = null;
			const string tranRef = "I0001";

			var options = new[]
			{
				new { Style = inv, ReceiptType = nul, ExpectedXML = nul, },
				new { Style = invX, ReceiptType = nul, ExpectedXML = nul, },
				new { Style = amd, ReceiptType = ide, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Data Entry</Reason></References>", },
				new { Style = amd, ReceiptType = iam, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Amounts</Reason></References>", },
				new { Style = amd, ReceiptType = txt, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Free Text</Reason></References>", },
				new { Style = amdX, ReceiptType = ide, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Data Entry</Reason></References>", },
				new { Style = amdX, ReceiptType = iam, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Amounts</Reason></References>", },
				new { Style = amdX, ReceiptType = txt, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Free Text</Reason></References>", },
				new { Style = canX, ReceiptType = ide, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Data Entry</Reason></References>", },
				new { Style = canX, ReceiptType = iam, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Incorrect Amounts</Reason></References>", },
				new { Style = canX, ReceiptType = txt, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Free Text</Reason></References>", },
				new { Style = canX, ReceiptType = wor, ExpectedXML = "<References><Reference>" + tranRef + "</Reference><Reason>Wrong Organization Code Used</Reason></References>", },
			};

			var actual = options.Select(option =>
			{
				var transactionHeaderDetails = new TransactionHeaderDetails
				{
					IsOriginalTransactionCancelled = (option.Style != inv && option.Style != invX) ? new bool?(option.Style == canX || option.Style == amdX) : null,
					ReceiptType = (option.Style == amd || option.Style == amdX || option.Style == canX) ? option.ReceiptType : null,
					OriginalTransactionReference = (option.Style != inv && option.Style != invX) ? tranRef : null,
					IsReversedAmendment = option.Style == amdX,
					IsCancelled = option.Style == invX || option.Style == amdX || option.Style == canX,
				};

				return SalesInvoices.BuildReferencesXml(transactionHeaderDetails)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			}).ToArray();

			AssertSequencesEqual(options.Select(option => option.ExpectedXML), actual);
		}

		public void TestBuildReferencesXml_OriginalTransaction()
		{
			var transactionHeaderDetailsWithOriginalTransactionReference = new TransactionHeaderDetails
			{
				OriginalTransactionReference = "transactionReferenceNumber",
				OriginalReferenceReversalReason = "TXT|custom description"
			};

			var actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithOriginalTransactionReference)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			var expectedResult = "<References><Reference>transactionReferenceNumber</Reference><Reason>custom description</Reason></References>";
			AssertEquals(expectedResult, actualResult);

			var transactionHeaderDetailsWithDateFromAndDateTo = new TransactionHeaderDetails
			{
				OriginalReferenceStartDate = new ZDateTime(2021, 5, 02),
				OriginalReferenceEndDate = new ZDateTime(2021, 5, 29)
			};

			AssertEquals(string.Empty, transactionHeaderDetailsWithDateFromAndDateTo.OriginalTransactionReference);
			AssertEquals(string.Empty, transactionHeaderDetailsWithDateFromAndDateTo.OriginalReferenceReversalReason);

			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithDateFromAndDateTo)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			expectedResult = "<References><Reason>2021-05-02 - 2021-05-29</Reason></References>";
			AssertEquals(expectedResult, actualResult);

			var transactionHeaderDetailsWithoutData = new TransactionHeaderDetails
			{
				OriginalReferenceStartDate = ZDateTime.Empty,
				OriginalReferenceEndDate = new ZDateTime(2021, 5, 29)
			};

			AssertEquals(string.Empty, transactionHeaderDetailsWithoutData.OriginalTransactionReference);
			AssertEquals(string.Empty, transactionHeaderDetailsWithoutData.OriginalReferenceReversalReason);

			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithoutData)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			AssertNull(actualResult);

			var transactionHeaderDetailsWithOriginalReferenceSourceReference = new TransactionHeaderDetails
			{
				OriginalTransactionReference = "transactionReferenceNumber",
				OriginalReferenceSourceReference = "FTM ABC/123456",
				OriginalReferenceComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI
			};

			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithOriginalReferenceSourceReference)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			expectedResult = "<References><Reference>FTM ABC/123456</Reference></References>";
			AssertEquals("TXI is not Compatible With SourceReference so it should return the complete OriginalReferenceSourceReference", expectedResult, actualResult);

			transactionHeaderDetailsWithOriginalReferenceSourceReference.OriginalReferenceComplianceSubType = string.Empty;
			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithOriginalReferenceSourceReference)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			expectedResult = "<References><Reference>FTM ABC/123456</Reference></References>";
			AssertEquals("string.empty is not Compatible With SourceReference so it should return the complete OriginalReferenceSourceReference", expectedResult, actualResult);

			transactionHeaderDetailsWithOriginalReferenceSourceReference.OriginalReferenceComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithOriginalReferenceSourceReference)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			expectedResult = "<References><Reference>ABC/123456</Reference></References>";
			AssertEquals("TXM is Compatible With SourceReference so it should return the OriginalReferenceSourceReference minus 4 first characters", expectedResult, actualResult);

			var transactionHeaderDetailsWithIncorrectOriginalReferenceSourceReference = new TransactionHeaderDetails
			{
				OriginalTransactionReference = "transactionReferenceNumber",
				OriginalReferenceSourceReference = "FT",
				OriginalReferenceComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM
			};

			actualResult = SalesInvoices.BuildReferencesXml(transactionHeaderDetailsWithIncorrectOriginalReferenceSourceReference)?.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces);
			expectedResult = "<References><Reference>FT</Reference></References>";
			AssertEquals("if the OriginalReferenceSourceReference is incorrect we don't remove characters", expectedResult, actualResult);
		}

		protected ZSaveFileDialog GetXmlFileSaveDialog(string fileName)
		{
			var result = new ZSaveFileDialog();
			result.DefaultExt = "xml"; // File extension
			result.FileName = fileName;
			result.AddExtension = true;
			result.Filter = "XML Files | *.xml"; // File extension filter
			result.InitialDirectory = "\\"; // File extension
			return result;
		}

		#region Implementation

		protected void CreateTestData(AccComplianceReport report, OrgHeader testOrg, AccChargeCode testCode, AccGLHeader testHeader, ZString authorizationNumberReferenceprefix)
		{
			var numberSuffix = report.AccountingPeriod.ToString().Substring(4, 2);
			var shipment = Creator.CreateShipment("S00100" + numberSuffix, true);
			var job = Creator.CreateJob(shipment);
			var charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 110m, creditor: Creator.Creditor1, osSellAmt: 160m, debtor: Creator.Debtor1);
			Factory.Save();

			var postDate = new ZDateTime(report.ACR_DateFrom.Year, report.ACR_DateFrom.Month, ZDateTime.Today.Day);
			charge.WIP.AL_PostDate = charge.Accrual.AL_PostDate = postDate;
			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "*JC*ACR*ACRCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.Accrual, 1, "**JC*ACR**");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "*JC*WIP*WIPCtrl*-");
			Creator.CreateComplianceReportTransactionPivot(report, charge.WIP, 2, "**JC*WIP**");

			var originalInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0000" + numberSuffix, Creator.AUD, 1m, 100m, 0m, 100m, 0m);
			originalInvoice.AH_TransactionReference = "originalRef1" + numberSuffix;
			originalInvoice.AH_InvoiceDate = originalInvoice.AH_PostDate = postDate;

			var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate = originalInvoice.AH_PostDate.AddDays(1);
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
			invoice.AH_TransactionReference = "ref1" + numberSuffix;
			invoice.AuthorizationNumberReference = authorizationNumberReferenceprefix + "-01";
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AC = testCode.PK;
			line.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
			(invoice as IAmending).AmendingReason = "AmendingReason1";

			var receipt = Creator.CreateARReceipt(1m, 150m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
			Factory.Save();

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line, 3);
				Creator.CreateComplianceReportTransactionPivot(report, line2, 4);
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice, 3, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*GSTOut*-");

				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*Bank*-");
				Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*ARCtrl*");
			}

			var invoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice2.AH_OH = testOrg.PK;
			invoice2.AH_InvoiceDate = originalInvoice.AH_PostDate.AddDays(2);
			invoice2.AH_ComplianceSubType = "TXI";
			invoice2.AH_TransactionReference = "ref2" + numberSuffix;
			(invoice2 as IAmending).AmendingReason = "AmendingReason2";
			invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			invoice2.AuthorizationNumberReference = authorizationNumberReferenceprefix + "-02";

			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AC = testCode.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_AG = testHeader.PK;
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.CC1.PK, 100m, Creator.USD);
			Factory.Save();

			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 5);
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 6);
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, invoice2, 7, "*AR*INV*ARCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV*GSTOut*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV*ARSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV*GSTOut*-");
			}
		}

		protected void AssertSAFTXml(ISAFTXMLWriter writer, string filePathOfExpectedXML, IReadOnlyDictionary<string, string> replaceTags, bool isAnnualReport = false)
		{
			var actualXml = GetGeneratedSaftXml(writer, isAnnualReport);
			var expectedXml = File.ReadAllText(filePathOfExpectedXML);
			foreach (var replaceTag in replaceTags)
			{
				expectedXml = expectedXml.Replace(replaceTag.Key, replaceTag.Value);
			}
			expectedXml = string.Join(string.Empty, expectedXml.Split('\r', '\n').ToList().Select(line => line.Trim()));
			this.AssertXMLEqualsByDiff(expectedXml, actualXml, XmlDiffEquals.XmlCompareOptions.IgnoreXmlDecl);
		}

		protected string GetGeneratedSaftXml(ISAFTXMLWriter writer, bool isAnnualReport = false)
		{
			string result = null;

			try
			{
				using (var dialog = GetXmlFileSaveDialog("Test.xml"))
				{
					(IEnumerable<string> FileNames, ZStringBuilder Messages, bool HasValidationError) writerResult = (null, null, false);
					if (isAnnualReport)
					{
						writerResult = writer.WriteAnnualReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);
					}
					else
					{
						writerResult = writer.WriteSingleReportXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);
					}

					Assert("No Errors", !writerResult.HasValidationError);

					using (var resultFileDialog = new ZOpenFileDialog())
					{
						resultFileDialog.FileName = "Test.xml";

						using (var resultStream = resultFileDialog.OpenFile())
						using (var reader = new StreamReader(resultStream))
						{
							result = reader.ReadToEnd();
						}
					}
					AssertNotNull(result);
				}
			}
			finally
			{
				var file = new FileInfo(GetFullPath("Test.xml"));
				file?.Delete();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Legacy Path to Test Files")]
		protected string GetFullPath(string fileName, string folderName = "SAFT1_10")
		{
			return @$"{BaseSourcePath}Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\ComplianceReport\SAFT\TestFiles\{folderName}\{fileName}";
		}

		#endregion
		protected TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;
	}
}

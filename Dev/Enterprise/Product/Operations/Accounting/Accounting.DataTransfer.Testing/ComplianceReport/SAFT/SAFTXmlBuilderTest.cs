using System;
using System.Collections.Generic;
using System.Linq;
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
using static Enterprise.Accounting.Business.ComplianceReport.AccComplianceReport;
using static Enterprise.Core.Constants;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	sealed class SAFTXmlBuilderTest : TestCaseWithFactory
	{
		readonly string VersionNumber = string.Join(".", ReleaseInfo.Instance.VersionNumber.ToString().Split('.').Take(3)); // deliberately creating the version number a different way to the production code

		public void TestCheckCompleteItems_WriteSAFTReport_NoConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(0);
		}

		public void TestCheckCompleteItems_WriteSAFTReport_SingleConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(1);
		}

		public void TestCheckCompleteItems_WriteSAFTReport_OnlyConsumidorFinal()
		{
			AssertWriteSAFTReportForConsumidorFinal(2);
		}

		void AssertWriteSAFTReportForConsumidorFinal(int invoiceCountWithoutPTIVA)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
				{
					Creator.ABIGAS.OH_RL_NKClosestPort = "PTLIS";
					Creator.ABIGAS.OH_Code = "ABIGAS";
					AssertEquals("Precond: org is from Portugal", CountryCodes.Portugal, Creator.ABIGAS.CountryCode);

					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					report.ACR_ReportType = reportType;
					report.ACR_DateFrom = ZDate.Today.AddDays(-7);
					report.ACR_DateTo = ZDate.Today;
					Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

					if (invoiceCountWithoutPTIVA == 0)
					{
						Creator.CreateCustomsCodesIfNotExists(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT111");
					}
					Factory.Save();

					var invoice1 = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.USD, 1m, Creator.ABIGAS);
					invoice1.AH_TransactionReference = "ref1";
					invoice1.AH_ComplianceSubType = "TXI";
					Factory.Save();

					var invoice2 = Creator.CreateInvoice(typeof(ARInvoice), "INV002", Creator.USD, 1m, Creator.ABIGAS);
					invoice2.AH_TransactionReference = "ref2";
					invoice2.AH_ComplianceSubType = "TXI";

					if (invoiceCountWithoutPTIVA == 1)
					{
						Creator.CreateCustomsCodesIfNotExists(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT111");
					}
					Factory.Save();

					Creator.CreateComplianceReportTransactionPivot(report, invoice1, 1);
					Creator.CreateComplianceReportTransactionPivot(report, invoice2, 1);

					var selector = new ReportModeAndCreditorSelector(Factory);
					var writer = new SAFTXmlBuilder(selector, report);
					CheckCompleteItems(writer);
				}
			}
		}

		public void TestCheckCompleteItems_WriteSAFTReport_SourceBilling()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV001", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV002", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
		}

		public void TestCheckCompleteItems_WriteSAFTReport_HashControl()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV001", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV002", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "INV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "CRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
		}

		public void TestCheckCompleteItems_WriteSAFTReport_InvoiceType()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD004", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV004", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
		}

		public void TestCheckCompleteItems_WriteSAFTReport_ThirdPartiesBillingIndicator()
		{
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV001", PortugalComplianceInfo.ComplianceSubTypeCodes.TCD);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.LCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD002", PortugalComplianceInfo.ComplianceSubTypeCodes.TCR);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APCreditNote), "APCRD001", PortugalComplianceInfo.ComplianceSubTypeCodes.SBC);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD004", PortugalComplianceInfo.ComplianceSubTypeCodes.TCM);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(APInvoice), "APINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.SBI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV002", PortugalComplianceInfo.ComplianceSubTypeCodes.LTX);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV003", PortugalComplianceInfo.ComplianceSubTypeCodes.TXI);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV004", PortugalComplianceInfo.ComplianceSubTypeCodes.TXM);

			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARInvoice), "ARINV005", PortugalComplianceInfo.ComplianceSubTypeCodes.XCL);
			AssertSAFTReportXmlSingleSectionWithComplianceSubType(typeof(ARCreditNote), "ARCRD003", PortugalComplianceInfo.ComplianceSubTypeCodes.XCR);
		}

		void AssertSAFTReportXmlSingleSectionWithComplianceSubType(Type transactionType, string transactionNum, string complianceSubType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var invoice1 = Creator.CreateInvoice(transactionType, transactionNum, Creator.USD, 1m, Creator.ABIGAS);
				invoice1.AH_TransactionReference = "ref" + transactionNum;
				invoice1.AH_ComplianceSubType = complianceSubType;
				invoice1.SourceReference = $"FTM a/{transactionNum}";

				if (invoice1.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					invoice1.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
				}
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, invoice1, 1);

				var selector = new ReportModeAndCreditorSelector(Factory);
				if (invoice1.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					selector.GenerateSalesInvoices = false;
					selector.CreditorPK = Creator.ABIGAS.PK;
				}
				var writer = new SAFTXmlBuilder(selector, report);
				CheckCompleteItems(writer);
			}
		}

		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_WriteSAFTReport()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData();
				AssertWriteSAFTReport(report);
			}
		}

		[TestDate(2023, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_WriteSAFTReportWithATCUD()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData();
				var replaceTags = new Dictionary<string, string>
				{
					{ @"<InvoiceNo>ref1</InvoiceNo>
				<ATCUD>0</ATCUD>",
						@"<InvoiceNo>ref1</InvoiceNo>
				<ATCUD>TEST-ATCUD-01</ATCUD>" },
					{ @"<InvoiceNo>ref3</InvoiceNo>
				<ATCUD>0</ATCUD>",
						@"<InvoiceNo>ref3</InvoiceNo>
				<ATCUD>TEST-ATCUD-03</ATCUD>" },
					{ "2018", "2023" },
					{ "2019", "2024" },
				};
				AssertWriteSAFTReport(report, replaceTags);
			}
		}

		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_SAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionDisabled()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CC10.AC_LocalLanguageDescription = "Local Charge Code 10";
				AssertNotEquals("preCond to test ChargeCode ProductDescription", Creator.CC10.AC_Desc, Creator.CC10.AC_LocalLanguageDescription);

				var report = CreateWriteSAFTReportData();

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertWriteSAFTReport(report);
				}
			}
		}

		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_SAFTReportLocalLanguageDescription_WhenLocalChargeCodeDescriptionEnabled()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CC10.AC_LocalLanguageDescription = "Local Charge Code 10";
				AssertNotEquals("preCond to test ChargeCode ProductDescription", Creator.CC10.AC_Desc, Creator.CC10.AC_LocalLanguageDescription);

				var report = CreateWriteSAFTReportData();

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductDescription>Charge Code 10</ProductDescription>", "<ProductDescription>Local Charge Code 10</ProductDescription>" }
				};

				using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertWriteSAFTReport(report, replaceTags);
				}
			}
		}

		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_SAFTReportEndDate()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var report = CreateWriteSAFTReportData();
				AssertEquals(new ZDate(2018, 9, 10), report.ACR_DateFrom);
				AssertEquals(new ZDate(2018, 10, 9), report.ACR_DateTo);

				AssertResult(new DateTime(2018, 10, 1, 12, 0, 0), "2018-10-01");
				AssertResult(new DateTime(2018, 10, 9, 12, 0, 0), "2018-10-09");
				AssertResult(new DateTime(2018, 10, 10, 12, 0, 0), "2018-10-09");

				void AssertResult(DateTime todayDate, string expectedEndDate)
				{
					TestDateAttribute.Date = todayDate;

					var replaceTags = new Dictionary<string, string>
					{
						{ "<DateCreated>2018-09-10</DateCreated>", $"<DateCreated>{todayDate.ToString("yyyy-MM-dd")}</DateCreated>" },
						{ "<EndDate>2018-09-10</EndDate>", $"<EndDate>{expectedEndDate}</EndDate>" }
					};

					AssertWriteSAFTReport(report, replaceTags);
				}
			}
		}

		void AssertWriteSAFTReport(AccComplianceReport report, Dictionary<string, string> replaceTags = null)
		{
			if (replaceTags == null)
			{
				replaceTags = new Dictionary<string, string>();
			}

			replaceTags.Add("<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>");

			var selector = new ReportModeAndCreditorSelector(Factory);
			CheckCompleteItems(new SAFTXmlBuilder(selector, report));

			report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);

			CheckCompleteItems(new SAFTXmlBuilder(selector, report));
		}

		AccComplianceReport CreateWriteSAFTReportData()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = ReportTypes.SAFT;
			report.ACR_DateFrom = ZDate.Today.AddDays(-7);
			report.ACR_DateTo = ZDate.Today;
			report.Company.GC_Phone = "012345678";
			report.Company.GC_Email = "email@company.com";

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
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

			return report;
		}

		[TestDate(2018, 9, 10, 23, 42, 59)]
		public void TestCheckCompleteItems_SAFTReportSignature()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = Creator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "ref", 1, 100, 1);
				Factory.Save();

				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";

				report.ACR_ReportType = ReportTypes.SAFT;
				report.ACR_DateFrom = ZDate.Today.AddDays(-7);
				report.ACR_DateTo = ZDate.Today.AddDays(-1);
				Assert("preCondition to test DateEnd: report creation date must be later then report period end date", ZDate.Today > report.ACR_DateTo);

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
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
				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				Creator.ABIGAS.CompanyData.OB_ARCustomerSelfBillsRevenue = true;
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(-1);
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
				invoice.AH_TransactionReference = "ref1";
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

				var selector = new ReportModeAndCreditorSelector(Factory);
				report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);
				var saftXmlWriter = new SAFTXmlBuilder(selector, report);
				CheckCompleteItems(saftXmlWriter);
			}
		}

		[TestDate(2023, 3, 8)]
		public void TestCheckCompleteItems_WriteSAFTAnnualReportWithATCUD()
		{
			var replaceTags = new Dictionary<string, string>
			{
				{ @"<InvoiceNo>ref102</InvoiceNo>
				<ATCUD>0</ATCUD>",
					@"<InvoiceNo>ref102</InvoiceNo>
				<ATCUD>Report1-ATCUD-01</ATCUD>" },
				{ @"<InvoiceNo>ref103</InvoiceNo>
				<ATCUD>0</ATCUD>",
					@"<InvoiceNo>ref103</InvoiceNo>
				<ATCUD>Report2-ATCUD-01</ATCUD>" },
				{ "2019", "2023" },
			};
			AssertWriteSAFTAnnualReport_Portugal(replaceTags);
		}

		[TestDate(2019, 3, 8)]
		public void TestCheckCompleteItems_WriteSAFTAnnualReport()
		{
			AssertWriteSAFTAnnualReport_Portugal(new Dictionary<string, string>());
		}

		[TestDate(2019, 3, 8)]
		public void TestCheckCompleteItems_WriteSAFTAnnualReport_ConsumidorFinal()
		{
			var orgHeadersWithoutTaxNumber = new List<ZGuid> { Creator.ABIGAS.PK, Creator.AALSHI.PK };
			var replaceTags = new Dictionary<string, string>();
			replaceTags.Add("<CustomerTaxID>231231231</CustomerTaxID>", "<CustomerTaxID>999999990</CustomerTaxID>");
			replaceTags.Add("<CustomerTaxID>123123123</CustomerTaxID>", "<CustomerTaxID>999999990</CustomerTaxID>");
			replaceTags.Add(
@"</SystemEntryDate>
				<CustomerID>ABIGAS</CustomerID>",
@"</SystemEntryDate>
				<CustomerID>Consumidor final</CustomerID>");
			replaceTags.Add(
@"		<Customer>
			<CustomerID>ABIGAS</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>ABI GAS &amp; TOOLS</CompanyName>
			<BillingAddress>
				<AddressDetail>171 ABBOTSFORD ROAD</AddressDetail>
				<City />
				<PostalCode>4006</PostalCode>
				<Country>PT</Country>
			</BillingAddress>
			<SelfBillingIndicator>1</SelfBillingIndicator>
		</Customer>",
@"		<Customer>
			<CustomerID>Consumidor final</CustomerID>
			<AccountID>Desconhecido</AccountID>
			<CustomerTaxID>999999990</CustomerTaxID>
			<CompanyName>Desconhecido</CompanyName>
			<BillingAddress>
				<AddressDetail>Desconhecido</AddressDetail>
				<City>Desconhecido</City>
				<PostalCode>Desconhecido</PostalCode>
				<Country>Desconhecido</Country>
			</BillingAddress>
			<SelfBillingIndicator>0</SelfBillingIndicator>
		</Customer>");
			AssertWriteSAFTAnnualReport_Portugal(replaceTags, orgHeadersWithoutTaxNumber);
		}

		void AssertWriteSAFTAnnualReport_Portugal(Dictionary<string, string> replaceTags, IEnumerable<ZGuid> orgHeadersWithoutTaxNumber = null)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "0000"))
			using (AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				AssertWriteSAFTAnnualReportCore(replaceTags, orgHeadersWithoutTaxNumber);
			}
		}

		void AssertWriteSAFTAnnualReportCore(Dictionary<string, string> replaceTags, IEnumerable<ZGuid> orgHeadersWithoutTaxNumber = null, bool shouldAddTaxNumber = true,
			string reportType = "SAF", string tablePrefix = "**", string lineGrouping = "DBW")
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
			Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

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
			Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);
			Factory.Save();

			CreateTestData(report1, Creator.ABIGAS, Creator.CC10, Creator.GLHeader1, "Report1-ATCUD");

			AssertEquals("GLOpeningBalanceDR is set for test", 123m, report1.GLOpeningBalanceDR);
			AssertEquals("GLOpeningBalanceCR is set for test", 123m, report1.GLOpeningBalanceCR);

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

			AssertEquals("GLOpeningBalanceDR", 1033m, report2.GLOpeningBalanceDR);
			AssertEquals("GLOpeningBalanceCR", 1033m, report2.GLOpeningBalanceCR);

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

			CreateTestData(report3, Creator.ZECTRA, creator.CC6, Creator.GLHeader2, "Report3-ATCUD");

			AssertEquals("GLOpeningBalanceDR", 1033m, report3.GLOpeningBalanceDR);
			AssertEquals("GLOpeningBalanceCR", 1033m, report3.GLOpeningBalanceCR);

			replaceTags.Add("<SoftwareVersion>20.8.20.0</SoftwareVersion>", $"<SoftwareVersion>{ReleaseInfo.Instance.VersionNumber}</SoftwareVersion>");
			replaceTags.Add("<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>");
			var selector = new ReportModeAndCreditorSelector(Factory);
			CheckCompleteItems(new SAFTXmlBuilder(selector, new AccComplianceReport[] { report1, report2, report3 }));
		}

		[TestDate(2019, 6, 14)]
		public void TestCheckCompleteItems_WriteSAFTSelfBilledReport()
		{
			var softwareCertificateNumber = "0000";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, softwareCertificateNumber))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = "SAF";

				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today, report.ACR_GC_Company);
				report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVA6"));

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

				Creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "012345678");
				var address = report.Company.OrgProxy.Addresses.GetAddressWithMainAddressFallback(report.Company.OrgProxy.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				Creator.ABIGAS.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "123123123");

				Creator.AALSHI.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "231231231");
				address = Creator.AALSHI.Addresses.GetAddressWithMainAddressFallback(Creator.AALSHI.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				var abigasSelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
				abigasSelfBillingInvoice.AH_OH = Creator.ABIGAS.PK;
				abigasSelfBillingInvoice.AH_ComplianceSubType = "TXI";
				abigasSelfBillingInvoice.AH_TransactionReference = "Ref1";
				abigasSelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
				Assert("Has Lines", abigasSelfBillingInvoice.Lines.Count > 0);
				var line1 = abigasSelfBillingInvoice.Lines[0];
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var aalshiSelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				Creator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
				aalshiSelfBillingInvoice.AH_OH = Creator.AALSHI.PK;
				aalshiSelfBillingInvoice.AH_ComplianceSubType = "TXI";
				aalshiSelfBillingInvoice.AH_InvoiceDate = aalshiSelfBillingInvoice.AH_PostDate.AddDays(1);
				aalshiSelfBillingInvoice.AH_TransactionReference = "Ref1";
				aalshiSelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

				Assert("Has Lines", aalshiSelfBillingInvoice.Lines.Count > 0);
				var line2_1 = aalshiSelfBillingInvoice.Lines[0];
				line2_1.AL_AC = Creator.CC10.PK;
				line2_1.AL_AT = taxRate.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line2_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var line2_2 = Creator.CreateInvoiceLine(aalshiSelfBillingInvoice, Creator.CC1.PK, 100m, Creator.USD);
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, costCurrency: Creator.USD, osCostAmt: 100m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line2_2.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				Creator.GLHeader1.AG_AccountNum = "8888.10.00";
				Creator.GLHeader1.AG_DebitCredit = Constants.DebitCredit.Debit;
				Creator.GLHeader1.AG_Description = "Test GL Account";

				var receipt = Creator.CreateARReceipt(1m, 150m, aalshiSelfBillingInvoice.AH_PostDate, aalshiSelfBillingInvoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, abigasSelfBillingInvoice, 3, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line1, 4, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, aalshiSelfBillingInvoice, 5, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_1, 6, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line2_2, 7, "*AP*INV*APSusp*-");

				Creator.CreateComplianceReportTransactionPivot(report, receipt, 8, "*AR*REC*Bank*-");
				Creator.CreateComplianceReportTransactionPivot(report, receipt, 8, "*AR*REC*ARCtrl*");

				AssertEquals("GLOpeningBalanceDR is set for test", 123m, report.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR is set for test", 123m, report.GLOpeningBalanceCR);

				var creditor2SelfBillingInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0002", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				creditor2SelfBillingInvoice.AH_OH = Creator.Creditor2.PK;
				creditor2SelfBillingInvoice.AH_ComplianceSubType = "TXM";
				creditor2SelfBillingInvoice.AH_InvoiceDate = creditor2SelfBillingInvoice.AH_PostDate.AddDays(2);
				creditor2SelfBillingInvoice.AH_TransactionReference = "ref3";
				creditor2SelfBillingInvoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

				Assert("Has Lines", creditor2SelfBillingInvoice.Lines.Count > 0);
				var line3_1 = creditor2SelfBillingInvoice.Lines[0];
				line3_1.AL_AC = Creator.CC10.PK;
				line3_1.AL_AT = Creator.GST1.PK;
				line3_1.AL_AG = Creator.GLHeader1.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.Creditor2, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line3_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();

				var line3_2 = Creator.CreateInvoiceLine(creditor2SelfBillingInvoice, Creator.CC1.PK, 100m, Creator.USD);
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, costCurrency: Creator.USD, osCostAmt: 100m, creditor: Creator.Creditor2, osSellAmt: 0m, debtor: Creator.Debtor1);
				charge.JR_AL_APLine = line3_2.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				Factory.Save();

				var aalshiStandardInvoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0003", Creator.USD, 1m, 200m, 20m, 200m, 20m);
				aalshiStandardInvoice.AH_OH = Creator.AALSHI.PK;
				aalshiStandardInvoice.AH_ComplianceSubType = "TXI";
				aalshiStandardInvoice.AH_InvoiceDate = aalshiStandardInvoice.AH_PostDate.AddDays(2);
				aalshiStandardInvoice.AH_TransactionNum = "I0003";
				aalshiStandardInvoice.AH_TransactionReference = "ref4";
				aalshiStandardInvoice.AH_TransactionCategory = TransactionCategory.Codes.Standard;

				Assert("Has Lines", aalshiStandardInvoice.Lines.Count > 0);
				var line4_1 = aalshiStandardInvoice.Lines[0];
				line4_1.AL_AC = Creator.CC11.PK;
				line4_1.AL_AT = Creator.GST1.PK;
				line4_1.AL_AG = Creator.GLHeader1.PK;
				charge = Creator.CreateCharge(job, chargeCode: Creator.CC11, costCurrency: Creator.USD, osCostAmt: 200m, creditor: Creator.AALSHI, osSellAmt: 0m, debtor: Creator.Creditor2);
				charge.JR_AL_APLine = line4_1.PK;
				charge.SetChargeValuesFromLinkedAPLineForTests();
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(report, creditor2SelfBillingInvoice, 9, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line3_1, 10, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line3_2, 11, "*AP*INV*APSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, aalshiStandardInvoice, 12, "*AP*INV*APCtrl*Total");
				Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV**Rev-");
				Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AR*INV*ARSusp*-");
				Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV*APSusp*Rev");
				Creator.CreateComplianceReportTransactionPivot(report, line4_1, 13, "*AP*INV*APSusp*-");

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" },
					{ "<SoftwareCertificateNumber>0000</SoftwareCertificateNumber>", $"<SoftwareCertificateNumber>{softwareCertificateNumber}</SoftwareCertificateNumber>" },
				};
				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				CheckCompleteItems(new SAFTXmlBuilder(selector, report));

				report.ReportLines.Sort(AccComplianceReportLine.Schema.ReportSubCode);

				CheckCompleteItems(new SAFTXmlBuilder(selector, report));
			}
		}

		[TestDate(2019, 6, 17)]
		public void TestCheckCompleteItems_WriteSAFTSelfBilledAnnualReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "1234"))
			using (AccountingMasterFilesRegistry.Instance.PTSoftwareName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Portugal Test Software Name"))
			{
				var startOfFinancialYear = new ZDateTime(2019, 1, 1);
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

				Factory.Save();

				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_ReportType = "SAF";
				report1.ACR_DateFrom = new ZDate(2019, 5, 1);
				report1.ACR_DateTo = new ZDate(2019, 5, 31);

				var reportPeriod = periodCalculator.GetPeriodManagementFromDate(report1.ACR_DateFrom, report1.ACR_GC_Company);
				report1.AccountingPeriod = reportPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report1.ACR_DateFrom);
				AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report1.ACR_DateTo);

				Creator.CreateCustomsCodes(report1.Company.OrgProxy, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "012345678");
				var address = report1.Company.OrgProxy.Addresses.GetAddressWithMainAddressFallback(report1.Company.OrgProxy.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";

				Creator.ABIGAS.CompanyData.OB_IsCreditor = true;
				Creator.CreateCustomsCodes(Creator.ABIGAS, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "123123123");

				Creator.AALSHI.CompanyData.OB_IsCreditor = true;
				Creator.AALSHI.CompanyData.OB_APCostsSelfBilled = true;
				Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "231231231");
				address = Creator.AALSHI.Addresses.GetAddressWithMainAddressFallback(Creator.AALSHI.OH_RL_NKClosestPort, OrgAddressType.Office);
				address.City = "Brisbane";
				Factory.Save();

				CreateSelfBilledTestData(report1, Creator.AALSHI, Creator.CC10, Creator.GLHeader1);

				AssertEquals("GLOpeningBalanceDR is set for test", 123m, report1.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR is set for test", 123m, report1.GLOpeningBalanceCR);

				// Simulate GL TakeUp for the report1
				Creator.CreateAccGLAggregate(250m, reportPeriod.AM_Period, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-210m, reportPeriod.AM_Period, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(510m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6210.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-60m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8310.00.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(-600m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "8888.10.00").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				Creator.CreateAccGLAggregate(150m, reportPeriod.AM_Period, Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "ZUSDHeader").PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = report1.ACR_ReportType;
				report2.ACR_DateFrom = new ZDate(2019, 6, 1);
				report2.ACR_DateTo = new ZDate(2019, 6, 30);
				reportPeriod = periodCalculator.GetPeriodManagementFromDate(report2.ACR_DateFrom, report2.ACR_GC_Company);
				report2.AccountingPeriod = reportPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", reportPeriod.AM_StartDate.Date, report2.ACR_DateFrom);
				AssertEquals("ACR_DateTo", reportPeriod.AM_EndDate.Date, report2.ACR_DateTo);
				Factory.Save();

				CreateSelfBilledTestData(report2, Creator.AALSHI, Creator.CC6, Creator.GLHeader2);

				AssertEquals("GLOpeningBalanceDR", 1033m, report2.GLOpeningBalanceDR);
				AssertEquals("GLOpeningBalanceCR", 1033m, report2.GLOpeningBalanceCR);

				var replaceTags = new Dictionary<string, string>
				{
					{ "<ProductVersion>1.0.0.0</ProductVersion>", $"<ProductVersion>{VersionNumber}</ProductVersion>" },
					{ "<SoftwareCertificateNumber>0000</SoftwareCertificateNumber>", $"<SoftwareCertificateNumber>1234</SoftwareCertificateNumber>" },
				};

				var selector = new ReportModeAndCreditorSelector(Factory);
				selector.GenerateCreditorInvoices = true;
				selector.CreditorPK = Creator.AALSHI.PK;
				CheckCompleteItems(new SAFTXmlBuilder(selector, new AccComplianceReport[] { report1, report2 }));
			}
		}

		ZSaveFileDialog GetXmlFileSaveDialog(string fileName)
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

		void CreateTestData(AccComplianceReport report, OrgHeader testOrg, AccChargeCode testCode, AccGLHeader testHeader, ZString authorizationNumberReferenceprefix)
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

			if (report.ACR_ReportType == ReportTypes.SAFTOnlyTransactions)
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

			if (report.ACR_ReportType == ReportTypes.SAFTOnlyTransactions)
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

		void CheckCompleteItems(SAFTXmlBuilder writer)
		{
			using (var dialog = GetXmlFileSaveDialog("Test.xml"))
			{
				writer.WriteXmlToStream(() => dialog.OpenFile(), dialog.UnmappedFileName);

				AssertEquals(writer.TotaItemsToComplete, writer.CompletedItems);
				AssertNotEquals(0, writer.TotaItemsToComplete);
				AssertEquals("Validating the SAFT XML generated file...", writer.CurrentStatusText);
			}
		}

		void CreateSelfBilledTestData(AccComplianceReport report, OrgHeader testOrg, AccChargeCode testCode, AccGLHeader testHeader)
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

			var invoice = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0001" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			invoice.AH_ComplianceSubType = "TXI";
			invoice.AH_TransactionReference = "ref1" + numberSuffix;
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AC = testCode.PK;
			line.AL_AT = Creator.GST1.PK;
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC1, osCostAmt: 100m, creditor: Creator.ABIGAS, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();

			var receipt = Creator.CreateARReceipt(1m, 150m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(5), Creator.ABIGAS.PK, Creator.USDBankAccount.PK);
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, invoice, 3, "*AP*INV*APCtrl*Total");
			Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV*APSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line, 4, "*AP*INV*APSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV*APSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 5, "*AP*INV*APSusp*-");

			Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*Bank*-");
			Creator.CreateComplianceReportTransactionPivot(report, receipt, 6, "*AR*REC*ARCtrl*");

			var invoice2 = Creator.CreateInvoiceWithLine(typeof(APInvoice), "I0002" + numberSuffix, Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice2.AH_OH = testOrg.PK;
			invoice2.AH_InvoiceDate = invoice.AH_PostDate.AddDays(2);
			invoice2.AH_ComplianceSubType = "TXI";
			invoice2.AH_TransactionReference = "ref2" + numberSuffix;
			invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;

			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AC = testCode.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_AG = testHeader.PK;
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 200m, creditor: testOrg, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2_1.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.CC10.PK, 100m, Creator.USD);
			charge = Creator.CreateCharge(job, chargeCode: Creator.CC10, costCurrency: Creator.USD, osCostAmt: 100m, creditor: testOrg, osSellAmt: 0m, debtor: Creator.Debtor1);
			charge.JR_AL_APLine = line2_2.PK;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, invoice2, 7, "*AP*INV*APCtrl*Total");
			Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV*APSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line2_1, 8, "*AP*INV*APSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV*APSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line2_2, 9, "*AP*INV*APSusp*-");
		}

		#endregion
		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;
	}
}

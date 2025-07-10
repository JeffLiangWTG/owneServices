using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ComplianceReport.JPKV7M;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using PeriodicityCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportPeriodicityCodes;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccComplianceReport))]
	public partial class AccComplianceReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var report = Factory.New<AccComplianceReport>();
			AssertEquals(AccComplianceReport.Schema.ACR_GC_Company, GlbCompany.CurrentCompany.PK, report.ACR_GC_Company);
			Assert("default value for ACR_ReferenceNumber column is empty", report.ACR_ReferenceNumber.IsEmpty);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportDefaultValue>>().Setup(x => x.Get().GetReferenceNumber()).Returns("123456789");
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			report = Factory.New<AccComplianceReport>();
			AssertEquals("default value can be customised with IComplianceReportDefaultValue", "123456789", report.ACR_ReferenceNumber);
		}

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccComplianceReport>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestIncludeQueuedForPreviousPeriod()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			Report.ACR_ReportType = "TST";

			Assert("default IncludeQueuedForPreviousPeriod is false", !Report.IncludeQueuedForPreviousPeriod);

			reportConfig.IncludeQueuedForPreviousPeriod = false;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Assert(!Report.IncludeQueuedForPreviousPeriod);

			reportConfig.IncludeQueuedForPreviousPeriod = true;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Assert(Report.IncludeQueuedForPreviousPeriod);
		}

		public void TestAccountingPeriodReadOnlyExcludingComplianceFinancialYear()
		{
			var accountingPeriodCodes = new List<string>
			{
				PeriodicityCodes.AccountingPeriod,
				PeriodicityCodes.FinancialYear
			};

			bool IsIncludedPeriodicity(string periodicity) => periodicity != PeriodicityCodes.ComplianceFinancialYear;

			foreach (var reportType in new[] { AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.ReportTypes.SAFTOnlyTransactions })
			{
				foreach (var reportStatus in typeof(AccComplianceReport.Status).GetFields().Select(x => x.GetValue(null).ToString()).ToList())
				{
					foreach (var periodicity in typeof(PeriodicityCodes).GetFields().Select(x => x.GetValue(null).ToString()).Where(IsIncludedPeriodicity).ToList())
					{
						var objectCreator = new TestObjectCreator(Factory);
						objectCreator.CreateTestPeriods(ZDateTime.Today);

						var report = objectCreator.CreateComplianceReport(reportType, reportStatus);
						var periodCalculator = new AccountingPeriodCalculator(Factory);
						var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
						report.ACR_Periodicity = periodicity;
						report.AccountingPeriod = report.ACR_Periodicity == PeriodicityCodes.FinancialYear ? currentPeriod.AM_Year : currentPeriod.AM_Period;

						bool periodicityCodeUsesAccountingPeriod = accountingPeriodCodes.Contains(periodicity);

						var isAccountingPeriodReadOnly =
								!periodicityCodeUsesAccountingPeriod
								|| (reportStatus != AccComplianceReport.Status.ReportCreated && reportStatus != AccComplianceReport.Status.ReportPendingQueueing && reportStatus != AccComplianceReport.Status.ReportDataQueued);

						var arePeriodDatesReadOnly =
								periodicityCodeUsesAccountingPeriod
								|| (reportStatus != AccComplianceReport.Status.ReportCreated && reportStatus != AccComplianceReport.Status.ReportPendingQueueing && reportStatus != AccComplianceReport.Status.ReportDataQueued);

						AssertEquals(string.Format("When report status is {0} and report periodicity is {1}, the report period field readOnly should be {2}", reportStatus, periodicity, isAccountingPeriodReadOnly),
								isAccountingPeriodReadOnly, report.AccountingPeriodInfo.ReadOnly);

						AssertEquals(string.Format("When report status is {0} and report periodicity is {1}, the report date from field readOnly should be {2}", reportStatus, periodicity, arePeriodDatesReadOnly),
														arePeriodDatesReadOnly, report.ACR_DateFromInfo.ReadOnly);

						AssertEquals(string.Format("When report status is {0} and report periodicity is {1}, the report date to field readOnly should be {2}", reportStatus, periodicity, arePeriodDatesReadOnly),
														arePeriodDatesReadOnly, report.ACR_DateToInfo.ReadOnly);
					}
				}
			}
		}

		public void TestAccountingPeriodReadOnlyForComplianceFinancialYear()
		{
			using (SetupAustraliaTparReportForComplianceFinancialYearTest())
			{
				foreach (var reportStatus in typeof(AccComplianceReport.Status).GetFields().Select(x => x.GetValue(null).ToString()).ToList())
				{
					Assert($"Accounting period", !Report.AccountingPeriodInfo.ReadOnly);
					Assert("Date from", Report.ACR_DateFromInfo.ReadOnly);
					Assert("Date to", Report.ACR_DateToInfo.ReadOnly);
				}
			}
		}

		public void TestHasPreviousPeriodData()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", includeQueuedForPreviousPeriod: false);

			Assert(!Report.IncludeQueuedForPreviousPeriod);
			Assert(!Report.HasPreviousPeriodData);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.Cast<ComplianceReportConfiguration>().First(x => x.ReportCode == Report.ACR_ReportType);
			reportConfig.IncludeQueuedForPreviousPeriod = true;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Assert(Report.IncludeQueuedForPreviousPeriod);
			Assert(!Report.HasPreviousPeriodData);

			var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.ACR_ReportType = Report.ACR_ReportType;
			previousReport.ACR_DateFrom = Report.ACR_DateFrom.AddDays(-10);
			previousReport.ACR_DateTo = Report.ACR_DateFrom.AddDays(-1);
			previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Factory.Save();

			var previousPeriodInvoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			previousPeriodInvoice.AH_PostDate = Report.ACR_DateFrom.AddDays(-1);
			Assert("Has Lines", previousPeriodInvoice.Lines.Any());
			var line01 = previousPeriodInvoice.Lines[0];
			line01.AL_AT = Creator.GST1.PK;
			var line02 = Creator.CreateInvoiceLine(previousPeriodInvoice, Creator.AUD, 1m, 200m);
			line02.AL_AT = Creator.GSTFREE1.PK;

			var invoice = Creator.CreateAPInvoice<APInvoice>("I001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Any());
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST2.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 250m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line01, line02);
			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			Assert(!Report.IsFirstReport());
			AssertEquals(2, Report.ReportLinesPreviousPeriod.Count);
			Assert(Report.IncludeQueuedForPreviousPeriod);
			Assert(Report.HasPreviousPeriodData);
		}

		public void TestIsPreviousReportFinalized()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_ReportType = "TST";
			report1.ACR_DateFrom = new ZDate(2018, 3, 1);
			report1.ACR_DateTo = new ZDate(2018, 3, 31);
			report1.ACR_Status = AccComplianceReport.Status.ReportFinalised;

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = "TST";
			report2.ACR_DateFrom = new ZDate(2018, 4, 1);
			report2.ACR_DateTo = new ZDate(2018, 4, 30);
			report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Report.ACR_ReportType = "TST";
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);

			Assert("There is no previous report so it should be true", report1.IsPreviousReportFinalized());
			Assert("previous report is report1 which is finalized", report2.IsPreviousReportFinalized());
			Assert("previous report is report2 which is not finalized", !Report.IsPreviousReportFinalized());

			Report.ACR_DateFrom = ZDate.Empty;
			Assert("if we cannot find the previous report than it should be true", Report.IsPreviousReportFinalized());

			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_ReportType = string.Empty;
			Assert("if we cannot find the previous report than it should be true", Report.IsPreviousReportFinalized());
		}

		public void TestSupportsSAFTXmlGeneration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;

			var complianceReportGUIActionProviderMock = new Mock<IComplianceReportGUIActionProvider>();
			var accountingCountryFactoryMock = new Mock<IAccountingCountryFactory>();
			accountingCountryFactoryMock.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(complianceReportGUIActionProviderMock.Object);
			var globalAccountingCountryFactoryMock = new Mock<IGlobalAccountingCountryFactory>();
			globalAccountingCountryFactoryMock.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(accountingCountryFactoryMock.Object);
			ObjectFactory.Substitute(globalAccountingCountryFactoryMock.Object);

			foreach (var reportType in new[] { AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.ReportTypes.SAFTOnlyTransactions })
			{
				reportConfig.ReportCode = reportType;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report.ACR_ReportType = reportType;
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				complianceReportGUIActionProviderMock.SetupGet(x => x.IsCountrySupportGenerateSAFT).Returns(false);

				Assert("IsCountrySupportGenerateSAFT affect DoesCountrySupportSAFTGeneration.", !AccountingUtils.DoesCountrySupportSAFTGeneration);
				Assert("DoesCountrySupportSAFTGeneration affect SupportsSAFT.", !Report.SupportsSAFT);

				complianceReportGUIActionProviderMock.SetupGet(x => x.IsCountrySupportGenerateSAFT).Returns(true);
				Assert(AccountingUtils.DoesCountrySupportSAFTGeneration);
				Assert("DoesCountrySupportSAFTGeneration affect SupportsSAFT.", Report.SupportsSAFT);

				Report.ACR_ReportType = "TST";
				Assert("SAFT not supported because not 'SAF' type", !Report.SupportsSAFT);
				Report.ACR_ReportType = reportType;
				Assert("SAFT supported for PT company, 'SAF' type", Report.SupportsSAFT);

				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert("SAFT XML not supported because not Generated", !Report.SupportsSAFTXmlGeneration);

				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Assert("SAFT XML supported", Report.SupportsSAFTXmlGeneration);
			}
		}

		public void TestSupportsMTD()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "MTD";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			Report.FillWithValidTestData();
			Report.ACR_ReportType = "MTD";
			Report.ACR_DateFrom = ZDate.Today.AddDays(-10);
			Report.ACR_DateTo = ZDate.Today.AddDays(10);
			Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();

			AssertNotEquals(Core.Constants.CountryCodes.UnitedKingdom, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("compliance report configuration is set", ReportLineGroupingListCodes.TaxReporting, Report.ReportLineGrouping);
			Assert("MTD not supported because not United Kingdom", !Report.SupportsMTD);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.DefaultValue);
				complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				reportConfig = complianceConfig.Cast<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "MTD");
				AssertNotNull(reportConfig);
				AssertEquals(PeriodicityCodes.DateRange, reportConfig.ReportPeriodicity);
				AssertEquals(Env.CurrentCompany.Country.Code, reportConfig.Country);
				AssertEquals("VAT", reportConfig.TaxRegistrationType);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, reportConfig.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, reportConfig.ReportLineGrouping);

				AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("compliance report configuration is set for United Kingdom", ReportLineGroupingListCodes.TaxReporting, Report.ReportLineGrouping);
				Assert("MTD supported", Report.SupportsMTD);

				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert("MTD not supported because wrong report status type", !Report.SupportsMTD);

				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Assert("MTD supported because report is generated", Report.SupportsMTD);

				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Assert("MTD supported because report is finalised", Report.SupportsMTD);
			}
		}

		public void TestSupportsTPAR()
		{
			AssertEquals("Pre-condition: Country is Australia", Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				Report.FillWithValidTestData();
				Report.ACR_ReportType = AccountingConstants.ComplianceReportTypes.TaxablePaymentsAnnualReportType;
				Report.ACR_DateFrom = ZDate.Today.AddDays(-10);
				Report.ACR_DateTo = ZDate.Today.AddDays(10);
				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Factory.Save();
				Assert("TPAR not supported because of Status", !Report.SupportsTPAR);

				Report.ACR_Status = AccComplianceReport.Status.ReportDataQueued;
				Factory.Save();
				Assert("TPAR not supported because of Status", !Report.SupportsTPAR);

				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();
				Assert("TPAR is supported", Report.SupportsTPAR);

				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Factory.Save();
				Assert("TPAR is supported", Report.SupportsTPAR);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					Assert("TPAR not supported because it is not Australia", !Report.SupportsTPAR);
				}

				Assert("TPAR is supported", Report.SupportsTPAR);
				using (new DisposableAction(() => Environment.Env.Security.FinalizeComplianceReport.IsAllowed = false, () => Environment.Env.Security.FinalizeComplianceReport.IsAllowed = true))
				{
					Assert("TPAR not supported because of security", !Report.SupportsTPAR);
				}

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "ABN";
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Assert("TPAR is supported", Report.SupportsTPAR);
				Report.ACR_ReportType = "TST";
				Assert("TPAR not supported because of wrong Report Type", !Report.SupportsTPAR);
			}
		}

		public void TestPtrsSmallBusinessSupportsImportABN()
		{
			AssertSupportsImportABNs(ComplianceReportTypes.PaymentTimesSmallBusinessReportType);
		}

		public void TestPtrsSmallBusiness2024SupportsImportABN()
		{
			AssertSupportsImportABNs(ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType);
		}

		void AssertSupportsImportABNs(string reportType)
		{
			var creator = new TestObjectCreator(Factory);
			var report = creator.CreateComplianceReport(reportType, AccComplianceReport.Status.ReportCreated);
			Assert("User need save report first", !report.IsInDatabase);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				CombineAssertions("if user did not generate report, user can import ABNs", () =>
				{
					var allowStatus = new[]
					{
						AccComplianceReport.Status.ReportCreated,
						AccComplianceReport.Status.ReportDataQueued,
						AccComplianceReport.Status.ReportPendingQueueing,
					};
					foreach (var status in allowStatus)
					{
						report.ACR_Status = status;
						Assert(report.SupportsImportABNs);
					}
				});

				CombineAssertions("if user has generated report, user can not import ABNs", () =>
				{
					var denyStatus = new[]
					{
						AccComplianceReport.Status.ReportFinalised,
						AccComplianceReport.Status.ReportGenerated,
						AccComplianceReport.Status.ReportInvalidated,
						AccComplianceReport.Status.ReportError,
					};
					foreach (var status in denyStatus)
					{
						report.ACR_Status = status;
						Assert(!report.SupportsImportABNs);
					}
				});
			}
		}

		public void TestSupportsEsterometroXmlGeneration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = AccComplianceReport.ReportTypes.Esterometro;
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			Report.ACR_ReportType = AccComplianceReport.ReportTypes.Esterometro;
			Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			AssertNotEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("compliance report configuration is set", ReportLineGroupingListCodes.TaxReporting, Report.ReportLineGrouping);
			Assert("Esterometro not supported because not Italy", !Report.SupportsEsterometro);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "IVA";
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report.ACR_ReportType = "TST";
				Assert("Esterometro not supported because not 'EST' type", !Report.SupportsEsterometro);
				Report.ACR_ReportType = AccComplianceReport.ReportTypes.Esterometro;

				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert("Esterometro XML not supported because not Generated", !Report.SupportsSAFTXmlGeneration);
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				AssertEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				Assert("Esterometro XML supported", Report.SupportsEsterometroXmlGeneration);

				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Assert("Esterometro XML supported", Report.SupportsEsterometroXmlGeneration);

				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert("Esterometro Xml not supported because wrong report status type", !Report.SupportsEsterometroXmlGeneration);
				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			}
		}

		public void TestSupportsLiquidazioneIVA()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = AccComplianceReport.ReportTypes.LiquidazioneIVA;
			reportConfig.ReportTitle = "Test Liquidazione Iva";
			reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			Report.ACR_ReportType = AccComplianceReport.ReportTypes.LiquidazioneIVA;
			Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			AssertNotEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("compliance report configuration is set", ReportLineGroupingListCodes.TaxReporting, Report.ReportLineGrouping);
			Assert("Liquidazione IVA not supported because not Italy", !Report.SupportsLiquidazioneIVA);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "IVA";
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report.ACR_ReportType = "TST";
				Assert("Liquidazione IVA not supported because not 'LIQ' type", !Report.SupportsLiquidazioneIVA);
				Report.ACR_ReportType = AccComplianceReport.ReportTypes.LiquidazioneIVA;

				Report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert("Liquidazione IVA not supported because not Generated", !Report.SupportsLiquidazioneIVA);
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				AssertEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				Assert("Liquidazione IVA supported", Report.SupportsLiquidazioneIVA);
			}
		}

		public void TestSummaryReportData()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				Report.FillWithValidTestData();
				Report.ACR_ReportType = AccComplianceReport.ReportTypes.LiquidazioneIVA;
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				Factory.Save();

				AssertEquals("compliance report configuration is set", ReportLineGroupingListCodes.TaxReporting, Report.ReportLineGrouping);
				Assert("Liquidazione IVA supported", Report.SupportsLiquidazioneIVA);

				for (int i = 1; i < 6; i++)
				{
					var summaryReport = new AccComplianceReportLineCollectionBase<AccComplianceReportSummaryLine>(Report);

					var table = ((INeedTable)summaryReport).Table;
					var row = table.NewRow();

					row[AccComplianceReportSummaryLine.Schema.ColumnName] = "B" + i.ToString();
					row[AccComplianceReportSummaryLine.Schema.Value] = (decimal)(i * 11);
					row[AccComplianceReportSummaryLine.Schema.Adjustment] = (decimal)(i);
					row[AccComplianceReportSummaryLine.Schema.Total] = (decimal)(i * 12);
					row[AccComplianceReportSummaryLine.Schema.Comment] = "Error To test";

					table.Rows.Add(row);
					row.AcceptChanges();

					var result = new AccComplianceReportSummaryLine(Factory, row);
					Report.ReportSummaryLines.Add(result);
				}

				AssertEquals("Summary report - Row 1 Total", Report.ReportSummaryLines[0].Total, 12m);
				AssertEquals("Summary report - Row 2 Total", Report.ReportSummaryLines[1].Value, 22m);
				AssertEquals("Summary report - Row 3 Total", Report.ReportSummaryLines[2].Total, 36m);
				AssertEquals("Summary report - Row 4 Total", Report.ReportSummaryLines[3].Value, 44m);
				AssertEquals("Summary report - Row 5 Total", Report.ReportSummaryLines[4].Total, 60m);
			}
		}

		public void TestSupportsExportVATFile()
		{
			Action<string, string> setupConfig = (string reportCode, string tablePrefix) =>
			{
				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = reportCode;
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = tablePrefix;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				setupConfig("TS1", ReportBaseTablePrefixListCodes.ComplianceDocumentHeader);
				Report.ACR_ReportType = "TS1";
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Assert("Export VAT is supported", Report.SupportsExportVATFile);

				setupConfig("TS2", ReportBaseTablePrefixListCodes.AllTransactions);
				Report.ACR_ReportType = "TS2";
				Assert("Export VAT is not supported because table prefix is not compliance document header", !Report.SupportsExportVATFile);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				setupConfig("TS3", ReportBaseTablePrefixListCodes.ComplianceDocumentHeader);
				Report.ACR_ReportType = "TS3";
				Report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Assert("Export VAT is not supported because curent country is not TW", !Report.SupportsExportVATFile);
			}
		}

		public void TestSupportsDayBook()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			var curCompany = GlbCompany.CurrentCompany;
			var testDateTime = new DateTime(2023, 1, 1);
			Creator.CreateTestPeriodsForEntireYear(curCompany, 2023);
			Creator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(curCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testDateTime);
			FallbackLevel fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			reportConfig.CurrentFallbackLevel = fallbackLevel;

			Report.ACR_ReportType = "TST";

			foreach (var tablePrefix in new ComplianceReportConfigurationLookups(reportConfig).ReportBaseTablePrefixList.ToList<CodeDescriptionPair>().Select(x => x.Code))
			{
				reportConfig.ReportBaseTablePrefix = tablePrefix;
				var groupingCodes = new ComplianceReportConfigurationLookups(reportConfig).ReportLineGroupingList.ToList<CodeDescriptionPair>().Select(x => x.Code);
				foreach (var groupingCode in groupingCodes)
				{
					reportConfig.ReportBaseTablePrefix = tablePrefix;
					reportConfig.ReportLineGrouping = groupingCode;
					AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

					var isPrefixAll = tablePrefix == ReportBaseTablePrefixListCodes.AllTransactions || tablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData;
					var isCodeDayBook = groupingCode == ReportLineGroupingListCodes.DayBook || groupingCode == ReportLineGroupingListCodes.DayBookWithoutGrouping || groupingCode == ReportLineGroupingListCodes.DayBookWithPresentation;
					AssertEquals("Should support day book if prefix is ** and code is day book", isPrefixAll && isCodeDayBook, Report.SupportsDayBook);
				}
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccComplianceReport()
		{
			var localList = new List<string>
			{
				nameof(Report.GLOpeningBalanceDR),
				nameof(Report.GLOpeningBalanceCR),
				nameof(Report.GeneralLedgerAmountDR),
				nameof(Report.GeneralLedgerAmountCR),
			};

			var tester = new DecimalPlacesAttributeTester(Report, Report.Company);
			tester.CheckLocalCurrency(localList, nameof(Report.LocalDecimals));
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			Report.FillWithValidTestData();

			var taxReturn = Factory.New<AccTaxReturn>();
			taxReturn.ATR_ACR_ComplianceReport = Report.PK;
			Factory.Save();

			AssertCollectionContains("BusinessObjectsWithRelatedEvents should contain taxReturn.", taxReturn, Report.BusinessObjectsWithRelatedEvents);
		}

		public void TestUniqueReportID()
		{
			AssertEquals("ACR_GC_Company", GlbCompany.CurrentCompany.PK, Report.ACR_GC_Company);
			Report.ACR_ReportType = "TST";
			Report.ACR_Periodicity = "RNG";
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");

			Report.ACR_GB_Branch = GlbBranch.CurrentBranch.PK;
			Report.ACR_DateFrom = new ZDate(2015, 3, 16);
			Report.ACR_DateTo = new ZDate(2015, 3, 23);
			AssertEquals("UniqueReportID", "EDI-BNE-AU-TST-20150316-20150323", Report.UniqueReportID);
		}

		public void TestReportConfiguration()
		{
			Report.FillWithValidTestData();
			AssertEquals("ReportBaseTablePrefix", ZString.Empty, Report.ReportBaseTablePrefix);
			AssertEquals("ReportLineGrouping", ZString.Empty, Report.ReportLineGrouping);
			AssertEquals("GoodsServiceType", ZString.Empty, Report.GoodsServiceType);
			AssertEquals("TaxRegistrationType", ZString.Empty, Report.TaxRegistrationType);
			AssertEquals("ReportTypeDescription", "", Report.ReportTypeDescription);

			Creator.CreateConfigurationForComplianceReport(Report, "AL", "HDR", "SRV");
			AssertEquals("ReportBaseTablePrefix", "AL", Report.ReportBaseTablePrefix);
			AssertEquals("ReportLineGrouping", "HDR", Report.ReportLineGrouping);
			AssertEquals("GoodsServiceType", "SRV", Report.GoodsServiceType);
			AssertEquals("TaxRegistrationType", "1ST", Report.TaxRegistrationType);
			AssertEquals("ReportTypeDescription", Report.ACR_ReportType + " Report Title", Report.ReportTypeDescription);
		}

		public void TestPeriodicityDescriptions()
		{
			var complianceCountry = Core.Constants.CountryCodes.Australia;
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			var reportType = ComplianceReportTypes.TaxablePaymentsAnnualReportType;
			reportConfig.ReportCode = reportType;
			reportConfig.ReportTitle = "Test Report";
			reportConfig.Country = complianceCountry;
			reportConfig.ReportPeriodicity = PeriodicityCodes.AccountingPeriod;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Report.FillWithValidTestData();
			Report.ACR_ReportType = reportType;

			foreach (CodeDescriptionPair code in Report.Lookups.PeriodicityList)
			{
				Report.ACR_Periodicity = code.Code;
				AssertEquals("PeriodicityDescription", code.Description, Report.PeriodicityDescription);
			}
		}

		[TestDate(2015, 7, 7, 14, 22, 25)]
		public void TestReportAuditDetais()
		{
			AssertEquals("ACR_Calc_CreatedTime", ZDateTime.Empty, Report.ACR_Calc_CreatedTime);
			AssertEquals("ACR_Calc_CreatingUser", "", Report.ACR_Calc_CreatingUser);
			AssertEquals("ACR_Calc_LastEditedTime", ZDateTime.Empty, Report.ACR_Calc_LastEditedTime);
			AssertEquals("ACR_Calc_LastEditUser", "", Report.ACR_Calc_LastEditUser);

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "HDR", "SRV");
			AssertEquals("ACR_Calc_CreatedTime", ZDateTime.Empty, Report.ACR_Calc_CreatedTime);
			AssertEquals("ACR_Calc_CreatingUser", "", Report.ACR_Calc_CreatingUser);
			AssertEquals("ACR_Calc_LastEditedTime", ZDateTime.Empty, Report.ACR_Calc_LastEditedTime);
			AssertEquals("ACR_Calc_LastEditUser", "", Report.ACR_Calc_LastEditUser);

			Factory.Save();
			var currentTime = new ZDateTime(2015, 7, 7, 14, 22, 25);
			AssertEquals("ACR_Calc_CreatedTime", currentTime, Report.ACR_Calc_CreatedTime);
			AssertEquals("ACR_Calc_CreatingUser", "E", Report.ACR_Calc_CreatingUser);
			AssertEquals("ACR_Calc_LastEditedTime", ZDateTime.Empty, Report.ACR_Calc_LastEditedTime);
			AssertEquals("ACR_Calc_LastEditUser", "", Report.ACR_Calc_LastEditUser);

			Report.ACR_Description = "Changed";
			Factory.Save();
			AssertEquals("ACR_Calc_CreatedTime", currentTime, Report.ACR_Calc_CreatedTime);
			AssertEquals("ACR_Calc_CreatingUser", "E", Report.ACR_Calc_CreatingUser);
			AssertEquals("ACR_Calc_LastEditedTime", currentTime, Report.ACR_Calc_LastEditedTime);
			AssertEquals("ACR_Calc_LastEditUser", "E", Report.ACR_Calc_LastEditUser);
		}

		public void TestCannotUpdateStatusOnUnsavedComplianceReport()
		{
			Report.FillWithValidTestData();

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = Report.ACR_ReportType;
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			var developerError = "Cannot update Status on unsaved Compliance Report.";

			Report.ReQueue();
			AssertEquals("Developer Error Reported", developerError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Report.GenerateFromQueue();
			AssertEquals("Developer Error Reported", developerError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			Report.Finalise();
			AssertEquals("Developer Error Reported", developerError, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestStatusMessages()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var repFECAdd = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, AccComplianceReport.ReportTypes.FEC);
				var repFECQue = CreateReportForStatusMessage(AccComplianceReport.Status.ReportDataQueued, AccComplianceReport.ReportTypes.FEC);
				var repFECPqu = CreateReportForStatusMessage(AccComplianceReport.Status.ReportPendingQueueing, AccComplianceReport.ReportTypes.FEC);
				var repFECGen = CreateReportForStatusMessage(AccComplianceReport.Status.ReportGenerated, AccComplianceReport.ReportTypes.FEC);
				var repFECFin = CreateReportForStatusMessage(AccComplianceReport.Status.ReportFinalised, AccComplianceReport.ReportTypes.FEC);

				CombineAssertions(() =>
				{
					AssertEquals(AccComplianceReport.Status.ReportCreated + " status default message (repFECAdd)", "Report is waiting to be queued. Please wait.", repFECAdd.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportDataQueued + " status default message (repFECQue)", "Report is being generated. Please wait.", repFECQue.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportPendingQueueing + " status default message (repFECPqu)", "Report is being re-queued. Please wait.", repFECPqu.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportGenerated + " status default message (repFECGen)", "Report has been generated. Please check eDocs.", repFECGen.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportFinalised + " status with empty message (repFECFin)", ZString.Empty, repFECFin.ACR_StatusMessage);
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				// enable ZMD report which is currently still disabled by default
				var reportTypeList = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				reportTypeList.Cast<CodeDescriptionBool>().Where(v => !v.Bool).ForEach(x => x.Bool = true);
				AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportTypeList);

				var repZMDAdd = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, AccComplianceReport.ReportTypes.ZMGermany);
				var repZMDQue = CreateReportForStatusMessage(AccComplianceReport.Status.ReportDataQueued, AccComplianceReport.ReportTypes.ZMGermany);
				var repZMDPqu = CreateReportForStatusMessage(AccComplianceReport.Status.ReportPendingQueueing, AccComplianceReport.ReportTypes.ZMGermany);
				var repZMDGen = CreateReportForStatusMessage(AccComplianceReport.Status.ReportGenerated, AccComplianceReport.ReportTypes.ZMGermany);
				var repZMDFin = CreateReportForStatusMessage(AccComplianceReport.Status.ReportFinalised, AccComplianceReport.ReportTypes.ZMGermany);

				CombineAssertions(() =>
				{
					AssertEquals(AccComplianceReport.Status.ReportCreated + " status default message (repZMDAdd)", "Report is waiting to be queued. Please wait.", repZMDAdd.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportDataQueued + " status default message (repZMDQue)", "Report data has been queued. Please click Generate to proceed.", repZMDQue.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportPendingQueueing + " status default message (repZMDPqu)", "Report is being re-queued. Please wait.", repZMDPqu.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportGenerated + " status default message (repZMDGen)", "Report has been generated.", repZMDGen.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportFinalised + " status with empty message (repZMDFin)", ZString.Empty, repZMDFin.ACR_StatusMessage);
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				var repJPKAdd = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, AccComplianceReport.ReportTypes.JPKV7M);
				var repJPKQue = CreateReportForStatusMessage(AccComplianceReport.Status.ReportDataQueued, AccComplianceReport.ReportTypes.JPKV7M);
				var repJPKPqu = CreateReportForStatusMessage(AccComplianceReport.Status.ReportPendingQueueing, AccComplianceReport.ReportTypes.JPKV7M);
				var repJPKGen = CreateReportForStatusMessage(AccComplianceReport.Status.ReportGenerated, AccComplianceReport.ReportTypes.JPKV7M);
				var repJPKOut = CreateReportForStatusMessage(AccComplianceReport.Status.ReportOutputGenerated, AccComplianceReport.ReportTypes.JPKV7M);
				var repJPKFin = CreateReportForStatusMessage(AccComplianceReport.Status.ReportFinalised, AccComplianceReport.ReportTypes.JPKV7M);

				CombineAssertions(() =>
				{
					AssertEquals(AccComplianceReport.Status.ReportCreated + " status default message (repJPKAdd)", "Report is waiting to be generated. Please click Generate to proceed.", repJPKAdd.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportDataQueued + " status with empty message (repJPKQue)", ZString.Empty, repJPKQue.ACR_StatusMessage);       // It is expected that this Status is never set.
					AssertEquals(AccComplianceReport.Status.ReportPendingQueueing + " status with empty message (repJPKPqu)", ZString.Empty, repJPKPqu.ACR_StatusMessage);  // It is expected that this Status is never set.
					AssertEquals(AccComplianceReport.Status.ReportGenerated + " status default message (repJPKGen)", "Report has been generated. Please wait for output to be generated.", repJPKGen.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportOutputGenerated + " status default message (repJPKGen)", "Output has been generated for this Report. Please check eDocs.", repJPKOut.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportFinalised + " status with empty message (repJPKFin)", ZString.Empty, repJPKFin.ACR_StatusMessage);
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var repOTHAdd = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, "OTH");
				var repOTHQue = CreateReportForStatusMessage(AccComplianceReport.Status.ReportDataQueued, "OTH");
				var repOTHPqu = CreateReportForStatusMessage(AccComplianceReport.Status.ReportPendingQueueing, "OTH");
				var repOTHGen = CreateReportForStatusMessage(AccComplianceReport.Status.ReportGenerated, "OTH");
				var repOTHFin = CreateReportForStatusMessage(AccComplianceReport.Status.ReportFinalised, "OTH");
				var repOTHInv = CreateReportForStatusMessage(AccComplianceReport.Status.ReportInvalidated, "OTH");

				var repCUS = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, "OTH", "Custom Message");

				var repERR1 = CreateReportForStatusMessage(AccComplianceReport.Status.ReportError, "ER1");
				var repERR2 = CreateReportForStatusMessage(AccComplianceReport.Status.ReportError, "ER2", "Custom Error Message");

				CombineAssertions(() =>
				{
					AssertEquals(AccComplianceReport.Status.ReportCreated + " status default message (repOTHAdd)", "Report is waiting to be generated. Please click Generate to proceed.", repOTHAdd.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportDataQueued + " status with empty message (repOTHQue)", ZString.Empty, repOTHQue.ACR_StatusMessage);       // It is expected that this Status is never set.
					AssertEquals(AccComplianceReport.Status.ReportPendingQueueing + " status with empty message (repOTHPqu)", ZString.Empty, repOTHPqu.ACR_StatusMessage); // It is expected that this Status is never set.
					AssertEquals(AccComplianceReport.Status.ReportGenerated + " status default message (repOTHGen)", "Report has been generated.", repOTHGen.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportFinalised + " status with empty message (repOTHFin)", ZString.Empty, repOTHFin.ACR_StatusMessage);
					AssertEquals(AccComplianceReport.Status.ReportInvalidated + " status default message (repOTHInv)", "Report Invalidated by Transactions updated.", repOTHInv.ACR_StatusMessage);

					AssertEquals("Report with custom message", "Custom Message", repCUS.ACR_StatusMessage);
					repCUS.ACR_Status = AccComplianceReport.Status.ReportGenerated;
					AssertEquals("CUS Report with default message when status is changed to GEN", "Report has been generated.", repCUS.ACR_StatusMessage);

					AssertEquals("ERR status with empty message", ZString.Empty, repERR1.ACR_StatusMessage);
					AssertEquals("ERR status with assigned message", "Custom Error Message", repERR2.ACR_StatusMessage);
				});
			}
		}

		public void TestStatusMessagesDayBookReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "DAY";
				reportConfig.ReportTitle = "TEST DAY BOOK REPORT";
				reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;
				reportConfig.TaxRegistrationType = "VGM";

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK,
					Guid.Empty, Guid.Empty, complianceConfig);

				var dayBookRepAdd = CreateReportForStatusMessage(AccComplianceReport.Status.ReportCreated, "DAY");
				var dayBookRepQue = CreateReportForStatusMessage(AccComplianceReport.Status.ReportDataQueued, "DAY");
				var dayBookRepPqu = CreateReportForStatusMessage(AccComplianceReport.Status.ReportPendingQueueing, "DAY");
				var dayBookRepGen = CreateReportForStatusMessage(AccComplianceReport.Status.ReportGenerated, "DAY");
				var dayBookRepFin = CreateReportForStatusMessage(AccComplianceReport.Status.ReportFinalised, "DAY");

				CombineAssertions(() =>
				{
					AssertEquals("Report is waiting to be queued. Please wait.", dayBookRepAdd.ACR_StatusMessage);
					AssertEquals("Report data has been queued. Please click Generate to proceed.", dayBookRepQue.ACR_StatusMessage);
					AssertEquals("Report is being re-queued. Please wait.", dayBookRepPqu.ACR_StatusMessage);
					AssertEquals("Report has been generated.", dayBookRepGen.ACR_StatusMessage);
					AssertEquals(ZString.Empty, dayBookRepFin.ACR_StatusMessage);
				});
			}
		}

		AccComplianceReport CreateReportForStatusMessage(string status, string reportType, string statusMessage = "")
		{
			var report = Factory.New<AccComplianceReport>();
			report.ACR_Status = status;
			report.ACR_ReportType = reportType;
			report.ACR_StatusMessage = statusMessage;

			return report;
		}

		public void TestFinalisedComplianceDocuments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Report.FillWithValidTestData();
				Report.ACR_ReportType = "TXT";
				Report.ACR_DateFrom = new ZDate(2018, 7, 1);
				Report.ACR_DateTo = new ZDate(2018, 7, 31);
				Factory.Save();

				var invoice = Creator.CreateInvoice(typeof(ARInvoice), "INV001", Creator.TWD, 1M, Creator.ABIGAS);
				var line = Creator.CreateInvoiceLine(invoice, Creator.TWD, 1M, 100M, 10M, 0M, Creator.CC1.PK);
				line.AL_AT = Creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var complianceDocument = Factory.LoadTop1<AccComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, Creator.ABIGAS.PK));
				complianceDocument.ADH_DocumentDate = new ZDate(2018, 7, 5);
				complianceDocument.ADH_ComplianceSubType = "TXI";
				complianceDocument.ADH_DocumentNumber = "001";
				complianceDocument.ADH_ReportingPeriod = 201807;
				Factory.Save();

				Report.GenerateFromQueue();
				AssertEquals(Core.Constants.ComplianceDocumentStatus.NumberSet, complianceDocument.ADH_DocumentStatus);

				Report.Finalise();
				AssertEquals(Core.Constants.ComplianceDocumentStatus.Finalised, complianceDocument.ADH_DocumentStatus);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
				complianceDocument.Logs.HasLogWith(query);
			}
		}

		public void TestACR_IsFinalisedAndACR_Status()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");
			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, false, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportCreated, Report.ACR_Status);

			var invoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportCreated, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Created", Report.ReportStatusDescription);

			Report.GenerateFromQueue();
			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Generated", Report.ReportStatusDescription);

			invoice.AH_ComplianceSubType = "TXI";
			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Generated", Report.ReportStatusDescription);

			Factory.Save();
			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportInvalidated, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Invalidated by Transactions updated", Report.ReportStatusDescription);

			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Report.GenerateFromQueue();
			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Generated", Report.ReportStatusDescription);

			System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(100));

			Report.Finalise();
			AssertEquals("ACR_IsFinalised", true, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Finalized", Report.ReportStatusDescription);
		}

		public void TestACR_IsFinalisedWhenConfigurationTablePrefixIsGLD()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "GLD", "");
			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, false, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportCreated, Report.ACR_Status);

			var invoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Report.Finalise();
			AssertEquals("ACR_IsFinalised", true, Report.ACR_IsFinalised);
			AssertEquals("ACR_Status", AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);
			AssertEquals("ReportStatusDescription", "Report Finalized", Report.ReportStatusDescription);

			Report.ACR_Status = AccComplianceReport.Status.ReportInvalidated;
			Factory.Save();

			AssertEquals("ACR_IsFinalised", false, Report.ACR_IsFinalised);
		}

		public void TestACR_Status_OnUpdateComplianceSubType()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");
			AssertEquals(AccComplianceReport.Status.ReportCreated, Report.ACR_Status);

			var invoice = Creator.CreateAPInvoice<APInvoice>("I0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			invoice.AH_ComplianceSubType = "TXI";
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line1);
			AssertEquals(AccComplianceReport.Status.ReportCreated, Report.ACR_Status);
			Report.GenerateFromQueue();
			AssertEquals(AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);

			var newFactory = new BusinessObjectFactory();
			var govInvoice = newFactory.Load<GovernmentInvoice>(invoice.PK);
			govInvoice.AH_ComplianceSubType = "ARI";
			AssertNoExceptionThrown("Should not throw exception when Invoice saved from update form", () => newFactory.Save());
			AssertEquals(AccComplianceReport.Status.ReportInvalidated, Report.ACR_Status);

			Creator.CreateComplianceReportQueueEntry(Report, line1);
			Report.GenerateFromQueue();
			AssertEquals(AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			Report.Finalise();
			AssertEquals(AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);

			newFactory = new BusinessObjectFactory();
			govInvoice = newFactory.Load<GovernmentInvoice>(invoice.PK);
			govInvoice.AH_ComplianceSubType = "ARE";
			AssertNoExceptionThrown("Should not throw exception when Invoice saved from update form", () => newFactory.Save());
			AssertEquals(AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);
		}

		[TestDate(2018, 08, 15)]
		public void TestIncludeQueuedTransactionsFromPreviousPeriod()
		{
			AssertIncludeQueuedTransactionsFromPreviousPeriodWhenNeeded(true);
		}

		[TestDate(2018, 08, 15)]
		public void TestDoesNotIncludeQueuedTransactionsFromPreviousPeriod()
		{
			AssertIncludeQueuedTransactionsFromPreviousPeriodWhenNeeded(false);
		}

		void AssertIncludeQueuedTransactionsFromPreviousPeriodWhenNeeded(bool includeQueuedForPreviousPeriod)
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", "", null, includeQueuedForPreviousPeriod);

			Report.ACR_DateFrom = new ZDate(2018, 7, 1);
			Report.ACR_DateTo = new ZDate(2018, 7, 31);

			var invoice1 = Creator.CreateAPInvoice<APInvoice>("I0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice1.Lines.Count > 0);
			var line1_1 = invoice1.Lines[0];
			line1_1.AL_AT = Creator.GST1.PK;
			var line1_2 = Creator.CreateInvoiceLine(invoice1, Creator.AUD, 1m, 200m);
			line1_2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, string.Empty, new ZDateTime(2018, 07, 15), line1_1, line1_2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			Report.Finalise();
			AssertEquals(AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);

			var invoice2 = Creator.CreateAPInvoice<APInvoice>("I0002", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AT = Creator.GST1.PK;
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.AUD, 1m, 150m);
			line2_2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();
			line2_1.AL_PostDate = new ZDateTime(2018, 07, 20);
			line2_2.AL_PostDate = new ZDateTime(2018, 07, 20);
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, string.Empty, new ZDateTime(2018, 07, 20), line2_1, line2_2);

			var invoice3 = Creator.CreateAPInvoice<APInvoice>("I0003", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice3.Lines.Count > 0);
			var line3_1 = invoice3.Lines[0];
			line3_1.AL_AT = Creator.GST1.PK;
			var line3_2 = Creator.CreateInvoiceLine(invoice3, Creator.AUD, 1m, 225m);
			line3_2.AL_AT = Creator.GSTFREE1.PK;

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = Report.ACR_ReportType;
			report2.ACR_DateFrom = new ZDate(2018, 8, 1);
			report2.ACR_DateTo = new ZDate(2018, 8, 31);
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(report2, string.Empty, new ZDateTime(2018, 08, 15), line3_1, line3_2);
			report2.GenerateFromQueue();

			report2.ClearReportLines_ForTestOnly();
			var hitCountBefore = Db.Connection.ExecutedCommandCount;
			AssertEquals("ReportLines.Count", includeQueuedForPreviousPeriod ? 4 : 2, report2.ReportLines.Count);
			var hitCountAfterLines = Db.Connection.ExecutedCommandCount;
			AssertEquals("Getting ReportLines does a Db Hit", 1, hitCountAfterLines - hitCountBefore);

			Assert("ReportTotals present", report2.ReportTotals.Any());
			var hitCountAfterTotals = Db.Connection.ExecutedCommandCount;
			AssertEquals("Getting ReportTotals includes a Db Hit from the GetMovementsTotals", 1, hitCountAfterTotals - hitCountAfterLines);

			AssertEquals(includeQueuedForPreviousPeriod ? -575m : -325m, report2.ReportTotals[0].TotalExTaxAmount);
			AssertEquals(includeQueuedForPreviousPeriod ? -20m : -10m, report2.ReportTotals[0].TotalTaxAmount);

			Assert("ReportTotalsCurrentPeriod present", report2.ReportTotalsCurrentPeriod.Any());
			AssertEquals(-325m, report2.ReportTotalsCurrentPeriod[0].TotalExTaxAmount);
			AssertEquals(-10m, report2.ReportTotalsCurrentPeriod[0].TotalTaxAmount);

			Assert("ReportTotalsPreviousPeriod present", includeQueuedForPreviousPeriod == report2.ReportTotalsPreviousPeriod.Any());
			if (includeQueuedForPreviousPeriod)
			{
				AssertEquals(-250m, report2.ReportTotalsPreviousPeriod[0].TotalExTaxAmount);
				AssertEquals(-10m, report2.ReportTotalsPreviousPeriod[0].TotalTaxAmount);
			}
		}

		[TestDate(2018, 08, 15)]
		public void TestFinalizeDeletePreviousRecordsWhenIncludeQueuedTransactionsFromPreviousPeriodIsTrueAndIsFirstReport()
		{
			AssertFinalizeDeletePreviousRecords(true, true);
		}

		public void TestFinalizeDeletePreviousRecordsWhenIncludeQueuedTransactionsFromPreviousPeriodIsTrueAndIsNotFirstReport()
		{
			AssertFinalizeDeletePreviousRecords(true, false);
		}

		[TestDate(2018, 08, 15)]
		public void TestFinalizeDoesNotDeletePreviousRecordsWhenIncludeQueuedTransactionsFromPreviousPeriodIsFalse()
		{
			AssertFinalizeDeletePreviousRecords(false);
		}

		public void AssertFinalizeDeletePreviousRecords(bool includeQueuedForPreviousPeriod, bool isFirstReport = false)
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", "", null, includeQueuedForPreviousPeriod);

			Report.ACR_DateFrom = new ZDate(2018, 7, 1);
			Report.ACR_DateTo = new ZDate(2018, 7, 31);
			if (!isFirstReport)
			{
				var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
				previousReport.ACR_ReportType = Report.ACR_ReportType;
				previousReport.ACR_DateFrom = new ZDate(2018, 6, 1);
				previousReport.ACR_DateTo = new ZDate(2018, 6, 30);
				previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			}
			Factory.Save();

			var invoice1 = Creator.CreateAPInvoice<APInvoice>("I0004", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice1.Lines.Count > 0);
			var line1_1 = invoice1.Lines[0];
			line1_1.AL_AT = Creator.GST1.PK;
			var line1_2 = Creator.CreateInvoiceLine(invoice1, Creator.AUD, 1m, 200m);
			line1_2.AL_AT = Creator.GSTFREE1.PK;
			invoice1.AH_PostDate = new ZDate(2018, 6, 15);
			Factory.Save();

			var invoice2 = Creator.CreateAPInvoice<APInvoice>("I0005", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line2_1 = invoice2.Lines[0];
			line2_1.AL_AT = Creator.GST1.PK;
			var line2_2 = Creator.CreateInvoiceLine(invoice2, Creator.AUD, 1m, 200m);
			line2_2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, string.Empty, new ZDateTime(2018, 06, 15), line1_1, line1_2);
			Creator.CreateComplianceReportQueueEntry(Report, string.Empty, new ZDateTime(2018, 07, 15), line2_1, line2_2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", includeQueuedForPreviousPeriod && !isFirstReport ? 4 : 2, Report.ReportLines.Count);

			Report.Finalise();
			AssertEquals(AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);

			var invoice3 = Creator.CreateAPInvoice<APInvoice>("I0006", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice3.Lines.Count > 0);
			var line3_1 = invoice3.Lines[0];
			line3_1.AL_AT = Creator.GST1.PK;
			var line3_2 = Creator.CreateInvoiceLine(invoice3, Creator.AUD, 1m, 200m);
			line3_2.AL_AT = Creator.GSTFREE1.PK;

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = Report.ACR_ReportType;
			report2.ACR_DateFrom = new ZDate(2018, 8, 1);
			report2.ACR_DateTo = new ZDate(2018, 8, 31);
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(report2, string.Empty, new ZDateTime(2018, 08, 15), line3_1, line3_2);
			report2.GenerateFromQueue();

			report2.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, report2.ReportLines.Count);
		}

		public void TestLoadReportLines()
		{
			int transactionCount = 1;
			var firstDayOfMonth = ZDateTime.Today.AddDays(-ZDateTime.Today.Day + 1).Date;

			int handlerInvokeCount = 0;
			int handlerInvokeCountAllLines = 0;

			Creator.CreateTestPeriods(firstDayOfMonth);

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

			Factory.Save();

			Creator.AddTransactionsToComplianceReport(20, Report, ref transactionCount);

			AssertEquals("ReportLines.Count", 0, Report.ReportLines.Count);
			Report.GenerateFromQueue();
			AssertEquals("All saved after generating", false, Report.HasChanges);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 40, Report.ReportLines.Count);

			Creator.AddTransactionsToComplianceReport(50, Report, ref transactionCount);
			Report.GenerateFromQueue();
			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 140, Report.ReportLines.Count);

			Report.ApplyLinesLoadingLimit = true;
			AssertEquals("ReportLines.Count", 100, Report.ReportLines.Count);

			Report.ConfirmLinesLoadingLimit += ComplianceReport_ConfirmLoadingAllLines;
			AssertEquals("ReportLines.Count", 140, Report.ReportLines.Count);
			AssertEquals(1, handlerInvokeCountAllLines);
			AssertEquals("ApplyLinesLoadingLimit must be FALSE", Report.ApplyLinesLoadingLimit, false);

			AssertNotNull("Get GLMovementDetails", Report.GLMovementDetails);
			AssertNotNull("Get GLMovementDetailsView", Report.GLMovementDetailsView);
			AssertNotNull("Get GLClosingBalanceDetails", Report.GLClosingBalanceDetails);
			AssertNotNull("Get GLClosingBalanceDetailsView", Report.GLClosingBalanceDetailsView);
			AssertNotNull("Get ReportLinesCurrentPeriod", Report.ReportLinesCurrentPeriod);
			AssertNotNull("Get ReportLinesPreviousPeriod", Report.ReportLinesPreviousPeriod);
			AssertEquals(1, handlerInvokeCountAllLines);

			Report.ConfirmLinesLoadingLimit -= ComplianceReport_ConfirmLoadingAllLines;

			Report.ApplyLinesLoadingLimit = true;
			Report.ConfirmLinesLoadingLimit += ComplianceReport_ConfirmLinesLoadingLimit;
			AssertEquals("ReportLines.Count", 100, Report.ReportLines.Count);
			AssertEquals(1, handlerInvokeCount);

			AssertNotNull("Get GLMovementDetails", Report.GLMovementDetails);
			AssertEquals(3, handlerInvokeCount);
			AssertNotNull("Get GLMovementDetailsView", Report.GLMovementDetailsView);
			AssertEquals(4, handlerInvokeCount);
			AssertNotNull("Get GLClosingBalanceDetails", Report.GLClosingBalanceDetails);
			AssertEquals(6, handlerInvokeCount);
			AssertNotNull("Get GLClosingBalanceDetailsView", Report.GLClosingBalanceDetailsView);
			AssertEquals(7, handlerInvokeCount);
			AssertNotNull("Get ReportLinesCurrentPeriod", Report.ReportLinesCurrentPeriod);
			AssertEquals(8, handlerInvokeCount);
			AssertNotNull("Get ReportLinesPreviousPeriod", Report.ReportLinesPreviousPeriod);
			AssertEquals(9, handlerInvokeCount);

			void ComplianceReport_ConfirmLinesLoadingLimit(object sender, AccComplianceReport.BoolResponseEventArgs e)
			{
				handlerInvokeCount++;
				e.Response = true;
			}

			void ComplianceReport_ConfirmLoadingAllLines(object sender, AccComplianceReport.BoolResponseEventArgs e)
			{
				handlerInvokeCountAllLines++;
				e.Response = false;
			}
		}

		public void TestCheckExeedsMaxReportLinesToLoadReturnFalseForNewReport()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
			Report.ACR_DateFrom = Report.ACR_DateTo = ZDate.Empty; // Simulate we created this report with setting its Type but Dates are not set

			Assert("A new report", !Report.IsInDatabase);
			Assert("Indirectly checking the report's Configuration is available", Report.SupportsDayBook);

			AssertNoExceptionThrown(() => Report.CheckExeedsMaxReportLinesToLoad());
			AssertEquals("New report should not exceed MaxReportLines", false, Report.CheckExeedsMaxReportLinesToLoad());
		}

		public void TestGenerateFromQueue()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");

			var invoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);

			AssertEquals("ReportLines.Count", 0, Report.ReportLines.Count);
			Report.GenerateFromQueue();
			AssertEquals("All saved after generating", false, Report.HasChanges);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			var reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -200m));

			// This test calls ComplianceReportQueuingServiceTask.QueueForCompany() which starts the usage collector.
			// Test usage data written to DATABASE.
			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Expected one usage record for report status changes: ADD/QUE -> GEN", 1, ediMessages.Length);
		}

		public void TestGenerateFromQueue_TransactionPayments()
		{
			var registryValue = new MaximumAllowedTransactionAmount()
			{
				MaximumAllowedHeaderAmount = 1200000000000000M,
				MaximumAllowedLineAmount = 1200000000000000M
			};

			using (AccountingMasterFilesRegistry.Instance.SystemDefinedMaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			using (AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				Report.FillWithValidTestData();
				Creator.CreateConfigurationForComplianceReport(Report, "AH", ReportLineGroupingListCodes.TransactionPayments, reportLineOrdering: ReportLineGroupingListCodes.Organisation,
					roundingType: ReportAmountsRoundingTypeListCodes.Rounding, rounding: 2);

				var invoice1 = Creator.CreateAPInvoice<APInvoice>("ABI0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
				Assert("Has Lines", invoice1.Lines.Count > 0);
				var line11 = invoice1.Lines[0];
				line11.AL_AT = Creator.GST1.PK;
				var line12 = Creator.CreateInvoiceLine(invoice1, Creator.AUD, 1m, 200m);
				line12.AL_AT = Creator.GSTFREE1.PK;
				var matchLink = Creator.CreateMatchLinkToPayAPInvoice(invoice1, invoice1.AH_PostDate, -155m);
				Creator.CreateLineMatchLink(matchLink, line11, -55m);
				Creator.CreateLineMatchLink(matchLink, line12, -100m);

				var invoice2 = Creator.CreateAPInvoice<APInvoice>("SHI0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.AALSHI);
				Assert("Has Lines", invoice2.Lines.Count > 0);
				var line21 = invoice2.Lines[0];
				line21.AL_AT = Creator.GST1.PK;
				Creator.CreateAndMatchAPPaymentForAPInvoice(invoice2, invoice2.AH_PostDate);

				var invoice3 = Creator.CreateAPInvoice<APInvoice>("SHI0002", Creator.AUD, 1m, 838488366986797.27m, 83848836698679.73m, 0m, 838488366986797.27m, 83848836698679.73m, 0m, Creator.AALSHI);
				invoice3.AH_PostDate = invoice2.AH_PostDate.AddDays(-1);
				Creator.CreateAndMatchAPPaymentForAPInvoice(invoice3, invoice3.AH_PostDate, -(838488366986797.27m - 1m)); // Baaber's test

				Factory.Save();

				Creator.CreateComplianceReportQueueEntry(Report, invoice1, invoice2, invoice3);

				AssertEquals("ReportLines.Count", 0, Report.ReportLines.Count);
				Report.GenerateFromQueue();
				AssertEquals("All saved after generating", false, Report.HasChanges);

				var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
				AssertEquals("Result should have rows", 3, result.Rows.Count);

				Assert("ACL_GC_Company", result.Rows.Cast<DataRow>().All(x => Report.ACR_GC_Company.ToGuid() == (Guid)x["ACL_GC_Company"]));
				Assert("ACL_ACR_Report", result.Rows.Cast<DataRow>().All(x => Report.PK.ToGuid() == (Guid)x["ACL_ACR_Report"]));
				Assert("ACL_ParentTableCode", result.Rows.Cast<DataRow>().All(x => "AH" == (string)x["ACL_ParentTableCode"]));
				// Pivots shoud be ordered by Org Name and Queue record Date
				AssertEquals("Pivot 0", invoice3.PK.ToGuid(), (Guid)result.Rows[0]["ACL_ParentID"]);
				AssertEquals("Pivot 1", invoice2.PK.ToGuid(), (Guid)result.Rows[1]["ACL_ParentID"]);
				AssertEquals("Pivot 2", invoice1.PK.ToGuid(), (Guid)result.Rows[2]["ACL_ParentID"]);

				Report.ClearReportLines_ForTestOnly();
				AssertEquals("ReportLines.Count", 3, Report.ReportLines.Count);

				var reportLines = Report.ReportLines.Cast<AccComplianceReportLine>();
				Assert("AH_InvoiceAmount is populated", reportLines.All(x => !x.AH_InvoiceAmount.IsEmpty));
				Assert("AH_GSTAmount is populated", reportLines.All(x => !x.AH_GSTAmount.IsEmpty));

				AssertEquals("invoice3.AH_InvoiceAmount", -838488366986797.27m, Report.ReportLines[0].AH_InvoiceAmount);
				AssertEquals("invoice3.AH_GSTAmount", -83848836698679.73m, Report.ReportLines[0].AH_GSTAmount);

				AssertEquals("invoice2.AH_InvoiceAmount", -100m, Report.ReportLines[1].AH_InvoiceAmount);
				AssertEquals("invoice2.AH_GSTAmount", -10m, Report.ReportLines[1].AH_GSTAmount);

				AssertEquals("invoice1.AH_InvoiceAmount", -300m, Report.ReportLines[2].AH_InvoiceAmount);
				AssertEquals("invoice1.AH_GSTAmount", -10m, Report.ReportLines[2].AH_GSTAmount);

				AssertEquals("Partially paid invoice3.TotalExTaxAmount", -762262151806171.71m, Report.ReportLines[0].TotalExTaxAmount);
				AssertEquals("Partislly paid invoice3.TotalTaxAmount", -76226215180617.85m, Report.ReportLines[0].TotalTaxAmount);

				AssertEquals("Fully paid invoice2.TotalExTaxAmount", -100m, Report.ReportLines[1].TotalExTaxAmount);
				AssertEquals("Fully paid invoice2.TotalTaxAmount", -10m, Report.ReportLines[1].TotalTaxAmount);

				AssertEquals("Partially paid by lines invoice1.TotalExTaxAmount", -150m, Report.ReportLines[2].TotalExTaxAmount);
				AssertEquals("Partially paid by lines invoice1.TotalTaxAmount", -5m, Report.ReportLines[2].TotalTaxAmount);

				Assert("Goods Amounts are not populated", reportLines.All(x => x.GoodsExTaxAmount.IsEmpty && x.GoodsTaxAmount.IsEmpty));
				Assert("Service Amounts are not populated", reportLines.All(x => x.ServiceExTaxAmount.IsEmpty && x.ServiceTaxAmount.IsEmpty));
			}
		}

		public void TestReportLinesPreviousAndCurrentPeriodCollections()
		{
			AssertReportLinesPreviousAndCurrentPeriodCollections(false);
		}

		public void TestReportLinesPreviousAndCurrentPeriodCollectionsWhenIncludeQueuedForPreviousPeriod()
		{
			AssertReportLinesPreviousAndCurrentPeriodCollections(true);
		}

		void AssertReportLinesPreviousAndCurrentPeriodCollections(bool includeQueuedForPreviousPeriod)
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", includeQueuedForPreviousPeriod: includeQueuedForPreviousPeriod);

			var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.ACR_ReportType = Report.ACR_ReportType;
			previousReport.ACR_DateFrom = Report.ACR_DateFrom.AddDays(-10);
			previousReport.ACR_DateTo = Report.ACR_DateFrom.AddDays(-1);
			previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Factory.Save();

			var previousPeriodInvoice = Creator.CreateAPInvoice<APInvoice>("75" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			previousPeriodInvoice.AH_PostDate = Report.ACR_DateFrom.AddDays(-1);
			Assert("Has Lines", previousPeriodInvoice.Lines.Count > 0);
			var line01 = previousPeriodInvoice.Lines[0];
			line01.AL_AT = Creator.GST1.PK;
			var line02 = Creator.CreateInvoiceLine(previousPeriodInvoice, Creator.AUD, 1m, 200m);
			line02.AL_AT = Creator.GSTFREE1.PK;

			var invoice = Creator.CreateAPInvoice<APInvoice>("125" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST2.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 250m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line01, line02);
			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			Assert(!Report.IsFirstReport());
			AssertEquals("Report include lines from previous period", includeQueuedForPreviousPeriod, Report.IncludeQueuedForPreviousPeriod);
			AssertEquals(includeQueuedForPreviousPeriod ? 4 : 2, Report.ReportLines.Count);

			AssertEquals(includeQueuedForPreviousPeriod ? 2 : 0, Report.ReportLinesPreviousPeriod.Count);
			Assert(Report.ReportLinesPreviousPeriod.Cast<AccComplianceReportLine>().All(x => x.AH_PK == previousPeriodInvoice.PK));
			AssertEquals(2, Report.ReportLinesCurrentPeriod.Count);
			Assert(Report.ReportLinesCurrentPeriod.Cast<AccComplianceReportLine>().All(x => x.AH_PK == invoice.PK));

			AssertEquals(1, Report.ReportTotalsCurrentPeriod.Count);
			AssertEquals(includeQueuedForPreviousPeriod ? 1 : 0, Report.ReportTotalsPreviousPeriod.Count);

			var reportTotalsCurrentPeriodComment = Report.HasPreviousPeriodData ? "Report Totals Current Period" : "Report Totals";
			AssertReportTotals(Report, -350m, -20m, -20m, 0m, 0, reportTotalsCurrentPeriodComment);

			if (includeQueuedForPreviousPeriod)
			{
				AssertReportTotals(Report, -300m, -10m, -10m, 0m, 0m, "Report Totals Previous Period", true);
				AssertNotEquals("ReportTotalsCurrentPeriod should use own collection", Report.ReportTotals, Report.ReportTotalsCurrentPeriod);

				AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 2 }, Report.ReportLinesPreviousPeriod.Cast<AccComplianceReportLine>().Select(x => x.ACL_ReportSequence));
				AssertContainsExactElementsInAnyOrder(new ZInt[] { 3, 4 }, Report.ReportLinesCurrentPeriod.Cast<AccComplianceReportLine>().Select(x => x.ACL_ReportSequence));
			}
			else
			{
				AssertEquals("Should use ReportTotals for the ReportTotalsCurrentPeriod", Report.ReportTotals, Report.ReportTotalsCurrentPeriod);
				AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 2 }, Report.ReportLinesCurrentPeriod.Cast<AccComplianceReportLine>().Select(x => x.ACL_ReportSequence));
			}
		}

		public void TestGenerateFromQueue_IncludeQueuedForPreviousPeriod()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", includeQueuedForPreviousPeriod: true);

			var previousPeriodInvoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			previousPeriodInvoice.AH_PostDate = Report.ACR_DateFrom.AddDays(-1);
			Assert("Has Lines", previousPeriodInvoice.Lines.Count > 0);
			var line01 = previousPeriodInvoice.Lines[0];
			line01.AL_AT = Creator.GST1.PK;
			var line02 = Creator.CreateInvoiceLine(previousPeriodInvoice, Creator.AUD, 1m, 200m);
			line02.AL_AT = Creator.GSTFREE1.PK;

			var invoice = Creator.CreateAPInvoice<APInvoice>("I001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Invoice Post Date is in the report date range", invoice.AH_PostDate >= Report.ACR_DateFrom && invoice.AH_PostDate < Report.ACR_DateTo.AddDays(1));
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST2.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 250m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line01, line02);
			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);

			Assert("Include from previous period", Report.IncludeQueuedForPreviousPeriod);
			Assert("Is first report", Report.IsFirstReport());
			Assert("Do not include from previous period for generating", !Report.IncludeQueuedForPreviousPeriodForGenerating);
			AssertEquals("ReportLines.Count", 0, Report.ReportLines.Count);
			Report.GenerateFromQueue();
			AssertEquals("All saved after generating", false, Report.HasChanges);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			var reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST2.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -250m));

			var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.ACR_ReportType = Report.ACR_ReportType;
			previousReport.ACR_DateFrom = Report.ACR_DateFrom.AddDays(-10);
			previousReport.ACR_DateTo = Report.ACR_DateFrom.AddDays(-1);
			previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Factory.Save();

			Assert("Include from previous period", previousReport.IncludeQueuedForPreviousPeriod);
			Assert("Is first report", previousReport.IsFirstReport());
			Assert("Include from previous period", Report.IncludeQueuedForPreviousPeriod);
			Assert("Is not first report anymore", !Report.IsFirstReport());
			Assert("Include from previous period for generating", Report.IncludeQueuedForPreviousPeriodForGenerating);
			Report.GenerateFromQueue();
			AssertEquals("All saved after generating", false, Report.HasChanges);

			Report.ClearReportLines_ForTestOnly();
			reportLines = Report.ReportLines;
			AssertEquals("Lines count", 4, reportLines.Count);
			Assert("Transaction Nums", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == previousPeriodInvoice.AH_TransactionNum || x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line01", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line02", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -200m));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST2.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -250m));
		}

		public void TestFinalise()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");

			var invoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Report.GenerateFromQueue();

			var queueEntries = new DynamicBusinessObjectCollection(Factory);
			queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
			AssertEquals(2, queueEntries.Count);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			var reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -200m));

			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, false, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			AssertEquals("ReadOnly", false, Report.ReadOnly);
			AssertEquals("CanDelete", true, Report.CanDelete);

			Report.Finalise();
			AssertEquals("All saved after finalizing", false, Report.HasChanges);
			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, true, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);
			AssertEquals("ReadOnly", true, Report.ReadOnly);

			queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Queue is cleared", 0, queueEntries.Count);
			AssertEquals("CanDelete", false, Report.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "Finalized Compliance Report cannot be deleted.", Report.ReasonForNotAbleToDelete);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -200m));

			// This test calls ComplianceReportQueuingServiceTask.QueueForCompany() which starts the usage collector.
			// Test usage data written to DATABASE.
			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Expected two usage records for report status changes: ADD/QUE -> GEN -> FIN", 2, ediMessages.Length);
		}

		public void TestFinalise_IncludeQueuedForPreviousPeriod()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "", includeQueuedForPreviousPeriod: true);

			var previousPeriodInvoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			previousPeriodInvoice.AH_PostDate = Report.ACR_DateFrom.AddDays(-1);
			Assert("Has Lines", previousPeriodInvoice.Lines.Count > 0);
			var line01 = previousPeriodInvoice.Lines[0];
			line01.AL_AT = Creator.GST1.PK;
			var line02 = Creator.CreateInvoiceLine(previousPeriodInvoice, Creator.AUD, 1m, 200m);
			line02.AL_AT = Creator.GSTFREE1.PK;

			var invoice = Creator.CreateAPInvoice<APInvoice>("I001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST2.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 250m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line01, line02);
			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Assert("Include from previous period", Report.IncludeQueuedForPreviousPeriod);
			Assert("Include from previous period for deleting", Report.IncludeQueuedForPreviousPeriodForDeleting);
			Assert("Is first report", Report.IsFirstReport());
			Report.GenerateFromQueue();

			var queueEntries = new DynamicBusinessObjectCollection(Factory);
			queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
			AssertEquals(4, queueEntries.Count);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			var reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST2.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -250m));

			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, false, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportGenerated, Report.ACR_Status);
			AssertEquals("ReadOnly", false, Report.ReadOnly);
			AssertEquals("CanDelete", true, Report.CanDelete);

			Report.Finalise();
			AssertEquals("All saved after finalizing", false, Report.HasChanges);
			AssertEquals(AccComplianceReport.Schema.ACR_IsFinalised, true, Report.ACR_IsFinalised);
			AssertEquals(AccComplianceReport.Schema.ACR_Status, AccComplianceReport.Status.ReportFinalised, Report.ACR_Status);
			AssertEquals("ReadOnly", true, Report.ReadOnly);

			queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Queue is cleared", 0, queueEntries.Count);
			AssertEquals("CanDelete", false, Report.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "Finalized Compliance Report cannot be deleted.", Report.ReasonForNotAbleToDelete);

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			reportLines = Report.ReportLines;
			Assert("Transaction Num", reportLines.Cast<AccComplianceReportLine>().All(x => x.AH_TransactionNum == invoice.AH_TransactionNum));
			AssertNotNull("Line1", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST2.AT_Code && x.ServiceExTaxAmount == -100m));
			AssertNotNull("Line2", reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -250m));
		}

		[ExpectNoExceptions]
		public void TestDelete()
		{
			AssertTestDelete();
		}

		public void TestDeleteWithSummary()
		{
			AssertTestDelete(true);
		}

		void AssertTestDelete(bool addSummary = false)
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");

			var invoice = Creator.CreateAPInvoice<APInvoice>("I000" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = Creator.GST1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.AUD, 1m, 200m);
			line2.AL_AT = Creator.GSTFREE1.PK;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, line1, line2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);

			AssertEquals("ReadOnly", false, Report.ReadOnly);
			AssertEquals("CanDelete", true, Report.CanDelete);

			if (addSummary)
			{
				Report.ACR_ReportType = AccComplianceReport.ReportTypes.LiquidazioneIVA;

				Creator.CreateAccTaxReturn(Report, true);
				Factory.Save();
			}

			Report.Delete();
			Assert(Report.IsDeleted);
			Factory.Save();
		}

		public void TestReportLinesAndTotals_FromLinesNoGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 6, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var line1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m);
			AssertNotNull("Invoice1.Line1", line1);

			var line2 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -120m);
			AssertNotNull("Invoice1.Line2", line2);

			var line3 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.REV.AT_Code && x.ServiceExTaxAmount == -80m);
			AssertNotNull("Invoice1.Line3", line3);

			var invoice1Sequence = new[] { line1.ACL_ReportSequence, line2.ACL_ReportSequence, line3.ACL_ReportSequence };
			Assert("All Invoice1 ACL_ReportSequence are between 1 and 3", invoice1Sequence.All(x => x >= 1 && x <= 3));
			AssertEquals("All Invoice1 ACL_ReportSequence are unique", 3, invoice1Sequence.Distinct().Count());

			var line4 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -50m);
			AssertNotNull("Invoice2.Line1", line4);

			var line5 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -130m);
			AssertNotNull("Invoice2.Line2", line5);

			var line6 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.REV.AT_Code && x.ServiceExTaxAmount == -20m);
			AssertNotNull("Invoice2.Line3", line6);

			var invoice2Sequence = new[] { line4.ACL_ReportSequence, line5.ACL_ReportSequence, line6.ACL_ReportSequence };
			Assert("All Invoice2 ACL_ReportSequence are between 1 and 3", invoice1Sequence.All(x => x >= 1 && x <= 3));
			AssertEquals("All Invoice2 ACL_ReportSequence are unique", 3, invoice1Sequence.Distinct().Count());

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromHeadersNoGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AH", "");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, APInvoice1, APInvoice2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 6, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var line1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -100m);
			AssertNotNull("Invoice1.Line1", line1);
			AssertEquals("line1.ACL_ReportSequence", 1, line1.ACL_ReportSequence);

			var line2 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -120m);
			AssertNotNull("Invoice1.Line2", line2);
			AssertEquals("line2.ACL_ReportSequence", 1, line2.ACL_ReportSequence);

			var line3 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.REV.AT_Code && x.ServiceExTaxAmount == -80m);
			AssertNotNull("Invoice1.Line3", line3);
			AssertEquals("line3.ACL_ReportSequence", 1, line3.ACL_ReportSequence);

			var line4 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GST1.AT_Code && x.ServiceExTaxAmount == -50m);
			AssertNotNull("Invoice2.Line1", line4);
			AssertEquals("line4.ACL_ReportSequence", 2, line4.ACL_ReportSequence);

			var line5 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.GSTFREE1.AT_Code && x.ServiceExTaxAmount == -130m);
			AssertNotNull("Invoice2.Line2", line5);
			AssertEquals("line5.ACL_ReportSequence", 2, line5.ACL_ReportSequence);

			var line6 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AT_Code == Creator.REV.AT_Code && x.ServiceExTaxAmount == -20m);
			AssertNotNull("Invoice2.Line3", line6);
			AssertEquals("line6.ACL_ReportSequence", 2, line6.ACL_ReportSequence);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromLinesHeaderGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "HDR");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.ServiceExTaxAmount == -300m);
			AssertNotNull("Invoice1", invoice1);
			AssertEquals("Invoice1.ACL_ReportSequence", 1, invoice1.ACL_ReportSequence);

			var invoice2 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.ServiceExTaxAmount == -200m);
			AssertNotNull("Invoice2", invoice2);
			AssertEquals("Invoice2.ACL_ReportSequence", 2, invoice2.ACL_ReportSequence);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromLinesHeaderWithLinesGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "HDL");

			CreateAndSaveTwoAPInvoices();
			foreach (var line in InvoiceLines)
			{
				Creator.CreateComplianceReportQueueEntry(Report, line.AL_Sequence.ToString("D5"), null, line);
			}
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", InvoiceLines.Count, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum);
			AssertEquals("invoice1Lines.Count", 3, invoice1Lines.Count());
			Assert("Invoice1.ACL_ReportSequence is 1", invoice1Lines.All(x => x.ACL_ReportSequence == 1));
			Assert("invoice1Lines have Amount 100", invoice1Lines.Any(x => x.ServiceExTaxAmount == -100m));
			Assert("invoice1Lines have Amount 120", invoice1Lines.Any(x => x.ServiceExTaxAmount == -120m));
			Assert("invoice1Lines have Amount 80", invoice1Lines.Any(x => x.ServiceExTaxAmount == -80m));

			var invoice2Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum);
			AssertEquals("invoice2Lines.Count", 3, invoice2Lines.Count());
			Assert("Invoice2.ACL_ReportSequence is 2", invoice2Lines.All(x => x.ACL_ReportSequence == 2));
			Assert("invoice2Lines have Amount 50", invoice2Lines.Any(x => x.ServiceExTaxAmount == -50m));
			Assert("invoice2Lines have Amount 130", invoice2Lines.Any(x => x.ServiceExTaxAmount == -130m));
			Assert("invoice2Lines have Amount 20", invoice2Lines.Any(x => x.ServiceExTaxAmount == -20m));

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromHeadersHeaderGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AH", "HDR");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, APInvoice1, APInvoice2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.ServiceExTaxAmount == -300m);
			AssertNotNull("Invoice1", invoice1);
			AssertEquals("Invoice1.ACL_ReportSequence", 1, invoice1.ACL_ReportSequence);

			var invoice2 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.ServiceExTaxAmount == -200m);
			AssertNotNull("Invoice2", invoice2);
			AssertEquals("Invoice2.ACL_ReportSequence", 2, invoice2.ACL_ReportSequence);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromLinesOrgGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "ORG");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var abigas = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.OH_Code == Creator.ABIGAS.OH_Code);
			AssertNotNull("ABIGAS", abigas);
			AssertEquals("ABIGAS.ACL_ReportSequence", 1, abigas.ACL_ReportSequence);
			AssertEquals("ABIGAS.ServiceExTaxAmount", -300m, abigas.ServiceExTaxAmount);

			var creditor1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.OH_Code == Creator.Creditor1.OH_Code);
			AssertNotNull("Creditor1", creditor1);
			AssertEquals("Creditor1.ACL_ReportSequence", 2, creditor1.ACL_ReportSequence);
			AssertEquals("Creditor1.ServiceExTaxAmount", -200m, creditor1.ServiceExTaxAmount);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromHeadersOrgGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AH", "ORG");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, APInvoice1, APInvoice2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var abigas = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.OH_Code == Creator.ABIGAS.OH_Code);
			AssertNotNull("ABIGAS", abigas);
			AssertEquals("ABIGAS.ACL_ReportSequence", 1, abigas.ACL_ReportSequence);
			AssertEquals("ABIGAS.ServiceExTaxAmount", -300m, abigas.ServiceExTaxAmount);

			var creditor1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.OH_Code == Creator.Creditor1.OH_Code);
			AssertNotNull("Creditor1", creditor1);
			AssertEquals("Creditor1.ACL_ReportSequence", 2, creditor1.ACL_ReportSequence);
			AssertEquals("Creditor1.ServiceExTaxAmount", -200m, creditor1.ServiceExTaxAmount);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromLinesTaxGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");

			CreateAPInvoice1();
			var line1_4 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1m, 45m);
			line1_4.AL_AT = Creator.GSTFREE1.PK;
			var line1_5 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1m, 20m);
			line1_5.AL_AT = Creator.GSTFREE1.PK;
			line1_5.AL_AG = Creator.GLHeader1.PK;

			var line1_6 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1M, 35M);
			line1_6.AL_AT = Creator.RVS1.PK;
			CreateAPInvoice2();
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, line1_4);
			Creator.CreateComplianceReportQueueEntry(Report, line1_5);
			Creator.CreateComplianceReportQueueEntry(Report, line1_6);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 7, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1GST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.GST1.AT_Code);
			AssertNotNull("Invoice1.GST", invoice1GST);

			var invoice1GSTFREE = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.GSTFREE1.AT_Code);
			AssertNotNull("Invoice1.GSTFREE", invoice1GSTFREE);
			AssertEquals("Two GSTFREE lines combined", -185m, invoice1GSTFREE.ServiceExTaxAmount);

			var invoice1REV = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.REV.AT_Code);
			AssertNotNull("Invoice1.REV", invoice1REV);

			var invoice1Sequence = new[] { invoice1GST.ACL_ReportSequence, invoice1GSTFREE.ACL_ReportSequence, invoice1REV.ACL_ReportSequence };
			Assert("All Invoice1 ACL_ReportSequence are between 1 and 4", invoice1Sequence.All(x => x >= 1 && x <= 4));
			AssertEquals("All Invoice1 ACL_ReportSequence are unique", 3, invoice1Sequence.Distinct().Count());

			var invoice2GST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.AT_Code == Creator.GST1.AT_Code);
			AssertNotNull("Invoice2.GST", invoice2GST);

			var invoice2GSTFREE = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.AT_Code == Creator.GSTFREE1.AT_Code);
			AssertNotNull("Invoice2.GSTFREE", invoice2GSTFREE);

			var invoice2REV = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.AT_Code == Creator.REV.AT_Code);
			AssertNotNull("Invoice2.REV", invoice2REV);

			var invoice2Sequence = new[] { invoice2GST.ACL_ReportSequence, invoice2GSTFREE.ACL_ReportSequence, invoice2REV.ACL_ReportSequence };
			Assert("All Invoice2 ACL_ReportSequence are between 5 and 7", invoice2Sequence.All(x => x >= 5 && x <= 7));
			AssertEquals("All Invoice2 ACL_ReportSequence are unique", 3, invoice2Sequence.Distinct().Count());

			var invoiceRVS = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.RVS1.AT_Code);
			AssertNotNull("Invoice1.RVS", invoiceRVS);
			AssertEquals(-6.3M, invoiceRVS.TaxReverseChargeAmount);

			AssertReportTotals(Report, -600m, -15m, -10.5m, -4.5m, -16.3M);
		}

		public void TestReportLinesAndTotals_FromHeadersTaxGrouping()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "AH", "TXR");

			CreateAPInvoice1();
			var line1_3 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1m, 80m);
			line1_3.AL_AT = Creator.GSTFREE1.PK;
			CreateAPInvoice2();
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(Report, APInvoice1, APInvoice2);
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 6, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1GST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.GST1.AT_Code);
			AssertNotNull("Invoice1.GST", invoice1GST);
			AssertEquals("Invoice1.GST.ACL_ReportSequence", 1, invoice1GST.ACL_ReportSequence);

			var invoice1GSTFREE = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.AT_Code == Creator.GSTFREE1.AT_Code);
			AssertNotNull("Invoice1.GSTFREE", invoice1GSTFREE);
			AssertEquals("Invoice1.GSTFREE.ACL_ReportSequence", 1, invoice1GSTFREE.ACL_ReportSequence);
			AssertEquals("Two GSTFREE lines combined", -200m, invoice1GSTFREE.ServiceExTaxAmount);

			var invoice2GST = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.AT_Code == Creator.GST1.AT_Code);
			AssertNotNull("Invoice2.GST", invoice2GST);
			AssertEquals("Invoice2.GST.ACL_ReportSequence", 2, invoice2GST.ACL_ReportSequence);

			var invoice2GSTFREE = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.AT_Code == Creator.GSTFREE1.AT_Code);
			AssertNotNull("Invoice2.GSTFREE", invoice2GSTFREE);
			AssertEquals("Invoice2.GSTFREE.ACL_ReportSequence", 2, invoice2GSTFREE.ACL_ReportSequence);

			AssertReportTotals(Report, -580m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportLinesAndTotals_FromAllTransactionsDayBookGrouping()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside FInancial Year

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DAB");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APSusp*-", null, InvoiceLines.ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTIn*-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**GSTNotRec-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APCtrl*Total", null, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 8, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum).ToArray();
			AssertEquals("Invoice1 Lines", 4, invoice1Lines.Length);
			AssertEquals("All Invoice1 lines have ACL_ReportSequence 1", invoice1Lines.Length, invoice1Lines.Where(x => x.ACL_ReportSequence == 1).Count());

			var invoice2Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum).ToArray();
			AssertEquals("Invoice2 Lines", 4, invoice2Lines.Length);
			AssertEquals("All Invoice2 lines have ACL_ReportSequence 2", invoice1Lines.Length, invoice2Lines.Where(x => x.ACL_ReportSequence == 2).Count());

			AssertReportTotalsWithGLBalance(Report, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 515m, 515m, 2);

			var newReport = Factory.NewWithValidTestData<AccComplianceReport>();
			newReport.ACR_ReportType = Report.ACR_ReportType;
			newReport.ACR_DateTo = newReport.ACR_DateFrom = Report.ACR_DateTo.AddDays(1); // New report should be after the previous one
			Factory.Save();

			// Reuse Invoice 1 for the new Report
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*APSusp*-", newReport.ACR_DateFrom, APInvoice1.Lines.ToArray<AccTransactionLines>());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*GSTIn*-", newReport.ACR_DateFrom, APInvoice1.Lines.Cast<InvoicingLineBase>().Where(l => !l.AL_GSTVAT.IsEmpty).ToArray<AccTransactionLines>());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV**GSTNotRec-", newReport.ACR_DateFrom, APInvoice1.Lines.Cast<InvoicingLineBase>().Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*APCtrl*Total", newReport.ACR_DateFrom, APInvoice1.Lines.ToArray<AccTransactionLines>());
			newReport.GenerateFromQueue();

			newReport.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 4, newReport.ReportLines.Count);
			reportLines = newReport.ReportLines;

			invoice1Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum).ToArray();
			AssertEquals("Invoice1 Lines", 4, invoice1Lines.Length);
			AssertEquals("All Invoice1 lines have ACL_ReportSequence 3 continuing the previous Report", invoice1Lines.Length, invoice1Lines.Where(x => x.ACL_ReportSequence == 3).Count());

			AssertReportTotalsWithGLBalance(newReport, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 310m, 310m, 1, 515m, 515m);
		}

		[TestDate(2018, 01, 01)]
		public void TestReportLines_OpeningAndClosingJournalsForDayBookGrouping()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside FInancial Year

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DAB");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APSusp*-", null, InvoiceLines.ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTIn*-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**GSTNotRec-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APCtrl*Total", null, InvoiceLines.ToArray());

			CreateAndSaveOpeningAndClosingGLJournal();
			Creator.CreateComplianceReportQueueEntry(Report, "*GL*GJL**", null, GLJournalLines.ToArray());

			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 12, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var openingJournalLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == OpeningJournal.AH_TransactionNum).ToArray();
			AssertEquals("OpeningJournal Lines", 2, openingJournalLines.Length);
			var minSequenceNum = reportLines.Cast<AccComplianceReportLine>().Min(x => x.ACL_ReportSequence);
			AssertEquals("Expect minimum sequence number is 1", 1, minSequenceNum);
			AssertEquals("Expect all opening journal lines have ACL_ReportSequence 1", openingJournalLines.Length, openingJournalLines.Where(x => x.ACL_ReportSequence == 1).Count());

			var closingJournalLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == ClosingJournal.AH_TransactionNum).ToArray();
			AssertEquals("OpeningJournal Lines", 2, closingJournalLines.Length);
			var maxSequenceNum = reportLines.Cast<AccComplianceReportLine>().Max(x => x.ACL_ReportSequence);
			AssertEquals("Expect maximum sequence number is 4", 4, maxSequenceNum);
			AssertEquals("Expect all closing journal lines have ACL_ReportSequence 4", closingJournalLines.Length, closingJournalLines.Where(x => x.ACL_ReportSequence == 4).Count());
		}

		[TestDate(2018, 01, 01)]
		public virtual void TestReportLinesAndTotals_FromAllTransactionsDayBookWithoutGrouping()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year
			Report.FillWithValidTestData();
			Factory.Save();

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(Report.ACR_DateTo, Report.ACR_GC_Company);
			Report.ACR_Periodicity = PeriodicityCodes.AccountingPeriod;
			Report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, Report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, Report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DBW");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);

			CreateAndSaveTwoAPInvoices(currentPeriod.AM_StartDate);
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APSusp*-", null, InvoiceLines.ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTIn*-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**GSTNotRec-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APCtrl*Total", null, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 16, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1ReportLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum).ToArray();
			AssertEquals("Invoice1 Lines", 8, invoice1ReportLines.Length);
			AssertEquals("Every Invoice1 line has own ACL_ReportSequence", APInvoice1.Lines.Count, invoice1ReportLines.Select(x => x.ACL_ReportSequence).Distinct().Count());

			var invoice2ReportLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum).ToArray();
			AssertEquals("Invoice2 Lines", 8, invoice2ReportLines.Length);
			AssertEquals("Every Invoice2 line has own ACL_ReportSequence", APInvoice2.Lines.Count, invoice2ReportLines.Select(x => x.ACL_ReportSequence).Distinct().Count());

			AssertReportTotalsWithGLBalanceDetails(Report, 515m, 515m, 6, 0m, 0m, 515m, 515m);

			AssertGLBalanceCollection(Report.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance", 0m);
			AssertGLBalanceCollection(Report.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance View", 0m, 0);

			AssertGLBalanceCollection(Report.GLMovementDetails.Cast<GeneralLedgerBalanceLine>(), "GL Movements", 515m);
			AssertGLBalanceCollection(Report.GLMovementDetailsView.Cast<GeneralLedgerBalanceLine>(), "GL Movements View", 515m, 4);

			AssertGLBalanceCollection(Report.GLClosingBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance", 515m);
			AssertGLBalanceCollection(Report.GLClosingBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance View", 515m, 4);

			var newReport = (AccComplianceReport)GetNewBusinessObject();
			newReport.ACR_ReportType = Report.ACR_ReportType;
			newReport.ACR_Periodicity = Report.ACR_Periodicity;
			newReport.AccountingPeriod = Report.AccountingPeriod + 1;

			var chargeCodesToFix = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, new string[] { "BOND", "CLAIM" }));
			chargeCodesToFix.ForEach(x => x.AC_AG_CostAccount = Creator.GLHeader1.PK);
			Factory.Save();

			Assert("Aggregation was successful", new BatchAggregator().Aggregate());

			var apInvoice = APInvoice1;
			CreateAPInvoice1(newReport.ACR_DateFrom.AddDays(1));
			APInvoice3 = APInvoice1;
			APInvoice1 = apInvoice;
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*APSusp*-", null, APInvoice3.Lines.ToArray<AccTransactionLines>());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*GSTIn*-", null, APInvoice3.Lines.Cast<InvoicingLineBase>().Where(l => !l.AL_GSTVAT.IsEmpty).ToArray<AccTransactionLines>());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV**GSTNotRec-", null, APInvoice3.Lines.Cast<InvoicingLineBase>().Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(newReport, "*AP*INV*APCtrl*Total", null, APInvoice3.Lines.ToArray<AccTransactionLines>());
			newReport.GenerateFromQueue();

			newReport.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 8, newReport.ReportLines.Count);
			reportLines = newReport.ReportLines;

			invoice1ReportLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice3.AH_TransactionNum).ToArray();
			AssertEquals("Invoice1 Lines", 8, invoice1ReportLines.Length);
			AssertEquals("Every Invoice1 line has own ACL_ReportSequence", APInvoice3.Lines.Count, invoice1ReportLines.Select(x => x.ACL_ReportSequence).Distinct().Count());

			AssertReportTotalsWithGLBalanceDetails(newReport, 310m, 310m, 3, 515m, 515m, 825m, 825m);

			AssertGLBalanceCollection(newReport.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance", 515m);
			AssertGLBalanceCollection(newReport.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance View", 515m, 3);

			AssertGLBalanceCollection(newReport.GLMovementDetails.Cast<GeneralLedgerBalanceLine>(), "GL Movements", 310m);
			AssertGLBalanceCollection(newReport.GLMovementDetailsView.Cast<GeneralLedgerBalanceLine>(), "GL Movements View", 310m, 4);

			AssertGLBalanceCollection(newReport.GLClosingBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance", 825m);
			AssertGLBalanceCollection(newReport.GLClosingBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance View", 825m, 4);

			Assert("Aggregation was successful", new BatchAggregator().Aggregate());

			newReport = Factory.NewWithValidTestData<AccComplianceReport>();
			newReport.ACR_ReportType = Report.ACR_ReportType;
			newReport.ACR_Periodicity = Report.ACR_Periodicity;
			newReport.AccountingPeriod = Report.AccountingPeriod + 2;   // New report should be after the previous one
			Factory.Save();

			CreateAndSaveWIPAccrual(newReport.ACR_DateFrom.AddDays(2));
			// use WIP/Accrual for the new Report
			Creator.CreateComplianceReportQueueEntry(newReport, "*JC*WIP**", newReport.ACR_DateFrom, wip1, wip2);
			Creator.CreateComplianceReportQueueEntry(newReport, "*JC*WIP*WIPCtrl*-", newReport.ACR_DateFrom, wip1, wip2);
			Creator.CreateComplianceReportQueueEntry(newReport, "*JC*ACR**", newReport.ACR_DateFrom, accrual1, accrual2);
			Creator.CreateComplianceReportQueueEntry(newReport, "*JC*ACR*ACRCtrl*-", newReport.ACR_DateFrom, accrual1, accrual2);
			newReport.GenerateFromQueue();

			newReport.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 8, newReport.ReportLines.Count);
			reportLines = newReport.ReportLines;

			invoice1ReportLines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == ZString.Empty).ToArray();
			AssertEquals("WIP/Accrual Lines", 8, invoice1ReportLines.Length);
			AssertEquals("All WIP/Accrual Lines have duplicate ACL_ReportSequence continuing the previous Report", 2, invoice1ReportLines.Select(x => x.ACL_ReportSequence).Distinct().Count());

			AssertReportTotalsWithGLBalanceDetails(newReport, 600m, 600m, 2, 825m, 825m, 1125m, 1125m);

			AssertGLBalanceCollection(newReport.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance", 825m);
			AssertGLBalanceCollection(newReport.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance View", 825m, 3);

			AssertGLBalanceCollection(newReport.GLMovementDetails.Cast<GeneralLedgerBalanceLine>(), "GL Movements", 600m);
			AssertGLBalanceCollection(newReport.GLMovementDetailsView.Cast<GeneralLedgerBalanceLine>(), "GL Movements View", 600m, 3);

			AssertGLBalanceCollection(newReport.GLClosingBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance", 1125m);
			AssertGLBalanceCollection(newReport.GLClosingBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Closing GL Balance View", 1125m, 5);
		}

		protected void AssertGLBalanceCollection(IEnumerable<GeneralLedgerBalanceLine> lines, string collectionName, decimal expectedAmountDRCR, int? expectedCount = null)
		{
			AssertNotNull(collectionName, lines);

			AssertEquals(collectionName + " Debit", expectedAmountDRCR, lines.Cast<GeneralLedgerBalanceLine>().Sum(x => x.GeneralLedgerAmountDR));
			AssertEquals(collectionName + " Credit", expectedAmountDRCR, lines.Cast<GeneralLedgerBalanceLine>().Sum(x => x.GeneralLedgerAmountCR));

			if (expectedCount.HasValue)
			{
				AssertEquals(collectionName + " Count", expectedCount.Value, lines.Count());
			}
		}

		public virtual void TestReportOpeningGLBalanceDetailsDetectDRCRBasedOnBalanceSign_AllTransactionsDayBookWithoutGrouping()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year
			Report.FillWithValidTestData();
			Factory.Save();

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(Report.ACR_DateTo, Report.ACR_GC_Company);
			Report.ACR_Periodicity = PeriodicityCodes.AccountingPeriod;
			Report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, Report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, Report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DBW");
			Factory.Save();

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);

			var debitGLAccount = Creator.GLHeader1;
			debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;
			var creditGLAccount = Creator.GLHeader2;
			creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

			Factory.Save();

			Report.ClearReportLines_ForTestOnly();
			AssertGLBalanceCollection(Report.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance", 0m);
			AssertGLBalanceCollection(Report.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance View", 0m, 0);

			var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

			var openingBalance1 = Factory.New<AccGLAggregate>();
			openingBalance1.AA_AG = debitGLAccount.PK;
			openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance1.AA_Period = previousPeriod;
			openingBalance1.AA_Amount = -123m;   // Set a wrong sign amount for opening balance

			var openingBalance2 = Factory.New<AccGLAggregate>();
			openingBalance2.AA_AG = creditGLAccount.PK;
			openingBalance2.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance2.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance2.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance2.AA_Period = previousPeriod;
			openingBalance2.AA_Amount = 123m;    // Set a wrong sign amount for opening balance

			Factory.Save();

			Report.ClearReportLines_ForTestOnly();
			AssertGLBalanceCollection(Report.GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance", 123m);
			AssertGLBalanceCollection(Report.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>(), "Opening GL Balance View", 123m, 2);

			AssertNotNull("Debit account opening balance should be on Credit side as per amount negative sign",
				Report.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>().FirstOrDefault(x => x.AG_PK == debitGLAccount.PK && x.GeneralLedgerAmountCR == 123m));
			AssertNotNull("Credit account opening balance should be on Debit side as per amount positive sign",
				Report.GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>().FirstOrDefault(x => x.AG_PK == creditGLAccount.PK && x.GeneralLedgerAmountDR == 123m));
		}

		public void TestReportShowsProperTaxRegistrationNumber()
		{
			Report.FillWithValidTestData();
			Report.Company.SetCountry("IT");
			Creator.CreateConfigurationForComplianceReport(Report, "AL", "HDR", "", "COD");

			var cusCodeCOD = Creator.ABIGAS.CustomsCodes.AddNew();
			cusCodeCOD.OK_CodeType = "COD";
			cusCodeCOD.OK_CustomsRegNo = "123456";
			cusCodeCOD.OK_RN_NKCodeCountry = "IT";

			Creator.Creditor1.OH_RL_NKClosestPort = "GRALX";
			var cusCode = Creator.Creditor1.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "AFM"; // Proper code type for GR country from EUTaxBusinessRegistrationCodes SQL function
			cusCode.OK_CustomsRegNo = "987654";
			cusCode.OK_RN_NKCodeCountry = "GR";

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum && x.ServiceExTaxAmount == -300m);
			AssertNotNull("Invoice1", invoice1);
			AssertEquals("Invoice1.ACL_ReportSequence", 1, invoice1.ACL_ReportSequence);
			AssertEquals("Report Country Code", "IT", invoice1.GC_RN_NKCountryCode);
			AssertEquals("Org Code", Creator.ABIGAS.OH_Code, invoice1.OH_Code);
			AssertEquals("Org Country Code", "AU", invoice1.OrgCountryCode);
			AssertEquals("OK_CustomsRegNo", "IT123456", invoice1.OK_CustomsRegNo);

			var invoice2 = reportLines.Cast<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum && x.ServiceExTaxAmount == -200m);
			AssertNotNull("Invoice2", invoice2);
			AssertEquals("Invoice2.ACL_ReportSequence", 2, invoice2.ACL_ReportSequence);
			AssertEquals("Report Country Code", "IT", invoice2.GC_RN_NKCountryCode);
			AssertEquals("Org Code", Creator.Creditor1.OH_Code, invoice2.OH_Code);
			AssertEquals("Org Country Code", "GR", invoice2.OrgCountryCode);
			AssertEquals("OK_CustomsRegNo", "EL987654", invoice2.OK_CustomsRegNo);

			AssertReportTotals(Report, -500m, -15m, -10.5m, -4.5m, -10m);
		}

		public void TestReportTotalsWithPreCalculatedAmount()
		{
			Creator.CreateTestPeriods(ZDateTime.Today);

			Creator.EnsureComplianceReportConfigInRegistry(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, "ABN", PeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable);
			Report.FillWithValidTestData();
			Report.ACR_ReportType = ComplianceReportTypes.PaymentTimesSmallBusinessReportType;

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, new[] { APInvoice1, APInvoice2 }, (x) => x.AH_InvoiceDate.Date, (x) => $"{-x.AH_LocalTotal}|1");
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 2, Report.ReportLines.Count);
			AssertEquals("APInvoice1 PreCalculatedAmount", 310m, getLineForInvoice(APInvoice1)?.PreCalculatedAmount);
			AssertEquals("APInvoice2 PreCalculatedAmount", 205m, getLineForInvoice(APInvoice2)?.PreCalculatedAmount);

			AssertReportTotals(Report, 0m, preCalculatedAmount: 515m);

			AccComplianceReportLine getLineForInvoice(APInvoice invoice) => Report.ReportLines.OfType<AccComplianceReportLine>().FirstOrDefault(x => x.AH_TransactionNum == invoice.AH_TransactionNum);
		}

		public void TestDocumentSupporter()
		{
			AssertNotNull("AccComplianceReportDocumentSupporter", Report.DocumentSupporter as AccComplianceReportDocumentSupporter);
		}

		public void TestIsAutoLogged()
		{
			Report.FillWithValidTestData();
			Factory.Save();
			AssertNotNull("AddedLog", Report.Logs.AddedLog);
		}

		public void TestGetWorkflowInformationProvider()
		{
			AssertNull(((IWorkflowProvider)Report).GetWorkflowInformationProvider());
		}

		public void TestWorkflowItems()
		{
			var provider = Report as IWorkflowProvider;
			AssertNotNull(provider);
			AssertNotNull(provider.WorkflowItems);
			Assert("IsRegisteredEditableChildObject", Report.IsRegisteredEditableChildObject(provider.WorkflowItems));
		}

		public void TestGetTemplateSelectionCriteria()
		{
			AssertNotNull(((IWorkflowProvider)Report).GetTemplateSelectionCriteria());
			Assert("is ColumnValueRanker", ((IWorkflowProvider)Report).GetTemplateSelectionCriteria() is ColumnValueRanker);
		}

		public void TestWorkflowType()
		{
			Assert(Report.GetType().ToString() + " must support Workflow", Report.WorkflowType.Equals(WorkflowDescriptors.AccComplianceReportCode));
		}

		public void TestACR_ReportType_Concurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var report = factory1.NewWithValidTestData<AccComplianceReport>();
			factory1.Save();

			var reloadedReport = factory2.Load<AccComplianceReport>(report.PK);
			reloadedReport.ACR_ReportType = "ABC";
			factory2.Save();

			report.ACR_ReportType = "BCD";

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccComplianceReport
PK: {report.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestACR_DateFrom_Concurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var report = factory1.NewWithValidTestData<AccComplianceReport>();
			factory1.Save();

			var reloadedReport = factory2.Load<AccComplianceReport>(report.PK);
			reloadedReport.ACR_DateFrom = ZDate.Today.AddDays(-1);
			factory2.Save();

			report.ACR_DateFrom = ZDate.Today.AddDays(-3);

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccComplianceReport
PK: {report.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestACR_DateTo_Concurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var report = factory1.NewWithValidTestData<AccComplianceReport>();
			factory1.Save();

			var reloadedReport = factory2.Load<AccComplianceReport>(report.PK);
			reloadedReport.ACR_DateTo = ZDate.Today.AddDays(-1);
			factory2.Save();

			report.ACR_DateTo = ZDate.Today.AddDays(-3);

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccComplianceReport
PK: {report.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestACR_Status_Concurrency()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var report = factory1.NewWithValidTestData<AccComplianceReport>();
			factory1.Save();

			var reloadedReport = factory2.Load<AccComplianceReport>(report.PK);
			reloadedReport.ACR_Status = "FIN";
			factory2.Save();

			report.ACR_Status = "GEN";

			var exception = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			var errorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccComplianceReport
PK: {report.PK}
RowState: Modified
";
			Assert(string.Format(
@"Message 
{0} 
should be part of error message
{1}", errorMessage, exception.Message), exception.Message.Contains(errorMessage));
		}

		public void TestGetComplianceSubTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = PeriodicityCodes.DateRange;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

				var setting1 = reportConfig.Settings.AddNew();
				setting1.ComplianceSubType = "TCR";
				setting1.LedgerType = LedgerTypes.AccountsPayable;

				var setting2 = reportConfig.Settings.AddNew();
				setting2.ComplianceSubType = "TDP";
				setting2.LedgerType = LedgerTypes.AccountsPayable;

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report.ACR_ReportType = "TST";
				var subTypes = Report.GetComplianceSubTypes();
				AssertEquals(2, subTypes.Length);
				AssertCollectionContains("TCR", subTypes);
				AssertCollectionContains("TDP", subTypes);
			}
		}

		public void TestNextProcessingStepFromDate()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AssertEquals("Default value of NextProcessingStepFromDate", complianceReport.ACR_DateFrom, complianceReport.NextProcessingStepFromDate);

			var stepFromDate = new ZDate(2022, 7, 5);
			complianceReport.NextProcessingStepFromDate = stepFromDate;
			Factory.Save();
			var reportPK = complianceReport.PK;

			var factory = new BusinessObjectFactory();
			complianceReport = factory.LoadTop1<AccComplianceReport>(new ZQuery(AccComplianceReportSchema.PK, reportPK));
			AssertEquals("Value of NextProcessingStepFromDate", stepFromDate, complianceReport.NextProcessingStepFromDate);
		}

		public void TestSetNextProcessingStepFromDate_ToDefault_DoesNotAddOnColumn()
		{
			//Arrange: SetNextProcessingStepFromDate = 0
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.NextProcessingStepFromDate = ZDate.Empty;

			//Act //Assert
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, AccComplianceReport.Schema.NextProcessingStepFromDate);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, complianceReport.PK);
			var addOnColumn = Factory.LoadTop1<MasterFiles.Business.CustomValues.GenAddOnColumn>(query);

			AssertEquals("Precondition: addOnColumn has not been created", null, addOnColumn);
			AssertEquals(complianceReport.ACR_DateFrom, complianceReport.NextProcessingStepFromDate);
		}

		public void TestReportPeriodicityMonthlyQuarterlyYearly()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = ZDate.Empty;
			complianceReport.ACR_DateTo = ZDate.Empty;
			complianceReport.ACR_Periodicity = PeriodicityCodes.MonthlyQuarterlyYearly;

			complianceReport.ACR_DateFrom = new ZDate(2022, 4, 18);
			AssertEquals("Date-from adjusted to first of month when date-from entered while date-to is empty", new ZDate(2022, 4, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to not adjusted when date-from entered while date-to is empty", ZDate.Empty, complianceReport.ACR_DateTo);

			complianceReport.ACR_DateFrom = ZDate.Empty;
			complianceReport.ACR_DateTo = new ZDate(2022, 5, 19);
			AssertEquals("Date-from not adjusted when date-to entered while date-from is empty", ZDate.Empty, complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of month when date-to entered while date-from is empty", new ZDate(2022, 5, 31), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateFrom = new ZDate(2022, 5, 18);
			AssertEquals("Date-from adjusted to first of month when date-from entered while date-from and date-to have same month", new ZDate(2022, 5, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of month when date-from entered while date-from and date-to have same month", new ZDate(2022, 5, 31), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateTo = new ZDate(2022, 5, 24);
			AssertEquals("Date-from adjusted to first of month when date-to entered while date-from and date-to have same month", new ZDate(2022, 5, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of month when date-to entered while date-from and date-to have same month", new ZDate(2022, 5, 31), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateFrom = new ZDate(2022, 4, 2);
			AssertEquals("Date-from adjusted to first of quarter when date-from entered while date-from and date-to have timespan of a quarter", new ZDate(2022, 4, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of quarter when date-from entered while date-from and date-to have timespan of a quarter", new ZDate(2022, 6, 30), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateTo = new ZDate(2022, 6, 23);
			AssertEquals("Date-from adjusted to first of quarter when date-to entered while date-from and date-to have timespan of a quarter", new ZDate(2022, 4, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of quarter when date-to entered while date-from and date-to have timespan of a quarter", new ZDate(2022, 6, 30), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateFrom = new ZDate(2022, 3, 1);
			AssertEquals("Date-from adjusted to first of year when date-from entered while date-from and date-to have timespan of more than a quarter", new ZDate(2022, 1, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of year when date-from entered while date-from and date-to have timespan of more than a quarter", new ZDate(2022, 12, 31), complianceReport.ACR_DateTo);

			complianceReport.ACR_DateTo = new ZDate(2022, 9, 30);
			AssertEquals("Date-from adjusted to first of year when date-to entered while date-from and date-to have timespan of more than a quarter", new ZDate(2022, 1, 1), complianceReport.ACR_DateFrom);
			AssertEquals("Date-to adjusted to last of year when date-to entered while date-from and date-to have timespan of more than a quarter", new ZDate(2022, 12, 31), complianceReport.ACR_DateTo);
		}

		public void TestReportPeriodicityMQYalwaysHasRangeMonthOrQuarterOrYear()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateTo = ZDate.Empty;
			complianceReport.ACR_Periodicity = PeriodicityCodes.MonthlyQuarterlyYearly;

			for (var dateFrom = 0; dateFrom < 750; dateFrom += 20)
			{
				complianceReport.ACR_DateFrom = ZDate.Today.AddDays(dateFrom);
				for (var dateTo = 0; dateTo < 750; dateTo += 20)
				{
					complianceReport.ACR_DateTo = ZDate.Today.AddDays(dateTo);
					AssertEquals("From and to date must have the same year", complianceReport.ACR_DateFrom.Year, complianceReport.ACR_DateTo.Year);
					var monthDifference = complianceReport.ACR_DateTo.Month - complianceReport.ACR_DateFrom.Month + 1;
					Assert("Month difference must either be 1 or 3 or 12", monthDifference == 1 || monthDifference == 3 || monthDifference == 12);
					AssertEquals("From date must be first day of the month", 1, complianceReport.ACR_DateFrom.Day);
					AssertEquals("To date must be last day of the month", (new ZDate(complianceReport.ACR_DateTo.Year, complianceReport.ACR_DateTo.Month, 1).AddMonths(1).AddDays(-1)).Day, complianceReport.ACR_DateTo.Day);
				}
			}
		}

		public void TestReportPeriodicityMQYReverseAlwaysHasRangeMonthOrQuarterOrYear()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_DateFrom = ZDate.Empty;
			complianceReport.ACR_Periodicity = PeriodicityCodes.MonthlyQuarterlyYearly;

			for (var dateTo = 0; dateTo < 750; dateTo += 20)
			{
				complianceReport.ACR_DateTo = ZDate.Today.AddDays(dateTo);
				for (var dateFrom = 0; dateFrom < 750; dateFrom += 20)
				{
					complianceReport.ACR_DateFrom = ZDate.Today.AddDays(dateFrom);
					AssertEquals("From and to date must have the same year", complianceReport.ACR_DateFrom.Year, complianceReport.ACR_DateTo.Year);
					var monthDifference = complianceReport.ACR_DateTo.Month - complianceReport.ACR_DateFrom.Month + 1;
					Assert("Month difference must either be 1 or 3 or 12", monthDifference == 1 || monthDifference == 3 || monthDifference == 12);
					AssertEquals("From date must be first day of the month", 1, complianceReport.ACR_DateFrom.Day);
					AssertEquals("To date must be last day of the month", (new ZDate(complianceReport.ACR_DateTo.Year, complianceReport.ACR_DateTo.Month, 1).AddMonths(1).AddDays(-1)).Day, complianceReport.ACR_DateTo.Day);
				}
			}
		}

		public void TestPeriodicityWhenNoComplianceFYThenCFYPeriodicityNotAvailable()
		{
			var nonComplianceFYCountry = CountryCodes.Italy;
			var validTaxRegistrationType = "IVA";
			var validPeriodicityCode = PeriodicityCodes.FinancialYear;
			AssertNull("Country does not implement IComplianceFinancialYear", GetComplianceFinancialYear(nonComplianceFYCountry));

			using (SetupTparReportForComplianceFinancialYearTest(nonComplianceFYCountry, validTaxRegistrationType, validPeriodicityCode))
			{
				Assert(
					"CFY not available when no compliance financial year implementation for country",
					Report.Lookups.PeriodicityList.Cast<CodeDescriptionPair>().All(cp => cp.Code != PeriodicityCodes.ComplianceFinancialYear));
			}
		}

		public void TestPeriodicityWhenComplianceFYExistsThenCFYPeriodicityAvailable()
		{
			using (SetupAustraliaTparReportForComplianceFinancialYearTest())
			{
				Assert(
					"CFY available when compliance financial year implemented for country",
					Report.Lookups.PeriodicityList.Cast<CodeDescriptionPair>().Any(cp => cp.Code == PeriodicityCodes.ComplianceFinancialYear));
			}
		}

		public void TestPeriodicityCFYMatchesCFYRangeForAustralia()
		{
			RunReportCFYMatchesCFYRangeForCountryTest(CountryCodes.Australia, SetupAustraliaTparReportForComplianceFinancialYearTest, AustralianFinancialYear);
		}

		void RunReportCFYMatchesCFYRangeForCountryTest(string countryCode, Func<IDisposable> setupReport, Func<ZInt, DateRange> getFY)
		{
			using (setupReport())
			{
				Report.ACR_DateFrom = ZDate.Today.AddDays(-10);
				Report.ACR_DateTo = ZDate.Today.AddDays(10);
				int year = DateTime.Today.Year;
				Report.AccountingPeriod = year;
				var fy = getFY(year);
				AssertEquals($"Start date of {countryCode}", fy.Start, Report.ACR_DateFrom.ToDateTime());
				AssertEquals($"End date of {countryCode}", fy.End, Report.ACR_DateTo.ToDateTime());
			}
		}

		IComplianceFinancialYear GetComplianceFinancialYear(string countryCode) =>
			(ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<IComplianceFinancialYear>)?.Get();

		IDisposable SetupAustraliaTparReportForComplianceFinancialYearTest() =>
			new DisposableList(new IDisposable[]
			{
				SetupTparReportForComplianceFinancialYearTest(CountryCodes.Australia, "ABN", PeriodicityCodes.ComplianceFinancialYear),
				AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true)
			});

		IDisposable SetupTparReportForComplianceFinancialYearTest(string countryCode, string taxRegistrationType, string periodicityCode)
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = ComplianceReportTypes.TaxablePaymentsAnnualReportType;
			reportConfig.ReportTitle = "Test Report";
			reportConfig.Country = countryCode;
			reportConfig.ReportPeriodicity = periodicityCode;
			reportConfig.TaxRegistrationType = taxRegistrationType;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Report.ACR_ReportType = ComplianceReportTypes.TaxablePaymentsAnnualReportType;

			return GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode);
		}

		DateRange AustralianFinancialYear(ZInt year) =>
			new DateRange(new DateTime(year - 1, 7, 1), new DateTime(year, 6, 30));

		public void TestComplianceReportQueryTimeout_RaisesNoError_WhenDefaultTimeout()
		{
			ErrorReporter.Clear();
			ErrorReporter.SuppressReportingOfErrors = true;
			var timeoutInMinutes = 30;
			AccountingMasterFilesRegistry.Instance.ComplianceReportQueryTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeoutInMinutes);
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(complianceReport, "AL", "HDR", "SRV");
			var reportLines = complianceReport.ReportLines;
			AssertEquals($"Zero ErrorReporter.TotalErrorCount Excepted", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestComplianceReportQueryTimeout_RaisesError_WhenIncreasedTimeout()
		{
			ErrorReporter.Clear();
			ErrorReporter.SuppressReportingOfErrors = true;
			var timeoutInMinutes = 31;
			AccountingMasterFilesRegistry.Instance.ComplianceReportQueryTimeoutInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, timeoutInMinutes);
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(complianceReport, "AL", "HDR", "SRV");
			Factory.Save();
			var reportLines = complianceReport.ReportLines;
			AssertEquals($"One error should have been reported for CommandTimeout value:{timeoutInMinutes}", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestGenerateJPKV7MFile_ComplianceReportIsNotValid_DoesNotCallExportXmlToEDocs()
		{
			// Arrange
			var polandCompany = Creator.CreateCompanyAndBranch("PLWAW");
			Factory.Save();

			var polandReportMock = new Mock<IJPKV7MReport>();
			var logger = new LoggerForTest();

			using (Creator.SwitchEnvToCompany(polandCompany))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportCreated;
				complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
				Factory.Save();

				// Act
				complianceReport.GenerateJPKV7MFile(polandReportMock.Object, logger, null);

				// Assert
				polandReportMock.Verify(m => m.ExportXmlToEDocs(complianceReport, logger), Times.Never);
				AssertEquals("Report status should not change", AccComplianceReport.Status.ReportCreated, complianceReport.ACR_Status);

				// Arrange
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				// Act
				complianceReport.GenerateJPKV7MFile(polandReportMock.Object, logger, null);

				// Assert
				polandReportMock.Verify(m => m.ExportXmlToEDocs(complianceReport, logger), Times.Never);
				AssertEquals("Report status should not change", AccComplianceReport.Status.ReportGenerated, complianceReport.ACR_Status);
			}

			// Arrange
			var auComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			auComplianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
			auComplianceReport.ACR_Status = AccComplianceReport.Status.ReportDataQueued;

			// Act
			auComplianceReport.GenerateJPKV7MFile(polandReportMock.Object, logger, null);

			// Assert
			polandReportMock.Verify(m => m.ExportXmlToEDocs(auComplianceReport, logger), Times.Never);
		}

		public void TestGenerateJPKV7MFile_ComplianceReportIsValid_CallsExportXmlToEDocs()
		{
			// Arrange
			var polandCompany = Creator.CreateCompanyAndBranch("PLWAW");
			Factory.Save();

			var logger = new LoggerForTest();
			var polandReportMock = new Mock<IJPKV7MReport>();
			polandReportMock
				.Setup(m => m.ExportXmlToEDocs(It.IsAny<AccComplianceReport>(), logger))
				.Returns(AccComplianceReport.Status.ReportFinalised);

			using (Creator.SwitchEnvToCompany(polandCompany))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
				Factory.Save();

				// Act
				complianceReport.GenerateJPKV7MFile(polandReportMock.Object, logger, null);

				// Assert
				polandReportMock.Verify(m => m.ExportXmlToEDocs(complianceReport, logger), Times.Once);
				AssertEquals(AccComplianceReport.Status.ReportOutputGenerated, complianceReport.ACR_Status);
			}
		}

		public void TestGenerateIDEAFromQueue_NoOrgProxy()
		{
			var company = Creator.CreateNewCompany("DE1", "DE");
			company.GC_Name = "German Test Company";
			Creator.CreateNewBranch(company, "BRN");
			Factory.Save();

			// Company was created without an org proxy
			AssertIDEAReportError(company, typeof(InvalidOperationException),
				"No organization proxy is found for the reporting company DE1 - German Test Company");
		}

		public void TestGenerateIDEAFromQueue_NoMainOfficeAddress()
		{
			var company = Creator.CreateNewCompany("DE1", "DE");
			company.GC_Name = "German Test Company";
			Creator.CreateNewBranch(company, "BRN");

			var orgProxy = Creator.CreateOrgHeaderDE("DEAGENHAM", false, false, false, false, false, false);
			orgProxy.OH_FullName = "DE1 Org Proxy";

			// Main office address must be active and with capability office and flagged as main		
			Creator.CreateAddress(orgProxy, OrgAddressType.Office, isMain: false,           // active, office, not main
				"Street C", "Street D", "City2", "BV2", "DE", "23456", "+4912345", "b@invalid.invalid");
			Creator.CreateAddress(orgProxy, OrgAddressType.Delivery, isMain: true,          // active, not office, main
				"Street A", "Street B", "City1", "BV1", "DE", "12345", "+4912345", "a@invalid.invalid");
			orgProxy.Addresses[0].OA_IsActive = false;										// not active, office, main

			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			// Company has three addresses, but none of them satisfies all requirements
			AssertIDEAReportError(company, typeof(InvalidOperationException),
				"Please define a single office address as main address for the organization proxy of the reporting company DE1 - German Test Company. This address will be used for the compliance report.");
		}

		void AssertIDEAReportError(GlbCompany company, Type expectedExceptionType, string expectedExceptionMessage)
		{
			var logger = new LoggerForTest();
			using (Creator.SwitchEnvToCompany(company))
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
				complianceReport.ACR_DateFrom = new ZDate(2020, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2020, 12, 31);
				Factory.Save();

				try
				{
					complianceReport.GenerateIDEAFromQueue(logger, new AccComplianceReport.AccComplianceReportUsageCollectorContextData());
				}
				catch (Exception ex)
				{
					AssertEquals(AccComplianceReport.Status.ReportError, complianceReport.ACR_Status);
					AssertEquals(expectedExceptionType, ex.GetType());
					AssertEquals(expectedExceptionMessage, ex.Message);
				}
			}
		}

		public void TestAttachFileToEdoc()
		{
			const string fileName = "edocstestfile.txt";
			const string testData = "Test data";
			const string fileDescription = "File Description";

			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			using (complianceReport.Factory.AddDisposableService())
			{
				var tempDir = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
				var testFileFullPath = Path.Combine(tempDir, fileName);

				File.WriteAllText(testFileFullPath, testData);

				complianceReport.AttachFileToEdoc(testFileFullPath, fileDescription);
				Factory.Save();

				var factory = new BusinessObjectFactory();
				var complianceReport2 = factory.LoadTop1<AccComplianceReport>(new ZQuery(AccComplianceReportSchema.PK, complianceReport.PK));
				AssertEquals("Number of files attached to the report", 1, complianceReport2.DocManagerInfo().AllEDocs.Count);
				AssertEquals("Name of attached file", fileName, complianceReport2.DocManagerInfo().AllEDocs[0].FileName);
				AssertEquals("Content of attached file", testData, complianceReport2.DocManagerInfo().AllEDocs[0].ImageData.ToUTF8());
			}
		}

		public void TestCalculateGeneralLedgerAmountViaReportSubCode()
		{
			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DAB");

			// AVE: I had to comment out most of this test because it is not correct. Some of the GL Account abbreviation do not exist
			// I discussed this test with Daniel (DAB) and he suggested to remove it for now, to be reworked later. Still there is a single case which seems to work.
			//var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP INVOICE", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			//var apInvoiceLine = apInvoice.Lines[0];
			//apInvoiceLine.AL_AT = Creator.GST1.PK;

			var apInvoiceRecoverable = Creator.CreateAPInvoice<APInvoice>("AP RECOVERABLE", Creator.AUD, 1m, 111m, 11.1m, 0m, 111m, 11.1m, 0m, Creator.ABIGAS);
			var apInvoiceRecoverableLine = apInvoiceRecoverable.Lines[0];
			apInvoiceRecoverable.Lines[0].AL_AT = Creator.GST1.PK;
			apInvoiceRecoverable.Lines[0].AL_InputGSTVATRecoverable = 0.25m;
			apInvoiceRecoverable.Lines[0].AL_LineType = TransactionLineTypes.Cost;
			apInvoiceRecoverable.Lines[0].AL_GSTVATBasis = "A";
			Factory.Save();

			//Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APCtrl*Total", null, apInvoiceLine);
			//Creator.CreateComplianceReportQueueEntry(Report, "*AP*CTR*APCtrl*", null, apInvoiceLine);
			//Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**Rev-", null, apInvoiceLine);
			//Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTOut*-", null, apInvoiceLine);
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTIn*-", null, apInvoiceRecoverableLine);
			//Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**GSTNotRec-", null, apInvoiceRecoverableLine);
			Report.GenerateFromQueue();

			//AssertDBViewResultMatchesCSMethodResult("*AP*INV*APCtrl*Total");
			//AssertDBViewResultMatchesCSMethodResult("*AP*CTR*APCtrl*");
			// AssertDBViewResultMatchesCSMethodResult("*AP*INV**Rev-"); // AVE: Invalid Account code
			//AssertDBViewResultMatchesCSMethodResult("*AP*INV*GSTOut*-");
			AssertDBViewResultMatchesCSMethodResult("*AP*INV*GSTIn*-", apInvoiceRecoverableLine.AL_LineType, apInvoiceRecoverableLine.AL_InputGSTVATRecoverable);
			//AssertDBViewResultMatchesCSMethodResult("*AP*INV**GSTNotRec-", apInvoiceRecoverableLine.AL_LineType, apInvoiceRecoverableLine.AL_InputGSTVATRecoverable);  // AVE: Invalid Account code

			void AssertDBViewResultMatchesCSMethodResult(ZString reportSubCode, string lineType = "", decimal inputGstVatRecoverable = 0m)
			{
				// ReportSubCode is not returned to DAB reports. GL Account movement amounts are not loaded for a non DAB reports.
				// Here is an attempt to mapping via Account Number
				var accountMappings = (new ControlAccountAndReportSubCodeMapping()).AccountPkToSubCodesTable;
				var accountMapping = accountMappings.Rows.OfType<DataRow>().FirstOrDefault(x => x[0].ToString() == reportSubCode);
				AssertNotNull(reportSubCode + " account mapping should exist", accountMapping);
				var accountPK = new ZGuid(accountMapping[1]);
				var account = Factory.Load<AccGLHeader>(accountPK);

				var reportLine = Report.ReportLines.OfType<AccComplianceReportLine>().FirstOrDefault(v => v.AG_AccountNum == account.AG_AccountNum);
				AssertNotNull($"No report line found with sub code {reportSubCode}, account number {account.AG_AccountNum}", reportLine);
				var amountCalculatedInDBView = reportLine.GeneralLedgerAmountDR - reportLine.GeneralLedgerAmountCR;
				var amountCalculatedInCSMethod = Report.CalculateGeneralLedgerAmountViaReportSubCode(reportLine.TotalExTaxAmount, reportLine.TotalTaxAmount, reportSubCode, lineType, "A", inputGstVatRecoverable, reportLine.GC_RX_NKLocalCurrency, Report.Company.LocalCurrency.Decimals);
				AssertEquals($"Sub code {reportSubCode}", amountCalculatedInDBView, amountCalculatedInCSMethod);
			}
		}

		public void TestReportType_ReadOnly()
		{
			AssertNullOrEmpty(Report.ACR_ReportType);
			Assert(Report.ACR_ReportTypeInfo.ReadOnly);
			Report.FillWithValidTestData();
			AssertNotNullOrEmpty(Report.ACR_ReportType);
			Assert(Report.ACR_ReportTypeInfo.ReadOnly);
		}

		[TestDate(2023, 11, 15)]
		public void TestLoadOpeningBalancesAndLineNumOffsetWithComma()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));

			Report.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(Report, "**", "DAB");

			CreateAndSaveTwoAPInvoices();
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APSusp*-", null, InvoiceLines.ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*GSTIn*-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV**GSTNotRec-", null, InvoiceLines.Where(l => !l.AL_GSTVAT.IsEmpty).ToArray());
			Creator.CreateComplianceReportQueueEntry(Report, "*AP*INV*APCtrl*Total", null, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			Report.ClearReportLines_ForTestOnly();
			AssertEquals("ReportLines.Count", 8, Report.ReportLines.Count);
			var reportLines = Report.ReportLines;

			var invoice1Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice1.AH_TransactionNum).ToArray();
			AssertEquals("Invoice1 Lines", 4, invoice1Lines.Length);
			AssertEquals("All Invoice1 lines have ACL_ReportSequence 1", invoice1Lines.Length, invoice1Lines.Where(x => x.ACL_ReportSequence == 1).Count());

			var invoice2Lines = reportLines.Cast<AccComplianceReportLine>().Where(x => x.AH_TransactionNum == APInvoice2.AH_TransactionNum).ToArray();
			AssertEquals("Invoice2 Lines", 4, invoice2Lines.Length);
			AssertEquals("All Invoice2 lines have ACL_ReportSequence 2", invoice1Lines.Length, invoice2Lines.Where(x => x.ACL_ReportSequence == 2).Count());

			AssertReportTotalsWithGLBalance(Report, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 515m, 515m, 2);

			var sqlQuery = "UPDATE dbo.GenAddOnColumn SET XA_Data = REPLACE(XA_Data,'.',','), XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name IN ('GeneralLedgerAmountDR','GeneralLedgerAmountDR')";
			DbCommand command = ((IDbConnected)Factory).Connection.Command(sqlQuery);
			command.ExecuteScalar();

			var newReport = Factory.NewWithValidTestData<AccComplianceReport>();
			newReport.ACR_ReportType = Report.ACR_ReportType;
			newReport.ACR_DateTo = newReport.ACR_DateFrom = Report.ACR_DateTo.AddDays(1);
			Factory.Save();

			newReport.GenerateFromQueue();

			AssertEquals(newReport.LineNumOffset, 2);
			AssertEquals(newReport.GLOpeningBalanceCR, 515m);
			AssertEquals(newReport.GLOpeningBalanceDR, 515m);
		}

		public void TestSetAddOnColumnDecimalWithComma()
		{
			Report.FillWithValidTestData();

			Report.SetAddOnColumnDecimal("GeneralLedgerAmountDR", 123.45m);
			var addOnDef = Report.AddOnColumnsForTestOnly.FirstOrDefault(x => x.XA_Name == "GeneralLedgerAmountDR");
			AssertEquals("123.45000000", addOnDef.XA_Data);

			var saveCultureDefaultNumberFormat = new NumberFormatInfo();

			using (new DisposableAction(() => getDefaultNumberFormat(), () => resetNumberFormat()))
			{
				Culture.Default.NumberFormat.NumberGroupSeparator = "@";
				Culture.Default.NumberFormat.CurrencyDecimalSeparator = ",";
				Culture.Default.NumberFormat.CurrencyDecimalDigits = 5;
				Culture.Default.NumberFormat.NumberDecimalDigits = 5;
				Culture.Default.NumberFormat.NumberDecimalSeparator = ",";

				using (Culture.SetTemporarily(Culture.Default))
				{
					Report.SetAddOnColumnDecimal("GeneralLedgerAmountDR", 321.54m);
					var addOn = Report.AddOnColumnsForTestOnly.FirstOrDefault(x => x.XA_Name == "GeneralLedgerAmountDR");
					AssertEquals("321.54000000", addOn.XA_Data);
				}

				using (Culture.SetTemporarily(Culture.GetCulture("SE")))
				{
					Report.SetAddOnColumnDecimal("GeneralLedgerAmountCR", 123.45m);
					var addOn = Report.AddOnColumnsForTestOnly.FirstOrDefault(x => x.XA_Name == "GeneralLedgerAmountCR");
					AssertEquals("123.45000000", addOn.XA_Data);
				}
			}

			void getDefaultNumberFormat()
			{
				saveCultureDefaultNumberFormat = new NumberFormatInfo()
				{
					NumberGroupSeparator = Culture.Default.NumberFormat.NumberGroupSeparator,
					CurrencyDecimalSeparator = Culture.Default.NumberFormat.CurrencyDecimalSeparator,
					CurrencyDecimalDigits = Culture.Default.NumberFormat.CurrencyDecimalDigits,
					NumberDecimalDigits = Culture.Default.NumberFormat.NumberDecimalDigits,
					NumberDecimalSeparator = Culture.Default.NumberFormat.NumberDecimalSeparator
				};
			}

			void resetNumberFormat()
			{
				Culture.Default.NumberFormat.NumberGroupSeparator = saveCultureDefaultNumberFormat.NumberGroupSeparator;
				Culture.Default.NumberFormat.CurrencyDecimalSeparator = saveCultureDefaultNumberFormat.CurrencyDecimalSeparator;
				Culture.Default.NumberFormat.CurrencyDecimalDigits = saveCultureDefaultNumberFormat.CurrencyDecimalDigits;
				Culture.Default.NumberFormat.NumberDecimalDigits = saveCultureDefaultNumberFormat.NumberDecimalDigits;
				Culture.Default.NumberFormat.NumberDecimalSeparator = saveCultureDefaultNumberFormat.NumberDecimalSeparator;
			}
		}

		public void TestGetAddOnDecimal()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			report.SetAddOnColumnDecimal("GeneralLedgerAmountCR", 123.45m);
			report.SetAddOnColumnDecimal("GeneralLedgerAmountDR", 567.89m);

			using (Culture.SetTemporarily(Culture.GetCulture("EN")))
			{
				AssertEquals(123.45m, report.GetAddOnDecimal("GeneralLedgerAmountCR"));
				AssertEquals(567.89m, report.GetAddOnDecimal("GeneralLedgerAmountDR"));
			}

			using (Culture.SetTemporarily(Culture.GetCulture("IT")))
			{
				AssertEquals(123.45m, report.GetAddOnDecimal("GeneralLedgerAmountCR"));
				AssertEquals(567.89m, report.GetAddOnDecimal("GeneralLedgerAmountDR"));
			}
		}

		public void TestSupportsGLBalanceIsTrue_GLD()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			AssertEquals(false, report.SupportsGLBalance);

			Creator.CreateConfigurationForComplianceReport(report,
				baseTablePrefix: ReportBaseTablePrefixListCodes.GeneralLedgerData, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			Assert("SupportsGLBalance is true when baseTablePrefix is GLD", report.SupportsGLBalance);
		}

		public void TestSupportsDayBookIsTrue_GLD()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			AssertEquals(false, report.SupportsDayBook);

			Creator.CreateConfigurationForComplianceReport(report,
				baseTablePrefix: ReportBaseTablePrefixListCodes.GeneralLedgerData, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			Assert("SupportsDayBook is true when baseTablePrefix is GLD", report.SupportsDayBook);
		}

		[TestDate(2023, 01, 1)]
		[SuspendCriticalValidation]
		public virtual void TestReportLinesExportWhenTablePrefixIsAll()
		{
			PrepareTransactions();
			CreateComplianceReportAndConfiguration(ReportBaseTablePrefixListCodes.AllTransactions);
			DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueAllForComplianceReport @ReportPK = '{0}', @OpeningCategory = '{1}', @ClosingCategory = '{2}'", reportForExport.PK, "", ""));
			reportForExport.GenerateFromQueue();
			var reportLinesExport = reportForExport.ReportLinesExport.Cast<AccComplianceReportLine>();
			AssertEquals(26, reportLinesExport.Count());
		}

		protected void PrepareTransactions()
		{
			Creator.CreateTestPeriodsForEntireYear(2023);

			var testDate = new ZDateTime(2023, 01, 1);

			aRInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "121", Creator.AUD, 1M, 10M, 10M, 10M, 10M);
			aRInvoice.Lines[0].AL_PostDate = testDate;
			aRInvoice.Lines[0].AL_ReverseDate = testDate;
			aRInvoice.AH_OH = Creator.ABIGAS.PK;
			Creator.CreateInvoiceLine(aRInvoice, GlbCompany.CurrentCompany.LocalCurrency, 1, 100, 10, 0, Creator.GLHeader1.PK);

			apJournal = Creator.CreateJournal<APJournal>(100m, testDate, Creator.ABIGAS.PK);

			cashBasisVAT = Creator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001",
				GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasisVAT.TransactionLine.AL_AT = Creator.GST1.PK;

			var job = Creator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			wip = Creator.CreateWIP(job);
			wip.AL_LineAmount = 20m;
			wip.AL_OSAmount = 20m;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = Creator.CC1.PK;
			acr = Creator.CreateAccrual(charge1);

			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var gLGJL = Creator.CreateGLJournal("GJL", testDate, testDate);
			gLGJLline1 = Creator.CreateGLJournalLine(gLGJL, 250M, DebitCredit.DR, Creator.GLHeader1.PK);
			gLGJLline2 = Creator.CreateGLJournalLine(gLGJL, 250M, DebitCredit.CR, Creator.GLHeader2.PK);

			Creator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			Factory.Save();
		}

		protected void CreateComplianceReportAndConfiguration(string tablePrefix)
		{
			var newFactory = Factory.CreateNewFactory();
			reportForExport = (AccComplianceReport)newFactory.NewWithValidTestData(BusinessObjectType);
			reportForExport.FillWithValidTestData();
			Creator.CreateConfigurationForComplianceReport(reportForExport,
				baseTablePrefix: tablePrefix, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();
		}

		void GenerateGLDData()
		{
			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();
			((INeedRow)aRInvoice.Lines[1]).Row.SetAdded();

			new TestObjectCreator(Factory).MockNudgeGLDProcessData([((INeedRow)aRInvoice.Lines[0]).Row, ((INeedRow)aRInvoice.Lines[1]).Row, ((INeedRow)cashBasisVAT).Row, ((INeedRow)apJournal).Row, ((INeedRow)wip).Row, ((INeedRow)acr).Row, ((INeedRow)gLGJLline1).Row, ((INeedRow)gLGJLline2).Row]);
		}

		protected void TestGLDReportGLMovementDetailsExport(bool isGLDComplianceReportUsingEDWEnabled, bool transferDataToEDW, bool expectedEmptyResult)
		{
			using (ObjectFactory.Substitute(SetGldComplianceReportUsingEDWFeatureControl(isGLDComplianceReportUsingEDWEnabled)))
			{
				PrepareDataAndAssert(
					() =>
					{
						PrepareGLMovementDetailsData();
					},
					() =>
					{
						reportForExport.ClearReportLines_ForTestOnly();
						AssertGLBalanceCollection(reportForExport.GLMovementDetailsExport.Cast<GeneralLedgerBalanceLine>(), "GL Movements", expectedEmptyResult ? 0 : 1015m);
					},
					transferDataToEDW
				);
			}
		}

		protected void TestGLDReportLinesExport(bool isGLDComplianceReportUsingEDWEnabled, bool transferDataToEDW, bool expectedEmptyResult)
		{
			using (ObjectFactory.Substitute(SetGldComplianceReportUsingEDWFeatureControl(isGLDComplianceReportUsingEDWEnabled)))
			{
				PrepareDataAndAssert(
					() =>
					{
						PrepareTransactions();
						GenerateGLDData();
						CreateComplianceReportAndConfiguration(ReportBaseTablePrefixListCodes.GeneralLedgerData);
						reportForExport.GenerateFromQueue();
					},
					() =>
					{
						AssertEquals(expectedEmptyResult ? 0 : 11, reportForExport.ReportLinesExport.Count);
					},
					transferDataToEDW
				);
			}
		}

		protected IFeatureControlManager SetGldComplianceReportUsingEDWFeatureControl(bool isEnabled)
		{
			var mockIFeatureData = new Mock<IFeatureData>();
			var gLDFeatureControlData = new GeneralLedgerDataFeatureControlModel() { EnableGLDComplianceReportUsingEDW = isEnabled };
			var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
			mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out gLDFeatureControlData)).Returns(true);
			mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

			return mockIFeatureControlManager.Object;
		}

		void PrepareDataAndAssert(Action insertAction, Action assertAction, bool transferDataToEDW)
		{
			using (transferDataToEDW ? AccountingDataTransform.CreateSnapShotForEdwAndTransformData(insertAction, InsertTableList) : null)
			{
				if (!transferDataToEDW)
				{
					insertAction.Invoke();
				}
				assertAction();
			}
		}

		List<string> InsertTableList => new List<string>()
		{
			"AccPeriodManagement",
			"GlbCompany",
			"GlbBranch",
			"GlbDepartment",
			"OrgHeader",
			"StmData",
			"OrgAddress",
			"OrgAddressCapability",
			"JobHeader",
			"JobShipment",
			"AccTransactionHeader",
			"AccTransactionLines",
			"AccCashBasisVAT",
			"AccGeneralLedgerData",
			"AccComplianceReportTransactionPivot"
		};

		void PrepareGLMovementDetailsData()
		{
			Creator.CreateTestPeriods(ZDateTime.Today);
			Report.FillWithValidTestData();
			Factory.Save();

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(Report.ACR_DateTo, Report.ACR_GC_Company);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);

			CreateAndSaveTwoAPInvoices(currentPeriod.AM_StartDate);

			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2018, 1, 1));

			((INeedRow)InvoiceLines[0]).Row.SetAdded();
			((INeedRow)InvoiceLines[1]).Row.SetAdded();
			((INeedRow)InvoiceLines[2]).Row.SetAdded();
			((INeedRow)InvoiceLines[3]).Row.SetAdded();
			((INeedRow)InvoiceLines[4]).Row.SetAdded();
			((INeedRow)InvoiceLines[5]).Row.SetAdded();

			var processor = new GeneralLedgerDataProcessor();
			var processorAsInterface = processor as IGeneralLedgerDataProcessor;
			AssertNotNull(processorAsInterface);
			processorAsInterface.ProcessData(new[] { ((INeedRow)InvoiceLines[0]).Row, ((INeedRow)InvoiceLines[1]).Row, ((INeedRow)InvoiceLines[2]).Row, ((INeedRow)InvoiceLines[3]).Row, ((INeedRow)InvoiceLines[4]).Row, ((INeedRow)InvoiceLines[5]).Row });

			var newFactory = Factory.CreateNewFactory();
			reportForExport = newFactory.NewWithValidTestData<AccComplianceReport>();
			reportForExport.ACR_Periodicity = PeriodicityCodes.AccountingPeriod;
			reportForExport.AccountingPeriod = currentPeriod.AM_Period;
			Creator.CreateConfigurationForComplianceReport(reportForExport, baseTablePrefix: ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();

			reportForExport.GenerateFromQueue();
		}

		protected InvoicingBase aRInvoice;
		protected APJournal apJournal;
		protected AccCashBasisVAT cashBasisVAT;
		protected WIP wip;
		protected Accrual acr;
		protected GLJournalLine gLGJLline1;
		protected GLJournalLine gLGJLline2;
		protected AccComplianceReport reportForExport;

		public void TestTaxGroupColumnValueforPortugalSAFReports_SAFT()
		{
			TestTaxGroupColumnValueforPortugalSAFReports(AccComplianceReport.ReportTypes.SAFT, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		public void TestTaxGroupColumnValueforPortugalSAFReports_SAFTOnlyTransactions()
		{
			TestTaxGroupColumnValueforPortugalSAFReports(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.NoGrouping);
		}

		[TestDate(2024, 1, 17)]
		public void TestTaxGroupColumnValueforPortugalSAFReports(string reportType, string tablePrefix, string lineGrouping)
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Portuguese))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.Company.GC_Phone = "012345678";
				report.Company.GC_Email = "email@company.com";
				var periodCalculator = new AccountingPeriodCalculator(Factory);
				var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);

				report.ACR_ReportType = reportType;
				report.ACR_Periodicity = PeriodicityCodes.AccountingPeriod;
				report.AccountingPeriod = currentPeriod.AM_Period;
				AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);
				Factory.Save();

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVAREV13"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "M30";
				taxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxMessage.A9_LocalMsg = "Local Tax Message";
				Factory.Save();

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0003", Creator.AUD, 1m, 200m, 0m, 200m, 0m);
				invoice.AH_OH = Creator.AALSHI.PK;
				invoice.AH_ComplianceSubType = "TXI";
				invoice.AH_InvoiceTerm = Constants.InvoiceTerms.FromMonthEnd;
				invoice.AH_InvoiceTermDays = 10;
				invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

				Assert("Has Lines", invoice.Lines.Count > 0);
				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = taxRate.PK;
				line.AL_A9_VATClass = taxMessage.PK;
				Creator.ABIGAS.MainAddress.Postcode = "";

				Factory.Save();

				if (reportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, 1);
					AssertEquals(1, report.ReportLines.Count);
				}
				else
				{
					Creator.CreateComplianceReportTransactionPivot(report, invoice, 1, "*AR*INV*ARCtrl*Total");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV**Rev-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*-");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*Rev");
					Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*GSTOut*-");
					AssertEquals(5, report.ReportLines.Count);
				}

				AssertEquals("IVA - autoliquidação", AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.Value.OfType<CodeDescriptionBoolRelatedItem>().FirstOrDefault(x => x.Code == taxMessage.A9_TaxGroupCode).Description);

				foreach (var row in report.ReportLines.Cast<AccComplianceReportLine>())
				{
					if (row.AL_A9_VATClass != Guid.Empty)
					{
						AssertEquals("IVA - autoliquidação", row.TaxGroupDescription);
					}
				}
			}
		}

		#region Implementation

		protected AccComplianceReport Report
		{
			get { return (AccComplianceReport)BusinessObject; }
		}

		protected TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;
		APInvoice APInvoice1, APInvoice2, APInvoice3;
		protected readonly List<InvoicingLineBase> InvoiceLines = new List<InvoicingLineBase>();
		GLJournal OpeningJournal, ClosingJournal;
		readonly List<GLJournalLine> GLJournalLines = new List<GLJournalLine>();
		WIP wip1, wip2;
		Accrual accrual1, accrual2;

		AccGLHeader RevenueGLAccount => Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1010.10.10"));
		AccGLHeader CostGLAccount => Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1010.20.10"));
		AccGLHeader DebtorsControlGLAccount => Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6210.00.00"));

		protected void CreateAndSaveTwoAPInvoices(ZDateTime? postDate = null)
		{
			CreateAPInvoice1(postDate);
			CreateAPInvoice2(postDate);

			Factory.Save();
		}

		void CreateAPInvoice1(ZDateTime? postDate = null)
		{
			AssertNotNull(CostGLAccount);
			APInvoice1 = Creator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", APInvoice1.Lines.Count > 0);
			var line1_1 = APInvoice1.Lines[0];
			line1_1.AL_AG = CostGLAccount.PK;
			line1_1.AL_AT = Creator.GST1.PK;
			line1_1.AL_InputGSTVATRecoverable = 0.8m;
			var line1_2 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1m, 120m);
			line1_2.AL_AG = CostGLAccount.PK;
			line1_2.AL_AT = Creator.GSTFREE1.PK;
			var line1_3 = Creator.CreateInvoiceLine(APInvoice1, Creator.AUD, 1m, 80m);
			line1_3.AL_AG = CostGLAccount.PK;
			line1_3.AL_AT = Creator.REV.PK;

			if (postDate.HasValue)
			{
				APInvoice1.AH_PostDate = postDate.Value;
			}

			InvoiceLines.AddRange(new InvoicingLineBase[] { line1_1, line1_2, line1_3 });
		}

		void CreateAPInvoice2(ZDateTime? postDate = null)
		{
			AssertNotNull(CostGLAccount);
			APInvoice2 = Creator.CreateAPInvoice<APInvoice>("I0002" + Report.ReportLines.Count.ToString(), Creator.AUD, 1m, 50m, 10m, 0m, 50m, 10m, 0m, Creator.Creditor1);
			Assert("Has Lines", APInvoice2.Lines.Count > 0);
			var line2_1 = APInvoice2.Lines[0];
			line2_1.AL_AG = CostGLAccount.PK;
			line2_1.AL_AT = Creator.GST1.PK;
			line2_1.AL_InputGSTVATRecoverable = 0.5m;
			var line2_2 = Creator.CreateInvoiceLine(APInvoice2, Creator.AUD, 1m, 130m);
			line2_2.AL_AG = CostGLAccount.PK;
			line2_2.AL_AT = Creator.GSTFREE1.PK;
			var line2_3 = Creator.CreateInvoiceLine(APInvoice2, Creator.AUD, 1m, 20m);
			line2_3.AL_AG = CostGLAccount.PK;
			line2_3.AL_AT = Creator.REV.PK;

			if (postDate.HasValue)
			{
				APInvoice2.AH_PostDate = postDate.Value;
			}

			InvoiceLines.AddRange(new InvoicingLineBase[] { line2_1, line2_2, line2_3 });
		}

		void CreateAndSaveWIPAccrual(ZDateTime? postDate = null)
		{
			Creator.AALSHI.OH_IsDebtor = true;
			wip1 = Creator.CreateWIP(Creator.Job1, Creator.CC1, 1M, "Desc1", 100M, null, Creator.AALSHI);
			wip2 = Creator.CreateWIP(Creator.Job1, Creator.CC1, 1M, "Desc2", 200M, null, Creator.ABIGAS);
			Creator.ABIGAS.OH_IsCreditor = true;
			accrual1 = Creator.CreateAccrual(Creator.Job1, Creator.CC1, 1M, "Desc1", 100M, null, Creator.AALSHI);
			accrual2 = Creator.CreateAccrual(Creator.Job1, Creator.CC1, 1M, "Desc2", 200M, null, Creator.ABIGAS);

			if (postDate.HasValue)
			{
				wip1.AL_PostDate = wip2.AL_PostDate = postDate.Value;
				accrual1.AL_PostDate = accrual2.AL_PostDate = postDate.Value;
			}

			Factory.Save();
		}

		void CreateAndSaveOpeningAndClosingGLJournal()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CLS";
			category1.Description = (NoResString)"Category Closing";
			category1.Bool = true; // active
			category1.Bool2 = false; // elimination
			category1.Bool3 = true; // closing
			category1.Bool4 = false; // opening
			var category2 = list.AddNew();
			category2.Code = "OPN";
			category2.Description = (NoResString)"Category Opening";
			category2.Bool = true; // active
			category2.Bool2 = false; // elimination
			category2.Bool3 = false; // closing
			category2.Bool4 = true; // opening

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			OpeningJournal = Creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			OpeningJournal.AH_TransactionCategory = "OPN";
			var line1_1 = Creator.CreateGLJournalLine(OpeningJournal, 10, DebitCredit.CR, RevenueGLAccount.PK);
			var line1_2 = Creator.CreateGLJournalLine(OpeningJournal, 10, DebitCredit.DR, DebtorsControlGLAccount.PK);

			ClosingJournal = Creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			ClosingJournal.AH_TransactionCategory = "CLS";
			var line2_1 = Creator.CreateGLJournalLine(ClosingJournal, 30, DebitCredit.CR, RevenueGLAccount.PK);
			var line2_2 = Creator.CreateGLJournalLine(ClosingJournal, 30, DebitCredit.DR, DebtorsControlGLAccount.PK);

			GLJournalLines.AddRange(new GLJournalLine[] { line1_1, line1_2, line2_1, line2_2 });
			Factory.Save();
		}

		void AssertReportTotals(AccComplianceReport report, decimal exTaxAmount, decimal taxAmount = 0m, decimal taxRecoverableAmount = 0m, decimal taxNotRecoverableAmount = 0m, decimal taxReverseChargeAmount = 0m, string comment = "Report Totals", bool isPreviousPeriod = false, decimal preCalculatedAmount = 0m)
		{
			var reportTotals = isPreviousPeriod ? report.ReportTotalsPreviousPeriod : report.ReportTotalsCurrentPeriod;
			AssertEquals("There should be only one line in report Totals", 1, reportTotals.Count);
			AssertReportTotalLine(reportTotals[0], exTaxAmount, taxAmount, 0m, 0m, taxRecoverableAmount, taxNotRecoverableAmount, taxReverseChargeAmount, 0m, 0m, comment, preCalculatedAmount: preCalculatedAmount);
		}

		void AssertReportTotalsWithGLBalance(AccComplianceReport report, decimal serviceExTaxAmount, decimal serviceTaxAmount,
			decimal goodsExTaxAmount = 0m, decimal goodsTaxAmount = 0m,
			decimal taxRecoverableAmount = 0m, decimal taxNotRecoverableAmount = 0m,
			decimal taxReverseChargeAmount = 0m,
			decimal glAmountDR = 0m, decimal glAmountCR = 0m, int lineNum = 0, decimal glOpeningDR = 0m, decimal glOpeningCR = 0m)
		{
			AssertEquals("ReportTotals", 3, report.ReportTotals.Count);
			var totals = report.ReportTotals;
			totals.Sort(AccComplianceReportLineBase.Schema.Comment, ListSortDirection.Ascending);

			AssertReportTotalLine(totals[0], 0m, 0m, 0m, 0m, 0m, 0m, 0m, glOpeningDR, glOpeningCR, "  Opening Balance", 0);
			AssertReportTotalLine(totals[1], serviceExTaxAmount, serviceTaxAmount, goodsExTaxAmount, goodsTaxAmount, taxRecoverableAmount, taxNotRecoverableAmount, taxReverseChargeAmount, glAmountDR, glAmountCR, " Balance Movements", lineNum);
			AssertReportTotalLine(totals[2], 0m, 0m, 0m, 0m, 0m, 0m, 0m, glOpeningDR + glAmountDR, glOpeningCR + glAmountCR, "Closing Balance", 0);
		}

		void AssertReportTotalsWithGLBalanceDetails(AccComplianceReport report,
			decimal glAmountDR = 0m, decimal glAmountCR = 0m, int lineNum = 0, decimal glOpeningDR = 0m, decimal glOpeningCR = 0m, decimal glClosingDR = 0m, decimal glClosingCR = 0m)
		{
			AssertEquals("ReportTotals", 3, report.ReportTotals.Count);
			var totals = report.ReportTotals;
			totals.Sort(AccComplianceReportLineBase.Schema.Comment, ListSortDirection.Ascending);

			AssertReportTotalLine(totals[0], 0m, 0m, 0m, 0m, 0m, 0m, 0m, glOpeningDR, glOpeningCR, "  Opening Balance", 0);
			AssertReportTotalLine(totals[1], 0m, 0m, 0m, 0m, 0m, 0m, 0m, glAmountDR, glAmountCR, " Balance Movements", lineNum);
			AssertReportTotalLine(totals[2], 0m, 0m, 0m, 0m, 0m, 0m, 0m, glClosingDR, glClosingCR, "Closing Balance", 0);
		}

		void AssertReportTotalLine(AccComplianceReportLineBase line, decimal serviceExTaxAmount, decimal serviceTaxAmount,
			decimal goodsExTaxAmount = 0m, decimal goodsTaxAmount = 0m,
			decimal taxRecoverableAmount = 0m, decimal taxNotRecoverableAmount = 0m,
			decimal taxReverseChargeAmount = 0m,
			decimal glAmountDR = 0m, decimal glAmountCR = 0m,
			string comment = "Report Totals", int lineNum = 0,
			decimal preCalculatedAmount = 0m)
		{
			AssertEquals("ServiceExTaxAmount", serviceExTaxAmount, line.ServiceExTaxAmount);
			AssertEquals("ServiceTaxAmount", serviceTaxAmount, line.ServiceTaxAmount);
			AssertEquals("GoodsExTaxAmount", goodsExTaxAmount, line.GoodsExTaxAmount);
			AssertEquals("GoodsTaxAmount", goodsTaxAmount, line.GoodsTaxAmount);
			AssertEquals("TaxRecoverableAmount", taxRecoverableAmount, line.TaxRecoverableAmount);
			AssertEquals("TaxNotRecoverableAmount", taxNotRecoverableAmount, line.TaxNotRecoverableAmount);
			AssertEquals("TaxReverseChargeAmount", taxReverseChargeAmount, line.TaxReverseChargeAmount);
			AssertEquals("TaxReverseChargeInputAmount", taxReverseChargeAmount, line.TaxReverseChargeInputAmount);
			AssertEquals("TaxReverseChargeOutputAmount", taxReverseChargeAmount, line.TaxReverseChargeOutputAmount);
			AssertEquals("GeneralLedgerAmountDR", glAmountDR, line.GeneralLedgerAmountDR);
			AssertEquals("GeneralLedgerAmountCR", glAmountCR, line.GeneralLedgerAmountCR);
			AssertEquals("Comment", comment, line.Comment);
			AssertEquals("LineNum", lineNum, line.LineNum);
			AssertEquals("PreCalculatedAmount", preCalculatedAmount, line.PreCalculatedAmount);
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AccComplianceReport>();
		}

		protected virtual Type BusinessObjectType => typeof(AccComplianceReport);
	}

	[TestedType(typeof(AccComplianceReportDocumentSupporter))]
	public class AccComplianceReportDocumentSupportTest : DocumentSupporterTest
	{
		public void TestDocManagerCode()
		{
			var report = (IDocManagerSupport)GetDocumentSupportableBusinessObject();
			AssertNotNull(report.DocManagerInfo);
			AssertEquals("Code should be ACR. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", Core.Constants.DocManagerCodes.ComplianceReport, report.DocManagerInfo.DocManagerCode);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<AccComplianceReport>();
		}
	}

	[TestedType(typeof(AccComplianceReport))]
	public class AccComplianceReportWorkflowProviderTest : WorkflowProviderTest<AccComplianceReport, AccComplianceReportProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.AccComplianceReportCode; }
		}
	}
}

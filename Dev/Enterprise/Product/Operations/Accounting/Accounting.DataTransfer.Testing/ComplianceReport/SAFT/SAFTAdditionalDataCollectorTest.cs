using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.SAFT.Testing
{
	public class SAFTAdditionalDataCollectorTest : TestCaseWithFactory
	{
		public void TestCustomerSalesInvoices()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				foreach (ComplianceReportDataCollectionMode mode in Enum.GetValues(typeof(ComplianceReportDataCollectionMode)))
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(mode == ComplianceReportDataCollectionMode.SAFT1_30 ? CountryCodes.Norway : CountryCodes.Portugal))
					{
						var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
						var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
						SetComplianceReportType(report1, reportType);
						SetComplianceReportType(report2, reportType, report1.ACR_DateTo.AddDays(1), report2.ACR_DateFrom.AddMonths(1));

						Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);
						Creator.CreateConfigurationForComplianceReport(report2, tablePrefix, lineGrouping);

						var invoiceType = mode == ComplianceReportDataCollectionMode.SAFTSelfBilling ? typeof(APInvoice) : typeof(ARInvoice);
						var invoice1 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + reportType + "1", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice1.AH_OH = Creator.ABIGAS.PK;
						invoice1.AH_TransactionReference = "ref1";
						var invoice2 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + reportType + "2", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice2.AH_OH = Creator.AALSHI.PK;
						invoice2.AH_TransactionReference = "ref2";
						var invoice3 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + reportType + "3", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice3.AH_OH = Creator.ABIGAS.PK;
						invoice3.AH_PostDate = invoice3.AH_InvoiceDate = report2.ACR_DateFrom;
						invoice3.AH_TransactionReference = "ref3";
						var invoice4 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + reportType + "4", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice4.AH_OH = Creator.ZECTRA.PK;
						invoice4.AH_PostDate = invoice4.AH_InvoiceDate = report2.ACR_DateFrom;
						invoice4.AH_TransactionReference = "ref4";

						if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
						{
							invoice1.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice3.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
						}

						Factory.Save();

						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice1, 1);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice2, 2);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report2, invoice3, 1);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report2, invoice4, 2);

						AssertCustomerSalesInvoices();

						void AssertCustomerSalesInvoices()
						{
							var dataCollector = new SAFTAdditionalDataCollector(report1, mode, Creator.ABIGAS.PK, Creator.ABIGAS.OH_Code);
							if (mode == ComplianceReportDataCollectionMode.SAFT)
							{
								AssertEquals(2, dataCollector.SalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK, invoice2.PK }, dataCollector.SalesInvoices.Keys);
								AssertEquals(2, dataCollector.CustomerSalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZString> { Creator.ABIGAS.OH_Code, Creator.AALSHI.OH_Code }, dataCollector.CustomerSalesInvoices.Keys);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK }, dataCollector.CustomerSalesInvoices[Creator.ABIGAS.OH_Code]);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice2.PK }, dataCollector.CustomerSalesInvoices[Creator.AALSHI.OH_Code]);

								dataCollector.MergeDataFromAnotherReport(report2);

								AssertEquals("Override", 2, dataCollector.SalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice3.PK, invoice4.PK }, dataCollector.SalesInvoices.Keys);
								AssertEquals("Merge", 3, dataCollector.CustomerSalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZString> { Creator.ABIGAS.OH_Code, Creator.AALSHI.OH_Code, Creator.ZECTRA.OH_Code }, dataCollector.CustomerSalesInvoices.Keys);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK, invoice3.PK }, dataCollector.CustomerSalesInvoices[Creator.ABIGAS.OH_Code]);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice2.PK }, dataCollector.CustomerSalesInvoices[Creator.AALSHI.OH_Code]);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice4.PK }, dataCollector.CustomerSalesInvoices[Creator.ZECTRA.OH_Code]);
							}
							else if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
							{
								AssertEquals(1, dataCollector.SalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK }, dataCollector.SalesInvoices.Keys);
								AssertEquals(1, dataCollector.CustomerSalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZString> { Creator.ABIGAS.OH_Code }, dataCollector.CustomerSalesInvoices.Keys);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK }, dataCollector.CustomerSalesInvoices[Creator.ABIGAS.OH_Code]);

								dataCollector.MergeDataFromAnotherReport(report2);

								AssertEquals("Override", 1, dataCollector.SalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice3.PK }, dataCollector.SalesInvoices.Keys);
								AssertEquals("Merge", 1, dataCollector.CustomerSalesInvoices.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZString> { Creator.ABIGAS.OH_Code }, dataCollector.CustomerSalesInvoices.Keys);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK, invoice3.PK }, dataCollector.CustomerSalesInvoices[Creator.ABIGAS.OH_Code]);
							}
							else
							{
								AssertEquals(true, dataCollector.CustomerSalesInvoices == null || dataCollector.CustomerSalesInvoices.Count == 0);
							}
						}
					}
				}
			}
		}

		public void TestPostedWithoutIVASalesInvoicePKs()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				Creator.ABIGAS.OH_RL_NKClosestPort = "PTLIS";
				Creator.AALSHI.OH_RL_NKClosestPort = "PTLIS";
				Creator.ZECTRA.OH_RL_NKClosestPort = "PTLIS";
				Creator.XLINDU.OH_RL_NKClosestPort = "PTLIS";

				Creator.CreateCustomsCodesIfNotExists(Creator.AALSHI, CountryCodes.Portugal, OrgCusCode.CodeTypes.IVA, "PT111");
				Creator.CreateCustomsCodesIfNotExists(Creator.ZECTRA, CountryCodes.Mexico, OrgCusCode.CodeTypes.IVA, "PT222");
				Creator.CreateCustomsCodesIfNotExists(Creator.XLINDU, CountryCodes.Portugal, OrgCusCode.CodeTypes.GovBusinessCode, "PT333");
				Factory.Save();

				foreach (ComplianceReportDataCollectionMode mode in Enum.GetValues(typeof(ComplianceReportDataCollectionMode)))
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(mode == ComplianceReportDataCollectionMode.SAFT1_30 ? CountryCodes.Norway : CountryCodes.Portugal))
					{
						var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
						var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
						SetComplianceReportType(report1, reportType);
						SetComplianceReportType(report2, reportType, report1.ACR_DateTo.AddDays(1), report2.ACR_DateFrom.AddMonths(1));

						Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);
						Creator.CreateConfigurationForComplianceReport(report2, tablePrefix, lineGrouping);

						var invoiceType = mode == ComplianceReportDataCollectionMode.SAFTSelfBilling ? typeof(APInvoice) : typeof(ARInvoice);

						var invoice1 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + "1", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice1.AH_OH = Creator.ABIGAS.PK;
						invoice1.AH_TransactionReference = "ref1";
						var invoice2 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + "2", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice2.AH_OH = Creator.AALSHI.PK;
						invoice2.AH_TransactionReference = "ref2";
						var invoice3 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + "3", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice3.AH_OH = Creator.ZECTRA.PK;
						invoice3.AH_TransactionReference = "ref3";
						var invoice4 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + "4", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice4.AH_OH = Creator.XLINDU.PK;
						invoice4.AH_TransactionReference = "ref4";
						var invoice5 = Creator.CreateInvoiceWithLine(invoiceType, "I000" + mode.GetHashCode() + "5", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice5.AH_OH = Creator.ABIGAS.PK;
						invoice5.AH_PostDate = invoice5.AH_InvoiceDate = report2.ACR_DateFrom;
						invoice5.AH_TransactionReference = "ref5";

						if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
						{
							invoice1.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice2.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice3.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice4.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
							invoice5.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
						}

						Factory.Save();

						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice1, 1);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice2, 2);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice3, 3);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice4, 4);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report2, invoice5, 1);

						AssertAuthorizationNumberReference();

						void AssertAuthorizationNumberReference()
						{
							var dataCollector = new SAFTAdditionalDataCollector(report1, mode, Creator.ABIGAS.PK, Creator.ABIGAS.OH_Code);
							if (mode == ComplianceReportDataCollectionMode.SAFT)
							{
								AssertEquals(4, dataCollector.HeaderData.Count);
								AssertEquals(4, dataCollector.SalesInvoices.Count);
								AssertEquals(3, dataCollector.PostedWithoutIVASalesInvoicePKs.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK, invoice3.PK, invoice4.PK }, dataCollector.PostedWithoutIVASalesInvoicePKs);

								dataCollector.MergeDataFromAnotherReport(report2);

								AssertEquals("Override", 1, dataCollector.HeaderData.Count);
								AssertEquals("Override", 1, dataCollector.SalesInvoices.Count);
								AssertEquals("Merge", 4, dataCollector.PostedWithoutIVASalesInvoicePKs.Count);
								AssertContainsExactElementsInAnyOrder(new List<ZGuid> { invoice1.PK, invoice3.PK, invoice4.PK, invoice5.PK }, dataCollector.PostedWithoutIVASalesInvoicePKs);
							}
							else if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
							{
								AssertEquals(1, dataCollector.HeaderData.Count);
								AssertEquals(1, dataCollector.SalesInvoices.Count);
								AssertEquals(0, dataCollector.PostedWithoutIVASalesInvoicePKs.Count); //PostedWithoutIVASalesInvoicePKs is only for AR transactions

								dataCollector.MergeDataFromAnotherReport(report2);

								AssertEquals("Override", 1, dataCollector.HeaderData.Count);
								AssertEquals("Override", 1, dataCollector.SalesInvoices.Count);
								AssertEquals(0, dataCollector.PostedWithoutIVASalesInvoicePKs.Count);
							}
							else
							{
								AssertEquals(true, dataCollector.PostedWithoutIVASalesInvoicePKs == null || dataCollector.PostedWithoutIVASalesInvoicePKs.Count == 0);
							}
						}
					}
				}
			}
		}

		public void TestInvoiceForAuthorizationNumberReference()
		{
			foreach (ComplianceReportDataCollectionMode mode in Enum.GetValues(typeof(ComplianceReportDataCollectionMode)))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(mode == ComplianceReportDataCollectionMode.SAFT1_30 ? CountryCodes.Norway : CountryCodes.Portugal))
				{
					foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
							(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
							(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
						})
					{
						var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
						var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
						SetComplianceReportType(report1, reportType);
						SetComplianceReportType(report2, reportType, report1.ACR_DateTo.AddDays(1), report2.ACR_DateFrom.AddMonths(1));

						Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);
						Creator.CreateConfigurationForComplianceReport(report2, tablePrefix, lineGrouping);

						var invoice1 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice1.AH_OH = Creator.ABIGAS.PK;
						invoice1.AH_TransactionReference = "ref1";
						invoice1.AuthorizationNumberReference = "ATCUD-001";
						var invoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice2.AH_OH = Creator.ABIGAS.PK;
						invoice2.AH_TransactionReference = "ref2";
						var invoice3 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0003", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice3.AH_OH = Creator.ABIGAS.PK;
						invoice3.AH_TransactionReference = "ref3";
						invoice3.SourceReference = "SourceReference";

						var invoice4 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0004", Creator.USD, 1m, 100m, 0m, 100m, 0m);
						invoice4.AH_OH = Creator.ABIGAS.PK;
						invoice4.AH_PostDate = invoice4.AH_InvoiceDate = report2.ACR_DateFrom;
						invoice4.AH_TransactionReference = "ref4";
						invoice4.AuthorizationNumberReference = "ATCUD-002";
						Factory.Save();

						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice1, 1);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice2, 2);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice3, 3);
						CreateComplianceReportTransactionPivotForPortugalSAFT(report2, invoice4, 1);

						AssertAuthorizationNumberReference(mode);

						void AssertAuthorizationNumberReference(ComplianceReportDataCollectionMode mode)
						{
							var dataCollector = new SAFTAdditionalDataCollector(report1, mode, Creator.ABIGAS.PK, Creator.ABIGAS.OH_Code);

							if (dataCollector.DataCollectionMode == ComplianceReportDataCollectionMode.SAFT)
							{
								AssertEquals(3, dataCollector.HeaderData.Count);
								AssertEquals(1, dataCollector.HeaderAuthorizationNumberReferences.Count);
								AssertEquals("ATCUD-001", dataCollector.HeaderAuthorizationNumberReferences[invoice1.PK]);

								dataCollector.MergeDataFromAnotherReport(report2);

								AssertEquals(1, dataCollector.HeaderData.Count);
								AssertEquals(1, dataCollector.HeaderAuthorizationNumberReferences.Count);
								AssertEquals("ATCUD-002", dataCollector.HeaderAuthorizationNumberReferences[invoice4.PK]);
							}
							else
							{
								AssertEquals(true, dataCollector.HeaderAuthorizationNumberReferences == null || dataCollector.HeaderAuthorizationNumberReferences.Count == 0);
							}
						}
					}
				}
			}
		}

		public void TestInvoiceForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice", 1, transactionHeaderDetails.Count);

				Assert("Should be no reversed ammendments", transactionHeaderDetails.Values.All(x => !x.IsReversedAmendment));
			}
		}

		public void TestCollectHeaderDescriptions()
		{
			TestCase(ComplianceReportDataCollectionMode.SAFT1_10);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				TestCase(ComplianceReportDataCollectionMode.SAFT1_30);
			}

			void TestCase(ComplianceReportDataCollectionMode dataCollectionMode)
			{
				foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
				{
					var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
					var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
					SetComplianceReportType(report1, reportType);
					SetComplianceReportType(report2, reportType);
					Creator.CreateConfigurationForComplianceReport(report1, tablePrefix, lineGrouping);
					Creator.CreateConfigurationForComplianceReport(report2, tablePrefix, lineGrouping);

					var description1 = "INV1";
					var description2 = "INV2";

					var invoice1 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
					invoice1.AH_OH = Creator.ABIGAS.PK;
					var invoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", Creator.USD, 2m, 200m, 0m, 100m, 0m);
					invoice2.AH_OH = Creator.ABIGAS.PK;
					invoice1.AH_Desc = description1;
					invoice2.AH_Desc = description2;

					Factory.Save();

					CreateComplianceReportTransactionPivotForPortugalSAFT(report1, invoice1, 1);
					CreateComplianceReportTransactionPivotForPortugalSAFT(report2, invoice2, 1);

					var collector = new SAFTAdditionalDataCollector(report1);
					collector.GetTransactionHeaderDetails(report1);
					AssertNull("HeaderDescriptions should be null without SAFT1_10 or SAFT1_30 mode", collector.HeaderDescriptions);

					collector = new SAFTAdditionalDataCollector(report1, dataCollectionMode);
					collector.GetTransactionHeaderDetails(report1);

					AssertEquals(1, collector.HeaderDescriptions.Count);
					AssertEquals(description1, collector.HeaderDescriptions[invoice1.PK]);

					collector.GetTransactionHeaderDetails(report2);

					AssertEquals(2, collector.HeaderDescriptions.Count);
					AssertEquals(description1, collector.HeaderDescriptions[invoice1.PK]);
					AssertEquals("The descriptions should be collected into HeaderDescriptions", description2, collector.HeaderDescriptions[invoice2.PK]);
				}
			}
		}

		[TestDate(2023, 01, 1)]
		public void TestGetAllHeadersSqlGLD_ForSAFTv1_10()
		{
			var aRInvoice = PrepareTestDataForGLD(out var collector, ComplianceReportDataCollectionMode.SAFT1_10);
			AssertEquals("The descriptions should be collected into HeaderDescriptions", aRInvoice.AH_Desc, collector.HeaderDescriptions[aRInvoice.PK]);
		}

		[TestDate(2023, 01, 1)]
		public void TestGetAllHeadersSqlGLD_ForSAFTv1_30()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				var aRInvoice = PrepareTestDataForGLD(out var collector, ComplianceReportDataCollectionMode.SAFT1_30);
				AssertEquals("The descriptions should be collected into HeaderDescriptions", aRInvoice.AH_Desc, collector.HeaderDescriptions[aRInvoice.PK]);
			}
		}

		[TestDate(2023, 01, 1)]
		public void TestGetTransactionLineDetailsSqlGLD_ForSAFTv1_10()
		{
			var aRInvoice = PrepareTestDataForGLD(out var collector, ComplianceReportDataCollectionMode.SAFT1_10);
			AssertEquals("The GLAccountTypes should be collected", "BSH", collector.GLAccountTypes[aRInvoice.Lines[0].AL_AG]);
		}

		[TestDate(2023, 01, 1)]
		public void TestGetTransactionLineDetailsSqlGLD_ForSAFTv1_30()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				var aRInvoice = PrepareTestDataForGLD(out var collector, ComplianceReportDataCollectionMode.SAFT1_30);
				AssertEquals("The GLAccountTypes should be collected", "BSH", collector.GLAccountTypes[aRInvoice.Lines[0].AL_AG]);
			}
		}

		InvoicingBase PrepareTestDataForGLD(out SAFTAdditionalDataCollector collector, ComplianceReportDataCollectionMode dataCollectionMode)
		{
			Creator.CreateTestPeriodsForEntireYear(2023);

			var testDate = new ZDateTime(2023, 01, 1);

			var aRInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "121", Creator.AUD, 1M, 10M, 10M, 10M, 10M);
			aRInvoice.Lines[0].AL_PostDate = testDate;
			aRInvoice.Lines[0].AL_ReverseDate = testDate;
			aRInvoice.AH_OH = Creator.ABIGAS.PK;
			Creator.CreateInvoiceLine(aRInvoice, GlbCompany.CurrentCompany.LocalCurrency, 1, 100, 10, 0, Creator.GLHeader1.PK);

			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Creator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			Factory.Save();

			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();
			((INeedRow)aRInvoice.Lines[1]).Row.SetAdded();

			Creator.MockNudgeGLDProcessData([aRInvoice.Lines[0], aRInvoice.Lines[1]]);

			var newFactory = Factory.CreateNewFactory();
			var report = newFactory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateConfigurationForComplianceReport(report,
				baseTablePrefix: Lookups.ReportBaseTablePrefixListCodes.GeneralLedgerData,
				reportLineGrouping: Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping,
				reportLineOrdering: Lookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();
			report.GenerateFromQueue();
			collector = new SAFTAdditionalDataCollector(report, dataCollectionMode);
			collector.GetTransactionHeaderDetails(report);
			return aRInvoice;
		}

		public void TestInvoiceWithAmendmentForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";

				var invoiceAmendment = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoiceAmendment.AH_OH = Creator.ABIGAS.PK;
				invoiceAmendment.AH_TransactionReference = "ref1";
				invoiceAmendment.AH_TransactionBelongsToGroup = invoice.PK;
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceAmendment, 2);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice and one amendment", 2, transactionHeaderDetails.Count);

				Assert("Should be no reversed ammendments", transactionHeaderDetails.Values.All(x => !x.IsReversedAmendment));
			}
		}

		public void TestReversedInvoiceForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";
				var reversingFactory = new ReversingFactory();
				var reversingBase = reversingFactory.NewReversing(invoice);
				reversingBase.Reverse();
				var invoiceReversal = invoice.ReverseInvoice;
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceReversal, 2);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice and one amendment", 2, transactionHeaderDetails.Count);

				Assert("Should be no reversed ammendments", transactionHeaderDetails.Values.All(x => !x.IsReversedAmendment));
			}
		}

		public void TestARCreditNoteReferences()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
				{
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					SetComplianceReportType(report, reportType);
					Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

					var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV001", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					invoice.AH_OH = Creator.AALSHI.PK;
					invoice.AH_TransactionReference = "ref1";
					var creditNote = Creator.CreateInvoiceWithLine(typeof(ARCreditNote), "ARCRD001", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					creditNote.AH_OH = Creator.AALSHI.PK;
					creditNote.OriginalTransactionReference = invoice.PK;
					creditNote.ReasonCode = "TXT";
					creditNote.ReasonDescription = "custom desc";

					var creditNote2 = Creator.CreateInvoiceWithLine(typeof(ARCreditNote), "ARCRD002", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					creditNote2.AH_OH = Creator.AALSHI.PK;
					creditNote2.AH_OriginalReferenceStartDate = ZDate.BrettsBirthday;
					creditNote2.AH_OriginalReferenceEndDate = ZDate.BrettsBirthday.AddDays(1);
					Factory.Save();

					var addOnColumns = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, InvoicingBase.GenAddOnColumnReasonName));
					AssertEquals(1, addOnColumns.Length);
					AssertEquals(creditNote.PK, addOnColumns[0].XA_ParentID);
					AssertEquals("TXT|custom desc", addOnColumns[0].XA_Data);

					CreateComplianceReportTransactionPivotForPortugalSAFT(report, creditNote, 1);
					CreateComplianceReportTransactionPivotForPortugalSAFT(report, creditNote2, 2);

					var transactionHeaderDetails = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT).GetTransactionHeaderDetails(report);
					AssertEquals("report type: " + reportType, 2, transactionHeaderDetails.Count);

					var crd1Details = transactionHeaderDetails.First(x => x.Key == creditNote.PK).Value;
					AssertEquals("TXT|custom desc", crd1Details.OriginalReferenceReversalReason);
					AssertEquals("ref1", crd1Details.OriginalTransactionReference);
					Assert(!crd1Details.OriginalReferenceStartDate.IsValid);
					Assert(!crd1Details.OriginalReferenceEndDate.IsValid);

					var crd2Details = transactionHeaderDetails.First(x => x.Key == creditNote2.PK).Value;
					AssertEquals("", crd2Details.OriginalReferenceReversalReason);
					AssertEquals("", crd2Details.OriginalTransactionReference);
					AssertEquals(ZDate.BrettsBirthday, crd2Details.OriginalReferenceStartDate);
					AssertEquals(ZDate.BrettsBirthday.AddDays(1), crd2Details.OriginalReferenceEndDate);

					addOnColumns.DeleteAll();
				}
			}
		}

		public void TestPaymentTerms()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
						(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
						(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
					})
				{
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					SetComplianceReportType(report, reportType);
					Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

					var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV001", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					invoice.AH_OH = Creator.AALSHI.PK;
					invoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
					invoice.AH_InvoiceTermDays = 10;
					invoice.AH_DueDate = ZDate.BrettsBirthday;
					invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;
					Factory.Save();

					CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);

					var transactionHeaderDetails = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT).GetTransactionHeaderDetails(report);
					AssertEquals(1, transactionHeaderDetails.Count);

					var invoiceDetails = transactionHeaderDetails.First().Value;
					AssertEquals("MTH", invoiceDetails.InvoiceTerm);
					AssertEquals("10", invoiceDetails.InvoiceTermDays.ToString());
					AssertEquals(ZDate.BrettsBirthday, invoiceDetails.DueDate);
					AssertEquals("CHK", invoiceDetails.AgreedPaymentMethodOverride);
				}
			}
		}

		public void TestARInvoiceReferences()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
				{
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					SetComplianceReportType(report, reportType);
					Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

					var creditNote = Creator.CreateInvoiceWithLine(typeof(ARCreditNote), "ARCRD001", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					creditNote.AH_OH = Creator.AALSHI.PK;
					creditNote.AH_TransactionReference = "ref1";
					creditNote.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCD;

					var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV001", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					invoice.AH_OH = Creator.AALSHI.PK;
					invoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LCD;
					invoice.OriginalTransactionReference = creditNote.PK;
					invoice.ReasonCode = "TXT";
					invoice.ReasonDescription = "custom desc";

					var invoice2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV002", Creator.EUR, 1m, 100m, 0m, 100m, 0m);
					invoice.AH_OH = Creator.AALSHI.PK;
					invoice2.AH_OriginalReferenceStartDate = ZDate.BrettsBirthday;
					invoice2.AH_OriginalReferenceEndDate = ZDate.BrettsBirthday.AddDays(1);
					Factory.Save();

					var addOnColumns = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, InvoicingBase.GenAddOnColumnReasonName));
					AssertEquals(1, addOnColumns.Length);
					AssertEquals(invoice.PK, addOnColumns[0].XA_ParentID);
					AssertEquals("TXT|custom desc", addOnColumns[0].XA_Data);

					CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
					CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice2, 2);

					var transactionHeaderDetails = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT).GetTransactionHeaderDetails(report);
					AssertEquals(2, transactionHeaderDetails.Count);

					var inv1Details = transactionHeaderDetails.First(x => x.Key == invoice.PK).Value;
					AssertEquals("TXT|custom desc", inv1Details.OriginalReferenceReversalReason);
					AssertEquals("ref1", inv1Details.OriginalTransactionReference);
					AssertEquals("TCD", inv1Details.OriginalReferenceComplianceSubType);
					Assert(!inv1Details.OriginalReferenceStartDate.IsValid);
					Assert(!inv1Details.OriginalReferenceEndDate.IsValid);

					var inv2Details = transactionHeaderDetails.First(x => x.Key == invoice2.PK).Value;
					AssertEquals("", inv2Details.OriginalReferenceReversalReason);
					AssertEquals("", inv2Details.OriginalTransactionReference);
					AssertEquals("", inv2Details.OriginalReferenceComplianceSubType);
					AssertEquals(ZDate.BrettsBirthday, inv2Details.OriginalReferenceStartDate);
					AssertEquals(ZDate.BrettsBirthday.AddDays(1), inv2Details.OriginalReferenceEndDate);

					addOnColumns.DeleteAll();
				}
			}
		}

		public void TestInvoiceWithReversedAmendmentForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";

				var invoiceAmendment = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoiceAmendment.AH_OH = Creator.ABIGAS.PK;
				invoiceAmendment.AH_TransactionReference = "ref1";
				invoiceAmendment.AH_TransactionBelongsToGroup = invoice.PK;
				Factory.Save();

				new ReversingFactory().NewReversing(invoiceAmendment).Reverse();
				var amendmentReversal = invoiceAmendment.ReverseInvoice;
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceAmendment, 2);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, amendmentReversal, 3);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice, one amendment and one reversal of the amendment", 3, transactionHeaderDetails.Count);

				AssertEquals("Should be one reversed ammendment", 1, transactionHeaderDetails.Values.Count(x => x.IsReversedAmendment));
			}
		}

		public void TestReversedInvoiceWithReversedAmendmentForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";

				var invoiceAmendment = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoiceAmendment.AH_OH = Creator.ABIGAS.PK;
				invoiceAmendment.AH_TransactionReference = "ref1";
				invoiceAmendment.AH_TransactionBelongsToGroup = invoice.PK;
				Factory.Save();

				new ReversingFactory().NewReversing(invoiceAmendment).Reverse();
				var amendmentReversal = invoiceAmendment.ReverseInvoice;

				new ReversingFactory().NewReversing(invoice).Reverse();
				var invoiceReversal = invoice.ReverseInvoice;
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceAmendment, 2);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, amendmentReversal, 3);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceReversal, 4);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice, one amendment, one reversal of the amendment and one reversal of the invoice", 4, transactionHeaderDetails.Count);

				AssertEquals("Should be one reversed ammendment", 1, transactionHeaderDetails.Values.Count(x => x.IsReversedAmendment));
			}
		}

		public void TestReversedInvoiceWithAmendmentForIsReversedAmendment()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoice.AH_OH = Creator.ABIGAS.PK;
				invoice.AH_TransactionReference = "ref1";
				var invoiceAmendment = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0002", Creator.USD, 1m, 100m, 0m, 100m, 0m);
				invoiceAmendment.AH_OH = Creator.ABIGAS.PK;
				invoiceAmendment.AH_TransactionReference = "ref1";
				invoiceAmendment.AH_TransactionBelongsToGroup = invoice.PK;
				Factory.Save();

				new ReversingFactory().NewReversing(invoice).Reverse();
				var invoiceReversal = invoice.ReverseInvoice;
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoice, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceAmendment, 2);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, invoiceReversal, 3);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report).GetTransactionHeaderDetails(report);
				AssertEquals("Only created one invoice, one amendment, one reversal of the amendment and one reversal of the invoice", 3, transactionHeaderDetails.Count);

				Assert("Should be no reversed ammendments", transactionHeaderDetails.Values.All(x => !x.IsReversedAmendment));
			}
		}

		[TestDate(2019, 8, 22, 12, 30, 05)]
		public void TestInclusionOfSecondsInComplianceReport()
		{
			foreach ((string reportType, string tablePrefix, string lineGrouping) in new[] {
					(AccComplianceReport.ReportTypes.SAFT, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping),
					(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping)
				})
			{
				AssertNotEquals("Pre-Condition: For the test to run correctly, seconds part shouldn't be zero", 0, TestDateAttribute.Date.Second);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				SetComplianceReportType(report, reportType);
				Creator.CreateConfigurationForComplianceReport(report, tablePrefix, lineGrouping);

				var arInv = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV01", Creator.USD, 1M, 0, 0, 0, 0);
				Factory.Save();

				TestDateAttribute.AddSeconds(5);
				var arInv2 = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "APINV02", Creator.USD, 2M, 0, 0, 0, 0);
				Factory.Save();

				CreateComplianceReportTransactionPivotForPortugalSAFT(report, arInv, 1);
				CreateComplianceReportTransactionPivotForPortugalSAFT(report, arInv2, 2);

				TestDateAttribute.AddSeconds(-5); //reverting the TestDate to it's originally set value for testing it's inclusion in HeaderDetails.HeaderData.CreateTime
				var testDate = new ZDateTime(TestDateAttribute.Date);

				var transactionHeaderDetails = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.SAFT);
				Assert("CreateTime on both Header Data should include seconds in SAFT Reports", transactionHeaderDetails.HeaderData.Values.All(x => x.CreateTime.Second != 0));
				AssertNotNull("Header Data CreateTime and TestDate should be equal on the first one in SAFT Reports", transactionHeaderDetails.HeaderData.Values.FirstOrDefault(x => x.CreateTime == testDate));

				var nonSAFTTransactionHeaderDetails = new SAFTAdditionalDataCollector(report, ComplianceReportDataCollectionMode.Esterometro);
				Assert("Header Data CreateTime for non SAFT records should be 0 as they don't include seconds.", nonSAFTTransactionHeaderDetails.HeaderData.Values.All(x => x.CreateTime.Second == 0));
				Assert("Header Data CreateTime and TestDate should not be equal in non SAFT Reports", nonSAFTTransactionHeaderDetails.HeaderData.Values.All(x => x.CreateTime != testDate));
			}
		}

		public void TestOnlyPopulatesCorrectData()
		{
			var report = SetupComplianceReportWithAllPropertiesNonDefault(Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBook);
			CombineAssertions(() =>
			{
				AssertOnlyPopulatesCorrectData_DocumentMode(report);
				AssertOnlyPopulatesCorrectData_TransactionBatchMode(report);
				AssertOnlyPopulatesCorrectData_DefaultMode(report);
				AssertOnlyPopulatesCorrectData_SAFT(report, AccComplianceReport.ReportTypes.SAFT);
				AssertOnlyPopulatesCorrectData_Esterometro(report);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
				AssertOnlyPopulatesCorrectData_DocumentMode(report);
				AssertOnlyPopulatesCorrectData_TransactionBatchMode(report);
				AssertOnlyPopulatesCorrectData_DefaultMode(report);
				AssertOnlyPopulatesCorrectData_SAFT(report, AccComplianceReport.ReportTypes.SAFT);
				AssertOnlyPopulatesCorrectData_Esterometro(report);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.TransactionHeader);
				AssertOnlyPopulatesCorrectData_TransactionBatchMode(report);
				AssertOnlyPopulatesCorrectData_DefaultMode(report);
				AssertOnlyPopulatesCorrectData_SAFT(report, AccComplianceReport.ReportTypes.SAFT);
				AssertOnlyPopulatesCorrectData_Esterometro(report);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.TransactionHeaderWithLines);
				AssertOnlyPopulatesCorrectData_TransactionBatchMode(report);
				AssertOnlyPopulatesCorrectData_DefaultMode(report);
				AssertOnlyPopulatesCorrectData_SAFT(report, AccComplianceReport.ReportTypes.SAFT);
				AssertOnlyPopulatesCorrectData_Esterometro(report);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
				AssertOnlyPopulatesCorrectData_SAFT(report, AccComplianceReport.ReportTypes.SAFTOnlyTransactions);
			});
		}

		void AssertOnlyPopulatesCorrectData_DocumentMode(AccComplianceReport report)
		{
			var collectionsToFill = new[]
			{
				nameof(SAFTAdditionalDataCollector.HeaderData)
			};

			var collectionsNotToFill = new[]
			{
				nameof(ComplianceReportAdditionalDataCollector.OrgAddresses),
				nameof(ComplianceReportAdditionalDataCollector.OrgTaxRegistrationNumberDetails),
				nameof(ComplianceReportAdditionalDataCollector.ChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.UsedChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.TaxIDs),
				nameof(ComplianceReportAdditionalDataCollector.DebitBankAccounts),
				nameof(SAFTAdditionalDataCollector.ActiveGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.WithholdingTaxIDs),
				nameof(ComplianceReportAdditionalDataCollector.OrgHeaderDetails),
				nameof(SAFTAdditionalDataCollector.CustomerCodes),
				nameof(SAFTAdditionalDataCollector.SupplierCodes),
				nameof(SAFTAdditionalDataCollector.SalesInvoices),
				nameof(SAFTAdditionalDataCollector.Payments),
				nameof(ComplianceReportAdditionalDataCollector.LineData),
				nameof(SAFTAdditionalDataCollector.UsedGLAccounts),
				nameof(SAFTAdditionalDataCollector.GLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.TaxMsgGroupCodes)
			};

			AssertOnlyPopulatesCorrectData(report, ComplianceReportDataCollectionMode.Document, collectionsToFill, collectionsNotToFill);
		}

		void AssertOnlyPopulatesCorrectData_TransactionBatchMode(AccComplianceReport report)
		{
			var collectionsToFill = new[]
			{
				nameof(SAFTAdditionalDataCollector.HeaderData),
				nameof(ComplianceReportAdditionalDataCollector.OrgAddresses),
				nameof(ComplianceReportAdditionalDataCollector.OrgTaxRegistrationNumberDetails),
				nameof(ComplianceReportAdditionalDataCollector.TaxIDs)
			};

			var collectionsNotToFill = new[]
			{
				nameof(ComplianceReportAdditionalDataCollector.OrgHeaderDetails),
				nameof(SAFTAdditionalDataCollector.ActiveGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.WithholdingTaxIDs),
				nameof(SAFTAdditionalDataCollector.CustomerCodes),
				nameof(SAFTAdditionalDataCollector.SupplierCodes),
				nameof(SAFTAdditionalDataCollector.SalesInvoices),
				nameof(ComplianceReportAdditionalDataCollector.TaxMsgGroupCodes),
				nameof(SAFTAdditionalDataCollector.GLAccounts),
				nameof(SAFTAdditionalDataCollector.Payments)
			};

			var extraPropertiesDestination = report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.TransactionHeaderWithLines || report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping
				? collectionsToFill
				: collectionsNotToFill;

			extraPropertiesDestination.Concat(new string[]
			{
				nameof(ComplianceReportAdditionalDataCollector.LineData),
				nameof(SAFTAdditionalDataCollector.UsedGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.ChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.DebitBankAccounts),
				nameof(ComplianceReportAdditionalDataCollector.UsedChargeCodes)
			});

			AssertOnlyPopulatesCorrectData(report, ComplianceReportDataCollectionMode.TransactionBatch, collectionsToFill, collectionsNotToFill);
		}

		void AssertOnlyPopulatesCorrectData_DefaultMode(AccComplianceReport report)
		{
			var collectionsToFill = new[]
			{
				nameof(SAFTAdditionalDataCollector.HeaderData),
				nameof(ComplianceReportAdditionalDataCollector.OrgAddresses),
				nameof(ComplianceReportAdditionalDataCollector.OrgTaxRegistrationNumberDetails),
				nameof(ComplianceReportAdditionalDataCollector.TaxIDs),
				nameof(ComplianceReportAdditionalDataCollector.LineData),
				nameof(SAFTAdditionalDataCollector.UsedGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.ChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.UsedChargeCodes),
			};

			var collectionsNotToFill = new[]
			{
				nameof(ComplianceReportAdditionalDataCollector.OrgHeaderDetails),
				nameof(SAFTAdditionalDataCollector.ActiveGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.WithholdingTaxIDs),
				nameof(SAFTAdditionalDataCollector.CustomerCodes),
				nameof(SAFTAdditionalDataCollector.SupplierCodes),
				nameof(SAFTAdditionalDataCollector.SalesInvoices),
				nameof(ComplianceReportAdditionalDataCollector.DebitBankAccounts),
				nameof(SAFTAdditionalDataCollector.Payments),
				nameof(SAFTAdditionalDataCollector.GLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.TaxMsgGroupCodes)
			};

			AssertOnlyPopulatesCorrectData(report, ComplianceReportDataCollectionMode.None, collectionsToFill, collectionsNotToFill);
		}

		void AssertOnlyPopulatesCorrectData_SAFT(AccComplianceReport report, string reportType)
		{
			var collectionsToFill = new List<string>()
			{
				nameof(SAFTAdditionalDataCollector.HeaderData),
				nameof(ComplianceReportAdditionalDataCollector.OrgAddresses),
				nameof(ComplianceReportAdditionalDataCollector.OrgTaxRegistrationNumberDetails),
				nameof(ComplianceReportAdditionalDataCollector.TaxIDs),
				nameof(ComplianceReportAdditionalDataCollector.LineData),
				nameof(SAFTAdditionalDataCollector.UsedGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.ChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.UsedChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.OrgHeaderDetails),
				nameof(SAFTAdditionalDataCollector.ActiveGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.WithholdingTaxIDs),
				nameof(SAFTAdditionalDataCollector.CustomerCodes),
				nameof(SAFTAdditionalDataCollector.SupplierCodes),
				nameof(SAFTAdditionalDataCollector.SalesInvoices),
				nameof(SAFTAdditionalDataCollector.Payments),
			};

			if (reportType == AccComplianceReport.ReportTypes.SAFT)
			{
				collectionsToFill.Add(nameof(SAFTAdditionalDataCollector.GLAccounts));
			}

			var collectionsNotToFill = new[]
			{
				nameof(ComplianceReportAdditionalDataCollector.DebitBankAccounts),
				nameof(ComplianceReportAdditionalDataCollector.TaxMsgGroupCodes)
			};

			AssertOnlyPopulatesCorrectData(report, ComplianceReportDataCollectionMode.SAFT, collectionsToFill.ToArray(), collectionsNotToFill);
		}

		void AssertOnlyPopulatesCorrectData_Esterometro(AccComplianceReport report)
		{
			var collectionsToFill = new[]
			{
				nameof(SAFTAdditionalDataCollector.HeaderData),
				nameof(ComplianceReportAdditionalDataCollector.OrgAddresses),
				nameof(ComplianceReportAdditionalDataCollector.OrgTaxRegistrationNumberDetails),
				nameof(ComplianceReportAdditionalDataCollector.TaxIDs),
				nameof(ComplianceReportAdditionalDataCollector.LineData),
				nameof(SAFTAdditionalDataCollector.UsedGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.ChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.UsedChargeCodes),
				nameof(ComplianceReportAdditionalDataCollector.TaxMsgGroupCodes)
			};

			var collectionsNotToFill = new[]
			{
				nameof(ComplianceReportAdditionalDataCollector.OrgHeaderDetails),
				nameof(SAFTAdditionalDataCollector.ActiveGLAccounts),
				nameof(ComplianceReportAdditionalDataCollector.WithholdingTaxIDs),
				nameof(SAFTAdditionalDataCollector.CustomerCodes),
				nameof(SAFTAdditionalDataCollector.SupplierCodes),
				nameof(SAFTAdditionalDataCollector.SalesInvoices),
				nameof(ComplianceReportAdditionalDataCollector.DebitBankAccounts),
				nameof(SAFTAdditionalDataCollector.GLAccounts),
				nameof(SAFTAdditionalDataCollector.Payments)
			};

			AssertOnlyPopulatesCorrectData(report, ComplianceReportDataCollectionMode.Esterometro, collectionsToFill, collectionsNotToFill);
		}

		void AssertOnlyPopulatesCorrectData(AccComplianceReport report, ComplianceReportDataCollectionMode mode, string[] collectionsToFill, string[] collectionsNotToFill)
		{
			var collector = new SAFTAdditionalDataCollector(report, mode);

			var collectorProperties = typeof(ComplianceReportAdditionalDataCollector).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			foreach (var property in collectorProperties.Where(x => collectionsToFill.Contains(x.Name)))
			{
				AssertNotNull($"collector should attempt to fill {property.Name} for configuration:\r\nmode: {mode.ToString()}\r\ngrouping: {report.ReportLineGrouping}",
					property.GetValue(collector));
			}
			foreach (var property in collectorProperties.Where(x => collectionsNotToFill.Contains(x.Name)))
			{
				AssertNull($"collector should not attempt to fill {property.Name} for configuration:\r\nmode: {mode.ToString()}\r\ngrouping: {report.ReportLineGrouping}",
					property.GetValue(collector));
			}
		}

		public void TestFillsHeaderDataWithCorrectFields()
		{
			var report = SetupComplianceReportWithAllPropertiesNonDefault(Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBook);
			CombineAssertions(() =>
			{
				AssertFillHeaderDataCorrectFields_MinDetails(report, ComplianceReportDataCollectionMode.Document);
				AssertFillHeaderDataCorrectFields_MinDetails(report, ComplianceReportDataCollectionMode.TransactionBatch);
				AssertFillHeaderDataCorrectFields_NoDetails(report, ComplianceReportDataCollectionMode.None);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
				AssertFillHeaderDataCorrectFields_MinDetails(report, ComplianceReportDataCollectionMode.Document);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.TransactionBatch);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.None);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.TransactionHeader);
				AssertFillHeaderDataCorrectFields_NoDetails(report, ComplianceReportDataCollectionMode.TransactionBatch);
				AssertFillHeaderDataCorrectFields_NoDetails(report, ComplianceReportDataCollectionMode.None);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.TransactionHeaderWithLines);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.TransactionBatch);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.None);

				SetComplianceConfigurationWithGrouping(report, Lookups.ReportBaseTablePrefixListCodes.TransactionLine, Lookups.ReportLineGroupingListCodes.NoGrouping);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.TransactionBatch);
				AssertFillHeaderDataCorrectFields_AllDetails(report, ComplianceReportDataCollectionMode.None);
			});
		}

		void AssertFillHeaderDataCorrectFields_NoDetails(AccComplianceReport report, ComplianceReportDataCollectionMode mode)
		{
			AssertHeaderDataFillsCorrectFields(report, mode, expectDetailsUnfilled: true);
		}

		void AssertFillHeaderDataCorrectFields_MinDetails(AccComplianceReport report, ComplianceReportDataCollectionMode mode)
		{
			var documentFilledProperties = new[]
			{
				nameof(TransactionHeaderDetails.HeaderSequence),
				nameof(TransactionHeaderDetails.Description),
				nameof(TransactionHeaderDetails.CreateTime),
				nameof(TransactionHeaderDetails.CreateUserCode),
				nameof(TransactionHeaderDetails.CreateUserName)
			};

			AssertHeaderDataFillsCorrectFields(report, mode, documentFilledProperties);
		}

		List<string> GetNotPopulatedHeaderDataFieldNames(ComplianceReportDataCollectionMode mode)
		{
			var result = new List<string>();

			if (mode != ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				result.Add(nameof(TransactionHeaderDetails.IsSelfBilling));

				if (mode != ComplianceReportDataCollectionMode.SAFT)
				{
					result.Add(nameof(TransactionHeaderDetails.InvoiceTerm));
					result.Add(nameof(TransactionHeaderDetails.InvoiceTermDays));
					result.Add(nameof(TransactionHeaderDetails.SourceReference));
					result.Add(nameof(TransactionHeaderDetails.OriginalReferenceReversalReason));
					result.Add(nameof(TransactionHeaderDetails.OriginalReferenceStartDate));
					result.Add(nameof(TransactionHeaderDetails.OriginalReferenceEndDate));
					result.Add(nameof(TransactionHeaderDetails.DueDate));
					result.Add(nameof(TransactionHeaderDetails.AgreedPaymentMethodOverride));
					result.Add(nameof(TransactionHeaderDetails.OriginalReferenceSourceReference));
					result.Add(nameof(TransactionHeaderDetails.OriginalReferenceComplianceSubType));
				}
			}

			return result;
		}

		void AssertFillHeaderDataCorrectFields_AllDetails(AccComplianceReport report, ComplianceReportDataCollectionMode mode)
		{
			var excludedProperties = GetNotPopulatedHeaderDataFieldNames(mode);
			var documentFilledProperties = typeof(TransactionHeaderDetails).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(x => x.Name).Except(excludedProperties).ToArray();
			AssertHeaderDataFillsCorrectFields(report, mode, documentFilledProperties);
		}

		void AssertHeaderDataFillsCorrectFields(AccComplianceReport report, ComplianceReportDataCollectionMode mode, string[] documentFilledProperties = null, bool expectDetailsUnfilled = false)
		{
			var headerPKs = report.ReportLines.Where(x => !x.AH_PK.IsEmpty).Select(x => x.AH_PK).Distinct().ToArray();
			var collector = new SAFTAdditionalDataCollector(report, mode);
			var allHeaderDetails = headerPKs.Select(x => collector.GetTransactionHeader(x)).Where(x => x != null);
			if (expectDetailsUnfilled)
			{
				Assert("Header details should not be filled", !allHeaderDetails.Any());
			}
			else
			{
				var properties = typeof(TransactionHeaderDetails).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				foreach (var property in properties)
				{
					Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
					AssertNotNull($"Prerequisite: all properties are ZTypes, TransactionHeaderDetails.{property.Name} is not", propertyType.GetInterface(nameof(IZType)));
				}

				foreach (var property in properties)
				{
					AssertEquals($"If not meant to be filled, property should be default value\r\n{property.Name} does not match this for the configuration\r\nmode: {mode.ToString()}\r\ngrouping: {report.ReportLineGrouping}",
						documentFilledProperties.Contains(property.Name),
						allHeaderDetails.Any(x => !((IZType)property.GetValue(x))?.IsDefault ?? false));
				}
			}
		}

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;

		void SetComplianceConfigurationWithGrouping(AccComplianceReport report, string reportBaseTablePrefix, ZString lineGrouping)
		{
			Creator.EnsureComplianceReportConfigInRegistry(report.ACR_ReportType, new OrgCodeLists().CustomsCodes_List(report.Company.Country).ToArray()[0].Code, report.ACR_Periodicity, reportBaseTablePrefix, lineGrouping);
		}

		void SetComplianceReportType(AccComplianceReport report, string reportType, ZDate? dateFrom = null, ZDate? dateTo = null)
		{
			report.ACR_ReportType = reportType;
			report.ACR_DateFrom = dateFrom ?? ZDate.Today.AddDays(-7);
			report.ACR_DateTo = dateTo ?? ZDate.Today;
		}

		void CreateComplianceReportTransactionPivotForPortugalSAFT(AccComplianceReport report, InvoicingBase transaction, int sequence = 1)
		{
			if (report.ACR_ReportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
			{
				foreach (InvoicingLineBase line in transaction.Lines)
				{
					Creator.CreateComplianceReportTransactionPivot(report, line, sequence);
				}
			}
			else
			{
				Creator.CreateComplianceReportTransactionPivot(report, transaction, sequence);
			}
		}

		AccComplianceReport SetupComplianceReportWithAllPropertiesNonDefault(string reportBaseTablePrefix, string lineGrouping)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_Code = "NBR";
			branch.GB_BranchName = "New Branch";
			var orgGBHeader = Factory.NewWithValidTestData<OrgHeader>();
			var companyGBData = Factory.NewWithValidTestData<OrgCompanyData>();
			branch.GB_OH_OrgProxy = orgGBHeader.PK;
			companyGBData.OB_OH = orgGBHeader.PK;

			Factory.Save();
			AccComplianceReport report;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var invoice = Creator.CreateAPInvoice<APInvoice>("I0002", Creator.AUD, 1m, 100m, 10m, 1m, 100m, 10m, 1m, orgGBHeader);
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
				invoice.AH_ReceiptType = ReceiptTypes.Cash;
				invoice.AH_Desc = "New AP Invoice";
				invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice.AH_ExchangeRate = 2;
				invoice.AH_DigitalSignature_COMPRESSED = new ZBlob(new byte[] { 0, 1, 2, 3 });
				invoice.AH_InvoiceDate = ZDateTime.Now;
				invoice.AH_ChequeOrReference = "chequeRef1";
				invoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromMonthEnd;
				invoice.AH_InvoiceTermDays = 10;
				invoice.SourceReference = "FTM a/465";
				invoice.AH_OriginalReferenceStartDate = ZDate.BrettsBirthday;
				invoice.AH_OriginalReferenceEndDate = ZDate.BrettsBirthday;
				invoice.AH_DueDate = ZDate.BrettsBirthday;
				invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck;

				var line = invoice.Lines[0];
				var firstGroupPK = ZGuid.NewZGuid();
				var taxGroupSQL = $"insert into AccInvMsg (A9_PK, A9_Code, A9_SystemCreateTimeUtc, A9_SystemCreateUser, A9_SystemLastEditTimeUtc, A9_SystemLastEditUser) values ('{firstGroupPK.ToString()}', '111', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				Db.Connection.ExecuteNonQuery(taxGroupSQL);
				line.AL_A9_VATClass = firstGroupPK;
				line.AL_AG = Creator.GLHeader1.PK;
				line.AL_AT = Creator.VATSPV.PK;
				line.AL_LocalExTaxAmount = 100;
				line.AL_OSExTaxAmount = 200;
				line.AL_Desc = "New AP Invoice Line";
				line.AL_TaxDate = ZDate.Today;
				line.AL_ExchangeRate = 2;
				line.AL_AC = Creator.NonAccrualChargeCode.PK;
				line.AL_AG = Creator.GLHeader1.PK;
				line.AL_AW = Creator.WHT1.PK;
				line.AL_WithholdingTax = 500;

				var addOnColumn = Factory.New<GenAddOnColumn>();
				addOnColumn.XA_ParentID = invoice.PK;
				addOnColumn.XA_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
				addOnColumn.XA_Name = ARCreditNote.GenAddOnColumnReasonName;
				addOnColumn.XA_Type = AddOnColumnDataType.Codes.String;
				addOnColumn.XA_Data = "TXT|reason description";

				var accountDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
				accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				accountDetails.A1_IsDefaultAccount = true;
				accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
				accountDetails.A1_OB = companyGBData.PK;
				var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				bankAccount.AB_AccountNum = "12345654321";
				bankAccount.AB_RN_NKBankAccountCountry = accountDetails.A1_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				bankAccount.AB_BankName = accountDetails.A1_BankName = "Some New Bank Name";
				bankAccount.AB_BankAccountName = accountDetails.A1_BankAccount = "Some new Account name";
				invoice.AH_AB = bankAccount.PK;

				var newUser = Factory.NewWithValidTestData<GlbStaff>();
				newUser.GS_Code = invoice.AH_SystemCreateUser = invoice.AH_SystemLastEditUser = "NU1";
				newUser.GS_FullName = "new user";

				var groupInvoice = Factory.NewWithValidTestData<APCreditNote>();
				groupInvoice.AH_TransactionNum = "01010101";
				groupInvoice.AH_TransactionReference = "txnRef1";
				groupInvoice.SourceReference = "FTM a/789";
				groupInvoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				groupInvoice.AH_TransactionBelongsToGroup = invoice.PK;
				invoice.AH_TransactionBelongsToGroup = groupInvoice.PK;

				var apPayment = Creator.CreateAndMatchAPPaymentForAPInvoice(invoice, invoice.AH_PostDate);
				apPayment.AH_AB = bankAccount.PK;
				apPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				apPayment.AH_ExchangeRate = 2m;
				apPayment.AH_OutstandingAmount = 0m;

				report = Factory.NewWithValidTestData<AccComplianceReport>();
				Factory.Save();

				var illegalSetupSQL = $@"update AccTransactionHeader set AH_IsCancelled = 1, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = '~BP' where AH_PK IN('{invoice.PK}', '{apPayment.PK}', '{groupInvoice.PK}')";
				Db.Connection.ExecuteNonQuery(illegalSetupSQL);

				Creator.CreateComplianceReportTransactionPivot(report, line);
				Creator.CreateComplianceReportTransactionPivot(report, apPayment);
				SetComplianceConfigurationWithGrouping(report, reportBaseTablePrefix, lineGrouping);
				Factory.Save();
				var a = new BusinessObjectFactory().Load<AccTransactionHeader>(invoice.PK);
				var rLine = report.ReportLines[0];
			}
			return report;
		}
	}
}

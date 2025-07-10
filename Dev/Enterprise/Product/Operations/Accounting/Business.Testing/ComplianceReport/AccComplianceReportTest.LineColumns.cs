using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	public partial class AccComplianceReportTest
	{
		public void TestLineColumns_ConfigurableReports()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			var creator = new TestObjectCreator(Factory);
			var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry("TST", "ABN", ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.TransactionHeader);
			creator.CreateConfigurationSettingsForComplianceReport(reportConfiguration, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice
				, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAnAmountOfTax
				, disbursementRule: DisbursementRuleCodes.AllTransactions
				, originalRule: OriginalRuleCodes.AllTransactions);
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			reportConfigurations.Add(reportConfiguration);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
			var report = creator.CreateComplianceReport("TST", AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.AccountingPeriod);
			Factory.Save();
			creator.CreateComplianceReportTransactionPivot(report, invoice);
			Factory.Save();

			AssertReportCapabilities(report);
			AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
		}

		public void TestLineColumns_DayBookReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AG_AccountNum,
				AccComplianceReportLine.Schema.AG_Description,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.GLAccountPK,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			var creator = new TestObjectCreator(Factory);
			var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry("TST", "ABN", ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBook);
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			reportConfigurations.Add(reportConfiguration);
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations))
			{
				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateComplianceReport("TST", AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.AccountingPeriod);
				Factory.Save();
				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				AssertReportCapabilities(report, supportsDayBook: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_ComplianceDocumentReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var creator = new TestObjectCreator(Factory);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.TWD, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.TWD, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var complianceDocument = new ComplianceDocumentCreator(new[] { invoice }, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords().FirstOrDefault();
				AssertNotNull(complianceDocument);
				complianceDocument.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI;
				complianceDocument.ADH_DocumentNumber = "001";
				Factory.Save();

				AssertEquals("compliance document has lines", 1, complianceDocument.ComplianceDocumentLines.Count);
				AssertEquals("compliance document has lines", 1, complianceDocument.OriginalComplianceDocumentLines.Count);

				var report = creator.CreateComplianceReport("TXT", AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, isComplianceDocumentHeaderReport: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_TransactionPaymentsReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			var creator = new TestObjectCreator(Factory);
			var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry("TST", "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.TransactionPayments);

			var reportConfigurations = new ComplianceReportConfigurationCollection();
			reportConfigurations.Add(reportConfiguration);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

			var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
			var report = creator.CreateComplianceReport("TST", AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
			Factory.Save();
			creator.CreateComplianceReportTransactionPivot(report, invoice);
			Factory.Save();

			AssertReportCapabilities(report, isPaymentReportGrouping: true);
			AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
		}

		public void TestLineColumns_PaymentTimesSmallBusinessReportable()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.PreCalculatedAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);

				var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				AssertReportCapabilities(report, isPaymentReportGrouping: true, isPaymentTimesReportGrouping: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_PaymentTimesAllReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.PreCalculatedAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPaymentsReportType, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				AssertReportCapabilities(report, isPaymentReportGrouping: true, isPaymentTimesReportGrouping: true, isPaymentTimesAllReportGouping: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_EsterometroReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.EUR, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, supportsEsterometro: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_LiquidazioneIVAReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.LiquidazioneIVA, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.EUR, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.LiquidazioneIVA, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, supportsLiquidazioneIVA: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_PT_SAFTReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AG_AccountNum,
				AccComplianceReportLine.Schema.AG_Description,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.GLAccountPK,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
				AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.EUR, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.AccountingPeriod);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoice);
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, supportsDayBook: true, supportsSAFT: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_PT_SAFTOnlyAR_Report()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TransactionHeaderWithLines);

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.EUR, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.AccountingPeriod);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoice);
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, supportsDayBook: false, supportsSAFT: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_MTDReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(AccountingConstants.ComplianceReportTypes.MakeTaxDigitalReportType, OrgCusCode.CodeTypes.VATCode, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
				reportConfiguration.IncludeQueuedForPreviousPeriod = true;
				var setting = reportConfiguration.Settings.AddNew();
				setting.LedgerType = LedgerTypes.AccountsReceivable;
				setting.InvoiceType = TransactionTypes.Invoice;
				setting.OriginalRule = OriginalRuleCodes.AllTransactions;
				setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.GBP, 1m, creator.ABIGAS);
				var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.GBP, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = creator.GST1.PK;

				var report = creator.CreateComplianceReport(ComplianceReportTypes.MakeTaxDigitalReportType, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();

				AssertReportCapabilities(report, supportsMTD: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_TPARReport()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);
				var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateComplianceReport(ComplianceReportTypes.TaxablePaymentsAnnualReportType, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.FinancialYear);
				Factory.Save();
				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				AssertReportCapabilities(report, isPaymentReportGrouping: true, supportsTPAR: true);
				AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
			}
		}

		public void TestLineColumns_U11Germany()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var germanyReports = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.Value;
				germanyReports.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
				using (AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, germanyReports))
				{
					var creator = new TestObjectCreator(Factory);
					var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.EUR, 1m, Creator.ABIGAS);
					var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.GLHeader1.PK);
					invoiceLine.AL_AT = creator.GST1.PK;

					var report = creator.CreateComplianceReport("U11", AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
					Factory.Save();
					creator.CreateComplianceReportTransactionPivot(report, invoiceLine);
					Factory.Save();

					AssertReportCapabilities(report);
					AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
				}
			}
		}

		public void TestLineColumns_UVAGermany()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var germanyReports = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.Value;
				germanyReports.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
				using (AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, germanyReports))
				{
					var creator = new TestObjectCreator(Factory);
					var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.EUR, 1m, Creator.ABIGAS);
					var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.GLHeader1.PK);
					invoiceLine.AL_AT = creator.GST1.PK;

					var report = creator.CreateComplianceReport("UVA", AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
					Factory.Save();
					creator.CreateComplianceReportTransactionPivot(report, invoiceLine);
					Factory.Save();

					AssertReportCapabilities(report);
					AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
				}
			}
		}

		public void TestLineColumns_ZMGermany()
		{
			var expectedLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLine.Schema.ACL_ReportSequence,
				AccComplianceReportLine.Schema.OH_Code,
				AccComplianceReportLine.Schema.OH_FullName,
				AccComplianceReportLine.Schema.OrgCountryCode,
				AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
				AccComplianceReportLine.Schema.AH_PK,
				AccComplianceReportLine.Schema.AH_Ledger,
				AccComplianceReportLine.Schema.AH_TransactionType,
				AccComplianceReportLine.Schema.PostDate,
				AccComplianceReportLine.Schema.InvoiceDate,
				AccComplianceReportLine.Schema.AH_TransactionNum,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.AH_InvoiceAmount,
				AccComplianceReportLine.Schema.AH_GSTAmount,
				AccComplianceReportLine.Schema.SPVTaxAmount,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.AL_A9_VATClass,
				AccComplianceReportLine.Schema.AL_TaxRate,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLine.Schema.TaxGroupCode,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
			};

			var expectedTotalsLineColumnNames = new[]
			{
				AccComplianceReportLineBase.Schema.PK,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
				AccComplianceReportLineBase.Schema.Comment,
				AccComplianceReportLineBase.Schema.LineNum,
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var germanyReports = AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.Value;
				germanyReports.Cast<CodeDescriptionBool>().ForEach(x => x.Bool = true);
				using (AccountingMasterFilesRegistry.Instance.VisibleGermanComplianceReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, germanyReports))
				{
					var creator = new TestObjectCreator(Factory);
					var invoice = creator.CreateInvoice(typeof(APInvoice), "I0001", creator.EUR, 1m, Creator.ABIGAS);
					var invoiceLine = Creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.GLHeader1.PK);
					invoiceLine.AL_AT = creator.GST1.PK;

					var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.ZMGermany, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
					Factory.Save();
					creator.CreateComplianceReportTransactionPivot(report, invoiceLine);
					Factory.Save();

					AssertReportCapabilities(report, supportsZMGermany: true);
					AssertExpectedCollectionColumns(report, expectedLineColumnNames, expectedTotalsLineColumnNames);
				}
			}
		}

		public void TestLineColumnsToBeExcluded_UsingGLD()
		{
			var expectedLineColumnNamesToBeExcluded = new[]
			{
				AccComplianceReportLine.Schema.AT_Code,
				AccComplianceReportLine.Schema.AT_Type,
				AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
				AccComplianceReportLineBase.Schema.ServiceTaxAmount,
				AccComplianceReportLineBase.Schema.TotalExTaxAmount,
				AccComplianceReportLineBase.Schema.TotalTaxAmount,
				AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
				AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
				AccComplianceReportLine.Schema.AH_TransactionReference,
				AccComplianceReportLine.Schema.TaxMessage,
				AccComplianceReportLine.Schema.GB_Code,
				AccComplianceReportLine.Schema.GE_Code,
				AccComplianceReportLine.Schema.OK_CustomsRegNo,
				AccComplianceReportLine.Schema.ReportSubCode,
				AccComplianceReportLine.Schema.AH_ComplianceSubType,
				AccComplianceReportLine.Schema.ComplianceSequence,
				AccComplianceReportLineBase.Schema.GoodsTaxAmount,
				AccComplianceReportLine.Schema.RepCountryRegNo,
				AccComplianceReportLine.Schema.RepCountryRegNo,
			};

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateConfigurationForComplianceReport(report, baseTablePrefix: ReportBaseTablePrefixListCodes.GeneralLedgerData, reportLineGrouping: ReportLineGroupingListCodes.DayBookWithoutGrouping, reportLineOrdering: ReportLineOrderingListCodes.ComplianceSubType);
			Factory.Save();
			var actualLineColumnNamesToBeExcluded = report.GetColumnNamesToExclude();
			Assert(expectedLineColumnNamesToBeExcluded.All(x => actualLineColumnNamesToBeExcluded.Contains(x)));
		}

		public void TestLineColumnsToBeExcluded_IncludesPreCalculatedProperties()
		{
			var expectedLineColumnNamesToBeExcluded = new[]
			{
				nameof(AccComplianceReportLine.PreCalculatedCount),
				nameof(AccComplianceReportLine.PreCalculatedCount2),
				nameof(AccComplianceReportLine.PreCalculatedCount3),
			};

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.Instance.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var creator = new TestObjectCreator(Factory);

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.AUD, 1m, Creator.ABIGAS);
				var report = creator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPaymentsReportType, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.DateRange);
				Factory.Save();
				creator.CreateComplianceReportTransactionPivot(report, invoice);
				Factory.Save();

				AssertReportCapabilities(report, isPaymentReportGrouping: true, isPaymentTimesReportGrouping: true, isPaymentTimesAllReportGouping: true);

				var actualLineColumnNamesToBeExcluded = report.GetColumnNamesToExclude(true);
				Assert(expectedLineColumnNamesToBeExcluded.All(x => actualLineColumnNamesToBeExcluded.Contains(x)));
			}
		}

		void AssertReportCapabilities(AccComplianceReport report
			, bool supportsDayBook = false
			, bool isComplianceDocumentHeaderReport = false
			, bool isPaymentReportGrouping = false
			, bool isPaymentTimesReportGrouping = false
			, bool isPaymentTimesAllReportGouping = false
			, bool supportsEsterometro = false
			, bool supportsLiquidazioneIVA = false
			, bool supportsSAFT = false
			, bool supportsMTD = false
			, bool supportsTPAR = false
			, bool supportsZMGermany = false
			, bool supportsIDEA = false)
		{
			AssertEquals("SupportsDayBook", supportsDayBook, report.SupportsDayBook);
			AssertEquals("IsComplianceDocumentHeaderReport", isComplianceDocumentHeaderReport, report.IsComplianceDocumentHeaderReport);
			AssertEquals("isPaymentReportGrouping", isPaymentReportGrouping, report.IsPaymentReportGrouping);
			AssertEquals("IsPaymentTimesReportGrouping", isPaymentTimesReportGrouping, report.IsPaymentTimesReportGrouping);
			AssertEquals("IsPaymentTimesAllReportGouping", isPaymentTimesAllReportGouping, report.IsPaymentTimesAllReportGouping);
			AssertEquals("SupportsEsterometro", supportsEsterometro, report.SupportsEsterometro);
			AssertEquals("SupportsLiquidazioneIVA", supportsLiquidazioneIVA, report.SupportsLiquidazioneIVA);
			AssertEquals("SupportsSAFT", supportsSAFT, report.SupportsSAFT);
			AssertEquals("SupportsMTD", supportsMTD, report.SupportsMTD);
			AssertEquals("SupportsTPAR", supportsTPAR, report.SupportsTPAR);
			AssertEquals("SupportsZMGermany", supportsZMGermany, report.SupportsZMGermany);
			AssertEquals("SupportsIDEA", supportsIDEA, report.SupportsIDEA);
		}

		void AssertExpectedCollectionColumns(AccComplianceReport report, string[] expectedLineColumnNames, string[] expectedTotalsLineColumnNames)
		{
			Assert("Has ReportLines", report.ReportLines.Any());
			var lineColumns = (report.ReportLines[0] as INeedRow)?.Row.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
			AssertContainsExactElementsInAnyOrder("ReportLines", expectedLineColumnNames, lineColumns);

			Assert("Has ReportTotals", report.ReportTotals.Any());
			var totalsLineColumns = (report.ReportTotals[0] as INeedRow)?.Row.Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
			AssertContainsExactElementsInAnyOrder("ReportTotals", expectedTotalsLineColumnNames, totalsLineColumns);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Business.Testing
{
	public class ComplianceReportTransactionQueuerTest : TestCaseWithFactory
	{
		public void TestQueueForComplianceReport_TransactionHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				var postDate = ZDateTime.Now;
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK
					, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));

				Factory.Save();

				ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.Constants.Prefix, CreateTaxRegistration().Code);

				transaction.QueueForComplianceReports();

				var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
				AssertEquals("Queue records added", 1, reportQueuedHeaders.Count);

				AssertQueueForReport(reportQueuedHeaders, "ABC", AccTransactionHeaderSchema.Constants.Prefix, queueDate: postDate.Date);
			}
		}

		#region ReportingDate options for TransactionHeader

		public void TestQueueForComplianceReport_TransactionHeader_PostDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionHeader(ReportingDateCodes.PostDate, baseDate: baseDate, expectedQueueDate: baseDate.Date);
		}

		public void TestQueueForComplianceReport_TransactionHeader_InvoiceDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionHeader(ReportingDateCodes.InvoiceDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(1).Date);
		}

		public void TestQueueForComplianceReport_TransactionHeader_ReceivedDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionHeader(ReportingDateCodes.DocumentReceivedDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(-1).Date);
		}

		public void TestQueueForComplianceReport_TransactionHeader_EarliestTaxDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionHeader(ReportingDateCodes.EarliestTaxDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(-2).Date);
		}

		public void TestQueueForComplianceReport_TransactionHeader_LatestTaxDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionHeader(ReportingDateCodes.LatestTaxDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(2).Date);
		}

		void AssertQueueForComplianceReport_TransactionHeader(string reportingDate, ZDateTime baseDate, ZDate expectedQueueDate)
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			Factory.Save();

			var transaction = CreateMockTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionLineTypes.Cost, TaxRate.PK, addLine: false
				, postDate: baseDate, invoiceDate: baseDate.AddDays(1), receivedDate: baseDate.AddDays(-1));

			var line1 = AddLine(transaction
				, ObjectCreator.CC1
				, TransactionLineTypes.Cost
				, TaxRate.PK
				, 1450
				, 145
				, job.PK
				, taxDate: baseDate.AddDays(2).Date);
			var line2 = AddLine(transaction
				, ObjectCreator.CC2
				, TransactionLineTypes.Cost
				, TaxRate.PK
				, 2450
				, 245
				, job.PK
				, taxDate: baseDate.AddDays(-2).Date);

			UpdateHeaderTotals(transaction);
			Factory.Save();

			ConfigureRegistry(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, AccTransactionHeaderSchema.Constants.Prefix, CreateTaxRegistration().Code, reportingDate: reportingDate);

			transaction.QueueForComplianceReports();

			var reportQueues = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
			AssertEquals("Queue record for Header added", 1, reportQueues.Count);

			AssertQueueForReport(reportQueues, "ABC", AccTransactionHeaderSchema.Constants.Prefix, expectedQueueDate);
		}

		#endregion

		public void TestQueueForComplianceReport_TransactionLine()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				var postDate = ZDateTime.Now;
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK
					, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));

				Factory.Save();

				ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionLinesSchema.Constants.Prefix, CreateTaxRegistration().Code);

				transaction.QueueForComplianceReports();

				var reportQueuedLine = ObjectCreator.LoadReportQueuedLinesByHeaderPK(transaction.PK);
				AssertEquals("Queue records added", 1, reportQueuedLine.Count);

				AssertQueueForReport(reportQueuedLine, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date);
			}
		}

		#region ReportingDate options for TransactionLine

		public void TestQueueForComplianceReport_TransactionLine_PostDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionLine(ReportingDateCodes.PostDate, baseDate: baseDate, expectedQueueDate: baseDate.Date);
		}

		public void TestQueueForComplianceReport_TransactionLine_InvoiceDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionLine(ReportingDateCodes.InvoiceDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(1).Date);
		}

		public void TestQueueForComplianceReport_TransactionLine_ReceivedDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionLine(ReportingDateCodes.DocumentReceivedDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(-1).Date);
		}

		public void TestQueueForComplianceReport_TransactionLine_EarliestTaxDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionLine(ReportingDateCodes.EarliestTaxDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(-2).Date);
		}

		public void TestQueueForComplianceReport_TransactionLine_LatestTaxDate()
		{
			var baseDate = ZDateTime.Now;
			AssertQueueForComplianceReport_TransactionLine(ReportingDateCodes.LatestTaxDate, baseDate: baseDate, expectedQueueDate: baseDate.AddDays(2).Date);
		}

		void AssertQueueForComplianceReport_TransactionLine(string reportingDate, ZDateTime baseDate, ZDate expectedQueueDate)
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			Factory.Save();

			var transaction = CreateMockTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, TransactionLineTypes.Cost, TaxRate.PK, addLine: false
				, postDate: baseDate, invoiceDate: baseDate.AddDays(1), receivedDate: baseDate.AddDays(-1));

			var line1 = AddLine(transaction
				, ObjectCreator.CC1
				, TransactionLineTypes.Cost
				, TaxRate.PK
				, 1450
				, 145
				, job.PK
				, taxDate: baseDate.AddDays(2).Date);
			var line2 = AddLine(transaction
				, ObjectCreator.CC2
				, TransactionLineTypes.Cost
				, TaxRate.PK
				, 2450
				, 245
				, job.PK
				, taxDate: baseDate.AddDays(-2).Date);
			var line3 = AddLine(transaction
				, ObjectCreator.CommentChargeCode
				, TransactionLineTypes.Cost
				, TaxRate.PK
				, 250
				, 25
				, job.PK
				, taxDate: baseDate.Date);

			UpdateHeaderTotals(transaction);
			Factory.Save();

			ConfigureRegistry(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, AccTransactionLinesSchema.Constants.Prefix, CreateTaxRegistration().Code, reportingDate: reportingDate);

			transaction.QueueForComplianceReports();

			var reportQueues = ObjectCreator.LoadReportQueuesByParentID(line1.PK);
			AssertEquals("Queue record for line1 added", 1, reportQueues.Count);

			AssertQueueForReport(reportQueues, "ABC", AccTransactionLinesSchema.Constants.Prefix, expectedQueueDate);

			reportQueues = ObjectCreator.LoadReportQueuesByParentID(line2.PK);
			AssertEquals("Queue record for line2 added", 1, reportQueues.Count);

			AssertQueueForReport(reportQueues, "ABC", AccTransactionLinesSchema.Constants.Prefix, expectedQueueDate);

			reportQueues = ObjectCreator.LoadReportQueuesByParentID(line3.PK);
			AssertEquals("Queue record for comment line should not be added", 0, reportQueues.Count);
		}

		#endregion

		public void TestQueueForComplianceReport_TransactionLine_Italy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				Factory.Save();

				var expectedValueMapping = new Dictionary<ZString, ZString>();

				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsReceivable, AccTaxRate.Types.Rated), "BL003");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsReceivable, AccTaxRate.Types.CapitalRated), "BL003");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsReceivable, AccTaxRate.Types.Exempt), "BL003");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsReceivable, ZString.Empty), "BL004");

				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsPayable, AccTaxRate.Types.Rated), "BL006");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsPayable, AccTaxRate.Types.CapitalRated), "BL006");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsPayable, AccTaxRate.Types.Exempt), "BL006");
				expectedValueMapping.Add(BuildKey(LedgerTypes.AccountsPayable, ZString.Empty), "BL007");

				ConfigureRegistryForBothARAndAP(TransactionTypes.Invoice, AccTransactionLinesSchema.Constants.Prefix, CreateTaxRegistration().Code, ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.OrganisationBLCode);
				var postDate = ZDateTime.Now;

				foreach (var ledger in new ZString[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable })
				{
					foreach (var rate in new AccTaxRate[] { RatedTaxRate, CapitalRatedTaxRate, ExemptTaxRate, TaxRate })
					{
						var transaction = CreateMockTransaction(ledger
							, TransactionTypes.Invoice
							, ledger == LedgerTypes.AccountsPayable ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue
							, taxRatePK: rate.PK
							, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));

						AddLine(transaction
						, ObjectCreator.CommentChargeCode
						, ledger == LedgerTypes.AccountsPayable ? TransactionLineTypes.Cost : TransactionLineTypes.Revenue
						, TaxRate.PK
						, 250
						, 25
						, job.PK);

						UpdateHeaderTotals(transaction);

						Factory.Save();

						transaction.QueueForComplianceReports();

						var reportQueuedLine = ObjectCreator.LoadReportQueuedLinesByHeaderPK(transaction.PK);
						AssertEquals("Queue records added", 1, reportQueuedLine.Count);

						AssertQueueForReport(reportQueuedLine, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date, expectedValueMapping[BuildKey(ledger, rate.AT_Type)]
							, "ACQ_ReportSubCode when Ledger:" + ledger + " and TaxRate Type: " + rate.AT_Type);
					}
				}
			}

			ZString BuildKey(ZString ledger, ZString taxType) =>
				FormattableString.Invariant($"{ledger}#{taxType}");
		}

		public void TestQueueForComplianceReportWithMappedReportSubCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				ConfigureRegistry(LedgerTypes.AccountsReceivable
					, TransactionTypes.Invoice
					, AccTransactionLinesSchema.Constants.Prefix
					, CreateTaxRegistration().Code
					, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.OrganisationSubCode
					, recipientOrgPK: ObjectCreator.AALSHI.PK);

				var companyCodeMap = GlbCompany.CurrentCompany.OrgProxy.CreatePatternMatchOverrideForTest();
				companyCodeMap.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				companyCodeMap.OO_LocalCode = ObjectCreator.CC1.AC_Code;
				companyCodeMap.OO_ForeignCode = "ABC12345";

				var recipientCodeMap = ObjectCreator.AALSHI.CreatePatternMatchOverrideForTest();
				recipientCodeMap.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				recipientCodeMap.OO_LocalCode = ObjectCreator.CC1.AC_Code;
				recipientCodeMap.OO_ForeignCode = "BBC23456";

				var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				Factory.Save();

				var postDate = ZDateTime.Now;
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, orgPK: ObjectCreator.AALSHI.PK
					, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));

				var line1 = AddLine(transaction
					, ObjectCreator.CC1
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 1450
					, 145
					, job.PK);
				var line2 = AddLine(transaction
					, ObjectCreator.CC2
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 2450
					, 245
					, job.PK);
				UpdateHeaderTotals(transaction);

				transaction.QueueForComplianceReports();

				var reportQueuedLine = ObjectCreator.LoadReportQueuesByParentID(line1.PK);
				AssertEquals("Queue records added", 1, reportQueuedLine.Count);

				var reportQueuedLine2 = ObjectCreator.LoadReportQueuesByParentID(line2.PK);
				AssertEquals("Queue records added", 0, reportQueuedLine2.Count);

				AssertQueueForReport(reportQueuedLine, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date, reportSubCode: "BBC23456");
			}
		}

		public void TestQueueForComplianceReportWithLineNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				ConfigureRegistry(LedgerTypes.AccountsReceivable
					, TransactionTypes.Invoice
					, AccTransactionLinesSchema.Constants.Prefix
					, CreateTaxRegistration().Code
					, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TransactionHeaderWithLines
					, recipientOrgPK: ObjectCreator.AALSHI.PK);

				var companyCodeMap = GlbCompany.CurrentCompany.OrgProxy.CreatePatternMatchOverrideForTest();
				companyCodeMap.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				companyCodeMap.OO_LocalCode = ObjectCreator.CC1.AC_Code;
				companyCodeMap.OO_ForeignCode = "ABC12345";

				var recipientCodeMap = ObjectCreator.AALSHI.CreatePatternMatchOverrideForTest();
				recipientCodeMap.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				recipientCodeMap.OO_LocalCode = ObjectCreator.CC1.AC_Code;
				recipientCodeMap.OO_ForeignCode = "BBC23456";

				var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				Factory.Save();

				var postDate = ZDateTime.Now;
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, orgPK: ObjectCreator.AALSHI.PK
					, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));

				var line1 = AddLine(transaction
					, ObjectCreator.CC1
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 1450
					, 145
					, job.PK);
				var line2 = AddLine(transaction
					, ObjectCreator.CC2
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 2450
					, 245
					, job.PK);
				var line3 = AddLine(transaction
					, ObjectCreator.CommentChargeCode
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 500
					, 50
					, job.PK);
				UpdateHeaderTotals(transaction);

				transaction.QueueForComplianceReports();

				var reportQueuedLine1 = ObjectCreator.LoadReportQueuesByParentID(line1.PK);
				AssertEquals("Queue records added", 1, reportQueuedLine1.Count);

				var reportQueuedLine2 = ObjectCreator.LoadReportQueuesByParentID(line2.PK);
				AssertEquals("Queue records added", 1, reportQueuedLine2.Count);

				var reportQueuedLine3 = ObjectCreator.LoadReportQueuesByParentID(line3.PK);
				AssertEquals("Queue records not added for comment line", 0, reportQueuedLine3.Count);

				AssertQueueForReport(reportQueuedLine1, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date, reportSubCode: line1.AL_Sequence.ToString("D5"));
				AssertQueueForReport(reportQueuedLine2, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date, reportSubCode: line2.AL_Sequence.ToString("D5"));
			}
		}

		public void TestQueueForComplianceReport_ExemptReports()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Mexico))
			{
				ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.Constants.Prefix, CreateTaxRegistration().Code);

				var postDate = ZDateTime.Now;
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK);
				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				var org = newFactory.Load<OrgHeader>(transaction.AH_OH);
				AssertNotNull("InvoicngBase Org in a newFactory", org);

				var exemptReport = org.RequiredDocuments.AddNew();
				exemptReport.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
				exemptReport.EQ_DocType = "ABC";
				exemptReport.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Mexico;
				exemptReport.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
				exemptReport.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				exemptReport.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-7);
				exemptReport.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
				exemptReport.RunPreSaveValidation();
				Assert("Is a valid JobRequiredDocument record", !exemptReport.HasErrors);
				newFactory.Save();

				transaction.QueueForComplianceReports();

				var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
				AssertEquals("Queue records added", 0, reportQueuedHeaders.Count);
			}
		}

		public void TestQueueForComplianceReport_ExemptGroupingCodeDAB()
		{
			ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, ReportBaseTablePrefixListCodes.AllTransactions, CreateTaxRegistration().Code,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);

			var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK);
			Factory.Save();

			transaction.QueueForComplianceReports();

			var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
			AssertEquals("Queue records added", 0, reportQueuedHeaders.Count);
		}

		public void TestQueueForComplianceReport_ExemptGroupingCodeDBW()
		{
			ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, ReportBaseTablePrefixListCodes.AllTransactions, CreateTaxRegistration().Code,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

			var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK);
			Factory.Save();

			transaction.QueueForComplianceReports();

			var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
			AssertEquals("Queue records added", 0, reportQueuedHeaders.Count);
		}

		public void TestQueueForComplianceReport_ExemptGroupingCodeTPA()
		{
			ConfigureRegistry(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, AccTransactionHeaderSchema.Constants.Prefix, CreateTaxRegistration().Code,
				ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TransactionPayments);

			var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, TransactionLineTypes.Revenue, TaxRate.PK);
			Factory.Save();

			transaction.QueueForComplianceReports();

			var reportQueuedHeaders = ObjectCreator.LoadReportQueuesByParentID(transaction.PK);
			AssertEquals("Queue records added", 0, reportQueuedHeaders.Count);
		}

		public void Test_ThrowsException_WhenTaxRuleIsAll_AndReportingDateIsEarliestTaxDateForLines()
		{
			var collection = new ComplianceReportConfigurationCollection(Factory);

			var report = collection.AddNew();
			report.Country = Env.CurrentCompany.Country.Code;
			report.ReportCode = "JPK";
			report.ReportBaseTablePrefix = AccTransactionLinesSchema.Constants.Prefix;
			report.ReportPeriodicity = "RNG";
			report.ReportLineGrouping = "";
			report.TaxRegistrationType = "ABN";

			var setting = report.Settings.AddNew();
			setting.ComplianceSubType = "";
			setting.LedgerType = LedgerTypes.AccountsReceivable;
			setting.InvoiceType = TransactionTypes.Invoice;
			setting.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			setting.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			setting.OrganisationLocation = "";

			// These two settings cause the system to use a tax date that is empty/invalid:
			// 1. TaxInvoiceRule = All → includes all tax lines (even those with no date)
			// 2. ReportingDate = EarliestTaxDate → tries to extract the earliest tax date, but it's empty
			setting.ReportingDate = ReportingDateCodes.EarliestTaxDate;
			setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;

			using var config = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			ObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var invoicingBase = ObjectCreator.CreateARInvoice<ARInvoice>("I001", ObjectCreator.AUD, 1M, ObjectCreator.Debtor);
			var line = ObjectCreator.CreateInvoiceLine(invoicingBase, ObjectCreator.CC1.PK, 100m);
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			line.AL_JH = job.PK;
			line.AL_AT = ZGuid.Empty;
			ObjectCreator.CreateCharge(line);

			Assert("Line Tax ID should be null", line.AL_TaxDate.IsEmpty);
			Assert("Line Tax ID should be null", line.AL_AT.IsEmpty);

			AssertExceptionThrown<ApplicationException>(
				"Cannot obtain date for queueing compliance report of type JPK. Please contact support.",
				() => Factory.Save()
			);
		}

		public void Test_ThrowsException_WhenTaxRuleIsAll_AndReportingDateIsEarliestTaxDateforHeader()
		{
			var collection = new ComplianceReportConfigurationCollection(Factory);

			var report = collection.AddNew();
			report.Country = Env.CurrentCompany.Country.Code;
			report.ReportCode = "JPK";
			report.ReportBaseTablePrefix = AccTransactionHeaderSchema.Constants.Prefix;
			report.ReportPeriodicity = "RNG";
			report.ReportLineGrouping = "";
			report.TaxRegistrationType = "ABN";

			var setting = report.Settings.AddNew();
			setting.ComplianceSubType = "";
			setting.LedgerType = LedgerTypes.AccountsReceivable;
			setting.InvoiceType = TransactionTypes.Invoice;
			setting.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly;
			setting.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			setting.OrganisationLocation = "";

			// These two settings cause the system to use a tax date that is empty/invalid:
			// 1. TaxInvoiceRule = All → includes all tax lines (even those with no date)
			// 2. ReportingDate = EarliestTaxDate → tries to extract the earliest tax date, but it's empty
			setting.ReportingDate = ReportingDateCodes.EarliestTaxDate;
			setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;

			using var config = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration
				.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			ObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			var invoicingBase = ObjectCreator.CreateARInvoice<ARInvoice>("I001", ObjectCreator.AUD, 1M, ObjectCreator.Debtor);
			var line = ObjectCreator.CreateInvoiceLine(invoicingBase, ObjectCreator.CC1.PK, 100m);
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			line.AL_JH = job.PK;
			line.AL_AT = ZGuid.Empty;
			ObjectCreator.CreateCharge(line);

			Assert("Line Tax ID should be null", line.AL_TaxDate.IsEmpty);
			Assert("Line Tax ID should be null", line.AL_AT.IsEmpty);

			AssertExceptionThrown<ApplicationException>(
				"Cannot obtain date for queueing compliance report of type JPK. Please contact support.",
				() => Factory.Save()
			);
		}

		#region Updating Compliance Report Queue for Government Invoice

		public void TestQueueForComplianceReport_GovernmentInvoice_Italy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				ConfigureRegistry(LedgerTypes.AccountsReceivable,
					TransactionTypes.Invoice,
					AccTransactionLinesSchema.Constants.Prefix,
					CreateTaxRegistration().Code,
					"TXR",
					complianceSubType: ItalyComplianceInfo.ComplianceSubTypeCodes.ARI);

				var postDate = ZDateTime.Now;
				var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
				var transaction = CreateMockTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, orgPK: ObjectCreator.AALSHI.PK
					, postDate: postDate, invoiceDate: postDate.AddDays(1), receivedDate: postDate.AddDays(-1));
				AddLine(transaction
					, ObjectCreator.CC1
					, TransactionLineTypes.Revenue
					, TaxRate.PK
					, 1450
					, 145
					, job.PK);

				AddLine(transaction
				, ObjectCreator.CommentChargeCode
				, TransactionLineTypes.Revenue
				, TaxRate.PK
				, 250
				, 25
				, job.PK);

				UpdateHeaderTotals(transaction);

				Factory.Save();

				var reportQueuedLine = ObjectCreator.LoadReportQueuedLinesByHeaderPK(transaction.PK);
				AssertEquals("Queue records added", 1, reportQueuedLine.Count);
				AssertQueueForReport(reportQueuedLine, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date);

				var newFactory = new BusinessObjectFactory();
				var govInvoice = newFactory.Load<GovernmentInvoice>(transaction.PK);
				govInvoice.AH_TransactionReference = "00000123";
				govInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARN;
				newFactory.Save();

				reportQueuedLine = ObjectCreator.LoadReportQueuedLinesByHeaderPK(govInvoice.PK);
				AssertEquals("Queue records deleted", 0, reportQueuedLine.Count);

				govInvoice.AH_ComplianceSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
				newFactory.Save();

				reportQueuedLine = ObjectCreator.LoadReportQueuedLinesByHeaderPK(govInvoice.PK);
				AssertEquals("Queue records added again", 1, reportQueuedLine.Count);
				AssertQueueForReport(reportQueuedLine, "ABC", AccTransactionLinesSchema.Constants.Prefix, queueDate: postDate.Date);
			}
		}

		#endregion

		void AssertQueueForReport(DynamicBusinessObjectCollection reportQueues, string reportType, string parentTablePrefix, ZDate? queueDate = null, string reportSubCode = "", string subCodeMessage = "ACQ_ReportSubCode")
		{
			var reportQueue = reportQueues.Cast<DynamicBusinessObject>().FirstOrDefault(x => x[AccTransactionComplianceReportQueueSchema.ACQ_ReportType].ToString() == reportType);
			AssertNotNull(reportType + " reportqueue", reportQueue);
			AssertEquals("ACQ_GC_Company", GlbCompany.CurrentCompany.PK.ToString(), reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_GC_Company].ToString());
			AssertEquals("ACQ_GB_Branch", GlbBranch.CurrentBranch.PK.ToString(), reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_GB_Branch].ToString());
			AssertEquals("ACQ_ParentTableCode", parentTablePrefix, reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_ParentTableCode].ToString());
			AssertEquals(subCodeMessage, reportSubCode, reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_ReportSubCode].ToString());
			if (queueDate.HasValue)
			{
				AssertEquals("ACQ_Date", queueDate.Value, new ZDate(reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_Date]));
			}
			else
			{
				AssertNotEquals("ACQ_Date has value", DBNull.Value, reportQueue[AccTransactionComplianceReportQueueSchema.ACQ_Date]);
			}
		}

		#region Report Configuration

		void ConfigureRegistryForBothARAndAP(string transactionType, string tablePrefix, string taxRegistrationCode, string reportLineGrouping)
		{
			ConfigureRegistry(LedgerTypes.AccountsReceivable, transactionType, tablePrefix, taxRegistrationCode, reportLineGrouping: reportLineGrouping);

			var configCollection = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;

			var setting = ObjectCreator.CreateConfigurationSettingsForComplianceReport(configCollection.Cast<ComplianceReportConfiguration>().First()
				, LedgerTypes.AccountsPayable
				, transactionType
				, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID
				, disbursementRule: DisbursementRuleCodes.NonDisbursementOnly
				, originalRule: OriginalRuleCodes.OriginalTransactionOnly);

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, configCollection);
		}

		void ConfigureRegistry(string ledger, string transactionType, string tablePrefix, string taxRegistrationCode, string reportLineGrouping = "", ZGuid? recipientOrgPK = null, string complianceSubType = null, string reportingDate = ReportingDateCodes.PostDate)
		{
			var configCollection = new ComplianceReportConfigurationCollection();

			var config = ObjectCreator.CreateConfigurationForComplianceReport(
				configCollection
				, GlbCompany.CurrentCompany.Country.Code
				, "ABC"
				, "ABC Compliance Report"
				, tablePrefix
				, reportLineGrouping: reportLineGrouping
				, goodsAndService: ""
				, "RNG"
				, taxRegistrationType: taxRegistrationCode
				, recipientOrgPK ?? ZGuid.Empty
				, false);

			var setting = ObjectCreator.CreateConfigurationSettingsForComplianceReport(config
				, ledger
				, transactionType
				, taxInvoiceRule: TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID
				, disbursementRule: DisbursementRuleCodes.NonDisbursementOnly
				, originalRule: OriginalRuleCodes.OriginalTransactionOnly
				, complianceSubType: complianceSubType ?? ""
				, reportingDate: reportingDate);

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, configCollection);
		}

		#endregion

		#region Transaction

		MockTransaction CreateMockTransaction(string ledger, string transactionType, ZGuid? orgPK = null
			, ZDateTime? postDate = null, ZDateTime? invoiceDate = null, ZDateTime? receivedDate = null)
		{
			return CreateMockTransaction(ledger
					, transactionType
					, ZString.Empty
					, ZGuid.Empty
					, orgPK: ObjectCreator.AALSHI.PK
					, addLine: false
					, postDate: postDate, invoiceDate: invoiceDate, receivedDate: receivedDate);
		}

		MockTransaction CreateMockTransaction(string ledger, string transactionType, string lineType, ZGuid taxRatePK, ZGuid? orgPK = null, bool addLine = true,
			ZDateTime? postDate = null, ZDateTime? invoiceDate = null, ZDateTime? receivedDate = null)
		{
			var transaction = Factory.NewWithValidTestData<MockTransaction>();
			transaction.AH_OH = orgPK ?? ObjectCreator.ABIGAS.PK;
			transaction.AH_Desc = "test";
			transaction.AH_TransactionReference = "";
			transaction.AH_ComplianceSubType = "";
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_PostDate = postDate ?? transaction.AH_PostDate;
			transaction.AH_InvoiceDate = invoiceDate ?? transaction.AH_InvoiceDate;
			transaction.AH_DocumentReceivedDate = receivedDate ?? transaction.AH_DocumentReceivedDate;

			if (addLine)
			{
				AddLine(transaction, null, lineType, taxRatePK, 3650M, 10M, ZGuid.Empty);
			}

			UpdateHeaderTotals(transaction);

			return transaction;
		}

		MockTransactionLine AddLine(MockTransaction transaction, AccChargeCode chargeCode, ZString lineType, ZGuid taxRatePK, ZDecimal lineAmount, ZDecimal gstVat, ZGuid jobPK, ZDate? taxDate = null)
		{
			var line = (MockTransactionLine)transaction.Lines.AddNew();
			line.AL_AT = taxRatePK;
			line.AL_LineAmount = lineAmount;
			line.AL_GSTVAT = gstVat;
			line.AL_LineType = lineType;
			line.AL_OSAmount = line.AL_LineAmount + line.AL_GSTVAT;

			if (jobPK.IsValid)
			{
				using (line.GetValidationSuspender())
				{
					line.AL_AG = chargeCode?.AC_AG_RevenueAccount ?? ZGuid.Empty;
					line.AL_AC = chargeCode?.PK ?? ZGuid.Empty;
					line.AL_JH = jobPK;
					line.AL_PostDate = ZDateTime.Today;
					ObjectCreator.CreateCharge(line);
				}
			}

			if (line.AL_AG.IsEmpty)
			{
				line.AL_AG = objectCreator.GLHeader1.PK;
			}

			line.AL_TaxDate = taxDate ?? line.AL_TaxDate;

			return line;
		}

		void UpdateHeaderTotals(MockTransaction transaction)
		{
			var lines = transaction.Lines.Cast<MockTransactionLine>().ToArray();
			transaction.AH_InvoiceAmount = lines.Sum(l => l.AL_LineAmount);
			transaction.AH_GSTAmount = lines.Sum(l => l.AL_GSTVAT);
			transaction.AH_OutstandingAmount = transaction.AH_OSTotal = transaction.AH_InvoiceAmount + transaction.AH_GSTAmount;
		}

		#endregion

		#region Tax

		ICodeDescription CreateTaxRegistration()
		{
			var taxRegistrationCodes = new OrgCodeLists().CustomsCodes_List(RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code));
			var taxRegistration = taxRegistrationCodes.ToArray().FirstOrDefault(x => !string.IsNullOrEmpty(x.Code));
			AssertNotNull("There should be a Tax Registration", taxRegistration);
			return taxRegistration;
		}

		AccTaxRate RatedTaxRate
		{
			get
			{
				if (ratedTaxRate == null)
				{
					ratedTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
					ratedTaxRate.AT_Code = "TaxRate1";
					ratedTaxRate.AT_Type = AccTaxRate.Types.Rated;
					ratedTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					ratedTaxRate.SetRateNumerator_ForTestOnly(100);
				}
				return ratedTaxRate;
			}
		}
		AccTaxRate ratedTaxRate;

		AccTaxRate CapitalRatedTaxRate
		{
			get
			{
				if (capitalRatedTaxRate == null)
				{
					capitalRatedTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
					capitalRatedTaxRate.AT_Code = "TaxRate2";
					capitalRatedTaxRate.AT_Type = AccTaxRate.Types.CapitalRated;
					capitalRatedTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					capitalRatedTaxRate.SetRateNumerator_ForTestOnly(100);
				}
				return capitalRatedTaxRate;
			}
		}
		AccTaxRate capitalRatedTaxRate;

		AccTaxRate ExemptTaxRate
		{
			get
			{
				if (exemptTaxRate == null)
				{
					exemptTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
					exemptTaxRate.AT_Code = "TaxRate3";
					exemptTaxRate.AT_Type = AccTaxRate.Types.Exempt;
					exemptTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					exemptTaxRate.SetRateNumerator_ForTestOnly(100);
				}
				return exemptTaxRate;
			}
		}
		AccTaxRate exemptTaxRate;

		AccTaxRate TaxRate
		{
			get
			{
				if (taxRate == null)
				{
					taxRate = Factory.NewWithValidTestData<AccTaxRate>();
					taxRate.AT_Code = "TaxRate4";
					taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					taxRate.SetRateNumerator_ForTestOnly(100);
				}
				return taxRate;
			}
		}
		AccTaxRate taxRate;

		#endregion

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		public static class Helpers
		{
			public static IReadOnlyCollection<string> GetClusteredIndexFieldsForAcqTable()
				=> Db.Connection.ExecuteScalar("SELECT name FROM sys.indexes WHERE type = 1 AND NAME LIKE '%\\_\\_ACQ%' ESCAPE '\\'").ToString()
						.Substring(6)
						.Split(new string[] { "_ACQ_" }, StringSplitOptions.RemoveEmptyEntries)
						.Select(col => "ACQ_" + col)
						.ToArray();

			public static IEnumerable<(string sql, bool isCovered)> GetDeleteCommandsCoveredByIndexFields(IEnumerable<string> trackedCommands, IReadOnlyCollection<string> indexFields)
			{
				return trackedCommands
						.Where(sql => sql.Contains($"DELETE {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}", StringComparison.OrdinalIgnoreCase))
						.SelectMany(sqls => sqls
											.Split(new string[] { "DELETE" }, StringSplitOptions.RemoveEmptyEntries)
											.Skip(1)
											.Where(s => !string.IsNullOrWhiteSpace(s))
						)
						.Select(sql => new
						{
							sql,
							isCovered = indexFields.All(f => sql.Contains("WHERE " + f, StringComparison.OrdinalIgnoreCase)
															|| sql.Contains("AND " + f, StringComparison.OrdinalIgnoreCase)
													),
						})
						.Select(x => (x.sql, x.isCovered));
			}
		}
	}

	public class MockTransaction : TransactionHeaderWithLines, ISupportQueueingForComplianceReports, IEvaluateComplianceRule
	{
		public MockTransaction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region overrides

		protected override bool InvertSigns => false;

		protected override ZString TransactionType => AH_TransactionType;

		protected override ZString Ledger => AH_Ledger;

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber => null;

		public override Type DependentTransactionLineType => typeof(MockTransactionLine);

		#endregion

		#region ISupportQueueingForComplianceReports

		ComplianceSubTypeRule ISupportQueueingForComplianceReports.ComplianceMatchingRule => complianceMatchingRule ?? (complianceMatchingRule = new ComplianceSubTypeRule(Factory, this));
		ComplianceSubTypeRule complianceMatchingRule;

		IComplianceReportQueuer ISupportQueueingForComplianceReports.Queuer => new ComplianceReportTransactionQueuer<MockTransaction>(this);

		bool ISupportQueueingForComplianceReports.CheckIsValidForQueueing(BusinessObjectFactory factory) => true;

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new MockTransactionLineCollection(this, Factory);
		}

		#endregion

		#region IEvaluateComplianceRule

		ZString IEvaluateComplianceRule.Ledger => AH_Ledger;

		ZString IEvaluateComplianceRule.TransactionType => AH_TransactionType;

		ZString IEvaluateComplianceRule.ComplianceSubType => AH_ComplianceSubType;

		ZBool IEvaluateComplianceRule.IsDisbursementOrFinal => false;

		ZBool IEvaluateComplianceRule.IsSelfBillingInvoice => false;

		ZBool IEvaluateComplianceRule.IsAmendingTransaction => false;

		ZBool IEvaluateComplianceRule.IsReversalTransaction => false;

		OrgHeader IEvaluateComplianceRule.Header => Header;

		GlbCompany IEvaluateComplianceRule.Company => Company;

		IEnumerable<AccTransactionLines> IEvaluateComplianceRule.Lines => Lines.ToArray<AccTransactionLines>();

		ZBool IEvaluateComplianceRule.EmptyLedgerMatchesAll => true;

		ZBool IEvaluateComplianceRule.EmptyTransactionTypeMatchesAll => true;

		IEnumerable<AccTaxTransaction> IEvaluateComplianceRule.TaxTransactions => Array.Empty<AccTaxTransaction>();

		ZDecimal IEvaluateComplianceRule.LocalTotalAmount => AH_LocalTotalAmount;

		#endregion

		public override ZBool CanApplyTaxBranch => IsMiscServTaxApplicable;
	}

	public class MockTransactionLineCollection : DependentTransactionLineCollection
	{
		public MockTransactionLineCollection(MockTransaction transaction, BusinessObjectFactory factory)
			: base(transaction, factory)
		{
		}

		public new MockTransactionLine this[int index]
		{
			get { return (MockTransactionLine)Elements[index]; }
		}

		public new MockTransactionLine AddNew()
		{
			return (MockTransactionLine)base.AddNew();
		}
	}

	public class MockTransactionLine : DependentTransactionLine
	{
		public MockTransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override ZString LineType => AL_LineType;

		protected override bool InvertSigns => false;
	}
}

using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_ADHTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(report, reportConfigurations, TablePrefix, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TXI");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var customCode = TestObjectCreator.ABIGAS.CustomsCodes.AddNew();
				customCode.OK_CodeType = "VAT";
				customCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				customCode.OK_CustomsRegNo = "1234567";
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals("compliance document is created", 1, complianceDocuments.Length);
				var complianceDocument = complianceDocuments[0];
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				complianceDocument.ADH_ComplianceSubType = "TXI";
				complianceDocument.ADH_XD_ComplianceBook = sequenceBook.PK;
				complianceDocument.ADH_DocumentNumber = "AA00000001";
				complianceDocument.ADH_ReportingPeriod = 201901;
				Factory.Save();

				report.GenerateFromQueue();

				var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable();
				var row = rows.FirstOrDefault();
				AssertNotNull("compliance report line is generated", row);
				AssertEquals("Org Code", TestObjectCreator.ABIGAS.OH_Code, row.Field<string>("OH_Code").Trim());
				AssertEquals("Org FullName", TestObjectCreator.ABIGAS.OH_FullName, row.Field<string>("OH_FullName").Trim());
				AssertEquals("OK_CustomsRegNo", "1234567", row.Field<string>("OK_CustomsRegNo").Trim());
				AssertEquals("Compliance Document Header's PK", complianceDocument.PK, row.Field<Guid>("AH_PK"));
				AssertEquals("Ledger", "AR", row.Field<string>("AH_Ledger"));
				AssertEquals("Sequence Book", sequenceBook.XD_Code, row.Field<string>("AH_TransactionType"));
				AssertEquals("InvoiceAmount", 100M, row.Field<decimal>("AH_InvoiceAmount"));
				AssertEquals("GSTAmount", 10M, row.Field<decimal>("AH_GSTAmount"));
				AssertEquals("DocumentDate", complianceDocument.ADH_DocumentDate.Date, row.Field<DateTime>("PostDate").Date);
				AssertEquals("Reporting Period", "201901", row.Field<string>("AH_TransactionReference"));
				AssertEquals("Document Number", "AA00000001", row.Field<string>("AH_TransactionNum"));
				AssertEquals("Compliance Sub Type", "TXI", row.Field<string>("AH_ComplianceSubType"));
			}
		}

		public void TestGetOverrideVATRegistrationNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(report, reportConfigurations, AccComplianceDocumentHeaderSchema.Constants.Prefix, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TXI");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var customCode = TestObjectCreator.ABIGAS.CustomsCodes.AddNew();
				customCode.OK_CodeType = "VAT";
				customCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				customCode.OK_CustomsRegNo = "1234567";
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var complianceDocuments = Factory.Load<AccComplianceDocumentHeader>(new ZQuery());
				AssertEquals("compliance document is created", 1, complianceDocuments.Length);
				var complianceDocument = complianceDocuments[0];
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				complianceDocument.ADH_ComplianceSubType = "TXI";
				complianceDocument.ADH_XD_ComplianceBook = sequenceBook.PK;
				complianceDocument.ADH_DocumentNumber = "AA00000001";
				complianceDocument.ADH_ReportingPeriod = 201901;
				complianceDocument.ADH_VATRegistrationNumberOverride = "7654321";
				Factory.Save();

				report.GenerateFromQueue();

				var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable();
				var row = rows.FirstOrDefault();
				AssertNotNull("compliance report line is generated", row);
				AssertEquals("OK_CustomsRegNo", "7654321", row.Field<string>("OK_CustomsRegNo").Trim());
			}
		}

		protected override string ReportName => "GetComplianceReportLines_ADH";
		protected override string GroupByCode => ReportLineGroupingListCodes.NoGrouping;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
	}
}

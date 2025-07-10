using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ViewComplianceReportLineTest : ScriptTest
	{
		public void TestGetTaxRateFromLines()
		{
			Report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(Report, "AL", "TXR");
			CreateAndSaveAPInvoices();
			TestObjectCreator.CreateComplianceReportQueueEntry(Report, InvoiceLines.ToArray());
			Report.GenerateFromQueue();

			var result = RunScript().AsEnumerable();
			AssertResult(TestObjectCreator.GST1.PK, 10M, 0M);
			AssertResult(TestObjectCreator.GSTANDQST1.PK, 5M, 9.5M);
			AssertResult(TestObjectCreator.STAGST.PK, 9M, 9M);
			AssertResult(TestObjectCreator.ServiceTax.PK, 5M, 5M);
			AssertResult(TestObjectCreator.INP7.PK, 0M, 7M);
			AssertResult(TestObjectCreator.REF.PK, 9M, 12.5M);

			TestObjectCreator.GST1.SetRate_ForTestOnly(6, 2);
			TestObjectCreator.GST1.SetExtraRate_ForTestOnly(4, 2);
			TestObjectCreator.GSTANDQST1.SetRate_ForTestOnly(125, 10);
			TestObjectCreator.GSTANDQST1.SetExtraRate_ForTestOnly(75, 10);
			TestObjectCreator.STAGST.SetRate_ForTestOnly(18, 6);
			TestObjectCreator.STAGST.SetExtraRate_ForTestOnly(12, 4);
			TestObjectCreator.ServiceTax.SetRate_ForTestOnly(36, 6);
			TestObjectCreator.ServiceTax.SetExtraRate_ForTestOnly(14, 7);
			TestObjectCreator.INP7.SetRate_ForTestOnly(16, 2);
			TestObjectCreator.INP7.SetExtraRate_ForTestOnly(0, 1);
			TestObjectCreator.REF.SetRate_ForTestOnly(13, 1);
			TestObjectCreator.REF.SetExtraRate_ForTestOnly(12, 2);

			Factory.Save();

			result = RunScript().AsEnumerable();
			AssertResult(TestObjectCreator.GST1.PK, 10M, 0M);
			AssertResult(TestObjectCreator.GSTANDQST1.PK, 5M, 9.5M);
			AssertResult(TestObjectCreator.STAGST.PK, 9M, 9M);
			AssertResult(TestObjectCreator.ServiceTax.PK, 5M, 5M);
			AssertResult(TestObjectCreator.INP7.PK, 0M, 7M);
			AssertResult(TestObjectCreator.REF.PK, 9M, 12.5M);

			void AssertResult(ZGuid pk, decimal expectedRate, decimal expectedExtraRate)
			{
				var row = result.FirstOrDefault(x => pk.Equals(x["AL_AT"]));
				AssertNotNull(row);
				AssertEquals(expectedRate, row.Field<decimal>("AL_TaxRate"));
				AssertEquals(expectedExtraRate, row.Field<decimal>("AL_TaxExtraRate"));
			}
		}

		[TestDate(2019, 1, 18)]
		public void TestGetComplianceDocumentHeadersOrderByFDN()
		{
			AssertComplianceReportByComplianceDocument(ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
		}

		[TestDate(2019, 1, 18)]
		public void TestGetVoidComplianceDocumentHeadersOrderByFDN()
		{
			AssertComplianceReportByComplianceDocument(ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber, true);
		}

		[TestDate(2019, 1, 18)]
		public void TestGetComplianceDocumentHeadersOrderByCDN()
		{
			AssertComplianceReportByComplianceDocument(ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceDocumentNumber);
		}

		void AssertComplianceReportByComplianceDocument(ZString reportLineOrdering, bool isVoid = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				Report = Factory.NewWithValidTestData<AccComplianceReport>();

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(Report, reportConfigurations, AccComplianceDocumentHeaderSchema.Constants.Prefix, reportLineOrdering: reportLineOrdering);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TXI");
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsPayable, "", complianceSubType: "TXI");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				Factory.Save();

				var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
				var arInvoiceLine = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				arInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;

				var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV002", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
				var apInvoiceline = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1M, 200M, 20M, 0M, true);
				apInvoiceline.AL_Desc = "test";
				apInvoiceline.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				new ComplianceDocumentCreator(new[] { apInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
				Factory.Save();

				var arComplianceDocument = Factory.LoadTop1<ARComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsReceivable));
				AssertNotNull("AR Compliance Document is created", arComplianceDocument);
				arComplianceDocument.ADH_ComplianceSubType = "TXI";
				arComplianceDocument.ADH_DocumentNumber = "001";
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				arComplianceDocument.ADH_XD_ComplianceBook = sequenceBook.PK;

				var apComplianceDocument = Factory.LoadTop1<APComplianceDocumentHeader>(new ZQuery(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerTypes.AccountsPayable));
				AssertNotNull("AP Compliance Document is created", apComplianceDocument);
				apComplianceDocument.ADH_ComplianceSubType = "TXI";
				apComplianceDocument.ADH_DocumentNumber = "002";

				Factory.Save();

				if (isVoid)
				{
					arComplianceDocument.Void();
					Factory.Save();
				}

				Report.GenerateFromQueue();
				var result = RunScript().AsEnumerable();
				AssertEquals("there are 2 report lines", 2, result.Count());

				if (reportLineOrdering == ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber)
				{
					AssertViewDetailsForAP(result.ElementAt(0), apComplianceDocument);
					AssertViewDetailsForAR(result.ElementAt(1), arComplianceDocument);
				}
				else if (reportLineOrdering == ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceDocumentNumber)
				{
					AssertViewDetailsForAR(result.ElementAt(0), arComplianceDocument);
					AssertViewDetailsForAP(result.ElementAt(1), apComplianceDocument);
				}
			}
		}

		void AssertViewDetailsForAR(DataRow row, AccComplianceDocumentHeader complianceDocument)
		{
			AssertEquals("ParentTableCode", "ADH", row.Field<string>("ACL_ParentTableCode"));
			AssertEquals("Org's PK", TestObjectCreator.ABIGAS.PK, row.Field<Guid>("AH_OH"));
			AssertEquals("Compliance Document Header's PK", complianceDocument.PK, row.Field<Guid>("AH_PK"));
			AssertEquals("Ledger", "AR", row.Field<string>("AH_Ledger"));
			AssertEquals("DocumentDate", complianceDocument.ADH_DocumentDate.Date, row.Field<DateTime>("PostDate").Date);
			AssertEquals("Reporting Period", "201901", row.Field<string>("AH_TransactionReference"));
			AssertEquals("Document Number", complianceDocument.ADH_DocumentNumber, row.Field<string>("AH_TransactionNum"));
			AssertEquals("Compliance SubType", complianceDocument.ADH_ComplianceSubType, row.Field<string>("AH_ComplianceSubType"));
			AssertEquals("Compliance Book", "ABC", row.Field<string>("AH_TransactionType"));
			if (complianceDocument.ADH_DocumentStatus == ComplianceDocumentStatus.Voided)
			{
				AssertEquals("Amount", 0M, row.Field<decimal>("AH_InvoiceAmount"));
				AssertEquals("Tax Amount", 0M, row.Field<decimal>("AH_GSTAmount"));
				AssertEquals("TotalExTaxAmount", 0M, row.Field<decimal>("TotalExTaxAmount"));
				AssertEquals("TotalTaxAmount", 0M, row.Field<decimal>("TotalTaxAmount"));
			}
			else
			{
				AssertEquals("Amount", 100M, row.Field<decimal>("AH_InvoiceAmount"));
				AssertEquals("Tax Amount", 10M, row.Field<decimal>("AH_GSTAmount"));
				AssertEquals("TotalExTaxAmount", 100M, row.Field<decimal>("TotalExTaxAmount"));
				AssertEquals("TotalTaxAmount", 10M, row.Field<decimal>("TotalTaxAmount"));
			}
		}

		void AssertViewDetailsForAP(DataRow row, AccComplianceDocumentHeader complianceDocument)
		{
			AssertEquals("ParentTableCode", "ADH", row.Field<string>("ACL_ParentTableCode"));
			AssertEquals("Org's PK", TestObjectCreator.AALSHI.PK, row.Field<Guid>("AH_OH"));
			AssertEquals("Compliance Document Header's PK", complianceDocument.PK, row.Field<Guid>("AH_PK"));
			AssertEquals("Ledger", "AP", row.Field<string>("AH_Ledger"));
			AssertEquals("Amount", 200M, row.Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("Tax Amount", 20M, row.Field<decimal>("AH_GSTAmount"));
			AssertEquals("DocumentDate", complianceDocument.ADH_DocumentDate.Date, row.Field<DateTime>("PostDate").Date);
			AssertEquals("Reporting Period", "201901", row.Field<string>("AH_TransactionReference"));
			AssertEquals("Document Number", complianceDocument.ADH_DocumentNumber, row.Field<string>("AH_TransactionNum"));
			AssertEquals("Compliance SubType", complianceDocument.ADH_ComplianceSubType, row.Field<string>("AH_ComplianceSubType"));
			AssertEquals("Compliance Book", null, row.Field<string>("AH_TransactionType"));
			AssertEquals("TotalExTaxAmount", 200M, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("TotalTaxAmount", 20M, row.Field<decimal>("TotalTaxAmount"));
		}

		void CreateAndSaveAPInvoices()
		{
			APInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("I0001" + Report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", APInvoice1.Lines.Count > 0);
			var line1_1 = APInvoice1.Lines[0];
			line1_1.AL_AT = TestObjectCreator.GST1.PK;
			line1_1.AL_InputGSTVATRecoverable = 0.8m;
			var line1_2 = TestObjectCreator.CreateInvoiceLine(APInvoice1, TestObjectCreator.AUD, 1m, 120m);
			line1_2.AL_AT = TestObjectCreator.GSTANDQST1.PK;
			var line1_3 = TestObjectCreator.CreateInvoiceLine(APInvoice1, TestObjectCreator.AUD, 1m, 80m);
			line1_3.AL_AT = TestObjectCreator.STAGST.PK;

			APInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("I0002" + Report.ReportLines.Count, TestObjectCreator.AUD, 1m, 50m, 10m, 0m, 50m, 10m, 0m, TestObjectCreator.Creditor1);
			Assert("Has Lines", APInvoice2.Lines.Count > 0);
			var line2_1 = APInvoice2.Lines[0];
			line2_1.AL_AT = TestObjectCreator.ServiceTax.PK;
			line2_1.AL_InputGSTVATRecoverable = 0.5m;
			var line2_2 = TestObjectCreator.CreateInvoiceLine(APInvoice2, TestObjectCreator.AUD, 1m, 130m);
			line2_2.AL_AT = TestObjectCreator.INP7.PK;
			var line2_3 = TestObjectCreator.CreateInvoiceLine(APInvoice2, TestObjectCreator.AUD, 1m, 20m);
			line2_3.AL_AT = TestObjectCreator.REF.PK;

			InvoiceLines.AddRange(new [] { line1_1, line1_2, line1_3, line2_1, line2_2, line2_3 });
			Factory.Save();
		}

		static DataTable RunScript() => DataUtils.GetDataTableFromQuery(Db.Connection, "SELECT * FROM dbo.ViewComplianceReportLine");

		AccComplianceReport Report;
		APInvoice APInvoice1, APInvoice2;
		readonly List<InvoicingLineBase> InvoiceLines = new List<InvoicingLineBase>();
	}
}

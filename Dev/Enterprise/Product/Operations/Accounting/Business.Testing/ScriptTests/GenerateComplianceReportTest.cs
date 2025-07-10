using System;
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
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GenerateComplianceReportTest : ScriptTest
	{
		public void TestGenerateComplianceReport()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.TaxReporting);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = TestObjectCreator.GST1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 200m);
			line2.AL_AT = TestObjectCreator.GSTFREE1.PK;
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 300m);
			line3.AL_AT = TestObjectCreator.GST1.PK;
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 400m);
			line4.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			var line5 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 500m);
			line5.AL_AT = TestObjectCreator.GSTFREE1.PK;
			var line6 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 600m);
			line6.AL_AT = TestObjectCreator.GST1.PK;
			var line7 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 700m);
			line7.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			var line8 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 800m);
			line8.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line1, line2, line3, line4, line5, line6, line7, line8);
			report.GenerateFromQueue();

			var sql = "SELECT  ACL_ParentID, ACL_ReportSequence FROM dbo.AccComplianceReportTransactionPivot";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			var group1 = new [] { line2.PK, line5.PK };
			var group2 = new [] { line1.PK, line3.PK, line6.PK };
			var group3 = new [] { line4.PK, line7.PK, line8.PK };

			var sequenceForGroup1 = rows.FirstOrDefault(x => x.Field<Guid>("ACL_ParentID") == group1[0])?.Field<int>("ACL_ReportSequence");
			AssertNotNull(sequenceForGroup1);
			AssertEquals(group1.Length, rows.Count(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup1));
			AssertEquals(1, rows.Count(x => (x.Field<Guid>("ACL_ParentID") == group1[1] && x.Field<int>("ACL_ReportSequence") == sequenceForGroup1)));

			var sequenceForGroup2 = rows.FirstOrDefault(x => x.Field<Guid>("ACL_ParentID") == group2[0])?.Field<int>("ACL_ReportSequence");
			AssertNotNull(sequenceForGroup2);
			AssertEquals(group2.Length, rows.Count(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup2));
			AssertEquals(1, rows.Count(x => (x.Field<Guid>("ACL_ParentID") == group2[1] && x.Field<int>("ACL_ReportSequence") == sequenceForGroup2)));
			AssertEquals(1, rows.Count(x => x.Field<Guid>("ACL_ParentID") == group2[2] && x.Field<int>("ACL_ReportSequence") == sequenceForGroup2));

			var sequenceForGroup3 = rows.FirstOrDefault(x => x.Field<Guid>("ACL_ParentID") == group3[0])?.Field<int>("ACL_ReportSequence");
			AssertNotNull(sequenceForGroup3);
			AssertEquals(group3.Length, rows.Count(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup3));
			AssertEquals(1, rows.Count(x => x.Field<Guid>("ACL_ParentID") == group3[1] && x.Field<int>("ACL_ReportSequence") == sequenceForGroup3));
			AssertEquals(1, rows.Count(x => x.Field<Guid>("ACL_ParentID") == group3[2] && x.Field<int>("ACL_ReportSequence") == sequenceForGroup3));
		}

		public void TestGenerateComplianceReport_UVA()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode);

			var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var line11 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.EUR, 1m, 200m);
			var line12 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.EUR, 1m, 200m);
			var line13 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.EUR, 1m, 300m);
			var line14 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.EUR, 1m, 600m);
			var line15 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.EUR, 1m, 700m);
			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.EUR, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			var line21 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.EUR, 1m, 400m);
			var line22 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.EUR, 1m, 800m);
			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR003", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var line31 = TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.EUR, 1m, 500m);
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, "81_NA_NA", null, line11, line13, line14);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "86_NA_NA", null, line12, line15);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "NA_NA_66", null, line21, line22);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "46_47_67", null, line31);
			report.GenerateFromQueue();

			var sql = "SELECT  ACL_ParentID, ACL_ReportSequence, ACL_ReportSubCode FROM dbo.AccComplianceReportTransactionPivot";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			var group1 = new[] { line21.PK, line22.PK };
			var group2 = new[] { line11.PK, line13.PK, line14.PK };
			var group3 = new[] { line12.PK, line15.PK };
			var group4 = new[] { line31.PK };

			var rowsOfGroup1 = rows.Where(x => group1.Contains(x.Field<Guid>("ACL_ParentID")));
			Assert("group1 ACL_ReportSequence", rowsOfGroup1.All(x => x.Field<int>("ACL_ReportSequence") == 1));
			Assert("group1 ACL_ReportSubCode", rowsOfGroup1.All(x => x.Field<string>("ACL_ReportSubCode") == "NA_NA_66"));
			AssertEquals("Contains all elements group1", group1.Length, rowsOfGroup1.Select(x => x.Field<Guid>("ACL_ParentID")).Distinct().Count());

			var rowsOfGroup2 = rows.Where(x => group2.Contains(x.Field<Guid>("ACL_ParentID")));
			Assert("group2 ACL_ReportSequence", rowsOfGroup2.All(x => x.Field<int>("ACL_ReportSequence") == 2));
			Assert("group2 ACL_ReportSubCode", rowsOfGroup2.All(x => x.Field<string>("ACL_ReportSubCode") == "81_NA_NA"));
			AssertEquals("Contains all elements group2", group2.Length, rowsOfGroup2.Select(x => x.Field<Guid>("ACL_ParentID")).Distinct().Count());

			var rowsOfGroup3 = rows.Where(x => group3.Contains(x.Field<Guid>("ACL_ParentID")));
			Assert("group3 ACL_ReportSequence", rowsOfGroup3.All(x => x.Field<int>("ACL_ReportSequence") == 3));
			Assert("group3 ACL_ReportSubCode", rowsOfGroup3.All(x => x.Field<string>("ACL_ReportSubCode") == "86_NA_NA"));
			AssertEquals("Contains all elements group3", group3.Length, rowsOfGroup3.Select(x => x.Field<Guid>("ACL_ParentID")).Distinct().Count());

			var rowsOfGroup4 = rows.Where(x => group4.Contains(x.Field<Guid>("ACL_ParentID")));
			Assert("group4 ACL_ReportSequence", rowsOfGroup4.All(x => x.Field<int>("ACL_ReportSequence") == 4));
			Assert("group4 ACL_ReportSubCode", rowsOfGroup4.All(x => x.Field<string>("ACL_ReportSubCode") == "46_47_67"));
			AssertEquals("Contains all elements group4", group4.Length, rowsOfGroup4.Select(x => x.Field<Guid>("ACL_ParentID")).Distinct().Count());
		}

		public void TestGenerateComplianceReport_FromLine()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.TaxReporting);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = TestObjectCreator.GST1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 200m);
			line2.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 300m);
			line3.AL_AT = TestObjectCreator.GST1.PK;
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 400m);
			line4.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			var line5 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 500m);
			line5.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			var line6 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 600m);
			line6.AL_AT = TestObjectCreator.GST1.PK;
			var line7 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 700m);
			line7.AL_AT = TestObjectCreator.GST1.PK;
			var line8 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 800m);
			line8.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;

			UpdateLineData(line1, 8, 2);
			UpdateLineData(line2, 8, 2);
			UpdateLineData(line4, 8, 2);

			UpdateLineData(line3, 18, 3);
			UpdateLineData(line5, 18, 3);
			UpdateLineData(line7, 18, 3);

			UpdateLineData(line6, 16, 2, 14, 2);
			UpdateLineData(line8, 16, 2, 14, 2);
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line1, line2, line3, line4, line5, line6, line7, line8);
			report.GenerateFromQueue();

			var sql = "SELECT  ACL_ParentID, ACL_ReportSequence FROM dbo.AccComplianceReportTransactionPivot";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			var group1 = new[] { line2.PK.ToGuid(), line4.PK.ToGuid(), line5.PK.ToGuid(), line8.PK.ToGuid() };
			var group2 = new[] { line1.PK.ToGuid(), line3.PK.ToGuid(), line7.PK.ToGuid(), line6.PK.ToGuid() };

			var sequenceForGroup1 = rows.FirstOrDefault(x => x.Field<Guid>("ACL_ParentID") == group1[0])?.Field<int>("ACL_ReportSequence");
			AssertNotNull(sequenceForGroup1);
			AssertEquals(group1.Length, rows.Count(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup1));
			AssertContainsExactElementsInAnyOrder(group1.Take(2), rows.Where(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup1).Select(x => x.Field<Guid>("ACL_ParentID")).Take(2));
			AssertEquals(true, group1.Skip(2).Take(2).SequenceEqual(rows.Where(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup1).Select(x => x.Field<Guid>("ACL_ParentID")).Skip(2).Take(2)));

			var sequenceForGroup2 = rows.FirstOrDefault(x => x.Field<Guid>("ACL_ParentID") == group2[0])?.Field<int>("ACL_ReportSequence");
			AssertNotNull(sequenceForGroup2);
			AssertEquals(group2.Length, rows.Count(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup2));
			AssertEquals(line1.PK, rows.FirstOrDefault(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup2).Field<Guid>("ACL_ParentID"));
			AssertContainsExactElementsInAnyOrder(group2.Skip(1).Take(2), rows.Where(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup2).Select(x => x.Field<Guid>("ACL_ParentID")).Skip(1).Take(2));
			AssertEquals(line6.PK, rows.LastOrDefault(x => x.Field<int>("ACL_ReportSequence") == sequenceForGroup2).Field<Guid>("ACL_ParentID"));

			void UpdateLineData(InvoicingLineBase line, int numerator, int denominator, int extraRateNumerator = 0, int extraRateDenominator = 1)
			{
				line.AL_TaxRateNumerator = numerator;
				line.AL_TaxRateDenominator = denominator;
				line.AL_TaxExtraRateNumerator = extraRateNumerator;
				line.AL_TaxExtraRateDenominator = extraRateDenominator;
			}
		}

		public void TestGenerateComplianceReport_FromComplianceDocumentOrderByFDN()
		{
			AssertGenerateComplianceReport_FromComplianceDocument(ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
		}

		public void TestGenerateComplianceReport_FromComplianceDocumentOrderByCDN()
		{
			AssertGenerateComplianceReport_FromComplianceDocument(ReportLineOrderingListCodes.ComplianceDocumentNumber);
		}

		void AssertGenerateComplianceReport_FromComplianceDocument(ZString reportLineOrdering)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(report, reportConfigurations, AccComplianceDocumentHeaderSchema.Constants.Prefix, reportLineOrdering: reportLineOrdering);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TXI");
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsReceivable, "", complianceSubType: "TDP");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				Factory.Save();

				var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line.AL_AT = TestObjectCreator.GST1.PK;

				var creditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV002", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line2 = TestObjectCreator.CreateInvoiceLine(creditNote, TestObjectCreator.TWD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
				line2.AL_AT = TestObjectCreator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice, creditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();
				Factory.Save();

				var complianceDocument = Factory.Load<ARComplianceDocumentHeader>(new ZQuery());
				AssertEquals("compliance document is created", 2, complianceDocument.Length);
				complianceDocument[0].ADH_ComplianceSubType = "TDP";
				complianceDocument[0].ADH_DocumentNumber = "001";
				complianceDocument[1].ADH_ComplianceSubType = "TXI";
				complianceDocument[1].ADH_DocumentNumber = "002";
				Factory.Save();

				var complianceDocumentPK1 = complianceDocument[0].PK;
				var complianceDocumentPK2 = complianceDocument[1].PK;

				report.GenerateFromQueue();

				var sql = "SELECT ACL_ParentID, ACL_ReportSubCode, ACL_ParentTableCode, ACL_ACR_Report, ACL_ReportSequence FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence";
				var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
				AssertNotNull("2 compliance report line are generated", rows.Count());
				var firstRow = rows.ElementAt(0);
				var secondRow = rows.ElementAt(1);

				AssertEquals("First Row: ParentTableCode", "ADH", firstRow.Field<string>("ACL_ParentTableCode"));
				AssertEquals("Second Row: ParentTableCode", "ADH", secondRow.Field<string>("ACL_ParentTableCode"));

				AssertEquals("First Row: ReportPK", report.PK, firstRow.Field<Guid>("ACL_ACR_Report"));
				AssertEquals("Second Row: ReportPK", report.PK, secondRow.Field<Guid>("ACL_ACR_Report"));
				if (reportLineOrdering == ReportLineOrderingListCodes.FormatCodeAndDocumentNumber)
				{
					AssertEquals("First Row: ParentID", complianceDocumentPK2, firstRow.Field<Guid>("ACL_ParentID"));
					AssertEquals("Second Row: ParentID", complianceDocumentPK1, secondRow.Field<Guid>("ACL_ParentID"));

					AssertEquals("First Row: ReportSubCode", "31", firstRow.Field<string>("ACL_ReportSubCode"));
					AssertEquals("Second Row: ReportSubCode", "32", secondRow.Field<string>("ACL_ReportSubCode"));
				}
				else
				{
					AssertEquals("First Row: ParentID", complianceDocumentPK1, firstRow.Field<Guid>("ACL_ParentID"));
					AssertEquals("Second Row: ParentID", complianceDocumentPK2, secondRow.Field<Guid>("ACL_ParentID"));

					AssertEquals("First Row: ReportSubCode", "", firstRow.Field<string>("ACL_ReportSubCode"));
					AssertEquals("Second Row: ReportSubCode", "", secondRow.Field<string>("ACL_ReportSubCode"));
				}
			}
		}
	}
}

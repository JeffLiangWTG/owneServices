using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	public class FRDeltaGRegularizationMatchTableProviderTest : TestCaseWithFactory
	{
		public void TestFilters()
		{
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			var dec1 = CreateDeclaration("B10001", Importer1, Supplier1);
			var dec2 = CreateDeclaration("B10002", Importer2, Supplier2);
			var dec3 = CreateDeclaration("B10003", Importer3, Supplier3);

			var today = ZDateTime.Today;
			dec1.JE_DateOfArrival = today.AddDays(-9);
			dec2.JE_DateOfArrival = today.AddDays(-8);
			dec3.JE_DateOfArrival = today.AddDays(-7);

			dec2.JE_GB = branch2.PK;
			dec3.JE_GB = branch3.PK;

			Factory.Save();

			var provider = new FRDeltaGRegularizationMatchTableProvider();
			using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
			{
				AddEmptyFilters(report);

				report.FilterCollection.ClearValues();
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10001",
					"B10002",
					"B10003"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Declaration Branch"]).Value = $"{branch2.GB_Code}, {branch3.GB_Code}";
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10002",
					"B10003"
				);

				report.FilterCollection.ClearValues();
				((TextField)report.FilterCollection["Declarations"]).Value = "B10001, B10002";
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10001",
					"B10002"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Importer"]).Value = Importer1.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10001"
				);

				var yesterdayOrTodayUtc = ZDateTime.UtcNow.AddMinutes(-10).Date;
				var todayOrTomorrowUtc = ZDateTime.UtcNow.AddMinutes(10).Date;

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = yesterdayOrTodayUtc.AddMinutes(30);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = todayOrTomorrowUtc.AddMinutes(31);
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10001",
					"B10002",
					"B10003"
				);

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = yesterdayOrTodayUtc.AddMinutes(30).AddDays(-1);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = yesterdayOrTodayUtc.AddMinutes(31).AddDays(-1);
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}");

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueLow = todayOrTomorrowUtc.AddMinutes(30).AddDays(1);
				((DateRangeField)report.FilterCollection["Job Registered On"]).ValueHigh = todayOrTomorrowUtc.AddMinutes(31).AddDays(1);
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}");

				report.FilterCollection.ClearValues();
				((DateRangeField)report.FilterCollection["Import Date"]).ValueLow = today.AddDays(-8).AddHours(5);
				((DateRangeField)report.FilterCollection["Import Date"]).ValueHigh = today.AddDays(-8).AddHours(15);
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10002"
				);

				report.FilterCollection.ClearValues();
				((LookupField)report.FilterCollection["Supplier"]).Value = Supplier2.PK.ToGuid();
				AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
					"B10002"
				);
			}
		}

		public void TestBatching()
		{
			var declaration1 = CreateDeclaration("B10001", Importer1, Supplier1);

			var declaration2 = CreateDeclaration("B10002", Importer2, Supplier2);

			var declaration3 = CreateDeclaration("B10003", Importer3, Supplier3);

			Factory.Save();

			// Results should be the same when batch size is below and above number of declatarions, but number of DB hits should be different.

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 100, expectedHitCounts: new Dictionary<string, int>
			{
				// 2 queries for JobDeclaration (one to estimate total count, one to load batch)
				{ JobDeclarationSchema.Constants.TableName, 2 }
			}))
			{
				var provider = new FRDeltaGRegularizationMatchTableProvider();
				using (var report = new Report(null, null, Guid.Empty, Constants.DataContext.None))
				{
					AddEmptyFilters(report);
					AssertReport(provider, report, r => $"{r.JE_DeclarationReference}",
						"B10001",
						"B10002",
						"B10003"
					);
				}
			}
		}

		void AssertReport(FRDeltaGRegularizationMatchTableProvider provider, Report report, Func<FRDeltaGRegularizationMatchDataSet.FRDeltaGRegularizationMatchDataSetRow, string> lineFormat, params string[] expectedLines)
		{
			var dataTable = GetDataTable(provider, report);
			AssertMultilineASCIIEquals(
				string.Join("\r\n", expectedLines),
				string.Join("\r\n", dataTable.Rows.OfType<FRDeltaGRegularizationMatchDataSet.FRDeltaGRegularizationMatchDataSetRow>().Select(lineFormat))
			);
		}

		static DataTable GetDataTable(FRDeltaGRegularizationMatchTableProvider provider, Report report)
		{
			return provider.GetDataTable("SomeTable",
				"SomeReport(<Declaration Branch>, <Declarations>, <Job Registered On->FromDate>, <Job Registered On->ToDate>, <Import Date->FromDate>, <Import Date->ToDate>, <Importer>, <Supplier>)",
				report, false);
		}

		void AddEmptyFilters(Report report)
		{
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Declaration Branch",
				Value = string.Empty
			});
			report.FilterCollection.Add(new TextField(Factory)
			{
				DisplayName = "Declarations",
				Value = string.Empty
			});
			report.FilterCollection.Add(new DateRangeField(Factory)
			{
				DisplayName = "Job Registered On",
				ValueLow = ZDateTime.Empty,
				ValueHigh = ZDateTime.Empty
			});
			report.FilterCollection.Add(new DateRangeField(Factory)
			{
				DisplayName = "Import Date",
				ValueLow = ZDateTime.Empty,
				ValueHigh = ZDateTime.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Importer",
				Value = Guid.Empty
			});
			report.FilterCollection.Add(new LookupField(Factory)
			{
				DisplayName = "Supplier",
				Value = Guid.Empty
			});
		}

		JobDeclaration CreateDeclaration(string referenceNumber, OrgHeader importer, OrgHeader supplier)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = referenceNumber;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Importer1 = Factory.NewWithValidTestData<OrgHeader>();
			Importer1.OH_Code = "TESTIMP1";

			Importer2 = Factory.NewWithValidTestData<OrgHeader>();
			Importer2.OH_Code = "TESTIMP2";

			Importer3 = Factory.NewWithValidTestData<OrgHeader>();
			Importer3.OH_Code = "TESTIMP3";

			Supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier1.OH_Code = "TESTSUP1";

			Supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier2.OH_Code = "TESTSUP2";

			Supplier3 = Factory.NewWithValidTestData<OrgHeader>();
			Supplier3.OH_Code = "TESTSUP3";
		}

		OrgHeader Importer1 { get; set; }
		OrgHeader Importer2 { get; set; }
		OrgHeader Importer3 { get; set; }
		OrgHeader Supplier1 { get; set; }
		OrgHeader Supplier2 { get; set; }
		OrgHeader Supplier3 { get; set; }
	}
}

using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class ComplianceReportDataTest : TestCaseWithFactory
	{
		[TestDate(2023, 1, 7)]
		public void TestGetEDocViaUniversalXmlSupport()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			var uxmlSupport = new AccComplianceReportData().GetEDocsViaUniversalXmlSupport();
			var nowDate = ZDate.Today;
			report1.ACR_DateFrom = nowDate.AddDays(-14);
			report1.ACR_DateTo = nowDate.AddDays(-7);
			report1.ACR_ReportType = AccComplianceReport.ReportTypes.SAFTOnlyTransactions;
			report1.ACR_GC_Company = Env.CurrentCompany.PK;
			Factory.Save();

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_DateFrom = nowDate.AddDays(-6);
			report2.ACR_DateTo = nowDate;
			report2.ACR_ReportType = AccComplianceReport.ReportTypes.SAFTOnlyTransactions;
			report2.ACR_GC_Company = Env.CurrentCompany.PK;
			Factory.Save();

			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report3.ACR_DateFrom = nowDate.AddDays(-7);
			report3.ACR_DateTo = nowDate;
			report3.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			report3.ACR_GC_Company = Env.CurrentCompany.PK;
			Factory.Save();

			AssertEquals(report1.PK, uxmlSupport.LoadBusinessObjectFromCode(Factory, $"SAT|{nowDate.AddDays(-14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}")?.PK);
			AssertNull("Expecting a key in the format 'ACR ReportType|ReportDateFrom, report type is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "2023-01-01"));
			AssertNull("Expecting a key in the format 'ACR ReportType|ReportDateFrom, report date from is required", uxmlSupport.LoadBusinessObjectFromCode(Factory, "SAT"));
			AssertNull("There is no business object matching the criteria, report from date is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "SAT|2024-01-01"));
			AssertNull("There is no business object matching the criteria,report type is incorrect", uxmlSupport.LoadBusinessObjectFromCode(Factory, "SATT |2023-01-01"));
		}

		public void TestNullOrWhiteSpaceArqumentThrows()
		{
			var uxmlSupport = new AccComplianceReportData().GetEDocsViaUniversalXmlSupport();
			AssertExceptionThrown<ArgumentException>("Factory Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(null, "SAT|2023-01-01"));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be string empty", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, ""));
			AssertExceptionThrown<ArgumentException>("Code Argument could not be null", () => uxmlSupport.LoadBusinessObjectFromCode(Factory, null));
		}

		public void TestGetEDocViaUniversalXmlSupportExpectedCodeAndExampleCodeFormat()
		{
			AssertEquals("SAT|2023-02-02", new AccComplianceReportData().GetEDocsViaUniversalXmlSupport().ExampleCodeFormat);
			AssertEquals("ReportTypeCode|ReportDateFrom", new AccComplianceReportData().GetEDocsViaUniversalXmlSupport().ExpectedCodeFormat);
		}
	}
}

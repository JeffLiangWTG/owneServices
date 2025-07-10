using CargoWise.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.Business.Test
{
	[TestedType(typeof(StmErrorReport))]
	sealed class StmErrorReportTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasCodeAsErrorReportID()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = XmlWithReportID;
			var codeDescription = (ICodeDescription)report;

			AssertEquals(report.ErrorReportID, codeDescription.Code);
		}

		public void TestErrorReportIDFromXML()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = XmlWithReportID;

			AssertEquals("E123456", report.ErrorReportID);
		}

		public void TestErrorReportIDWhenXMLCannotBeParsed()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = "{ 'jason': { 'is': [ 'a', 'pretty', 'cool', 'guy' ] } }";

			AssertEquals(string.Empty, report.ErrorReportID);
		}

		public void TestErrorReportIDWhenXMLElementIsAWOL()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = "<EDI_Exception_Report><NotWhatYou>arelookingfor</NotWhatYou></EDI_Exception_Report>";

			AssertEquals(string.Empty, report.ErrorReportID);
		}

		public void TestDoesNotCacheErrorReportIDAcrossNewQERReportXmlValues()
		{
			var report = Factory.New<StmErrorReport>();

			report.QER_ReportXml = XmlWithReportID;
			AssertEquals("E123456", report.ErrorReportID);

			report.QER_ReportXml = XmlWithReportID2;
			AssertEquals("RAWR", report.ErrorReportID);

			report.QER_ReportXml = XmlWithReportID;
			AssertEquals("E123456", report.ErrorReportID);
		}

		public void TestHumanReadableNameIsErrorReportID()
		{
			var report = Factory.New<StmErrorReport>();
			report.QER_ReportXml = XmlWithReportID;

			AssertEquals(report.ErrorReportID, report.HumanReadableName);
		}

		const string XmlWithReportID = "<EDI_Exception_Report><ErrorReportID>E123456</ErrorReportID></EDI_Exception_Report>";
		const string XmlWithReportID2 = "<EDI_Exception_Report><ErrorReportID>RAWR</ErrorReportID></EDI_Exception_Report>";
	}
}

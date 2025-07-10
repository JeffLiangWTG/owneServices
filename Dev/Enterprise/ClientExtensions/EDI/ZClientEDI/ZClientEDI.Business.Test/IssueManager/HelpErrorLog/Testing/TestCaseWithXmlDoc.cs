using System;
using System.Globalization;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	public abstract class TestCaseWithXmlDoc : TestCaseWithFactory
	{
		public string GetSampleFilePath(string fileName) => Path.Combine(resourcesFolder, "ZClientEDI.Business.Test.IssueManager.SampleFiles." + fileName);
		public string GetTestFilePath(string fileName) => Path.Combine(resourcesFolder, "ZClientEDI.Business.Test.IssueManager.HelpErrorLog.Testing." + fileName);

		EmbeddedResourceRetriever resourceRetriever;
		string resourcesFolder;
		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(typeof(TestCaseWithXmlDoc).Assembly);
			resourcesFolder = resourceRetriever.SaveAllResourcesToFiles();
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		protected string SampleTestFile => GetTestFilePath("SampleExceptionReport.xml");
		protected string SampleBrokenXML => GetSampleFilePath("SampleBrokenXML.xml");
		protected string ClientVisibleTestFile => GetTestFilePath("SampleExceptionReport.xml");
		protected string SmallCallstackTestFile => GetTestFilePath("SampleExceptionReport_SmallCallstack.xml");
		protected string SilentExceptionTestFile => GetTestFilePath("SampleExceptionReport_SmallCallstack.xml");
		protected string SmallCallstackTestFileWithKey => GetTestFilePath("SampleExceptionReport_SmallCallstackWithKey.xml");
		protected string SmallCallstackTestFileWithGuidKey => GetTestFilePath("SampleExceptionReport_SmallCallstackWithGuidKey.xml");
		protected string SampleGlowWindowsCEFile => GetTestFilePath("SampleExceptionReport_GlowWindowsCE.xml");
		protected string SampleGlowWebFileWithFullStackTraceData => GetTestFilePath("SampleExceptionReport_GlowWebWithFullStackTraceData.xml");
		protected string SampleGlowWebFileWithFullStackTraceData2 => GetTestFilePath("SampleExceptionReport_GlowWebWithFullStackTraceData2.xml");
		protected string SampleNonEnglishMessageExceptionXML => GetTestFilePath("SampleExceptionReport_NonEglishMessage.xml");

		protected enum ErrorReportIDFormat
		{
			FromEnterpriseNumberFountain,
			RandomlyGenerated
		}

		protected ErrorReportIDFormat[] AcceptableErrorReportIDFormats = new[] { ErrorReportIDFormat.FromEnterpriseNumberFountain, ErrorReportIDFormat.RandomlyGenerated };
		protected char[] AcceptableErrorReportIDPrefixes = new[] { 'E', 'R' };
		const string exeCreationTimeReplaceString = "#EXECREATIONTIME#";
		const string exeOccurrenceReplaceString = "#OCCURRENCEID#";
		const string exeTimeOfExceptionReplaceString = "#TIMEOFEXCEPTION#";
		const string exceptionMessageReplaceString = "#EXCEPTIONMESSAGE#";

		protected string CreateOccurrenceXml(ZDateTime exeCreationTime, ZDateTime timeOfException, ZString occurrenceId, string exceptionMessage = "This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.", string key = "", string exceptionType = null, string exceptionDescription = null)
		{
			AssertEquals("Sanity check", AcceptableErrorReportIDFormats.Length, AcceptableErrorReportIDPrefixes.Length);

			var acceptabilityIndex = Array.IndexOf(AcceptableErrorReportIDPrefixes, occurrenceId[0]);
			var format = AcceptableErrorReportIDFormats[acceptabilityIndex];

			switch (format)
			{
				case ErrorReportIDFormat.FromEnterpriseNumberFountain:
					AssertEquals("occurrence id format", 17, occurrenceId.Length);
					Assert("occurrence id format", occurrenceId.Substring(1, 8).IsNumbersOnlyOrEmpty);
					AssertContains("occurrence id format", "-", occurrenceId);
					break;

				case ErrorReportIDFormat.RandomlyGenerated:
					AssertEquals("occurrence id format", 19, occurrenceId.Length);
					Assert("occurrence id format", occurrenceId.Substring(1).IsNumbersOnlyOrEmpty);
					break;

				default:
					Assert("Unknown error report ID format", false);
					break;
			}

			var result = LogTemplate;

			if (!string.IsNullOrEmpty(key))
			{
				result = result.Replace("<Key>CF2F07A1-07E9-4414-9D74-2A1AC4B36E53</Key>", "<Key>" + key + "</Key>");
			}
			result = result.Replace(exeCreationTimeReplaceString, exeCreationTime.ToString("dd-MMM-yy HH:mm:sss"));
			result = result.Replace(exeOccurrenceReplaceString, occurrenceId);
			result = result.Replace(exeTimeOfExceptionReplaceString, timeOfException.ToString("dd-MMM-yy HH:mm:sss"));
			result = result.Replace(exceptionMessageReplaceString, exceptionMessage);

			if (!string.IsNullOrEmpty(exceptionType))
			{
				result = UpdateElementValue(result, "ExceptionType", exceptionType);
			}

			if (!string.IsNullOrEmpty(exceptionDescription))
			{
				result = UpdateElementValue(result, "ExceptionDescription", exceptionDescription);
			}

			return result;
		}

		string UpdateElementValue(string xml, string elementName, string value)
		{
			var elementXml = "<" + elementName + ">";
			var start = xml.LastIndexOf(elementXml) + elementXml.Length;
			var end = xml.LastIndexOf("</" + elementName + ">") - start;
			return xml.Replace(xml.Substring(start, end), value);
		}

		protected string LogTemplate
		{
			get
			{
				string result = File.ReadAllText(ClientVisibleTestFile)
						.Replace("<ExeCreationTime>08-Jun-05 10:56:08</ExeCreationTime>", string.Format(CultureInfo.CurrentCulture, "<ExeCreationTime>{0}</ExeCreationTime>", exeCreationTimeReplaceString))
						.Replace("<TimeOfException>27-Jun-05 10:04:11</TimeOfException>", string.Format(CultureInfo.CurrentCulture, "<TimeOfException>{0}</TimeOfException>", exeTimeOfExceptionReplaceString))
						.Replace("<Message>This row has been removed from a table and does not have any data.  BeginEdit() will allow creation of new data in this row.</Message>", string.Format(CultureInfo.CurrentCulture, "<Message>{0}</Message>", exceptionMessageReplaceString));

				result = result.Replace("E00000199", "#OCCURRENCEID#");

				return result;
			}
		}
	}
}

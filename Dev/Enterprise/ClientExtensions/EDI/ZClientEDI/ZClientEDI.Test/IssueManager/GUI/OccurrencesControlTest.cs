using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Test;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Client.EDI.IssueManager.GUI.OccurrencesControl;

namespace Enterprise.Client.EDI.IssueManager.GUI.Testing
{
	public class OccurrencesControlTest : TestCaseWithXmlDoc
	{
		public class FormForTest : ZForm
		{
			public FormForTest(object helpErrorLog) : base(helpErrorLog)
			{
			}

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				Controls.Add(UserControl);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (userControl != null)
					{
						userControl.Dispose();
					}
				}

				base.Dispose(disposing);
			}

			OccurrencesControlForTest userControl;
			public OccurrencesControlForTest UserControl
			{
				get
				{
					return userControl ?? (userControl = new OccurrencesControlForTest());
				}
			}
		}

		public class OccurrencesControlForTest : OccurrencesControl
		{
			public TempFile LastExportedFile { get; private set; }
			public object LastOpenExportedFileHandler { get; private set; }
			public bool EnableOpenFileException { get; set; }

			readonly bool isCalculateMoreDetails;

			public OccurrencesControlForTest(bool isCalculate = true)
			{
				isCalculateMoreDetails = isCalculate;
			}

			public new ZGrid OccurrencesGrid
			{
				get
				{
					return base.OccurrencesGrid;
				}
			}

			protected override object OpenFile(TempFile tempFile)
			{
				var fileName = tempFile.Filename;
				LastExportedFile = tempFile;
				if (EnableOpenFileException)
				{
					throw new FileNotFoundException("File not found.", fileName);
				}

				LastOpenExportedFileHandler = base.OpenFile(tempFile);
				return LastExportedFile;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					LastExportedFile?.Dispose();
				}

				base.Dispose(disposing);
			}
			protected override bool IsContinueCalculateMoreDetails()
			{
				return isCalculateMoreDetails;
			}

			internal Hashtable ObtainReportHash()
			{
				return reportHash;
			}

			internal ZRichTextBox ObtainRichTextBox()
			{
				return richTextBox;
			}
		}

		#region Export To Xml tests

		public void TestExportErrorReportToXmlFileMenuItemExists()
		{
			var xmlData = File.ReadAllText(ClientVisibleTestFile);
			var log = Factory.New<EdiHelpErrorLog>();
			var occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_XMLData = xmlData;
			using (var form = new FormForTest(log))
			using (var grid = form.UserControl.OccurrencesGrid)
			{
				var exportReportToXmlFileMenuItem = grid.ContextMenu.MenuItems.FindByText("Export Error Report To XML File");

				AssertNotNull(exportReportToXmlFileMenuItem);
			}
		}

		public void TestExportErrorReportToXmlFile()
		{
			var xmlData = $"This is for export error report to xml menuItem test {ZDateTime.UtcNow}";
			var log = Factory.New<EdiHelpErrorLog>();
			var occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_XMLData = xmlData;
			using (var form = new FormForTest(log))
			using (var grid = form.UserControl.OccurrencesGrid)
			{
				form.Show();
				Application.DoEvents();
				var exportReportToXmlFileMenuItem = grid.ContextMenu.MenuItems.FindByText("Export Error Report To XML File");
				AssertNotNull(exportReportToXmlFileMenuItem);
				exportReportToXmlFileMenuItem.PerformClick();
				var expectedXml = File.ReadAllText(form.UserControl.LastExportedFile.Filename);
				AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, xmlData);
				AssertNotNull(form.UserControl.LastOpenExportedFileHandler);
			}
		}

		public void TestExportErrorReportToXmlFileFailedToAccessFile()
		{
			var xmlData = $"This is for export error report to xml menuItem test {ZDateTime.UtcNow}";
			var log = Factory.New<EdiHelpErrorLog>();
			var occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_XMLData = xmlData;

			using (var form = new FormForTest(log))
			using (var grid = form.UserControl.OccurrencesGrid)
			{
				form.UserControl.EnableOpenFileException = true;
				form.Show();
				Application.DoEvents();
				var exportReportToXmlFileMenuItem = grid.ContextMenu.MenuItems.FindByText("Export Error Report To XML File");
				AssertNotNull(exportReportToXmlFileMenuItem);

				exportReportToXmlFileMenuItem.PerformClick();
				var msg = UnitTestUserNotification.Instance.LastMessage.Text;

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Failed to open exported error report xml file"));
			}
		}
		#endregion
		public void TestShowCurrentOccurrence()
		{
			string xmlData = File.ReadAllText(ClientVisibleTestFile);
			EdiHelpErrorLog log = Factory.New<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_XMLData = xmlData;
			HelpErrorLogOccurrence occurrence2 = log.Occurrences.AddNew();
			occurrence2.HO_XMLData = xmlData;
			HelpErrorLogOccurrence occurrenceWithEmptyXML = log.Occurrences.AddNew();
			occurrenceWithEmptyXML.HO_XMLData = String.Empty;
			int beforeCount = Directory.GetFiles(Env.TempPath).Length;
			int afterCount;
			using (var form = new ZForm())
			using (var oc = new OccurrencesControlForTest())
			{
				form.Controls.Add(oc);
				oc.SetDataBinding(log, "");
				form.Show();
				Application.DoEvents();
				afterCount = Directory.GetFiles(Env.TempPath).Length;
				AssertEquals("After one issue viewed", beforeCount + 1, afterCount);
				oc.OccurrencesBindingManager.Position++;
				afterCount = Directory.GetFiles(Env.TempPath).Length;
				AssertEquals("After two issues viewed", beforeCount + 2, afterCount);
				oc.OccurrencesBindingManager.Position++;
				afterCount = Directory.GetFiles(Env.TempPath).Length;
				AssertEquals("After empty XML issue viewed", beforeCount + 2, afterCount);
			}

			afterCount = Directory.GetFiles(Env.TempPath).Length;
			AssertEquals("All temp files deleted", beforeCount, afterCount);
		}

		#region TestViewLocallyButtonProcess

		public void TestViewLocallyButtonExistsOnCW1()
		{
			using (var form = new ZForm())
			using (var occControl = new OccurrencesControlForTest())
			{
				form.Controls.Add(occControl);
				form.Show();
				var viewLocallyBtn = form.Controls.Find("ViewLocallyButton", true);

				AssertEquals(1, viewLocallyBtn.Length);
				AssertEquals("View Locally", viewLocallyBtn[0].Text);
			}
		}

		public void TestViewLocallyButtonProcess()
		{
			try
			{
				AddReleaseVersionForTest();
				AddInnerExceptionIssueForTest();
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest())
				{
					form.Controls.Add(occControl);
					var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
					occControl.SetDataBinding(log, "");
					form.Show();

					var occurrence = occControl.OccurrencesBindingManager.GetCurrent() as HelpErrorLogOccurrence;
					var cacheFile = (TempFile)occControl.ObtainReportHash()[occurrence];
					var cacheFileContent = File.ReadAllText(cacheFile.Filename);

					AssertNotNull(occControl.viewLocallyButton);
					AssertEquals("View Locally", occControl.viewLocallyButton.Text);
					occControl.viewLocallyButton.PerformClick();
					var viewLocallyFile = occControl.LastExportedFile;
					var html = File.ReadAllText(viewLocallyFile.Filename);
					AssertNotNull(occControl.LastOpenExportedFileHandler);
					AssertEquals(cacheFileContent, html);
				}
			}
			finally
			{
				DirectoryInfo tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (FileInfo file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (DirectoryInfo dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		#endregion

		#region TestMoreDetailsButtonProcess
		class ExceptionInnerClass
		{
			public class ExceptionInnerMostClass
			{
				public static void ExceptionFunction(int level)
				{
					string str = null;
					if (level == 0)
					{
						if (str.Length > 0)
						{
							return;
						}
					}

					ExceptionFunction(level - 1);
				}
			}
		}

		void AddIssueForTest()
		{
			TestCaseHelper.ClearTable("HelpErrorLogOccurrence");
			TestCaseHelper.ClearTable("HelpErrorLogKey");
			TestCaseHelper.ClearTable("HelpErrorLog");
			// create an issue
			ExceptionReportBuilder testReportBuilder = null;
			try
			{
				ExceptionInnerClass.ExceptionInnerMostClass.ExceptionFunction(3);
			}
			catch (Exception ex)
			{
				ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key add line no", "Test Error add line no");
				reportArgs.SessionId = new Guid("15F3F612-6749-4F45-92E5-713ED6D863E9");
				reportArgs.Sequence = 5678;
				testReportBuilder = new ExceptionReportBuilder(reportArgs);
			}

			string attachment = testReportBuilder.GenerateReport();
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			IHelpErrorLogCollection errorLogs = new ScalableHelpErrorLogCollection(factory);
			new ExceptionXml(attachment).Process(errorLogs, true);
		}

		void AddReleaseVersionForTest()
		{
			var requiredFiles = new[]
			{
				"ZClientEDI.dll",
				"ZClientEDI.Test.dll",
				"Enterprise.ZArchitecture.Core.dll",
				"NUnitCore.dll",
				"CargoWise.EntityFramework.dll",
				"CargoWise.Data.dll",
				ExeFileNames.CargoWiseOneExeForVersionInfo
			};

			var sourceFolder = RuntimePackageBuilderTest.GetTrueRootBinPath();

			var mainExeFile = Path.Combine(sourceFolder, ExeFileNames.CargoWiseOneExeForVersionInfo);
			if (!File.Exists(mainExeFile))
			{
				throw new FileNotFoundException($"{ExeFileNames.CargoWiseOneExeForVersionInfo} not found in {sourceFolder}");
			}

			var versionNumber = new VersionNumber(FileVersionInfo.GetVersionInfo(mainExeFile));

			var zipFolder = Path.Combine(Env.TempPath, "FilesNeeded");
			var destFolder = Path.Combine(zipFolder, "Distribution", "Application");
			Directory.CreateDirectory(destFolder);

			foreach (var fileName in requiredFiles)
			{
				CopyIfExists(fileName);
				CopyIfExists(Path.ChangeExtension(fileName, ".pdb"));
			}

			void CopyIfExists(string fileName)
			{
				var sourceFile = Path.Combine(sourceFolder, fileName);
				if (File.Exists(sourceFile))
				{
					File.Copy(sourceFile, Path.Combine(destFolder, fileName));
				}
			}

			ImportableBuild.ExtractReleaseInfoXml(destFolder);
			var packageName = ReleaseBuild.GetPackageName(versionNumber);
			string packageFile = Path.Combine(Env.TempPath, packageName);
			ZipFile.CreateFromDirectory(zipFolder, packageFile);
			using (var importer = new PackageImporter(Factory, packageFile))
			{
				importer.Import();
			}
		}

		public void TestCalcLineNoPackagePath()
		{
			CalcLineErrorCore(releaseBuild => releaseBuild.HL_PackagePath = string.Empty, version => $"Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseBuildException: Software release build ver {version} does not have a valid Package Path.");
		}

		public void TestCalcLineNoLongerActive()
		{
			CalcLineErrorCore(releaseBuild => releaseBuild.HL_IsActive = false, version => $"Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseBuildException: Software release build version {version} has not been retained.");
		}

		public void CalcLineErrorCore(Action<ReleaseBuild> ruinTheReleaseBuild, Func<VersionNumber, string> errorMessage)
		{
			try
			{
				AddReleaseVersionForTest();
				AddIssueForTest();
				var releaseBuild = Factory.LoadTop1<ReleaseBuild>(new ZQuery());
				ruinTheReleaseBuild(releaseBuild);
				var version = releaseBuild.VersionNumber;
				Factory.Save();
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest())
				{
					form.Controls.Add(occControl);
					var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
					occControl.SetDataBinding(log, "");
					form.Show();
					// check data in db
					{
						var xml = (string)Db.Connection.ExecuteScalar("SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence");
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(xml);
						Assert(ExceptionReportRenderer.GetLineNoInfoState(xmlDoc.DocumentElement["ExceptionDetails"]) == ExceptionReportRenderer.LineNoInfoState.HAS_ILOFFSET_INFO);
					}

					// check html
					var moreDetailButton = occControl.moreDetailsButton;
					Assert(moreDetailButton.Text.Equals("More Details"));
					moreDetailButton.PerformClick();

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(errorMessage(version)));
					AssertNull(occControl.LastExportedFile);
					AssertNull(occControl.LastOpenExportedFileHandler);
				}
			}
			finally
			{
				var tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (var file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (var dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		public void TestMoreDetailsButtonProcess_WhenChooseNo()
		{
			try
			{
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest(false))
				{
					form.Controls.Add(occControl);
					var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
					occControl.SetDataBinding(log, "");
					form.Show();

					occControl.moreDetailsButton.PerformClick();
					var moreDetailsFile = occControl.LastExportedFile;
					AssertNull(moreDetailsFile);
					AssertNull(occControl.LastOpenExportedFileHandler);
				}
			}
			finally
			{
				var tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (var file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (var dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		public void TestMoreDetailsButtonProcess()
		{
			try
			{
				AddReleaseVersionForTest();
				AddIssueForTest();
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest())
				{
					form.Controls.Add(occControl);
					var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
					occControl.SetDataBinding(log, "");
					form.Show();
					// check data in db
					{
						var xml = (string)Db.Connection.ExecuteScalar("SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence");
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(xml);
						Assert(ExceptionReportRenderer.GetLineNoInfoState(xmlDoc.DocumentElement["ExceptionDetails"]) == ExceptionReportRenderer.LineNoInfoState.HAS_ILOFFSET_INFO);
					}

					occControl.moreDetailsButton.PerformClick();
					var moreDetailsFile = occControl.LastExportedFile;
					var html = File.ReadAllText(moreDetailsFile.Filename);

					// check data in db again
					{
						var xml = (string)Db.Connection.ExecuteScalar("SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence");
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(xml);
						Assert(ExceptionReportRenderer.GetLineNoInfoState(xmlDoc.DocumentElement["ExceptionDetails"]) == ExceptionReportRenderer.LineNoInfoState.HAS_LINENO_INFO);
					}

					// check html
					AssertNotNull(occControl.LastOpenExportedFileHandler);
					var doc = new HtmlAgilityPack.HtmlDocument();
					doc.LoadHtml(html);

					AssertNotNull(doc.GetElementbyId("overlay"));
					AssertNotNull(doc.GetElementbyId("MsgBoxContent"));

					var messageBox = doc.DocumentNode
						.SelectSingleNode("//div[@class='message-box']");
					AssertNotNull(messageBox);

					var closeBtn = messageBox.SelectSingleNode(".//button[@class='close-btn' and @onclick='closeMessageBox()']");
					AssertNotNull(closeBtn);

					var sourceLinks = doc.DocumentNode
						.SelectNodes("//*[starts-with(@name, 'sourceLineNoHyperLink')]");
					AssertNotNull("Expected elements with name='sourceLineNoHyperLink'", sourceLinks);
					Assert("Expected at least one sourceLineNoHyperLink element", sourceLinks.Count > 0);

					var links = doc.DocumentNode
						.SelectNodes("//a[starts-with(@name, 'advancedLineNoHyperLink')]");
					AssertNotNull(links);
					Assert("Expected at least one advancedLineNoHyperLink anchor", links.Count > 0);

					var scriptTags = doc.DocumentNode.SelectNodes("//script");
					Assert("Expected script defining closeMessageBox", scriptTags.Any(script => script.InnerText.Contains($"function closeMessageBox()")));
					Assert("Expected script defining openMessageBox", scriptTags.Any(script => script.InnerText.Contains($"function openMessageBox()")));

					// Get all script contents
					var scriptNodes = doc.DocumentNode.SelectNodes("//script") ?? new HtmlAgilityPack.HtmlNodeCollection(null);
					var scriptContents = scriptNodes.Select(n => n.InnerText).ToList();

					var matchedScript = scriptContents.FirstOrDefault(script =>
						script.Contains("advancedLineNoHyperLinks.forEach") &&
						script.Contains("addEventListener(\"click\"") &&
						script.Contains("getElementById(\"MsgBoxContent\")") &&
						script.Contains("atob(encodedValue)") &&
						script.Contains("openMessageBox();"));

					if (matchedScript == null)
					{
						var combinedScriptDump = string.Join(
							"\n--- SCRIPT ---\n",
							scriptContents.Select((s, i) => $"[Script {i}]:\n{s.Trim()}"));

						Fail($"Expected JavaScript block not found.\nSearched for key markers in <script> tags.\nAvailable scripts:\n{combinedScriptDump}");
					}
				}
			}
			finally
			{
				var tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (var file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (var dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		#endregion
		#region TestCalcLineNoWithSpecialTypeName
		class ExceptionSpecialTypeName
		{
			public static void CreateException()
			{
				foreach (var str in MyList())
				{
					if (str.Length == 0)
					{
						return;
					}
				}
			}

			public static IEnumerable<string> MyList()
			{
				yield return "ABC";
				string s = null;
				if (s.Length == 0)
				{
					yield return "DEF";
				}
			}
		}

		void AddSpecialTypeNameIssueForTest()
		{
			TestCaseHelper.ClearTable("HelpErrorLogOccurrence");
			TestCaseHelper.ClearTable("HelpErrorLogKey");
			TestCaseHelper.ClearTable("HelpErrorLog");
			// create an issue
			ExceptionReportBuilder testReportBuilder = null;
			try
			{
				ExceptionSpecialTypeName.CreateException();
			}
			catch (Exception ex)
			{
				var reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key add line no", "Test Error add line no");
				reportArgs.SessionId = new Guid("4E25FA97-451A-44E1-9F72-554C6FC24412");
				reportArgs.Sequence = 5678;
				testReportBuilder = new ExceptionReportBuilder(reportArgs);
			}

			var attachment = testReportBuilder.GenerateReport();
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var errorLogs = new ScalableHelpErrorLogCollection(factory);
			new ExceptionXml(attachment).Process(errorLogs, true);
		}

		static HtmlAgilityPack.HtmlNode RetrieveMatchedNode(string html, string pattern)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);

			var matchedNode = doc.DocumentNode
				.Descendants()
				.FirstOrDefault(n =>
					n.Attributes.Any(a =>
						a.Name == "type" &&
						a.Value.Contains(pattern, StringComparison.Ordinal)));

			return matchedNode;
		}

		public void TestCalcLineNoWithSpecialTypeName()
		{
			// Arrange
			AddSpecialTypeNameIssueForTest();
			using var form = new ZForm();
			using var occControl = new OccurrencesControlForTest();

			form.Controls.Add(occControl);
			var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
			occControl.SetDataBinding(log, string.Empty);
			form.Show();
			var occurrenceFile = (TempFile)occControl.ObtainReportHash()[log.Occurrences.First()];
			var html = File.ReadAllText(occurrenceFile.Filename);

			// Act
			var matchedNode = RetrieveMatchedNode(html, "Enterprise.Client.EDI.IssueManager.GUI.Testing.OccurrencesControlTest.ExceptionSpecialTypeName.&lt;MyList&gt;d__1");

			// Assert
			AssertNotNull("Expected to find a node with the given type attribute.", matchedNode);
		}

		#endregion
		#region TestCalcLineNoException
		public void TestCalcLineNoException()
		{
			AddIssueForTest();
			using (var form = new ZForm())
			using (var occControl = new OccurrencesControlForTest())
			{
				form.Controls.Add(occControl);
				var log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
				occControl.SetDataBinding(log, "");
				form.Show();
				var occurrence = occControl.OccurrencesBindingManager.GetCurrent() as HelpErrorLogOccurrence;
				var errMsg = "";
				try
				{
					CalculateLineNo.Calculate(occurrence);
				}
				catch (Exception ex)
				{
					errMsg = ex.Message;
				}

				Assert(errMsg, errMsg.Contains("Software release build ver"));
				Assert(errMsg, errMsg.Contains("not found in database"));
			}
		}

		#endregion
		#region TestInnerExceptionLineNo
		void ExceptionFunctionInnerMost(int level)
		{
			string str = null;
			if (level == 0)
			{
				if (str.Length > 0)
				{
					return;
				}
			}

			ExceptionFunctionInnerMost(level - 1);
		}

		void ExceptionFunctionOutter()
		{
			try
			{
				ExceptionFunctionInnerMost(3);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Caught inner exception", e);
			}
		}

		void AddInnerExceptionIssueForTest()
		{
			TestCaseHelper.ClearTable("HelpErrorLogOccurrence");
			TestCaseHelper.ClearTable("HelpErrorLogKey");
			TestCaseHelper.ClearTable("HelpErrorLog");
			// create an issue
			ExceptionReportBuilder testExceptionReportBuilder = null;
			try
			{
				ExceptionFunctionOutter();
			}
			catch (Exception ex)
			{
				ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key add line no", "Test Error add line no");
				reportArgs.SessionId = new Guid("A8C7067A-E676-45B6-A7AE-936E0EBBA305");
				reportArgs.Sequence = 1234;
				testExceptionReportBuilder = new ExceptionReportBuilder(reportArgs);
			}

			var report = testExceptionReportBuilder.GenerateReport();
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			IHelpErrorLogCollection errorLogs = new ScalableHelpErrorLogCollection(factory);
			new ExceptionXml(report).Process(errorLogs, true);
		}

		public void TestInnerExceptionLineNo()
		{
			try
			{
				AddReleaseVersionForTest();
				AddInnerExceptionIssueForTest();
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest())
				{
					form.Controls.Add(occControl);
					EdiHelpErrorLog log = Factory.LoadTop1<EdiHelpErrorLog>(new ZQuery());
					occControl.SetDataBinding(log, "");
					form.Show();
					occControl.moreDetailsButton.PerformClick();

					string xml = (string)Db.Connection.ExecuteScalar("SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence");
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xml);
					var exceptionDetailsNode = xmlDoc.DocumentElement["ExceptionDetails"];
					Assert(exceptionDetailsNode != null);
					Assert(ExceptionReportRenderer.GetLineNoInfoState(exceptionDetailsNode["InnerException"]) == ExceptionReportRenderer.LineNoInfoState.HAS_LINENO_INFO);
				}
			}
			finally
			{
				DirectoryInfo tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (FileInfo file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (DirectoryInfo dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		#endregion
		#region TestOverloadedMethodsLineNo
		void ExceptionFunctionOverloaded(int level, string name = "")
		{
			string str = null;
			if (level == 0)
			{
				if (str.Length > 0)
				{
					return;
				}
			}

			var newLevel = level - 1;
			ExceptionFunctionOverloaded(ref newLevel);
		}

		EdiHelpErrorLog AddOverloadedMethodsIssueForTest()
		{
			TestCaseHelper.ClearTable("HelpErrorLogOccurrence");
			TestCaseHelper.ClearTable("HelpErrorLogKey");
			TestCaseHelper.ClearTable("HelpErrorLog");
			// create an issue
			ExceptionReportBuilder testExceptionReportBuilder = null;
			try
			{
				ExceptionFunctionOverloaded(6);
			}
			catch (Exception ex)
			{
				ExceptionReportArgs reportArgs = new ExceptionReportArgs(ex, "Test Error Id", "Test Key add line no", "Test Error add line no");
				reportArgs.SessionId = new Guid("0FB94475-D028-4202-85E3-3EB902A2E416");
				reportArgs.Sequence = 1234;
				testExceptionReportBuilder = new ExceptionReportBuilder(reportArgs);
			}

			var report = testExceptionReportBuilder.GenerateReport();
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			IHelpErrorLogCollection errorLogs = new ScalableHelpErrorLogCollection(factory);
			return new ExceptionXml(report).Process(errorLogs, true);
		}

		public void TestOverloadedMethodsLineNo()
		{
			try
			{
				ErrorReporter.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AddReleaseVersionForTest();
				var errorLogPk = AddOverloadedMethodsIssueForTest().PK;
				using (var form = new ZForm())
				using (var occControl = new OccurrencesControlForTest())
				{
					form.Controls.Add(occControl);
					EdiHelpErrorLog log = Factory.Load<EdiHelpErrorLog>(errorLogPk);
					occControl.SetDataBinding(log, "");
					form.Show();
					occControl.moreDetailsButton.PerformClick();
					CombineAssertions(() =>
					{
						//the line# calculation via web browser may trigger random error message.
						AssertNullOrEmpty(ErrorReporter.LastKeyReported);
						AssertNullOrEmpty(ErrorReporter.LastMessageReported);
						AssertNullOrEmpty(ErrorReporter.LastExceptionReported?.ToString());
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					});
					string xml = (string)Db.Connection.ExecuteScalar($"SELECT TOP 1 dbo.ClrUncompressAsString(HO_CompressedXMLData) FROM dbo.HelpErrorLogOccurrence WHERE HO_HE = '{errorLogPk.ToString()}';");
					XmlDocument xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(xml);
					var result = xmlDoc.InnerXml;
					try
					{
						string pattern1 = " Line=\"";
						string pattern2 = "\" ApproxLine=";
						var num1Start = result.IndexOf(pattern1) + pattern1.Length;
						var num1End = result.IndexOf(pattern2, num1Start);
						var num2Start = result.IndexOf(pattern1, num1End) + pattern1.Length;
						var num2End = result.IndexOf(pattern2, num2Start);
						var num1 = Convert.ToInt32(result.Substring(num1Start, num1End - num1Start));
						var num2 = Convert.ToInt32(result.Substring(num2Start, num2End - num2Start));
						Assert(num2 - num1 > 50);
					}
					catch (Exception ex)
					{
						Fail($"String Format Exception. xml=[{result}]. Exception=[{ex.Message}\r\n{ex.StackTrace}]");
					}
				}
			}
			finally
			{
				DirectoryInfo tempPathInfo = new DirectoryInfo(Env.TempPath);
				foreach (FileInfo file in tempPathInfo.GetFiles())
				{
					file.Delete();
				}

				foreach (DirectoryInfo dir in tempPathInfo.GetDirectories())
				{
					dir.Delete(true);
				}
			}
		}

		void ExceptionFunctionOverloaded(ref int level, string name = "")
		{
			string str = null;
			if (level == 0)
			{
				if (str.Length > 0)
				{
					level = 10;
					return;
				}
			}

			ExceptionFunctionOverloaded(level - 1);
		}

		#endregion
		public void TestOpenLicence()
		{
			EdiHelpErrorLog log = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			HelpErrorLogOccurrence occurrenceWithoutLicence = log.Occurrences.AddNew();
			EDIOrgHeader org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			LicenceDatabase database1 = org1.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			database1.LicEnterprise.LE_EnterpriseCode = "ABC";
			HelpErrorLogOccurrence occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_LD = database1.PK;
			EDIOrgHeader org2 = log.Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.CreateAndLoadLicenceForOrg();
			LicenceDatabase database2 = org2.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_HostedLocation = "SYD";
			database2.LicEnterprise.LE_EnterpriseCode = "DEF";
			ClientCompany clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_Code = org2.LicCompany.LC_CompanyCode;
			clientCompany2.LCC_LD = database2.PK;
			clientCompany2.LCC_OH = org2.PK;
			HelpErrorLogOccurrence occurrence2 = log.Occurrences.AddNew();
			occurrence2.HO_LCC = clientCompany2.PK;
			occurrence2.HO_LD = clientCompany2.LCC_LD;
			EDIOrgHeader org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org3.CreateAndLoadLicenceForOrg();
			LicenceDatabase database3 = org2.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_HostedLocation = "CHI";
			database3.LicEnterprise.LE_EnterpriseCode = "GHI";
			ClientCompany clientCompany3 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany3.LCC_Code = org3.LicCompany.LC_CompanyCode;
			clientCompany3.LCC_LD = database3.PK;
			HelpErrorLogOccurrence occurrence3 = log.Occurrences.AddNew();
			occurrence3.HO_LCC = clientCompany3.PK;
			occurrence3.HO_LD = clientCompany3.LCC_LD;
			Factory.Save();
			using (FormForTest form = new FormForTest(log))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(4, form.UserControl.OccurrencesGrid.List.Count);
				AssertLicence(form, 0, false);
				AssertLicence(form, 2, true);
				AssertLicence(form, 1, true);
				AssertLicence(form, 3, true);
			}
		}

		void AssertLicence(FormForTest form, int occurenceIndex, bool shouldOpen)
		{
			form.UserControl.OccurrencesGrid.ListManager.Position = occurenceIndex;
			var selectedOccurrence = (HelpErrorLogOccurrence)form.UserControl.OccurrencesGrid.ListManager.List[occurenceIndex];
			form.UserControl.OpenLicence();
			if (!shouldOpen)
			{
				AssertNull(selectedOccurrence.Database);
				AssertNull(form.UserControl.LastController);
			}
			else
			{
				AssertEquals(typeof(EDIOrganisationForm), form.UserControl.LastController.LastShownForm.GetType());
				var orgform = (EDIOrganisationForm)form.UserControl.LastController.LastShownForm;
				var currentLicenceDatabase = orgform.BuilderTabPage.LicenceControl.SelectedLicenceDatabase;
				AssertEquals(selectedOccurrence.Database.PK, currentLicenceDatabase.PK);
				form.UserControl.LastController.LastShownForm.Dispose();
			}
		}

		static string GetSampleFileContents(string fileName)
		{
			var sampleFilesPrefix = "ZClientEDI.Test.";
			var resourceRetriever = new EmbeddedResourceRetriever(typeof(OccurrencesControlTest).Assembly);
			return resourceRetriever.GetString(sampleFilesPrefix + fileName);
		}

		static void AssertTransform(string source, string expectedResult, Func<string, string> transformAction)
		{
			var result = transformAction(source);

			var normalizedExpected = NormalizeHtml(expectedResult);
			var normalizedResult = NormalizeHtml(result);

			AssertMultilineASCIIEquals(normalizedExpected, normalizedResult);
		}

		[SuppressMessage("RegEx", "SYSLIB1045", Justification = "RegEx static is messy.")]
		static string NormalizeHtml(string html)
		{
			var doc = new HtmlAgilityPack.HtmlDocument
			{
				OptionOutputAsXml = true,
				OptionWriteEmptyNodes = true
			};

			doc.LoadHtml(html);

			// Get the cleaned HTML string
			using var sw = new StringWriter();
			doc.Save(sw);
			var output = sw.ToString();

			// Normalize line endings and trim
			output = output.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

			// Remove extra whitespace between tags (e.g., >   < becomes ><)
			output = Regex.Replace(output, @">\s+<", "><");

			// Remove trailing whitespace before closing tags (e.g., \n</tag>)
			output = Regex.Replace(output, @"\s+(</[^>]+>)", "$1");

			return output;
		}

		#region TestRemoveEmptyTable

		public void TestRemoveEmptyTable()
		{
			var expectedResult = @"
			<html>
				<body>
					
					
					
					
					
					
					<table><tr><td>content</td></tr></table>
				</body>
			</html>
			";
			var htmlContent = @"
			<html>
				<body>
					<table></table>
					<table>
					</table>
					<table> </table>
					<table>\r\n</table>
					<table>\r</table>
					<table>\n</table>
					<table><tr><td>content</td></tr></table>
				</body>
			</html>
			";
			var result = RemoveEmptyTable(htmlContent);
			AssertEquals(expectedResult, result);
		}

		#endregion

#if !WINZOR
		#region TestConvertFactoryStatisticsNestedTableToRtf
		public void TestConvertFactoryStatisticsNestedTableToRtf()
		{
			var htmlContent = GetSampleFileContents("FactoryStatisticsNestedTable.html");
			var htmlDoc = new HtmlAgilityPack.HtmlDocument();
			htmlDoc.LoadHtml(htmlContent);
			var tableNode = htmlDoc.DocumentNode.SelectSingleNode("//body").SelectSingleNode("table");
			var rtfContent = ExceptionReportRtfConvertHelper.ConvertFactoryStatisticsNestedTableToRtf(tableNode);
			var expectedRtfContent = GetSampleFileContents("FactoryStatisticsNestedTable_rtf.txt");
			AssertEquals(expectedRtfContent, rtfContent);
		}
		#endregion

		#region TestConvertHtmlToRtf

		public void TestConvertHtmlToRtf_EscapeContentForSimpleHtmlTags()
		{
			var content = @"C:\MyContent";
			var simpleHtmlTags = new[] { "p", "b" };
			var htmlContents = GenerateSimpleHtmlContent(simpleHtmlTags, content);
			htmlContents.ForEach(c =>
			{
				var rtf = ConvertHtmlToRtf(c);
				AssertContains($"haven't escape the content for {c}", "C:\\\\MyContent", rtf);
			});
		}

		public void TestConvertHtmlToRtf_EscapeContentForSpecialSymbols()
		{
			var htmlContent = @"<html>
<body>
    <table>
        <tbody>
            <tr>
                <td>0&nbsp;1</td>
                <td>&quot;4&quot;</td>
                <td>1<br>new line</td>
                <td>The query has too many nodes. URL: https://wi00827608.testrig.sand.wtg.zone/Glow/odata/Global/GlbBranches?$filter=(GB_PK%20eq%20)%20and%20(GB_GC%20eq%202845e4a6-5758-439f-b191-f5abb34c9968)</td>
                <td><pre>EnsureRequiredReviewsExist failed with status &#39;ServiceUnavailable&#39; error: &lt;!DOCTYPE html&gt;
&lt;html lang=&quot;en&quot;&gt;
&lt;head&gt;
	&lt;style id=&#39;style-wtg&#39;&gt;.default-error {font-family:Arial; margin: 30px;}&lt;/style&gt;
	&lt;title&gt;Website Offline&lt;/title&gt;
	&lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1&quot;&gt;
	&lt;link rel=&quot;preconnect&quot; href=&quot;https://fonts.googleapis.com&quot;&gt;
	&lt;link rel=&quot;preconnect&quot; href=&quot;https://fonts.gstatic.com&quot; crossorigin&gt;
	&lt;link href=&quot;https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;700&amp;display=swap&quot; rel=&quot;stylesheet&quot;&gt;
&lt;/head&gt;
&lt;body&gt;
&lt;div id=content&gt;
	&lt;div class=&quot;default-error&quot;&gt;
	&lt;h1&gt;This website is currently offline&lt;/h1&gt;
	&lt;p&gt;We are currently performing maintenance on this website.  Please check back soon.&lt;/p&gt;
	&lt;/div&gt;
&lt;/div&gt;
&lt;script&gt;
cur_url = new URL(window.location.href)
if (cur_url.hostname.match(/myaccount(?:-portal)?\.cargowise\.com$/)){
	document.getElementById(&quot;style-wtg&quot;).innerHTML = `.container{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;gap: 24px;width: 600px;flex: none;order: 0;flex-grow: 0;}h1{font-family: &#39;DM Sans&#39;, sans-serif;font-style: normal;font-weight: 700;font-size: 36px;line-height: 100%;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;}h2{font-family: &#39;DM Sans&#39;;font-style: normal;font-weight: 500;font-size: 20px;line-height: 28px;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;}p{font-family: &#39;DM Sans&#39;;font-style: normal;font-weight: 400;font-size: 14px;line-height: 20px;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;}body{height: 100vh;background-color: #371EE1;position: relative;display: flex;align-items: center;justify-content: center;margin: 0;padding: 0;}.error-code-frame{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;flex: none;order: 0;flex-grow: 0;}.error-message-frame{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;gap: 8px;flex: none;order: 1;flex-grow: 0;}.logos-frame{display: flex;flex-direction: row;align-items: flex-start;padding: 0px;gap: 48px;flex: none;order: 1;flex-grow: 0;}`;
	document.getElementById(&quot;content&quot;).innerHTML = `&lt;div class=&quot;container&quot;&gt;&lt;div class=&quot;error-message-frame&quot;&gt;&lt;h1 id=&quot;error-heading&quot;&gt;This website is currently undergoing maintenance&lt;/h1&gt;&lt;h2&gt;Please check back soon.&lt;/h2&gt;&lt;p&gt;My Account and eRequests have a daily maintenance window from 7:00am - 7:10am Australian Eastern Time. Your patience is appreciated as we deploy enhancements to our product.&lt;/p&gt;&lt;p&gt;(Code 503)&lt;/p&gt;&lt;/div&gt;&lt;div class=&quot;logos-frame&quot;&gt;&lt;svg width=&quot;134&quot; height=&quot;32&quot; viewBox=&quot;0 0 134 32&quot; fill=&quot;none&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot;&gt;&lt;path fill=&quot;#FDFDFD&quot; d=&quot;M6.726 32v-2.908h8.727V32H6.726zm11.602-5.817l.01-2.911H32.87v2.91H18.328zM.902 20.363v-2.909h14.543v2.91H.902zm70.346-3.785l1.972-.937c.498 1.096 1.453 1.793 2.968 1.793 1.812 0 2.908-1.345 2.908-3.048V13.13a4.252 4.252 0 0 1-3.346 1.615 5.01 5.01 0 0 1-4.962-5.181c0-3.188 2.372-5.181 4.98-5.181a4.242 4.242 0 0 1 3.309 1.555V4.59h2.132v9.715c0 2.925-1.834 5.079-5.021 5.079-2.49.003-4.183-1.191-4.94-2.806zM72.9 9.545a3.167 3.167 0 0 0 3.147 3.247 3.15 3.15 0 0 0 3.148-3.247 3.146 3.146 0 0 0-4.372-2.98 3.148 3.148 0 0 0-1.923 2.98zm11.198 3.696a5.128 5.128 0 0 1-1.491-3.678 5.172 5.172 0 1 1 5.16 5.19h-.033a5.126 5.126 0 0 1-3.635-1.509v-.003zm.613-3.678a3.111 3.111 0 0 0 3.056 3.187 3.12 3.12 0 0 0 3.068-3.187 3.124 3.124 0 0 0-3.075-3.188 3.1 3.1 0 0 0-3.048 3.19v-.002zm33.388 5.181c-2.146 0-3.583-.778-4.438-2.25l1.922-1.035a2.889 2.889 0 0 0 2.63 1.434c1.234 0 1.872-.598 1.872-1.296 0-.817-1.195-.996-2.47-1.275-1.703-.34-3.498-.903-3.498-2.95 0-1.575 1.515-3.029 3.885-3.01 1.922 0 3.227.717 4.063 1.932l-1.797.977a2.559 2.559 0 0 0-2.271-1.136c-1.174 0-1.73.539-1.73 1.196 0 .717.857.876 2.372 1.216 1.672.358 3.565.916 3.565 3.007 0 1.488-1.331 3.19-4.058 3.19h-.047zm-75.295-1.498a5.136 5.136 0 0 1-1.51-3.683 5.124 5.124 0 0 1 5.19-5.182 5.013 5.013 0 0 1 4.722 3.05l-1.912.817a2.972 2.972 0 0 0-2.81-1.873 3.11 3.11 0 0 0-3.103 3.187 3.137 3.137 0 0 0 3.145 3.19 2.997 2.997 0 0 0 2.81-1.95l1.934.815a5.005 5.005 0 0 1-4.779 3.13h-.062a5.134 5.134 0 0 1-3.625-1.5v-.001zm14.119 1.498a5.01 5.01 0 0 1-4.961-5.181c0-3.188 2.371-5.182 4.98-5.182a4.287 4.287 0 0 1 3.308 1.496V4.58h2.132v9.963h-2.132v-1.375a4.228 4.228 0 0 1-3.29 1.575h-.037zm-2.849-5.2a3.165 3.165 0 0 0 3.147 3.247 3.149 3.149 0 0 0 3.148-3.247A3.146 3.146 0 0 0 56 6.562a3.148 3.148 0 0 0-1.925 2.983v-.001zm68.947.067a5.12 5.12 0 0 1 3.179-4.838 5.127 5.127 0 0 1 2-.384c2.925 0 5.119 2.124 5.119 5.074v.838h-8.246c.279 1.54 1.474 2.549 3.208 2.549a3.12 3.12 0 0 0 2.909-1.793l1.753.976a5.033 5.033 0 0 1-4.662 2.71c-3.15.003-5.261-2.247-5.261-5.13l.001-.002zm2.151-1.124v.007h5.976c-.32-1.434-1.415-2.23-2.949-2.23h-.059a3.092 3.092 0 0 0-2.969 2.224l.001-.001zm-14.672 6.057V4.582h2.151v9.962h-2.151zm-6.555 0l-2.649-7.113-2.69 7.113h-2.09l-3.308-9.98 2.172.018 2.272 7.073 2.61-7.073h2.115l2.569 7.073 2.351-7.073h2.172l-3.46 9.963h-2.064zm-39.702 0V4.582h2.132V6.15c.538-1.017 1.512-1.574 2.908-1.574h.962v2.038H68.95c-1.813 0-2.551.917-2.551 2.989v4.94l-2.154.001zm-45.882 0v-2.908h14.54v2.908h-14.54zm-17.46 0v-2.908h8.729l-.002 2.908H.902zm17.46-5.817v-2.91h14.54v2.91h-14.54zm91.781-7.113A1.387 1.387 0 0 1 111.578.18a1.377 1.377 0 0 1 1.433 1.434 1.434 1.434 0 0 1-2.868.001v-.001zM18.362 2.908V0h14.54v2.909l-14.54-.001zm-11.636 0V0h8.727v2.909l-8.727-.001z&quot;/&gt;&lt;/svg&gt;&lt;svg width=&quot;103&quot; height=&quot;32&quot; xmlns=&quot;http://www.w3.org/2000/svg&quot; fill=&quot;none&quot; viewBox=&quot;0 0 103 32&quot;&gt;&lt;g fill=&quot;#fff&quot; clip-path=&quot;url(#clip0_1_24)&quot;&gt;&lt;path d=&quot;M34.769 3.962l1.877.018 1.962 6.138 2.256-6.138h1.824l2.22 6.138 2.033-6.138h1.876l-2.996 8.644h-1.773L41.76 6.453l-2.325 6.17h-1.808L34.77 3.963zM50.643.16c.723 0 1.239.518 1.239 1.245 0 .726-.517 1.243-1.239 1.243-.721 0-1.24-.519-1.24-1.243 0-.725.517-1.246 1.24-1.246zm-.93 3.82h1.86v8.644h-1.86V3.98zM52.445 10.844l1.652-.9c.499.813 1.24 1.246 2.273 1.246 1.033 0 1.619-.52 1.619-1.123 0-.71-1.033-.864-2.134-1.107-1.464-.293-3.013-.777-3.013-2.558 0-1.366 1.308-2.629 3.357-2.612 1.67 0 2.79.623 3.512 1.677l-1.55.846c-.412-.622-1.05-.985-1.963-.985-1.016 0-1.497.466-1.497 1.036 0 .623.74.76 2.05 1.056 1.445.31 3.081.796 3.081 2.61 0 1.296-1.154 2.782-3.547 2.765-1.858 0-3.098-.673-3.838-1.953M60.532 8.336c0-2.542 1.947-4.53 4.476-4.53 2.53 0 4.425 1.85 4.425 4.408v.726h-7.127c.242 1.33 1.274 2.213 2.771 2.213 1.171 0 2.067-.623 2.514-1.556l1.515.847c-.757 1.418-2.118 2.351-4.029 2.351-2.72 0-4.545-1.953-4.545-4.46m1.859-.97h5.164c-.276-1.243-1.222-1.935-2.549-1.935-1.326 0-2.272.795-2.617 1.936M69.889 3.98h1.463V1.94L73.21.92v3.06h1.824v1.66h-1.824v3.44c0 1.555.241 1.798 1.824 1.798v1.746h-.275c-2.617 0-3.408-.83-3.408-3.528V5.638h-1.463v-1.66.002zM75.554 8.336c0-2.542 1.945-4.53 4.476-4.53 2.531 0 4.425 1.85 4.425 4.408v.726h-7.127c.242 1.33 1.274 2.213 2.771 2.213 1.171 0 2.066-.623 2.514-1.556l1.514.847c-.757 1.418-2.117 2.351-4.028 2.351-2.72 0-4.545-1.953-4.545-4.46m1.859-.97h5.165c-.276-1.243-1.222-1.935-2.548-1.935s-2.273.795-2.617 1.936zM85.263 8.302c0-2.542 1.98-4.495 4.493-4.495 1.91 0 3.426 1.09 4.08 2.646l-1.652.709c-.431-1.003-1.308-1.626-2.428-1.626-1.533 0-2.685 1.227-2.685 2.766s1.17 2.766 2.72 2.766c1.171 0 2.031-.726 2.428-1.694l1.67.71c-.69 1.59-2.118 2.714-4.132 2.714-2.548 0-4.493-2.005-4.493-4.494M94.93.003h1.825v5.065c.585-.76 1.532-1.262 2.651-1.262 2.17 0 3.494 1.417 3.494 3.768v5.048h-1.875V7.747c0-1.435-.688-2.333-2.084-2.333-1.221 0-2.17.933-2.17 2.455v4.755h-1.84V.004zM0 22.738h2.516V15.16H0v7.58zm10.063 5.056h2.515v-7.579h-2.515v7.58zm15.095-5.056h2.516V15.16H25.16v7.58zM0 12.633h2.516V0H0v12.633zm5.031 0h2.516V0H5.031v12.633zm5.032 0h2.515V0h-2.515v12.633zM15.094 27.8h2.516V15.166h-2.516v12.633zm5.032-15.146h2.515V.02h-2.515v12.633zM35.226 23.468c0-2.77 2.05-4.5 4.308-4.5 1.137 0 2.207.52 2.86 1.35v-1.177h1.844v8.446c0 2.543-1.585 4.413-4.342 4.413-2.154 0-3.62-1.038-4.275-2.44l1.707-.813c.43.952 1.258 1.558 2.568 1.558 1.568 0 2.515-1.159 2.515-2.648v-1.091c-.655.865-1.757 1.401-2.895 1.401-2.188 0-4.292-1.747-4.292-4.5m7.272-.016c0-1.54-1.154-2.803-2.723-2.803-1.57 0-2.724 1.228-2.724 2.803 0 1.574 1.19 2.821 2.724 2.821 1.534 0 2.723-1.264 2.723-2.821zM47.706 15.161h-1.843v12.633h1.843V15.161zM48.921 23.468c0-2.527 1.982-4.5 4.464-4.5a4.46 4.46 0 0 1 4.48 4.5 4.46 4.46 0 0 1-4.48 4.5c-2.482 0-4.464-1.991-4.464-4.5zm7.118 0c0-1.524-1.172-2.77-2.654-2.77s-2.637 1.246-2.637 2.77c0 1.523 1.154 2.769 2.637 2.769s2.654-1.246 2.654-2.77zM60.886 26.618v1.176h-1.81V15.161h1.827v5.14c.637-.814 1.706-1.333 2.86-1.333 2.258 0 4.292 1.73 4.292 4.5 0 2.769-2.085 4.5-4.275 4.5-1.155 0-2.257-.503-2.895-1.351m5.342-3.168c0-1.574-1.19-2.803-2.724-2.803-1.534 0-2.705 1.264-2.705 2.803 0 1.54 1.156 2.822 2.705 2.822 1.55 0 2.724-1.246 2.724-2.822zM68.849 23.468c0-2.77 2.05-4.5 4.308-4.5 1.137 0 2.223.502 2.86 1.298v-1.125h1.843v8.653h-1.843V26.6c-.655.848-1.74 1.367-2.877 1.367-2.188 0-4.291-1.747-4.291-4.5zm7.273-.017c0-1.54-1.154-2.803-2.724-2.803-1.57 0-2.723 1.228-2.723 2.803 0 1.574 1.19 2.821 2.723 2.821 1.534 0 2.724-1.264 2.724-2.821zM81.32 15.161h-1.842v12.633h1.843V15.161z&quot;/&gt;&lt;path d=&quot;M0 22.738h2.516V15.16H0v7.58zm10.063 5.056h2.515v-7.579h-2.515v7.58zm15.095-5.056h2.516V15.16H25.16v7.58zM0 12.633h2.516V0H0v12.633zm5.031 0h2.516V0H5.031v12.633zm5.032 0h2.515V0h-2.515v12.633zM15.094 27.8h2.516V15.166h-2.516v12.633zm5.032-15.146h2.515V.02h-2.515v12.633z&quot;/&gt;&lt;/g&gt;&lt;defs&gt;&lt;clipPath id=&quot;clip0_1_24&quot;&gt;&lt;path fill=&quot;#fff&quot; d=&quot;M0 0h102.902v32H0z&quot;/&gt;&lt;/clipPath&gt;&lt;/defs&gt;&lt;/svg&gt;&lt;/div&gt;`;
}
&lt;/script&gt;
&lt;/body&gt;
&lt;/html&gt;
.
Requested URL was: https://myaccount-portal.cargowise.com/myaccount/api/workitem/EnsureRequiredReviewsExist
Request payload was: {&quot;ShelfTaskPK&quot;:&quot;23432d0f-ed05-468f-91c2-4420d1c3a4d3&quot;,&quot;ReviewTaskCapability&quot;:&quot;I&amp;R&quot;,&quot;ReviewName&quot;:&quot;IdentityAndSecurity (Dev)&quot;,&quot;TaskNotes&quot;:&quot;Please review \&quot;IdentityAndSecurity (Dev)\&quot; Aspect Data: https://crikey.wtg.zone/AspectReview/4e128f1a-6c27-4bfe-bf0a-e1b68bf4ebc4&quot;}</pre></td>
            </tr>
        </tbody>
    </table>
</body>
</html>";
			// for the hyperlink, the URL in the browser after you click it should be same with the display URL in the RTB
			var expectedRtfContent = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
{\trowd\clftsWidth1\cellx5000\clftsWidth1\cellx10000\clftsWidth1\cellx15000\clftsWidth1\cellx20000\clftsWidth1\cellx25000{{0\~1}\cell}{{""4""}\cell}{{1\line new line}\cell}{{The query has too many nodes. URL: }{{\field{\*\fldinst HYPERLINK ""https://wi00827608.testrig.sand.wtg.zone/Glow/odata/Global/GlbBranches?$filter=(GB_PK%20eq%20)%20and%20(GB_GC%20eq%202845e4a6-5758-439f-b191-f5abb34c9968)"" }{\fldrslt https://wi00827608.testrig.sand.wtg.zone/Glow/odata/Global/GlbBranches?$filter=(GB_PK%20eq%20)%20and%20(GB_GC%20eq%202845e4a6-5758-439f-b191-f5abb34c9968)}}}\cell}{{EnsureRequiredReviewsExist failed with status 'ServiceUnavailable' error: <!DOCTYPE html>\line <html lang=""en"">\line <head>\line\tab <style id='style-wtg'>.default-error \{font-family:Arial; margin: 30px;\}</style>\line\tab <title>Website Offline</title>\line\tab <meta name=""viewport"" content=""width=device-width, initial-scale=1"">\line\tab <link rel=""preconnect"" href=""}{{\field{\*\fldinst HYPERLINK ""https://fonts.googleapis.com"" }{\fldrslt https://fonts.googleapis.com}}}{"">\line\tab <link rel=""preconnect"" href=""}{{\field{\*\fldinst HYPERLINK ""https://fonts.gstatic.com"" }{\fldrslt https://fonts.gstatic.com}}}{"" crossorigin>\line\tab <link href=""}{{\field{\*\fldinst HYPERLINK ""https://fonts.googleapis.com/css2?family=DM+Sans:wght@400"" }{\fldrslt https://fonts.googleapis.com/css2?family=DM+Sans:wght@400}}}{;500;700&display=swap"" rel=""stylesheet"">\line </head>\line <body>\line <div id=content>\line\tab <div class=""default-error"">\line\tab <h1>This website is currently offline</h1>\line\tab <p>We are currently performing maintenance on this website.  Please check back soon.</p>\line\tab </div>\line </div>\line <script>\line cur_url = new URL(window.location.href)\line if (cur_url.hostname.match(/myaccount(?:-portal)?\\.cargowise\\.com$/))\{\line\tab document.getElementById(""style-wtg"").innerHTML = `.container\{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;gap: 24px;width: 600px;flex: none;order: 0;flex-grow: 0;\}h1\{font-family: 'DM Sans', sans-serif;font-style: normal;font-weight: 700;font-size: 36px;line-height: 100%;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;\}h2\{font-family: 'DM Sans';font-style: normal;font-weight: 500;font-size: 20px;line-height: 28px;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;\}p\{font-family: 'DM Sans';font-style: normal;font-weight: 400;font-size: 14px;line-height: 20px;color: #FFFFFF;margin-block-start: 0px;margin-block-end: 0px;margin-inline-start: 0px;margin-inline-end: 0px;\}body\{height: 100vh;background-color: #371EE1;position: relative;display: flex;align-items: center;justify-content: center;margin: 0;padding: 0;\}.error-code-frame\{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;flex: none;order: 0;flex-grow: 0;\}.error-message-frame\{display: flex;flex-direction: column;align-items: flex-start;padding: 0px;gap: 8px;flex: none;order: 1;flex-grow: 0;\}.logos-frame\{display: flex;flex-direction: row;align-items: flex-start;padding: 0px;gap: 48px;flex: none;order: 1;flex-grow: 0;\}`;\line\tab document.getElementById(""content"").innerHTML = `<div class=""container""><div class=""error-message-frame""><h1 id=""error-heading"">This website is currently undergoing maintenance</h1><h2>Please check back soon.</h2><p>My Account and eRequests have a daily maintenance window from 7:00am - 7:10am Australian Eastern Time. Your patience is appreciated as we deploy enhancements to our product.</p><p>(Code 503)</p></div><div class=""logos-frame""><svg width=""134"" height=""32"" viewBox=""0 0 134 32"" fill=""none"" xmlns=""}{{\field{\*\fldinst HYPERLINK ""http://www.w3.org/2000/svg"" }{\fldrslt http://www.w3.org/2000/svg}}}{""><path fill=""#FDFDFD"" d=""M6.726 32v-2.908h8.727V32H6.726zm11.602-5.817l.01-2.911H32.87v2.91H18.328zM.902 20.363v-2.909h14.543v2.91H.902zm70.346-3.785l1.972-.937c.498 1.096 1.453 1.793 2.968 1.793 1.812 0 2.908-1.345 2.908-3.048V13.13a4.252 4.252 0 0 1-3.346 1.615 5.01 5.01 0 0 1-4.962-5.181c0-3.188 2.372-5.181 4.98-5.181a4.242 4.242 0 0 1 3.309 1.555V4.59h2.132v9.715c0 2.925-1.834 5.079-5.021 5.079-2.49.003-4.183-1.191-4.94-2.806zM72.9 9.545a3.167 3.167 0 0 0 3.147 3.247 3.15 3.15 0 0 0 3.148-3.247 3.146 3.146 0 0 0-4.372-2.98 3.148 3.148 0 0 0-1.923 2.98zm11.198 3.696a5.128 5.128 0 0 1-1.491-3.678 5.172 5.172 0 1 1 5.16 5.19h-.033a5.126 5.126 0 0 1-3.635-1.509v-.003zm.613-3.678a3.111 3.111 0 0 0 3.056 3.187 3.12 3.12 0 0 0 3.068-3.187 3.124 3.124 0 0 0-3.075-3.188 3.1 3.1 0 0 0-3.048 3.19v-.002zm33.388 5.181c-2.146 0-3.583-.778-4.438-2.25l1.922-1.035a2.889 2.889 0 0 0 2.63 1.434c1.234 0 1.872-.598 1.872-1.296 0-.817-1.195-.996-2.47-1.275-1.703-.34-3.498-.903-3.498-2.95 0-1.575 1.515-3.029 3.885-3.01 1.922 0 3.227.717 4.063 1.932l-1.797.977a2.559 2.559 0 0 0-2.271-1.136c-1.174 0-1.73.539-1.73 1.196 0 .717.857.876 2.372 1.216 1.672.358 3.565.916 3.565 3.007 0 1.488-1.331 3.19-4.058 3.19h-.047zm-75.295-1.498a5.136 5.136 0 0 1-1.51-3.683 5.124 5.124 0 0 1 5.19-5.182 5.013 5.013 0 0 1 4.722 3.05l-1.912.817a2.972 2.972 0 0 0-2.81-1.873 3.11 3.11 0 0 0-3.103 3.187 3.137 3.137 0 0 0 3.145 3.19 2.997 2.997 0 0 0 2.81-1.95l1.934.815a5.005 5.005 0 0 1-4.779 3.13h-.062a5.134 5.134 0 0 1-3.625-1.5v-.001zm14.119 1.498a5.01 5.01 0 0 1-4.961-5.181c0-3.188 2.371-5.182 4.98-5.182a4.287 4.287 0 0 1 3.308 1.496V4.58h2.132v9.963h-2.132v-1.375a4.228 4.228 0 0 1-3.29 1.575h-.037zm-2.849-5.2a3.165 3.165 0 0 0 3.147 3.247 3.149 3.149 0 0 0 3.148-3.247A3.146 3.146 0 0 0 56 6.562a3.148 3.148 0 0 0-1.925 2.983v-.001zm68.947.067a5.12 5.12 0 0 1 3.179-4.838 5.127 5.127 0 0 1 2-.384c2.925 0 5.119 2.124 5.119 5.074v.838h-8.246c.279 1.54 1.474 2.549 3.208 2.549a3.12 3.12 0 0 0 2.909-1.793l1.753.976a5.033 5.033 0 0 1-4.662 2.71c-3.15.003-5.261-2.247-5.261-5.13l.001-.002zm2.151-1.124v.007h5.976c-.32-1.434-1.415-2.23-2.949-2.23h-.059a3.092 3.092 0 0 0-2.969 2.224l.001-.001zm-14.672 6.057V4.582h2.151v9.962h-2.151zm-6.555 0l-2.649-7.113-2.69 7.113h-2.09l-3.308-9.98 2.172.018 2.272 7.073 2.61-7.073h2.115l2.569 7.073 2.351-7.073h2.172l-3.46 9.963h-2.064zm-39.702 0V4.582h2.132V6.15c.538-1.017 1.512-1.574 2.908-1.574h.962v2.038H68.95c-1.813 0-2.551.917-2.551 2.989v4.94l-2.154.001zm-45.882 0v-2.908h14.54v2.908h-14.54zm-17.46 0v-2.908h8.729l-.002 2.908H.902zm17.46-5.817v-2.91h14.54v2.91h-14.54zm91.781-7.113A1.387 1.387 0 0 1 111.578.18a1.377 1.377 0 0 1 1.433 1.434 1.434 1.434 0 0 1-2.868.001v-.001zM18.362 2.908V0h14.54v2.909l-14.54-.001zm-11.636 0V0h8.727v2.909l-8.727-.001z""/></svg><svg width=""103"" height=""32"" xmlns=""}{{\field{\*\fldinst HYPERLINK ""http://www.w3.org/2000/svg"" }{\fldrslt http://www.w3.org/2000/svg}}}{"" fill=""none"" viewBox=""0 0 103 32""><g fill=""#fff"" clip-path=""url(#clip0_1_24)""><path d=""M34.769 3.962l1.877.018 1.962 6.138 2.256-6.138h1.824l2.22 6.138 2.033-6.138h1.876l-2.996 8.644h-1.773L41.76 6.453l-2.325 6.17h-1.808L34.77 3.963zM50.643.16c.723 0 1.239.518 1.239 1.245 0 .726-.517 1.243-1.239 1.243-.721 0-1.24-.519-1.24-1.243 0-.725.517-1.246 1.24-1.246zm-.93 3.82h1.86v8.644h-1.86V3.98zM52.445 10.844l1.652-.9c.499.813 1.24 1.246 2.273 1.246 1.033 0 1.619-.52 1.619-1.123 0-.71-1.033-.864-2.134-1.107-1.464-.293-3.013-.777-3.013-2.558 0-1.366 1.308-2.629 3.357-2.612 1.67 0 2.79.623 3.512 1.677l-1.55.846c-.412-.622-1.05-.985-1.963-.985-1.016 0-1.497.466-1.497 1.036 0 .623.74.76 2.05 1.056 1.445.31 3.081.796 3.081 2.61 0 1.296-1.154 2.782-3.547 2.765-1.858 0-3.098-.673-3.838-1.953M60.532 8.336c0-2.542 1.947-4.53 4.476-4.53 2.53 0 4.425 1.85 4.425 4.408v.726h-7.127c.242 1.33 1.274 2.213 2.771 2.213 1.171 0 2.067-.623 2.514-1.556l1.515.847c-.757 1.418-2.118 2.351-4.029 2.351-2.72 0-4.545-1.953-4.545-4.46m1.859-.97h5.164c-.276-1.243-1.222-1.935-2.549-1.935-1.326 0-2.272.795-2.617 1.936M69.889 3.98h1.463V1.94L73.21.92v3.06h1.824v1.66h-1.824v3.44c0 1.555.241 1.798 1.824 1.798v1.746h-.275c-2.617 0-3.408-.83-3.408-3.528V5.638h-1.463v-1.66.002zM75.554 8.336c0-2.542 1.945-4.53 4.476-4.53 2.531 0 4.425 1.85 4.425 4.408v.726h-7.127c.242 1.33 1.274 2.213 2.771 2.213 1.171 0 2.066-.623 2.514-1.556l1.514.847c-.757 1.418-2.117 2.351-4.028 2.351-2.72 0-4.545-1.953-4.545-4.46m1.859-.97h5.165c-.276-1.243-1.222-1.935-2.548-1.935s-2.273.795-2.617 1.936zM85.263 8.302c0-2.542 1.98-4.495 4.493-4.495 1.91 0 3.426 1.09 4.08 2.646l-1.652.709c-.431-1.003-1.308-1.626-2.428-1.626-1.533 0-2.685 1.227-2.685 2.766s1.17 2.766 2.72 2.766c1.171 0 2.031-.726 2.428-1.694l1.67.71c-.69 1.59-2.118 2.714-4.132 2.714-2.548 0-4.493-2.005-4.493-4.494M94.93.003h1.825v5.065c.585-.76 1.532-1.262 2.651-1.262 2.17 0 3.494 1.417 3.494 3.768v5.048h-1.875V7.747c0-1.435-.688-2.333-2.084-2.333-1.221 0-2.17.933-2.17 2.455v4.755h-1.84V.004zM0 22.738h2.516V15.16H0v7.58zm10.063 5.056h2.515v-7.579h-2.515v7.58zm15.095-5.056h2.516V15.16H25.16v7.58zM0 12.633h2.516V0H0v12.633zm5.031 0h2.516V0H5.031v12.633zm5.032 0h2.515V0h-2.515v12.633zM15.094 27.8h2.516V15.166h-2.516v12.633zm5.032-15.146h2.515V.02h-2.515v12.633zM35.226 23.468c0-2.77 2.05-4.5 4.308-4.5 1.137 0 2.207.52 2.86 1.35v-1.177h1.844v8.446c0 2.543-1.585 4.413-4.342 4.413-2.154 0-3.62-1.038-4.275-2.44l1.707-.813c.43.952 1.258 1.558 2.568 1.558 1.568 0 2.515-1.159 2.515-2.648v-1.091c-.655.865-1.757 1.401-2.895 1.401-2.188 0-4.292-1.747-4.292-4.5m7.272-.016c0-1.54-1.154-2.803-2.723-2.803-1.57 0-2.724 1.228-2.724 2.803 0 1.574 1.19 2.821 2.724 2.821 1.534 0 2.723-1.264 2.723-2.821zM47.706 15.161h-1.843v12.633h1.843V15.161zM48.921 23.468c0-2.527 1.982-4.5 4.464-4.5a4.46 4.46 0 0 1 4.48 4.5 4.46 4.46 0 0 1-4.48 4.5c-2.482 0-4.464-1.991-4.464-4.5zm7.118 0c0-1.524-1.172-2.77-2.654-2.77s-2.637 1.246-2.637 2.77c0 1.523 1.154 2.769 2.637 2.769s2.654-1.246 2.654-2.77zM60.886 26.618v1.176h-1.81V15.161h1.827v5.14c.637-.814 1.706-1.333 2.86-1.333 2.258 0 4.292 1.73 4.292 4.5 0 2.769-2.085 4.5-4.275 4.5-1.155 0-2.257-.503-2.895-1.351m5.342-3.168c0-1.574-1.19-2.803-2.724-2.803-1.534 0-2.705 1.264-2.705 2.803 0 1.54 1.156 2.822 2.705 2.822 1.55 0 2.724-1.246 2.724-2.822zM68.849 23.468c0-2.77 2.05-4.5 4.308-4.5 1.137 0 2.223.502 2.86 1.298v-1.125h1.843v8.653h-1.843V26.6c-.655.848-1.74 1.367-2.877 1.367-2.188 0-4.291-1.747-4.291-4.5zm7.273-.017c0-1.54-1.154-2.803-2.724-2.803-1.57 0-2.723 1.228-2.723 2.803 0 1.574 1.19 2.821 2.723 2.821 1.534 0 2.724-1.264 2.724-2.821zM81.32 15.161h-1.842v12.633h1.843V15.161z""/><path d=""M0 22.738h2.516V15.16H0v7.58zm10.063 5.056h2.515v-7.579h-2.515v7.58zm15.095-5.056h2.516V15.16H25.16v7.58zM0 12.633h2.516V0H0v12.633zm5.031 0h2.516V0H5.031v12.633zm5.032 0h2.515V0h-2.515v12.633zM15.094 27.8h2.516V15.166h-2.516v12.633zm5.032-15.146h2.515V.02h-2.515v12.633z""/></g><defs><clipPath id=""clip0_1_24""><path fill=""#fff"" d=""M0 0h102.902v32H0z""/></clipPath></defs></svg></div>`;\line\}\line </script>\line </body>\line </html>\line .\line Requested URL was: }{{\field{\*\fldinst HYPERLINK ""https://myaccount-portal.cargowise.com/myaccount/api/workitem/EnsureRequiredReviewsExist"" }{\fldrslt https://myaccount-portal.cargowise.com/myaccount/api/workitem/EnsureRequiredReviewsExist}}}{\line Request payload was: \{""ShelfTaskPK"":""23432d0f-ed05-468f-91c2-4420d1c3a4d3"",""ReviewTaskCapability"":""I&R"",""ReviewName"":""IdentityAndSecurity (Dev)"",""TaskNotes"":""Please review \\""IdentityAndSecurity (Dev)\\"" Aspect Data: }{{\field{\*\fldinst HYPERLINK ""https://crikey.wtg.zone/AspectReview/4e128f1a-6c27-4bfe-bf0a-e1b68bf4ebc4"" }{\fldrslt https://crikey.wtg.zone/AspectReview/4e128f1a-6c27-4bfe-bf0a-e1b68bf4ebc4}}}{""\}}\cell}\row}}";

			var rtfContent = ConvertHtmlToRtf(htmlContent);
			AssertMultilineASCIIEquals(expectedRtfContent, rtfContent);
		}

		public void TestConvertHtmlToRtf_EscapeContentForSimpleTable()
		{
			var htmlContents = new[]
			{
				@"<html>
<body>
    <table>
        <tbody>
            <tr>
                <td>C:\MyContent</td>
                <td>C:\MyContent</td>
            </tr>
        </tbody>
    </table>
</body>
</html>", @"<html>

<body>
    <table>
        <tbody>
            <tr>
                <td><b>Name</b></td>
                <td><b>Creation Stack</b></td>
                <td><b>Factory Statistics Count</b></td>
                <td><b>Details</b></td>
            </tr>
            <tr>
                <td>UserContext</td>
                <td>C:\MyContent</td>
                <td>1</td>
                <td>
                    <table>
                        <tbody>
                            <tr>
                                <td><b>Creation Thread ID</b></td>
                                <td><b>Active Fetch Hints</b></td>
                                <td><b>Business Objects</b></td>
                                <td><b>Child Factories</b></td>
                                <td><b>Database Loads</b></td>
                                <td><b>Data Rows</b></td>
                                <td><b>Factory Instance</b></td>
                                <td><b>Creation Time</b></td>
                                <td><b>Tracked Objects</b></td>
                            </tr>
                            <tr>
                                <td>1</td>
                                <td>1</td>
                                <td>11</td>
                                <td>0</td>
                                <td>4</td>
                                <td>11</td>
                                <td>4</td>
                                <td>31/12/2016 4:03:04 AM</td>
                                <td>C:\MyContent<br></td>
                            </tr>
                        </tbody>

                    </table>
                </td>
            </tr>
        </tbody>
    </table>
</body>

</html>"
			};
			htmlContents.ForEach(html =>
			{
				var rtf = OccurrencesControl.ConvertHtmlToRtf(html);
				var split = rtf.Split(new[] { "C:\\\\MyContent" }, StringSplitOptions.None);
				AssertEquals("haven't escape the content", 3, split.Length);
			});
		}

		string[] GenerateSimpleHtmlContent(string[] simpleHtmlTags, string content)
		{
			return simpleHtmlTags.Select(tag =>
			{
				var html = $@"<html>
<body>
<{tag}>{content}</{tag}>
</body>
</html>";
				return html;
			}).ToArray();
		}

		public void TestConvertHtmlToRtf_SimpleH1AndTableContent()
		{
			var html = $@"<html>
<head>
</head>
<body>
	<h1>Summary</h1>
	<br>
	<table>
		<tr>
			<td>ExceptionDescription</td>
			<td>
				<pre>  --- Because this issue has been fixed this log has been cleared to save space. ---</pre>
			</td>
		</tr>
		<tr>
			<td>ExceptionDetails</td>
			<td></td>
		</tr>
	</table>
	<h1>Exception Details</h1>
	<table>
		<tr>
			<td>StackTrace</td>
			<td>&lt;null&gt;</td>
		</tr>
	</table>
</body>
</html>
";
			var expectedRtfContent = $@"{{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{{\fonttbl{{\f0\fnil\fcharset0 Microsoft Sans Serif;}}}}
{{\*\generator Riched20 10.0.19041}}\viewkind4\uc1
\pard\f0\b\fs48 Summary \b0\par\fs20\line
{{\trowd\clftsWidth1\cellx5000\clftsWidth1\cellx25000{{{{ExceptionDescription}}\cell}}{{{{\line\tab\tab\tab\tab   --- Because this issue has been fixed this log has been cleared to save space. ---\line\tab\tab\tab}}\cell}}\row}}{{\trowd\clftsWidth1\cellx5000\clftsWidth1\cellx25000{{{{ExceptionDetails}}\cell}}\cell\row}}\pard\f0\b\fs48 Exception Details \b0\par\fs20\line
{{\trowd\clftsWidth1\cellx5000\clftsWidth1\cellx25000{{{{StackTrace}}\cell}}{{{{<null>}}\cell}}\row}}}}";
			AssertTransform(html, expectedRtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_WithFactoryStatistics()
		{
			var htmlContent = GetSampleFileContents("SampleWithFactoryStatistics.html");
			var expectedRtfContent = GetSampleFileContents("SampleWithFactoryStatistics_rtf.txt");
			AssertTransform(RemoveEmptyTable(htmlContent), expectedRtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleAppleCrashHTML()
		{
			var htmlContent = GetSampleFileContents("SampleAppleCrashHTML_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleAppleCrashHTML_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleHTML()
		{
			var htmlContent = GetSampleFileContents("SampleHTML_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleHTML_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleHTMLEmptyOrMissingStackTrace()
		{
			var htmlContent = GetSampleFileContents("SampleHTMLEmptyOrMissingStackTrace_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleHTMLEmptyOrMissingStackTrace_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleHTMLLargeCommand()
		{
			var htmlContent = GetSampleFileContents("SampleHTMLLargeCommand_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleHTMLLargeCommand_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleOldHTML()
		{
			var htmlContent = GetSampleFileContents("SampleOldHTML_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleOldHTML_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleWithAggregateException()
		{
			var htmlContent = GetSampleFileContents("SampleWithAggregateException_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleWithAggregateException_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleWithDevice()
		{
			var htmlContent = GetSampleFileContents("SampleWithDevice_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleWithDevice_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleWithProcessAndHttp()
		{
			var htmlContent = GetSampleFileContents("SampleWithProcessAndHttp_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleWithProcessAndHttp_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}

		public void TestConvertHtmlToRtf_SampleWithRelatedLogs()
		{
			var htmlContent = GetSampleFileContents("SampleWithRelatedLogs_AfterRemovedEles.htm");
			var rtfContent = GetSampleFileContents("SampleWithRelatedLogs_rtf.txt");
			AssertTransform(htmlContent, rtfContent, ConvertHtmlToRtf);
		}
		#endregion

		#region BeautifyRtfContentStyle

		public void TestBeautifyRtfContentStyle()
		{
			var xmlData = File.ReadAllText(ClientVisibleTestFile);
			var log = Factory.New<EdiHelpErrorLog>();
			var occurrence1 = log.Occurrences.AddNew();
			occurrence1.HO_XMLData = xmlData;
			using (var form = new ZForm())
			using (var oc = new OccurrencesControlForTest())
			{
				form.Controls.Add(oc);
				oc.SetDataBinding(log, "");
				form.Show();
				Application.DoEvents();
				var rtb = oc.ObtainRichTextBox();
				AssertEquals(ControlDpiScalingHelper.NewScaledPadding(15).Left, rtb.SelectionIndent);
			}
		}

		#endregion
#endif

		#region TestRemoveUnusedHtmlElesInRtf
		public void TestRemoveUnusedHtmlElesInRtf_WhenHtmlIsNullOrEmpty()
		{
			// Arrange
			string html = null;
			var expectedResult = string.Empty;

			// Act
			var result = OccurrencesControl.RemoveUnusedHtmlElesInRtf(html);

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestRemoveUnusedHtmlElesInRtf_WhenHtmlIsPlainText()
		{
			// Arrange
			var html = "this is a plain text";
			var expectedResult = "this is a plain text";

			// Act
			var result = OccurrencesControl.RemoveUnusedHtmlElesInRtf(html);

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestRemoveUnusedHtmlElesInRtf_WhenToRemoveElesAllExist()
		{
			// Arrange
			var html = @"<html>
<head></head>
<body>
	<a href=""#Exception Details"">Exception Details</a> | <a href=""#Environment Info"">Environment Info</a> | <a href=""#User Events"">User Events</a> | <a href=""#Sql Events"">Sql Events</a> | <a href=""#Sql Failed Events"">Sql Failed Events</a><br>
	<h1>Summary</h1><br>
	<table>
		<tr>
			<td>ErrorReportID</td>
			<td></td>
		</tr>
	</table>
	<a name=""Exception Details"">
		<h1>Exception Details</h1>
	</a>(<a href=""#Top"">Top</a>)<br><br><button name=""Calculate Line Numbers"">Calculate Line Numbers</button><table>
<tr>
	<td>ExceptionType</td>
	<td>System.Exception</td>
</tr>
<tr>
	<td>Message</td>
	<td>Test Exception</td>
</tr>
<tr>
	<td>Source</td>
	<td>ZClientEDI</td>
</tr>
<a name=""sourceLineNoHyperLink0"">source line code</a>
</table>
	<a name=""User Events"">
		<h1>User Events</h1>
	</a>(<a href=""#Top"">Top</a>)<br><br>
<table></table>
</body>
</html>";
			var expectedResult = @"<html>
<head></head>
<body><h1>Summary</h1><br>
	<table>
		<tr>
			<td>ErrorReportID</td>
			<td></td>
		</tr>
	</table>
	<h1>Exception Details</h1><br><br><table>
<tr>
	<td>ExceptionType</td>
	<td>System.Exception</td>
</tr>
<tr>
	<td>Message</td>
	<td>Test Exception</td>
</tr>
<tr>
	<td>Source</td>
	<td>ZClientEDI</td>
</tr>

</table>
	<h1>User Events</h1><br><br>
<table></table>
</body>
</html>";

			// Act
			var result = OccurrencesControl.RemoveUnusedHtmlElesInRtf(html);

			// Assert
			AssertEquals(expectedResult, result);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleAppleCrashHTML()
		{
			var htmlContent = GetSampleFileContents("SampleAppleCrashHTML.htm");
			var removedContent = GetSampleFileContents("SampleAppleCrashHTML_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleHTML()
		{
			var htmlContent = GetSampleFileContents("SampleHTML.htm");
			var removedContent = GetSampleFileContents("SampleHTML_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleHTMLEmptyOrMissingStackTrace()
		{
			var htmlContent = GetSampleFileContents("SampleHTMLEmptyOrMissingStackTrace.htm");
			var removedContent = GetSampleFileContents("SampleHTMLEmptyOrMissingStackTrace_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleHTMLLargeCommand()
		{
			var htmlContent = GetSampleFileContents("SampleHTMLLargeCommand.htm");
			var removedContent = GetSampleFileContents("SampleHTMLLargeCommand_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleOldHTML()
		{
			var htmlContent = GetSampleFileContents("SampleOldHTML.htm");
			var removedContent = GetSampleFileContents("SampleOldHTML_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleWithAggregateException()
		{
			var htmlContent = GetSampleFileContents("SampleWithAggregateException.htm");
			var removedContent = GetSampleFileContents("SampleWithAggregateException_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleWithDevice()
		{
			var htmlContent = GetSampleFileContents("SampleWithDevice.htm");
			var removedContent = GetSampleFileContents("SampleWithDevice_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleWithProcessAndHttp()
		{
			var htmlContent = GetSampleFileContents("SampleWithProcessAndHttp.htm");
			var removedContent = GetSampleFileContents("SampleWithProcessAndHttp_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}

		public void TestRemoveUnusedHtmlElesInRtf_SampleWithRelatedLogs()
		{
			var htmlContent = GetSampleFileContents("SampleWithRelatedLogs.htm");
			var removedContent = GetSampleFileContents("SampleWithRelatedLogs_AfterRemovedEles.htm");
			AssertTransform(htmlContent, removedContent, OccurrencesControl.RemoveUnusedHtmlElesInRtf);
		}
		#endregion
	}
}

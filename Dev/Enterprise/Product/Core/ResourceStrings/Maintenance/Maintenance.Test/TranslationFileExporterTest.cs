using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.BuildTools.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.ResourceStrings.Cache.Testing;
using CargoWiseOne.ResourceStrings;
using Dat.Integration;
using Dat.Integration.VersionControl;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Analysis;
using Moq;
using NUnit.Framework;
using TestResult = Dat.Integration.TestResult;
namespace Enterprise.ResourceStrings.Maintenance.Test
{
	sealed class TranslationFileExporterTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetNamespaceFromClassName()
		{
			var ns = TranslationFileExporter.GetNamespaceFromClassName("Dummy.ClassA");
			AssertEquals("Dummy", ns);

			ns = TranslationFileExporter.GetNamespaceFromClassName("");
			AssertNullOrEmpty("ns", ns);

			ns = TranslationFileExporter.GetNamespaceFromClassName(null);
			AssertNullOrEmpty("ns", ns);

			ns = TranslationFileExporter.GetNamespaceFromClassName("DummyClassName");
			AssertNullOrEmpty("ns", ns);
		}

		public void TestExportProjectTranslatedXmlHasSourceHash()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("K1", "Key1", new ResourceStringMetaData(contextClassName: "ZArchitecture.Web.Testing")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("K2", "Key2", new ResourceStringMetaData(contextClassName: "ZArchitecture.Web.Testing")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", new ResourceStringData("K3", "Key3", new ResourceStringMetaData(contextClassName: "ZArchitecture.Web.Testing")));

			using (var exporter = new TranslationFileExporter())
			using (var tempDirectory = new TempDirectory())
			{
				var directory = Path.Combine(tempDirectory.DirectoryName, "tradosProject");
				Directory.CreateDirectory(directory);
				exporter.Run(new TradosProjectCreator() { ProjectType = TradosProjectCreator.ProjectTypeCodes.WebTracker }, directory);

				var fileName = Path.Combine(directory, "Web Tracker", "Code", "ZArchitecture.Web.xml");
				using (var reader = new XmlTextReader(fileName))
				{
					string language;
					var datas = XmlResourceStringSource.ReadAll(reader, out language, true).ToList();
					AssertEquals(3, datas.Count);
					Assert("data1 should contain SourceHash attribute", !string.IsNullOrEmpty(datas[0].SourceHash));
					Assert("data2 should contain SourceHash attribute", !string.IsNullOrEmpty(datas[1].SourceHash));
					Assert("data3 should contain SourceHash attribute", !string.IsNullOrEmpty(datas[2].SourceHash));
				}
			}
		}

		public void TestTranslatableContentAdapterExportDocBuilder()
		{
			SetupSourceStringsForAllModules();
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).SetResourceGetter(key => new ResourceStringData(key, key));
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("DocBuilder", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "Labels.xml", "Titles.xml", "Currencies.xml", "MacroValues.xml" }, Directory.GetFiles(tempDirectory).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportGUI()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("GUI", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "Namespace.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportWebTracker()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("WebTracker", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "ZArchitecture.Web.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportReports()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("Reports", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "My Report.xml" }, Directory.GetFiles(tempDirectory).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportBilling()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("Billing", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "Client.EDI.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportPave()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("PAVE", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "BufferManagement.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportUpdateNotes()
		{
			SetupSourceStringsForAllModules();
			using (var tempDirectory = new TempDirectory())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			{
				exporter.ExportAllContent("UpdateNotes", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "UpdateNoteSummaries.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportUntranslatedExcludesTranslated()
		{
			var k1 = new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"));
			var k2 = new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			using (var tempDirectory = new TempDirectory())
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			{
				exporter.WriteTranslations(Core.SharedConstants.Languages.French, new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k1)));
				exporter.WriteTranslations(Core.SharedConstants.Languages.Spanish, new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k2)));
				exporter.ExportUntranslatedContent("GUI", "fr-FR", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "Two.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportUntranslatedForMultipleLanguagesUsingSameInstance()
		{
			var k1 = new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"));
			var k2 = new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"));
			var k3 = new ResourceStringData("k3", "value", new ResourceStringMetaData("Enterprise.Three.Class", "File.cs"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", k3);
			using (var tempDirectory1 = new TempDirectory())
			using (var tempDirectory2 = new TempDirectory())
			using (var tempDirectory3 = new TempDirectory())
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			{
				exporter.WriteTranslations(Core.SharedConstants.Languages.French, new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k1)));
				exporter.WriteTranslations(Core.SharedConstants.Languages.Spanish, new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k2)));
				exporter.WriteTranslations(Core.SharedConstants.Languages.ChineseSimplified, new ResourceStringData("k3", "value", new ResourceStringMetaData("Enterprise.Three.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k3)));
				exporter.ExportUntranslatedContent("GUI", "fr-FR", tempDirectory1);
				AssertContainsExactElementsInAnyOrder(new[] { "Two.xml", "Three.xml" }, Directory.GetFiles(tempDirectory1, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
				exporter.ExportUntranslatedContent("GUI", "es-ES", tempDirectory2);
				AssertContainsExactElementsInAnyOrder(new[] { "One.xml", "Three.xml" }, Directory.GetFiles(tempDirectory2, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterExportUntranslatedIncludesTranslatedWithNonMatchingSourceHash()
		{
			var k1 = new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"));
			var k2 = new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			using (var tempDirectory = new TempDirectory())
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			{
				exporter.WriteTranslations(Core.SharedConstants.Languages.French, new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(new ResourceStringData("k1", "newvalue"))));
				exporter.ExportUntranslatedContent("GUI", "fr-FR", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "One.xml", "Two.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestExportNonGUIModuleExcludeGUITranslations()
		{
			SetupSourceStringsForAllModules();
			ResourceStringsMetaData.UseSourceFile = true;

			var metaData = ResourceStringsMetaData.GetInstance();

			var helpDataString = new HelpDataString();
			helpDataString.HD_Language = Core.SharedConstants.Languages.English;
			helpDataString.HD_Code = "k1";
			helpDataString.HD_Caption = "value";
			helpDataString.HD_FullDescription = "value";
			helpDataString.HD_ControlPath = "Enterprise.GUI,Enterprise.GUI.Testing.FormTest:";
			helpDataString.HD_IsCheckedOut = false;

			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings(MacroString))
			using (var tempDirectory = new TempDirectory())
			{
				ResourcesDeltaSource.ForcedSourcePath = tempDirectory;

				var deltaSourceFilePath = metaData.File;
				var subDirectory = Path.GetDirectoryName(deltaSourceFilePath);
				Directory.CreateDirectory(subDirectory);
				using (File.Create(deltaSourceFilePath))
				{
				}

				metaData.Save(new[] { helpDataString });
				exporter.ExportAllContent("PAVE", tempDirectory);

				var fileName = Path.Combine(tempDirectory, "Core", "GUI", "Enterprise.GUI.Form.xml");
				Assert("Should not export GUI translations", !File.Exists(fileName));
			}
		}

		public void TestTranslatableContentAdapterImportTranslatedContent()
		{
			var k = new ResourceStringData("k", "value");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k", k);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var kfrn = new ResourceStringData("k", "translated", sourceHash: resHashCalculator.GetHash(k));
				WriteStrings(Path.Combine(tempDirectory, "Stuff.xml"), kfrn);

				exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				var checkedOutString = checkedOutStrings.Single();
				AssertEquals(checkedOutString.ToResourceStringData(), kfrn);
			}
		}

		public void TestTranslatableContentAdapterImportTranslatedContentExcludesNonMatchingSourceHash()
		{
			var k1 = new ResourceStringData("k1", "one");
			var k2 = new ResourceStringData("k2", "two");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var k1frn = new ResourceStringData("k1", "fr one", sourceHash: resHashCalculator.GetHash(new ResourceStringData("k1", "newvalue")));
				var k2frn = new ResourceStringData("k2", "fr two", sourceHash: resHashCalculator.GetHash(k2));
				WriteStrings(Path.Combine(tempDirectory, "Stuff.xml"), k1frn, k2frn);

				exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				var checkedOutString = checkedOutStrings.Single();
				AssertEquals(checkedOutString.ToResourceStringData(), k2frn);
			}
		}

		public void TestTranslatableContentAdapterImportTranslatedContentExcludesNonMatchingSource()
		{
			var k2 = new ResourceStringData("k2", "two");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var k1frn = new ResourceStringData("k1", "fr one", sourceHash: resHashCalculator.GetHash(new ResourceStringData("k1", "one")));
				var k2frn = new ResourceStringData("k2", "fr two", sourceHash: resHashCalculator.GetHash(k2));
				WriteStrings(Path.Combine(tempDirectory, "Stuff.xml"), k1frn, k2frn);

				exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				var checkedOutString = checkedOutStrings.Single();
				AssertEquals(checkedOutString.ToResourceStringData(), k2frn);
			}
		}

		public void TestTranslatableContentAdapterImportTranslatedContentMultipleFiles()
		{
			var k1 = new ResourceStringData("k1", "one");
			var k2 = new ResourceStringData("k2", "two");
			var k3 = new ResourceStringData("k3", "three");
			var k4 = new ResourceStringData("k4", "four");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", k3);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k4", k4);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var k1frn = new ResourceStringData("k1", "fr one", sourceHash: resHashCalculator.GetHash(k1));
				var k2frn = new ResourceStringData("k2", "fr two", sourceHash: resHashCalculator.GetHash(k2));
				var k3frn = new ResourceStringData("k3", "fr three", sourceHash: resHashCalculator.GetHash(k3));
				var k4frn = new ResourceStringData("k4", "fr four", sourceHash: resHashCalculator.GetHash(k4));
				WriteStrings(Path.Combine(tempDirectory, "One.xml"), k1frn, k2frn);
				WriteStrings(Path.Combine(tempDirectory, "Two.xml"), k3frn, k4frn);

				exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				AssertContainsExactElementsInAnyOrder(checkedOutStrings.Select(s => s.ToResourceStringData()), new[] { k1frn, k2frn, k3frn, k4frn });
			}
		}

		public void TestTranslatableContentAdapterImportTranslatedContentCalledMultipleTimesOnMultipleFilesAndLanguages()
		{
			var k1 = new ResourceStringData("k1", "one");
			var k2 = new ResourceStringData("k2", "two");
			var k3 = new ResourceStringData("k3", "three");
			var k4 = new ResourceStringData("k4", "four");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", k3);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k4", k4);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			{
				using (var tempDirectory = new TempDirectory())
				{
					var k1frn = new ResourceStringData("k1", "fr one", sourceHash: resHashCalculator.GetHash(k1));
					var k2frn = new ResourceStringData("k2", "fr two", sourceHash: resHashCalculator.GetHash(k2));
					WriteStrings(Path.Combine(tempDirectory, "stuff.xml"), k1frn, k2frn);

					exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

					var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
					AssertContainsExactElementsInAnyOrder(checkedOutStrings.Select(s => s.ToResourceStringData()), new[] { k1frn, k2frn });
					ResourceStringsFactory.UndoCheckout(checkedOutStrings);
				}

				using (var tempDirectory = new TempDirectory())
				{
					var k1spn = new ResourceStringData("k1", "es one", sourceHash: resHashCalculator.GetHash(k1));
					var k2spn = new ResourceStringData("k2", "es two", sourceHash: resHashCalculator.GetHash(k2));
					WriteStrings(Path.Combine(tempDirectory, "stuff.xml"), k1spn, k2spn);

					exporter.ImportTranslatedContent("whatever", "es-ES", tempDirectory, null);

					var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
					AssertContainsExactElementsInAnyOrder(checkedOutStrings.Select(s => s.ToResourceStringData()), new[] { k1spn, k2spn });
					ResourceStringsFactory.UndoCheckout(checkedOutStrings);
				}

				using (var tempDirectory = new TempDirectory())
				{
					var k3frn = new ResourceStringData("k3", "fr three", sourceHash: resHashCalculator.GetHash(k3));
					var k4frn = new ResourceStringData("k4", "fr four", sourceHash: resHashCalculator.GetHash(k4));
					WriteStrings(Path.Combine(tempDirectory, "stuff.xml"), k3frn, k4frn);

					exporter.ImportTranslatedContent("whatever", "fr-FR", tempDirectory, null);

					var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
					AssertContainsExactElementsInAnyOrder(checkedOutStrings.Select(s => s.ToResourceStringData()), new[] { k3frn, k4frn });
					ResourceStringsFactory.UndoCheckout(checkedOutStrings);
				}

				using (var tempDirectory = new TempDirectory())
				{
					var k3spn = new ResourceStringData("k3", "es three", sourceHash: resHashCalculator.GetHash(k3));
					var k4spn = new ResourceStringData("k4", "es four", sourceHash: resHashCalculator.GetHash(k4));
					WriteStrings(Path.Combine(tempDirectory, "stuff.xml"), k3spn, k4spn);

					exporter.ImportTranslatedContent("whatever", "es-ES", tempDirectory, null);

					var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
					AssertContainsExactElementsInAnyOrder(checkedOutStrings.Select(s => s.ToResourceStringData()), new[] { k3spn, k4spn });
					ResourceStringsFactory.UndoCheckout(checkedOutStrings);
				}
			}
		}

		public void TestTranslatableContentAdapterExportLanguageCodeZh()
		{
			var k1 = new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"));
			var k2 = new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.Two.Class", "File.cs"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", k1);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", k2);
			using (var tempDirectory = new TempDirectory())
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			{
				exporter.WriteTranslations(Core.SharedConstants.Languages.ChineseTraditional, new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.One.Class", "File.cs"), sourceHash: resHashCalculator.GetHash(k1)));
				exporter.ExportUntranslatedContent("GUI", "zh", tempDirectory);
				AssertContainsExactElementsInAnyOrder(new[] { "Two.xml" }, Directory.GetFiles(tempDirectory, "*", SearchOption.AllDirectories).Select(f => Path.GetFileName(f)));
			}
		}

		public void TestTranslatableContentAdapterImportLanguageCodeZh()
		{
			var k = new ResourceStringData("k", "value");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k", k);
			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var zh = new ResourceStringData("k", "translated", sourceHash: resHashCalculator.GetHash(k));
				WriteStrings(Path.Combine(tempDirectory, "Stuff.xml"), zh);

				exporter.ImportTranslatedContent("whatever", "zh", tempDirectory, null);

				var checkedOutStrings = ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true));
				var checkedOutString = checkedOutStrings.Single();
				AssertEquals(checkedOutString.ToResourceStringData(), zh);
				AssertEquals(Core.SharedConstants.Languages.ChineseTraditional, checkedOutString.HD_Language);
			}
		}

		void WriteStrings(string file, params ResourceStringData[] data)
		{
			using (var stream = File.Create(file))
			{
				new ResourceStringXmSerializer(stream, Res.DefaultLanguage).Serialize(data);
			}
		}

		void SetupSourceStringsForAllModules()
		{
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put(DocBuilderResourceStrings.ReportNameKeyPrefix + "Something", new ResourceStringData(DocBuilderResourceStrings.ReportNameKeyPrefix + "Something", "Something"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("RX_Desc$Something", new ResourceStringData("RX_Desc$Something", "Something"));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put(MacroString.Key, MacroString);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k1", new ResourceStringData("k1", "value", new ResourceStringMetaData("Enterprise.Namespace.Class", "File.cs")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k2", new ResourceStringData("k2", "value", new ResourceStringMetaData("Enterprise.ZArchitecture.Web.Class", "File.cs")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k3", new ResourceStringData("k3", "dnt", new ResourceStringMetaData("Enterprise.Customs.AU.Business.Class", "File.cs")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k4", new ResourceStringData("k4", "value", new ResourceStringMetaData("Enterprise.Client.EDI.Billing.Class", "File.cs")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put(DocBuilderResourceStrings.ReportLabelKeyPrefix + "|My Report|Something", new ResourceStringData(DocBuilderResourceStrings.ReportLabelKeyPrefix + "|My Report|Something", "Something", new ResourceStringMetaData("My Report")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("k5", new ResourceStringData("k5", "value", new ResourceStringMetaData("Enterprise.BufferManagement.Business.TheBuffer", "TheBuffer.cs")));
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("GF_Summary$/hSAntTB2s53Ko53/L7YFA==#RW1haWwgRGVzdGluYXRpb24gT3ZlcnJpZGUgbm93IGxpc3RzIG9yaWdpbmFsIHJlY2lwaWVudHM6IElmIHRoZSBTeXN0ZW0gPiBUZXN0aW5nID4gRW1haWwgRGVzdGluYXRpb24gT3ZlcnJpZGUgaXMgc2V0LCB0aGUg", new ResourceStringData("GF_Summary$/hSAntTB2s53Ko53/L7YFA==#RW1haWwgRGVzdGluYXRpb24gT3ZlcnJpZGUgbm93IGxpc3RzIG9yaWdpbmFsIHJlY2lwaWVudHM6IElmIHRoZSBTeXN0ZW0gPiBUZXN0aW5nID4gRW1haWwgRGVzdGluYXRpb24gT3ZlcnJpZGUgaXMgc2V0LCB0aGUg", "Email Destination Override now lists original recipients: If the System > Testing > Email Destination Override is set, the original recipients of the email will be listed at the end of the email.", new ResourceStringMetaData("Enterprise.MasterFiles.Business.GlbReleaseNote", @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Global\ReleaseNotes\GlbReleaseNote\GlbReleaseNote.cs")));
		}

		ResourceStringData MacroString => new ResourceStringData("macrostuff", "value", new ResourceStringMetaData("Enterprise.Macros.Stuff", "File.cs"));

		public void TestCheckinWithFailResString()
		{
			var errData = new ResourceStringData("a", "incorrect");
			var correctData = new ResourceStringData("b", "correct");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("a", errData);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("b", correctData);

			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var tempPath = Path.Combine(tempDirectory, "ResourcesDelta.xml");
				var errorTranslation = new ResourceStringData("a", "translated", sourceHash: resHashCalculator.GetHash(errData));
				var correctTransltion = new ResourceStringData("b", "translated", sourceHash: resHashCalculator.GetHash(correctData));
				WriteStrings(tempPath, errorTranslation, correctTransltion);
				var errorFormatter = new TranslationErrorFormatter();
				errorFormatter.AddTranslationError("ZH-CN", "a", "Something wrong", "incorrect", "translated");

				var resources = new XmlDocument();
				resources.Load(tempPath);
				var resNodes = resources.DocumentElement.SelectNodes("Res");
				AssertEquals("Should have 2 translations in delta file", 2, resNodes.Count);

				var repositoryPendingChange = new MockRepository(MockBehavior.Strict);
				var pendingResChange = repositoryPendingChange.Create<IPendingChange>();
				pendingResChange.Setup(p => p.ServerItem).Returns(@"$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/ZH-CN/ResourcesDelta.xml");
				pendingResChange.Setup(p => p.LocalItem).Returns(tempPath);
				var repositoryWorkSpaceAccess = new MockRepository(MockBehavior.Strict);
				var workSpaceAccess = repositoryWorkSpaceAccess.Create<IWorkspaceAccess>();
				workSpaceAccess.Setup(p => p.GetPendingChanges()).Returns(new[] { pendingResChange.Object });

				var testIdentifier = new TestIdentifier("Test", "Test", "Test");
				var failedTest = new TestResult(new TestDescriptor(testIdentifier), TimeSpan.Zero, DateTime.MinValue, errorFormatter.GetFormattedTranslationError());

				var message = exporter.HandleFailures("", "ZH-CN", workSpaceAccess.Object, new[] { failedTest }, out bool reshelve);
				resources.Load(tempPath);
				resNodes = resources.DocumentElement.SelectNodes("Res");

				Assert("Should reshelve automatically", reshelve);
				AssertContains("Error message should be picked out in failure handling", "Something wrong", message);
				AssertEquals("Error translation should be removed from delta file", 1, resNodes.Count);
				AssertEquals("Error translation should be removed from delta file", "b", resNodes[0].SelectSingleNode("Key").InnerText);
			}
		}

		public void TestCheckinWithoutFailResString()
		{
			var correctData = new ResourceStringData("b", "correct");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("b", correctData);

			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var tempPath = Path.Combine(tempDirectory, "ResourcesDelta.xml");
				var correctTransltion = new ResourceStringData("b", "translated", sourceHash: resHashCalculator.GetHash(correctData));
				WriteStrings(tempPath, correctTransltion);

				var resources = new XmlDocument();
				resources.Load(tempPath);
				var resNodes = resources.DocumentElement.SelectNodes("Res");
				AssertEquals("Should have a translation in delta file", 1, resNodes.Count);

				var repositoryPendingChange = new MockRepository(MockBehavior.Strict);
				var pendingResChange = repositoryPendingChange.Create<IPendingChange>();
				pendingResChange.Setup(p => p.ServerItem).Returns(@"$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/ZH-CN/ResourcesDelta.xml");
				pendingResChange.Setup(p => p.LocalItem).Returns(tempPath);
				var repositoryWorkSpaceAccess = new MockRepository(MockBehavior.Strict);
				var workSpaceAccess = repositoryWorkSpaceAccess.Create<IWorkspaceAccess>();
				workSpaceAccess.Setup(p => p.GetPendingChanges()).Returns(new[] { pendingResChange.Object });

				var testIdentifier = new TestIdentifier("Test", "Test", "Test");
				var failedTest = new TestResult(new TestDescriptor(testIdentifier), TimeSpan.Zero, DateTime.MinValue, "Something else is wrong");

				var message = exporter.HandleFailures("", "ZH-CN", workSpaceAccess.Object, new[] { failedTest }, out bool reshelve);
				resources.Load(tempPath);
				resNodes = resources.DocumentElement.SelectNodes("Res");

				Assert("Should not reshelve automatically", !reshelve);
				AssertNullOrEmpty("No error message should be picked out in failure handling", message);
				AssertEquals("Nothing should be removed from delta file", 1, resNodes.Count);
				AssertEquals("Nothing should be removed from delta file", "b", resNodes[0].SelectSingleNode("Key").InnerText);
			}
		}

		public void TestHandleFailuresZhLanguage()
		{
			var errData = new ResourceStringData("a", "incorrect");
			var correctData = new ResourceStringData("b", "correct");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("a", errData);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("b", correctData);

			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var tempPath = Path.Combine(tempDirectory, "ResourcesDelta.xml");
				var errorTranslation = new ResourceStringData("a", "translated", sourceHash: resHashCalculator.GetHash(errData));
				var correctTransltion = new ResourceStringData("b", "translated", sourceHash: resHashCalculator.GetHash(correctData));
				WriteStrings(tempPath, errorTranslation, correctTransltion);
				var errorFormatter = new TranslationErrorFormatter();
				errorFormatter.AddTranslationError("ZH-TW", "a", "Something wrong", "incorrect", "translated");

				var resources = new XmlDocument();
				resources.Load(tempPath);
				var resNodes = resources.DocumentElement.SelectNodes("Res");
				AssertEquals("Should have 2 translations in delta file", 2, resNodes.Count);

				var repositoryPendingChange = new MockRepository(MockBehavior.Strict);
				var pendingResChange = repositoryPendingChange.Create<IPendingChange>();
				pendingResChange.Setup(p => p.ServerItem).Returns(@"$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/ZH-TW/ResourcesDelta.xml");
				pendingResChange.Setup(p => p.LocalItem).Returns(tempPath);
				var repositoryWorkSpaceAccess = new MockRepository(MockBehavior.Strict);
				var workSpaceAccess = repositoryWorkSpaceAccess.Create<IWorkspaceAccess>();
				workSpaceAccess.Setup(p => p.GetPendingChanges()).Returns(new[] { pendingResChange.Object });

				var testIdentifier = new TestIdentifier("Test", "Test", "Test");
				var failedTest = new TestResult(new TestDescriptor(testIdentifier), TimeSpan.Zero, DateTime.MinValue, errorFormatter.GetFormattedTranslationError());

				var message = exporter.HandleFailures("", "ZH", workSpaceAccess.Object, new[] { failedTest }, out bool reshelve);
				resources.Load(tempPath);
				resNodes = resources.DocumentElement.SelectNodes("Res");

				Assert("Should reshelve automatically", reshelve);
				AssertContains("Error message should be picked out in failure handling", "Something wrong", message);
				AssertEquals("Error translation should be removed from delta file", 1, resNodes.Count);
				AssertEquals("Error translation should be removed from delta file", "b", resNodes[0].SelectSingleNode("Key").InnerText);
			}
		}

		public void TestCheckinWithMultipleFailResStringSections()
		{
			var errData = new ResourceStringData("a", "incorrect");
			var correctData = new ResourceStringData("b", "correct");
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("a", errData);
			ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English).Put("b", correctData);

			using (var resHashCalculator = new ResourceStringHashCalculator())
			using (var exporter = new TranslationFileExporterWithMockBizOFieldStrings())
			using (var tempDirectory = new TempDirectory())
			{
				var tempPath = Path.Combine(tempDirectory, "ResourcesDelta.xml");
				var errorTranslation1 = new ResourceStringData("a", "translated", sourceHash: resHashCalculator.GetHash(errData));
				var errorTranslation2 = new ResourceStringData("b", "translated", sourceHash: resHashCalculator.GetHash(errData));
				var correctTransltion = new ResourceStringData("c", "translated", sourceHash: resHashCalculator.GetHash(correctData));
				WriteStrings(tempPath, errorTranslation1, errorTranslation2, correctTransltion);
				var errorFormatter1 = new TranslationErrorFormatter();
				errorFormatter1.AddTranslationError("ZH-CN", "a", "Something wrong", "incorrect", "translated");
				var errorFormatter2 = new TranslationErrorFormatter();
				errorFormatter2.AddTranslationError("ZH-CN", "b", "Something else wrong", "incorrect again", "translated again");

				var repositoryPendingChange = new MockRepository(MockBehavior.Strict);
				var pendingResChange = repositoryPendingChange.Create<IPendingChange>();
				pendingResChange.Setup(p => p.ServerItem).Returns(@"$/Dev/Enterprise/Product/Core/ResourceStrings/DataFiles/ZH-CN/ResourcesDelta.xml");
				pendingResChange.Setup(p => p.LocalItem).Returns(tempPath);
				var repositoryWorkSpaceAccess = new MockRepository(MockBehavior.Strict);
				var workSpaceAccess = repositoryWorkSpaceAccess.Create<IWorkspaceAccess>();
				workSpaceAccess.Setup(p => p.GetPendingChanges()).Returns(new[] { pendingResChange.Object });

				var testIdentifier = new TestIdentifier("Test", "Test", "Test");
				var failedTest = new TestResult(new TestDescriptor(testIdentifier), TimeSpan.Zero, DateTime.MinValue, "<tr><td>" + errorFormatter1.GetFormattedTranslationError() + "</td></tr><tr><td>" + errorFormatter2.GetFormattedTranslationError() + "</td></tr>");

				var message = exporter.HandleFailures("", "ZH-CN", workSpaceAccess.Object, new[] { failedTest }, out bool reshelve);
				var resources = new XmlDocument();
				resources.Load(tempPath);
				var resNodes = resources.DocumentElement.SelectNodes("Res");

				Assert("Should reshelve automatically", reshelve);
				AssertContains("Error message should be picked out in failure handling", "Something wrong", message);
				AssertEquals("Error translation should be removed from delta file", 1, resNodes.Count);
				AssertEquals("Error translation should be removed from delta file", "c", resNodes[0].SelectSingleNode("Key").InnerText);
			}
		}

		protected override void SetUp()
		{
			resourceStringsMockSources = ResourceStringsFactory.MockSources();
			MockSourceControl.Setup();
			base.SetUp();
		}

		protected override void TearDown()
		{
			resourceStringsMockSources.Dispose();
			MockSourceControl.TearDown();
			base.TearDown();
		}

		IDisposable resourceStringsMockSources;

		class TranslationFileExporterWithMockBizOFieldStrings : TranslationFileExporter
		{
			public TranslationFileExporterWithMockBizOFieldStrings(params ResourceStringData[] data)
			{
				DocBuilderBizOFieldCodeStrings = new Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>>();
				DocBuilderBizOFieldCodeStrings.Add("macro", new DataWithDocBuilderUsage<CodeStringFinder.ResourceStringReference[]>(data.Select(r => new CodeStringFinder.ResourceStringReference(r.Key, r.Caption, null, null)).ToArray()));
			}

			protected override Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> DocBuilderBizOFieldCodeStrings
			{
				get;
			}

			public void WriteTranslations(string langauge, params ResourceStringData[] data)
			{
				Directory.CreateDirectory(Path.Combine(DataFileSourcePath, langauge));
				using (var stream = File.Create(Path.Combine(DataFileSourcePath, langauge, "Resources.xml")))
				{
					new ResourceStringXmSerializer(stream, langauge).Serialize(data);
				}
			}

			protected override string DataFileSourcePath => dataFileSourcePath.DirectoryName;

			protected override void Dispose(bool isDisposing)
			{
				base.Dispose(isDisposing);
				dataFileSourcePath.Dispose();
			}

			readonly TempDirectory dataFileSourcePath = new TempDirectory();
		}
	}
}

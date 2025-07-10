using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GroupCount))]
	sealed class GroupCountTest : ValueProviderTest
	{
		public void TestGroupCountWithFieldDownTwoLevelsWithVisualizedDataSet()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionHeader]
{B}-[<GroupCount(Collection, Internal.Z0_VarCharMax)>]
{A}-[#SectionBody:Data=Collection]
{A}-[#GroupBy:Collection.Internal.Z0_VarCharMax]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected = "{B}-[3]";

			var dummy = Factory.New<DummyDocumentSupportable>();
			for (var group = 0; group < 3; group++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = group.ToString();
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("There should be 3 groups.", expected, excelInterface.WorkSheets.First().ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.VisualizerContentNote.DataSource);

				var grid = (VisualiserComponentGrid)converter.Components.First((component) => component is VisualiserComponentGrid);
				var dataSet = grid.DS;

				report.VisualizerContentNote.Factory.Save();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("There should be 3 groups.", expected, excelInterface.WorkSheets.First().ToString());
					}
				}
			}
		}

		public void TestReplacementNonExistingTable()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			PrepareRenderer();
			AssertEquals(0, ValueProviderToTest.GetReplacement("< GroupCount ( TableName, FieldName )  >", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in GroupCount Macro: Table TableName Does not belong to data provider.]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacementUsingBusinessObject()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			ZBool normalBool = true;
			ZBool filteredBool = false;
			List<ZDecimal> zDecimals = new List<ZDecimal>();
			zDecimals.Add(new ZDecimal(3.14m));
			zDecimals.Add(new ZDecimal(0.04m));
			zDecimals.Add(new ZDecimal(98.97m));
			List<ZString> zStrings = new List<ZString>();
			zStrings.Add(new ZString("foo"));
			zStrings.Add(new ZString("bar"));
			zStrings.Add(new ZString("hello"));
			zStrings.Add(new ZString("world"));

			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[0], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[1], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[2], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[2], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[1], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[0], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[2], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[1], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[1], zStrings[0]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[2], zStrings[3]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[1], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[0], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[1], zStrings[2]);
			AddChildToDummyObject(topLevelDataSource, filteredBool, zDecimals[1], zStrings[1]);
			AddChildToDummyObject(topLevelDataSource, normalBool, zDecimals[1], zStrings[1]);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupCountFilteredDataWithFilteredBizObjDataSource.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				report.PrepareForRender();
				List<Section> sections = report.Analyser.Sections;

				AssertEquals("Precondition: sections.Count", 2, sections.Count);

				AssertEquals("ValueProviderToTest.GetReplacement(\"<GroupCount(Collection, Z0_Decimal, <Z0_Bool>==false)>\", report)", 3, ValueProviderToTest.GetReplacement("<GroupCount(Collection, Z0_Decimal, <Z0_Bool>==false)>", report));
				AssertEquals("ValueProviderToTest.GetReplacement(\"<GroupCount(Collection, Z0_NVarCharMax, <Z0_Bool>==false)>\", report)", 4, ValueProviderToTest.GetReplacement("<GroupCount(Collection, Z0_NVarCharMax, <Z0_Bool>==false)>", report));
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupCount (    Tst , ewrwe ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupCount (    Tst , ewrwe+eee ) >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupCount (    Tst , ewrwe , \"FieldOne\"==\"FieldTwo\" && \"FieldOne\"==\"FieldTwo\") >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupCount (    Tst , ewrwe+eee , (\"FieldOne\"  == \"FieldTwo\" && \"FieldOne\" == \"FieldTwo\")  ) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GroupCount(12)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GroupCunt(Name, sad)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GroupCount(Tbl.Name)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GroupCount()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(1, ValueProviderToTest.GetReplacement("< GroupCount ( Header,  Number)  >", Report));
			AssertEquals(8, ValueProviderToTest.GetReplacement("< GroupCount ( Lines,GST )  >", Report));
			AssertEquals(1, ValueProviderToTest.GetReplacement("< GroupCount ( Lines,GST, <GST>==5 )  >", Report));
			AssertEquals(1, ValueProviderToTest.GetReplacement("< GroupCount ( Lines,GST, (<GST>==5) )  >", Report));
		}

		public void TestReplacementMultiField()
		{
			PrepareRenderer();
			AssertEquals(40, ValueProviderToTest.GetReplacement("< GroupCount ( Lines,GST+GSTRate )  >", Report));
			AssertEquals(40, ValueProviderToTest.GetReplacement("< GroupCount ( Lines,GST +GSTRate )  >", Report));
			AssertEquals(40, ValueProviderToTest.GetReplacement("< GroupCount ( Lines, GST + GSTRate )  >", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GroupCount();
		}

		#region Implementation
		DummyChildBusinessObject AddChildToDummyObject(DummyBusinessObject topLevelDataSource, ZBool valueBool, ZDecimal valueDecimal, ZString valueNText)
		{
			DummyChildBusinessObject result = topLevelDataSource.Collection.AddNew();
			result.Z0_Bool = valueBool;
			result.Z0_Decimal = valueDecimal;
			result.Z0_NVarCharMax = valueNText;
			return result;
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplateForUnitTesting excelTemplate)
		{
			return new Report(new DocumentPack(Factory.New<DocumentCommand>()), excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
		}

		protected override void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			AssertEquals(expectedResult, ValueProviderToTest.GetReplacement(example, Report));
		}

		#endregion
	}
}

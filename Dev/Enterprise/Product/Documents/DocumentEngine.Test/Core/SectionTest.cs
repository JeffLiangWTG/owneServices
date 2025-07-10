using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class SectionTest : TestCaseWithFactory
	{
		public void TestGetAllAreas()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				testSection.SectionPageHeader = new SectionPageHeaderArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				testSection.SectionPageFooter = new SectionPageFooterArea(1, 2, testReport, "");
				testSection.SectionFooter = new SectionFooterArea(1, 2, testReport, "");
				AssertEquals(4, testSection.DataAreas.Count);
			}
		}

		public void TestGetAllAreasWithSomeNullAreas()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				testSection.SectionPageFooter = new SectionPageFooterArea(1, 2, testReport, "");
				AssertEquals(3, testSection.DataAreas.Count);
			}
		}

		public void TestGetSectionBody()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				testSection.SectionBodyAndGroupByAreas.Add(new GroupByArea(1, 2, testReport, "#GroupBy:Data=test.SomeField"));
				testSection.SectionPageFooter = new SectionPageFooterArea(1, 2, testReport, "");
				AssertEquals(testSection.SectionBodyAndGroupByAreas[0], testSection.SectionBody);
			}
		}

		public void TestStartingRowOfSection()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				AssertEquals(1, testSection.StartingRowOfSection);

				testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				AssertEquals(1, testSection.StartingRowOfSection);

				testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(3, 4, testReport, "#SectionBody:Data=test"));
				AssertEquals(1, testSection.StartingRowOfSection);

				testSection = new Section();
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(3, 4, testReport, "#SectionBody:Data=test"));
				AssertEquals(3, testSection.StartingRowOfSection);

				testSection = new Section();
				AssertEquals(-1, testSection.StartingRowOfSection);
			}
		}

		public void TestAreasToRemoveIfThereIsNoData()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "");
				testSection.SectionFooter = new SectionFooterArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				AssertEquals(3, testSection.AreasToRemoveIfThereIsNoData.Count);

				testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "#SectionHeader:" + Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
				testSection.SectionFooter = new SectionFooterArea(1, 2, testReport, "");
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				AssertEquals(2, testSection.AreasToRemoveIfThereIsNoData.Count);
			}
		}

		public void TestAreasToRemoveIfThereIsNoDataWithPersistantSectionFooter()
		{
			using (var testReport = new Report(pack, TestReport))
			{
				var testSection = new Section();

				testSection = new Section();
				testSection.SectionHeader = new SectionHeaderArea(1, 2, testReport, "#SectionHeader:" + Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
				testSection.SectionFooter = new SectionFooterArea(1, 2, testReport, "#SectionFooter:" + Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
				testSection.SectionBodyAndGroupByAreas.Add(new SectionBodyArea(1, 2, testReport, "#SectionBody:Data=test"));
				AssertEquals(1, testSection.AreasToRemoveIfThereIsNoData.Count);
			}
		}

		public void TestAddAreaColumnsToFormulaProvider()
		{
			using (var report = new Report(pack, TestReport))
			{
				var workSheet = report.WorkSheetCurrentlyBeingProcessed;
				workSheet[1, 1] = "<Tbl.test>";
				workSheet[1, 2] = "<SomeMacro(SomeParameter)>";
				workSheet[1, 3] = "<SomeMacro(<Param1>, <Param2>)>";
				workSheet[1, 4] = "<AutoHeight><Table.Column>";

				var area = new ConfigArea(0, 2, report, "");
				var formulaProvider = new FormulaProvider(workSheet);
				var section = new Section();
				section.AddAreaColumnsToFormulaProvider(report, area, formulaProvider, false);
				AssertEquals(true, formulaProvider.ContainsColumn("Tbl.test"));
				AssertEquals(true, formulaProvider.ContainsColumn("SomeMacro(SomeParameter)"));
				AssertEquals(true, formulaProvider.ContainsColumn("SomeMacro(<Param1>, <Param2>)"));
				AssertEquals(true, formulaProvider.ContainsColumn("Table.Column"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRowRangeIsCorrectAfterGroupTitleMovement()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "Aye", 1);
			dummy.Collection.AddNew("A", "Aye", 2);
			dummy.Collection.AddNew("A", "Aye2", 3);
			dummy.Collection.AddNew("A", "Aye2", 4);
			dummy.Collection.AddNew("B", "Bee", 5);
			dummy.Collection.AddNew("B", "Bee", 6);
			dummy.Collection.AddNew("B", "Bee2", 7);
			dummy.Collection.AddNew("B", "Bee2", 8);
			var excelTemplate = new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSourceForTestRowRangeAfterMovingGroupBySection.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(DocumentPack.EmptyPack, excelTemplate, BODocDataProvider.Get(dummy), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertMultilineASCIIEquals("Sum value should be correct on both group title and group bottom.",
							@"{B}-[test]
{B}-[Code Group Top]   {D}-[A]
{B}-[Description Group Top]   {D}-[Aye]
{C}-[1]
{C}-[2]
{B}-[Sum at Bottom of Descritpion Level:]   {E}-[3]
{B}-[Description Group Top]   {D}-[Aye2]
{C}-[3]
{C}-[4]
{B}-[Sum at Bottom of Descritpion Level:]   {E}-[7]
{B}-[Sum at Bottom of Code Level:]   {E}-[10]
{B}-[Code Group Top]   {D}-[B]
{B}-[Description Group Top]   {D}-[Bee]
{C}-[5]
{C}-[6]
{B}-[Sum at Bottom of Descritpion Level:]   {E}-[11]
{B}-[Description Group Top]   {D}-[Bee2]
{C}-[7]
{C}-[8]
{B}-[Sum at Bottom of Descritpion Level:]   {E}-[15]
{B}-[Sum at Bottom of Code Level:]   {E}-[26]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			pack = new DocumentPack();
		}

		DocumentPack pack;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		ExcelTemplateForUnitTesting testReport;
		ExcelTemplateForUnitTesting TestReport
		{
			get
			{
				if (testReport == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testReport = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testReport;
			}
		}
	}
}

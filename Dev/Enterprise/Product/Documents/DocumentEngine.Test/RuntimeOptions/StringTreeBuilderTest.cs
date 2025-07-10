using System.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class StringTreeBuilderTest : TempFileTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadFilterDefinitionsIntoTree()
		{
			var expectedResult = "<Filters> (<Some description> (<Type> (<Text> ()), <Field> (<XX_Field> ())), <Def on same line> (<Col b> (<Col c> (<Col d on the line below> (), <Col d again> ()), <back to c> (<d on the same line> (), <another d> ()), <c by itself> (), <c> (<D> ())), <back 2 levels to b> (), <b again> (<c> (<d> (<e> (<f> (), <g> ()))))), <Nothing> (<on> (<the> (<same> (<line> ())))), <Everything> (<on> (<the> (<same> (<line> ())))))";
			AssertReadFilterDefinitionsIntoTree("MultipleFilters.xls", expectedResult);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestReadFilterDefinitionsIntoTree_CanHandleXLSXTemplate()
		{
			var expectedResult = "<Filters> (<Some description> (<Type> (<Text> ()), <Field> (<XX_Field> ())), <Def on same line> (<Col b> (<Col c> (<Col d on the line below> (), <Col d again> ()), <back to c> (<d on the same line> (), <another d> ()), <c by itself> (), <c> (<D> ())), <back 2 levels to b> (), <b again> (<c> (<d> (<e> (<f> (), <g> ()))))), <Nothing> (<on> (<the> (<same> (<line> ())))), <Everything> (<on> (<the> (<same> (<line> ())))), <A> (<far> (<far> (<away> (<filter> ())))))";
			AssertReadFilterDefinitionsIntoTree("MultipleFiltersWithLastFilterBeyondMaxRowSupportedByExcel97_2003.xlsx", expectedResult);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEmptyDisplayNameFilter()
		{
			AssertLastCollectionAtLevelFilter("EmptyDisplayNameFilter.xls", "The filter at row 1 does not have a Display Name. Please enter a value for it.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLastCollectionAtLevelFilter()
		{
			AssertLastCollectionAtLevelFilter("LastCollectionAtLevelFilter.xls", "The filter 'Status' is not properly configured. Please make sure you have provided a type and other suitable options for that filter.");
		}

		public void AssertLastCollectionAtLevelFilter(string fileName, string errorMessage)
		{
			using (var excelInterface = new ExcelInterface())
			{
				var excelFile = PrepFile(UnitTestingConstants.TestFilesDir + fileName);

				try
				{
					excelInterface.LoadExcelFile(excelFile);
					AssertExceptionThrown<TemplateDefinitionException>(errorMessage, () => new StringTreeBuilder(excelInterface.WorkSheets[0]).GetTree());
				}
				finally
				{
					File.Delete(excelFile);
				}
			}
		}

		void AssertReadFilterDefinitionsIntoTree(string reportPath, string expectedResult)
		{
			using (var excelInterface = new ExcelInterface())
			{
				var fileName = PrepFile(UnitTestingConstants.TestFilesDir + reportPath);

				try
				{
					excelInterface.LoadExcelFile(fileName);
					AssertEquals("Filter tree as string", expectedResult, new StringTreeBuilder(excelInterface.WorkSheets[0]).GetTree().ToString());
				}
				finally
				{
					File.Delete(fileName);
				}
			}
		}
	}
}

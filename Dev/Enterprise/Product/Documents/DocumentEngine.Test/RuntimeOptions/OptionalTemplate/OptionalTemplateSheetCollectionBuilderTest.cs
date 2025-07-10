using System.Collections.Generic;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class OptionalTemplateSheetCollectionBuilderTest : TempFileTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildNoErrors()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("MultipleTemplatesWithOptionalTemplates.xls");
			AssertEquals("No errors should return collection", 2, tscb.OptionalTemplateSheets.Count);
			AssertEquals("No errors should return no errors", 0, tscb.Errors.Count);
			AssertEquals("Collection Should contain sheet1", true, tscb.OptionalTemplateSheets.Contains("Sheet1"));
			AssertEquals("Collection Should contain sheet3", true, tscb.OptionalTemplateSheets.Contains("Sheet3"));
			AssertEquals("Collection Should not contain sheet2", false, tscb.OptionalTemplateSheets.Contains("Sheet2"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildErrorsNonRenderableSheetAsOption()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("MultipleTemplatesWithOptionalTemplatesThatArentRenderable.xls");
			AssertEquals("There are errors", 1, tscb.Errors.Count);
			AssertEquals("Contains GroupBys is not renederable errors", true, ContainsError("GroupBys is not a renderable template sheet.", tscb.Errors));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildErrorsOptionalTemplateSpecifiedMoreThanOnce()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("MultipleTemplatesWithOptionalTemplatesMorethanOnce.xls");
			AssertEquals("There are errors", 1, tscb.Errors.Count);
			AssertEquals("There should be an error", true, ContainsError("Sheet1 is specified on the optional templates sheet more than once.", tscb.Errors));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildErrorsNonExistentSheetAsOption()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("MultipleTemplatesWithOptionalTemplatesThatArentThere.xls");
			AssertEquals("There are errors", 1, tscb.Errors.Count);
			AssertEquals("There should be an error", true, ContainsError("Dave's not here is not a renderable template sheet.", tscb.Errors));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildErrorsSingleSheetDoesntErrorWhenNoOptions()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("SingleTemplateWithNoOptionalTemplates.xls");
			AssertEquals("There are not errors", 0, tscb.Errors.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBuildErrorsOnlyOneTemplate()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("SingleTemplateWithOptionalTemplates.xls");
			AssertEquals("There are errors", 1, tscb.Errors.Count);
			AssertEquals("There should be an error", true, ContainsError("This report only contains 1 template sheet. Can't specify optional templates if there's only 1 template sheet.", tscb.Errors));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHasErrors()
		{
			OptionalTemplateSheetCollectionBuilder tscb = GetBuilderForReport("SingleTemplateWithOptionalTemplates.xls");
			AssertEquals("There are errors so has errors should work and optionaltemplatecollection count will be 0", 0, tscb.OptionalTemplateSheets.Count);
		}

		OptionalTemplateSheetCollectionBuilder GetBuilderForReport(string reportName)
		{
			OptionalTemplateSheetCollectionBuilder result = null;
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting(reportName, TestFilesSubFolder.ReportTestFiles);
			using (Report report = new Report(new DocumentPack(), excelTemplate))
			{
				result = new OptionalTemplateSheetCollectionBuilder(report.OptionalTemplatesSheet, report.TemplateSheets, new ValidatorPack(), true);
				result.Build();
			}
			return result;
		}

		bool ContainsError(string expectedErrorMessage, List<IReportProcessingError> errors)
		{
			bool result = false;
			foreach (ReportProcessingError ex in errors)
			{
				if (ex.Message.Contains(expectedErrorMessage))
				{
					result = true;
				}
			}
			return result;
		}
	}
}

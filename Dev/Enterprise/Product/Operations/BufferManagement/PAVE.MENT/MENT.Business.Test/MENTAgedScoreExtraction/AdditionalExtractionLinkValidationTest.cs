using CargoWise.EntityFramework.Testing;

namespace Enterprise.PAVE.MENT.Business.Test
{
	public class AdditionalExtractionLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAdditionalExtractionHasTheSameCategoryColumn()
		{
			var baseExtraction = MENTTestHelper.CreateExtraction(Factory, "Hardunkichud");
			var anotherExtraction = MENTTestHelper.CreateExtraction(Factory, "WashingBeard", query: baseExtraction.RelatedQuery);

			var column = MENTTestHelper.GetSqlColumnFromCollection(baseExtraction.CategoryColumns, MENTColumns.Codes.Score, true);
			column.Sequence = 0;

			var anotherColumn = MENTTestHelper.GetSqlColumnFromCollection(anotherExtraction.CategoryColumns, MENTColumns.Codes.AttributeValue, true);

			var additionalExtractionLink = MENTTestHelper.CreateAdditionalExtractionLink(baseExtraction, baseExtraction.RelatedQuery, anotherExtraction);

			additionalExtractionLink.Validation.ValidateExtractionPK();

			AssertHasError(additionalExtractionLink.ExtractionPKInfo, "Category Columns must be the same across additional extractions");

			additionalExtractionLink = MENTTestHelper.CreateAdditionalExtractionLink(baseExtraction, baseExtraction.RelatedQuery, baseExtraction);

			AssertNoErrors(additionalExtractionLink.ExtractionPKInfo);

			anotherColumn.Selected = false;
			anotherColumn = MENTTestHelper.GetSqlColumnFromCollection(anotherExtraction.CategoryColumns, MENTColumns.Codes.Score, true);
			anotherColumn.Sequence = 0;
			additionalExtractionLink = MENTTestHelper.CreateAdditionalExtractionLink(baseExtraction, baseExtraction.RelatedQuery, anotherExtraction);

			AssertNoErrors(additionalExtractionLink.ExtractionPKInfo);
		}
	}
}

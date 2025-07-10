using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.PAVE.MENT.Business.Test
{
	internal class MENTAgedScoreExtractionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUniqueNameOnExtraction()
		{
			var query = MENTTestHelper.CreateQuery(Factory, "ABC123CODY");

			var extraction1 = MENTTestHelper.CreateExtraction(Factory, "ExtractionForMe", query);
			var extraction2 = MENTTestHelper.CreateExtraction(Factory, "ExtractionForPeeps", query);

			extraction2.MEX_Name = "ExtractionForMe";

			AssertHasError(extraction2.MEX_NameInfo, "The Extraction Name has been duplicated and must be unique.");

			extraction2.MEX_Name = "Dolphins do backflips on airplanes while using slippers";

			AssertNoErrors(extraction2.MEX_NameInfo);
		}

		public void TestSumOnColumns()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "TOOLTIME");

			extraction.AggregationType = ExtractionTypes.Codes.Sum;
			extraction.CollectionColumn = MENTColumns.Codes.AttributeValue;

			extraction.Validation.ValidateAggregationType();

			AssertHasError(extraction.AggregationTypeInfo, "Attribute Value is a text based column, Aggregation Types which are Numeric function can only be used on defined numeric columns.");

			extraction.CollectionColumn = MENTColumns.Codes.Score;
			extraction.Validation.ValidateAggregationType();
			extraction.Validation.ValidateCollectionColumn();

			AssertNoErrors(extraction.AggregationTypeInfo);
		}

		public void TestAggregationTypeColumns()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "TOOLTIME");

			extraction.AggregationType = ExtractionTypes.Codes.Sum;
			extraction.CollectionColumn = MENTColumns.Codes.AttributeValue;

			var expectedError = "Attribute Value is a text based column, Aggregation Types which are Numeric function can only be used on defined numeric columns.";

			extraction.Validation.ValidateAggregationType();
			AssertHasError(extraction.AggregationTypeInfo, expectedError);

			extraction.AggregationType = ExtractionTypes.Codes.Maximum;

			extraction.Validation.ValidateAggregationType();
			AssertHasError(extraction.AggregationTypeInfo, expectedError);

			extraction.AggregationType = ExtractionTypes.Codes.Minimum;

			extraction.Validation.ValidateAggregationType();
			AssertHasError(extraction.AggregationTypeInfo, expectedError);

			extraction.AggregationType = ExtractionTypes.Codes.Average;

			extraction.Validation.ValidateAggregationType();
			AssertHasError(extraction.AggregationTypeInfo, expectedError);

			extraction.AggregationType = ExtractionTypes.Codes.Count;
			extraction.Validation.ValidateAggregationType();

			AssertNoErrors(extraction.AggregationTypeInfo);
		}

		public void TestIsInstantaneous()
		{
			var extraction = MENTTestHelper.CreateExtraction(Factory, "Hardunkichud");

			extraction.IsInstantaneous = false;
			extraction.AggregationType = ExtractionTypes.Codes.None;
			AssertHasError(extraction.AggregationTypeInfo, "Non-Instantaneous Extractions must specify an Aggregation Type other than NON that can be used when more than one result is returned for a grouping.");
			extraction.CollectionColumn = MENTColumns.Codes.None;
			AssertHasError(extraction.CollectionColumnInfo, "Only Instantaneous Extractions can use NON Collection Columns.");

			extraction.AggregationType = ExtractionTypes.Codes.Sum;
			extraction.CollectionColumn = MENTColumns.Codes.None;
			extraction.IsInstantaneous = true;

			var types = new ExtractionTypes().ToArray().Select(t => t.Code).Where(t => t != ExtractionTypes.Codes.None);

			foreach (var type in types)
			{
				AssertAggregationType(extraction, type);
			}

			extraction.AggregationType = ExtractionTypes.Codes.None;
			AssertNoError(extraction.IsInstantaneousInfo, "Instantaneous Extractions require NON Aggregation and NON Collection Columns. No Aggregation is required on the rows returned.");

			extraction.CollectionColumn = MENTColumns.Codes.AttributeValue;
			AssertHasError(extraction.IsInstantaneousInfo, "Instantaneous Extractions require NON Aggregation and NON Collection Columns. No Aggregation is required on the rows returned.");

			extraction.CollectionColumn = MENTColumns.Codes.None;
			AssertNoError(extraction.IsInstantaneousInfo, "Instantaneous Extractions require NON Aggregation and NON Collection Columns. No Aggregation is required on the rows returned.");
		}

		void AssertAggregationType(MENTAgedScoreExtraction extraction, string type)
		{
			extraction.AggregationType = type;
			AssertHasError(type, extraction.IsInstantaneousInfo, "Instantaneous Extractions require NON Aggregation and NON Collection Columns. No Aggregation is required on the rows returned.");
		}
	}
}

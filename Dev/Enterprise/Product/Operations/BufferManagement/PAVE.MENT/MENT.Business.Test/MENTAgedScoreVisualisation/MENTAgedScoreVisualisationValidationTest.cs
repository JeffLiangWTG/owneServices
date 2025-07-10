using CargoWise.EntityFramework.Testing;

namespace Enterprise.PAVE.MENT.Business.Test
{
	internal class MENTAgedScoreVisualisationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUpperBoundLowerBoundIntersection()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Whalerus");

			var column1 = new VisualisationColumnSpecification(visualisation) { Sequence = 1 };
			var column2 = new VisualisationColumnSpecification(visualisation) { Sequence = 2 };
			var column3 = new VisualisationColumnSpecification(visualisation) { Sequence = 3 };
			var column4 = new VisualisationColumnSpecification(visualisation) { Sequence = 4 };
			var column5 = new VisualisationColumnSpecification(visualisation) { Sequence = 5 };

			visualisation.CategorySequenceCollection.Add(column1);
			visualisation.CategorySequenceCollection.Add(column2);
			visualisation.CategorySequenceCollection.Add(column3);
			visualisation.CategorySequenceCollection.Add(column4);
			visualisation.CategorySequenceCollection.Add(column5);

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.PerformUpperBoundAggregation = true;

			visualisation.LowerBoundAggregationSequence = 4;
			visualisation.UpperBoundAggregationSequence = 2;

			AssertHasError(visualisation.UpperBoundAggregationSequenceInfo, "Upper and Lower Bounds must not intersect.");
			AssertHasError(visualisation.LowerBoundAggregationSequenceInfo, "Upper and Lower Bounds must not intersect.");

			visualisation.LowerBoundAggregationSequence = 2;
			visualisation.UpperBoundAggregationSequence = 4;

			AssertNoErrors(visualisation.UpperBoundAggregationSequenceInfo);
			AssertNoErrors(visualisation.LowerBoundAggregationSequenceInfo);
		}

		public void TestUpperAndLowerAreNotSilly()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Nukacola");

			var column1 = new VisualisationColumnSpecification(visualisation) { Sequence = 1 };
			var column2 = new VisualisationColumnSpecification(visualisation) { Sequence = 2 };
			var column3 = new VisualisationColumnSpecification(visualisation) { Sequence = 3 };

			visualisation.CategorySequenceCollection.Add(column1);
			visualisation.CategorySequenceCollection.Add(column2);
			visualisation.CategorySequenceCollection.Add(column3);

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 3;

			AssertHasWarning(visualisation.LowerBoundAggregationSequenceInfo, "Lower Bound is the highest sequence. There will be only one column.");

			visualisation.LowerBoundAggregationSequence = 2;

			AssertNoWarnings(visualisation.LowerBoundAggregationSequenceInfo);

			visualisation.PerformLowerBoundAggregation = false;
			visualisation.PerformUpperBoundAggregation = true;

			visualisation.UpperBoundAggregationSequence = 1;

			AssertHasWarning(visualisation.UpperBoundAggregationSequenceInfo, "Upper Bound is the lowest sequence. There will be only one column.");

			visualisation.UpperBoundAggregationSequence = 2;

			AssertNoWarnings(visualisation.UpperBoundAggregationSequenceInfo);
		}

		public void TestUpperBoundAggregationValidation()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Lady Spatula");
			AssertEquals(0, visualisation.UpperBoundAggregationSequence);

			var column1 = new VisualisationColumnSpecification(visualisation) { Sequence = 1 };
			var column2 = new VisualisationColumnSpecification(visualisation) { Sequence = 2 };
			var column3 = new VisualisationColumnSpecification(visualisation) { Sequence = 3 };

			visualisation.CategorySequenceCollection.Add(column1);
			visualisation.CategorySequenceCollection.Add(column2);
			visualisation.CategorySequenceCollection.Add(column3);

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 4;

			AssertHasError(visualisation.UpperBoundAggregationSequenceInfo, "Upper Bound column doesn't exist.");

			visualisation.PerformUpperBoundAggregation = false;

			AssertNoErrors(visualisation.UpperBoundAggregationSequenceInfo);

			visualisation.PerformUpperBoundAggregation = true;
			visualisation.UpperBoundAggregationSequence = 3;

			AssertNoErrors(visualisation.UpperBoundAggregationSequenceInfo);

			visualisation.UpperBoundAggregationSequence = 4;
			column1.Sequence = 4;

			visualisation.Validation.ValidateUpperBoundAggregationSequence();

			AssertNoErrors(visualisation.UpperBoundAggregationSequenceInfo);
		}

		public void TestLowerBoundAggregationValidation()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Diver Dan");
			AssertEquals(0, visualisation.LowerBoundAggregationSequence);

			var column1 = new VisualisationColumnSpecification(visualisation) { Sequence = 1 };
			var column2 = new VisualisationColumnSpecification(visualisation) { Sequence = 2 };
			var column3 = new VisualisationColumnSpecification(visualisation) { Sequence = 3 };

			visualisation.CategorySequenceCollection.Add(column1);
			visualisation.CategorySequenceCollection.Add(column2);
			visualisation.CategorySequenceCollection.Add(column3);

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 0;

			AssertHasError(visualisation.LowerBoundAggregationSequenceInfo, "Lower Bound column doesn't exist.");

			visualisation.PerformLowerBoundAggregation = false;

			AssertNoErrors(visualisation.LowerBoundAggregationSequenceInfo);

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.LowerBoundAggregationSequence = 1;

			AssertNoErrors(visualisation.LowerBoundAggregationSequenceInfo);

			visualisation.LowerBoundAggregationSequence = 0;
			column1.Sequence = 0;

			visualisation.Validation.ValidateLowerBoundAggregationSequence();

			AssertNoErrors(visualisation.LowerBoundAggregationSequenceInfo);
		}

		public void TestAggregationValidation()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "Lawrence of Arabia");

			var column1 = new VisualisationColumnSpecification(visualisation) { Sequence = 1 };
			var column2 = new VisualisationColumnSpecification(visualisation) { Sequence = 2 };
			var column3 = new VisualisationColumnSpecification(visualisation) { Sequence = 3 };

			visualisation.CategorySequenceCollection.Add(column1);
			visualisation.CategorySequenceCollection.Add(column2);
			visualisation.CategorySequenceCollection.Add(column3);

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.PerformUpperBoundAggregation = true;

			visualisation.LowerBoundAggregationSequence = 1;
			visualisation.UpperBoundAggregationSequence = 1;

			AssertHasError(visualisation.LowerBoundAggregationSequenceInfo, "Aggregation Sequence Bounds must be different.");
			AssertHasError(visualisation.UpperBoundAggregationSequenceInfo, "Aggregation Sequence Bounds must be different.");

			visualisation.UpperBoundAggregationSequence = 2;

			AssertNoErrors(visualisation.LowerBoundAggregationSequenceInfo);
			AssertNoErrors(visualisation.UpperBoundAggregationSequenceInfo);
		}

		public void TestMinAndMaxZeroElements()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "I dream of a genie");

			visualisation.PerformLowerBoundAggregation = true;
			visualisation.PerformUpperBoundAggregation = true;

			visualisation.LowerBoundAggregationSequence = 1;
			visualisation.UpperBoundAggregationSequence = 1;

			AssertHasError(visualisation.LowerBoundAggregationSequenceInfo, "Aggregation Sequence Bounds must be different.");
			AssertHasError(visualisation.UpperBoundAggregationSequenceInfo, "Aggregation Sequence Bounds must be different.");
		}

		public void TestGraphType()
		{
			var visualisation = MENTTestHelper.CreateVisualisation(Factory, "I dream of a dog");

			visualisation.MVI_GraphType = GraphTypes.Codes.Column;

			AssertNoErrors(visualisation.MVI_GraphTypeInfo);

			visualisation.MVI_GraphType = "XXX";

			AssertHasErrors(visualisation.MVI_GraphTypeInfo);
		}
	}
}

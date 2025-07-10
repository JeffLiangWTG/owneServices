using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreVisualisationValidation : AutoMENTAgedScoreVisualisationValidation
	{
		public MENTAgedScoreVisualisationValidation(AutoMENTAgedScoreVisualisation parent)
			: base(parent)
		{
		}

		public new MENTAgedScoreVisualisation Parent
		{
			get { return (MENTAgedScoreVisualisation)base.Parent; }
		}

		public override void ValidateAll()
		{
			ValidateUpperBoundAggregationSequence();
			ValidateLowerBoundAggregationSequence();
			base.ValidateAll();
		}

		protected override void CheckMVI_GraphType()
		{
			base.CheckMVI_GraphType();

			ListValidation.ErrorIfInvalidCode(Parent.MVI_GraphTypeInfo);
			MandatoryValidation.CheckEntered(Parent.MVI_GraphTypeInfo);
		}

		public void ValidateLowerBoundAggregationSequence()
		{
			ValidateCalculatedProperty(Parent.LowerBoundAggregationSequenceInfo);
		}

		protected void CheckLowerBoundAggregationSequence()
		{
			CheckDifferentAggregationBounds(Parent.LowerBoundAggregationSequenceInfo);

			var sequences = Parent.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(c => c.Sequence);

			if (Parent.PerformLowerBoundAggregation)
			{
				if (sequences.All(s => s != Parent.LowerBoundAggregationSequence))
				{
					Parent.LowerBoundAggregationSequenceInfo.AddError(Res.GetString("85ecd92e-8dd0-44bb-a808-ee87073c704b", "Lower Bound column doesn't exist."));
				}

				if (sequences.Any() && sequences.Max() == Parent.LowerBoundAggregationSequence)
				{
					Parent.LowerBoundAggregationSequenceInfo.AddWarning(Res.GetString("27cee7d7-39a7-475e-a310-b5cc7a12c8c1", "Lower Bound is the highest sequence. There will be only one column."));
				}
			}
		}

		public void ValidateUpperBoundAggregationSequence()
		{
			ValidateCalculatedProperty(Parent.UpperBoundAggregationSequenceInfo);
		}

		protected void CheckUpperBoundAggregationSequence()
		{
			CheckDifferentAggregationBounds(Parent.UpperBoundAggregationSequenceInfo);

			var sequences = Parent.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Select(c => c.Sequence);

			if (Parent.PerformUpperBoundAggregation)
			{
				if (sequences.All(s => s != Parent.UpperBoundAggregationSequence))
				{
					Parent.UpperBoundAggregationSequenceInfo.AddError(Res.GetString("4d1b5025-d06c-420d-95b4-a9027ed60f6e", "Upper Bound column doesn't exist."));
				}

				if (sequences.Any() && sequences.Min() == Parent.UpperBoundAggregationSequence)
				{
					Parent.UpperBoundAggregationSequenceInfo.AddWarning(Res.GetString("1dea37a4-d0a7-489c-ba5a-5a18e4da9bf4", "Upper Bound is the lowest sequence. There will be only one column."));
				}
			}
		}

		void CheckDifferentAggregationBounds(ZPropertyInfo propertyInfo)
		{
			if (Parent.PerformLowerBoundAggregation && Parent.PerformUpperBoundAggregation)
			{
				if (Parent.LowerBoundAggregationSequence == Parent.UpperBoundAggregationSequence)
				{
					propertyInfo.AddError(Res.GetString("6253bf4f-2bff-4a3a-bb3a-6032cf828985", "Aggregation Sequence Bounds must be different."));
				}
				else if (Parent.LowerBoundAggregationSequence > Parent.UpperBoundAggregationSequence)
				{
					propertyInfo.AddError(Res.GetString("4fa42669-f5db-4e91-8417-5204e98d49f3", "Upper and Lower Bounds must not intersect."));
				}
			}
		}
	}
}

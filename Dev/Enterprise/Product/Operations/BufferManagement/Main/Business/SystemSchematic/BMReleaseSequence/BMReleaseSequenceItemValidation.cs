//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBMReleaseSequenceItemValidation
//
//    This class should be used for overriding validation in AutoBMReleaseSequenceItemValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMReleaseSequenceItemValidation : AutoBMReleaseSequenceItemValidation
	{
		readonly decimal[] ValidRange = { 1, 2, 3, 5, 8, 15, 25, 40, 75, 100, 250, 600, 1000 };

		public BMReleaseSequenceItemValidation(AutoBMReleaseSequenceItem parent) : base(parent)
		{
		}

		new BMReleaseSequenceItem Parent
		{
			get { return (BMReleaseSequenceItem)base.Parent; }
		}

		protected override void CheckBMI_BMR_Sequence()
		{
			base.CheckBMI_BMR_Sequence();

			if (Parent.Sequence == null)
			{
				Parent.BMI_BMR_SequenceInfo.AddError(Res.GetString("596F4A80-C1BF-42B1-AF53-1BD25860B174", "Sequence item must belong to a valid sequence."));
			}
		}

		protected override void CheckBMI_FH_ProcessHeader()
		{
			base.CheckBMI_FH_ProcessHeader();

			if (Parent.LinkedWorkflow == null)
			{
				Parent.BMI_FH_ProcessHeaderInfo.AddError(Res.GetString("2797CA1F-72A9-44DE-9E5B-2E50718FA587", "Sequence item must reference a valid process header."));
			}
		}

		protected override void CheckBMI_Position()
		{
			base.CheckBMI_Position();

			if (Parent.Sequence == null)
			{
				return;
			}

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.BMI_PositionInfo, Parent.Sequence.Items);
		}

		protected override void CheckBMI_Value()
		{
			base.CheckBMI_Value();
			CompareValidation.CheckWithinSet(Parent.BMI_ValueInfo, ValidRange);
		}

		protected override void CheckBMI_Investment()
		{
			base.CheckBMI_Investment();
			CompareValidation.CheckWithinSet(Parent.BMI_InvestmentInfo, ValidRange);
		}

		protected override void CheckBMI_EstimatedLeadTimeDays()
		{
			base.CheckBMI_EstimatedLeadTimeDays();
			CompareValidation.CheckWithinRange(Parent.BMI_EstimatedLeadTimeDaysInfo, 0, 365);
		}
	}
}

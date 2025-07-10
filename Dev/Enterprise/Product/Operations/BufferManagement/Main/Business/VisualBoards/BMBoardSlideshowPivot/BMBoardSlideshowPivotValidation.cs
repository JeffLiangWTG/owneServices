using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardSlideshowPivotValidation : AutoBMBoardSlideshowPivotValidation
	{
		public BMBoardSlideshowPivotValidation(AutoBMBoardSlideshowPivot parent) : base(parent)
		{
		}

		protected override void CheckMC_DurationInSeconds()
		{
			base.CheckMC_DurationInSeconds();
			MandatoryValidation.CheckEntered(Parent.MC_DurationInSecondsInfo);
			MandatoryValidation.CheckNotNegative(Parent.MC_SequenceInfo);
		}

		protected override void CheckMC_Sequence()
		{
			base.CheckMC_Sequence();
			MandatoryValidation.CheckEntered(Parent.MC_SequenceInfo);
			MandatoryValidation.CheckNotNegative(Parent.MC_SequenceInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.MC_SequenceInfo);
		}
	}
}

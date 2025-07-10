//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBMReleaseSequenceValidation
//
//    This class should be used for overriding validation in AutoBMReleaseSequenceValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMReleaseSequenceValidation : AutoBMReleaseSequenceValidation
	{
		public BMReleaseSequenceValidation(AutoBMReleaseSequence parent) : base(parent)
		{
		}

		protected override void CheckBMR_GG_ReleaseGroup()
		{
			base.CheckBMR_GG_ReleaseGroup();
			ListValidation.ErrorIfInvalidPK(Parent.BMR_GG_ReleaseGroupInfo);
		}

		protected override void CheckBMR_G4_Capability()
		{
			base.CheckBMR_G4_Capability();
			ListValidation.ErrorIfInvalidPK(Parent.BMR_G4_CapabilityInfo);
		}

		protected override void CheckBMR_Name()
		{
			base.CheckBMR_Name();
			MandatoryValidation.CheckEntered(Parent.BMR_NameInfo);
		}

		protected override void CheckBMR_SequenceNudge()
		{
			base.CheckBMR_SequenceNudge();
			CompareValidation.CheckWithinRange(Parent.BMR_SequenceNudgeInfo, -36000, 36000);
		}
	}
}

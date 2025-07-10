//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceRulePeriodSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceRulePeriodSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodSnapshotValidation : AutoCMRPreferenceRulePeriodSnapshotValidation
	{
		public CMRPreferenceRulePeriodSnapshotValidation(AutoCMRPreferenceRulePeriodSnapshot parent)
			: base(parent)
		{
		}
	}
}

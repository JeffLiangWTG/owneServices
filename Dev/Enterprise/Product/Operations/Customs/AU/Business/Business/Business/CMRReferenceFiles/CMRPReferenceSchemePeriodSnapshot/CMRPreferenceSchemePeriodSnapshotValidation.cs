//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRPreferenceSchemePeriodSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRPreferenceSchemePeriodSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodSnapshotValidation : AutoCMRPreferenceSchemePeriodSnapshotValidation
	{
		public CMRPreferenceSchemePeriodSnapshotValidation(AutoCMRPreferenceSchemePeriodSnapshot parent)
			: base(parent)
		{
		}
	}
}

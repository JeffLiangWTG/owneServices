//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentRatePeriodSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentRatePeriodSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodSnapshotValidation : AutoCMRTreatmentRatePeriodSnapshotValidation
	{
		public CMRTreatmentRatePeriodSnapshotValidation(AutoCMRTreatmentRatePeriodSnapshot parent)
			: base(parent)
		{
		}
	}
}

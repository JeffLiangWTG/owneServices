//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotValidation : AutoCMRTreatmentSnapshotValidation
	{
		public CMRTreatmentSnapshotValidation(AutoCMRTreatmentSnapshot parent)
			: base(parent)
		{
		}
	}
}

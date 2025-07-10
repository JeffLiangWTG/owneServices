//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRStatisticalClassificationPeriodSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRStatisticalClassificationPeriodSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodSnapshotValidation : AutoCMRStatisticalClassificationPeriodSnapshotValidation
	{
		public CMRStatisticalClassificationPeriodSnapshotValidation(AutoCMRStatisticalClassificationPeriodSnapshot parent)
			: base(parent)
		{
		}
	}
}

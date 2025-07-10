//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffRatePeriodSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRTariffRatePeriodSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodSnapshotValidation : AutoCMRTariffRatePeriodSnapshotValidation
	{
		public CMRTariffRatePeriodSnapshotValidation(AutoCMRTariffRatePeriodSnapshot parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentSnapshotTariffGroupValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentSnapshotTariffGroupValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotTariffGroupValidation : AutoCMRTreatmentSnapshotTariffGroupValidation
	{
		public CMRTreatmentSnapshotTariffGroupValidation(AutoCMRTreatmentSnapshotTariffGroup parent)
			: base(parent)
		{
		}
	}
}

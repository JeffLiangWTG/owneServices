//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTreatmentSnapshotCountryValidation
//
//    This class should be used for overriding validation in AutoCMRTreatmentSnapshotCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotCountryValidation : AutoCMRTreatmentSnapshotCountryValidation
	{
		public CMRTreatmentSnapshotCountryValidation(AutoCMRTreatmentSnapshotCountry parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRTariffClassificationSnapshotValidation
//
//    This class should be used for overriding validation in AutoCMRTariffClassificationSnapshotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationSnapshotValidation : AutoCMRTariffClassificationSnapshotValidation
	{
		public CMRTariffClassificationSnapshotValidation(AutoCMRTariffClassificationSnapshot parent)
			: base(parent)
		{
		}
	}
}

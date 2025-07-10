//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRSeaImpendingArrivalsValidation
//
//    This class should be used for overriding validation in AutoCMRSeaImpendingArrivalsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSeaImpendingArrivalsValidation : AutoCMRSeaImpendingArrivalsValidation
	{
		public CMRSeaImpendingArrivalsValidation(AutoCMRSeaImpendingArrivals parent) : base(parent)
		{
		}
	}
}

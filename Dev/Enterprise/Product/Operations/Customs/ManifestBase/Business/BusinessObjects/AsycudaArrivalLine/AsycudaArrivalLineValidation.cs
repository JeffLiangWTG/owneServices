//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaArrivalLineValidation
//
//    This class should be used for overriding validation in AutoAsycudaArrivalLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaArrivalLineValidation : AutoAsycudaArrivalLineValidation
	{
		public AsycudaArrivalLineValidation(AutoAsycudaArrivalLine parent) : base(parent)
		{
		}
	}
}


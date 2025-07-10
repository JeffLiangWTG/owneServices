//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaArrivalHeaderValidation
//
//    This class should be used for overriding validation in AutoAsycudaArrivalHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaArrivalHeaderValidation : AutoAsycudaArrivalHeaderValidation
	{
		public AsycudaArrivalHeaderValidation(AutoAsycudaArrivalHeader parent) : base(parent)
		{
		}
	}
}


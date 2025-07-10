//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAsycudaBillScreeningValidation
//
//    This class should be used for overriding validation in AutoAsycudaBillScreeningValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillScreeningValidation : AutoAsycudaBillScreeningValidation
	{
		public AsycudaBillScreeningValidation(AutoAsycudaBillScreening parent)
			: base(parent)
		{
		}
	}
}

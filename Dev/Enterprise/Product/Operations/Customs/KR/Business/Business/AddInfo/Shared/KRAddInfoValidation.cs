//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoKRAddInfoValidation
//
//    This class should be used for overriding validation in AutoKRAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class KRAddInfoValidation : AutoKRAddInfoValidation
	{
		public KRAddInfoValidation(AutoKRAddInfo parent) : base(parent)
		{
		}
		public new KRAddInfo Parent => (KRAddInfo)base.Parent;
	}
}

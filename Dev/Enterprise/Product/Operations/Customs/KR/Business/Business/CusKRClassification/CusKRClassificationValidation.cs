//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusKRClassificationValidation
//
//    This class should be used for overriding validation in AutoCusKRClassificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.KR.Business
{
	public class CusKRClassificationValidation : AutoCusKRClassificationValidation
	{
		public CusKRClassificationValidation(AutoCusKRClassification parent) : base(parent)
		{
		}
	}
}

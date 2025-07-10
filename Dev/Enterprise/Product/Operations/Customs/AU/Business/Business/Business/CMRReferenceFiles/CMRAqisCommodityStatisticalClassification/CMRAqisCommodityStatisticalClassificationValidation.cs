//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisCommodityStatisticalClassificationValidation
//
//    This class should be used for overriding validation in AutoCMRAqisCommodityStatisticalClassificationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisCommodityStatisticalClassificationValidation : AutoCMRAqisCommodityStatisticalClassificationValidation
	{
		public CMRAqisCommodityStatisticalClassificationValidation(AutoCMRAqisCommodityStatisticalClassification parent)
			: base(parent)
		{
		}
	}
}

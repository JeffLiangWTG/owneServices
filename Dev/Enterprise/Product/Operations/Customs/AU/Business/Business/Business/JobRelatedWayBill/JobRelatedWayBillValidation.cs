//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobRelatedWayBillValidation
//
//    This class should be used for overriding validation in AutoJobRelatedWayBillValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobRelatedWayBillValidation : AutoJobRelatedWayBillValidation
	{
		public JobRelatedWayBillValidation(AutoJobRelatedWayBill parent)
			: base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingPayableTransactionLineValidation
//
//    This class should be used for overriding validation in AutoNettingPayableTransactionLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionLineValidation : AutoNettingPayableTransactionLineValidation
	{
		public NettingPayableTransactionLineValidation(AutoNettingPayableTransactionLine parent) : base(parent)
		{
		}
	}
}

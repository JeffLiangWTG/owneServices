//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingPayableTransactionValidation
//
//    This class should be used for overriding validation in AutoNettingPayableTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionValidation : AutoNettingPayableTransactionValidation
	{
		public NettingPayableTransactionValidation(AutoNettingPayableTransaction parent) : base(parent)
		{
		}
	}
}

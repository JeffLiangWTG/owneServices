//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingPayableTransactionRefValidation
//
//    This class should be used for overriding validation in AutoNettingPayableTransactionRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingPayableTransactionRefValidation : AutoNettingPayableTransactionRefValidation
	{
		public NettingPayableTransactionRefValidation(AutoNettingPayableTransactionRef parent) : base(parent)
		{
		}
	}
}

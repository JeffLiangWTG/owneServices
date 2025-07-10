//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingReceivableTransactionValidation
//
//    This class should be used for overriding validation in AutoNettingReceivableTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionValidation : AutoNettingReceivableTransactionValidation
	{
		public NettingReceivableTransactionValidation(AutoNettingReceivableTransaction parent) : base(parent)
		{
		}
	}
}

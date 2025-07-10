//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingReceivableTransactionRefValidation
//
//    This class should be used for overriding validation in AutoNettingReceivableTransactionRefValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionRefValidation : AutoNettingReceivableTransactionRefValidation
	{
		public NettingReceivableTransactionRefValidation(AutoNettingReceivableTransactionRef parent) : base(parent)
		{
		}
	}
}

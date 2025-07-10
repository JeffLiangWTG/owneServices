//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNettingReceivableTransactionLineValidation
//
//    This class should be used for overriding validation in AutoNettingReceivableTransactionLineValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Netting
{
	public class NettingReceivableTransactionLineValidation : AutoNettingReceivableTransactionLineValidation
	{
		public NettingReceivableTransactionLineValidation(AutoNettingReceivableTransactionLine parent) : base(parent)
		{
		}
	}
}

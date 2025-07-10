//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenericTransactionValidation
//
//    This class should be used for overriding validation in AutoGenericTransactionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GenericTransaction
{
	public class GenericTransactionValidation : AutoGenericTransactionValidation
	{
		public GenericTransactionValidation(AutoGenericTransaction parent)
			: base(parent)
		{
		}
	}
}


//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientAccTransactionHeaderWithJobInfoValidation
//
//    This class should be used for overriding validation in AutoClientAccTransactionHeaderWithJobInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.JAS.Business
{
	public class ClientAccTransactionHeaderWithJobInfoValidation : AutoClientAccTransactionHeaderWithJobInfoValidation
	{
		public ClientAccTransactionHeaderWithJobInfoValidation(AutoClientAccTransactionHeaderWithJobInfo parent)
			: base(parent)
		{
		}
	}
}

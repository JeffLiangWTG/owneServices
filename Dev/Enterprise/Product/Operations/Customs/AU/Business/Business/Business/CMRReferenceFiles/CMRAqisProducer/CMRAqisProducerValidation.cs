//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCMRAqisProducerValidation
//
//    This class should be used for overriding validation in AutoCMRAqisProducerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisProducerValidation : AutoCMRAqisProducerValidation
	{
		public CMRAqisProducerValidation(AutoCMRAqisProducer parent)
			: base(parent)
		{
		}
	}
}

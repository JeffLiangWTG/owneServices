//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoApplicationLoggerValidation
//
//    This class should be used for overriding validation in AutoApplicationLoggerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	public class ApplicationLoggerValidation : AutoApplicationLoggerValidation
	{
		public ApplicationLoggerValidation(AutoApplicationLogger parent) : base(parent)
		{
		}
	}
}

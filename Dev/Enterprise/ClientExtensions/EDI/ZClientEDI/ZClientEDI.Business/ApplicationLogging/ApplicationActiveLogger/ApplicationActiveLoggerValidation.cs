//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoApplicationActiveLoggerValidation
//
//    This class should be used for overriding validation in AutoApplicationActiveLoggerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	public class ApplicationActiveLoggerValidation : AutoApplicationActiveLoggerValidation
	{
		public ApplicationActiveLoggerValidation(AutoApplicationActiveLogger parent) : base(parent)
		{
		}
	}
}

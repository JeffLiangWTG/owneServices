//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoApplicationActiveLoggerLookups
//
//    This class should be used for overriding collections in AutoApplicationActiveLoggerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	public class ApplicationActiveLoggerLookups : AutoApplicationActiveLoggerLookups
	{
		public ApplicationActiveLoggerLookups(AutoApplicationActiveLogger parent) : base(parent)
		{
		}

		public ApplicationLoggerCollection ApplicationLoggers => new ApplicationLoggerCollection(Factory);

		public LicenceHeaderCollection Licences => new LicenceHeaderCollection(Factory);
	}
}

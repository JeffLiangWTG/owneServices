//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoMailDBItemsLookups
//
//    This class should be used for overriding collections in AutoMailDBItemsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using MailManager;

namespace Enterprise.MailManager.Business
{
	public class MailDBItemsLookups : AutoMailDBItemsLookups
	{
		public MailDBItemsLookups(AutoMailDBItems parent)
			: base(parent)
		{
		}

		public StatusCodeList StatusCodes
		{
			get { return new StatusCodeList(); }
		}

		public DirectionList Directions
		{
			get { return new DirectionList(); }
		}
	}
}

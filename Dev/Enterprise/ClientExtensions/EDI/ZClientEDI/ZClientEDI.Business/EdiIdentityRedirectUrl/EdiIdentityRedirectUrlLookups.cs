//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityRedirectUrlLookups
//
//    This class should be used for overriding collections in AutoEdiIdentityRedirectUrlLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business
{
	public class EdiIdentityRedirectUrlLookups : AutoEdiIdentityRedirectUrlLookups
	{
		public EdiIdentityRedirectUrlLookups(AutoEdiIdentityRedirectUrl parent) : base(parent)
		{
		}

		public EdiIdentityRedirectType RedirectTypes => new EdiIdentityRedirectType();
	}
}

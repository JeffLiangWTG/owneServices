//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLicenceEnterpriseDomainsLookups
//
//    This class should be used for overriding collections in AutoLicenceEnterpriseDomainsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.LicenceEnterpriseDomains.Business
{
	public class LicenceEnterpriseDomainsLookups : AutoLicenceEnterpriseDomainsLookups
	{
		public LicenceEnterpriseDomainsLookups(AutoLicenceEnterpriseDomains parent) : base(parent)
		{
		}
	}
}

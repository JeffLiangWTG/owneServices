//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLicenceEnterpriseDomainsValidation
//
//    This class should be used for overriding validation in AutoLicenceEnterpriseDomainsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.LicenceEnterpriseDomains.Business
{
	public class LicenceEnterpriseDomainsValidation : AutoLicenceEnterpriseDomainsValidation
	{
		public LicenceEnterpriseDomainsValidation(AutoLicenceEnterpriseDomains parent) : base(parent)
		{
		}
	}
}
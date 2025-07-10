//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLicenceEnterpriseValidation
//
//    This class should be used for overriding validation in AutoLicenceEnterpriseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Licensing.Billing.Business
{
	public class LicenceEnterpriseValidation : AutoLicenceEnterpriseValidation
	{
		public LicenceEnterpriseValidation(AutoLicenceEnterprise parent) : base(parent)
		{
		}
	}
}

//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoClientLicenceBillingExcludeOrgValidation
//
//    This class should be used for overriding validation in AutoClientLicenceBillingExcludeOrgValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.EntityFramework;

	public class ClientLicenceBillingExcludeOrgValidation : AutoClientLicenceBillingExcludeOrgValidation
	{
		public ClientLicenceBillingExcludeOrgValidation(AutoClientLicenceBillingExcludeOrg parent) : base(parent)
		{
		}

		protected override void CheckCEX_BillingSystem()
		{
			base.CheckCEX_BillingSystem();
			MandatoryValidation.CheckEntered(Parent.CEX_BillingSystemInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CEX_BillingSystemInfo);
		}

		protected override void CheckCEX_LicenceCode()
		{
			base.CheckCEX_LicenceCode();
			MandatoryValidation.CheckEntered(Parent.CEX_LicenceCodeInfo);
		}
	}
}


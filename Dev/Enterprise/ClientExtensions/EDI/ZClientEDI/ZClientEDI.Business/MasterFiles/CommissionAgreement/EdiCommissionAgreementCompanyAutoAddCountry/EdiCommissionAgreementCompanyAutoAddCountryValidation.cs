//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementCompanyAutoAddCountryValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionAgreementCompanyAutoAddCountryValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddCountryValidation : AutoEdiCommissionAgreementCompanyAutoAddCountryValidation
	{
		public EdiCommissionAgreementCompanyAutoAddCountryValidation(AutoEdiCommissionAgreementCompanyAutoAddCountry parent) : base(parent)
		{
		}
	}
}


//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementCompanyAutoAddDatabaseValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionAgreementCompanyAutoAddDatabaseValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyAutoAddDatabaseValidation : AutoEdiCommissionAgreementCompanyAutoAddDatabaseValidation
	{
		public EdiCommissionAgreementCompanyAutoAddDatabaseValidation(AutoEdiCommissionAgreementCompanyAutoAddDatabase parent) : base(parent)
		{
		}
	}
}


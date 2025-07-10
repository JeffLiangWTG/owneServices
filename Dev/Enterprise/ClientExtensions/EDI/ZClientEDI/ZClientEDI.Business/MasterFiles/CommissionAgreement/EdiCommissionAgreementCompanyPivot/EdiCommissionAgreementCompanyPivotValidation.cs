//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementCompanyPivotValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionAgreementCompanyPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyPivotValidation : AutoEdiCommissionAgreementCompanyPivotValidation
	{
		public EdiCommissionAgreementCompanyPivotValidation(AutoEdiCommissionAgreementCompanyPivot parent) : base(parent)
		{
		}
	}
}


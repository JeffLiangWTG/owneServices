//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementDatabasePivotValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionAgreementDatabasePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementDatabasePivotValidation : AutoEdiCommissionAgreementDatabasePivotValidation
	{
		public EdiCommissionAgreementDatabasePivotValidation(AutoEdiCommissionAgreementDatabasePivot parent) : base(parent)
		{
		}
	}
}


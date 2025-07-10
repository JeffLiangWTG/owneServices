//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionHeaderAdditionalInfoValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionHeaderAdditionalInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EdiCommissionHeaderAdditionalInfoValidation : AutoEdiCommissionHeaderAdditionalInfoValidation
	{
		public EdiCommissionHeaderAdditionalInfoValidation(AutoEdiCommissionHeaderAdditionalInfo parent) : base(parent)
		{
		}
	}
}


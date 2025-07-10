//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCommissionAgreementCustomizationValidation
//
//    This class should be used for overriding validation in AutoEdiCommissionAgreementCustomizationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomizationValidation : AutoEdiCommissionAgreementCustomizationValidation
	{
		public EdiCommissionAgreementCustomizationValidation(AutoEdiCommissionAgreementCustomization parent) : base(parent)
		{
		}
	}
}


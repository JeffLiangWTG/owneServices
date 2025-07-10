//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccEInvoicingTransactionPivotValidation
//
//    This class should be used for overriding validation in AutoAccEInvoicingTransactionPivotValidation
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingTransactionPivotValidation : AutoAccEInvoicingTransactionPivotValidation
	{
		public AccEInvoicingTransactionPivotValidation(AutoAccEInvoicingTransactionPivot parent)
			: base(parent)
		{
		}
		
		protected override void CheckAIP_ActionType()
		{
			base.CheckAIP_ActionType();
			MandatoryValidation.CheckEntered(Parent.AIP_ActionTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AIP_ActionTypeInfo);
		}

		protected override void CheckAIP_Status()
		{
			base.CheckAIP_Status();
			MandatoryValidation.CheckEntered(Parent.AIP_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AIP_StatusInfo);
		}
	}
}

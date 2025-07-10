//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxRecordTransactionLinePivotValidation
//
//    This class should be used for overriding validation in AutoAccTaxRecordTransactionLinePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxRecordTransactionLinePivotValidation : AutoAccTaxRecordTransactionLinePivotValidation
	{
		public AccTaxRecordTransactionLinePivotValidation(AutoAccTaxRecordTransactionLinePivot parent) : base(parent)
		{
		}
	}
}

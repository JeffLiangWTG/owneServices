//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxRecordTransactionLinePivotLookups
//
//    This class should be used for overriding collections in AutoAccTaxRecordTransactionLinePivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxRecordTransactionLinePivotLookups : AutoAccTaxRecordTransactionLinePivotLookups
	{
		public AccTaxRecordTransactionLinePivotLookups(AutoAccTaxRecordTransactionLinePivot parent) : base(parent)
		{
		}
	}
}

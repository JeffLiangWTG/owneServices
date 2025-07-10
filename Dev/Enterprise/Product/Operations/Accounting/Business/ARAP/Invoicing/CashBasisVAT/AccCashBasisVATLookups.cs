//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCashBasisVATLookups
//
//    This class should be used for overriding collections in AutoAccCashBasisVATLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccCashBasisVATLookups : AutoAccCashBasisVATLookups
	{
		public AccCashBasisVATLookups(AutoAccCashBasisVAT parent) : base(parent)
		{
		}
	}
}

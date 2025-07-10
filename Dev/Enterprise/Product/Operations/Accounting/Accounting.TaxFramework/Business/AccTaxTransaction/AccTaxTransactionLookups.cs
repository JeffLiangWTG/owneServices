//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTaxTransactionLookups
//
//    This class should be used for overriding collections in AutoAccTaxTransactionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransactionLookups : AutoAccTaxTransactionLookups
	{
		public AccTaxTransactionLookups(AutoAccTaxTransaction parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TaxBasisList => new AccountingMasterFilesTaxFrameworkConstants.TaxBasisList();
	}
}

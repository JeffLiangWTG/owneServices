//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleSetLookups
//
//    This class should be used for overriding collections in AutoBarcodeRuleSetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleSetLookups : AutoBarcodeRuleSetLookups
	{
		public BarcodeRuleSetLookups(AutoBarcodeRuleSet parent)
			: base(parent)
		{
		}

		new BarcodeRuleSet Parent => (BarcodeRuleSet)base.Parent;

		#region Properties

		public override OrgHeaderCollection Buyers => BarcodeParsingLookupHelper.GetBuyers(Factory, Parent.BRS_Module);

		public override OrgHeaderCollection Suppliers => BarcodeParsingLookupHelper.GetSuppliers(Factory, Parent.BRS_Module);

		public BarcodeModuleTypes ModuleTypes => BarcodeParsingLookupHelper.ModuleTypes(Factory);

		public IBusinessObjectCollection RelatedEntityList => BarcodeParsingLookupHelper.RelatedEntityList(Factory, Parent.BRS_Module, Parent.Buyer, Parent.Supplier);

		#endregion
	}
}

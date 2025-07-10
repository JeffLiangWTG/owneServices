//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceUsageMappingLookups
//
//    This class should be used for overriding collections in AutoEdiPriceUsageMappingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceUsageMappingLookups : AutoEdiPriceUsageMappingLookups
	{
		public EdiPriceUsageMappingLookups(AutoEdiPriceUsageMapping parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PriceCategories
			=> ((EdiPriceUsageMapping)Parent).PriceHeader.Lookups.CachedPriceCategories;

		public ReadOnlyCodeDescriptionPairList AllCategories
			=> EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value;

		public CodeDescriptionPairList PriceCodesForCategory
		{
			get
			{
				var parent = (EdiPriceUsageMapping)Parent;
				return parent.PriceHeader.Lookups.CachedPriceCodesForCategory(parent.PUM_PriceCategory);
			}
		}
	}
}


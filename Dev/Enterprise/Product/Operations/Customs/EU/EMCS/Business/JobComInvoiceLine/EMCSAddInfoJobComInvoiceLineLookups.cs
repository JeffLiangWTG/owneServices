using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobComInvoiceLineLookups : EUEMCSAddInfoLookups
	{
		public EMCSAddInfoJobComInvoiceLineLookups(EMCSAddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new EMCSAddInfoJobComInvoiceLine Parent => (EMCSAddInfoJobComInvoiceLine)base.Parent;

		public CodeDescriptionPairList ExciseProductCodes
		{
			get
			{
				var dataGroupingCode = Parent.Parent.GetDefaultDataGroupingCode();
				return Factory.GetCachedValue("EMCSAddInfoJobComInvoiceLineLookups_ExciseProductCodes_" + dataGroupingCode, () =>
				{
					var result = new CodeDescriptionPairList();

					result.AddRange(Factory.GetExciseProductCodes(dataGroupingCode));
					result.Sort();

					return result;
				});
			}
		}

		public CodeDescriptionPairList WineCategoryList => Factory.GetCachedValue<EMCSWineCategoryList>();

		public CodeDescriptionPairList GrowingZoneList => Factory.GetCachedValue<EMCSGrowingZoneList>();

		public RefCountryCollection CountryOfOrigins => Factory.GetCachedValue("EMCSAddInfoJobComInvoiceLineLookups_CountryOfOrigins", () => new RefCountryCollection(Factory));
	}
}

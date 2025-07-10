//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderLinkLookups
//
//    This class should be used for overriding collections in AutoEdiPriceHeaderLinkLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderLinkLookups : AutoEdiPriceHeaderLinkLookups
	{
		public EdiPriceHeaderLinkLookups(AutoEdiPriceHeaderLink parent) : base(parent)
		{
		}

		public Dictionary<string, ClientLicencePriceHeader> PriceHeaderVersionMap
		{
			get
			{
				return GetPriceHeaderVersionMap(Factory);
			}
		}

		public static Dictionary<string, ClientLicencePriceHeader> GetPriceHeaderVersionMap(BusinessObjectFactory factory)
		{
			var stdPrices = LicenceCompany.GetStandardPriceHeaders(factory);

			if (stdPrices != null)
			{
				return stdPrices.Where(x =>
				x.L6_SystemCode != BillingConstants.PriceHeaderType.ODM &&
				x.L6_SystemCode != BillingConstants.PriceHeaderType.Maintenance &&
				x.L6_RN_NKCountry.IsEmpty &&
				!x.L6_PricelistVersion.IsEmpty)
				.GroupBy(x => x.L6_PricelistVersion)
				.OrderBy(x => x.Key)
				.ToDictionary(x => x.Key.ToString(), y => y.OrderBy(x => x.L6_RX_NKCurrency == "USD" ? 0 : 1).ThenBy(x => x.L6_RX_NKCurrency).First());
			}
			else
			{
				return new Dictionary<string, ClientLicencePriceHeader>(0);
			}
		}

		public static CodeDescriptionPairList GetPriceHeaderVersions(BusinessObjectFactory factory)
		{
			var map = GetPriceHeaderVersionMap(factory);
			var result = new CodeDescriptionPairList();
			foreach (var pair in map.OrderBy(x => x.Key))
			{
				result.AddPair(pair.Key, pair.Value.L6_PricelistVersion);
			}
			return result;
		}

		public CodeDescriptionPairList PriceHeaderVersions
		{
			get
			{
				return GetPriceHeaderVersions(Factory);
			}
		}

		public CodeDescriptionPairList VolumeCodes => Factory.GetCachedValue("EdiPriceHeaderLinkLookups.VolumeCodes", () => new EdiPriceHeaderLinkVolumeCodeList());

		public CodeDescriptionPairList CorePackCodes => Factory.GetCachedValue("EdiPriceHeaderLinkLookups.CorePackCodes", () => new EdiPriceHeaderLinkCorePackCodeList());
	}
}


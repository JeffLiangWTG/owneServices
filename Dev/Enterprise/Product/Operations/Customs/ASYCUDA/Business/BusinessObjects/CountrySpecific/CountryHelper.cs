using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CountryHelper
	{
		public CountryHelper(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		// We will only need a CommodityCodeListSECONDARY when we do a second country (other than Bangladesh) that needs commodity codes AND in the rare scenario that it's a two-country manifest.  We'll also need a second ACN_CommodityCode field :S

		public static List<string> SupportedCountries(BusinessObjectFactory factoryForCache)
		{
			return factoryForCache.GetCachedValue("GlobalManifest_SupportedCountries", delegate
			{
				return SupportedAsycudaCountryCodesList(factoryForCache).Select(x => (string)x.ZZD_Code)
					.Union(ApplicationBusinessProvider.GetManifestCountriesWithActiveManifestTypes(factoryForCache))
					.Distinct().ToList();
			});
		}

		public static List<ZZRefCusCodeListCombined> SupportedAsycudaCountryCodesList(BusinessObjectFactory factoryForCache)
		{
			return factoryForCache.GetCachedValue("SupportedAsycudaCountryCodesList", delegate
			{
				var countries = ZZRefCusCodeListCombined.Loader.Load(factoryForCache, universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, ZDateTime.Now).Distinct().ToList();
				// TODO: Remove this once RefCusCodeList[ManfiestCountry] has been cleaned up in ZZ.
				countries.RemoveAll(x => x.ZZD_Code == Core.Constants.CountryCodes.Singapore);
				countries.RemoveAll(x => x.ZZD_Code == Core.Constants.CountryCodes.SouthAfrica);
				countries.RemoveAll(x => x.ZZD_Code == Core.Constants.CountryCodes.UnitedStates);
				return countries;
			});
		}

		public static ZString GetPrimaryCountryCode(ZString originPort, ZString destinationPort, ForwardingConsol consol, BusinessObjectFactory factory)
		{
			var origin = originPort.Left(2);
			var destination = destinationPort.Left(2);

			if (destination.IsSupportedCountries(factory) && origin.IsSupportedCountries(factory))
			{
				return origin;  // Two-county manifest
			}

			if (DestinationXOROriginIsASupportedCountry(destination, origin, factory, out var supportedCountry))
			{
				return supportedCountry;
			}

			// Now check to see if a transhipment THROUGH this country is relevant
			if (consol != null)
			{
				foreach (Transport leg in consol.Transports)
				{
					origin = leg.JW_RL_NKLoadPort.Left(2);
					destination = leg.JW_RL_NKDiscPort.Left(2);

					if (DestinationXOROriginIsASupportedCountry(destination, origin, factory, out supportedCountry))
					{
						return supportedCountry;
					}
				}
			}
			return ZString.Empty;
		}

		static bool DestinationXOROriginIsASupportedCountry(ZString destination, ZString origin, BusinessObjectFactory factory, out ZString supportedCountry)
		{
			foreach (var c in SupportedCountries(factory))
			{
				if (destination == c ^ origin == c)
				{
					supportedCountry = c;
					return true;
				}
			}
			supportedCountry = ZString.Empty;
			return false;
		}

		internal CodeDescriptionPairList GetLocationsOfGoodsList(ZString countryCode)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(header.Factory, countryCode, universalAlias.RefCusCodeListTypes.Codes.Facilities);
		}

		public static CodeDescriptionPairList GetCustomsManifestStatusList(BusinessObjectFactory factory, ZString countryCode)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, countryCode, universalAlias.RefCusCodeListTypes.Codes.CustomsManifestStatus);
		}

		public static CodeDescriptionPairList GetCustomsStatusList(BusinessObjectFactory factory, ZString countryCode, ZString manifestType)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, countryCode, universalAlias.RefCusCodeListTypes.Codes.CustomsStatus, new ZString[] { manifestType });
		}

		public static void AddManifestCompanyFilterInSpecifiedCountry(ZQuery query)
		{
			if (!GlbStaff.CurrentUser.IsSupportUser)
			{
				var currentCompany = GlbCompany.CurrentCompany;
				var countryCode = currentCompany.GC_RN_NKCountryCode;
				if (countryCode == Core.Constants.CountryCodes.India)
				{
					var headerSubQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
					headerSubQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, SQLComparisonOperator.NotEqual, countryCode);
					var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), AsycudaManifestHeaderSchema.AMA_GB);
					branchQuery.AddToFilter(GlbBranchSchema.GB_GC, currentCompany.PK);
					headerSubQuery.AddSubQuery(branchQuery, JoinCondition.Or);
					query.AddToFilter(headerSubQuery, JoinCondition.And);
				}
			}
		}
	}
}

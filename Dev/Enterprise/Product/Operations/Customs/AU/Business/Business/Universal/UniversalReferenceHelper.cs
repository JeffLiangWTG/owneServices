using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class UniversalReferenceHelper
	{
#if DEBUG
		public static void InvalidateCache(BusinessObjectFactory factory)
		{
			factory.ClearCachedValue<ZDecimal>("AU.UniversalReferenceHelper.Deminimus");
		}
#endif

		public static ZDecimal GetDeminimus(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AU.UniversalReferenceHelper.Deminimus", () =>
			{
				return new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Australia, Universal.Constants.RateTypes.Deminimus, ZDateTime.Today)?.ZZF_Value ?? ZDecimal.Zero;
			});
		}

		public static bool Errata51Enabled()
		{
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
		}

		public static bool Errata53Enabled()
		{
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.EXDOCS_Errata53_1, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
		}

		public static bool Errata54Enabled()
		{
			return ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.EXDOCS_Errata54, Core.Constants.CountryCodes.Australia, ZDateTime.Today);
		}

		public static string GetPostcodeDeliveryClassificationAttribute(BusinessObjectFactory factory, string postcode)
		{
			if (string.IsNullOrWhiteSpace(postcode))
			{
				return string.Empty;
			}

			var postcodeCusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, postcode, Core.Constants.CountryCodes.Australia,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AqisPostCodes, ZDateTime.Today,
				attributeNames: new ZString[]
				{
					Core.Constants.Customs.Universal.RefCusCodeList.Attributes.PostcodeDeliveryClassification
				});
			return postcodeCusCode?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.PostcodeDeliveryClassification) ?? "Rural";
		}
	}
}

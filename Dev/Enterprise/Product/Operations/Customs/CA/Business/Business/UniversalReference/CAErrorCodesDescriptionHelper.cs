using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CA.Business
{
	public class CAErrorCodesDescriptionHelper
	{
		public ZString GetDescriptionFromCode(BusinessObjectFactory factory, ZString code)
		{
			return GetCusCodeListByCode(factory, code)?.ZZD_Description ?? ZString.Empty;
		}

		public ZString GetFrenchDescriptionFromCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.GetCachedValue($"CAErrorCodesDescriptionHelper|GetFrenchDescriptionFromCode|{code}|{ZDateTime.Today}", () =>
			{
				var cusCode = GetCusCodeListByCode(factory, code);
				return cusCode?.Languages.OfType<ZZRefCusCodeListLanguageCombined>().FirstOrDefault(x => x.ZXA_ZX6_NKLanguage == Core.Constants.CountryCodes.France)?.ZXA_Description ?? ZString.Empty;
			});
		}

		ZZRefCusCodeListCombined GetCusCodeListByCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.GetCachedValue($"CAErrorCodesDescriptionHelper|GetCusCodeListByCode|{code}|{ZDateTime.Today}", () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadForCodes(factory, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes, new ZString[] { code, code.TrimStart('0') }, ZDateTime.Today, null).FirstOrDefault();
			});
		}

		public ZString GetDescriptionFromMsgNoCode(BusinessObjectFactory factory, ZString code)
		{
			return factory.GetCachedValue($"CAErrorCodesDescriptionHelper|GetDescriptionFromMsgNoCode|{code}|{ZDateTime.Today}", () =>
			{
				var result = CCSErrorCodesList.GetDescriptionFromCode(code);
				if (string.IsNullOrEmpty(result))
				{
					var attributeFilters = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.MessageNum, JoinCondition.And, code) };
					var cusCode = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.CBSAErrorCodes, ZDateTime.Today, attributeFilters);
					result = cusCode.FirstOrDefault()?.ZZD_Description ?? ZString.Empty;
				}
				return result;
			});
		}

		CodeDescriptionPairList CCSErrorCodesList
		{
			get { return ccSErrorCodesList ?? (ccSErrorCodesList = new CCSErrorCodes()); }
		}
		CodeDescriptionPairList ccSErrorCodesList;
	}
}

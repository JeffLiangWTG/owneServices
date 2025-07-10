using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class AEONumberHelper
	{
		public static ZString GetAEONumber(this OrgHeader orgHeader, ZDateTime dateOfValuation)
		{
			var result = ZString.Empty;

			if (orgHeader != null)
			{
				OrgCusCode cusCode = null;

				var factory = orgHeader.Factory;
				var aeoCodes = orgHeader.CustomsCodes.Cast<OrgCusCode>().
					Where(x => (x.OK_CodeType == OrgCusCode.ChinaCodeTypes.AEO && IsAEOMutualRecognitionCountry(factory, x.OK_RN_NKCodeCountry, dateOfValuation))
							|| (x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && IsInEuropeanCustomsUnion(x.OK_RN_NKCodeCountry)))
					.ToArray();
				if (aeoCodes.Any())
				{
					var countryCode = orgHeader.MainAddress.OA_RN_NKCountryCode;
					cusCode = aeoCodes.FirstOrDefault(x => x.OK_RN_NKCodeCountry == countryCode);
					if (cusCode == null)
					{
						cusCode = aeoCodes.FirstOrDefault();
					}
				}

				if (cusCode != null)
				{
					result = cusCode.OK_RN_NKCodeCountry + cusCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		public static bool IsInEuropeanCustomsUnion(string countryCode)
		{
			return ObjectFactory
				.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>()
				.IsInEuropeanCustomsUnion(countryCode);
		}

		public static bool IsAEOMutualRecognitionCountry(BusinessObjectFactory factory, string code, ZDateTime dateOfValuation)
		{
			var aeoMutualRecognitionCountries = Universal.RefCusCodeListTypes.GetCachedList(factory
				, Core.Constants.CountryCodes.China
				, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries
				, dateOfValuation);

			return aeoMutualRecognitionCountries.ContainsCode(code);
		}
	}
}

using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public static class IdentificationTypeHelper
	{
		public static CodeDescriptionPairList GetIdentificationIndicatorListExcludingSIN(this CusTempStorageDec dec)
		{
			var resultList = new CodeDescriptionPairList();
			if (dec != null)
			{
				var officeCode = dec.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				resultList = dec.Factory.GetCachedValue(string.Join("|", "DE|CusTempStorageDecLookups|IdentificationIndicatorList|ExcludingSIN", officeCode), () =>
				{
					var temporaryIdentificationIndicatorList = new CodeDescriptionPairList(GetIdentificationIndicatorList(dec, officeCode));
					temporaryIdentificationIndicatorList.RemoveCode(TemporaryStorageIdentificationIndicatorList.Codes.SIN);
					return temporaryIdentificationIndicatorList;
				});
			}
			return resultList;
		}

		public static CodeDescriptionPairList GetIdentificationIndicatorListIncludingSIN(this CusTempStorageDec dec)
		{
			var resultList = new CodeDescriptionPairList();
			if (dec != null)
			{
				var officeCode = dec.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				resultList = dec.Factory.GetCachedValue(string.Join("|", "DE|CusTempStorageDecLookups|IdentificationIndicatorList|IncludingSIN", officeCode), () => new CodeDescriptionPairList(GetIdentificationIndicatorList(dec, officeCode)));
			}
			return resultList;
		}

		static CodeDescriptionPairList GetIdentificationIndicatorList(CusTempStorageDec dec, ZString officeCode)
		{
			return dec.Factory.GetCachedValue(string.Join("|", "DE|CusTempStorageDecLookups|IdentificationIndicatorList", officeCode), () =>
			{
				var identificationIndicatorList = new TemporaryStorageIdentificationIndicatorList();
				if (!officeCode.IsEmpty)
				{
					var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(dec.Factory, officeCode, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);
					if (cusCode != null && !cusCode.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(
						x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(RefCusCodeListAttributeTypes.Codes.ROLE) &&
						x.ZZE_Value.EqualsIgnoringCase(EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination) &&
						x.ZZE_TransportModes.Contains(RefTransportModeList.Codes.AIR, StringComparison.OrdinalIgnoreCase)))
					{
						identificationIndicatorList.RemoveCode(TemporaryStorageIdentificationIndicatorList.Codes.AWB);
					}
				}
				return identificationIndicatorList;
			});
		}
	}
}

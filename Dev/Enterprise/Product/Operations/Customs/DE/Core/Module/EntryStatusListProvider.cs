using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;
using IntegratedCountryEntryStatus = Enterprise.Customs.Common.Shared.IntegratedCountryCommonEntryStatusList;

namespace Enterprise.Customs.DE.Module
{
	public class EntryStatusListProvider : EU.Module.EntryStatusListProvider
	{
		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList(Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Germany, RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today));
			var exportList = new CodeDescriptionPairList(Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Germany, RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDateTime.Today));

			result.AddRangeOverwriteIfExists(exportList);
			result.AddPairIfNotExist(IntegratedCountryEntryStatus.Codes.Submitted, IntegratedCountryEntryStatus.Descriptions.Submitted);
			result.AddPairIfNotExist(IntegratedCountryEntryStatus.Codes.Acknowledged, IntegratedCountryEntryStatus.Descriptions.Acknowledged);
			result.Sort();
			return result;
		}
	}
}

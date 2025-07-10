using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public static class EntryInstructionStyleListHelper
	{
		public static CodeDescriptionPairList EntryInstructionStyleListForDataGrouping(BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			return factory.GetCachedValue(string.Join("_", "AsycudaCustoms.EntryInstructionStyleListForDataGrouping", dataGroupingCode),
				() =>
				{
					var styleCodes = new RefCusProcedure.Loader(factory).LoadDistinctGroupCodesForDatagrouping(dataGroupingCode);
					return GetEntryInstructionStyleDescriptionList(factory, dataGroupingCode, styleCodes);
				});
		}

		public static CodeDescriptionPairList EntryInstructionStyleListForDataGroupingAndShipmentType(BusinessObjectFactory factory, ZString dataGroupingCode, ZString shipmentType)
		{
			return factory.GetCachedValue(string.Join("_", "AsycudaCustoms.EntryInstructionStyleListForDataGroupingAndShipmentType", shipmentType, dataGroupingCode), () =>
			{
				var result = new CodeDescriptionPairList();
				var groupCodesForMessageType = new RefCusProcedure.Loader(factory).LoadDistinctGroupCodes(shipmentType, dataGroupingCode);
				result = GetEntryInstructionStyleDescriptionList(factory, dataGroupingCode, groupCodesForMessageType);
				return result;
			});
		}

		static CodeDescriptionPairList GetEntryInstructionStyleDescriptionList(BusinessObjectFactory factory, ZString dataGroupingCode, IEnumerable<ZString> styleCodes)
		{
			var result = new CodeDescriptionPairList();
			if (styleCodes.Any())
			{
				var descriptions = ZZRefCusCodeListCombined.Loader
								.LoadAndFallbackToParentDataGroupingIfNotFound(factory, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, ZDateTime.Now)
								.Select(x => (x.ZZD_Code, x.ZZD_Description))
								.ToArray();

				foreach (var code in styleCodes)
				{
					var description = descriptions.Where(x => x.ZZD_Code == code)
						.Select(x => x.ZZD_Description).FirstOrDefault();
					result.AddPairIfNotExist(code, description);
				}

				result.Sort();
			}
			return result;
		}
	}
}

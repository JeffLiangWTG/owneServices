using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Module.JobShipmentFilterBusinessObject;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePairImportExport = new LegacyCustomsEntryStatusList();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Australia, ReplaceSpecialCodeDescription(codePairImportExport).GetAllCodes(), new string[] { "RT1", "ROK", "CEO" });
			var codePairImport = new EdificeCustomsEntryStatusList();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Australia, ReplaceSpecialCodeDescription(codePairImport).GetAllCodes(), new string[] { "RT1", "ROK", "CEO" }, Common.Shared.SharedJobMessageTypeList.Codes.Import);
			var codePairExport = new ExportCustomsEntryStatusList();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Australia, ReplaceSpecialCodeDescription(codePairExport).GetAllCodes(), new string[] { "RT1", "ROK", "CEO" }, Common.Shared.SharedJobMessageTypeList.Codes.Export);
			var codePairShipment = new CustomsEntryStatusList();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Australia, ReplaceSpecialCodeDescription(codePairShipment).GetAllCodes(), new string[] { "RT1", "ROK", "CEO" }, "", true);
		}

		CodeDescriptionPairList ReplaceSpecialCodeDescription(CodeDescriptionPairList codePairList)
		{
			if (codePairList.ContainsCode(CustomsEntryStatus.NotSent.Code))
			{
				codePairList.RemoveCode(CustomsEntryStatus.NotSent.Code);
				codePairList.Insert(0, new CodeDescriptionPair(FilterStatus.NotSentCustomsStatusForFilter, CustomsEntryStatus.NotSent.MultilingualDescription));
			}

			return codePairList;
		}
	}
}

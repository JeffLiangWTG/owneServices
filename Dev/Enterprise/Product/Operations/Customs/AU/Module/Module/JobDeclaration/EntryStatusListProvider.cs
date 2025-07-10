using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Freight.Forwarding.Module.JobShipmentFilterBusinessObject;

namespace Enterprise.Customs.AU.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListForShipmentsCore(BusinessObjectFactory factory, ZString countryCode)
		{
			var result = new CustomsEntryStatusList();
			return ReplaceSpecialCodeDescription(result);
		}

		protected override CodeDescriptionPairList ReplaceSpecialCodeDescription(CodeDescriptionPairList codePairList)
		{
			if (codePairList.ContainsCode(CustomsEntryStatus.NotSent.Code))
			{
				codePairList.RemoveCode(CustomsEntryStatus.NotSent.Code);
				codePairList.Insert(0, new CodeDescriptionPair(FilterStatus.NotSentCustomsStatusForFilter, CustomsEntryStatus.NotSent.MultilingualDescription));
			}

			return codePairList;
		}

		protected override CodeDescriptionPairList EntryStatus_Import_List => new EdificeCustomsEntryStatusList();

		protected override CodeDescriptionPairList EntryStatus_Export_List => new ExportCustomsEntryStatusList();

		protected override CodeDescriptionPairList EntryStatus_ImportExport_List => new LegacyCustomsEntryStatusList();
	}
}

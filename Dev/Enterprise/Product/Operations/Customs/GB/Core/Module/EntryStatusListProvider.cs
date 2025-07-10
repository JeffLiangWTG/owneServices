using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider() { }

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			entryStatusImportExportList = factory.GetCachedValue("GBEntryStatusImportExportList", () =>
			{
				var combinedList = new CodeDescriptionPairList();
				var entryStatusList = factory.GetCachedValue<Common.EU.EntryStatusList>();
				var threeCharFunctionCodeList = factory.GetCachedValue<ThreeCharFunctionCode>();

				foreach (ICodeDescription pair in entryStatusList)
				{
					string description;
					if (threeCharFunctionCodeList.ContainsCode(pair.Code) && pair.Description != threeCharFunctionCodeList.GetDescriptionFromCode(pair.Code))
					{
						description = $"{pair.Description} / {threeCharFunctionCodeList.GetDescriptionFromCode(pair.Description)} (Applies to CDS and CHIEF)";
					}
					else
					{
						description = $"CHIEF - {pair.Description}";
					}
					combinedList.AddPair(pair.Description == "Not Sent" ? DeclarationFilterConstants.EntryStatus.NotSentForFilter : pair.Code, $"CHIEF - {pair.Description}");
				}

				foreach (ICodeDescription pair in threeCharFunctionCodeList)
				{
					if (!combinedList.ContainsCode(pair.Code))
					{
						combinedList.AddPair(pair.Code, $"CDS - {pair.Description}");
					}
				}

				return combinedList;
			});

			return base.EntryStatusListCore(factory);
		}

		protected override CodeDescriptionPairList EntryStatus_ImportExport_List => entryStatusImportExportList;
		CodeDescriptionPairList entryStatusImportExportList;
	}
}

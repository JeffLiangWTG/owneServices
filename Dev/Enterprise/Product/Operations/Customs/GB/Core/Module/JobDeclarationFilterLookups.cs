using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Module
{
	public class JobDeclarationFilterLookups : EU.Module.JobDeclarationFilterLookups
	{
		public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		public override CodeDescriptionPairList MessageStatusList()
		{
			return Factory.GetCachedValue("GBJobDeclarationFilterLookupsMessageStatusList", () =>
			{
				var list1 = Factory.GetCachedValue<Common.Shared.EntryStatusList>();
				var list2 = Factory.GetCachedValue<Common.Shared.MessageStatusList>();
				var result = new CodeDescriptionPairList();

				foreach (ICodeDescription pair in list1)
				{
					if (list2.ContainsCode(pair.Code) && pair.Description != list2.GetDescriptionFromCode(pair.Code))
					{
						result.AddPair(pair.Code, $"{pair.Description} / {list2.GetDescriptionFromCode(pair.Code)}");
					}
					else
					{
						result.Add(pair);
					}
				}

				foreach (ICodeDescription pair in list2)
				{
					if (!string.IsNullOrEmpty(pair.Code) && !list1.ContainsCode(pair.Code))
					{
						result.Add(pair);
					}
				}

				return result;
			});
		}

		protected override CodeDescriptionPairList EntrySubTypesCore
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsEntrySubTypes", () =>
				{
					var list = new CodeDescriptionPairList(Factory.GetCachedValue<EntrySubStyleListImport>());
					list.AddRange(Factory.GetCachedValue<EntrySubStyleListExport>());
					return list;
				});
			}
		}

		public override CodeDescriptionPairList DeclarationTypeList
		{
			get
			{
				return Factory.GetCachedValue("EUJobDeclarationFilterLookupsDeclarationTypeList", () =>
				{
					var list = new CodeDescriptionPairList(Factory.GetCachedValue<ImportDeclarationTypeList>());
					list.AddRange(Factory.GetCachedValue<ExportDeclarationTypeList>());
					return list;
				});
			}
		}

		public override CodeDescriptionPairList ApplicationCodeList() => Factory.GetCachedValue<Registry.Business.DeclarationApplicationCodeList>();

		protected override ZString[] GetDataGroupingCodesForSupportingDocumentList => new ZString[] { Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services };
	}
}

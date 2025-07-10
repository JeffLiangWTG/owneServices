using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.Module
{
	public class EntryStatusListProvider : EU.Module.EntryStatusListProvider
	{
		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("Enterprise.Customs.IE.Module.EntryStatusListProvider.EntryStatusListCore", () =>
			{
				var list = new ExportDeclarationTypeList();
				list.AddRange(new AESEntryStatusList());
				list.AddRange(new AISEntryStatusList());
				list.AddRange(new CustomsWareEntryStatusList()); // IE: currently support CustomsWare
				list.SortByDescriptionAndCombineIfSameCode();
				return list;
			});
		}
	}
}

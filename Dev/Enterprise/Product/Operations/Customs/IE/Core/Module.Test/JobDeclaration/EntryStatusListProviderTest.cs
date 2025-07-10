using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var list = new ExportDeclarationTypeList();
			list.AddRange(new AESEntryStatusList());
			list.AddRange(new AISEntryStatusList());
			list.AddRange(new CustomsWareEntryStatusList());
			list.SortByDescriptionAndCombineIfSameCode();

			var provider = new EntryStatusListProvider();
			var entryStatusList = (CodeDescriptionPairList)provider.EntryStatusList(Factory, Core.Constants.CountryCodes.Ireland, ZString.Empty);

			AssertEquals(list.CodesAsString, entryStatusList.CodesAsString);
		}
	}
}

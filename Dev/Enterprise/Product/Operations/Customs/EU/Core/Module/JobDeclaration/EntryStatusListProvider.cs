using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override CodeDescriptionPairList EntryStatusListForCustomsWare => new Common.EU.CustomsWareEntryStatusList();

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack => new Common.EU.EntryStatusList();
	}
}

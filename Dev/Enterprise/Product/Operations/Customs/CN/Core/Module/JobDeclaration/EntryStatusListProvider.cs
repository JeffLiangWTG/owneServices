using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override CodeDescriptionPairList EntryStatusListForDefaultFallBack => new Common.CN.EntryStatusList();
	}
}

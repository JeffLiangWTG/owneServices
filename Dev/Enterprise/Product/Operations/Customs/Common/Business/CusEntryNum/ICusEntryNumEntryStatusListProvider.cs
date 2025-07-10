using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public interface ICusEntryNumEntryStatusListProvider
	{
		CodeDescriptionPairList EntryStatusList { get; }
		void OnEntryStatusSet(ZString entryType, ZString newValue);
	}
}

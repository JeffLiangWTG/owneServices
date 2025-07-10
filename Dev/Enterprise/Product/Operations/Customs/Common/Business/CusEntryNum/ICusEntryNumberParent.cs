using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public interface ICusEntryNumberParent
	{
		bool CanBeChangedOrDeleted(CusEntryNumber entryNumber, out string errMsg);
		void EntryNumberChanged(ZString oldValue, ZString newValue);
		string EntryNumberChangedCallStack { get; }
	}
}

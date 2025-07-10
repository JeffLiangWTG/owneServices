using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR38Supporter
	{
		ZString EntryType { get; }
		void UpdateParent(CusEntryHeader entry, ZString typeCode, IGOVCBRR38MessageData messageData);
	}
}

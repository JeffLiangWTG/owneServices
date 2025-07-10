using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR99Supporter
	{
		ZString EntryType { get; }

		void UpdateParent(EDIMessage message, CusEntryHeader entry, ZString typeCode, IGOVCBRR99MessageData messageData, ZStringBuilder emailBuilder);
	}
}

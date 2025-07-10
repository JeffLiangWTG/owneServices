using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public interface IGOVCBRR20Supporter
	{
		ZString EntryType { get; }
		void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum);
		BusinessObject LoadParent(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType);
		EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum);
		ZString GetKeyToFindOutgoingMessageOrEntryNum(BusinessObject header, IGOVCBRR20MessageData messageData);
	}
}

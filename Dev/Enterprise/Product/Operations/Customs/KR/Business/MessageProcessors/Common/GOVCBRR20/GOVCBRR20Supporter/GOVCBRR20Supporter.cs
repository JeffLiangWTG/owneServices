using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.KR.Business
{
	abstract class GOVCBRR20Supporter : IGOVCBRR20Supporter
	{
		public abstract ZString EntryType { get; }

		public abstract void UpdateParent(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum);

		public virtual ZString GetKeyToFindOutgoingMessageOrEntryNum(BusinessObject header, IGOVCBRR20MessageData messageData)
		{
			return messageData.AmendSequence.ToString();
		}

		public virtual EDIMessage GetOutgoingMessage(BusinessObject header, ZString typeCode, ZString keyToFindOutgoingMessageOrEntryNum)
		{
			return (EDIMessage)((IEDIMessageCollectionProvider)header).Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, typeCode);
		}

		public virtual BusinessObject LoadParent(BusinessObjectFactory factory, GlbCompany company, ZString entryNumber, ZString entryType)
		{
			return MessageLinkedObjectManager.GetLinkedObject(factory, company, entryNumber, entryType);
		}
	}
}

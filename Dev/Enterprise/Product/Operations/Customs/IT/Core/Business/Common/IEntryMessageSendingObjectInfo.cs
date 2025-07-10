using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IEntryMessageSendingObjectInfo
{
	ZBool EntryStatusAllowsSending { get; }
	ZString EntryReference { get; }
	ZString EntryMessageStatus { get; }
	ZString EntryCustomsStatus { get; }
}

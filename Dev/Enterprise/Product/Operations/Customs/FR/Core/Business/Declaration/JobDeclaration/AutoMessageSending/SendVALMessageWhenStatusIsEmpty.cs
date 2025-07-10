using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class SendVALMessageWhenStatusIsEmpty : IAutoSendCustomsMessageRule
	{
		public ZString newMessageType => EntryActionCodeList.Codes.VAL;

		public ZBool CanSendMessage(CusEntryHeader entry) => entry.CH_EntryStatus.IsEmpty;
	}
}

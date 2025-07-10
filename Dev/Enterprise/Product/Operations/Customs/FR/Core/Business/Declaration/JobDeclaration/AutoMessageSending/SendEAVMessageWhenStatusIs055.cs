using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class SendEAVMessageWhenStatusIs055 : IAutoSendCustomsMessageRule
	{
		public ZString newMessageType => EntryActionCodeList.Codes.EAV;

		public ZBool CanSendMessage(CusEntryHeader entry)
		{
			return entry.CH_EntryStatus == EntryStatusDescriptionCodeList.Codes.ES055;
		}
	}
}

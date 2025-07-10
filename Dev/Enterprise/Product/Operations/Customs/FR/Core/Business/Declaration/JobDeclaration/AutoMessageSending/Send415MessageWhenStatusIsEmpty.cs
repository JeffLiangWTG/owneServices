using CargoWise.Types;
using Enterprise.Customs.FR.Business.Interfaces;

namespace Enterprise.Customs.FR.Business.Declaration
{
	class Send415MessageWhenStatusIsEmpty : IAutoSendCustomsMessageRule
	{
		public ZString newMessageType => DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;

		public ZBool CanSendMessage(CusEntryHeader entry) => entry.CH_EntryStatus.IsEmpty;
	}
}

using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPDATMessageHeaderProvider : AESMessageHeaderProvider
	{
		public EXPDATMessageHeaderProvider(ExportEntryMessageSendingAction action)
			: base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
		}
		readonly ExportEntryMessageSendingAction action;

		public override IAESHeader AESHeader => aesHeader ?? (aesHeader = new EXPDATHeaderProvider(action));
		IAESHeader aesHeader;
	}
}

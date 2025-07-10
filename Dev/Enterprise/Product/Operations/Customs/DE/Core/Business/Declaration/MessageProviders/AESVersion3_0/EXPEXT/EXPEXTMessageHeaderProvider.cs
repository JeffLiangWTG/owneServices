using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPEXTMessageHeaderProvider : AESMessageHeaderProvider
	{
		public EXPEXTMessageHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = action;
		}

		public override IAESHeader AESHeader => aesHeader ?? (aesHeader = new EXPEXTHeaderProvider(action));
		IAESHeader aesHeader;

		readonly ExportEntryMessageSendingAction action;
	}
}

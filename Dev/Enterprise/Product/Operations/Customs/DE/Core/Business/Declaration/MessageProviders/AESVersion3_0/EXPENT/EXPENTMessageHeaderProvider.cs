using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.AESVersion3_0
{
	public class EXPENTMessageHeaderProvider : AESMessageHeaderProvider, IEXPENTMessageHeader
	{
		public EXPENTMessageHeaderProvider(ExportEntryMessageSendingAction action) : base(action?.MessagingObject)
		{
			this.action = Argument.NotNull(action, nameof(action));
		}
		readonly ExportEntryMessageSendingAction action;

		public override IAESHeader AESHeader => aesHeader ?? (aesHeader = new EXPENTHeaderProvider(action));
		IAESHeader aesHeader;

		string IAESMessageHeader.InterchangeRecipientID => Declaration.GetOfficeReferenceNumber(EuOfficeCodesTypes.Codes.SupplementaryDeclarationOffice);
	}
}

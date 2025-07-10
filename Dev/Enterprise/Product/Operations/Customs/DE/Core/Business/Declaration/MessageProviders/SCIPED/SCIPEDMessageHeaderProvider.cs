using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIPEDMessageHeaderProvider : MonthlyClosingMessageHeaderProvider
	{
		public SCIPEDMessageHeaderProvider(CusReconDeclaration declaration, string messageRole)
			: base(declaration)
		{
			this.messageRole = messageRole;
		}
		readonly string messageRole;

		public override IImportHeader Header => header ?? (header = new SCIPEDHeaderProvider(Declaration, messageRole));
		IImportHeader header;

		public override string MessageGroup => MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingInwardProcessing;
	}
}

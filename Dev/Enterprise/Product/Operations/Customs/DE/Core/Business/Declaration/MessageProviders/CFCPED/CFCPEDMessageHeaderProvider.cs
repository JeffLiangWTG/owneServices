using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCPEDMessageHeaderProvider : MonthlyClosingMessageHeaderProvider
	{
		public CFCPEDMessageHeaderProvider(CusReconDeclaration declaration, string messageRole)
			: base(declaration)
		{
			this.messageRole = messageRole;
		}
		readonly string messageRole;

		public override IImportHeader Header => header ?? (header = new CFCPEDHeaderProvider(Declaration, messageRole));
		IImportHeader header;

		public override string MessageGroup => MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
	}
}

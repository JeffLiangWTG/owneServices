using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM413And415OperationProvider : IOperation
	{
		public IM413And415OperationProvider(CusEntryHeader entryHeader, bool generateNewLRN)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
			instruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
			this.generateNewLRN = generateNewLRN;
		}
		protected readonly JobDeclaration declaration;
		protected readonly CusEntryInstruction instruction;
		protected readonly CusEntryHeader entryHeader;
		readonly bool generateNewLRN;

		public string MsgType => instruction.CEI_Style;

		public string DeclarationType => declaration.JE_EntryStyle;

		public string AdditionalDeclarationType => instruction.CEI_SubStyle;

		public string LanguageCode => Constants.LanguageType.English;

		public string PreferredPaymentMethod => declaration.JE_PaymentMethod;

		public string LRN => generateNewLRN ? AISOutboundEDIMessage.LRNPlaceHolder : entryHeader.CH_BGMReference.ToString();
	}
}

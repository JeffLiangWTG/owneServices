using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCDECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public CFCDECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new CFCDECHeaderProvider(EntryHeader));

		public override string MessageGroup => ImportSubStyleList.IsFinalDeclaration(entryInstruction.Factory, entryInstruction.CEI_SubStyle)
			? ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration
			: ImportMessageSubTypeList.Codes.FreeCirculationPrematureInputSingleDeclaration;

		IImportHeader header;
	}
}

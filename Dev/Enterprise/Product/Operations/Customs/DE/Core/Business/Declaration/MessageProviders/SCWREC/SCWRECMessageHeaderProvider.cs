using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWRECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public SCWRECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new SCWRECHeaderProvider(EntryHeader));

		public override string MessageGroup
		{
			get
			{
				var result = string.Empty;
				if (entryInstruction.CEI_SubStyle == ImportSubStyleList.Codes.C)
				{
					result = ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedDeclaration;
				}
				else if (entryInstruction.CEI_SubStyle == ImportSubStyleList.Codes.F)
				{
					result = ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedPrematureDeclaration;
				}
				return result;
			}
		}

		IImportHeader header;
	}
}

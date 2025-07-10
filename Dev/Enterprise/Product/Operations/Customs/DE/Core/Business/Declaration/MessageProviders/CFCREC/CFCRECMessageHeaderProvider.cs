using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CFCRECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public CFCRECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new CFCRECHeaderProvider(EntryHeader));

		public override string MessageGroup
		{
			get
			{
				switch (entryInstruction.CEI_SubStyle)
				{
					case ImportSubStyleList.Codes.C:
						return ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedDeclaration;
					case ImportSubStyleList.Codes.F:
						return ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedPrematureDeclaration;
					default:
						return string.Empty;
				}
			}
		}

		IImportHeader header;
	}
}

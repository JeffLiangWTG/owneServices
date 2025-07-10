using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIRECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public SCIRECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new SCIRECHeaderProvider(EntryHeader));

		public override string MessageGroup
		{
			get
			{
				switch (entryInstruction.CEI_SubStyle)
				{
					case ImportSubStyleList.Codes.C:
						return ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedDeclaration;
					case ImportSubStyleList.Codes.F:
						return ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedPrematureDeclaration;
					default:
						return string.Empty;
				}
			}
		}

		IImportHeader header;
	}
}

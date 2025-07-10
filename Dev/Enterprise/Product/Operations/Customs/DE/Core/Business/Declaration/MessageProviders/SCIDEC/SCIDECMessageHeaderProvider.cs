using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIDECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public SCIDECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new SCIDECHeaderProvider(EntryHeader));

		public override string MessageGroup
		{
			get
			{
				switch (entryInstruction.CEI_SubStyle)
				{
					case ImportSubStyleList.Codes.A:
					case ImportSubStyleList.Codes.B:
						return ImportMessageSubTypeList.Codes.InwardProcessingSingleDeclaration;
					case ImportSubStyleList.Codes.D:
					case ImportSubStyleList.Codes.E:
						return ImportMessageSubTypeList.Codes.InwardProcessingSinglePrematureDeclaration;
					default:
						return string.Empty;
				}
			}
		}

		SCIDECHeaderProvider header;
	}
}

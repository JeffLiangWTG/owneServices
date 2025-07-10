using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWDECMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public SCWDECMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public override IImportHeader Header => header ?? (header = new SCWDECHeaderProvider(EntryHeader));
		IImportHeader header;

		public override string MessageGroup
		{
			get
			{
				switch (entryInstruction.CEI_SubStyle)
				{
					case ImportSubStyleList.Codes.A:
						return ImportMessageSubTypeList.Codes.BondedWarehouseSingleDeclaration;
					case ImportSubStyleList.Codes.D:
						return ImportMessageSubTypeList.Codes.BondedWarehouseSinglePrematureDeclaration;
					default:
						return string.Empty;
				}
			}
		}
	}
}

using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class CUSWATMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public CUSWATMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override IImportHeader Header => header ?? (header = new CUSWATHeaderProvider(EntryHeader));
		IImportHeader header;

		public override string MessageGroup => ImportMessageSubTypeList.Codes.WarehouseStockTransfer;
	}
}

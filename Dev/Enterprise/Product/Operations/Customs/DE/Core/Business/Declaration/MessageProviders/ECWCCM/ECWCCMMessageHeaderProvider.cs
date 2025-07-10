using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ECWCCMMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public ECWCCMMessageHeaderProvider(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override IImportHeader Header => header ?? (header = new ECWCCMHeaderProvider(EntryHeader));
		IImportHeader header;

		public override string MessageGroup => ImportMessageSubTypeList.Codes.WarehouseStockTransfer;
	}
}

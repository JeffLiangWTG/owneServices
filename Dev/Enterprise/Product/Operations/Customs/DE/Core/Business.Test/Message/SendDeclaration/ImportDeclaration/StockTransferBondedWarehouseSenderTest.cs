using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class StockTransferBondedWarehouseSenderTest : ImportDeclarationSenderTest<StockTransferBondedWarehouseSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LCUSWK);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.WarehouseStockTransfer;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.StockTransferBondedWarehouse;

		protected override ZString SubStyle => ZString.Empty;

		protected override StockTransferBondedWarehouseSender GetImportDeclarationSender() => new StockTransferBondedWarehouseSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			entryHeader.AllEntryLines.ForEach(e =>
			{
				var previousDocument = entryHeader.EntryInstruction.PreviousDocuments.AddNew();
				previousDocument.CSI_ReferenceNumber = "REF1";
				previousDocument.CSI_ItemNumber = e.RandomLine.JI_LineNo;
			});
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

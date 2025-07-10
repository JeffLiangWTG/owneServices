using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CollectiveClearanceBondedWarehouseSenderTest : ImportDeclarationSenderTest<CollectiveClearanceBondedWarehouseSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LECWCG);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.WarehouseStockTransfer;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse;

		protected override ZString SubStyle => ZString.Empty;

		protected override CollectiveClearanceBondedWarehouseSender GetImportDeclarationSender() => new CollectiveClearanceBondedWarehouseSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

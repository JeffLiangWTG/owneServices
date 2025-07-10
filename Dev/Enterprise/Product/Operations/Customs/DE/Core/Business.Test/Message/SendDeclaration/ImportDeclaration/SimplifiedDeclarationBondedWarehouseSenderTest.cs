using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SimplifiedDeclarationBondedWarehouseSenderTest : ImportDeclarationSenderTest<SimplifiedDeclarationBondedWarehouseSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.LSCWRL);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedDeclaration;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.EntryInDeclarantsRecordsBondedWarehouse;

		protected override ZString SubStyle => ImportSubStyleList.Codes.C;

		protected override ZBool ShouldCreateReconEntry => true;

		protected override ZBool ShouldResetEntryStatus => true;

		protected override SimplifiedDeclarationBondedWarehouseSender GetImportDeclarationSender() => new SimplifiedDeclarationBondedWarehouseSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

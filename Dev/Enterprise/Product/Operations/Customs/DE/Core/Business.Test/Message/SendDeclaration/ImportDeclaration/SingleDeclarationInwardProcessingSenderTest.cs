using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SingleDeclarationInwardProcessingSenderTest : ImportDeclarationSenderTest<SingleDeclarationInwardProcessingSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIDC);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.InwardProcessingSingleDeclaration;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.SingleDeclarationInwardProcessing;

		protected override ZString SubStyle => ImportSubStyleList.Codes.A;

		protected override ZBool ShouldResetEntryStatus => true;

		protected override SingleDeclarationInwardProcessingSender GetImportDeclarationSender() => new SingleDeclarationInwardProcessingSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

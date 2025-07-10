using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SimplifiedDeclarationInwardProcessingSenderTest : ImportDeclarationSenderTest<SimplifiedDeclarationInwardProcessingSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIRJ);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedDeclaration;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.SimplifiedDeclarationInwardProcessing;

		protected override ZString SubStyle => ImportSubStyleList.Codes.C;

		protected override ZBool ShouldCreateReconEntry => true;

		protected override ZBool ShouldResetEntryStatus => true;

		protected override SimplifiedDeclarationInwardProcessingSender GetImportDeclarationSender() => new SimplifiedDeclarationInwardProcessingSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

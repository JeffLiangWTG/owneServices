using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class SimplifiedDeclarationFreeCirculationSenderTest : ImportDeclarationSenderTest<SimplifiedDeclarationFreeCirculationSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.FCFCRF);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedDeclaration;

		protected override ZString DeclarationType => ImportEntryTypeList.Codes.SimplifiedDeclarationFreeCirculation;

		protected override ZString SubStyle => ImportSubStyleList.Codes.C;

		protected override ZBool ShouldCreateReconEntry => true;

		protected override ZBool ShouldResetEntryStatus => true;

		protected override SimplifiedDeclarationFreeCirculationSender GetImportDeclarationSender() => new SimplifiedDeclarationFreeCirculationSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
		}
		ImportEntryMessageSendingAction action;
	}
}

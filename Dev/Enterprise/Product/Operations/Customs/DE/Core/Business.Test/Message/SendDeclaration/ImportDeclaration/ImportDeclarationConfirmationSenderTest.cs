using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportDeclarationConfirmationSenderTest : ImportDeclarationSenderTest<ImportDeclarationConfirmationSender>
	{
		protected override ZString ExpectedMessageTypeATLASVersion10_1 => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.GCCONJ);

		protected override ZString ExpectedMessageSubType => Messaging.ImportMessageSubTypeList.Codes.FreeCirculationPrematureInputSingleDeclaration;

		protected override ZString DeclarationType => ImportDeclarationTypeList.Codes.EZA;

		protected override ZString SubStyle => ImportSubStyleList.Codes.D;

		protected override ZString ExpectedLogbookRegistrationNumber => "ATA001203191020203302";

		protected override ImportDeclarationConfirmationSender GetImportDeclarationSender() => new ImportDeclarationConfirmationSender(action);

		protected override void SetUp()
		{
			base.SetUp();
			action = new ImportEntryMessageSendingAction(entryHeader, null);
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = ExpectedLogbookRegistrationNumber;
		}
		ImportEntryMessageSendingAction action;
	}
}

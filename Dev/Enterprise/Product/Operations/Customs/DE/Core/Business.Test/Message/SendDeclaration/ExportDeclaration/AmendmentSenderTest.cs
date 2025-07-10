using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class AmendmentSenderTest : ExportDeclarationSenderTest<AmendmentSender>
	{
		protected override ExportDeclarationSender GetExportDeclarationSender() => new AmendmentSender(action);

		protected override ZString ExpectedMessageType => nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPAE);

		protected override ZString ExpectedMessageSubType => Messaging.ExportMessageSubTypeList.Codes.EXP;

		protected override bool LocalReferenceNumberExpected => false;

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}
		protected ExportEntryMessageSendingAction action;
	}
}

using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class EntireDataSenderTest : ExportDeclarationSenderTest<EntireDataSender>
	{
		protected override ExportDeclarationSender GetExportDeclarationSender() => new EntireDataSender(action);

		protected override ZString ExpectedMessageType => nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPEE);

		protected override ZString ExpectedMessageSubType => Messaging.ExportMessageSubTypeList.Codes.EXP;

		protected override bool LocalReferenceNumberExpected => true;

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
		}

		protected ExportEntryMessageSendingAction action;
	}
}

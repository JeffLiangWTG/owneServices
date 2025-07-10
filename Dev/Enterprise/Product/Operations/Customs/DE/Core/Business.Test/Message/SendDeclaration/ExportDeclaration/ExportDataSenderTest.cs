using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ExportDataSenderTest : ExportDeclarationSenderTest<ExportDataSender>
	{
		protected override ExportDeclarationSender GetExportDeclarationSender() => new ExportDataSender(action);

		protected override ZString ExpectedMessageType => nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDF);

		protected override ZString ExpectedMessageSubType => Messaging.ExportMessageSubTypeList.Codes.EXP;

		protected override bool LocalReferenceNumberExpected => true;

		protected override void SetUp()
		{
			base.SetUp();
			action = new ExportEntryMessageSendingAction(entryHeader);
			entryHeader.CH_CEI_Instruction = Factory.New<CusEntryInstruction>().PK;
		}

		protected ExportEntryMessageSendingAction action;
	}
}

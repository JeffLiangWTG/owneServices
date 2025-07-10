using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CancellationRequestSenderTest : ExportDeclarationSenderTest<CancellationRequestSender>
	{
		public override void TestSendMessage()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				base.TestSendMessage();
			}
		}

		protected override ExportDeclarationSender GetExportDeclarationSender() => new CancellationRequestSender(action);

		protected override ZString ExpectedMessageType => nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPCD);

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

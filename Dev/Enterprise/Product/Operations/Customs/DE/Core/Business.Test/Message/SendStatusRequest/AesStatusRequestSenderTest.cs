using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AesStatusRequestSender))]
	class AesStatusRequestSenderTest : StatusRequestSenderAbstractTest<AesStatusRequestSender>
	{
		public override void TestSend()
		{
			var messageVersionRegistryCollection = new MessageVersionRegistryCollection { new MessageVersionRegistry { SystemCode = MessageVersionRegistry.AESSystemCode, VersionNumber = AESVersionNumberList.Codes._30 } };
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234"))
			using (DECustomsDataRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
			{
				base.TestSend();
			}
		}

		protected override ZString ExpectedMessageType => Messaging.EDIMessageTypeList.Codes.AES;

		protected override ZString ExpectedMessageSubType => Messaging.ExportMessageSubTypeList.Codes.EXQ;

		protected override ZString ExpectedApplicationReference => nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXQQB);

		protected override ZString Module => ExportStatusRequestModuleCodeList.Codes.AES;

		protected override ZString ExpectedApplicationCode => ApplicationCodes.DECustomsAesSystem;

		protected override StatusRequestSender GetSender() => new AesStatusRequestSender(statusRequest);
	}
}

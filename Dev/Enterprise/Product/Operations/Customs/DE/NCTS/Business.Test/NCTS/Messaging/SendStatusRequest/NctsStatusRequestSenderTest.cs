using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsStatusRequestSender))]
	class NctsStatusRequestSenderTest : StatusRequestSenderAbstractTest<NctsStatusRequestSender>
	{
		public override void TestSend()
		{
			using (DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "1234"))
			{
				base.TestSend();
			}
		}

		protected override ZString ExpectedMessageType => Messaging.EDIMessageTypeList.Codes.NCTS;

		protected override ZString ExpectedMessageSubType => Messaging.NctsMessageSubTypeList.Codes.StatusRequestMessage;

		protected override ZString ExpectedApplicationReference => nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETQQC);

		protected override ZString Module => ExportStatusRequestModuleCodeList.Codes.NCTS;

		protected override ZString ExpectedApplicationCode => ApplicationCodes.DECustomsAtlasSystem;

		protected override StatusRequestSender GetSender() => new NctsStatusRequestSender(statusRequest);
	}
}

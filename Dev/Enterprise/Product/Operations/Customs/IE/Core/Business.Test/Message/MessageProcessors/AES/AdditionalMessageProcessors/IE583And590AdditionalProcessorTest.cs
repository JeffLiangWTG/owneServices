using System;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.IE.Business.Message.MessageProcessors.AdditionalMessageProcessors.AES;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE583And590AdditionalProcessorTest : AdditionalProcessorAbstractTest
	{
		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsExport;

		protected override ZString MessageType => AESOutgoingMessageTypeList.Codes.ExitNotification;

		protected override ZString MessageSubType => EDIMessage.Status.Acknowledged;

		protected override IAdditionalMessageProcessing Processor => new IE583And590AdditionalProcessor();

		protected override EDIMessage MessageToProcess
		{
			get
			{
				var message = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESInboundEDIMessage.Status.Acknowledged, Guid.NewGuid().ToString());
				message.EM_LinkedObject = exitReport;
				return message;
			}
		}

		protected override void AssertProcessMessageResultsCore()
		{
			AssertEquals("Exit report status is ACC", "ACC", exitReport.CER_MessageStatus);
		}

		protected override void SetUp()
		{
			var header = Factory.New<CusExitHeader>();
			exitReport = header.CusExitReports.AddNew();
			exitReport.CER_MessageStatus = "SNT";
		}

		CusExitReport exitReport;
	}
}

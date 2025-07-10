using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE515AdditionalProcessorTest : AdditionalProcessorAbstractTest
	{
		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsExport;

		protected override ZString MessageType => AESOutgoingMessageTypeList.Codes.ExportOriginal;

		protected override ZString MessageSubType => EDIMessage.Status.Acknowledged;

		protected override IAdditionalMessageProcessing Processor => new MessageAcknowledgementAdditionalProcessor();

		protected override EDIMessage MessageToProcess
		{
			get
			{
				var message = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESInboundEDIMessage.Status.Acknowledged, Guid.NewGuid().ToString());
				message.EM_LinkedObject = cusEntryHeader;
				return message;
			}
		}

		protected override void AssertProcessMessageResultsCore()
		{
			var logs = declaration.Logs.GetAllLogs();
			AssertGreaterThan(logs.Count, 0);
			AssertEquals("Entry header declaration logged ECM event", "ECM", logs[logs.Count - 1].Event.SE_Code);
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
		}

		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader;
	}
}

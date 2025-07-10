using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCDuimpLinesErrorResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCDuimpLinesErrorResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CIL" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCDuimpLinesErrorResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "DUIMP Lines Error Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("20BR00000012345");

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();
			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entryHeader.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entryHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entryHeader.CH_Status);
				AssertEquals("MessageRejected log added", AutoEvents.MessageRejected.Code, entryHeader.Logs.MostRecentLog.SL_SE_NKEvent);
			});
		}

		const string JsonMessage = @"{
  ""message"": ""Mensagem de exemplo."",
  ""identificacao"": {
    ""numero"": ""20BR00000012345"",
    ""versao"": ""1""
  },
  ""errors"": [
    {
      ""code"": ""DIMP-ER0004"",
      ""field"": ""numero"",
      ""message"": ""Número da Duimp inválido.""
    }
  ]
}
";
	}
}

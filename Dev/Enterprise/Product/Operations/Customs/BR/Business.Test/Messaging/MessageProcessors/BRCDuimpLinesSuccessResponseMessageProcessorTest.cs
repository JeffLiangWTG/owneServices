using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCDuimpLinesSuccessResponseMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCDuimpLinesSuccessResponseMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CIL" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "SUC" };

		public void TestMessageFriendlyName()
		{
			var processor = new BRCDuimpLinesSuccessResponseMessageProcessor(new LoggingInformation());
			AssertEquals("MessageFriendlyName", "DUIMP Lines Success Response Message", processor.MessageFriendlyName);
		}

		public void TestProcessResponseMessage_Success()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("24BR00000002090");

			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;

			CombineAssertions(() =>
			{
				AssertEquals("EntryLine 1 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 2 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 3 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine3.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 4 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine4.CL_CustomsPostedStatus);
			});

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entryHeader.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entryHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("CH_Status", ZString.Empty, entryHeader.CH_Status);

				AssertEquals("EntryLine 1 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 2 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 3 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine3.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 4 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine4.CL_CustomsPostedStatus);
			});

			entryHeader.CH_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;
			entryLine3.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.UpdatePending;

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entryHeader.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entryHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entryHeader.CH_Status);

				AssertEquals("EntryLine 1 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 2 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 3 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine3.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 4 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine4.CL_CustomsPostedStatus);
			});

			entryLine1.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Deleted;
			entryLine3.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.DeletePending;

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_GB", entryHeader.Branch.PK, responseMessage.EM_GB);
				AssertEquals("EM_LinkUniqueID", entryHeader.PK, responseMessage.EM_LinkUniqueID);
				AssertEquals("EM_LinkTable", entryHeader.TableName, responseMessage.EM_LinkTable);
				AssertEquals("EM_Status", EDIMessage.Status.Received, responseMessage.EM_Status);

				AssertEquals("CH_Status", BRMessageStatusList.Codes.Rejected, entryHeader.CH_Status);

				AssertEquals("EntryLine 1 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Deleted, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 2 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine2.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 3 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Deleted, entryLine3.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 4 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Active, entryLine4.CL_CustomsPostedStatus);
			});

			entryLine2.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;
			entryLine4.CL_CustomsPostedStatus = CustomsPostedStatusList.Codes.Accepted;

			responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessage;
			Factory.Save();

			ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("CH_Status", BRMessageStatusList.Codes.Accepted, entryHeader.CH_Status);

				AssertEquals("EntryLine 1 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Deleted, entryLine1.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 2 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine2.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 3 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Deleted, entryLine3.CL_CustomsPostedStatus);
				AssertEquals("EntryLine 4 CL_CustomsPostedStatus", CustomsPostedStatusList.Codes.Accepted, entryLine4.CL_CustomsPostedStatus);
			});
		}

		public void TestProcessResponseMessage_LogErrorWithoutMultiStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("24BR00000002090");

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = JsonMessageEmptyMultiStatus;
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("Logger", "Error: \tMessage #1: multiStatus tag is empty.\r\n", logger.LogMessages.ToString());
			});
		}

		public void TestProcessResponseMessage_LogErrorWithDeserializationFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("24BR00000002090");

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entryHeader, MessageTypeList.Codes.CIL, EDIMessageSubTypeList.Codes.Success).ResponseMessage;
			responseMessage.EM_MessageText = "";
			Factory.Save();

			var logger = ExecuteMessageProcessor(responseMessage);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
				AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
			});
		}

		const string JsonMessage = @"{
  ""message"": ""Mensagem de exemplo."",
  ""identificacao"": {
    ""numero"": ""24BR00000002090"",
    ""versao"": ""1""
  },
  ""links"": [
    {
      ""_rel"": ""string"",
      ""_href"": ""string"",
      ""_method"": ""GET""
    }
  ],
  ""multiStatus"": [
    {
      ""code"": 201,
      ""message"": ""Mensagem de exemplo."",
      ""identificacao"": {
        ""numeroItem"": ""1""
      },
      ""errors"": [
        {
          ""code"": ""DIMP-ER0004"",
          ""field"": ""numero"",
          ""message"": ""Número da Duimp inválido.""
        }
      ],
      ""links"": [
        {
          ""_rel"": ""string"",
          ""_href"": ""string"",
          ""_method"": ""GET""
        }
      ]
    },
    {
      ""code"": 207,
      ""message"": ""Mensagem de exemplo."",
      ""identificacao"": {
        ""numeroItem"": ""2""
      },
      ""errors"": [
        {
          ""code"": ""DIMP-ER0004"",
          ""field"": ""numero"",
          ""message"": ""Número da Duimp inválido.""
        }
      ],
      ""links"": [
        {
          ""_rel"": ""string"",
          ""_href"": ""string"",
          ""_method"": ""GET""
        }
      ]
    },
    {
      ""code"": 200,
      ""message"": ""Mensagem de exemplo."",
      ""identificacao"": {
        ""numeroItem"": ""3""
      },
      ""errors"": [
        {
          ""code"": ""DIMP-ER0004"",
          ""field"": ""numero"",
          ""message"": ""Número da Duimp inválido.""
        }
      ],
      ""links"": [
        {
          ""_rel"": ""string"",
          ""_href"": ""string"",
          ""_method"": ""GET""
        }
      ]
    },
    {
      ""code"": 201,
      ""message"": ""Mensagem de exemplo."",
      ""identificacao"": {
        ""numeroItem"": ""5""
      },
      ""errors"": [
        {
          ""code"": ""DIMP-ER0004"",
          ""field"": ""numero"",
          ""message"": ""Número da Duimp inválido.""
        }
      ],
      ""links"": [
        {
          ""_rel"": ""string"",
          ""_href"": ""string"",
          ""_method"": ""GET""
        }
      ]
    }
  ]
}
";

		const string JsonMessageEmptyMultiStatus = @"{
  ""message"": ""Mensagem de exemplo."",
  ""identificacao"": {
    ""numero"": ""24BR00000002090"",
    ""versao"": ""1""
  },
  ""links"": [
    {
      ""_rel"": ""string"",
      ""_href"": ""string"",
      ""_method"": ""GET""
    }
  ],
  ""multiStatus"": [ ]
}
";
	}
}

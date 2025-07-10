using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.GB.CDS.Testing.BatchProcessors
{
	sealed class CDSInterchangeExceptionTests : TestCaseWithFactory
	{
		public void TestThrowApplicationWhenDestinationIsEmptyForOutboundInterchange()
		{
			var provider = SetupInterchangeProvider(string.Empty);
			AssertResult(false, messages, provider, EDIInterchange.Status.Failed, new[]
			{
				"\tFailed to create interchange for CDS Message #1 : There is no customs interchange sender id (EM_MessageOwner).",
				"\tFailed to create interchange for CDS Message #2 : There is no customs interchange sender id (EM_MessageOwner).",
			});
		}

		public void TestThrowMessageProcessingExceptionWhenSenderIDIsNotKnown()
		{
			var provider = SetupInterchangeProvider("from");
			AssertResult(false, messages, provider, EDIInterchange.Status.Failed, Array.Empty<string>());
			ErrorReporter.Clear();
		}

		void AssertResult(bool assertException, NonDependentEDIMessageCollection messages, InterchangeProviderBase provider, string statusExpected, string[] messageExpected)
		{
			errorLog.ClearLogs();
			provider.PackCollatedMessagesIntoInterchanges();
			CombineAssertions(() =>
			{
				AssertEquals("Status ", statusExpected, messages[0].EM_Status);
				AssertEquals(statusExpected, messages[1].EM_Status);
				AssertContainsExactElementsInAnyOrder(messageExpected, errorLog.UserLogStrings.ToList<string>());
			});
		}

		InterchangeProviderBase SetupInterchangeProvider(string senderID)
		{
			messages = new NonDependentEDIMessageCollection(Factory);
			errorLog = new LoggingInformation();

			var message1 = CreateAndPopulateMessage("1", senderID);
			var message2 = CreateAndPopulateMessage("2", senderID);
			messages.AddRange(new CDSEDIMessage[] { message1, message2 });
			Factory.Save();
			message1.Reload();
			message2.Reload();

			var provider = new CDSInterchangeProvider(errorLog, messages);
			return provider;
		}

		CDSEDIMessage CreateAndPopulateMessage(string index, string senderID)
		{
			var message = Factory.New<CDSEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			message.EM_MessageType = CDSEDIMessageTypeList.Codes.NewDeclaration;
			message.EM_MessageOwner = senderID;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = "TEST MESSAGE TEXT" + index;
			return message;
		}

		LoggingInformation errorLog;
		NonDependentEDIMessageCollection messages;
	}
}

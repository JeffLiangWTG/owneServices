using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class StandardXMLMessageProcessorTest : ExtendedTestCaseWithFactory
	{
		public void TestProcessNewMessages()
		{
			var messagePKs = new List<ZGuid>();
			var companies = GlbCompany.GetActiveCompanies();
			Assert("Precondition: Should be at least 2 companies to run this test", companies.Length > 1);
			for (int i = 0; i < 2; i++)
			{
				var company = companies[i];
				var branch = company.Branches.Where(x => x.GB_IsActive).Select(x => x.PK.ToGuid()).First();
				using (DisposableEnvironment.ForBranch(branch))
				{
					var message = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, "GOA", DateTime.UtcNow);
					messagePKs.Add(message.PK);
				}
			}

			Factory.Save();

			var notifications = new NotificationBuffer();

			var processor = new Mock<StandardXMLMessageProcessor> { CallBase = true };

			var processedPKs = new List<ZGuid>();

			processor.Setup(m => m.ProcessMessageBatch(It.IsAny<MessageBatch>(), It.IsAny<CancellationToken>()))
				.Callback((MessageBatch messageBatch, CancellationToken token) =>
				{
					AssertEquals("Should be one message in batch", 1, messageBatch.MessagePKs.Count);
					processedPKs.Add(messageBatch.MessagePKs[0]);
				});

			processor.Object.Process(notifications);
			processor.VerifyAll();
			AssertContainsExactElementsInAnyOrder(processedPKs, messagePKs);
		}

		public void TestGetEDIMessagesToProcess()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";

			var company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "BBB";
			var branch2 = company2.Branches.AddNew();
			branch1.GB_Code = "BR2";

			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.CMR, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			var message3 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK);
			var message4 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Recognised, branch2.PK);
			var message5 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK, EDIMessageSubTypeList.Codes.Events);
			var message6 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, EDIMessageSubTypeList.Codes.Events);

			Factory.Save();

			var processor = new StandardXMLMessageProcessor();

			using (branch1.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(1, messagesToProcess.Count);
				AssertEquals(message2.PK, messagesToProcess[0]);
			}

			using (branch2.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(1, messagesToProcess.Count);
				AssertEquals(message3.PK, messagesToProcess[0]);
			}
		}

		public void TestGetEDIMessagesToProcessForInvalidEDIMessages()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAA";
			var branch = company
				.Branches.AddNew();
			branch.GB_Code = "BR1";

			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.CMR, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch.PK);
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch.PK);
			var message3 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch.PK);
			message1.EM_IsActive = false;
			message3.EM_IsActive = false;
			Factory.Save();

			var processor = new StandardXMLMessageProcessor();

			using (branch.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(1, messagesToProcess.Count);
				AssertEquals(message2.PK, messagesToProcess[0]);
			}
		}

		public void TestNewMessageAvailableForInvalidEDIMessages()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "BR1";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "BBB";
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "BR2";

			var message1 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.CMR, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			var message2 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			var message3 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK);
			message1.EM_IsActive = false;
			message2.EM_IsActive = false;
			message3.EM_IsActive = false;
			Factory.Save();

			var processor = new StandardXMLMessageProcessor();
			string[] companyCodes;
			Assert("should return false", !processor.NewMessageAvailable(out companyCodes));
			AssertEquals(0, companyCodes.Length);

			message2.EM_IsActive = true;
			Factory.Save();
			companyCodes = null;
			Assert("should return true", processor.NewMessageAvailable(out companyCodes));
			AssertEquals(1, companyCodes.Length);
			AssertEquals("AAA", companyCodes[0]);

			message3.EM_IsActive = true;
			Factory.Save();
			companyCodes = null;
			Assert("should return true", processor.NewMessageAvailable(out companyCodes));
			AssertEquals(2, companyCodes.Length);
			AssertCollectionContains("AAA", companyCodes);
			AssertCollectionContains("BBB", companyCodes);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Messaging.MessageProcessors.Testing;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class BranchCustomsMessageProcessorTest : TestCaseWithFactory
	{
		public void TestShouldLoadMessageWhenThereIsAnyApplicationCodes()
		{
			var message1 = AddNewMessage("MT1", "AC1");
			var message2 = AddNewMessage("MT1", "AC2");
			var message3 = AddNewMessage("MT1", "AC3");
			Factory.Save();

			var processor = new BranchCustomsMessageProcessorTestClass(new ZString[] { "AC1", "AC3" }, new ZString[] { "AC1" });
			processor.ExecuteBatch();
			AssertMessage(message1, EDIMessage.Status.Received);
			AssertMessage(message2, EDIMessage.Status.Queued);
			AssertMessage(message3, EDIMessage.Status.Failed);
		}

		public void TestShouldNotLoadMessageWhenThereNoApplicationCodes()
		{
			var message1 = AddNewMessage("MT1", "AC1");
			var message2 = AddNewMessage("MT2", "AC2");
			Factory.Save();

			var processor = new BranchCustomsMessageProcessorTestClass(Array.Empty<ZString>());
			processor.ExecuteBatch();
			AssertMessage(message1, EDIMessage.Status.Queued);
			AssertMessage(message2, EDIMessage.Status.Queued);
		}

		class BranchCustomsMessageProcessorTestClass : BranchCustomsMessageProcessor
		{
			public BranchCustomsMessageProcessorTestClass(ZString[] applicationCodes, ZString[] restrictMessageProcessorToApplicationCodes = null)
				: base(applicationCodes, Array.Empty<ZString>())
			{
				this.restrictMessageProcessorToApplicationCodes = restrictMessageProcessorToApplicationCodes?.ToHashSet();
			}
			readonly HashSet<ZString> restrictMessageProcessorToApplicationCodes;

			public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
			{
				var applicationCode = message.EM_ApplicationCode;
				return restrictMessageProcessorToApplicationCodes == null || restrictMessageProcessorToApplicationCodes.Contains(applicationCode) ? new BranchCustomsApplicationTypeMessageProcessorTestHelper(Logger, applicationCode) : null;
			}
		}

		public void TestOrderAndHint()
		{
			var processor = new BranchCustomsMessageProcessorTestHelper(new ZString[] { "ABC" }, new ZString[] { "CDE" });
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum, EM_SystemCreateTimeUtc", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestFilter()
		{
			var branch2 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUSYD";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var message1 = AddNewMessage("MT1", "AC1");
			var message2 = AddNewMessage("MT1", "AC1", EDIMessage.Status.PreProcessedOK);
			var message3 = AddNewMessage("MT1", "AC2");
			var message4 = AddNewMessage("MT2", "AC1");
			var message5 = AddNewMessage("MT3", "AC1");
			var message6 = AddNewMessage("MT1", "AC1");
			message6.EM_GB = branch2.PK;
			var message7 = AddNewMessage("MT1", "AC3");
			Factory.Save();
			var processor = new BranchCustomsMessageProcessorTestHelper(new ZString[] { "AC1", "AC3" }, new ZString[] { "MT1", "MT3" });
			processor.ExecuteBatch();
			AssertMessage(message1, EDIMessage.Status.Received);
			AssertMessage(message2, EDIMessage.Status.Received);
			AssertMessage(message3, EDIMessage.Status.Queued);
			AssertMessage(message4, EDIMessage.Status.Queued);
			AssertMessage(message5, EDIMessage.Status.Received);
			AssertMessage(message6, EDIMessage.Status.Queued);
			AssertMessage(message7, EDIMessage.Status.Received);
		}

		void AssertMessage(EDIMessage message, string status)
		{
			message.Reload();
			AssertEquals("message.EM_Status", status, message.EM_Status);
		}

		EDIMessage AddNewMessage(ZString messageType, ZString applicationCode, string status = EDIMessage.Status.Queued)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = applicationCode;
			message.EM_Status = status;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageType = messageType;
			return message;
		}
	}
}

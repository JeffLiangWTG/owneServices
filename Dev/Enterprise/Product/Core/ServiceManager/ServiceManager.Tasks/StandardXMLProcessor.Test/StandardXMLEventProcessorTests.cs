using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Testing
{
	class StandardXMLEventProcessorTest : ExtendedTestCaseWithFactory
	{
		public void TestProcessNewMessages()
		{
			var messagePKs = new List<ZGuid>();
			var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true))
				.Where(company => company.GC_Code != GlbCompany.DemoCompanyCode && company.HasActiveBranch).ToArray();
			Assert("Precondition: Should be at least 2 companies to run this test", companies.Length > 1);
			for (int i = 0; i < 2; i++)
			{
				var company = companies[i];
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					var message = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, GlbBranch.CurrentBranch.PK, EDIMessageSubTypeList.Codes.Events, DateTime.UtcNow);
					messagePKs.Add(message.PK);
				}
			}

			Factory.Save();

			var notifications = new NotificationBuffer();
			var processor = new Mock<StandardXMLEventProcessor> { CallBase = true };
			var processMessageBatchCallCount = 1;
			processor.Setup(m => m.ProcessMessageBatch(It.IsAny<MessageBatch>(), It.IsAny<CancellationToken>()))
				.Returns((MessageBatch m, CancellationToken c) =>
				{
					if (processMessageBatchCallCount > 0)
					{
						AssertEquals("Should be one message in batch", 1, m.MessagePKs.Count);
						AssertEquals(messagePKs[processMessageBatchCallCount], m.MessagePKs[0]);
						processMessageBatchCallCount--;
					}

					return true;
				});
			var companyCodes = new string[] { GlbCompany.CurrentCompany.GC_Code.ToString() };
			processor.SetupSequence(m => m.NewMessageAvailable(out companyCodes)).Returns(true).Returns(true).Returns(false);

			processor.Object.Process(notifications);

			processor.VerifyAll();
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

			_ = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.CMR, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			_ = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK);
			_ = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK);
			_ = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Recognised, branch2.PK);
			var message5 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch1.PK, EDIMessageSubTypeList.Codes.Events);
			var message6 = CreateMessage(EDIMessageTypeList.Codes.XMS, EDIMessage.ApplicationCodes.XMS, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, branch2.PK, EDIMessageSubTypeList.Codes.Events);

			Factory.Save();

			var processor = new StandardXMLEventProcessor();

			using (branch1.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(1, messagesToProcess.Count);
				AssertEquals(message5.PK, messagesToProcess[0]);
			}

			using (branch2.SetAsTemporaryContext())
			{
				var messagesToProcess = processor.GetEDIMessagePKs();
				AssertEquals(1, messagesToProcess.Count);
				AssertEquals(message6.PK, messagesToProcess[0]);
			}
		}

		public void TestImportEDIMessageShouldNOTCreateBillingLineLinkedToUnexistJobDocAddress()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "STVI0030294";
			CreateEDIMessages(1);
			Factory.Save();

			AssertNoJobDocAddressAndNoBillingLinesLinkedToJobDocAddressInDatabase("Before import: ");

			new StandardXMLEventProcessor().Process(new NotificationBuffer());

			AssertNoJobDocAddressAndNoBillingLinesLinkedToJobDocAddressInDatabase("After import: ");
		}

		#region Help TestImportEDIMessageShouldNOTCreateBillingLineLinkedToUnexistJobDocAddress

		void AssertNoJobDocAddressAndNoBillingLinesLinkedToJobDocAddressInDatabase(string messagePrefix)
		{
			var noOfJobDocAddress = Factory.GetDatabaseCount(typeof(JobDocAddress));
			AssertEquals(AddPrefix(messagePrefix, "No. of JobDocAddress should be"), 0, noOfJobDocAddress);
		}

		string AddPrefix(string prefix, string input)
		{
			return prefix + input;
		}

		#endregion

		#region Implementation

		#region Create Test EDIMessages

		EDIMessage[] CreateEDIMessages(int number)
		{
			var messages = new List<EDIMessage>();

			for (int i = 0; i < number; i++)
			{
				messages.Add(CreateOneEDIMessage());
			}

			return messages.ToArray();
		}

		EDIMessage CreateOneEDIMessage(string messageText = sampleMessageText, string interchangeText = sampleInterchangeText)
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var message = CreateMessage(EDIMessageTypeList.Codes.XMS, ApplicationCodeList.Codes.XMS, EDIMessage.Status.Received, EDIMessage.Status.Queued, currentBranch.PK, EDIMessageSubTypeList.Codes.Events, DateTime.UtcNow);
			message.EM_MessageText = messageText;
			var interchange = CreateInterchange(EDIInterchangeTypeList.Codes.XMS, EDIInterchange.Status.Received);
			interchange.EI_BodyText = interchangeText;
			interchange.ContainedMessages.Add(message);
			return message;
		}

		#region Sample Message Text

		const string sampleMessageText = @"<Event xmlns:ns0=""http://www.edi.com.au/EnterpriseService/"">
			<Source>NACCS</Source>
			<Code>CLR</Code>
			<DateTime>2013-05-30T00:00:00</DateTime>
			<Information>11730813610</Information>
			<ReferenceKeys>
				<ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">STVI0030294</ReferenceKey>
				<ReferenceKey ReferenceKeyCountry=""JP"" ReferenceKeyName=""CustomsEntryNumber"" ReferenceKeyType=""PMT"" ReferenceKeyDateTime=""2013-05-30T00:00:00"">11730813610</ReferenceKey>
			</ReferenceKeys>
		</Event>";

		const string sampleInterchangeText = @"<XmlInterchange xmlns:ns0=""http://www.edi.com.au/EnterpriseService/"">
			<InterchangeInfo>
				<Date>2013-06-03T00:00:00+10:00</Date>
			</InterchangeInfo>
			<Payload>
				<Events>
					<Event>
						<Source>NACCS</Source>
						<Code>CCC</Code>
						<DateTime>2013-05-21T00:00:00</DateTime>
						<Information>51888404250</Information>
						<ReferenceKeys>
							<ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">SNMA0006564</ReferenceKey>
						</ReferenceKeys>
					</Event>
					<Event>
						<Source>NACCS</Source>
						<Code>CLR</Code>
						<DateTime>2013-05-30T00:00:00</DateTime>
						<Information>11730813610</Information>
						<ReferenceKeys>
							<ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">STVI0030294</ReferenceKey>
							<ReferenceKey ReferenceKeyCountry=""JP"" ReferenceKeyName=""CustomsEntryNumber"" ReferenceKeyType=""PMT"" ReferenceKeyDateTime=""2013-05-30T00:00:00"">11730813610</ReferenceKey>
						</ReferenceKeys>
					</Event>
				</Events>
			</Payload>
		</XmlInterchange>";

		#endregion

		#endregion

		#endregion
	}
}

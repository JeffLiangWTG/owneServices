using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	[TestUtcOffset(-3, 0, 0)]
	class BaseOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new BaseOutgoingMessageProcessorForTest(null));
		}

		public void TestWillNotBeProcessed_IfMessageIsNotTransmit()
		{
			var message = CreateMessage("MSG", isTransmit: false);
			Factory.Save();
			AssertThatMessageWillNotBeProcessed(message);
		}

		public void TestWillNotBeProcessed_IfStatusIsNotQueue()
		{
			var message = CreateMessage("MSG", EDIMessage.Status.Pending);
			Factory.Save();
			AssertThatMessageWillNotBeProcessed(message);
		}

		public void TestWillNotBeProcessed_IfHeldDateInAdvance()
		{
			var message = CreateMessage("MSG");
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddHours(1);
			Factory.Save();
			AssertThatMessageWillNotBeProcessed(message);
		}

		public void TestWillNotBeProcessed_IfWrongBranch()
		{
			var message = CreateMessage("MSG");
			var alternateCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var alternateBranch = alternateCompany.Branches[0];
			message.EM_GB = alternateBranch.PK;
			Factory.Save();
			AssertThatMessageWillNotBeProcessed(message);
		}

		public void TestWillNotBeProcessed_IfMessageIsNotActive()
		{
			var message = CreateMessage("MSG");
			message.EM_IsActive = false;
			Factory.Save();
			AssertThatMessageWillNotBeProcessed(message);
		}

		public void TestProcessed()
		{
			var message = CreateMessage("MSG1");
			Factory.Save();
			Process<BaseOutgoingMessageProcessorForTest>();
			message.Reload();
			AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
		}

		public void TestProcessed_Retry3TimesIfFactorySaveFails()
		{
			var message = CreateMessage("MSG1");
			Factory.Save();
			Process<BaseOutgoingMessageProcessorForTestWithExceptionClass>();
			message.Reload();
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
				AssertEquals("Processing Log", "Message fail to saving 3 times.", message.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription)[0].ST_NoteText);
			});
		}

		public void TestProcessed_FactorySaveConcurrencyErrors_Failed()
		{
			var message = CreateMessage("MSG1");
			Factory.Save();

			var notificationHandler = new OrgCompanyDataTest.NotificationHandlerForTest();
			NotificationHandler.Instance = notificationHandler;
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				var processor = new BaseOutgoingMessageProcessorForTestWithZSaveConcurrencyException(new LoggingInformation());
				processor.Increment = 1;
				AssertNoExceptionThrown("1: No Exception", () => processor.ProcessMessage(CancellationToken.None));
				AssertNotContains("1: No concurrency message", @"While you have been working with this form, another user has made changes.", notificationHandler.Message);

				var factory = new BusinessObjectFactory();
				var messageReload = factory.Load<EDIMessage>(message.PK);
				var dummyBizoReload = factory.Load<DummyBusinessObject>(processor.DummyBizo.PK);
				AssertEquals("1: EM_Status set Failed after failing to factory.save for 3 times", EDIMessage.Status.Failed, messageReload.EM_Status);
				AssertEquals("1: The Z0_Description change not set due to failed save", "hello5", dummyBizoReload.Z0_Description);
			});
		}

		public void TestProcessed_FactorySaveConcurrencyErrors_Sent()
		{
			var message = CreateMessage("MSG1");
			Factory.Save();

			var notificationHandler = new OrgCompanyDataTest.NotificationHandlerForTest();
			NotificationHandler.Instance = notificationHandler;
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				var processor = new BaseOutgoingMessageProcessorForTestWithZSaveConcurrencyException(new LoggingInformation());
				processor.Increment = 3;
				AssertNoExceptionThrown("2: No Exception", () => processor.ProcessMessage(CancellationToken.None));
				AssertNotContains("2: No concurrency message", @"While you have been working with this form, another user has made changes.", notificationHandler.Message);

				var factory = new BusinessObjectFactory();
				var messageReload = factory.Load<EDIMessage>(message.PK);
				var dummyBizoReload = factory.Load<DummyBusinessObject>(processor.DummyBizo.PK);
				AssertEquals("2: EM_Status will not set to failed when the first factory.save failed due to concurrency issue and can save successfully at the second time", EDIMessage.Status.Sent, messageReload.EM_Status);
				AssertEquals("2: The Z0_Description change set as no concurrency issue on the second save", "changeValue", dummyBizoReload.Z0_Description);
			});
		}

		public void TestMaximumRows()
		{
			using (BaseOutgoingMessageProcessorTestHelper.SetMessagesPerInterchange(3))
			{
				CreateMessage("MSG1");
				CreateMessage("MSG2");
				CreateMessage("MSG3");
				CreateMessage("MSG4");
				CreateMessage("MSG5");
				Factory.Save();
				var logger = new LoggingInformation();
				Process<BaseOutgoingMessageProcessorForTest>(logger);
				var logs = string.Join("\r\n", logger.DebugLogStrings.ToList<string>());
				AssertEquals(@"	3 message(s) have been processed.
	2 message(s) have been processed.", logs);
			}
		}

		public void TestOrderAndHint()
		{
			using (var processor = new OutgoingMessageProcessorTestClass(new LoggingInformation()))
			{
				processor.EnableQueryLogging = true;
				processor.ProcessMessage(CancellationToken.None);

				var query = processor.Factories.SelectMany(f => f.TableSelects.Single(s => s.TableName == EDIMessageSchema.Constants.TableName).Queries).Single();
				AssertContains("ORDER BY EM_SystemCreateTimeUtc,EM_MessageNum", query.Query);
				AssertContains("WITH (INDEX(NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum))", query.Query);
			}
		}

		public void TestValidBranchesForMessageFilter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "Z1Z";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "Z1Z";
			branch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.Branches.Add(branch);

			Factory.Save();

			using (branch.SetAsTemporaryContext())
			using (var processor = new OutgoingMessageProcessorTestClass(new LoggingInformation()))
			{
				var messageInCurrentBranch = CreateMessage("MSG1");
				messageInCurrentBranch.EM_ApplicationCode = "TST";
				messageInCurrentBranch.EM_GB = branch.PK;
				Factory.Save();

				processor.ProcessMessage(CancellationToken.None);

				messageInCurrentBranch.Reload();
				AssertEquals("Should process the message as the branch is created at before.", EDIMessage.Status.Sent, messageInCurrentBranch.EM_Status);

				var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				var companyInAnotherFactory = anotherFactory.Load<GlbCompany>(branch.GB_GC);
				var newBranchInAnotherFactory = companyInAnotherFactory.Branches.AddNew();
				newBranchInAnotherFactory.FillWithValidTestData();
				newBranchInAnotherFactory.GB_Code = "Z2Z";

				anotherFactory.Save();

				AssertCollectionNotContains("Should not contains the new branch as it's created in a different thread.", newBranchInAnotherFactory.PK, branch.Company.Branches.GetPKs());

				var messageInNewBranch = CreateMessage("MSG2");
				messageInNewBranch.EM_ApplicationCode = "TST";
				messageInNewBranch.EM_GB = newBranchInAnotherFactory.PK;

				Factory.Save();

				processor.ProcessMessage(CancellationToken.None);

				messageInNewBranch.Reload();
				AssertEquals("Should process the message as ValidBranchesForFilter should get these latest branches from database.", EDIMessage.Status.Sent, messageInNewBranch.EM_Status);
			}
		}

		EDIMessage CreateMessage(string messageNum, string status = EDIMessage.Status.Queued, bool isTransmit = true)
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected()
					   .Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			mockMessage.CallBase = true;
			EDIMessage result = mockMessage.Object;
			result.EM_ReceiveTransmit = isTransmit ? EDIMessage.Direction.Transmit : EDIMessage.Direction.Receive;
			result.EM_MessageNum = messageNum;
			result.EM_MessageText = "MESSAGE TEXT FOR " + messageNum;
			result.EM_Status = status;
			return result;
		}

		static void AssertThatMessageWillNotBeProcessed(EDIMessage message)
		{
			var originalStatus = message.EM_Status;
			Process<BaseOutgoingMessageProcessorForTest>();
			message.Reload();
			AssertEquals(originalStatus, message.EM_Status);
		}

		static void Process<T>(LoggingInformation logger = null) where T : BaseOutgoingMessageProcessor
		{
			((T)Activator.CreateInstance(typeof(T), logger ?? new LoggingInformation())).ProcessMessage(CancellationToken.None);
		}
	}
}

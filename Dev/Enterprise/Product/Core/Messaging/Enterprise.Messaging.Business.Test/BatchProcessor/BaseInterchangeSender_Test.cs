using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business.Testing
{
	public class BaseInterchangeSender_Test : TestCaseWithFactory
	{
		public void TestAlwaysGetLatestBranchesWhenRunning()
		{
			var factory = NewFactory();

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.Branches.DeleteAll();

			var defaultBranch = company.Branches.AddNew();
			defaultBranch.FillWithValidTestData();

			var message1 = CreateTestMessage(factory);
			message1.EM_GB = defaultBranch.PK;

			factory.Save();

			using (DisposableEnvironment.ForCompany(company.GC_Code))
			{
				var sender = new BaseInterchangeSenderForTest();
				var query = sender.GetMessageQueryExposed();

				factory = NewFactory();
				factory.RefreshEnabled = false;

				var messages = factory.Load<EDIMessage>(query).Select(c => c.PK);
				AssertCollectionContains("Should contains the PK of message1 as its branch is belong to the current company.", message1.PK, messages);

				var anotherFactory = NewFactory();
				anotherFactory.RefreshEnabled = false;

				var companyInAnotherFactory = anotherFactory.Load<GlbCompany>(company.PK);

				var newBranch = companyInAnotherFactory.Branches.AddNew();
				newBranch.FillWithValidTestData();

				var message2 = CreateTestMessage(anotherFactory);
				message2.EM_GB = newBranch.PK;

				anotherFactory.Save();

				query = sender.GetMessageQueryExposed();
				messages = factory.Load<EDIMessage>(query).Select(c => c.PK);
				AssertContainsExactElementsInAnyOrder("Should contains the PK of message2 as its branch is also belong to the current company.", new[] { message1.PK, message2.PK }, messages);
			}
		}

		public void TestOrderAndHint()
		{
			var sender = new BaseInterchangeSenderForTest();
			var query = sender.GetMessageQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_MessageNum", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc", hint.IndexName);
		}

		public void TestExecuteCannotTakeLongerThanSpecified()
		{
			BaseInterchangeSender.TimedOutBehaviour.Value = BaseInterchangeSender.TimedOutBehaviourForTest.DEFAULT;
			var timedSender = new BaseInterchangeSenderTimed();
			timedSender.ExecuteBatch();
			AssertEquals(1, timedSender.SendOutboundInterchangeCallCount);
		}

		public void TestDontReportExcessDeveloperExceptions()
		{
			BaseInterchangeSenderForTest sender = new BaseInterchangeSenderForTest();
			sender.PackageMessagesIntoInterchangesImplementation = messages => { throw new InvalidOperationException("exception for test"); };

			CreateTestMessage(Factory);
			Factory.Save();

			AssertExceptionThrown(typeof(InvalidOperationException), "exception for test", sender.ExecuteBatch);
		}

		public void TestPackageMessagesIntoInterchangesCalledEventIfThereisNoMessages()
		{
			bool packageMessagesIntoInterchangesCalled = false;

			BaseInterchangeSenderForTest sender = new BaseInterchangeSenderForTest();
			sender.PackageMessagesIntoInterchangesImplementation = messages => packageMessagesIntoInterchangesCalled = true;
			sender.ExecuteBatch();
			AssertEquals(string.Empty, JoinUserLogStrings(sender.Logger.UserLogStrings));
			AssertEquals("PackageMessagesIntoInterchanges should be called even if there are no messages.", true, packageMessagesIntoInterchangesCalled);

			CreateTestMessage(Factory);
			Factory.Save();

			packageMessagesIntoInterchangesCalled = false;
			sender.PackageMessagesIntoInterchangesImplementation = messages => packageMessagesIntoInterchangesCalled = true;
			sender.ExecuteBatch();
			AssertContains("1 message(s) prepared for sending.", JoinUserLogStrings(sender.Logger.UserLogStrings));
			AssertEquals("PackageMessagesIntoInterchanges should be called.", true, packageMessagesIntoInterchangesCalled);

			packageMessagesIntoInterchangesCalled = false;
			sender.PackageMessagesIntoInterchangesImplementation = messages => { packageMessagesIntoInterchangesCalled = true; throw new InterchangePreparationException("Exception for test."); };
			AssertNoExceptionThrown("InterchangePreparationException should be caught.", sender.ExecuteBatch);
			AssertContains("Exception for test.", JoinUserLogStrings(sender.Logger.UserLogStrings));
			AssertNotContains("1 message(s) prepared for sending.", JoinUserLogStrings(sender.Logger.UserLogStrings));
			AssertEquals("PackageMessagesIntoInterchanges should be called even if there are no messages.", true, packageMessagesIntoInterchangesCalled);
		}

		public void TestPackOnlyActiveMessagesIntoInterchange()
		{
			var sender = new BaseInterchangeSenderForTest();
			CreateTestMessage(Factory);
			var inactiveMessage = CreateTestMessage(Factory);
			inactiveMessage.EM_IsActive = false;
			Factory.Save();
			sender.ExecuteBatch();
			AssertContains("1 message(s) prepared for sending.", JoinUserLogStrings(sender.Logger.UserLogStrings));
		}

		public void TestHandleSaveException()
		{
			BaseInterchangeSenderForTest sender = new BaseInterchangeSenderForTest();
			sender.PackageMessagesIntoInterchangesImplementation = messages =>
			{
				DummyDependantBusinessObject dummy = messages.Factory.New<DummyDependantBusinessObject>();
				dummy.ZD1_Z0 = Guid.NewGuid();
			};

			CreateTestMessage(Factory);
			CreateTestMessage(Factory);
			Factory.Save();

			AssertNoExceptionThrown(sender.ExecuteBatch);
			string log = JoinUserLogStrings(sender.Logger.UserLogStrings);
			AssertContains("The DummyDependentBizo cannot be inserted/updated, requires a reference to a valid DummyBizo.", log);
			AssertContains("** Error Saving Record **", log);
		}

		public void TestNotificationHandler()
		{
			BaseInterchangeSenderForTest sender = new BaseInterchangeSenderForTest();
			INotificationHandler notificationHandler = sender;

			notificationHandler.ReportError("error", "error caption");
			string log = JoinUserLogStrings(sender.Logger.UserLogStrings);
			AssertContains("error", log);

			notificationHandler.ReportInformation("information", "information caption");
			log = JoinUserLogStrings(sender.Logger.UserLogStrings);
			AssertContains("error", log);
			AssertContains("information", log);
		}

		#region Implementation

		class BaseInterchangeSenderTimed : BaseInterchangeSender
		{
			public override TimeSpan TimeToSpendProcessing
			{
				get { return new TimeSpan(0, 0, 3); }
			}

			protected override int SendOutboundInterchange(string applicationCode, int interchangeNum, int numberOfInterchangesSent, CancellationToken token)
			{
				SendOutboundInterchangeCallCount++;
				Thread.Sleep(TimeToSpendProcessing.Add(new TimeSpan(0, 0, 2)));
				return -1;
			}

			protected override bool SendInt(EDIInterchange interchange)
			{
				return true;
			}

			protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
			{
			}

			protected override void SendOutboundInterchanges(CancellationToken token)
			{
				var factory = new BusinessObjectFactory();
				EDIInterchange interchange0 = factory.New<EDIInterchange>();
				EDIInterchange interchange1 = factory.New<EDIInterchange>();

				SendableInterchanges = new BusinessObject[] { interchange0, interchange1 };
				var interchangesSent = 5;
				SendOutboundInterchange(EDIMessage.ApplicationCodes.CMR, 0, interchangesSent, token);
			}

			public int SendOutboundInterchangeCallCount;
		}

		class BaseInterchangeSenderForTest : BaseInterchangeSender
		{
			public const string TestApplicationCode = "TST";

			protected override bool SendInt(EDIInterchange interchange)
			{
				return true;
			}

			protected override void SendOutboundInterchanges(CancellationToken token)
			{
				SendOutboundInterchanges(TestApplicationCode, token);
			}

			public Action<NonDependentEDIMessageCollection> PackageMessagesIntoInterchangesImplementation { get; set; }

			protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
			{
				if (PackageMessagesIntoInterchangesImplementation != null)
				{
					PackageMessagesIntoInterchangesImplementation(messages);
				}
			}

			public ZQuery GetMessageQueryExposed() => base.GetMessageQuery(new[] { TestApplicationCode }, false);
		}

		EDIMessage CreateTestMessage(BusinessObjectFactory factory)
		{
			var message = factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			message.EM_Status = EDIInterchange.Status.Queued;
			message.EM_ApplicationCode = BaseInterchangeSenderForTest.TestApplicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			message.EM_HeldUntilDate = ZDateTime.Empty;
			message.EM_GB = GlbBranch.CurrentBranch.PK;

			return message;
		}

		string JoinUserLogStrings(StringCollection userLogStrings)
		{
			return string.Join("", userLogStrings.Cast<string>().ToArray());
		}

		#endregion
	}
}

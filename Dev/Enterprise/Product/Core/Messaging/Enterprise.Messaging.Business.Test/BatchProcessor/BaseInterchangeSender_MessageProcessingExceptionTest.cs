using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Messaging.Business
{
	sealed class BaseInterchangeSender_MessageProcessingExceptionTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestCommitPerInterchangeTrue()
		{
			EDIInterchange interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ApplicationCode = "XXX";
			interchange1.EI_ReceiveTransmit = "TRX";
			interchange1.EI_Status = "QUE";
			interchange1.EI_From = "FROM";
			interchange1.EI_To = "TO";
			EDIInterchange interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "XXX";
			interchange2.EI_ReceiveTransmit = "TRX";
			interchange2.EI_Status = "QUE";
			interchange2.EI_From = "FROM";
			interchange2.EI_To = "TO";
			Factory.Save();

			var sender = new TestSender(null, false, false);
			sender.CallBaseSendOutboundInterchange = true;
			sender.SendableInterchanges = new BusinessObject[] { interchange1, interchange2 };
			sender.CommitPerInterchange = true;
			var sentCount = 0;
			sentCount = sender.TestSendOutboundInterchange("XXX", 0, sentCount, CancellationToken.None);
			AssertEquals("precondition", 2, sentCount);
			AssertEquals(2, sender.CommitCount);
		}

		public void TestCommitPerInterchangeFalse()
		{
			EDIInterchange interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ApplicationCode = "XXX";
			interchange1.EI_ReceiveTransmit = "TRX";
			interchange1.EI_Status = "QUE";
			interchange1.EI_From = "FROM";
			interchange1.EI_To = "TO";
			EDIInterchange interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "XXX";
			interchange2.EI_ReceiveTransmit = "TRX";
			interchange2.EI_Status = "QUE";
			interchange2.EI_From = "FROM";
			interchange2.EI_To = "TO";
			Factory.Save();

			var sender = new TestSender(null, false, false);
			sender.CallBaseSendOutboundInterchange = true;
			sender.SendableInterchanges = new BusinessObject[] { interchange1, interchange2 };
			sender.CommitPerInterchange = false;
			var sentCount = 0;
			sentCount = sender.TestSendOutboundInterchange("XXX", 0, sentCount, CancellationToken.None);
			AssertEquals("precondition", 2, sentCount);
			AssertEquals(1, sender.CommitCount);
		}

		public void TestSendIntToFile()
		{
			var sender = new TestSender("", false, false);
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			using (TempFile tempFile = TempFile.New())
			{
				sender.SendIntToFile(interchange, Path.GetDirectoryName(tempFile.Filename), Path.GetFileName(tempFile.Filename));
				AssertEquals("HEADERBODYFOOTER", File.ReadAllText(tempFile.Filename));
			}
		}

		public void TestHandleExceptionForExecuteForEmailOption()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			var sender = new TestSender("hehe", true, false);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("Current count", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			sender.Logger.ClearLogs();

			sender.TestExecute();
			AssertEquals("One email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("One new log", 1, sender.Logger.UserLogStrings.Count);
			Assert(sender.Logger.UserLogStrings[0].Contains("hehe"));
		}

		public void TestHandleExceptionForExecuteForDeveloperInfo()
		{
			var sender = new TestSender("hehe", false, true);
			ErrorReporter.Clear();
			try
			{
				sender.TestExecute();
				AssertEquals("Key", "hehe", ErrorReporter.LastKeyReported.Trim());
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestLogExceptionsInPrepareInterchanges()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "AR1";
			interchange.EI_ReceiveTransmit = "TRX";
			interchange.EI_Status = "QUE";
			interchange.EI_From = "ME";
			interchange.EI_To = "YOU";
			Factory.Save();

			var sender = new TestSender(null, false, false);
			sender.SendableInterchanges = new BusinessObject[] { interchange };
			sender.CallBaseSendOutboundInterchange = true;
			sender.CommitPerInterchange = true;
			sender.PackageMessagesIntoInterchangesImplementation = messages =>
			{
				var message = messages.Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
				var dataException = new ZDataException(new Exception(), ((IBusinessObjectInternals)message).Row, Db.Connection);
				dataException.SetFriendlyMessageForTest("Need this to prevent exception being rethrow as RethrownByExceptionHandlerException(ex)");
				message.Saving += new SavingEventHandler<EDIMessage>((m) => throw new ZSaveException(dataException, Factory));
			};

			sender.TestSendOutboundInterchange("AR1", 0, 0, CancellationToken.None);
			AssertContains("** Error Saving Record **", sender.JoinUserLogStrings());
		}

		class TestSender : BaseInterchangeSender
		{
			public TestSender(string exceptionMessage, bool shouldSendEmail, bool shouldSendDeveloperInfo)
			{
				ExceptionMessage = exceptionMessage;
				ShouldSendEmail = shouldSendEmail;
				ShouldSendDeveloperInfo = shouldSendDeveloperInfo;
			}

			public bool CallBaseSendOutboundInterchange;
			readonly string ExceptionMessage;
			readonly bool ShouldSendEmail;
			readonly bool ShouldSendDeveloperInfo;

			protected override void SendOutboundInterchanges(CancellationToken token)
			{
				if (ExceptionMessage != null)
				{
					throw new MessageProcessingException(ExceptionMessage, "", ShouldSendEmail, ShouldSendDeveloperInfo);
				}
			}

			protected override int SendOutboundInterchange(string applicationCode, int interchangeNum, int numberOfInterchangesSent, CancellationToken token = new CancellationToken())
			{
				if (CallBaseSendOutboundInterchange)
				{
					return base.SendOutboundInterchange(applicationCode, interchangeNum, numberOfInterchangesSent, token);
				}
				return numberOfInterchangesSent;
			}

			protected override void Commit(BusinessObjectFactory factory)
			{
				base.Commit(factory);
				CommitCount++;
			}

			protected override bool SendInt(EDIInterchange interchange)
			{
				return true;
			}

			public int CommitCount;

			public void TestExecute() => Execute();
			public int TestSendOutboundInterchange(string applicationCode, int interchangeNum, int numberOfInterchangesSent, CancellationToken token = new CancellationToken()) => SendOutboundInterchange(applicationCode, interchangeNum, numberOfInterchangesSent, token);

			public Action<NonDependentEDIMessageCollection> PackageMessagesIntoInterchangesImplementation { get; set; }

			protected override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
			{
				PackageMessagesIntoInterchangesImplementation?.Invoke(messages);
			}

			public string JoinUserLogStrings()
			{
				return string.Join("", Logger?.UserLogStrings?.Cast<string>().ToArray());
			}
		}
	}
}

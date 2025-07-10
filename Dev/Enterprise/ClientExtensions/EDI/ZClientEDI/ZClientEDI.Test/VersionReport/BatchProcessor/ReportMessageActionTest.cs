using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.VersionReporting.BatchProcessor.Testing
{
	public class ReportMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
		{
			var stream = new MemoryStream(Encoding.ASCII.GetBytes("<foo />"));
			var interchange = SystemMessage.CreateSecureInterchange(Factory, "blah", stream);
			var ediMessage = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var action = new ReportMessageActionForTest(new BusinessObjectFactoryProvider(Factory));
			List<ITransactionParticipant> participants;
			((IMessageAction)action).ExecuteAction(ediMessage, null, out participants);
			AssertEquals("ProcessCalls", 1, action.Processor.ProcessCalls);
			AssertEquals("message", "<foo />", action.Processor.AttachmentText);
		}

		class ReportMessageActionForTest : ReportMessageAction
		{
			public ReportMessageActionForTest(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
			{
			}

			public EmailAttachmentProcessorForTest Processor;
			internal override IEmailAttachmentProcessor CreateProcessor(string from, INotifications notifications)
			{
				if (Processor == null)
				{
					Processor = new EmailAttachmentProcessorForTest(from, notifications);
				}

				return Processor;
			}
		}

		class EmailAttachmentProcessorForTest : IEmailAttachmentProcessor
		{
			public string AttachmentText;
			public int ProcessCalls;
			public string From;
			public INotifications Notifications;
			public EmailAttachmentProcessorForTest(string from, INotifications notifications)
			{
				From = from;
				Notifications = notifications;
			}

			public void Process(string attachmentText)
			{
				++ProcessCalls;
				AttachmentText = attachmentText;
			}
		}
	}
}

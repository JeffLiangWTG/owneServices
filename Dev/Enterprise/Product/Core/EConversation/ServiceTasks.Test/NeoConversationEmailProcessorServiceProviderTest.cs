using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.EConversation.ServiceTasks.Testing
{
	[TestedType(typeof(NeoConversationEmailProcessorServiceProvider))]
	class NeoConversationEmailProcessorServiceProviderTest : EmailProcessorServiceProviderTestCase<NeoConversationEmailProcessorServiceProvider>
	{
		[TestDate(2005, 11, 1)]
		public void TestRunTask()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(r => r.AddEmailBundle(email1, email2));

			CreateServiceTask(emailReaderFactory).RunTask();

			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:0 From:<test@cargowise.com> Subject:RE: New messages in Business Object ABC123", null),
				new LogForTest(LogType.Information, "Email:2 Attachments:0 From:<grandma@yahoo.com> Subject:FW: RE: LOL RE: RE: FW: ROFLMAO RE: YOU WON'T BELIEVE THIS", null),
				new LogForTest(LogType.Information, "Processed 2 emails", null),
				new LogForTest(LogType.Information, "2 emails read, processed and deleted", null));

			mockEmailProcessor1.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			mockEmailProcessor1.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			mockEmailProcessor2.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			mockEmailProcessor2.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
		}

		public void TestRunTask_IncompleteMailboxSettings()
		{
			Env.Registry.MailServer = "";
			Env.Registry.MailboxUserName = "";
			GlowRegistry.Instance.NeoConversationsMailBox.Server = "";
			GlowRegistry.Instance.NeoConversationsMailBox.UserName = "";

			CreateServiceTask(new EmailReaderFactory()).RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, "Mail server is not configured", null),
				new LogForTest(LogType.Information, "Mail account username is not configured", null));
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "NCV", hostedServiceAttribute.Code);
				AssertEquals("Description", "Neo eConversations Email Processor", hostedServiceAttribute.Description);
				AssertEquals("Category", "MAI", hostedServiceAttribute.Category);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestReturnsEarlyIfEmailProcessorReturnsTrue()
		{
			mockEmailProcessor1.Setup(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>())).Returns(true);
			mockEmailProcessor1.Setup(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>())).Returns(false);
			mockEmailProcessor2.Setup(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>())).Returns(false);
			mockEmailProcessor2.Setup(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>())).Returns(true);
			var emailReaderFactory = new EmailReaderFactoryForTest(r => r.AddEmailBundle(email1, email2));
			CreateServiceTask(emailReaderFactory).RunTask();

			mockEmailProcessor1.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			mockEmailProcessor1.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			mockEmailProcessor2.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email1.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Never);
			mockEmailProcessor2.Verify(x => x.CreateAndProcessMailItem(It.Is<Email>(y => y.Subject == email2.Subject), It.IsAny<NeoConversationEmailProcessorServiceProvider>()), Times.Once);
			Assert("Not an empty test", true);
		}

		protected override IRegistryItem EnableVerboseModeRegistryItem => GlowRegistry.Instance.NeoEnableVerboseModeOnEmailProcessors;

		protected override NeoConversationEmailProcessorServiceProvider CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new NeoConversationEmailProcessorServiceProvider(
				emailReaderFactory,
				GlowRegistry.Instance.NeoConversationsMailBox);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();

			GlowRegistry.Instance.NeoEnableConversations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.NeoConversationsMailBox.Server = "TestMailServer";
			GlowRegistry.Instance.NeoConversationsMailBox.UserName = "NeoConversations";

			mockEmailProcessor1 = new Mock<IBusinessObjectEmailProcessor>();
			mockEmailProcessor2 = new Mock<IBusinessObjectEmailProcessor>();
			mockEmailProcessor1.Setup(x => x.CreateAndProcessMailItem(null, It.IsAny<NeoConversationEmailProcessorServiceProvider>())).Throws(new ArgumentNullException("email"));

			var neoEmailProcessorList = new ArrayList { mockEmailProcessor1.Object, mockEmailProcessor2.Object };
			ObjectFactory.Substitute("NeoEmailProcessors", neoEmailProcessorList);
		}

		readonly Email email1 = new EmailBuilderForTesting().From("test@cargowise.com").Subject("RE: New messages in Business Object ABC123").GetEmail();
		readonly Email email2 = new EmailBuilderForTesting().From("grandma@yahoo.com").Subject("FW: RE: LOL RE: RE: FW: ROFLMAO RE: YOU WON'T BELIEVE THIS").GetEmail();
		Mock<IBusinessObjectEmailProcessor> mockEmailProcessor1;
		Mock<IBusinessObjectEmailProcessor> mockEmailProcessor2;
	}
}

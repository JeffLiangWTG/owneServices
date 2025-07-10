using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MailProcessor.Testing
{
	[TestedType(typeof(InboundMailTask))]
	class InboundMailTaskTest : ServiceTaskTestCase<InboundMailTask>
	{
		public void TestDownloadByGraphDownloader()
		{
			using (Env.Registry.RawRegistry.Ms365OAuth2TokenForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new Ms365OAuth2Token { Identifier = "Id", Token = new byte[] { 1, 2 }, User = "email@test.com" }))
			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var inboundMailTask = new InboundMailTask();
				InitialiseTaskSchedule(inboundMailTask);

				var query = new ZQuery();
				query.AddToFilter(MailDBItemsSchema.MI_From, "Babai <Babai@mabai.info>");
				AssertEquals(0, Factory.Load<MailItem>(query).Length);

				RunTaskSchedule(inboundMailTask);

				var serviceLogMessage = inboundMailTask.ServiceLogger.ToString();
#if NET48
				AssertContains("Error|Error downloading email from mail server - One or more errors occurred. --> No ClientId was specified. ", serviceLogMessage);

#else
				AssertContains("Error|Error downloading email from mail server - One or more errors occurred. (No ClientId was specified. ) --> No ClientId was specified. ", serviceLogMessage);
#endif
				AssertNotContains("As a result of incomplete mail configurations in the Registry, the Inbound Mail Service Task has been paused and has been rescheduled to run again on", serviceLogMessage);
			}

			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var inboundMailTask = new InboundMailTask();
				InitialiseTaskSchedule(inboundMailTask);

				var query = new ZQuery();
				query.AddToFilter(MailDBItemsSchema.MI_From, "Babai <Babai@mabai.info>");
				AssertEquals(0, Factory.Load<MailItem>(query).Length);

				RunTaskSchedule(inboundMailTask);

				var serviceLogMessage = inboundMailTask.ServiceLogger.ToString();
#if NET48
				AssertContains("Error|Error downloading email from mail server - One or more errors occurred. --> Value cannot be null.", serviceLogMessage);

#else
				AssertContains("Error|Error downloading email from mail server - One or more errors occurred. (Value cannot be null. (Parameter 'clientSecret')) --> Value cannot be null. (Parameter 'clientSecret')\r\n", serviceLogMessage);
#endif
				AssertNotContains("As a result of incomplete mail configurations in the Registry, the Inbound Mail Service Task has been paused and has been rescheduled to run again on", serviceLogMessage);

				using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
				using (Env.Registry.RawRegistry.Ms365AppSecretForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "appsecret"))
				{
					RunTaskSchedule(inboundMailTask);
#if NET48
					AssertContains("Error|Error downloading email from mail server - One or more errors occurred. --> No ClientId was specified. ", inboundMailTask.ServiceLogger.ToString());

#else
					AssertContains("Error|Error downloading email from mail server - One or more errors occurred. (No ClientId was specified. ) --> No ClientId was specified. ", inboundMailTask.ServiceLogger.ToString());
#endif
				}
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "IMS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Inbound Mail Service", hostedServiceAttribute.Description);
				AssertEquals("Category", "MAI", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("MaximumPeriod", "1hour", hostedServiceAttribute.MaximumPeriod);
			});
		}

		[TestDate(2024, 1, 1, 0, 0, 0)]
		public void TestRunTask_MailServerMustNotBeEmpty_WhenGraphAPIIsNotEnabled()
		{
			var serviceManagerGovernorMock = new Mock<IServiceManagerGovernor>();
			var serviceManagerQuerierMock = new Mock<IServiceManagerQuerier>();

			serviceManagerQuerierMock
				.Setup(querier => querier.CheckStateOfNamedServiceTask(It.IsAny<string>()))
				.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			schedule.S5_ScheduleType = "IMS";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var inboundMailTask = new InboundMailTask();
			InitialiseTaskSchedule(inboundMailTask);

			using (ObjectFactory.Substitute(serviceManagerGovernorMock.Object))
			using (ObjectFactory.Substitute(serviceManagerQuerierMock.Object))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				RunTaskSchedule(inboundMailTask);
				AssertEquals($@"Error|As a result of incomplete mail configurations in the Registry, the Inbound Mail Service Task has been paused and has been rescheduled to run again on 08-Jan-24 00:00:00.
Please verify the mail configuration settings in the Registry at Registry -> {RawDataRegistry.Instance.MailServer.GetLocation()}.
Once the mail configuration has been set, the Service Task will automatically resume running after {RegistryRefresh.FrequencyInSeconds} seconds.
", inboundMailTask.ServiceLogger.ToString());

				serviceManagerGovernorMock.Verify(governor => governor.SetServiceTaskNextRuntime("IMS", new DateTimeOffset(2024, 1, 8, 0, 0, 0, TimeSpan.Zero)));
			}
		}

		public void TestDownloadEmail()
		{
			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com"))
			{
				InboundMailTask inboundMailTask = new InboundMailTaskForTesting();
				InitialiseTaskSchedule(inboundMailTask);

				var query = new ZQuery();
				query.AddToFilter(MailDBItemsSchema.MI_From, "Babai <Babai@mabai.info>");
				AssertEquals(0, Factory.Load<MailItem>(query).Length);

				RunTaskSchedule(inboundMailTask);
				AssertEquals(5, Factory.Load<MailItem>(query).Length);
			}
		}

		public void TestIContinuousServiceTask()
		{
			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com"))
			{
				InboundMailTaskForTesting inboundMailTask = new InboundMailTaskForTesting();
				inboundMailTask.EmailDownloaded +=
					delegate(string _, ref string email, ref bool _)
					{
						if (email.EndsWith("Email 2"))
						{
							inboundMailTask.CancellationTokenSource.Cancel();
						}
					};
				InitialiseTaskSchedule(inboundMailTask);

				var query = new ZQuery();
				query.AddToFilter(MailDBItemsSchema.MI_From, "Babai <Babai@mabai.info>");
				AssertEquals(0, Factory.Load<MailItem>(query).Length);

				RunTaskSchedule(inboundMailTask);
				AssertEquals(2, Factory.Load<MailItem>(query).Length);
			}
		}

		public void TestDownloadEmailWithInvalidCredentialException()
		{
			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com"))
			{
				var inboundMailTask = new InboundMailTaskWithExceptionForTesting(new InvalidCredentialException("InvalidCredentialException ABC1"));
				InitialiseTaskSchedule(inboundMailTask);
				RunTaskSchedule(inboundMailTask);

				AssertEquals("Error|InvalidCredentialException ABC1\r\n", inboundMailTask.ServiceLogger.ToString());
			}
		}

		public void TestDownloadEmailWithRegistryJsonException()
		{
			ErrorReporter.Clear();

			TestConnection.ExecuteNonQuery(@"
IF EXISTS (select 1 from dbo.StmData where SD_Name = 'Ms365OAuth2TokenForIncoming')
BEGIN
	UPDATE dbo.StmData
	SET SD_BinaryValue = 01010100010001010101001101010100
	WHERE SD_Name = 'Ms365OAuth2TokenForIncoming'
END
ELSE
BEGIN
	INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled, SD_PreserveTestValue, SD_SystemCreateUser,
	SD_SystemCreateTimeUtc, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc)
	VALUES(NEWID(), 'Ms365OAuth2TokenForIncoming', 'BIN',
	01010100010001010101001101010100,
	1, 0, 1, 'E', GETDATE(), 'E', GETDATE())
END");

			using (Globals.SetIsUserInteractiveForTest(false))
			using (Env.Registry.RawRegistry.MailServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com"))
			using (Env.Registry.RawRegistry.MailboxUserName.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "test@test.com"))
			using (Env.Registry.RawRegistry.UseGraphApiForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365))
			{
				var inboundMailTask = new InboundMailTask();
				InitialiseTaskSchedule(inboundMailTask);
				inboundMailTask.RunTask();

				var logger = (inboundMailTask.ServiceLogger as TestServiceLogger);
				AssertContains("Please check the registry and renew the corresponding OAuth 2.0 settings.", logger.ToString());
			}
			ErrorReporter.Clear();
		}
		
		[TestDate(2024, 1, 1, 0, 0, 0)]
		public void TestDownloadEmail_WhenMailServerPortIsEmpty()
		{
			var inboundMailTask = new InboundMailTask();
			InitialiseTaskSchedule(inboundMailTask);

			Env.Registry.MailServer = "syd-smai-3.corporate.cargowise.com";
			Env.Registry.MailServerPort = 0;
			RunTaskSchedule(inboundMailTask);

			AssertEquals("Error|Mail server port should not be empty.\r\nThis can be entered in the Registry: System->Registry->Physical Server->Mail->Incoming->Mail Server Port\r\n", inboundMailTask.ServiceLogger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class InboundMailTaskForTesting : InboundMailTask
		{
			protected override IMailDownloader GetMailDownloader()
			{
				var result = new TestPop3Downloader();
				result.EmailDownloaded += OnEmailDownloaded;

				return result;
			}

			void OnEmailDownloaded(string uniqueId, ref string email, ref bool continueDownloading)
			{
				EmailDownloaded?.Invoke(uniqueId, ref email, ref continueDownloading);
			}

			public override void RunTask(CancellationToken token)
			{
				base.RunTask(CancellationTokenSource.Token);
			}

			public CancellationTokenSource CancellationTokenSource { get; } = new ();

			public event EmailDownloadedHandler EmailDownloaded;
		}

		class TestPop3Downloader : IMailDownloader
		{
			public long? MessageCount => 5;

			public void DownloadFromServer()
			{
				var continueDownloading = true;
				int i;

				for (i = 1; i <= MessageCount; i++)
				{
					if (EmailDownloaded != null)
					{
						var message = $"From: Babai <Babai@mabai.info>\r\n\r\nEmail {i}";
						EmailDownloaded(Guid.NewGuid().ToString(), ref message, ref continueDownloading);

						if (!continueDownloading)
						{
							i++;
							break;
						}
					}
				}

				DownloaderClosing?.Invoke(i);
			}

			public void DeleteMessage(string messageId)
			{
			}

			public event EmailDownloadedHandler EmailDownloaded;
			public event DownloaderClosingHandler DownloaderClosing;
			public event LogMessageHandler LogMessage { add { } remove { } }

			public void Dispose()
			{
			}
		}

		class InboundMailTaskWithExceptionForTesting : InboundMailTaskForTesting
		{
			public InboundMailTaskWithExceptionForTesting(Exception ex)
			{
				this.ex = ex;
			}

			readonly Exception ex;

			protected override MailSaver GetMailSaver(IMailDownloader downloader)
			{
				throw ex;
			}
		}
	}
}

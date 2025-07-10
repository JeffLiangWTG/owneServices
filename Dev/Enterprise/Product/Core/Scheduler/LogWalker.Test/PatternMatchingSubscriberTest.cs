using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.AuditDataServices.Notification;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.LogWalker.Testing
{
	[TestedType(typeof(PatternMatchingSubscriber))]
	sealed class PatternMatchingSubscriberTest : LogSubscriberTest<PatternMatchingSubscriber>
	{
		public void TestProcessLogs_AuditNotification_Enabled_ProcessLogs()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier
				.Setup(q => q.CheckStateOfNamedServiceTask(AuditSubscriberProcessorTask.ServiceTaskCode))
				.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				org.OH_Code = "BRXTON";
				org.OH_FullName = "TOLL PTY";

				org.MainAddress.OA_Address1 = "120 Graham Avenue";
				org.MainAddress.OA_City = "SYDNEY";
				org.MainAddress.OA_PostCode = "2015";
				org.MainAddress.OA_State = "NSW";
				org.MainAddress.OA_RN_NKCountryCode = "AU";

				DeleteStmALogQueue();
				((LoggerForTesting)Notifier).AllowDebug = true;

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"

processing 2 logged event(s).
Could not process logs because Change Data Capture is enabled.
finished processing logs.

						".Trim(), GetLogsAsString(Notifier));
			}
		}

		public void TestProcessLogs_AuditNotification_Disabled_DoNotProcessLogs()
		{
			var mockQuerier = new Mock<IServiceManagerQuerier>();
			mockQuerier
				.Setup(q => q.CheckStateOfNamedServiceTask(AuditSubscriberProcessorTask.ServiceTaskCode))
				.Returns(ServiceTaskStatus.ServiceTaskIsInactive);

			using (ObjectFactory.Substitute(mockQuerier.Object))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();

				org.OH_Code = "BRXTON";
				org.OH_FullName = "TOLL PTY";

				org.MainAddress.OA_Address1 = "120 Graham Avenue";
				org.MainAddress.OA_City = "SYDNEY";
				org.MainAddress.OA_PostCode = "2015";
				org.MainAddress.OA_State = "NSW";
				org.MainAddress.OA_RN_NKCountryCode = "AU";

				DeleteStmALogQueue();

				Factory.Save();
				RunLogWalkerCycleForTest();

				AssertMultilineASCIIEquals("Log", @"Logs in this batch processed successfully.", GetLogsAsString(Notifier));
			}
		}

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent => true;

		string GetLogsAsString(ILogger logger)
		{
			var logPrefix = string.Format(CultureInfo.InvariantCulture, "[{0}]", LogSubscriber.FriendlyName);
			var logs = ((LoggerForTesting)logger).NotifiedEventList.Where(l => l.StartsWith(logPrefix, StringComparison.Ordinal)).Select(l => l.Substring(logPrefix.Length + 1));

			return string.Join(System.Environment.NewLine, logs);
		}

		void DeleteStmALogQueue()
		{
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmALogQueue");
		}
	}
}

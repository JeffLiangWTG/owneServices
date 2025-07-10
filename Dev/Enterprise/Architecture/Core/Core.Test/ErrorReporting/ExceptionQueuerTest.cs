using System;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.ErrorReporting;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ExceptionQueuerTest : TransactionedTestCase
	{
		public void TestQueueExceptionEmailForLaterSending_InitialStatusQueued()
		{
			Internal_TestQueueExceptionEmailForLaterSending("Crash Report 1", StmErrorReportTransmitStatus.Codes.Queued);
		}

		public void TestQueueExceptionEmailForLaterSending_InitialStatusSent()
		{
			Internal_TestQueueExceptionEmailForLaterSending("Crash Report 2", StmErrorReportTransmitStatus.Codes.Sent);
		}

		public void TestQueueExceptionEmailForLaterSending_InitialStatusFailed()
		{
			Internal_TestQueueExceptionEmailForLaterSending("Crash Report 3", StmErrorReportTransmitStatus.Codes.Failed);
		}

		public void TestReportErrorsServiceTaskIsNudged_WhenThereIsAtLeastOneHostIsRunningHealthily()
		{
			var nudger = new Mock<IServiceTaskNudger>();
			var querier = new Mock<IServiceManagerQuerier>();

			using (ObjectFactory.Substitute(querier.Object))
			using (ObjectFactory.Substitute(nudger.Object))
			{
				const string serviceTaskCode = "RET";
				var isServiceTaskCalled = false;

				querier.Setup(m => m.CheckStateOfNamedServiceTask(serviceTaskCode))
					.Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

				nudger.Setup(m => m.NudgeServiceTask(serviceTaskCode, null))
					.Callback(() =>
					{
						isServiceTaskCalled = true;
					});

				var exceptionQueuer = new ExceptionQueuer();
				exceptionQueuer.QueueExceptionReportForLaterSending("Crash Report", StmErrorReportTransmitStatus.Codes.Queued, useNewDbConnection: false);
				AssertEquals("Service task should be nudged", expected: true, isServiceTaskCalled);
			}
		}

		public void TestReportErrorsServiceTaskIsNotNudged_WhenNoHostIsRunningHealthily()
		{
			var nudger = new Mock<IServiceTaskNudger>();
			var querier = new Mock<IServiceManagerQuerier>();

			using (ObjectFactory.Substitute(querier.Object))
			using (ObjectFactory.Substitute(nudger.Object))
			{
				const string serviceTaskCode = "RET";
				var isServiceTaskCalled = false;

				querier.Setup(m => m.CheckStateOfNamedServiceTask(serviceTaskCode))
					.Returns(ServiceTaskStatus.NoAvailableHosts);

				nudger.Setup(m => m.NudgeServiceTask(serviceTaskCode, null))
					.Callback(() =>
					{
						isServiceTaskCalled = true;
					});

				var exceptionQueuer = new ExceptionQueuer();
				exceptionQueuer.QueueExceptionReportForLaterSending("Crash Report", StmErrorReportTransmitStatus.Codes.Queued, useNewDbConnection: false);
				AssertEquals("Service task should not be nudged", expected: false, isServiceTaskCalled);
			}
		}

		void Internal_TestQueueExceptionEmailForLaterSending(string crashReport, string status)
		{
			var tableName = StmErrorReportSchema.Constants.TableName;
			using (var cmd = Db.Connection.Command($"DELETE [{tableName}]"))
			{
				cmd.ExecuteNonQuery();
			}

			var exceptionQueuer = new ExceptionQueuer();
			exceptionQueuer.QueueExceptionReportForLaterSending(crashReport, status, useNewDbConnection: false);

			using (var cmd = Db.Connection.Command($"SELECT TOP(1) * FROM [{tableName}]"))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals(true, reader.Read());

				var transmitStatus = (string)reader[StmErrorReportSchema.Constants.QER_TransmitStatus];
				AssertEquals(status, transmitStatus);

				var reportType = (ErrorReportType)reader[StmErrorReportSchema.Constants.QER_ReportType];
				AssertEquals(ErrorReportType.EnterpriseXml, reportType);

				var report = (string)reader[StmErrorReportSchema.Constants.QER_ReportXml];
				AssertEquals(crashReport, report);

				var createTime = (DateTime)reader[StmErrorReportSchema.Constants.QER_SystemCreateTimeUtc];
				AssertDateTimeWithinOneSecond("Create Time should be UTC now", DateTime.UtcNow, createTime);

				var createUser = (string)reader[StmErrorReportSchema.Constants.QER_SystemCreateUser];
				AssertEquals(EnvProxy.Instance.CurrentUser.Initials, createUser);

				AssertEquals(false, reader.Read());
				reader.Close();
			}
		}
	}
}

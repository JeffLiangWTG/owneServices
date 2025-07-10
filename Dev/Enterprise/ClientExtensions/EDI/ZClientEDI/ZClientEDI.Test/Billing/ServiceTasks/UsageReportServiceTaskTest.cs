using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Billing.ServiceTasks.Testing
{
	[TestedType(typeof(EdiReportingServiceTask))]
	public class UsageReportServiceTaskTest : ServiceTaskTestCase<EdiReportingServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var process = new EdiReportingServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			var header = Factory.New<OrgHeader>();
			header.FillWithValidTestData();
			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_Email = "contact@cw1.com";
			contact.OC_OH = header.PK;
			var database = Factory.New<LicenceDatabase>();
			database.FillWithValidTestData();

			var queue = Factory.New<StlCombinedUsageReportQueue>();
			queue.ERQ_ReportName = "Report1";
			queue.ERQ_ReportType = StlCombinedUsageReportQueue.ReportType;
			queue.ERQ_OH = header.PK;
			queue.ERQ_OC = contact.PK;
			queue.ERQ_LD = database.PK;
			Factory.Save();

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (var tempDir = new TempDirectory())
			{
				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				using (ClearUserContext())
				using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
				{
					AssertNoExceptionThrown(() => process.RunTask());

					queue.ERQ_CreateTimeUtc = ZDateTime.UtcNow.AddDays(-5).AddMinutes(-1);
					var fullFileName = queue.ERQ_ReportFileFullName;
					var queuePk = queue.PK;
					Factory.Save();

					AssertNoExceptionThrown(() => process.RunTask());
				}
				AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestRunTask()
		{
			var process = new EdiReportingServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_Email = "contact@cw1.com";
			var queue = Factory.New<EdiReportingQueue>();
			queue.ERQ_ReportName = "Report1";
			queue.ERQ_OC = contact.PK;
			Factory.Save();
			EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			logger.ClearLog();
			process.RunTask();
			AssertEquals("NEW", queue.ERQ_Status);
			var logs = $@"Information|Begin processing
Information|End processing
";
			AssertEquals(logs, logger.ToString());
			using (var tempDir = new TempDirectory())
			{
				AssertEquals("NEW", queue.ERQ_Status);
				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				logger.ClearLog();
				process.RunTask();
				AssertEquals("PCD", queue.ERQ_Status);
				AssertEquals(Path.Combine(tempDir.DirectoryName, queue.PK.ToString() + ".zip"), queue.ERQ_ReportFileFullName);
				AssertEquals(true, File.Exists(queue.ERQ_ReportFileFullName));
				var mailItems = new StandardMailItemCollection(Factory);
				mailItems.Load();
				AssertEquals("One email should be sent.", 1, mailItems.Count);
				AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
				AssertEquals("Email.MI_Subject", "CargoWise My Account - Report Download Notification", mailItems[0].MI_Subject);
				AssertEquals("Email.MI_Body", $"The Report is ready for download.<br/>Download Link:<a href='https://myaccount-portal.cargowise.com/myaccount/Download.aspx?report={queue.PK.ToString()}'>{queue.ERQ_ReportName}</a>", mailItems[0].MI_Body);
				AssertEquals("Email should have 1 recipients.", 1, mailItems[0].MailRecipients.Count);
				AssertEquals("First Recipient Address", "contact@cw1.com", mailItems[0].MailRecipients[0].EmailAddress);
				queue.ERQ_CreateTimeUtc = ZDateTime.UtcNow.AddDays(-5).AddMinutes(-1);
				var fullFileName = queue.ERQ_ReportFileFullName;
				var queuePk = queue.PK;
				Factory.Save();
				process.RunTask();
				AssertEquals(true, queue.IsDeleted);
				AssertEquals(false, File.Exists(fullFileName));
				logs = $@"Information|Begin processing
Information|Generating... [{queuePk.ToString()}]
Information|Generated... [Report1] [{fullFileName}]
Information|End processing
Information|Begin processing
Information|Removing... [Report1][{fullFileName}]
Information|Removed
Information|End processing
";
				AssertEquals(logs, logger.ToString());
			}
		}

		public void TestRunTask_ExceptionShouldReportError()
		{
			var process = new EdiReportingServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_Email = "contact@cw1.com";
			var queue = Factory.New<EdiReportingQueue>();
			queue.ERQ_ReportName = "Report1";
			queue.ERQ_OC = contact.PK;
			Factory.Save();
			EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			logger.ClearLog();
			ErrorReporter.Clear();
			var cancellationToken = new CancellationToken(true);
			using (var tempDir = new TempDirectory())
			{
				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				AssertExceptionThrown<OperationCanceledException>(() =>
				{
					process.RunTask(cancellationToken);
				});
				AssertEquals(typeof(OperationCanceledException), ErrorReporter.LastExceptionReported.GetType());
				AssertEquals("Unhandled exception from EdiReportingServiceTask", ErrorReporter.LastKeyReported);
				AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestRunTask_Retry()
		{
			EDIDataRegistry.Instance.EDIERouterUsageDBServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "localhost");
			EDIDataRegistry.Instance.EDIERouterUsageDBName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~!@#");
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			var contact = org.Contacts[0];
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			Factory.Save();

			Db.Connection.ExecuteNonQuery("DROP FUNCTION EdiGetStlUsageSummary;");

			Db.Connection.ExecuteNonQuery($@"
CREATE FUNCTION EdiGetStlUsageSummary(@OrgPk UNIQUEIDENTIFIER, @PeriodStart DATETIME)  
RETURNS @Result TABLE  
(  
 LD_PK UNIQUEIDENTIFIER NOT NULL,  
 LD_ServerCode VARCHAR(3) NOT NULL,  
 LE_EnterpriseCode VARCHAR(3) NOT NULL,  
 L7_PK UNIQUEIDENTIFIER NOT NULL,  
 L7_Description NVARCHAR(500) NOT NULL,  
 L7_Order SMALLINT NOT NULL,  
 LCC_PK UNIQUEIDENTIFIER,  
 LCC_Code VARCHAR(3) NOT NULL,  
 U1_Code VARCHAR(3) NOT NULL,  
 U1_UnitCount INT NOT NULL  
)  
BEGIN  
	INSERT INTO @Result VALUES
	('{db.PK}', 'SYD', 'ENT', 'c128bad9-e6fa-4b01-81aa-61e08457335f', 'DESC', 0, 'c128bad9-e6fa-4b01-81aa-61e08457335f', 'CO1', 'IQM', 1);
	RETURN;
END

");
			var result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201511, "");
			AssertEquals(true, result);
			var queue = Factory.LoadTop1<StlCombinedUsageReportQueue>(new ZQuery());

			var process = new EdiReportingServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			using (var tempDir = new TempDirectory())
			{
				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				process.RunTask();
				AssertEquals("NEW", queue.ERQ_Status);

				//Attempt retry within 1 hour.
				queue.ERQ_CreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-30);
				Factory.Save();
				process.RunTask();
				AssertEquals("NEW", queue.ERQ_Status);

				//just log the error after 1 hour
				queue.ERQ_CreateTimeUtc = ZDateTime.UtcNow.AddHours(-1).AddMinutes(-5);
				Factory.Save();
				process.RunTask();
				AssertEquals("PCD", queue.ERQ_Status);

				var logs = logger.ToString();
				AssertContains("Error|You have tried to establish a DB Connection using a generic machine name.", logs);
				AssertContains("   at CargoWise.Data.DbConnection.CheckServerNameIsNotLocalhost(String serverName)", logs);

				var trimmedLogs = logs.SplitByLine()
					.Where(x => !x.StartsWith("   at "))
					.Select(x => x.StartsWith("Error|You have tried to establish a DB Connection using a generic machine name.") ? "Error|You have tried to establish a DB Connection using a generic machine name." : x);

				var expectedLogs = $@"Information|Begin processing
Information|Generating... [{queue.PK.ToString()}]
Error|You have tried to establish a DB Connection using a generic machine name.
Information|Retry will be attempted later... [{queue.PK.ToString()}]
Information|End processing
Information|Begin processing
Information|Generating... [{queue.PK.ToString()}]
Error|You have tried to establish a DB Connection using a generic machine name.
Information|Retry will be attempted later... [{queue.PK.ToString()}]
Information|End processing
Information|Begin processing
Information|Generating... [{queue.PK.ToString()}]
Information|Generated... [201511_SYD_StlCombinedUsageReport] [{queue.ERQ_ReportFileFullName}]
Information|End processing";

				AssertEquals(expectedLogs, string.Join("\r\n", trimmedLogs));
				AssertEquals("StlCombinedUsageReportQueue.GenerateReportCore", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[] { new TaskNudgeInformationForTest(EdiReportingQueueSchema.Constants.TableName, null, EdiReportingQueueSchema.Constants.ERQ_Status + "=" + EdiReportingQueue.QueueStatus.NewReport), };
			}
		}
	}
}

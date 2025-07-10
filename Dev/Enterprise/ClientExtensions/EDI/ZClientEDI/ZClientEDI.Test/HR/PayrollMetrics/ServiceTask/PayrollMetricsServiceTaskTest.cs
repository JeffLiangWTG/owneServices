using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CargoWise.Common;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.HR.PayrollMetrics.Testing
{
	[TestedType(typeof(PayrollMetricsServiceTask))]
	class PayrollMetricsServiceTaskTest : ServiceTaskTestCase<PayrollMetricsServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1", "Payroll 2" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 5, 30, 14, 0, 0));
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			var mockSync = new Mock<ILeaveSynchronizer>();
			var mockClient = new Mock<IPayrollMetricsWebServiceClient>();
			task.LeaveSynchronizerForTest = mockSync.Object;
			task.ClientForTest = mockClient.Object;
			var response1 = new LeaveModifiedResponse();
			var response2 = new LeaveModifiedResponse();
			response1.Status = "0";
			response2.Status = "0";
			var employeeLeaveList = new List<EmployeeLeave>();
			var employeeLeave1 = new EmployeeLeave();
			employeeLeave1.PayrollName = "wise test";
			employeeLeave1.EmployeeNumber = "aaatest111";
			employeeLeave1.FirstName = "archie test";
			employeeLeave1.LastName = "xu test";
			employeeLeaveList.Add(employeeLeave1);
			response1.Output = employeeLeaveList;
			response2.Output = new List<EmployeeLeave>();
			EssLeaveRequestStatusModifiedApiParam params1 = null;
			EssLeaveRequestStatusModifiedApiParam params2 = null;
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					params1 = call;
				}).Returns(response1);
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[1])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					params2 = call;
				}).Returns(response2);

			var test = new LeaveSynchronizer(Factory, logger);
			var paramList = new List<EmployeeLeave>();
			mockSync.Setup(x => x.Process(paramList)).Callback(() => {
				var holiday = Factory.New<GlbStaffHoliday>();
				holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
				holiday.GA_WorkHolidayType = "TST";
			});

			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(task.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => task.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
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

		[TestDate(2017, 6, 1)]
		public void TestDoNotReportTaskCancellation()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = "SO1";
			staff.GS_FullName = "Staff One";
			staff.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			Factory.Save();
			EDIDataRegistry.Instance.HRNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			EDIDataRegistry.Instance.AllowPayrollMetricsServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1", "Payroll 2" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 5, 30, 14, 0, 0));
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			var mockSync = new Mock<ILeaveSynchronizer>();
			var mockClient = new Mock<IPayrollMetricsWebServiceClient>();
			task.LeaveSynchronizerForTest = mockSync.Object;
			task.ClientForTest = mockClient.Object;
			var response = new LeaveModifiedResponse();
			response.Status = "0";
			response.Output = new List<EmployeeLeave>();
			EssLeaveRequestStatusModifiedApiParam returnParams = null;
			var source = new CancellationTokenSource();
			var token = source.Token;
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					returnParams = call;
					source.Cancel();
				})
				.Returns(response);
			AssertExceptionThrown(typeof(OperationCanceledException), delegate
			{
				task.RunTask(token);
			});
			mockClient.VerifyAll();
			mockSync.VerifyAll();
			CombineAssertions(() =>
			{
				AssertEquals(1, logger.Count);
				AssertEquals("Information|Payroll Payroll 1 from 2017-05-30T23:45:00 to 2017-06-01T10:15:00", logger[0]);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			});
		}

		[TestDate(2017, 6, 1)]
		public void TestRunTask()
		{
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1", "Payroll 2" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 5, 30, 14, 0, 0));
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			var mockSync = new Mock<ILeaveSynchronizer>();
			var mockClient = new Mock<IPayrollMetricsWebServiceClient>();
			task.LeaveSynchronizerForTest = mockSync.Object;
			task.ClientForTest = mockClient.Object;
			var response1 = new LeaveModifiedResponse();
			var response2 = new LeaveModifiedResponse();
			response1.Status = "0";
			response2.Status = "0";
			response1.Output = new List<EmployeeLeave>();
			response2.Output = new List<EmployeeLeave>();
			EssLeaveRequestStatusModifiedApiParam params1 = null;
			EssLeaveRequestStatusModifiedApiParam params2 = null;
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					params1 = call;
				}).Returns(response1);
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[1])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					params2 = call;
				}).Returns(response2);
			mockSync.Setup(x => x.Process(new List<EmployeeLeave>()));
			task.RunTask();
			mockClient.VerifyAll();
			mockSync.VerifyAll();
			mockSync.Verify(x => x.Process(new List<EmployeeLeave>()), Times.Exactly(2));
			AssertEquals(new DateTime(2017, 6, 1, 0, 0, 0), EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.Value);
			AssertEquals(2, logger.Count);
			CombineAssertions(() =>
			{
				AssertEquals("StatusModifiedFrom1", new DateTime(2017, 5, 30, 23, 45, 0), params1.StatusModifiedFrom);
				AssertEquals("StatusModifiedTo1", new DateTime(2017, 6, 1, 10, 15, 0), params1.StatusModifiedTo);
				AssertEquals("StatusModifiedFrom2", new DateTime(2017, 5, 30, 23, 45, 0), params2.StatusModifiedFrom);
				AssertEquals("StatusModifiedTo2", new DateTime(2017, 6, 1, 10, 15, 0), params2.StatusModifiedTo);
				AssertEquals("Information|Payroll Payroll 1 from 2017-05-30T23:45:00 to 2017-06-01T10:15:00", logger[0]);
				AssertEquals("Information|Payroll Payroll 2 from 2017-05-30T23:45:00 to 2017-06-01T10:15:00", logger[1]);
			});
		}

		[TestDate(2017, 6, 1)]
		public void TestRunTask_ClientReturnsError()
		{
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			task.ClientForTest = new PayrollMetricsWebServiceClientForTest();
			task.RunTask();
			CombineAssertions(() =>
			{
				AssertEquals("PayrollMetricsLastLeaveSyncUtc", DateTime.MinValue, EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.Value);
				AssertEquals(2, logger.Count);
				AssertEquals("Information|Payroll Payroll 1 from 2017-05-31T09:45:00 to 2017-06-01T10:15:00", logger[0]);
				AssertEquals("Warning|Payroll Metrics web service returned: error. Request json: {\"PayrollName\":\"Payroll 1\",\"EmployeeNumber\":null,\"LeaveStatus\":null,\"LeaveIndicator\":null,\"StatusModifiedFrom\":\"2017-05-31T09:45:00\",\"StatusModifiedTo\":\"2017-06-01T10:15:00\"}", logger[1]);
			});
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		class PayrollMetricsWebServiceClientForTest : PayrollMetricsWebServiceClient
		{
			protected override string GetAccessToken()
			{
				return "";
			}

			protected override LeaveModifiedResponse GetResponse(Uri uri, StringContent httpContent)
			{
				return new LeaveModifiedResponse()
				{ ErrorMessages = new List<string>(new[] { "error" }), Status = "5" };
			}
		}

		[TestDate(2018, 10, 6, 17, 0, 0)]
		public void TestRunTask_DaylightSavings()
		{
			// Sydney daylight saving begins at UTC 2018-10-6 16:00
			// In local time
			//		15:59 UTC => 1:59am
			//		16:01 UTC => 3:01am
			// There is no such local time as 2:01am to 2:59am
			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2018, 10, 6, 16, 1, 0));
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			var mockSync = new Mock<ILeaveSynchronizer>();
			var mockClient = new Mock<IPayrollMetricsWebServiceClient>();
			task.LeaveSynchronizerForTest = mockSync.Object;
			task.ClientForTest = mockClient.Object;
			var response1 = new LeaveModifiedResponse();
			response1.Status = "0";
			response1.Output = new List<EmployeeLeave>();
			EssLeaveRequestStatusModifiedApiParam params1 = null;
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					params1 = call;
				}).Returns(response1);
			mockSync.Setup(x => x.Process(response1.Output));
			task.RunTask();
			mockClient.VerifyAll();
			mockSync.VerifyAll();
			CombineAssertions(() =>
			{
				AssertEquals("PayrollMetricsLastLeaveSyncUtc", new DateTime(2018, 10, 6, 17, 0, 0), EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.Value);
				AssertEquals("StatusModifiedFrom1", new DateTime(2018, 10, 7, 1, 46, 0), params1.StatusModifiedFrom);
				AssertEquals("StatusModifiedTo1", new DateTime(2018, 10, 7, 4, 15, 0), params1.StatusModifiedTo);
				AssertEquals("0", "Information|Payroll Payroll 1 from 2018-10-07T01:46:00 to 2018-10-07T04:15:00", logger[0]);
				AssertEquals(1, logger.Count);
			});
		}

		[TestDate(2017, 6, 1)]
		public void TestRunTask_Retry()
		{
			EDIDataRegistry.Instance.PayrollMetricsServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost");
			var payrolls = new string[] { "Payroll 1" };
			EDIDataRegistry.Instance.PayrollMetricsPayrollNames.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, payrolls);
			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 5, 31));
			var logger = new TestServiceLogger();
			var task = new PayrollMetricsServiceTask();
			task.ServiceLogger = logger;
			var mockSync = new Mock<ILeaveSynchronizer>();
			var mockClient = new Mock<IPayrollMetricsWebServiceClient>();
			task.LeaveSynchronizerForTest = mockSync.Object;
			task.ClientForTest = mockClient.Object;
			var response1 = new LeaveModifiedResponse();
			response1.Status = "0";
			response1.Output = new List<EmployeeLeave>();
			EssLeaveRequestStatusModifiedApiParam params1 = null;
			var numberOfCalls = 0;
			mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0])))
				.Callback((EssLeaveRequestStatusModifiedApiParam call) =>
				{
					numberOfCalls++;
					if (numberOfCalls <= 2)
					{
						params1 = call;
						throw new HttpRequestException("request failed");
					}
					else if (numberOfCalls == 3)
					{
						params1 = call;
					}
					else
					{
						mockClient.Setup(x => x.GetEssLeaveRequestStatusModified(It.Is<EssLeaveRequestStatusModifiedApiParam>(y => y.PayrollName == payrolls[0]))).CallBase();
					}
				}).Returns(response1);
			mockSync.Setup(x => x.Process(response1.Output));
			task.RunTask();
			mockClient.VerifyAll();
			mockSync.VerifyAll();
			CombineAssertions(() =>
			{
				AssertEquals("PayrollMetricsLastLeaveSyncUtc", new DateTime(2017, 6, 1, 0, 0, 0), EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.Value);
				AssertEquals(5, logger.Count);
				AssertEquals("StatusModifiedFrom1", new DateTime(2017, 5, 31, 9, 45, 0), params1.StatusModifiedFrom);
				AssertEquals("StatusModifiedTo1", new DateTime(2017, 6, 1, 10, 15, 0), params1.StatusModifiedTo);
				AssertEquals("0", "Information|Payroll Payroll 1 from 2017-05-31T09:45:00 to 2017-06-01T10:15:00", logger[0]);
				AssertStartsWith("1", "Information||System.Net.Http.HttpRequestException: request failed", logger[1]);
				AssertEquals("2", "Information|Payroll Payroll 1 from 2017-05-31T09:45:00 to 2017-06-01T10:15:00", logger[2]);
				AssertStartsWith("3", "Information||System.Net.Http.HttpRequestException: request failed", logger[3]);
				AssertEquals("4", "Information|Payroll Payroll 1 from 2017-05-31T09:45:00 to 2017-06-01T10:15:00", logger[4]);
			});
		}

		[TestDate(2019, 11, 13)]
		public void TestLogger()
		{
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			Factory.Save();
			EDIDataRegistry.Instance.HRNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			EDIDataRegistry.Instance.AllowPayrollMetricsServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var task = new PayrollMetricsServiceTask();
			var logger = new TestServiceLogger();
			task.ServiceLogger = logger;
			task.RunTask();
			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("ediProd PayrollMetrics sync failed", email.Subject);
			AssertEquals(@"[13-Nov-19 00:00] Warning Registry setting blank: WiseTech Global Client Extensions/HR/Payroll Metrics > Service URI


You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/HR > HR Notification Group", email.Body);
			AssertEquals(@"Warning|Registry setting blank: WiseTech Global Client Extensions/HR/Payroll Metrics > Service URI
", logger.ToString());
		}
	}
}

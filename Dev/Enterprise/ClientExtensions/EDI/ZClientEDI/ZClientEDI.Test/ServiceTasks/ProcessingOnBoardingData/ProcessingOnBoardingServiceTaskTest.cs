using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(ProcessingOnBoardingServiceTask))]
	class ProcessingOnBoardingServiceTaskTest : ServiceTaskTestCase<ProcessingOnBoardingServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
				"Queued Onboarding Data",
				EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.Queued
			),
			new TaskNudgeInformationForTest(
				EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
				"Staging Merged And Verified Onboarding Data",
				EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.StagingMergedAndVerified
			),
			new TaskNudgeInformationForTest(
				EdiTokenAuthOnBoardingDataSchema.Constants.TableName,
				"Reverted Onboarding Data",
				EdiTokenAuthOnBoardingDataSchema.Constants.TOD_Status + "=" + OnBoardingStatuses.Codes.Revert
			)
		};

		public void TestMinimumPeriod()
		{
			AssertEquals("30Minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestServiceTaskCode()
		{
			AssertEquals("POB", ProcessingOnBoardingServiceTask.Code);
		}

		[TestDate(2023, 10, 01)]
		[TestUtcOffset(0, 0, 0)]
		public void TestServiceTaskFail3TimesWillChangeStatusToError()
		{
			var serviceTask = new ProcessingOnBoardingServiceTask(Factory);

			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@123.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);

			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;

			Factory.Save();

			using (EDIDataRegistry.Instance.OnboardingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals(OnBoardingStatuses.Codes.Queued, ediTokenAuthOnBoardingData.TOD_Status);
				AssertEquals(1, ediTokenAuthOnBoardingData.TOD_Retry);

				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals(OnBoardingStatuses.Codes.Queued, ediTokenAuthOnBoardingData.TOD_Status);
				AssertEquals(2, ediTokenAuthOnBoardingData.TOD_Retry);

				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals(OnBoardingStatuses.Codes.Error, ediTokenAuthOnBoardingData.TOD_Status);
				AssertEquals(3, ediTokenAuthOnBoardingData.TOD_Retry);

				AssertEquals(1, Environment.Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Environment.Env.OutgoingMailManager.EmailsCreated.First();
				AssertEquals("Processing On Boarding Data Service Task Error", email.Subject);
				AssertStartsWith("Email body", "Value cannot be null.", email.Body);
			}
		}

		[TestDate(2023, 10, 01)]
		[TestUtcOffset(0, 0, 0)]
		public void TestOnBoardingDataQuery()
		{
			var serviceTask = new ProcessingOnBoardingServiceTask(Factory);

			var ediTokenAuthOnBoardingData1 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData1.TOD_Status = OnBoardingStatuses.Codes.Queued;

			var ediTokenAuthOnBoardingData2 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData2.TOD_Status = OnBoardingStatuses.Codes.StagingMergedAndVerified;

			var ediTokenAuthOnBoardingData3 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData3.TOD_Status = OnBoardingStatuses.Codes.Verified;

			var ediTokenAuthOnBoardingData4 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData4.TOD_Status = OnBoardingStatuses.Codes.Revert;

			var ediTokenAuthOnBoardingData5 = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData5.TOD_Status = OnBoardingStatuses.Codes.Completed;
			Factory.Save();

			var ediTokenAuthOnBoardingDatas = Factory.Load<EdiTokenAuthOnBoardingData>(serviceTask.OnBoardingDataQuery);

			AssertEquals(3, ediTokenAuthOnBoardingDatas.Length);
			AssertEquals(1, ediTokenAuthOnBoardingDatas.Count(e => e.TOD_Status == OnBoardingStatuses.Codes.Queued));
			AssertEquals(1, ediTokenAuthOnBoardingDatas.Count(e => e.TOD_Status == OnBoardingStatuses.Codes.StagingMergedAndVerified));
			AssertEquals(1, ediTokenAuthOnBoardingDatas.Count(e => e.TOD_Status == OnBoardingStatuses.Codes.Revert));
			AssertEquals(0, ediTokenAuthOnBoardingDatas.Count(e => e.TOD_Status == OnBoardingStatuses.Codes.Verified));
			AssertEquals(0, ediTokenAuthOnBoardingDatas.Count(e => e.TOD_Status == OnBoardingStatuses.Codes.Completed));
		}

		public void TestNoExceptionThrownWhenGroupDoesNotExist()
		{
			var query = new ZQuery(GlbGroupSchema.GG_Code, "PMG");
			var group = Factory.LoadTop1<GlbGroup>(query);
			group.GG_Code = "PMG1";
			var ediTokenAuthOnBoardingData = Factory.NewWithValidTestData<EdiTokenAuthOnBoardingData>();
			ediTokenAuthOnBoardingData.TOD_Status = OnBoardingStatuses.Codes.Queued;

			Factory.Save();
			var serviceTask = new ProcessingOnBoardingServiceTask(Factory);

			InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals(OnBoardingStatuses.Codes.Queued, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(1, ediTokenAuthOnBoardingData.TOD_Retry);

			InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals(OnBoardingStatuses.Codes.Queued, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(2, ediTokenAuthOnBoardingData.TOD_Retry);

			AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(serviceTask));

			AssertEquals(OnBoardingStatuses.Codes.Error, ediTokenAuthOnBoardingData.TOD_Status);
			AssertEquals(3, ediTokenAuthOnBoardingData.TOD_Retry);
		}
	}
}

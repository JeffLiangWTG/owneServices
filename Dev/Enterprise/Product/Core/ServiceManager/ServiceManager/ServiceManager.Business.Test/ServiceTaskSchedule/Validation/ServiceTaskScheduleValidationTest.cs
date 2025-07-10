using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceTaskScheduleValidationTest : TestCaseWithFactory
	{
		public void TestSRRSecondaryProcessesMaxCountLessThanMAXIMUMCONCURRENTREPORTSValidation()
		{
			CombineAssertions(() =>
			{
				Test(0, "Default value, should have no errors", expected: false);
				Test(1, "SecondaryProcessesMaxCount(1) < ReportMaxConnections(2), should have no errors", expected: false);
				Test(2, "SecondaryProcessesMaxCount(2) >= ReportMaxConnections(2), should have errors", expected: true);
			});

			void Test(int value, string message, bool expected)
			{
				// Arrange
				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Code == "SRR" &&
					a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.PrintJobProcessor.SRRSpecificValidation" &&
					a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.PrintJobProcessor");

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);
				var scheduledTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				scheduledTask.S5_ScheduleType = "SRR";

				const int maxValue = 2;
				using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, maxValue);
				using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

				// Act
				scheduledTask.SecondaryProcessesMaxCount = value;

				// Assert
				AssertEquals(message, expected, scheduledTask.HasErrors);

				if (expected)
				{
					var expectedErrorMessage = "Maximum count of secondary processes must be less than \"" + SystemDataRegistry.Instance.ReportMaxConnections.Caption + "\" value in Registry which is set to 2.";
					AssertHasErrorContaining(scheduledTask.SecondaryProcessesMaxCountInfo, expectedErrorMessage);
				}
			}
		}

		public void TestOMSSecondaryProcessesMaxCountNoMoreThan3IfUseOffice365()
		{
			// Arrange
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "OMS" &&
				a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
				a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor");

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			var scheduledTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			scheduledTask.S5_ScheduleType = "OMS";

			var expectedWarningMessage = "Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.";

			using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

			using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "abc"))
			{
				scheduledTask.SecondaryProcessesMaxCount = 4;
				Assert("SMTPServer is abc, should have no warnings", !scheduledTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should not be populated",
					String.Empty,
					scheduledTask.ExtendedConfigProcessesMaxCountWarningMessage);
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should not  be populated",
					String.Empty,
					scheduledTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());
			}

			using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp.office365.com"))
			{
				scheduledTask.SecondaryProcessesMaxCount = 0;
				Assert("Default value, should have no warnings", !scheduledTask.HasWarnings);

				scheduledTask.SecondaryProcessesMaxCount = 3;
				Assert("SecondaryProcessesMaxCount(3) <= 3, should have no warnings", !scheduledTask.HasWarnings);

				scheduledTask.SecondaryProcessesMaxCount = 4;
				Assert("SecondaryProcessesMaxCount(4) > 3, should have warnings", scheduledTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should be populated",
					"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
					scheduledTask.ExtendedConfigProcessesMaxCountWarningMessage.ToString());
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should be populated",
					@"https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages",
					scheduledTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());

				AssertHasWarningContaining(scheduledTask.SecondaryProcessesMaxCountInfo, expectedWarningMessage);
			}
		}

		public void TestOMSSecondaryProcessesMaxCountNoMoreThan3IfUseGraphAPI()
		{
			// Arrange
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "OMS" &&
				a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
				a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor");

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			var scheduledTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			scheduledTask.S5_ScheduleType = "OMS";

			var expectedWarningMessage = "Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.";

			using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

			using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				scheduledTask.SecondaryProcessesMaxCount = 4;
				Assert("UseGraphApiForIncoming is false, should have no warnings", !scheduledTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should not be populated",
					String.Empty,
					scheduledTask.ExtendedConfigProcessesMaxCountWarningMessage);
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should not  be populated",
					String.Empty,
					scheduledTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());
			}

			using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				scheduledTask.SecondaryProcessesMaxCount = 0;
				Assert("Default value, should have no warnings", !scheduledTask.HasWarnings);

				scheduledTask.SecondaryProcessesMaxCount = 3;
				Assert("SecondaryProcessesMaxCount(3) <= 3, should have no warnings", !scheduledTask.HasWarnings);

				scheduledTask.SecondaryProcessesMaxCount = 4;
				Assert("SecondaryProcessesMaxCount(4) > 3, should have warnings", scheduledTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should be populated",
					"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
					scheduledTask.ExtendedConfigProcessesMaxCountWarningMessage.ToString());
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should be populated",
					@"https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages",
					scheduledTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());

				AssertHasWarningContaining(scheduledTask.SecondaryProcessesMaxCountInfo, expectedWarningMessage);
			}
		}

		public void TestCheckS5_IsInactive_WhenBranchIsEmpty()
		{
			const string errorMessage = "Please enter a Branch.";

			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute());

			serviceTask.S5_IsActive = false;
			serviceTask.S5_GB = ZGuid.Empty;
			AssertNoError(serviceTask.S5_GBInfo, errorMessage);

			Factory.Save();

			serviceTask.S5_IsActive = true;
			serviceTask.S5_GB = ZGuid.Empty;
			AssertHasError(serviceTask.S5_GBInfo, errorMessage);

			serviceTask.S5_IsActive = false;
			serviceTask.S5_GB = ZGuid.Empty;
			AssertNoError(serviceTask.S5_GBInfo, errorMessage);
		}

		public void TestCheckS5_IsActive_WhenBranchSetToEmpty()
		{
			const string errorMessage = "Please enter a Branch.";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute());

			serviceTask.S5_GB = branch.PK;
			serviceTask.S5_IsActive = true;
			AssertNoError(serviceTask.S5_GBInfo, errorMessage);
			Factory.Save();

			serviceTask.S5_IsActive = true;
			serviceTask.S5_GB = ZGuid.Empty;
			AssertHasError(serviceTask.S5_GBInfo, errorMessage);

			serviceTask.S5_IsActive = false;
			serviceTask.S5_GB = ZGuid.Empty;
			AssertNoError(serviceTask.S5_GBInfo, errorMessage);
		}

		public void TestCheckS5_IsActive_WhenBranchIsInactive()
		{
			const string errorMessage = "You cannot activate a service task when the branch is inactive.";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = false;

			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.S5_GB = branch.PK;

			serviceTask.S5_IsActive = false;
			AssertNoError(serviceTask.S5_IsActiveInfo, errorMessage);

			Factory.Save();

			serviceTask.S5_IsActive = true;
			AssertHasError(serviceTask.S5_IsActiveInfo, errorMessage);

			branch.GB_IsActive = true;
			serviceTask.S5_IsActive = true;
			AssertNoError(serviceTask.S5_IsActiveInfo, errorMessage);
		}

		public void TestCheckS5_IsActive_WhenBranchWasAlwaysInactive()
		{
			const string errorMessage = "You cannot activate a service task when the branch is inactive.";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			var serviceTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTask.S5_GB = branch.PK;
			serviceTask.S5_IsActive = true;

			Factory.Save();

			using (branch.GetValidationSuspender())
			{
				branch.GB_IsActive = false;

				Factory.Save();
			}

			serviceTask.Validation.ValidateAll();

			AssertNoError("Because the ST is already active in the DB, we want it to be allowed to run", serviceTask.S5_IsActiveInfo, errorMessage);

			serviceTask.S5_IsActive = false;

			Factory.Save();

			serviceTask.S5_IsActive = true;

			AssertHasError("Because the ST has been set to from inactive to active, we want it to block saving", serviceTask.S5_IsActiveInfo, errorMessage);
		}

		public void TestIsActive_AllowsDeactivationOfNonMandatoryTasks()
		{
			// Arrange
			var schedule = GetServiceTaskSchedule(null, null, null);
			schedule.S5_ScheduleType = "XYZ";
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			// Act
			schedule.S5_IsActive = false;
			schedule.Factory.Save();
			// Assert
			AssertNoErrors(schedule.S5_IsActiveInfo);
		}

		public void TestIsActive_PreventsDeactivationOfMandatoryTasks()
		{
			// Arrange
			var schedule = GetServiceTaskSchedule(null, null, null);
			schedule.S5_ScheduleType = "XYZ";
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = true;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			// Act
			schedule.S5_IsActive = false;
			schedule.Factory.Save();
			// Assert
			AssertHasError(schedule.S5_IsActiveInfo, "This service task is mandatory and cannot be deactivated.");
		}

		public void TestIsActive_PreventsDeactivationOfMandatoryTasksInRequiredCountryCompany()
		{
			// Arrange
			var schedule = GetServiceTaskSchedule(null, null, null);
			schedule.S5_ScheduleType = "XYZ";

			var settings = new HostedServiceAttribute
			{
				IsMandatory = true,
				RequiresCompanyInCountry = "MY, ME"
			};
			schedule.SetStaticServiceAttributesDebugOnly(settings);

			// No required companies - no error
			schedule.S5_IsActive = false;
			Factory.Save();
			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.S5_IsActiveInfo);

			// Add company
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "ME";
			var branch = company.Branches.AddNew();
			branch.GB_RN_NKCountryCode = "ME";
			Factory.Save();
			// Act
			schedule.Validation.ValidateAll();
			// Assert
			AssertHasError(schedule.S5_IsActiveInfo, "This service task is mandatory for active companies in the following country(s): MY, ME, and cannot be deactivated.");

			// Deactivate company
			company.GC_IsActive = false;
			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.S5_IsActiveInfo);
		}

		public void TestIsActive_CheckIsReadOnlyForWiseCloudClient()
		{
			var schedule = GetServiceTaskSchedule(null, null, null);

			var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;
			try
			{
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = true;

				var settings = new HostedServiceAttribute();
				schedule.SetStaticServiceAttributesDebugOnly(settings);
				settings.IsReadOnlyForWiseCloudClient = true;
				settings.IsScheduleReadOnly = false;

				using (EnvProxy.Instance.SetTemporaryUserContext(null))
				{
					// Arrange - test a value change without user context
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertNoErrors(schedule.S5_IsActiveInfo);
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertNoErrors(schedule.S5_IsActiveInfo);
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act - change nothing
					schedule.S5_IsActive = true;
					// Assert
					AssertNoErrors(schedule.S5_IsActiveInfo);

					// Act - change value
					schedule.S5_IsActive = false;
					// Assert
					AssertHasError(schedule.S5_IsActiveInfo, "The configuration of this service task can only be modified by CargoWise support.");
				}

				settings.IsReadOnlyForWiseCloudClient = false;

				using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertNoErrors(schedule.S5_IsActiveInfo);
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertNoErrors(schedule.S5_IsActiveInfo);
				}

				settings.IsScheduleReadOnly = true;

				using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = false;
					Factory.Save();
					// Act
					schedule.S5_IsActive = true;
					// Assert
					AssertHasError(schedule.S5_IsActiveInfo, "The configuration of this service task is read only, it cannot be modified.");
				}

				using (EnvProxy.Instance.SetTemporaryUserContext(User.WebUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertHasError(schedule.S5_IsActiveInfo, "The configuration of this service task is read only, it cannot be modified.");
				}

				settings.IsMandatory = true;

				using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
				{
					// Arrange
					schedule.S5_IsActive = true;
					Factory.Save();
					// Act
					schedule.S5_IsActive = false;
					// Assert
					AssertHasError(schedule.S5_IsActiveInfo, "This service task is mandatory and cannot be deactivated.");
				}
			}
			finally
			{
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
			}
		}

		public void TestNextRunTime_PreventPostposingMandatoryTasks()
		{
			// Arrange
			var testTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = true;
			testTask.SetStaticServiceAttributesDebugOnly(settings);
			testTask.S5_ScheduleType = ScheduleTypeConstants.DbHealthCheckCode;

			// Act/Assert
			testTask.ClearRowNotifications();
			testTask.S5_NextScheduledPrintRunTimeUtc = DateTime.UtcNow.AddHours(41);
			Factory.Save();
			AssertNoErrors(testTask.S5_NextScheduledPrintRunTimeUtcInfo);

			testTask.S5_NextScheduledPrintRunTimeUtc = DateTime.UtcNow.AddHours(45);
			Factory.Save();
			AssertHasError(testTask.S5_NextScheduledPrintRunTimeUtcInfo, "Next runtime for mandatory service tasks cannot be postponed.");

			testTask.ClearRowNotifications();
			testTask.S5_NextScheduledPrintRunTimeUtc = DateTime.UtcNow.AddHours(35);
			Factory.Save();
			AssertNoErrors(testTask.S5_NextScheduledPrintRunTimeUtcInfo);
		}

		[TestDate(2008, 2, 4, 0, 30, 0)]
		public void TestCheckS5_NextScheduledPrintRunTime()
		{
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Daily, false);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Weekly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Monthly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Yearly, true);
		}

		void AssertIsValidTimeOfDayToRunForDailyTasks2(ZString taskPeriod, ZBool isValidTimeIfWeekDaysOnly)
		{
			var dayList = taskPeriod == "W" ? "YNNNNNN" : "NNNNNNN";

			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			scheduleTask.S5_TaskPeriod = taskPeriod;
			scheduleTask.S5_DayList = dayList;
			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(12);
			Factory.Save();

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-91);
			AssertHasError("NextRunTime more than 1 hour old => IsValidTimeOfDayToRun?", scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo, "The Next Run Time is in the past; please enter a valid Next Run Time (the present or anything in the future will work).");

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.AddHours(23).AddMinutes(30);
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-89);
			AssertNoErrors("NextRunTime only 1 hour old => IsValidTimeOfDayToRun?", scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo);

			scheduleTask.S5_WeekDaysOnly = true;
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-89);
			if (isValidTimeIfWeekDaysOnly)
			{
				AssertNoErrors("Task.WeekDaysOnly = TRUE => IsValidTimeOfDayToRun?", scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo);
			}
			else
			{
				AssertHasError("Task.WeekDaysOnly = TRUE => IsValidTimeOfDayToRun?", scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo, "The Next Run Time is in the past; please enter a valid Next Run Time (the present or anything in the future will work).");
			}

			scheduleTask.S5_WeekDaysOnly = false;
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-89);
			AssertNoErrors("Task.WeekDaysOnly = FALSE => IsValidTimeOfDayToRun?", scheduleTask.S5_NextScheduledPrintRunTimeUtcInfo);
		}

		GlbBranch GetBranch(string countryCode)
		{
			return GetBranch(Factory, countryCode);
		}

		static GlbBranch GetBranch(BusinessObjectFactory factory, string countryCode)
		{
			var newCompany = factory.New<GlbCompany>();
			newCompany.GC_Code = "~TC";
			newCompany.GC_RN_NKCountryCode = countryCode;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "~TB";
			newBranch.GB_RL_NKHomePort = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_Code;
			factory.Save();
			return newBranch;
		}

		public void TestCheckS5_GBErrorsOnNoBranchWhenNotCanRunInAnyBranch()
		{
			// Arrange
			var scheduleTask = GetServiceTaskSchedule(null, null, Core.Constants.CountryCodes.Bahamas, canRunInAnyBranch: false);

			// Act
			scheduleTask.S5_GB = ZGuid.Empty;

			// Assert
			AssertHasError(scheduleTask.S5_GBInfo, "Please enter a " + scheduleTask.S5_GBInfo.Description + ".");
		}

		public void TestCheckS5_GBErrorsOnInvalidBranchWithSingleAllowed()
		{
			// Arrange
			var scheduleTask = GetServiceTaskSchedule(null, null, Core.Constants.CountryCodes.Bahamas);
			var branch = GetBranch(Core.Constants.CountryCodes.TrinidadAndTobago);

			// Act
			scheduleTask.S5_GB = branch.PK;

			// Assert
			AssertHasError(scheduleTask.S5_GBInfo, $"The service task is unable to run as the branch selected '{branch.GB_Code}' is invalid. The country code of the branch and the branch company must be 'BS' - Bahamas.");
		}

		public void TestCheckS5_GBErrorsOnInvalidBranchWithTwoAllowed()
		{
			// Arrange
			var scheduleTask = GetServiceTaskSchedule(null, null, Core.Constants.CountryCodes.Bahamas + "," + Core.Constants.CountryCodes.TrinidadAndTobago);
			var branch = GetBranch(Core.Constants.CountryCodes.FrenchPolynesia);

			// Act
			scheduleTask.S5_GB = branch.PK;

			// Assert
			AssertHasError(scheduleTask.S5_GBInfo, $"The service task is unable to run as the branch selected '{branch.GB_Code}' is invalid. The country code of the branch and the branch company must be 'BS' - Bahamas, or 'TT' - Trinidad and Tobago.");
		}

		public void TestCheckS5_GBAllowsGoodBranch()
		{
			// Arrange
			var scheduleTask = GetServiceTaskSchedule(null, null, Core.Constants.CountryCodes.Bahamas);
			var branch = GetBranch(Core.Constants.CountryCodes.Bahamas);

			// Act
			scheduleTask.S5_GB = branch.PK;

			// Assert
			AssertNoErrors(scheduleTask.S5_GBInfo);
		}

		public void TestCheckS5_GBIsDisabledForNotActiveTask()
		{
			// Arrange
			var branch = GetBranch(Core.Constants.CountryCodes.Bahamas);

			var scheduleTask = GetServiceTaskSchedule(null, null, Core.Constants.CountryCodes.FrenchPolynesia);
			scheduleTask.S5_IsActive = false;
			scheduleTask.S5_GB = branch.PK;

			// Act
			var result = scheduleTask.S5_GBInfo;

			// Assert
			AssertNoErrors(result);
		}

		public class CheckS5_GBTest : TestCaseWithFactory
		{
			public class RequiresCompanyInCountryAndCanRunInAnyBranchTest : TestCaseWithFactory
			{
				public void TestSuitableCompanyInCountry()
				{
					// Arrange
					var scheduleTask = GetServiceTaskSchedule(Factory, minimumPeriod: null, maximumPeriod: null,
						requiresCompanyInCountry: string.Join(",", new string[] { Core.Constants.CountryCodes.Bahamas, Core.Constants.CountryCodes.Canada }),
						canRunInAnyBranch: true);

					var companyInCountry = Factory.NewWithValidTestData<GlbCompany>();
					companyInCountry.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
					companyInCountry.GC_IsActive = true;

					// suitable branch
					var branchOfCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					branchOfCompanyInCountry.GB_IsActive = true;
					branchOfCompanyInCountry.GB_GC = companyInCountry.PK;

					var companyInCountryWithoutBranches = Factory.NewWithValidTestData<GlbCompany>();
					companyInCountryWithoutBranches.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
					companyInCountryWithoutBranches.GC_IsActive = true;

					var companyNotInCountry = Factory.NewWithValidTestData<GlbCompany>();
					companyNotInCountry.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					companyNotInCountry.GC_IsActive = true;

					var branchOfCompanyNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					branchOfCompanyNotInCountry.GB_IsActive = true;
					branchOfCompanyNotInCountry.GB_GC = companyNotInCountry.PK;

					var inactiveCompanyInCountry = Factory.NewWithValidTestData<GlbCompany>();
					companyNotInCountry.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Bahamas;
					companyNotInCountry.GC_IsActive = false;

					var activeBranchOfInactiveCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					activeBranchOfInactiveCompanyInCountry.GB_IsActive = true;
					activeBranchOfInactiveCompanyInCountry.GB_GC = inactiveCompanyInCountry.PK;

					Factory.Save();

					// Act
					scheduleTask.S5_GB = ZGuid.Empty;

					// Assert
					AssertNoErrors(scheduleTask.S5_GBInfo);
				}

				public void TestNoSuitableCompanyButUnlocoInCountry()
				{
					// Arrange
					var scheduleTask = GetServiceTaskSchedule(Factory, minimumPeriod: null, maximumPeriod: null,
						requiresCompanyInCountry: string.Join(",", new string[] { Core.Constants.CountryCodes.Bahamas, Core.Constants.CountryCodes.Canada }),
						canRunInAnyBranch: true);

					var bsLOCO = Factory.LoadTop1<RefUNLOCO>(
						new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Bahamas));
					var auLOCO = Factory.LoadTop1<RefUNLOCO>(
						new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia));

					var companyNotInCountry = Factory.NewWithValidTestData<GlbCompany>();
					companyNotInCountry.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
					companyNotInCountry.GC_IsActive = true;

					// suitable branch
					var activeBranchWithUnlocoInCountry = Factory.NewWithValidTestData<GlbBranch>();
					activeBranchWithUnlocoInCountry.GB_IsActive = true;
					activeBranchWithUnlocoInCountry.GB_GC = companyNotInCountry.PK;
					activeBranchWithUnlocoInCountry.GB_RL_NKHomePort = bsLOCO.Code;

					var branchWithUnlocoNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					branchWithUnlocoNotInCountry.GB_IsActive = true;
					branchWithUnlocoNotInCountry.GB_GC = companyNotInCountry.PK;
					branchWithUnlocoNotInCountry.GB_RL_NKHomePort = auLOCO.Code;

					var inactiveBranchWithUnlocoInCountry = Factory.NewWithValidTestData<GlbBranch>();
					inactiveBranchWithUnlocoInCountry.GB_IsActive = false;
					inactiveBranchWithUnlocoInCountry.GB_GC = companyNotInCountry.PK;
					inactiveBranchWithUnlocoInCountry.GB_RL_NKHomePort = bsLOCO.Code;

					var inactiveBranchWithUnlocoNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					inactiveBranchWithUnlocoNotInCountry.GB_IsActive = false;
					inactiveBranchWithUnlocoNotInCountry.GB_GC = companyNotInCountry.PK;
					inactiveBranchWithUnlocoNotInCountry.GB_RL_NKHomePort = auLOCO.Code;

					Factory.Save();

					// Act
					scheduleTask.S5_GB = ZGuid.Empty;

					// Assert
					AssertNoErrors(scheduleTask.S5_GBInfo);
				}

				public void TestNoSuitableCompanyAndUnlocoNotInCountry()
				{
					// Arrange
					var scheduleTask = GetServiceTaskSchedule(Factory, minimumPeriod: null, maximumPeriod: null,
						requiresCompanyInCountry: string.Join(",", new string[] { Core.Constants.CountryCodes.Bahamas, Core.Constants.CountryCodes.Canada }),
						canRunInAnyBranch: true);

					// Act
					scheduleTask.S5_GB = ZGuid.Empty;

					// Assert
					AssertHasError(scheduleTask.S5_GBInfo, "No suitable branch found for the service task in the following required countries: 'BS' - Bahamas, or 'CA' - Canada.");
				}
			}
		}

		public void TestValidateFrequency()
		{
			var task = GetServiceTaskSchedule("1 second", "5s", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Second, 5, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Second, 10, true, false, "The task should be run at least every 5 seconds");

			task = GetServiceTaskSchedule("1 second", "15 minutes", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Second, 5, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Minute, 2, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Minute, 16, true, false, "The task should be run at least every 15 minutes");

			task = GetServiceTaskSchedule("2minute", "1h", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Second, 90, true, false, "The task schedule shouldn't be more frequent than every 2 minutes");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Second, 150, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Minute, 45, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 1, true, false, "The task should be run at least every 1 hour");

			task = GetServiceTaskSchedule("12hours", "1day", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Hourly, 2, true, false, "The task schedule shouldn't be more frequent than every 12 hours");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Hourly, 12, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 2, true, false, "The task should be run at least every 1 day");

			task = GetServiceTaskSchedule(null, "2 days", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Minute, 10, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Hourly, 49, true, false, "The task should be run at least every 2 days");

			task = GetServiceTaskSchedule("1w", null, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 6, true, false, "The task schedule shouldn't be more frequent than every 1 week");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 7, false, false, null);

			task = GetServiceTaskSchedule("1n", "6months", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 27, true, false, "The task schedule shouldn't be more frequent than every 1 month");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Monthly, 1, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Monthly, 6, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Monthly, 7, true, false, "The task should be run at least every 6 months");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 365, true, false, "The task should be run at least every 6 months");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 168, false, false, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 169, true, false, "The task should be run at least every 6 months");

			task = GetServiceTaskSchedule("2day", "2day", null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 1, true, false, "The task should be run every 2 days");
			AssertTaskFrequency(task, ScheduleRecurrenceType.Daily, 3, true, false, "The task should be run every 2 days");

			task = GetServiceTaskSchedule(null, null, null);
			AssertTaskFrequency(task, ScheduleRecurrenceType.Monthly, 1, false, false, null);
		}

		public void TestValidateFrequencyWhenUpdatePeriod()
		{
			var task = GetServiceTaskSchedule("2minute", "1h", null);

			task.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
			task.S5_TaskPeriodCount = 30;
			AssertEquals("Has Error", expected: false, task.S5_TaskPeriodCountInfo.HasErrors());

			task.S5_TaskPeriod = ScheduleRecurrenceType.Hourly;
			AssertEquals("Has Error", expected: true, task.S5_TaskPeriodCountInfo.HasError("The task should be run at least every 1 hour"));

			task.S5_TaskPeriod = ScheduleRecurrenceType.Second;
			AssertEquals("Has Error", expected: true, task.S5_TaskPeriodCountInfo.HasError("The task schedule shouldn't be more frequent than every 2 minutes"));
		}

		public void TestValidateFrequencyWhenUpdatePeriodCount()
		{
			var task = GetServiceTaskSchedule("2minute", "1h", null);

			task.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
			task.S5_TaskPeriodCount = 30;
			AssertEquals("Has Error", expected: false, task.S5_TaskPeriodCountInfo.HasErrors());

			task.S5_TaskPeriodCount = 99;
			AssertEquals("Has Error", expected: true, task.S5_TaskPeriodCountInfo.HasError("The task should be run at least every 1 hour"));

			task.S5_TaskPeriodCount = 1;
			AssertEquals("Has Error", expected: true, task.S5_TaskPeriodCountInfo.HasError("The task schedule shouldn't be more frequent than every 2 minutes"));
		}

		public void TestValidateFrequency_SchedulePeriodValidationIsSkippedIfServiceTaskIsNudgeable()
		{
			// Arrange
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
				{
					new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
					new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null),
				})))
			{
				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				var taskSchedule = Factory.New<ServiceTaskSchedule>();
				taskSchedule.S5_ScheduleType = "111";

				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;
				var validation = new ServiceTaskScheduleValidation(taskSchedule);

				// Act
				validation.ValidateFrequency();

				// Assert
				Assert(!validation.FrequencyValidationWasCalledForTest);
			}
		}

		public void TestValidateFrequencyActiveTask_DefaultMinValue()
		{
			TestValidateFrequency_DefaultMinValue(isActive: true);
		}

		public void TestValidateFrequencyInactiveTask_DefaultMinValue()
		{
			TestValidateFrequency_DefaultMinValue(isActive: false);
		}

		void TestValidateFrequency_DefaultMinValue(bool isActive)
		{
			// Arrange
			var attributes = new HostedServiceAttribute
			{
				MinimumPeriod = "1second",
			};
			var taskSchedule = Factory.New<ServiceTaskSchedule>();
			taskSchedule.SetStaticServiceAttributesDebugOnly(attributes);
			var validation = new ServiceTaskScheduleValidation(taskSchedule);
			taskSchedule.S5_TaskPeriod = "S";
			taskSchedule.S5_TaskPeriodCount = -1;
			taskSchedule.S5_IsActive = isActive;

			// Assert
			AssertEquals("Has Error", expected: true, taskSchedule.S5_TaskPeriodCountInfo.HasError("The task schedule shouldn't be more frequent than every 1 second"));
		}

		public void TestValidateFrequencyActiveTask_DefaultMaxValue()
		{
			TestValidateFrequency_DefaultMaxValue(isActive: true);
		}

		public void TestValidateFrequencyInactiveTask_DefaultMaxValue()
		{
			TestValidateFrequency_DefaultMaxValue(isActive: false);
		}

		public void TestValidateFrequency_DefaultMaxValue(bool isActive)
		{
			// Arrange
			var attributes = new HostedServiceAttribute();
			var taskSchedule = Factory.New<ServiceTaskSchedule>();
			taskSchedule.SetStaticServiceAttributesDebugOnly(attributes);
			var validation = new ServiceTaskScheduleValidation(taskSchedule);
			taskSchedule.S5_TaskPeriod = "Y";
			taskSchedule.S5_TaskPeriodCount = 1000;
			taskSchedule.S5_IsActive = isActive;

			// Act
			validation.ValidateFrequency();

			// Assert
			AssertEquals(false, taskSchedule.HasErrors);
		}

		void AssertTaskFrequency(ServiceTaskSchedule taskSchedule, string taskPeriod, int taskPeriodCount, bool hasErrors, bool hasWarnings, string expectedMessage)
		{
			taskSchedule.S5_TaskPeriod = taskPeriod;
			taskSchedule.S5_TaskPeriodCount = taskPeriodCount;

			AssertEquals("Has Error", hasErrors, taskSchedule.S5_TaskPeriodCountInfo.HasError(expectedMessage));
			AssertEquals("Has Warning", hasWarnings, taskSchedule.S5_TaskPeriodCountInfo.HasWarning(expectedMessage));
		}

		public void TestValidateTaskPeriod()
		{
			var task = GetServiceTaskSchedule("c", "5s", null);
			task.S5_TaskPeriod = ZString.Empty;
			task.ClearRowNotifications();
			((ServiceTaskScheduleValidation)task.Validation).ValidateTaskPeriod();
			AssertEquals("Has Row Error", true, task.HasRowErrors);
			AssertEquals("The Service Task does not have a valid Recurrence Pattern. Edit the Service Task Schedule and enter a valid Recurrence Pattern.", task.RowErrors.First().Message);
		}

		public void TestCheckSecondaryProcessesMaxCountDescription()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			AssertNoErrors(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo);
			scheduleTask.SecondaryProcessesMaxCountDescription = "-1";
			AssertHasError(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
			scheduleTask.SecondaryProcessesMaxCountDescription = "1";
			AssertNoErrors(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo);
			scheduleTask.SecondaryProcessesMaxCountDescription = "10";
			AssertNoErrors(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo);
			scheduleTask.SecondaryProcessesMaxCountDescription = "100";
			AssertHasError(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
			scheduleTask.SecondaryProcessesMaxCountDescription = "SYSTEM MANAGED";
			AssertNoErrors(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo);
			scheduleTask.SecondaryProcessesMaxCountDescription = "NULL";
			AssertHasError(scheduleTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
		}

		public void TestS5_TaskPeriodCountValuesAboveMaximumAreInvalid()
		{
			var testCases = new List<(ZString type, ZInt max)>
			{
				(ScheduleRecurrenceType.Hourly, 99999999),
				(ScheduleRecurrenceType.Daily, 9999999),
				(ScheduleRecurrenceType.Weekly, 999999),
				(ScheduleRecurrenceType.Monthly, 99999),
			};

			Dictionary<string, long> maxValues = new Dictionary<string, long>() {
				{ ScheduleRecurrenceType.Hourly, (long)TimeSpan.MaxValue.TotalHours },
				{ ScheduleRecurrenceType.Daily, (long)TimeSpan.MaxValue.TotalDays },
				{ ScheduleRecurrenceType.Weekly, (long)TimeSpan.MaxValue.TotalDays / 7 },
				{ ScheduleRecurrenceType.Monthly, (long)TimeSpan.MaxValue.TotalDays / 28 },
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					//Arrange
					var testTask = GetServiceTaskSchedule(null, null, null);

					//Act
					testTask.S5_TaskPeriod = testCase.type;
					testTask.S5_TaskPeriodCount = 999999999; //Above the maximum for all recurrence types

					//Assert
					AssertHasError(testTask.S5_TaskPeriodCountInfo, $"Period Count is out of bounds for associated TimeSpan. Bounds are +/- {maxValues[testCase.type]}.");
				}
			});
		}

		public void TestS5_TaskPeriodCountValuesBelowMinimumAreInvalid()
		{
			var testCases = new List<(ZString type, ZInt min)>
			{
				(ScheduleRecurrenceType.Hourly, -99999999),
				(ScheduleRecurrenceType.Daily, -9999999),
				(ScheduleRecurrenceType.Weekly, -999999),
				(ScheduleRecurrenceType.Monthly, -99999)
			};

			Dictionary<string, long> maxValues = new Dictionary<string, long>() {
				{ ScheduleRecurrenceType.Hourly, (long)TimeSpan.MaxValue.TotalHours },
				{ ScheduleRecurrenceType.Daily, (long)TimeSpan.MaxValue.TotalDays },
				{ ScheduleRecurrenceType.Weekly, (long)TimeSpan.MaxValue.TotalDays / 7 },
				{ ScheduleRecurrenceType.Monthly, (long)TimeSpan.MaxValue.TotalDays / 28 }
			};

			CombineAssertions(() =>
			{
				foreach (var testCase in testCases)
				{
					//Arrange
					var testTask = GetServiceTaskSchedule(null, null, null);

					//Act
					testTask.S5_TaskPeriod = testCase.type;
					testTask.S5_TaskPeriodCount = -999999999; //Below the minimum for all recurrence types

					//Assert
					AssertHasError(testTask.S5_TaskPeriodCountInfo, $"Period Count is out of bounds for associated TimeSpan. Bounds are +/- {maxValues[testCase.type]}.");
				}
			});
		}

		ServiceTaskSchedule GetServiceTaskSchedule(string minimumPeriod, string maximumPeriod, string requiresCompanyInCountry, bool canRunInAnyBranch = false)
		{
			return GetServiceTaskSchedule(Factory, minimumPeriod, maximumPeriod, requiresCompanyInCountry, canRunInAnyBranch);
		}

		static ServiceTaskSchedule GetServiceTaskSchedule(BusinessObjectFactory factory, string minimumPeriod, string maximumPeriod, string requiresCompanyInCountry, bool canRunInAnyBranch = false)
		{
			var attributes = new HostedServiceAttribute();
			if (minimumPeriod != null || maximumPeriod != null || requiresCompanyInCountry != null)
			{
				attributes = new HostedServiceAttribute
				{
					MinimumPeriod = minimumPeriod,
					MaximumPeriod = maximumPeriod,
					RequiresCompanyInCountry = requiresCompanyInCountry,
					CanRunInAnyBranch = canRunInAnyBranch,
					DefaultScheduleRunEvery = minimumPeriod
				};
			}

			var taskSchedule = factory.New<ServiceTaskSchedule>();
			taskSchedule.SetStaticServiceAttributesDebugOnly(attributes);

			return taskSchedule;
		}

		public class GetPeriodDurationTest : TestCase
		{
			public void TestInvalidString()
			{
				// Arrange
				var testData = new List<string>()
				{
					"",
					null,
					"abc",
					"Seconds",
					"Y",
					"20SS",
				};

				CombineAssertions(() =>
				{
					testData.ForEach(testValue =>
					{
						// Act
						var result = ServiceTaskScheduleValidation.GetPeriodDuration(testValue, TimeSpan.MaxValue, isRandomPeriod: false);

						// Assert
						AssertEquals($"{testValue} should not be parsed", TimeSpan.MaxValue, result);
					});
				});
			}

			public void TestCorrectTimeSpan()
			{
				// Arrange
				var testData = new List<(string, TimeSpan)>()
				{
					("1s", TimeSpan.FromSeconds(1)),
					("10s", TimeSpan.FromSeconds(10)),
					("2m", TimeSpan.FromMinutes(2)),
					("15m", TimeSpan.FromMinutes(15)),
					("3h", TimeSpan.FromHours(3)),
					("13h", TimeSpan.FromHours(13)),
					("4d", TimeSpan.FromDays(4)),
					("20d", TimeSpan.FromDays(20)),
					("4w", TimeSpan.FromDays(4 * 7)),
					("5w", TimeSpan.FromDays(5 * 7)),
					("9n", TimeSpan.FromDays(9 * 28)),
					("13n", TimeSpan.FromDays(13 * 28)),
				};

				CombineAssertions(() =>
				{
					testData.ForEach(x =>
					{
						// Act
						var result = ServiceTaskScheduleValidation.GetPeriodDuration(x.Item1, TimeSpan.MaxValue, isRandomPeriod: false);

						// Assert
						AssertEquals(x.Item2, result);
					});
				});
			}

			public void TestRandomTimeSpan()
			{
				// Arrange
				var testData = new List<(string, TimeSpan)>()
				{
					("1s", TimeSpan.FromSeconds(1)),
					("10s", TimeSpan.FromSeconds(10)),
					("2m", TimeSpan.FromMinutes(2)),
					("15m", TimeSpan.FromMinutes(15)),
					("3h", TimeSpan.FromHours(3)),
					("13h", TimeSpan.FromHours(13)),
					("4d", TimeSpan.FromDays(4)),
					("20d", TimeSpan.FromDays(20)),
					("4w", TimeSpan.FromDays(4 * 7)),
					("5w", TimeSpan.FromDays(5 * 7)),
					("9n", TimeSpan.FromDays(9 * 28)),
					("13n", TimeSpan.FromDays(13 * 28)),
				};

				CombineAssertions(() =>
				{
					testData.ForEach(x =>
					{
						// Act
						var result = ServiceTaskScheduleValidation.GetPeriodDuration(x.Item1, TimeSpan.MaxValue, isRandomPeriod: true);

						// Assert
						AssertGreaterThanOrEqualTo(result, TimeSpan.Zero);
						AssertLessThanOrEqualTo(result, x.Item2);
					});
				});
			}
		}

		public class ParseFrequencyTest : TestCase
		{
			public void TestInvalidString()
			{
				// Arrange
				var testString = new List<string>()
				{
					"",
					null,
					"abc",
					"Seconds",
					"Y",
					"20SS",
				};

				CombineAssertions(() =>
				{
					testString.ForEach(testValue =>
					{
						// Act
						var result = ServiceTaskScheduleValidation.ParseFrequency(testValue, out var period, out var type);

						// Assert
						Assert($"{testValue} should not be parsed", !result);
					});
				});
			}

			public void TestSeconds()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"s",
					" s",
					"S",
					"second",
					"seconds"
				};

				// Act
				// Assert
				TestParse(testSuffix, "S");
			}

			public void TestMinutes()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"m",
					" m",
					"M",
					"minute",
					"minutes"
				};

				// Act
				// Assert
				TestParse(testSuffix, "T");
			}

			public void TestHours()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"h",
					" h",
					"H",
					"hour",
					"hours"
				};

				// Act
				// Assert
				TestParse(testSuffix, "H");
			}

			public void TestDays()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"d",
					" d",
					"D",
					"Day",
					"Days"
				};

				// Act
				// Assert
				TestParse(testSuffix, "D");
			}

			public void TestWeeks()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"w",
					" w",
					"W",
					"week",
					"weeks"
				};

				// Act
				// Assert
				TestParse(testSuffix, "W");
			}

			public void TestMonths()
			{
				// Arrange
				var testSuffix = new List<string>()
				{
					"n",
					" n",
					"N",
					"month",
					"months"
				};

				// Act
				// Assert
				TestParse(testSuffix, "M");
			}

			public void TestParse(List<string> suffixList, string expectedType)
			{
				// Arrange
				var testData = new List<(string, int)>()
				{
					("1", 1),
					("40", 40),
					("200", 200),
					("300", 300),
					("6000", 6000),
				};

				CombineAssertions(() =>
				{
					suffixList.ForEach(suffix =>
					{
						testData.ForEach(testValuePair =>
						{
							// Act
							var testString = $"{testValuePair.Item1}{suffix}";
							var result = ServiceTaskScheduleValidation.ParseFrequency(testString, out var period, out var type);

							// Assert
							Assert($"{testString} should be parsed", result);
							AssertEquals($"value [{period}] should not be 0", testValuePair.Item2, period);
							AssertEquals($"recurrence [{type}] should not null", expectedType, type);
						});
					});
				});
			}
		}
	}
}

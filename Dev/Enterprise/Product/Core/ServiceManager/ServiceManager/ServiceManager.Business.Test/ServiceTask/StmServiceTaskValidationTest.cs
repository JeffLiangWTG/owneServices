using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	class StmServiceTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSecondaryProcessesMaxCountDescription()
		{
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>());
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock))
			{
				var serviceTask = Factory.New<StmServiceTask>();
				AssertNoErrors(serviceTask.SecondaryProcessesMaxCountDescriptionInfo);
				serviceTask.SecondaryProcessesMaxCountDescription = "-1";
				AssertHasError(serviceTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
				serviceTask.SecondaryProcessesMaxCountDescription = "1";
				AssertNoErrors(serviceTask.SecondaryProcessesMaxCountDescriptionInfo);
				serviceTask.SecondaryProcessesMaxCountDescription = "10";
				AssertNoErrors(serviceTask.SecondaryProcessesMaxCountDescriptionInfo);
				serviceTask.SecondaryProcessesMaxCountDescription = "100";
				AssertHasError(serviceTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
				serviceTask.SecondaryProcessesMaxCountDescription = "SYSTEM MANAGED";
				AssertNoErrors(serviceTask.SecondaryProcessesMaxCountDescriptionInfo);
				serviceTask.SecondaryProcessesMaxCountDescription = "NULL";
				AssertHasError(serviceTask.SecondaryProcessesMaxCountDescriptionInfo, "Please enter a valid maximum count for secondary processes.");
			}
		}

		public void TestValidateFrequency()
		{
			var task = GetServiceTask(Factory, "1 second", "5s", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorSeconds { Period = 5 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorSeconds { Period = 10 }, true, false, "The task should be run at least every 5 seconds");

			task = GetServiceTask(Factory, "1 second", "15 minutes", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorSeconds { Period = 5 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMinutes { Period = 2 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMinutes { Period = 16 }, true, false, "The task should be run at least every 15 minutes");

			task = GetServiceTask(Factory, "2minute", "1h", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorSeconds { Period = 90 }, true, false, "The task schedule shouldn't be more frequent than every 2 minutes");
			AssertTaskFrequency(task, new NextRunTimeCalculatorSeconds { Period = 150 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMinutes { Period = 45 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 1 }, true, false, "The task should be run at least every 1 hour");

			task = GetServiceTask(Factory, "12hours", "1day", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorHours { Period = 2 }, true, false, "The task schedule shouldn't be more frequent than every 12 hours");
			AssertTaskFrequency(task, new NextRunTimeCalculatorHours { Period = 12 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 2 }, true, false, "The task should be run at least every 1 day");

			task = GetServiceTask(Factory, null, "2 days", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMinutes { Period = 10 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorHours { Period = 49 }, true, false, "The task should be run at least every 2 days");

			task = GetServiceTask(Factory, "1w", null, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 6 }, true, false, "The task schedule shouldn't be more frequent than every 1 week");
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 7 }, false, false, null);

			task = GetServiceTask(Factory, "1n", "6months", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 27 }, true, false, "The task schedule shouldn't be more frequent than every 1 month");
			AssertTaskFrequency(task, new NextRunTimeCalculatorMonthsByLastDay { Period = 1 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMonthsByLastDay { Period = 6 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMonthsByLastDay { Period = 7 }, true, false, "The task should be run at least every 6 months");
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 365 }, true, false, "The task should be run at least every 6 months");
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 168 }, false, false, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 169 }, true, false, "The task should be run at least every 6 months");

			task = GetServiceTask(Factory, "2day", "2day", null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 1 }, true, false, "The task should be run every 2 days");
			AssertTaskFrequency(task, new NextRunTimeCalculatorDays { Period = 3 }, true, false, "The task should be run every 2 days");

			task = GetServiceTask(Factory, null, null, null);
			AssertTaskFrequency(task, new NextRunTimeCalculatorMonthsByLastDay { Period = 1 }, false, false, null);
		}

		void AssertTaskFrequency(StmServiceTask taskSchedule, INextRunTimeCalculator calculator, bool hasErrors, bool hasWarnings, string expectedMessage)
		{
			taskSchedule.NextRunTimeCalculator = calculator;

			AssertEquals("Has Error", hasErrors, taskSchedule.Recurrence.PeriodInfo.HasError(expectedMessage));
			AssertEquals("Has Warning", hasWarnings, taskSchedule.Recurrence.PeriodInfo.HasWarning(expectedMessage));
		}

		public void TestValidateFrequencyWhenUpdateCalculator()
		{
			var task = GetServiceTask(Factory, "2minute", "1h", null);

			task.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 30 };
			AssertEquals("Has Error", expected: false, task.Recurrence.PeriodInfo.HasErrors());

			task.NextRunTimeCalculator = new NextRunTimeCalculatorHours { Period = 30 };
			AssertEquals("Has Error", expected: true, task.Recurrence.PeriodInfo.HasError("The task should be run at least every 1 hour"));

			task.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds { Period = 30 };
			AssertEquals("Has Error", expected: true, task.Recurrence.PeriodInfo.HasError("The task schedule shouldn't be more frequent than every 2 minutes"));
		}

		public void TestValidateFrequencyWhenUpdatePeriod()
		{
			var task = GetServiceTask(Factory, "2minute", "1h", null);

			task.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 30 };
			AssertEquals("Has Error", expected: false, task.Recurrence.PeriodInfo.HasErrors());

			task.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 99 };
			AssertEquals("Has Error", expected: true, task.Recurrence.PeriodInfo.HasError("The task should be run at least every 1 hour"));

			task.NextRunTimeCalculator = new NextRunTimeCalculatorMinutes { Period = 1 };
			AssertEquals("Has Error", expected: true, task.Recurrence.PeriodInfo.HasError("The task schedule shouldn't be more frequent than every 2 minutes"));
		}

		static StmServiceTask GetServiceTask(BusinessObjectFactory factory, string minimumPeriod, string maximumPeriod, string requiresCompanyInCountry, bool canRunInAnyBranch = false)
		{
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock &&
				a.MinimumPeriod == minimumPeriod &&
				a.MaximumPeriod == maximumPeriod &&
				a.RequiresCompanyInCountry == requiresCompanyInCountry &&
				a.CanRunInAnyBranch == canRunInAnyBranch);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var task = factory.New<StmServiceTask>();
				_ = task.StaticServiceAttributes;

				return task;
			}
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

		public void TestValidateCalculatorWeeks()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);
			AssertNoErrors("Precondition: Config should not have errors.", serviceTask.Recurrence.MondayInfo);

			// Act
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWeeks { Period = 1, DaysOfOccurrence = new[] { DayOfWeek.Wednesday } };

			// Assert
			AssertNoErrors(serviceTask.Recurrence.MondayInfo);
		}

		public void TestValidateCalculatorMonths()
		{
			// Arrange
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);

			AssertNotNull("Precondition: Recurrence adapter instance is set", serviceTask.Recurrence);
			AssertNoErrors("Precondition: Config should not have errors.", serviceTask.Recurrence.DayOfMonthInfo);

			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 28 };

			AssertNoErrors(serviceTask.Recurrence.DayOfMonthInfo);

			// Act
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate { Period = 1, DayOfOccurrence = 32 };

			// Assert
			AssertHasError(serviceTask.Recurrence.DayOfMonthInfo, "Please enter a number from 1 to 28 for Day of the Month.");

			// Act
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorMonthsByDate { Period = 1, DayOfOccurrence = 2 };

			// Assert
			AssertNoErrors(serviceTask.Recurrence.DayOfMonthInfo);
		}

		public void TestValidateCalculatorYears()
		{
			// Arrange
			var message = "Please select a valid day for the chosen month.";
			CombineAssertions(() =>
			{
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 1, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 1, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 2, Day = 28 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 2, Day = 29 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 3, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 3, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 4, Day = 30 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 4, Day = 31 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 5, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 5, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 6, Day = 30 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 6, Day = 31 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 7, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 7, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 8, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 8, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 9, Day = 30 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 9, Day = 31 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 10, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 10, Day = 32 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 11, Day = 30 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 11, Day = 31 }, AssertHasError, message);

				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 12, Day = 31 }, AssertNoError, message);
				TestCalculatorYearsValidation(new NextRunTimeCalculatorYearsByDate { Month = 12, Day = 32 }, AssertHasError, message);
			});
		}

		public void TestValidatePeriodIsSet()
		{
			// Arrange
			CombineAssertions(() =>
			{
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorSeconds(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMinutes(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorHours(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorDays(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorWeeks(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByDate(), AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByLastDay(), AssertHasError, "Period cannot be zero or negative.");

				TestCalculatorPeriodValidation(new NextRunTimeCalculatorSeconds { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMinutes { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorHours { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorDays { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorWeeks { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByDate { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByLastDay { Period = 0 }, AssertHasError, "Period cannot be zero or negative.");

				TestCalculatorPeriodValidation(new NextRunTimeCalculatorSeconds { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMinutes { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorHours { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorDays { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorWeeks { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByDate { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByLastDay { Period = -1 }, AssertHasError, "Period cannot be zero or negative.");

				TestCalculatorPeriodValidation(new NextRunTimeCalculatorWorkingDays(), AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByDayOfWeek(), AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorYearsByDate(), AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorYearsByDayOfMonth(), AssertNoError, "Period cannot be zero or negative.");

				TestCalculatorPeriodValidation(new NextRunTimeCalculatorSeconds { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMinutes { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorHours { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorDays { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorWeeks { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByDate { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
				TestCalculatorPeriodValidation(new NextRunTimeCalculatorMonthsByLastDay { Period = 1 }, AssertNoError, "Period cannot be zero or negative.");
			});
		}

		void TestCalculatorPeriodValidation(INextRunTimeCalculator calculator, Action<ZPropertyInfo, string> assertion, string expectedMessage)
		{
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);

			// Act
			serviceTask.NextRunTimeCalculator = calculator;

			// Assert
			assertion(serviceTask.Recurrence.PeriodInfo, expectedMessage);
		}

		void TestCalculatorYearsValidation(INextRunTimeCalculator calculator, Action<ZPropertyInfo, string> assertion, string expectedMessage)
		{
			var serviceTask = GetServiceTask(Factory, "1second", "1year", null);

			// Act
			serviceTask.NextRunTimeCalculator = calculator;

			// Assert
			assertion(serviceTask.Recurrence.YearlyDayInfo, expectedMessage);
		}

		public void TestSRRSecondaryProcessesMaxCountLessThanMaxConcurrentReportsValidation()
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
				var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == "15minutes");

				var canRunInAnyBranch = false;

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Code == "SRR" &&
					a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.PrintJobProcessor.SRRSpecificValidation" &&
					a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.PrintJobProcessor" &&
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == defaultScheduleMock &&
					a.MinimumPeriod == "1second" &&
					a.MaximumPeriod == "1year" &&
					a.RequiresCompanyInCountry == null &&
					a.CanRunInAnyBranch == canRunInAnyBranch);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

				var serviceTask = Factory.New<StmServiceTask>();
				serviceTask.SST_ServiceTaskCode = "SRR";
				_ = serviceTask.StaticServiceAttributes;

				const int maxValue = 2;
				using var registry = SystemDataRegistry.Instance.ReportMaxConnections.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, maxValue);

				// Act
				serviceTask.SecondaryProcessesMaxCount = value;

				// Assert
				AssertEquals(message, expected, serviceTask.HasErrors);

				if (expected)
				{
					var expectedErrorMessage = "Maximum count of secondary processes must be less than \"" + SystemDataRegistry.Instance.ReportMaxConnections.Caption + "\" value in Registry which is set to 2.";
					AssertHasErrorContaining(serviceTask.SecondaryProcessesMaxCountInfo, expectedErrorMessage);
				}
			}
		}

		public void TestOMSSecondaryProcessesMaxCount_SMTPServerIsNotOffice365()
		{
			// Arrange
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var canRunInAnyBranch = false;

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "SRR" &&
				a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
				a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor" &&
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock &&
				a.MinimumPeriod == "1second" &&
				a.MaximumPeriod == "1year" &&
				a.RequiresCompanyInCountry == null &&
				a.CanRunInAnyBranch == canRunInAnyBranch);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.SST_ServiceTaskCode = "OMS";
			_ = serviceTask.StaticServiceAttributes;

			using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "not.office365"))
			{
				// Act
				serviceTask.SecondaryProcessesMaxCount = 4;

				// Assert
				Assert("SMTPServer is abc, should have no warnings", !serviceTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should not be populated",
					string.Empty,
					serviceTask.ExtendedConfigProcessesMaxCountWarningMessage);
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should not  be populated",
					string.Empty,
					serviceTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());
			}
		}

		public void TestOMSSecondaryProcessesMaxCount_SMTPServerIsOffice365()
		{
			CombineAssertions(() =>
			{
				Test(0, false, "Default value, should have no warnings", null, null);
				Test(3, false, "SecondaryProcessesMaxCount(3) <= 3, should have no warnings", null, null);
				var expectedWarningMessage = "Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.";
				var expectedWarningLink = @"https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages";
				Test(4, true, "SecondaryProcessesMaxCount(4) > 3, should have warnings", expectedWarningMessage, expectedWarningLink);
			});

			void Test(int value, bool hasError, string message, string expectedWarningMsg, string expectedWarningLink)
			{
				// Arrange
				var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == "15minutes");

				var canRunInAnyBranch = false;

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Code == "SRR" &&
					a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
					a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor" &&
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == defaultScheduleMock &&
					a.MinimumPeriod == "1second" &&
					a.MaximumPeriod == "1year" &&
					a.RequiresCompanyInCountry == null &&
					a.CanRunInAnyBranch == canRunInAnyBranch);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

				var serviceTask = Factory.New<StmServiceTask>();
				serviceTask.SST_ServiceTaskCode = "OMS";
				_ = serviceTask.StaticServiceAttributes;

				using (Env.Registry.RawRegistry.SMTPServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "smtp.office365.com"))
				{
					// Act
					serviceTask.SecondaryProcessesMaxCount = value;

					// Assert
					AssertEquals(message, hasError, serviceTask.HasWarnings);
					if (hasError)
					{
						AssertEquals(
							"ExtendedConfigurationWarningMessage should be populated",
							"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
							serviceTask.ExtendedConfigProcessesMaxCountWarningMessage.ToString());
						AssertEquals(
							"ExtendedConfigurationWarningMessageLink should be populated",
							expectedWarningLink,
							serviceTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());

						AssertHasWarningContaining(serviceTask.SecondaryProcessesMaxCountInfo, expectedWarningMsg);
					}
				}
			}
		}

		public void TestOMSSecondaryProcessesMaxCount_NotUseGraphApiForOutgoing()
		{
			// Arrange
			var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
				d.RunEvery == "15minutes");

			var canRunInAnyBranch = false;

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Code == "SRR" &&
				a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
				a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor" &&
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == defaultScheduleMock &&
				a.MinimumPeriod == "1second" &&
				a.MaximumPeriod == "1year" &&
				a.RequiresCompanyInCountry == null &&
				a.CanRunInAnyBranch == canRunInAnyBranch);

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

			var serviceTask = Factory.New<StmServiceTask>();
			serviceTask.SST_ServiceTaskCode = "OMS";
			_ = serviceTask.StaticServiceAttributes;

			using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Act
				serviceTask.SecondaryProcessesMaxCount = 4;

				// Assert
				Assert("SMTPServer is abc, should have no warnings", !serviceTask.HasWarnings);
				AssertEquals(
					"ExtendedConfigurationWarningMessage should not be populated",
					string.Empty,
					serviceTask.ExtendedConfigProcessesMaxCountWarningMessage);
				AssertEquals(
					"ExtendedConfigurationWarningMessageLink should not  be populated",
					string.Empty,
					serviceTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());
			}
		}

		public void TestOMSSecondaryProcessesMaxCount_UseGraphApiForOutgoing()
		{
			CombineAssertions(() =>
			{
				Test(0, false, "Default value, should have no warnings", null, null);
				Test(3, false, "SecondaryProcessesMaxCount(3) <= 3, should have no warnings", null, null);
				var expectedWarningMessage = "Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages for details.";
				var expectedWarningLink = @"https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages";
				Test(4, true, "SecondaryProcessesMaxCount(4) > 3, should have warnings", expectedWarningMessage, expectedWarningLink);
			});

			void Test(int value, bool hasError, string message, string expectedWarningMsg, string expectedWarningLink)
			{
				// Arrange
				var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == "15minutes");

				var canRunInAnyBranch = false;

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Code == "SRR" &&
					a.TaskSpecificValidationTypeName == "Enterprise.ServiceManager.Tasks.MailProcessor.OMSSpecificValidation" &&
					a.TaskSpecificValidationTypeAssemblyName == "Enterprise.ServiceManager.Tasks.MailProcessor" &&
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == defaultScheduleMock &&
					a.MinimumPeriod == "1second" &&
					a.MaximumPeriod == "1year" &&
					a.RequiresCompanyInCountry == null &&
					a.CanRunInAnyBranch == canRunInAnyBranch);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using var attribute = ObjectFactory.Substitute(hostedServiceProviderMock.Object);

				var serviceTask = Factory.New<StmServiceTask>();
				serviceTask.SST_ServiceTaskCode = "OMS";
				_ = serviceTask.StaticServiceAttributes;

				using (Env.Registry.RawRegistry.UseGraphApiForOutgoing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					serviceTask.SecondaryProcessesMaxCount = value;

					// Assert
					AssertEquals(message, hasError, serviceTask.HasWarnings);

					if (hasError)
					{
						AssertEquals(
							"ExtendedConfigurationWarningMessage should be populated",
							"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the Exchange Online/smtp.office365.com/smtp-mail.outlook.com server imposing a throttle limit for excessive concurrent connections. Please refer to the following for details:",
							serviceTask.ExtendedConfigProcessesMaxCountWarningMessage.ToString());
						AssertEquals(
							"ExtendedConfigurationWarningMessageLink should be populated",
							expectedWarningLink,
							serviceTask.ExtendedConfigProcessesMaxCountWarningLink.ToString());

						AssertHasWarningContaining(serviceTask.SecondaryProcessesMaxCountInfo, expectedWarningMsg);
					}
				}
			}
		}
	}
}

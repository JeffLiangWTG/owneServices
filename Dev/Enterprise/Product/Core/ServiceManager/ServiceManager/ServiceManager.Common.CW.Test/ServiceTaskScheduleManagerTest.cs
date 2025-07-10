using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Common.CW1.Test
{
	public class ServiceTaskScheduleManagerTest : TestCase
	{
		public void TestConfigureSchedulesInsertsNewSchedules_AllEmpty() => AssertConfiguredSchedules(
			Enumerable.Empty<IHostedServiceAttribute>(),
			Enumerable.Empty<IServiceTask>(),
			Enumerable.Empty<IServiceTask>());
		public void TestConfigureSchedulesInsertsNewSchedules_AllNewSchedules() => AssertConfiguredSchedules(
			new [] { Mock.Of<IHostedServiceAttribute>(a => a.Code == "ABC"), Mock.Of<IHostedServiceAttribute>(a => a.Code == "BCD"), },
			Enumerable.Empty<IServiceTask>(),
			new[] { Mock.Of<IServiceTask>(t => t.Code == "ABC" && t.Description == "new"), Mock.Of<IServiceTask>(t => t.Code == "BCD" && t.Description == "new"), });
		public void TestConfigureSchedulesInsertsNewSchedules_AllExistingSchedules() => AssertConfiguredSchedules(
			new[] { Mock.Of<IHostedServiceAttribute>(a => a.Code == "ABC"), Mock.Of<IHostedServiceAttribute>(a => a.Code == "BCD"), },
			new[] { Mock.Of<IServiceTask>(t => t.Code == "ABC" && t.Description == "old" && t.Pk == Guid.NewGuid()), Mock.Of<IServiceTask>(t => t.Code == "BCD" && t.Description == "old" && t.Pk == Guid.NewGuid()), },
			new[] { Mock.Of<IServiceTask>(t => t.Code == "ABC" && t.Description == "old"), Mock.Of<IServiceTask>(t => t.Code == "BCD" && t.Description == "old"), });
		public void TestConfigureSchedulesInsertsNewSchedules_MixedSchedules() => AssertConfiguredSchedules(
			new[] { Mock.Of<IHostedServiceAttribute>(a => a.Code == "ABC"), Mock.Of<IHostedServiceAttribute>(a => a.Code == "BCD"), Mock.Of<IHostedServiceAttribute>(a => a.Code == "CDE"), Mock.Of<IHostedServiceAttribute>(a => a.Code == "DEF"), },
			new[] { Mock.Of<IServiceTask>(t => t.Code == "CDE" && t.Description == "old" && t.Pk == Guid.NewGuid()), Mock.Of<IServiceTask>(t => t.Code == "DEF" && t.Description == "old" && t.Pk == Guid.NewGuid()), },
			new[] { Mock.Of<IServiceTask>(t => t.Code == "ABC" && t.Description == "new"), Mock.Of<IServiceTask>(t => t.Code == "BCD" && t.Description == "new"), Mock.Of<IServiceTask>(t => t.Code == "CDE" && t.Description == "old"), Mock.Of<IServiceTask>(t => t.Code == "DEF" && t.Description == "old"), });

		void AssertConfiguredSchedules(
			IEnumerable<IHostedServiceAttribute> hostedServiceAttributes,
			IEnumerable<IServiceTask> preLoadedServiceTasks,
			IEnumerable<IServiceTask> expectedServiceTasks)
		{
			// Arrange
			var preLoadedServiceTasksArray = preLoadedServiceTasks.ToArray();
			var hostedServiceAttributesArray = hostedServiceAttributes.ToArray();

			var expectedNewAttributes = hostedServiceAttributesArray
				.Where(a => !preLoadedServiceTasksArray.Select(t => t.Code).Contains(a.Code))
				.ToList();
			serviceTaskLoaderMock
				.Setup(l => l.LoadAll())
				.Returns(preLoadedServiceTasksArray);
			var serviceTaskGovernorDictionary = new Dictionary<IHostedServiceAttribute, Mock<IServiceTaskGovernor>>();
			foreach (var attribute in expectedNewAttributes)
			{
				var branchCode = attribute.Code;
				var serviceTaskGovernorMock = new Mock<IServiceTaskGovernor>();
				serviceTaskGovernorMock
					.SetupGet(g => g.GovernedTask)
					.Returns(Mock.Of<IServiceTask>(t => t.Code == attribute.Code && t.Description == "new"));
				serviceTaskRequirementsCheckerMock
					.Setup(g => g.AttributeSatisfiesRequirements(It.Is<IHostedServiceAttribute>(a => a.Code == attribute.Code), out branchCode))
					.Returns(true);
				transactionAdapterMock
					.Setup(a => a.GetNewServiceTaskGovernor(attribute))
					.Returns(serviceTaskGovernorMock.Object);
				serviceTaskGovernorDictionary.Add(attribute, serviceTaskGovernorMock);
			}

			foreach (var task in preLoadedServiceTasksArray)
			{
				var governorMock = new Mock<IServiceTaskGovernor>();
				governorMock
					.SetupGet(g => g.GovernedTask)
					.Returns(task);
				transactionAdapterMock
					.Setup(a => a.GetServiceTaskGovernor(task.Pk))
					.Returns(governorMock.Object);
			}

			// Act
			var result = serviceTaskScheduleManager.ConfigureSchedules(hostedServiceAttributesArray);

			// Assert
			AssertContainsExactElementsInAnyOrder(new ServiceTaskEqualityComparer() , expectedServiceTasks, result);
			foreach (var attribute in expectedNewAttributes)
			{
				var branchCode = attribute.Code;
				transactionAdapterMock.Verify(a => a.GetNewServiceTaskGovernor(attribute), Times.Once);
				defaultScheduleConfigurerMock.Verify(c => c.SetDefaultScheduleForTask(It.Is<IServiceTaskGovernor>(g => g.GovernedTask.Code == attribute.Code && g.GovernedTask.Description == "new"), attribute), Times.Once);
				serviceTaskRequirementsCheckerMock.Verify(g => g.AttributeSatisfiesRequirements(attribute, out branchCode), Times.Once);
				serviceTaskGovernorDictionary[attribute].Verify(g => g.SetBranchFromCode(attribute.Code), Times.Once);
			}
			serviceTaskLoaderMock.Verify(l => l.LoadAll(), Times.Once);
			serviceTaskLoaderMock.VerifyNoOtherCalls();
			transactionAdapterMock.Verify(a => a.Commit(), Times.Once);
			defaultScheduleConfigurerMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodingConvention", "WTG1008:Do not compare bool to a constant value in an expression.", Justification = "Mock.Of")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0100:Remove redundant equality", Justification = "Mock.Of")]
		public void TestMandatoryScheduleIsReactivated()
		{
			// Arrange
			var pk = Guid.NewGuid();
			var hostedServiceAttributes = new[] { Mock.Of<IHostedServiceAttribute>(a => a.Code == "ODT" && a.IsMandatory == true) };
			var mandatoryTaskMock = new Mock<IServiceTask>();
			mandatoryTaskMock.Setup(t => t.Code).Returns("ODT");
			mandatoryTaskMock.Setup(t => t.Pk).Returns(pk);
			mandatoryTaskMock.Setup(t => t.IsActive).Returns(false);
			var preLoadedServiceTasks = new[] { mandatoryTaskMock.Object };
			var hostedServiceAttributesArray = hostedServiceAttributes.ToArray();
			var serviceTaskGovernerMock = new Mock<IServiceTaskGovernor>();
			serviceTaskGovernerMock
				.SetupGet(g => g.GovernedTask)
				.Returns(mandatoryTaskMock.Object);
			serviceTaskLoaderMock
				.Setup(l => l.LoadAll())
				.Returns(preLoadedServiceTasks);
			transactionAdapterMock
				.Setup(a => a.GetServiceTaskGovernor(pk))
				.Returns(serviceTaskGovernerMock.Object);

			// Act
			var result = serviceTaskScheduleManager.ConfigureSchedules(hostedServiceAttributesArray).Single();

			// Assert
			AssertEquals(pk, result.Pk);
			AssertEquals("ODT", result.Code);
			serviceTaskGovernerMock.Verify(t => t.SetActive(true));
		}

		[ExpectNoExceptions]
		public void TestTransactionAdapterIsDisposed()
		{
			// Arrange
			var hostedServiceAttributes = new[] { Mock.Of<IHostedServiceAttribute>(a => a.Code == "~~T" && a.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute")) }.ToArray();
			var pk = Guid.NewGuid();
			var mandatoryTaskMock = Mock.Of<IServiceTask>(t => t.Code == "~~T" && t.Pk == pk && !t.IsActive);
			var preLoadedServiceTasks = new[] { mandatoryTaskMock };
			var serviceTaskGovernerMock = new Mock<IServiceTaskGovernor>();
			serviceTaskGovernerMock
				.SetupGet(g => g.GovernedTask)
				.Returns(mandatoryTaskMock);
			serviceTaskLoaderMock
				.Setup(l => l.LoadAll())
				.Returns(preLoadedServiceTasks);
			transactionAdapterMock
				.Setup(a => a.GetServiceTaskGovernor(pk))
				.Returns(serviceTaskGovernerMock.Object);

			// Act
			var result = serviceTaskScheduleManager.ConfigureSchedules(hostedServiceAttributes).Single();

			// Assert
			transactionAdapterMock.Verify(t => t.Dispose(), Times.Once());
		}

		class ServiceTaskEqualityComparer : IEqualityComparer<IServiceTask>
		{
			public bool Equals(IServiceTask x, IServiceTask y)
			{
				return x.Code == y.Code &&
						x.Description == y.Description;
			}

			public int GetHashCode(IServiceTask obj)
			{
				return obj.GetHashCode();
			}
		}

		class ServiceTaskScheduleManagerFactoryTests : TestCaseWithFactory
		{
			public void TestCreatesOneRecord()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock });
				var serviceTasksProvider = new ServiceTaskScheduleCollectionProvider();

				// Assert
				var serviceTaskScheduleCollection = serviceTasksProvider.Load(Factory);
				AssertEquals(1, serviceTaskScheduleCollection.Tasks.Count);
			}

			public void TestCode_1() => AssertCode("123");
			public void TestCode_2() => AssertCode("###");

			void AssertCode(string code)
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == code
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(code, result.Code);
			}

			public void TestDescription_1() => AssertDescription("###DESC");
			public void TestDescription_2() => AssertDescription("123DESC");

			void AssertDescription(string description)
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == description.Substring(0, 3)
								&& attribute.Description == description
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(description, result.Description);
			}

			[TestDate(2008, 2, 7, 13, 0, 0)]
			public void TestNextRuntime()
			{
				// Arrange
				var testDate = new DateTime(2008, 2, 7, 13, 0, 0);

				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "###"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				dateTimeProviderMock
					.Setup(c => c.CurrentDateTimeUtc)
					.Returns(testDate);

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(new ZDateTime(2008, 2, 7, 13, 0, 0, DateTimeKind.Utc).UtcToDateTimeOffset(), result.NextRunTime);
			}

			public void TestScheduleState()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "###"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals("", result.SettingsXml);
			}

			public void TestBranchIsNotEmpty()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertNotEquals(Guid.Empty, result.BranchPk);
			}

			public void TestBranchIsEmptyForCanRunInAnyBranch()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.CanRunInAnyBranch
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(Guid.Empty, result.BranchPk);
			}

			protected override void SetUp()
			{
				base.SetUp();
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				serviceTaskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
				transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
				schedulerServiceTaskTransactionAdapter = new SchedulerServiceTaskTransactionAdapter();
				serviceTaskLoaderFactoryMock
					.Setup(x => x.CreateServiceTaskLoader())
					.Returns(new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)));
				transactionAdapterFactoryMock
					.Setup(x => x.CreateTransactionAdapter())
					.Returns(schedulerServiceTaskTransactionAdapter);
				serviceTaskScheduleManager = new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, new DefaultScheduleConfigurer(dateTimeProviderMock.Object), new ServiceTaskRequirementsChecker());
			}

			protected override void TearDown()
			{
				base.TearDown();
				schedulerServiceTaskTransactionAdapter.Dispose();
			}

			SchedulerServiceTaskTransactionAdapter schedulerServiceTaskTransactionAdapter;
			Mock<IDateTimeProvider> dateTimeProviderMock;
			ServiceTaskScheduleManager serviceTaskScheduleManager;
			Mock<IServiceTaskLoaderFactory> serviceTaskLoaderFactoryMock;
			Mock<ITransactionAdapterFactory> transactionAdapterFactoryMock;
		}

		class ServiceTaskScheduleManagerFactoryTestsNew : TestCaseWithFactory
		{
			public void TestCreatesOneRecord()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock });

				// Assert
				var sqlText = "SELECT COUNT(*) FROM StmServiceTask;";
				var count = TestConnection.ExecuteScalar<int>(sqlText);
				AssertEquals(1, count);
			}

			public void TestCode_1() => AssertCode("123");
			public void TestCode_2() => AssertCode("###");

			void AssertCode(string code)
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == code
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(code, result.Code);
			}

			public void TestDescription_1() => AssertDescription("###DESC");
			public void TestDescription_2() => AssertDescription("123DESC");

			void AssertDescription(string description)
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == description.Substring(0, 3)
								&& attribute.Description == description
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				attributeProviderMock
					.Setup(x => x.GetClientHostedServiceAttribute(description.Substring(0, 3)))
					.Returns(hostedServiceConfigMock);

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(description, result.Description);
			}

			[TestDate(2008, 2, 7, 13, 0, 0)]
			public void TestNextRuntime()
			{
				// Arrange
				var testDate = new DateTime(2008, 2, 7, 13, 0, 0);

				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "###"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				dateTimeProviderMock
					.Setup(c => c.CurrentDateTimeUtc)
					.Returns(testDate);

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(new ZDateTime(2008, 2, 7, 13, 0, 0, DateTimeKind.Utc).UtcToDateTimeOffset(), result.NextRunTime);
			}

			public void TestScheduleState()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "###"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertContains("<NextRunTimeCalculatorMinutes Period=\"1\" />", result.SettingsXml);
			}

			public void TestBranchIsNotEmpty()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertNotEquals(Guid.Empty, result.BranchPk);
			}

			public void TestBranchIsEmptyForCanRunInAnyBranch()
			{
				// Arrange
				var hostedServiceConfigMock = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "XXX"
								&& attribute.CanRunInAnyBranch
								&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

				// Act
				var result = serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceConfigMock })
					.Single();

				// Assert
				AssertEquals(Guid.Empty, result.BranchPk);
			}

			protected override void SetUp()
			{
				base.SetUp();
				dateTimeProviderMock = new Mock<IDateTimeProvider>();
				serviceTaskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
				attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				var bindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
				var statusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
				transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
				nativeServiceTaskTransactionAdapter = new NativeServiceTaskTransactionAdapter(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object);
				serviceTaskLoaderFactoryMock
					.Setup(x => x.CreateServiceTaskLoader())
					.Returns(new NativeServiceTaskLoader(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object));
				transactionAdapterFactoryMock
					.Setup(x => x.CreateTransactionAdapter())
					.Returns(nativeServiceTaskTransactionAdapter);
				serviceTaskScheduleManager = new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, new DefaultScheduleConfigurer(dateTimeProviderMock.Object), new ServiceTaskRequirementsChecker());
			}

			protected override void TearDown()
			{
				base.TearDown();
				nativeServiceTaskTransactionAdapter.Dispose();
			}

			NativeServiceTaskTransactionAdapter nativeServiceTaskTransactionAdapter;
			Mock<IDateTimeProvider> dateTimeProviderMock;
			ServiceTaskScheduleManager serviceTaskScheduleManager;
			Mock<IServiceTaskLoaderFactory> serviceTaskLoaderFactoryMock;
			Mock<ITransactionAdapterFactory> transactionAdapterFactoryMock;
			Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceTaskScheduleManager(null, transactionAdapterFactoryMock.Object, defaultScheduleConfigurerMock.Object, serviceTaskRequirementsCheckerMock.Object));
				AssertEquals("serviceTaskLoaderFactory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, null, defaultScheduleConfigurerMock.Object, serviceTaskRequirementsCheckerMock.Object));
				AssertEquals("transactionAdapterFactory", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, null, serviceTaskRequirementsCheckerMock.Object));
				AssertEquals("defaultScheduleConfigurer", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, defaultScheduleConfigurerMock.Object, null));
				AssertEquals("serviceTaskRequirementsChecker", result.ParamName);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			serviceTaskLoaderMock = new Mock<IServiceTaskLoader>();
			transactionAdapterMock = new Mock<ITransactionAdapter>();
			defaultScheduleConfigurerMock = new Mock<IDefaultScheduleConfigurer>();
			serviceTaskRequirementsCheckerMock = new Mock<IServiceTaskRequirementsChecker>();
			serviceTaskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
			serviceTaskLoaderFactoryMock
				.Setup(f => f.CreateServiceTaskLoader())
				.Returns(serviceTaskLoaderMock.Object);
			transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
			transactionAdapterFactoryMock
				.Setup(f => f.CreateTransactionAdapter())
				.Returns(transactionAdapterMock.Object);
			serviceTaskScheduleManager = new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, defaultScheduleConfigurerMock.Object, serviceTaskRequirementsCheckerMock.Object);
		}

		ServiceTaskScheduleManager serviceTaskScheduleManager;
		Mock<IServiceTaskLoader> serviceTaskLoaderMock;
		Mock<ITransactionAdapter> transactionAdapterMock;
		Mock<IDefaultScheduleConfigurer> defaultScheduleConfigurerMock;
		Mock<IServiceTaskRequirementsChecker> serviceTaskRequirementsCheckerMock;
		Mock<IServiceTaskLoaderFactory> serviceTaskLoaderFactoryMock;
		Mock<ITransactionAdapterFactory> transactionAdapterFactoryMock;
	}
}

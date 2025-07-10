using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	[TestsSubclassesOf(typeof(ServiceProviderImpl))]
	public abstract class ServiceTaskTestCase<T> : TestCaseWithFactory where T : ServiceProviderImpl
	{
		[ExpectNoExceptions]
		public void TestTasksWithMutuallyExclusiveTaskGroupMustNotAllowMultiple()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null && thisClassAttribute.MutuallyExclusiveTaskGroup != MutuallyExclusiveServiceTaskGroups.NoGroup)
			{
				AssertEquals("AllowsMultipleInstances=true is not supported for tasks that cannot run at the same time as others", false, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		[ExpectNoExceptions]
		public void TestExistenceOfMinimumPeriod()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				var result = thisClassAttribute.MinimumPeriod;
				AssertNotNullOrEmpty($"Please provide value for {nameof(HostedServiceAttribute)}.{nameof(HostedServiceAttribute.MinimumPeriod)}", result);
			}
		}

		public void TestBranchIndependent()
		{
			// Arrange
			const string wikiLink = "https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/5509/Service-task-user-context";
			var thisClassAttribute = GetHostedServiceAttributeOrFail();
			var canRunInAnyBranch = thisClassAttribute.CanRunInAnyBranch;
			var taskKey = $"{thisClassAttribute.Code}-{GetType().FullName}";

			// Act
			var result = canRunInAnyBranch
						|| BranchDependentTasks.Contains(taskKey);

			// Assert
			HtmlAssertEquals($@"Service task [{thisClassAttribute.Code} {thisClassAttribute.Description}] in [{GetType().FullName}] is branch specific. All service tasks should be able to run in any branch.<br/>
If a specific piece of work being processed requires a specific branch, that context should be applied at that time, rather than having specify the whole service task as running in that branch.<br/>
Please set the value for <i>{nameof(HostedServiceAttribute)}.{nameof(HostedServiceAttribute.CanRunInAnyBranch)} = {true}</i><br/>
For more information please refer to our Wiki <a href=""{wikiLink}"" target=""_blank"" rel=""noopener noreferrer"">{wikiLink}</a>.<br/>
",
				true,
				result);
		}

		public void TestNotIncludedInBranchDependentTasksIfNotBranchDependent()
		{
			// Arrange
			var thisClassAttribute = GetHostedServiceAttributeOrFail();
			var canRunInAnyBranch = thisClassAttribute.CanRunInAnyBranch;
			var taskKey = $"{thisClassAttribute.Code}-{GetType().FullName}";

			// Act
			var result = canRunInAnyBranch
						&& BranchDependentTasks.Contains(taskKey);

			// Assert
			HtmlAssertEquals($@"Service task [{thisClassAttribute.Code} {thisClassAttribute.Description}] in [{GetType().FullName}] is branch agnostic.<br/>
Please exclude it from the base line list in [{typeof(ServiceTaskTestCase<>).FullName}.{nameof(BranchDependentTasks)}].<br/>
",
				false,
				result);
		}

		public void TestDoesNotContainStaticFields()
		{
			foreach (var fieldInfo in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
			{
				if (fieldInfo.FieldType.IsSubclassOf(typeof(Delegate)))
				{
					continue;
				}

				Assert(string.Format(
					"An IServiceTaskHandler implementation should contain only readonly static fields because its AppDomain can be unloaded. Please refactor the usage of {0}:{1} field.",
					typeof(T).Name, fieldInfo.Name), fieldInfo.IsInitOnly || fieldInfo.IsLiteral);
			}
			Assert(true);
		}

		public void TestHostedServiceRequirementMethods()
		{
			AssertHostedServiceRequirementMethods(typeof(HostedServiceRequirementAttribute), typeof(string));
		}

		public void TestHostedServiceRequirementsMethods()
		{
			AssertHostedServiceRequirementMethods(typeof(HostedServiceRequirementsAttribute), typeof(string[]));
		}

		public void TestConfigControlTypeInheritance()
		{
			foreach (var serviceTaskAttribute in GetHostedServiceAttributes())
			{
				if (serviceTaskAttribute.ConfigControlType != null)
				{
					Type t;
					if (serviceTaskAttribute.ConfigControlType.IsInterface)
					{
						var obj = CargoWise.Application.ObjectFactory.Get(serviceTaskAttribute.ConfigControlType.Name);
						t = obj.GetType();
						((IDisposable)obj).Dispose();
					}
					else
					{
						t = serviceTaskAttribute.ConfigControlType;
					}

					var expectedType = Type.GetType("Enterprise.ZArchitecture.GUI.ZUserControl, Enterprise.ZArchitecture.GUI");
					if (expectedType == null || !t.IsSubclassOf(expectedType))
					{
						Assert(string.Format("The ConfigControlType of {0} should be inheritanted from ZUserControl.", serviceTaskAttribute.ConfigControlType), false);
					}
				}
			}
			Assert(true);
		}

		public void TestTaskSpecificValidationTypeImplementsIServiceTaskSpecificValidation()
		{
			foreach (var serviceTaskAttribute in GetHostedServiceAttributes())
			{
				if (serviceTaskAttribute.TaskSpecificValidationType != null)
				{
					var t = serviceTaskAttribute.TaskSpecificValidationType;

					if (!typeof(IServiceTaskSpecificValidation).IsAssignableFrom(t))
					{
						Assert($"The {nameof(serviceTaskAttribute.TaskSpecificValidationType)} should implement {nameof(IServiceTaskSpecificValidation)}.", false);
					}
				}
			}
			Assert(true);
		}

		public void TestCategoryExists()
		{
			var serviceTaskFilterType = Type.GetType("Enterprise.ServiceManager.Module.StmServiceTaskFilterBusinessObject, Enterprise.ServiceManager.Module", throwOnError: true);
			var serviceTaskFilterBO = Activator.CreateInstance(serviceTaskFilterType!);
			var categories = (CodeDescriptionPairList)serviceTaskFilterBO!.GetType().InvokeMember("Categories", BindingFlags.GetProperty, null, serviceTaskFilterBO, Array.Empty<object>())!;

			foreach (var serviceTaskAttribute in GetHostedServiceAttributes())
			{
				var message = string.Format("'{0}' category should exist in Enterprise.ServiceManager.Module.StmServiceTaskFilterBusinessObject.Categories.",
					serviceTaskAttribute.Category);
				Assert(message, categories.ContainsCode(serviceTaskAttribute.Category));
			}
		}

		public void TestHasDefaultSchedule()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				var defaultSchedule = thisClassAttribute.DefaultScheduleRunEvery;
				AssertNotNullOrEmpty($"Service task should define a valid default schedule in {nameof(HostedServiceAttribute)}.{nameof(HostedServiceAttribute.DefaultScheduleRunEvery)}", defaultSchedule);
			}
		}

		public void TestExpectedBindingAttributesForHostedServices()
		{
			// Arrange
			var actualNudges = BindingAttributesForHostedServices;

			if (actualNudges.Count(x => x.QueueName == null) < 2)
			{
				const string tipMessage = "\r\n Tip: if this fails you may need to run AssemblyMetaDataExtractor.exe";
				var expectedNudges = ExpectedHostedServiceBusinessObjectBindingAttributes;
				AssertContainsExactElementsInAnyOrder("Expected Nudges matches Service Task nudges", expectedNudges.Select(x => x.QueueName), actualNudges.Select(x => x.QueueName));
				CombineAssertions(() =>
				{
					foreach (var expectedNudge in expectedNudges)
					{
						var matchingActualNudge = actualNudges.Single(n => string.Equals(n.QueueName, expectedNudge.QueueName, StringComparison.OrdinalIgnoreCase));

						var allCleanedExpectedPredicates = expectedNudge
							.Predicates
							.Select(Normalise)
							.OrderBy(s => s)
							.ToList();
						var allCleanedActualPredicates = matchingActualNudge
							.Predicates
							.Select(Normalise)
							.OrderBy(s => s)
							.ToList();

						// Act
						// Assert
						AssertContainsExactElementsInAnyOrder($"Predicates mismatch for '{expectedNudge.QueueName}'.{tipMessage}",
							allCleanedExpectedPredicates,
							allCleanedActualPredicates);

						AssertEquals($"Nudge is for right table.{tipMessage}",
							matchingActualNudge.Table,
							expectedNudge.Table);
					}
				});
			}
			else
			{
				Assert($"It is not possible to test the {nameof(HostedServiceBusinessObjectBindingAttribute)} when there are multiple QueueNames (key) of null. These will need to be tested by another means.", true);
			}

			string Normalise(string input)
			{
				return System.Text.RegularExpressions.Regex
					.Replace(input, @"\s+", " ")
					.Replace(System.Environment.NewLine, " ");
			}
		}

		public void TestEdiMessageMinimumPredicates()
		{
			var ediMessageHostedServiceAttributes = BindingAttributesForHostedServices.Where(x => x.Table == EDIMessageSchema.Constants.TableName);
			CombineAssertions(() =>
			{
				if (ediMessageHostedServiceAttributes.Any())
				{
					foreach (var hostedServiceAttribute in ediMessageHostedServiceAttributes)
					{
						AssertMinimumPredicates(hostedServiceAttribute, EDIMessageSchema.Constants.EM_IsActive + "=Y", EDIMessageSchema.Constants.EM_Status + "=", EDIMessageSchema.Constants.EM_ReceiveTransmit + "=", EDIMessageSchema.Constants.EM_ApplicationCode + "=");
					}
				}
				else
				{
					Assert($"No {nameof(HostedServiceBusinessObjectBindingAttribute)} for EDIMessage table", true);
				}
			});
		}

		public void TestEdiInterchangeMinimumPredicates()
		{
			var ediInterchangeHostedServiceAttributes = BindingAttributesForHostedServices.Where(x => x.Table == EDIInterchangeSchema.Constants.TableName);
			CombineAssertions(() =>
			{
				if (ediInterchangeHostedServiceAttributes.Any())
				{
					foreach (var hostedServiceAttribute in ediInterchangeHostedServiceAttributes)
					{
						AssertMinimumPredicates(hostedServiceAttribute, EDIInterchangeSchema.Constants.EI_IsActive + "=Y", EDIInterchangeSchema.Constants.EI_Status + "=", EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=", EDIInterchangeSchema.Constants.EI_ApplicationCode + "=");
					}
				}
				else
				{
					Assert($"No {nameof(HostedServiceBusinessObjectBindingAttribute)} for EDIInterchange table", true);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestRequiresCompanyInCountry()
		{
			// Arrange
			var requiredCountries = GetHostedServiceAttribute().RequiresCompanyInCountry?.Split(',');
			if (requiredCountries.IsNullOrEmpty())
			{
				return;
			}

			var allCountries = Enterprise.Core.Constants.CountryCodes.GetAll();
			var countrySet = requiredCountries
				.Select(c => c.Trim().ToUpperInvariant())
				.Where(c => allCountries.Contains(c))
				.ToHashSet();

			// Assert
			AssertContainsExactElementsInAnyOrder($"Normalized required country string is '{string.Join(",", countrySet)}'",
				expected: countrySet, actual: requiredCountries);
		}

		public void TestDefaultScheduleRuntimeIsValidForNudging()
		{
			if (!HostedServicesHasBindingAttribute)
			{
				Assert(true);
				return;
			}

			var attribute = GetHostedServiceAttributes().Single();
			var defaultScheduleRunEvery = attribute.DefaultScheduleRunEvery;
			ServiceTaskScheduleValidation.ParseFrequency(defaultScheduleRunEvery, out int period, out string type);

			CombineAssertions(() =>
			{
				Assert("Default Schedule Run Every should be no less than 15 minutes",
					type != ScheduleType.Seconds || period >= 900);
				Assert("Default Schedule Run Every should be no less than 15 minutes",
					type != ScheduleType.Minutes || period >= 15);
			});
		}

		protected void AssertSingleHostedServiceBusinessObjectBindingAttribute(string expectedServiceTaskCode, string expectedTable, string[] expectedPredicates, string expectedQueueName)
		{
			var attributes = typeof(T).Assembly.GetCustomAttributes(true).OfType<HostedServiceBusinessObjectBindingAttribute>();
			var result = attributes.SingleOrDefault(attribute => attribute.ServiceTaskCode == expectedServiceTaskCode && attribute.Table == expectedTable);

			CombineAssertions($"Combined assertion failed for ServiceTaskCode={expectedServiceTaskCode} with Table={expectedTable}.", () =>
			{
				AssertNotNull($"Expected to find {nameof(HostedServiceBusinessObjectBindingAttribute)}", result);
				AssertEquals("QueueName", expectedQueueName, result?.QueueName);
				AssertContainsExactElementsInAnyOrder("Predicates", expectedPredicates, result?.Predicates);
			});
		}

		protected void AssertHostedServiceBusinessObjectBindingAttribute(HostedServiceBusinessObjectBindingAttribute attribute, string expectedServiceTaskCode, string expectedTable, string[] expectedPredicates, string expectedQueueName)
		{
			AssertEquals("ServiceTaskCode", expectedServiceTaskCode, attribute.ServiceTaskCode);
			AssertEquals("Table", expectedTable, attribute.Table);
			AssertEquals("QueueName", expectedQueueName, attribute.QueueName);
			AssertContainsExactElementsInAnyOrder("Predicates", expectedPredicates, attribute.Predicates);
		}

		protected ServiceTaskTestCase(bool useSameConnectionAcrossThreads = false)
		{
			this.useSameConnectionAcrossThreads = useSameConnectionAcrossThreads;
		}

		protected void AssertSingleHostedServiceAttribute(string expectedCode, string expectedDescription, string expectedCategory)
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			CombineAssertions(() =>
			{
				AssertEquals("Code", expectedCode, hostedServiceAttribute.Code);
				AssertEquals("Description", expectedDescription, hostedServiceAttribute.Description);
				AssertEquals("Category", expectedCategory, hostedServiceAttribute.Category);
			});
		}

		protected HostedServiceAttribute[] GetHostedServiceAttributes()
		{
			var serviceTaskType = typeof(T);
			return serviceTaskType.Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false)
				.Cast<HostedServiceAttribute>()
				.Where(x => x.TypeName.Equals(serviceTaskType.FullName, StringComparison.Ordinal))
				.ToArray();
		}

		protected HostedServiceAttribute GetHostedServiceAttributeOrFail()
		{
			var result = GetHostedServiceAttributes().SingleOrDefault();

			AssertNotNull("Expected HostedServiceAttribute but none found.", result);

			return result!;
		}

		protected TestServiceLogger InitialiseAndRunTaskSchedule(T serviceTask)
		{
			return InitialiseAndRunTaskSchedule(serviceTask, CancellationToken.None);
		}

		protected TestServiceLogger InitialiseAndRunTaskSchedule(T serviceTask, CancellationToken token)
		{
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask, token);

			return log;
		}

		protected TestServiceLogger InitialiseTaskSchedule(T serviceTask)
		{
			return InitialiseTaskSchedule(serviceTask, out _, out _);
		}

		protected TestServiceLogger InitialiseTaskSchedule<TBizObj>(T serviceTask, out TBizObj schedule) where TBizObj : BusinessObject
		{
			var log = InitialiseTaskSchedule(serviceTask, out var scheduleInterface, out _);
			schedule = (TBizObj)(BusinessObject)scheduleInterface;

			return log;
		}

		protected TestServiceLogger InitialiseTaskSchedule(T serviceTask, out IStmServiceTask schedule)
		{
			return InitialiseTaskSchedule(serviceTask, out schedule, out _);
		}

		protected TestServiceLogger InitialiseTaskSchedule(T serviceTask, out IStmServiceTask schedule, out ServiceTaskGovernorProxy scheduleGovernor)
		{
			var attribute = GetHostedServiceAttribute();

			var query = new ZQuery();
			query.AddToFilter(StmServiceTaskSchema.SST_ServiceTaskCode, attribute.Code);

			var loggerFactoryMock = new Mock<ILoggerFactory>();
			loggerFactoryMock
				.Setup(x => x.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new TestServiceLogger());

			using (ObjectFactory.Substitute(loggerFactoryMock.Object))
			{
				serviceTask.ServiceLogger ??= loggerFactoryMock.Object.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, attribute.Code);
				var task = GetTask(serviceTask);
				serviceTaskScheduleManager.ConfigureSchedules(new[] { attribute });
				var taskSchedules = Factory.Load<IStmServiceTask>(query);
				AssertEquals(1, taskSchedules.Length);
				schedule = taskSchedules[0];
				scheduleGovernor = new ServiceTaskGovernorProxy(transactionAdapter, (StmServiceTask)taskSchedules[0]);
			}

			return GetTestLogger(serviceTask);
		}

		IHostedServiceAttribute GetHostedServiceAttribute()
		{
			var attributes = GetHostedServiceAttributes();
			if (attributes.Length != 1)
			{
				throw new InvalidOperationException(
					$"Expected exactly 1 {nameof(HostedServiceAttribute)} on class {typeof(T).Name}, but found {attributes.Length}.");
			}

			return attributes[0];
		}

		protected void RunTaskSchedule(T serviceTask, CancellationToken token)
		{
			GetTask(serviceTask).InitializeRunningEnvironment(GetHostedServiceAttribute(), serviceTask is IServiceTaskConfigurationUser configurationUser ? configurationUser.ConfigString : string.Empty, LoggerMock);
			if (!ObjectFactory.HasBeenSubstituted<INudgingController>())
			{
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					GetTask(serviceTask).Run(token);
				}
			}
			else
			{
				GetTask(serviceTask).Run(token);
			}
		}

		protected void RunTaskSchedule(T serviceTask)
		{
			RunTaskSchedule(serviceTask, CancellationToken.None);
		}

		protected Type GetHostedServiceQueueProviderTypeByServiceTaskCode(string serviceTaskCode)
			=> typeof(T).Assembly
				.GetCustomAttributes(typeof(HostedServiceQueueProviderAttribute))
				.Cast<HostedServiceQueueProviderAttribute>()
				.FirstOrDefault(attribute => string.Equals(attribute.ServiceTaskCode, serviceTaskCode, StringComparison.OrdinalIgnoreCase))
				?.Type;

		protected IHostedServiceQueueProvider GetHostedServiceQueueProviderInstanceByServiceTaskCode(string serviceTaskCode)
		{
			// TODO: push to base class.
			var queueProviderType = GetHostedServiceQueueProviderTypeByServiceTaskCode(serviceTaskCode);
			if (queueProviderType == null)
			{
				return null;
			}

			return (IHostedServiceQueueProvider)Activator.CreateInstance(queueProviderType);
		}

		protected sealed override void SetUp()
		{
			base.SetUp();
			SetupMocks();

			serviceTaskScheduleManager = GetScheduleManager();
			if (useSameConnectionAcrossThreads)
			{
				mainConnectionUsageHolder = ((IDbConnectionMultiThreadControl)Db.Instance).UseMainConnectionAcrossAllThreadsForTests();
			}
			LoggerMock = Mock.Of<ILogger>();
			SetUpCore();

			IServiceTaskScheduleManager GetScheduleManager()
			{
				var type = Assembly.Load("ServiceManager.Common").GetType("ServiceManager.Common.ServiceTaskScheduleManager", throwOnError: true);
				var taskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
				taskLoaderFactoryMock
					.Setup(x => x.CreateServiceTaskLoader())
					.Returns(new NativeServiceTaskLoader(attributeProviderMock, statusProviderMock, bindingsProviderMock, DateTimeMock.Object));
				var transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
				transactionAdapterFactoryMock
					.Setup(x => x.CreateTransactionAdapter())
					.Returns(() => new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, DateTimeMock.Object));
				return (IServiceTaskScheduleManager)Activator.CreateInstance(type, new object[] { taskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, new DefaultScheduleConfigurer(DateTimeMock.Object), new ServiceTaskRequirementsChecker() });
			}
		}

		protected virtual void SetUpCore()
		{
		}

		protected sealed override void TearDown()
		{
			TearDownCore();
			if (mainConnectionUsageHolder != null)
			{
				mainConnectionUsageHolder.Dispose();
			}
			transactionAdapter?.Dispose();
			base.TearDown();
		}

		protected virtual void TearDownCore()
		{
		}

		protected abstract IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes { get; }

		TestServiceLogger GetTestLogger(T serviceTask)
		{
			return (TestServiceLogger)serviceTask.ServiceLogger;
		}

		IServiceTaskHandler GetTask(T serviceTask)
		{
			if (!tasks.TryGetValue(serviceTask, out var result))
			{
				result = (IServiceTaskHandler)Activator.CreateInstance(
					GetHandlerType(),
					new object[] {
						serviceTask,
						new NativeServiceTaskLoader(attributeProviderMock, statusProviderMock, bindingsProviderMock, DateTimeMock.Object),
						ObjectFactory.Get<ILoggerFactory>() });

				tasks.Add(serviceTask, result);
			}

			return result;

			Type GetHandlerType()
			{
#if NET8_0_OR_GREATER
				return Assembly.LoadFrom(Path.Combine(AssemblyLoader.GetBinPath(), "ServiceManager.Runner.CW.dll")).GetType("Enterprise.ServiceManager.Runner.ServiceProviderImplProxy", throwOnError: true);
#else
				return Assembly.LoadFrom(Path.Combine(AssemblyLoader.GetBinPath(), "ServiceManager.Runner.CW.exe")).GetType("Enterprise.ServiceManager.Runner.ServiceProviderImplProxy", throwOnError: true);
#endif
			}
		}

		void SetupMocks()
		{
			DateTimeMock = new Mock<IDateTimeProvider>();

			DateTimeMock
				.Setup(d => d.CurrentDateTimeUtc)
				.Returns(() => TestDateAttribute.Date != DateTime.MinValue
					? DateTime.SpecifyKind(TestDateAttribute.Date, DateTimeKind.Utc)
					: ZDateTime.UtcNow.ToDateTime());

			attributeProviderMock ??= Mock.Of<IClientHostedServiceAttributeProvider>(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()) == GetHostedServiceAttributeOrFail());
			statusProviderMock ??= Mock.Of<IServiceTaskScheduleStatusProvider>();
			bindingsProviderMock ??= Mock.Of<IHostedServiceBusinessObjectBindingsProvider>();
			transactionAdapter ??= new NativeServiceTaskTransactionAdapter(attributeProviderMock, statusProviderMock, bindingsProviderMock, DateTimeMock.Object);
		}

		readonly Dictionary<T, IServiceTaskHandler> tasks = new Dictionary<T, IServiceTaskHandler>();
		readonly bool useSameConnectionAcrossThreads;
		IServiceTaskScheduleManager serviceTaskScheduleManager;
		IDisposable mainConnectionUsageHolder;
		ITransactionAdapter transactionAdapter;
		IClientHostedServiceAttributeProvider attributeProviderMock;
		IServiceTaskScheduleStatusProvider statusProviderMock;
		IHostedServiceBusinessObjectBindingsProvider bindingsProviderMock;

		void AssertHostedServiceRequirementMethods(Type attributeType, Type expectedReturnType)
		{
			var methodInfos = Array.FindAll(
				typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy),
				method => method.IsDefined(attributeType, false));

			if (methodInfos.Length > 0)
			{
				foreach (var mi in methodInfos)
				{
					Assert("Method " + mi.Name + " must be static.", mi.IsStatic);
					Assert("Method " + mi.Name + " must be public.", mi.IsPublic);
					Assert("Method " + mi.Name + " must return " + expectedReturnType.ToString() + " and have no parameters.", mi.ReturnType.Equals(expectedReturnType) && mi.GetParameters().Length == 0);
				}
			}
			else
			{
				Assert(true);
			}
		}

		void AssertMinimumPredicates(HostedServiceBusinessObjectBindingAttribute hostedServiceAttribute, string isActivePredicate, string statusPredicate, string directionPredicate, string applicationCodePredicate)
		{
			var queueName = hostedServiceAttribute.QueueName;
			var predicates = hostedServiceAttribute.Predicates;
			AssertEquals($"{queueName} missing {isActivePredicate} predicate", true, predicates.Contains(isActivePredicate));
			AssertEquals($"{queueName} missing {statusPredicate} predicate", true, predicates.Any(x => x.StartsWith(statusPredicate)));
			AssertEquals($"{queueName} missing {directionPredicate} predicate", true, predicates.Any(x => x.StartsWith(directionPredicate)));
			if (!ExcludedApplicationCodeQueues.Contains(queueName))
			{
				AssertEquals($"{queueName} missing {applicationCodePredicate} predicate", true, predicates.Any(x => x.StartsWith(applicationCodePredicate)));
			}
		}

		HostedServiceBusinessObjectBindingAttribute[] BindingAttributesForHostedServices
		{
			get
			{
				var hostedServiceAttributeCodes = GetHostedServiceAttributes().Select(x => x.Code.ToUpperInvariant()).ToHashSet();
				return typeof(T).Assembly.GetCustomAttributes(typeof(HostedServiceBusinessObjectBindingAttribute))
					.Cast<HostedServiceBusinessObjectBindingAttribute>()
					.Where(x => hostedServiceAttributeCodes.Contains(x.ServiceTaskCode.ToUpperInvariant()))
					.ToArray();
			}
		}

		bool HostedServicesHasBindingAttribute => BindingAttributesForHostedServices.Length > 0;

		HashSet<string> ExcludedApplicationCodeQueues { get; } = new()
		{
			"eAdaptor Outbound",
			"eHub Outbound Messages",
			"Universal Event Key Generator",
			"Universal Shipment Key Generator",
			"Universal Transaction Batch Key Generator",
			"Universal Transaction Key Generator",
			"XML Universal Event",
			"XML Universal Schedule",
			"XML Universal Shipment",
			"XML Universal Transaction",
			"XML Universal Transaction Batch",
			"xT Test Message Sending",
			"xT Message Sending",
		};

		HashSet<string> BranchDependentTasks { get; } = new(new[]
			{
				"ACS-Enterprise.Recruiter.ServiceTasks.Testing.GlbAccreditationCompletionServiceTaskTest",
				"CMX-Enterprise.Freight.Agency.ServiceTasks.Testing.MovementExporterServiceTaskTest",
				"CPS-ZClientEDI.Test.ServiceTasks.EDICertProcessing.Test.EDICertProcessingServiceTaskTest",
				"DMI-Enterprise.DocumentScanning.ServiceTasks.Test.DocManagerImportTaskTest",
				"EAM-Enterprise.eHubMessaging.Tests.eAdaptorOutboundServiceTaskTests",
				"EBS-Enterprise.Client.EDI.Billing.ServiceTasks.Testing.EdiBillingTestingEnvironmentSyncServiceTaskTest",
				"EMI-Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks.Test.EmailImporterServiceTaskTest",
				"LWK-Enterprise.ServiceManager.Tasks.LogWalker.Test.LogWalkerServiceTaskTest",
				"LWK-Enterprise.WorkflowManager.ServiceTasks.Testing.WorkflowEventTriggerProcessorServiceLevelTest",
				"LWM-Enterprise.ServiceManager.Tasks.LogWalker.Test.LogWalkerMasterServiceTaskTest",
				"LWP-Enterprise.ServiceManager.Tasks.LogWalker.Test.LogWalkerPurgeServiceTaskTest",
				"NMI-Enterprise.DataTransfer.Native.ServiceTasks.Testing.ServiceTaskTest",
				"POB-Enterprise.Client.EDI.Test.ProcessingOnBoardingServiceTaskTest",
				"RPN-Enterprise.HRM.ServiceTasks.Testing.ReviewProcessNodeCreationServiceTaskTest",
				"RPR-Enterprise.HRM.ServiceTasks.Testing.ReviewProcessNodeCopybackServiceTaskTest",
				"SYS-Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests.SystemXmlMessageServiceTaskTest",
				"TSS-Enterprise.AuditDataServices.Telematics.Test.TelematicsSubscriberServiceTaskTest",
				"UAR-Enterprise.Client.EDI.ServiceTask.EventLogs.Testing.UserAccountReportingServiceTaskTest",
				"UCI-Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing.UCMServiceTaskMasterTest",
				"UCK-Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing.UCMServiceTaskKeyGenTest",
				"UCP-Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing.UCPServiceTaskTest",
				"UCQ-Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing.UCMServiceTaskWorkerTest",
				"UCT-Enterprise.Freight.LocalCartage.Service.Testing.GPSUpdateCartageTaskTest",
				"UCU-Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing.UCUServiceTaskTest",
				"UMI-Enterprise.UniversalDataBuss.ServiceTasks.Testing.UMIServiceTaskGrEngineTest",
				"UMI-Enterprise.UniversalDataBuss.ServiceTasks.Testing.UMIServiceTaskTest",
				"UMK-Enterprise.UniversalDataBuss.ServiceTasks.Testing.UMIServiceTaskKeyGenTest",
				"UMQ-Enterprise.UniversalDataBuss.ServiceTasks.Testing.UMIServiceTaskWorkerTest",
				"USI-Enterprise.UniversalDataBuss.ServiceTasks.Testing.USIServiceTaskTest",
				"WEX-Enterprise.WorkflowManager.ServiceTasks.Testing.WorkflowExceptionGenerationServiceTaskTest",
				"WMF-Enterprise.WorkflowManager.ServiceTasks.Testing.WorkflowModifiedFieldChangeTriggerServiceTaskTest",
				"XES-Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests.StandardXMLEventServiceTaskTest",
				"XME-Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks.Test.DataExporterServiceTaskTest",
				"XMI-Enterprise.ServiceManager.Tasks.XMLAutomation.ServiceTasks.Test.DataImporterServiceTaskTest",
				"XMS-Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests.StandardXMLMessageServiceTaskTest",
				"ZC1-Enterprise.Client.CLE.OrdersDataImport.Testing.CLEOrderDataImportServiceTaskTest",
				"ZC2-Enterprise.Client.CLE.MattelARInvoiceExport.Testing.MattelARInvoiceExportServiceTaskTest",
				"ZD3-Enterprise.Client.DFD.ServiceTasks.Testing.ARTransactionsServiceTaskTest",
				"ZE1-Enterprise.Client.ELG.ServiceTasks.Testing.SagAccountsServiceTaskTest",
				"ZI1-Enterprise.Client.IFC.ConsolExport.Testing.IFCServiceTaskTest",
				"ZK1-Enterprise.Client.KNA.ServiceTasks.Testing.ImportXmlServiceTaskTest",
				"ZM1-Enterprise.Client.MFI.ServiceTasks.Testing.DocumentAllocationServiceTaskTest",
				"ZM2-Enterprise.Client.MFI.ServiceTasks.Testing.CoroTransExportServiceTaskTest",
				"ZN1-Enterprise.Client.NIP.ServiceTasks.Testing.NIPConsolAndShipmentServiceTaskTest",
				"ZS1-Enterprise.Client.STI.Navision.Testing.NavisionServiceTaskTest",
				"ZS1-Enterprise.Client.SWL.ServiceTasks.Testing.ShipnetServiceTaskTest",
				"ZS1-Enterprise.Client.SWT.ServiceTasks.Testing.SWTNotYetArrivedRepServiceTaskTest",
				"ZT1-Enterprise.Client.TEL.ServiceTasks.Testing.TELConsolShipServiceTaskTest",
				"ZT1-Enterprise.Client.TGE.ServiceTask.Testing.AAEDebtorTrensactionsServiceTaskTest",
				"ZT1-Enterprise.Client.TNT.ServiceTasks.Testing.NADImportServiceTaskTest",
				"ZT2-Enterprise.Client.TIP.ServiceTask.Testing.ProductExportServiceTaskTest",
				"ZT2-Enterprise.Client.TNT.ServiceTasks.Testing.OutTurnImportServiceTaskTest",
				"ZT3-Enterprise.Client.TNT.ServiceTasks.Testing.AirCargoResponseExportServiceTaskTest",
				"ZT4-Enterprise.Client.TNT.ServiceTasks.Testing.QuantumDataImportServiceTaskTest",
				"ZU1-Enterprise.Client.UPE.ServiceTask.Testing.BISIDownloadServiceTaskTest",
				"ZU2-Enterprise.Client.UPE.ServiceTask.Testing.BISIEntryPrintServiceTaskTest",
				"ZU3-Enterprise.Client.UPE.Business.ServiceTask.BISIUploadServiceTaskTest",
				"ZU4-Enterprise.Client.UPE.ServiceTask.GSSi.Testing.GSSiServiceTaskTest",
				"ZU5-Enterprise.Client.UPE.Business.ServiceTask.Testing.DocumentImageImportServiceTaskTest",
				"ZU6-Enterprise.Client.UPE.ServiceTask.Testing.MatchingActivitiesServiceTaskTest",
				"ZU7-Enterprise.Client.UPE.Business.ServiceTask.Testing.ResolutionQueueBatchProcessorTest",
				"ZU8-Enterprise.Client.UPE.ServiceTask.Testing.SMSNotificationsServiceTaskTest",
				"ZW1-Enterprise.Client.WFN.ServiceTasks.Testing.WFNShipmentAndConsolImportServiceTaskTest",
				"ZW2-Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.ExportDeclarationInvoiceServiceTaskTest",
				"ZY4-Enterprise.Client.YAS.ServiceTasks.ProofOfDeliveryInterface.Testing.PODDataImportServiceTaskTest",
			},
			StringComparer.InvariantCultureIgnoreCase);

		//🚩🚩DO NOT ADD TO THE WHITELIST BELOW 🚩 🚩 
		HashSet<string> ExcludedServiceTaskCodesThatFailTestNudgeMustNotHaveTableScanOrIndexScan { get; } = new()
		{
			"EPA-Enterprise.Accounting.ElectronicMessaging.Panama.Testing.ElectronicMessagingProcessingServiceTaskTest",
			"ESA-Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing.ElectronicMessagingProcessingServiceTaskForSaudiArabiaTest",
			"GEP-Enterprise.Accounting.ElectronicPayment.Testing.GlobalElectronicPaymentProcessingServiceTaskTest",
			"CGN-Enterprise.CommissionManagement.ServiceTasks.Testing.CommissionGenerator.CommissionGeneratorServiceTaskTest",
			"BGC-Enterprise.Customs.ServiceTasks.Testing.CalculateGuaranteeBalanceServiceTaskTest",
			"ZRP-Enterprise.Customs.ZA.ServiceTasks.Testing.ProcessReceiptServiceTest",
			"USS-Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging.UsageSubmissionServiceTaskTests",
			"RET-Enterprise.ErrorReporting.ServiceTasks.Test.ReportErrorsServiceTaskTest_BatchesLargeDataSet",
			"RET-Enterprise.ErrorReporting.ServiceTasks.Test.ReportErrorsServiceTaskTest_EmptyDataSet",
			"RET-Enterprise.ErrorReporting.ServiceTasks.Test.ReportErrorsServiceTaskTest_SmallDataSet",
			"UCT-Enterprise.Freight.LocalCartage.Service.Testing.GPSUpdateCartageTaskTest",
			"HPQ-Enterprise.Recruiter.ServiceTasks.Testing.HRJobApplicationParsingQueueServiceTaskTest",
			"ADS-Enterprise.Security.ActiveDirectory.Test.ActiveDirectorySynchronisationTaskServiceTaskTestCase",
			"TGP-Enterprise.Telematics.ServiceTasks.Test.TelematicsGpsLocationRoadTypeServiceTaskTest",
			"WPI-Enterprise.Warehouse.Transactions.Business.Testing.WhsQueuedInvoiceAutoRatingServiceTaskTest",
			"ZRP-Enterprise.Customs.ZA.ServiceTasks.Testing.ProcessReceiptServiceTest",
			"TGP-Enterprise.Telematics.ServiceTasks.Test.TelematicsGpsLocationRoadTypeServiceTaskTest",
			"CSP-Enterprise.Client.EDI.Billing.ServiceTasks.Testing.UsageReportServiceTaskTest",
			"WQR-Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing.WebRequestServiceTaskTest",
			"ADE-ZClientEDI.ServiceTasks.WTGADUserSynchronization.Test.SynchronizeEDIStaffToActiveDirectoryTaskTestCase",
			"ERS-Enterprise.Client.EDI.Billing.ServiceTasks.Testing.UsageReportServiceTaskTest",
			"WRQ-Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing.WebRequestServiceTaskTest",
			"ADE-ZClientEDI.ServiceTasks.WTGADUserSynchronization.Test.SynchronizeEDIStaffToActiveDirectoryTaskTestCase",
			"AAP-ZClientEDI.Test.ServiceTasks.ApplicationProcessing.Test.ApplicationProcessingServiceTaskTest",
		};
		//🚩🚩DO NOT ADD TO THE WHITELIST ABOVE 🚩 🚩

		protected ILogger LoggerMock { get; private set; }
		protected Mock<IDateTimeProvider> DateTimeMock { get; private set; }
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestNudgeMustNotHaveTableScanOrIndexScan()
		{
			// Arrange
			var resolver = new NudgingSchemaResolver(ObjectFactory.Get<IApplicationSchemaResolver>());
			var predicateFactory = new PredicateFactory(resolver);
			var thisClassAttribute = GetHostedServiceAttributeOrFail();
			var serviceTaskCode = thisClassAttribute.Code;

			if (string.IsNullOrEmpty(serviceTaskCode))
			{
				Assert("No service task code found.", true);
				return;
			}

			var taskKey = $"{serviceTaskCode}-{GetType().FullName}";
			var isInWhitelist = ExcludedServiceTaskCodesThatFailTestNudgeMustNotHaveTableScanOrIndexScan.Contains(taskKey);
			var nudgeAttributes = BindingAttributesForHostedServices
				.Where(attribute => string.Equals(attribute.ServiceTaskCode, serviceTaskCode, StringComparison.OrdinalIgnoreCase))
				.ToList();

			if (!nudgeAttributes.Any() && !isInWhitelist)
			{
				Assert("No binding attributes found.", true);
				return;
			}

			var hasHostedServiceQueueProviders = nudgeAttributes.Any(attribute =>
			{
				var hostedServiceQueueProviderType = GetHostedServiceQueueProviderTypeByServiceTaskCode(attribute.ServiceTaskCode);
				return hostedServiceQueueProviderType != null;
			});

			if (hasHostedServiceQueueProviders && !isInWhitelist)
			{
				Assert("HostedServiceQueueProviders found in the defined attributes.", true);
				return;
			}
			// Act
			var errors = new List<string>();
			var adminConnection = Db.NewAdminConnection();

			foreach (var attribute in nudgeAttributes)
			{
				var attributeErrors = CheckBindingAttribute(attribute, adminConnection, predicateFactory);
				errors.AddRange(attributeErrors);
			}

			// Assert
			if (isInWhitelist && errors.Count != 0)
			{
				Assert("Test passed due to whitelist inclusion.", true);
				return;
			}

			Assert($"Test failed due to the following errors:\n{string.Join("\n", errors)}. \nPlease take a look at this wiki for further information: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/13988/Service-task-queue-(backlog)-monitoring", !errors.Any());
			Assert("Service task has passed all checks but is still in the whitelist. Please remove it from the whitelist.", !(isInWhitelist && errors.Count == 0));
		}

		IReadOnlyCollection<string> CheckBindingAttribute(HostedServiceBusinessObjectBindingAttribute nudgeAttribute, DbConnection adminConnection, PredicateFactory predicateFactory)
		{
			var errors = new List<string>();

			// Arrange
			var tableName = nudgeAttribute.Table;
			var queueName = nudgeAttribute.QueueName;
			var sourceName = string.IsNullOrEmpty(queueName) ? tableName : queueName;

			if (string.IsNullOrEmpty(tableName))
			{
				errors.Add("Invalid table name in binding attribute.");
				return errors;
			}

			var predicates = predicateFactory.GeneratePredicates(nudgeAttribute.Predicates, tableName).ToArray();
			var predicateText = predicates.Length > 0 ? " WHERE (" + string.Join(") AND (", predicates.Select(x => x.ParameterizedSqlCondition)) + ")" : string.Empty;
			var predicateDescription = string.IsNullOrEmpty(predicateText) ? "empty predicates" : predicateText;

			adminConnection.ExecuteNonQuery($"UPDATE STATISTICS {tableName} WITH ROWCOUNT=10000000");

			// Act
			using (adminConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				using (var cmd = adminConnection.Command($"SELECT COUNT(*) FROM {tableName}{predicateText}"))
				{
					foreach (var predicate in predicates)
					{
						predicate.Parameters.ForEach(param => cmd.AddParameter(param.ParameterName, param.Type, param.Size, param.Value));
					}

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							totalRows++;
						}
					}
				}

				if (totalRows != 1)
				{
					errors.Add($"Expected total rows to be 1 but found {totalRows} for table {tableName}.");
				}

				var queryPlanErrors = AnalyzeQueryPlan(adminConnection, tableName, sourceName, predicateDescription);
				errors.AddRange(queryPlanErrors);
			}

			return errors;
		}

		IReadOnlyCollection<string> AnalyzeQueryPlan(DbConnection adminConnection, string tableName, string sourceName, string description)
		{
			var errors = new List<string>();

			// Act
			var matchingQueryPlan = adminConnection.ExecutedCommandsAndQueryPlans
				.SingleOrDefault(t => t.Item1.Contains(tableName));

			if (matchingQueryPlan == null)
			{
				errors.Add($"No matching query plan found for table '{tableName}'. This might indicate the query did not execute or was not tracked.");
				return errors;
			}

			var queryPlanAnalyzer = new QueryPlanalyzer(matchingQueryPlan.Item2.Single());
			var key = $"'{sourceName}' with '{description}'";

			var tableScans = queryPlanAnalyzer.TableScans.Select(x => x.TableName).ToList();
			if (tableScans.Count > 0)
			{
				errors.Add($"{key} has Table Scan(s) - {string.Join(",", tableScans)}");
			}

			var indexScans = queryPlanAnalyzer.IndexScans.Select(x => x.IndexName).ToList();
			if (indexScans.Count > 0)
			{
				errors.Add($"{key} has Index Scan(s) - {string.Join(",", indexScans)}");
			}

			return errors;
		}

		protected static string ConvertToScheduleRecurrence(RunsEvery runsEvery)
		{
			return runsEvery switch
			{
				RunsEvery.Second => ScheduleType.Seconds,
				RunsEvery.Minute => ScheduleType.Minutes,
				RunsEvery.Hour => ScheduleType.Hours,
				RunsEvery.Day => ScheduleType.Days,
				RunsEvery.Week => ScheduleType.Weeks,
				RunsEvery.Month => ScheduleType.Months,
				RunsEvery.Yearly => ScheduleType.Years,
				_ => ScheduleType.Minutes,
			};
		}

		protected enum RunsEvery
		{
			Second,
			Minute,
			Hour,
			Day,
			Week,
			Month,
			Yearly
		}

		static class ScheduleType
		{
			public const string Seconds = "S";
			public const string Minutes = "T";
			public const string Hours = "H";
			public const string Days = "D";
			public const string Weeks = "W";
			public const string Months = "M";
			public const string Years = "Y";
		}

		protected class TaskNudgeInformationForTest
		{
			public TaskNudgeInformationForTest(string table, string queueName, params string[] predicates)
			{
				Table = table;
				QueueName = queueName;
				Predicates = predicates;
			}

			public string Table { get; }
			public string QueueName { get; }
			public IEnumerable<string> Predicates { get; }
		}

		protected class ServiceTaskGovernorProxy
		{
			protected internal ServiceTaskGovernorProxy(ITransactionAdapter transactionAdapter, StmServiceTask task)
			{
				this.transactionAdapter = transactionAdapter;
				governor = transactionAdapter.GetServiceTaskGovernor(task.PK.ToGuid());
			}

			public void SetBranchPk(Guid pk)
			{
				governor.SetBranchPk(pk);
				transactionAdapter.Commit();
			}

			readonly ITransactionAdapter transactionAdapter;
			readonly IServiceTaskGovernor governor;
		}
	}
}

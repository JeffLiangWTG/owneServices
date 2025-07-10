using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class QueuedLogReferenceProviderTest : TestCaseWithFactory
	{
		public void TestCreateReferenceMap()
		{
			var provider = new QueuedLogReferenceProvider();

			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			triggerAction.PQ_TriggerType = "ACT";
			var queuedLogMock = new Mock<IQueuedLog>();
			var bizo = Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			var userContext = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			var referenceMap = provider.CreateReferenceMap(triggerAction, queuedLogMock.Object, bizo, userContext);

			AssertEquals("It should have correct TriggerAction", "ACT", referenceMap[JobQueueReferenceParameters.TriggerAction]);
			AssertEquals("It should have correct ParentPK", "d976a46b-8184-4433-995a-5fb3dac5c79d", referenceMap[JobQueueReferenceParameters.ParentPK]);
			AssertEquals("It should have correct ParentType", "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business", referenceMap[JobQueueReferenceParameters.ParentType]);
			AssertEquals("It should have 3 pairs", 3, referenceMap.Count);
		}

		public void TestGetParametersFromReferenceMap()
		{
			var referenceMap = new Dictionary<string, string>() {
				{ JobQueueReferenceParameters.TriggerAction, "ACT" },
				{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
				{ JobQueueReferenceParameters.ParentType, "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business" }
			};
			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);
			Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			Factory.Save();
			var provider = new QueuedLogReferenceProvider();

			var parameters = provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object);

			AssertEquals("It should have correct PK", "d976a46b-8184-4433-995a-5fb3dac5c79d", parameters.WorkflowProvider.PK.ToString());
			AssertEquals("It should have correct Type", typeof(DummyWithWorkflow), parameters.WorkflowProvider.GetType());
			AssertEquals("It should not have other parameter", ZGuid.Empty, parameters.CompanyPK);
		}

		public void TestGetParametersFromReferenceMap_WithInvalidParentPK()
		{
			TestCase(null);
			TestCase("Invalid PK");
			TestCase(string.Empty);

			void TestCase(string parentPKValue)
			{
				var referenceMap = new Dictionary<string, string>() {
					{ JobQueueReferenceParameters.TriggerAction, "ACT" },
					{ JobQueueReferenceParameters.ParentType, "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business" }
				};

				if (parentPKValue is not null)
				{
					referenceMap.Add(JobQueueReferenceParameters.ParentPK, parentPKValue);
				}

				var queuedLogMock = new Mock<IQueuedLog>();
				queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);
				var provider = new QueuedLogReferenceProvider();

				var exception = AssertExceptionThrown<InvalidOperationException>($"It should throw InvalidOperationException when ParentPK is {parentPKValue}", () => provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object));
				AssertContains("It should contain correct message", "parentPKString is invalid.", exception.Message);
			}
		}

		public void TestGetParametersFromReferenceMap_WithInvalidParentType()
		{
			TestCase(null);
			TestCase("Invalid Type");
			TestCase(string.Empty);

			void TestCase(string parentTypeValue)
			{
				var referenceMap = new Dictionary<string, string>() {
					{ JobQueueReferenceParameters.TriggerAction, "ACT" },
					{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
				};

				if (parentTypeValue is not null)
				{
					referenceMap.Add(JobQueueReferenceParameters.ParentType, parentTypeValue);
				}

				var queuedLogMock = new Mock<IQueuedLog>();
				queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);
				var provider = new QueuedLogReferenceProvider();

				var exception = AssertExceptionThrown<InvalidOperationException>($"It should throw InvalidOperationException when parentType is {parentTypeValue}", () => provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object));
				AssertContains("It should contain correct message", "typeName is invalid.", exception.Message);
			}
		}

		public void TestGetParametersFromReferenceMap_CheckLogAdded_WhenParentIsInvalid()
		{
			var logger = new LoggerForTesting();
			var referenceMap = new Dictionary<string, string>() {
				{ JobQueueReferenceParameters.TriggerAction, "ACT" },
				{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
				{ JobQueueReferenceParameters.ParentType, "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business" }
			};

			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);

			var provider = new QueuedLogReferenceProvider(logger);

			var parameters = provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object);
			AssertNull(parameters.WorkflowProvider);
		}

		public void TestGetParametersFromReferenceMap_WithInvalidWorkflowProvider()
		{
			var referenceMap = new Dictionary<string, string>() {
				{ JobQueueReferenceParameters.TriggerAction, "ACT" },
				{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
				{ JobQueueReferenceParameters.ParentType, "CargoWise.EntityFramework.Testing.DummyBusinessObject, CargoWise.EntityFramework" }
			};

			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);
			Factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			Factory.Save();
			var provider = new QueuedLogReferenceProvider();

			var exception = AssertExceptionThrown<InvalidOperationException>($"It should throw InvalidOperationException", () => provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object));
			AssertContains("It should contain correct message", "workflowProvider is invalid.", exception.Message);
		}

		public void TestProviderImplementationCount()
		{
			var interfaceType = typeof(IQueuedLogReferenceProvider);
			var decoratorType = typeof(QueuedLogReferenceProviderDecorator);
			var implementClasses = interfaceType.Assembly
										.GetTypes()
										.Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsSubclassOf(decoratorType) && t != decoratorType);

			AssertEquals($@"There should be only one provider implementation. Now we have:
{string.Join("\n", implementClasses.ToList())}
NOTE: We are using decorator pattern here, it is recommended that implement new decorators and combine decorators to add new paramter.
If you are sure you need to implement a new provider, please change this test.",
				1,
				implementClasses.Count());
		}
	}
}

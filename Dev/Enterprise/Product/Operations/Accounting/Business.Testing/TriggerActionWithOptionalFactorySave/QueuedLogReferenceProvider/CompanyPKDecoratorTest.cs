using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class CompanyPKDecoratorTest : TestCaseWithFactory
	{
		public void TestCreateReferenceParameters()
		{
			var provider = new CompanyPKDecorator(new QueuedLogReferenceProvider());

			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			triggerAction.PQ_TriggerType = "ACT";
			var trigger = Factory.Load<ProcessTask>(triggerAction.PQ_P9);
			trigger.P9_GC = Env.CurrentCompanyPK;

			var queuedLogMock = new Mock<IQueuedLog>();
			var bizo = Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			var userContext = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var referenceMap = provider.CreateReferenceMap(triggerAction, queuedLogMock.Object, bizo, userContext);
			AssertEquals("It should have correct CompanyPK", Env.CurrentCompanyPK.ToString(), referenceMap[JobQueueReferenceParameters.TriggerCompanyPK]);
			AssertEquals("It should have correct TriggerAction", "ACT", referenceMap[JobQueueReferenceParameters.TriggerAction]);
			AssertEquals("It should have correct ParentPK", "d976a46b-8184-4433-995a-5fb3dac5c79d", referenceMap[JobQueueReferenceParameters.ParentPK]);
			AssertEquals("It should have correct ParentType", "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business", referenceMap[JobQueueReferenceParameters.ParentType]);
			AssertEquals("It should have 4 pairs", 4, referenceMap.Count);
		}

		public void TestGetParametersFromReference()
		{
			Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			Factory.Save();

			TestCase(Env.CurrentCompanyPK.ToString(), Env.CurrentCompanyPK);
			TestCase(null, Guid.Empty);
			TestCase(string.Empty, Guid.Empty);
			TestCase(Guid.Empty.ToString(), Guid.Empty);

			void TestCase(string companyPk, Guid expectCompanyPk)
			{
				var referenceMap = new Dictionary<string, string>() {
					{ JobQueueReferenceParameters.TriggerAction, "ACT" },
					{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
					{ JobQueueReferenceParameters.ParentType, "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business" }
				};

				if (companyPk is not null)
				{
					referenceMap.Add(JobQueueReferenceParameters.TriggerCompanyPK, companyPk);
				}

				var queuedLogMock = new Mock<IQueuedLog>();
				queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);

				var provider = new CompanyPKDecorator(new QueuedLogReferenceProvider());
				var parameters = provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object);
				AssertEquals("It should have correct CompanyPK", expectCompanyPk, parameters.CompanyPK);
				AssertEquals("It should have correct ParentPK", "d976a46b-8184-4433-995a-5fb3dac5c79d", parameters.WorkflowProvider.PK.ToString());
				AssertEquals("It should have correct ParentType", typeof(DummyWithWorkflow), parameters.WorkflowProvider.GetType());
			}
		}

		public void TestGetParametersFromReference_WithInvalidCompanyPK()
		{
			var referenceMap = new Dictionary<string, string>() {
				{ JobQueueReferenceParameters.TriggerAction, "ACT" },
				{ JobQueueReferenceParameters.ParentPK, "d976a46b-8184-4433-995a-5fb3dac5c79d" },
				{ JobQueueReferenceParameters.ParentType, "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, Enterprise.MasterFiles.Business" },
				{ JobQueueReferenceParameters.TriggerCompanyPK, "Invalid PK" }
			};
			var queuedLogMock = new Mock<IQueuedLog>();
			queuedLogMock.SetupGet(x => x.Factory).Returns(Factory);
			Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			Factory.Save();

			var provider = new CompanyPKDecorator(new QueuedLogReferenceProvider());
			var exception = AssertExceptionThrown<InvalidOperationException>($"It should throw InvalidOperationException", () => provider.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object));
			AssertContains("It should contain correct message", "triggerCompanyPK is not a valid GUID.", exception.Message);
		}
	}
}

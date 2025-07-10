using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.Testing
{
	class QueuedLogReferenceProviderDecoratorTest : TestCaseWithFactory
	{
		public void TestCreateReferenceMap()
		{
			var decorator = new DummyQueuedLogReferenceProviderDecorator(new DummyQueuedLogReferenceProvider());

			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			triggerAction.PQ_TriggerType = "ACT";
			var queuedLogMock = new Mock<IQueuedLog>();
			var bizo = Factory.NewWithPrimaryKey<DummyWithWorkflow>(new Guid("d976a46b-8184-4433-995a-5fb3dac5c79d"));
			var userContext = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			decorator.CreateReferenceMap(triggerAction, queuedLogMock.Object, bizo, userContext);
			AssertEquals("DummyQueuedLogReferenceProvider.CreateReferenceMap should be called once", 1, (decorator.GetProvider_ForTestOnly() as DummyQueuedLogReferenceProvider).CreateReferenceMapHitCount);
		}

		public void TestGetParametersFromReferenceMap()
		{
			var decorator = new DummyQueuedLogReferenceProviderDecorator(new DummyQueuedLogReferenceProvider());

			var referenceMap = new Dictionary<string, string>();
			var queuedLogMock = new Mock<IQueuedLog>();

			decorator.GetParametersFromReferenceMap(referenceMap, queuedLogMock.Object);
			AssertEquals("DummyQueuedLogReferenceProvider.GetParametersFromReferenceMap should be called once", 1, (decorator.GetProvider_ForTestOnly() as DummyQueuedLogReferenceProvider).GetParametersFromReferenceMapHitCount);
		}

		public void TestAllDecoratorsShouldBeSealed()
		{
			var baseType = typeof(QueuedLogReferenceProviderDecorator);
			var subClasses = baseType.Assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));

			foreach (var type in subClasses)
			{
				Assert(@$"Decorator class should be sealed. {type.Name} is not sealed.
NOTE: We are using decorator pattern here, it is recommended that each decorator inherit from the QueuedLogReferenceProviderDecorator and be combined in factory.
If you are sure you need to use one decorator to inherit another decorator, please change this test.",
				type.IsSealed);
			}
		}

		public class DummyQueuedLogReferenceProviderDecorator : QueuedLogReferenceProviderDecorator
		{
			public DummyQueuedLogReferenceProviderDecorator(IQueuedLogReferenceProvider provider) : base(provider)
			{ }
		}

		public class DummyQueuedLogReferenceProvider : IQueuedLogReferenceProvider
		{
			public IDictionary<string, string> CreateReferenceMap(ProcessTaskNotification triggerAction, IQueuedLog queuedLog, BusinessObject parent, GlbStaff userContext)
			{
				CreateReferenceMapHitCount++;
				return new Dictionary<string, string> { };
			}

			public QueuedLogParameters GetParametersFromReferenceMap(IDictionary<string, string> referenceMap, IQueuedLog queuedLog)
			{
				GetParametersFromReferenceMapHitCount++;
				return new QueuedLogParameters();
			}

			public int CreateReferenceMapHitCount { get; private set; }
			public int GetParametersFromReferenceMapHitCount { get; private set; }
		}
	}
}

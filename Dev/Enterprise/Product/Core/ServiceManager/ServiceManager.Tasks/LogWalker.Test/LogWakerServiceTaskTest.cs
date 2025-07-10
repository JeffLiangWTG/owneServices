using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.LogWalker.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LogWalker.Test
{
	public abstract class LogSubscriberServiceLevelTest<T> : ServiceTaskTestCase<LogWalkerServiceTask> where T : LogSubscriber, new()
	{
		class LogWalkerServiceTaskForTest : LogWalkerServiceTask
		{
			public Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
			Func<BusinessObjectFactory> getFactoryMethod;

			protected override LogWalkerRunner Runner { get { return new LogWalkerRunnerForServiceLevelTest<T>() { GetFactoryMethod = getFactoryMethod }; } }
		}

		protected Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
		Func<BusinessObjectFactory> getFactoryMethod;

		LogWalkerServiceTask ServiceTask { get { return new LogWalkerServiceTaskForTest() { GetFactoryMethod = getFactoryMethod }; } }

		protected void InitialiseAndRunTaskSchedule()
		{
			InitialiseAndRunTaskSchedule(ServiceTask);
		}
	}

	[TestedType(typeof(LogWalkerServiceTask))]
	class LogWalkerServiceTaskTest : ServiceTaskTestCase<LogWalkerServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(StmJobQueueSchema.Constants.TableName, null, StmJobQueueSchema.Constants.SJ_Status + "=QUE"),
				};
			}
		}
	}
}

using System;
using CargoWise.EntityFramework;

namespace Enterprise.LogWalker.Testing
{
	public sealed class LogWalkerRunnerForServiceLevelTest<T> : LogWalkerRunner where T : LogSubscriber, new()
	{
		public LogWalkerRunnerForServiceLevelTest()
			: base(null)
		{
		}

		public Func<BusinessObjectFactory> GetFactoryMethod { set { getFactoryMethod = value; } }
		Func<BusinessObjectFactory> getFactoryMethod;

		internal override OperationsManager Manager
		{
			get { return new OperationsManagerForServiceLevelTest(new LogSubscriber[] { new T() }) { GetFactoryMethod = getFactoryMethod }; }
		}
	}
}

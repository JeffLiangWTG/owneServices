using System;
using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ObjectFactoryCleanupListener : BaseTestListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.DisposeSingletons();
			base.StartTest(test, startTime);
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);
			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.DisposeSingletons();
		}
	}
}

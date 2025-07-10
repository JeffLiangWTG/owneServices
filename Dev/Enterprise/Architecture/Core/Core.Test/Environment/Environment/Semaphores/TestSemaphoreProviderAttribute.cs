using System;
using Enterprise.Semaphores.Common;
using NUnit.Framework;

namespace Enterprise.Core.Environment.Semaphores.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class TestSemaphoreProviderAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			TestProvider = new SemaphoreProviderWithCurrentUserForTesting();
		}

		public override void TearDown(TestCase testCase)
		{
			var disposable = TestProvider as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}

			TestProvider = null;
		}

		public static ISemaphoreProvider TestProvider { get; set; }
	}
}

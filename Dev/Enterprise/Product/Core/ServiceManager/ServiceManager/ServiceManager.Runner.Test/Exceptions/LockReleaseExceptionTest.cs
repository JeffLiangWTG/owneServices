using System;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class LockReleaseExceptionTest
	{
		[Test]
		public void TestIsAbstract()
		{
			var result = typeof(LockReleaseException).IsAbstract;
			Assert.That(result, Is.True);
		}

		[Test]
		public void TestIsNotRunnerInternalException()
		{
			var exception = new Mock<LockReleaseException>(string.Empty) { CallBase = true };
			Assert.That(() =>
			{
				try
				{
					throw exception.Object;
				}
				catch (RunnerInternalException)
				{
					// Hide
				}
			}, Throws.InstanceOf<Exception>());
		}
	}
}

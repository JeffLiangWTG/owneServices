using System;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class ServiceTaskLockReleaseExceptionTest
	{
		[Test]
		public void TestIsRunnerInternalException()
		{
			var exception = new ServiceTaskLockReleaseException();
			Assert.That(() => throw exception, Throws.InstanceOf<LockReleaseException>());
		}

		[Test]
		public void TestMessageDefault()
		{
			var result = new ServiceTaskLockReleaseException().Message;
			Assert.That(result, Is.EqualTo("Lock for single instance Service task could not be released."));
		}

		[TestCase("AAA")]
		[TestCase("BBB")]
		public void TestMessage(string taskCode)
		{
			var result = new ServiceTaskLockReleaseException(new Exception(), taskCode).Message;
			Assert.That(result, Is.EqualTo($"Lock for single instance Service task [{taskCode}] could not be released."));
		}

		[Test]
		public void TestInnerException()
		{
			Assert.Multiple(() =>
			{
				Test(new Exception());
				Test(new InvalidOperationException());
				Test(new AccessViolationException());
			});

			void Test(Exception exception)
			{
				var result = new ServiceTaskLockReleaseException(exception, string.Empty).InnerException;
				Assert.That(result, Is.EqualTo(exception));
			}
		}
	}
}

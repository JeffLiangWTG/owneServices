using System;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class MutualExclusiveLockReleaseExceptionTest
	{
		[Test]
		public void TestIsRunnerInternalException()
		{
			var exception = new MutualExclusiveLockReleaseException();
			Assert.That(() => throw exception, Throws.InstanceOf<LockReleaseException>());
		}

		[Test]
		public void TestMessageDefault()
		{
			var result = new MutualExclusiveLockReleaseException().Message;
			Assert.That(result, Is.EqualTo("Lock for mutual exclusive group for Service task could not be released."));
		}

		[TestCase("AAA", "XXX")]
		[TestCase("BBB", "YYY")]
		public void TestMessage(string taskCode, string groupCode)
		{
			var result = new MutualExclusiveLockReleaseException(new Exception(), taskCode, groupCode).Message;
			Assert.That(result, Is.EqualTo($"Lock for mutual exclusive group [{groupCode}] for Service task [{taskCode}] could not be released."));
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
				var result = new MutualExclusiveLockReleaseException(exception, string.Empty, string.Empty).InnerException;
				Assert.That(result, Is.EqualTo(exception));
			}
		}
	}
}

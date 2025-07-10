using System;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class CouldNotReadNextRunTimeExceptionTest
	{
		[Test]
		public void TestIsRunnerInternalException()
		{
			var exception = new CouldNotReadNextRunTimeException();
			Assert.That(() => throw exception, Throws.InstanceOf<RunnerInternalException>());
		}

		[Test]
		public void TestMessageDefault()
		{
			var result = new CouldNotReadNextRunTimeException().Message;
			Assert.That(result, Is.EqualTo("Could not read next run time."));
		}

		[Test]
		public void TestMessage()
		{
			var result = new CouldNotReadNextRunTimeException(new Exception()).Message;
			Assert.That(result, Is.EqualTo("Could not read next run time."));
		}

		[Test]
		public void TestInnerException()
		{
			var exception = new Exception();
			var result = new CouldNotReadNextRunTimeException(exception).InnerException;
			Assert.That(result, Is.EqualTo(exception));
		}
	}
}

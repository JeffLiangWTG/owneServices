using System;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class InitialisationRequestExceptionTest
	{
		[Test]
		public void TestMessageDefault()
		{
			var result = new InitialisationRequestException().Message;
			Assert.That(result, Is.EqualTo("Reinitialisation of the task is requested."));
		}

		[Test]
		public void TestInitialisationException()
		{
			var exception = new Exception("test");
			var result = new InitialisationRequestException(exception);
			Assert.That(result.Message, Is.EqualTo("Reinitialisation of the task is requested."));
			Assert.That(result.InnerException, Is.EqualTo(exception));
		}
	}
}

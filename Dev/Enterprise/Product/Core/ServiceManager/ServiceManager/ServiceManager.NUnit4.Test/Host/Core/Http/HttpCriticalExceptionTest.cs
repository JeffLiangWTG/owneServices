using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class HttpCriticalExceptionTest
	{
		[Test]
		public void TestMessageDefault()
		{
			var result = new HttpCriticalException().Message;
			Assert.That(result, Is.EqualTo("A critical http exception occurred."));
		}

		[Test]
		public void TestExceptionIsCritical()
		{
			var exception = new HttpCriticalException();
			Assert.That(exception.IsCriticalException, Is.EqualTo(true));
		}
	}
}

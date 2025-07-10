using System;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http.RequestHandlers
{
	[TestsSubclassesOf(typeof(RequestHandler))]
	abstract class RequestHandlerBaseTest : TestCase
	{
		protected abstract Uri GetExpectedUri();

		[ExpectNoExceptions]
		public void TestUri()
		{
			var result = handler.Uri;
			NUnit.Framework.Assert.That(result, Is.EqualTo(GetExpectedUri()));
		}

		[ExpectNoExceptions]
		public void TestRequestHandlerIsInitialized()
		{
			NUnit.Framework.Assert.That(handler, Is.Not.EqualTo(default(RequestHandler)), "$Instance of {TestedTypeHelper.GetTestedType(GetType())} must be initialized in {nameof(SetUp)}. - should not be [null]");
		}

		protected RequestHandler handler;
	}
}

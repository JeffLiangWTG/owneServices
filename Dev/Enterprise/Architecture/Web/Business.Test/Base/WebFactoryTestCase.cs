#if NETFRAMEWORK
using System.IO;
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class WebFactoryTestCase : TestCaseWithFactory
	{
		public void TestGetOrCreatePerRequestFactory_Context()
		{
#if NETFRAMEWORK
			HttpContext testContext1 = new HttpContext(new HttpRequest(string.Empty, "http://localhost", string.Empty), new HttpResponse(new StringWriter()));
			HttpContext testContext2 = new HttpContext(new HttpRequest(string.Empty, "http://localhost", string.Empty), new HttpResponse(new StringWriter()));
			HttpContext.Current = testContext1;

			var factory = WebFactory.GetOrCreatePerRequestFactory();
			var factory2 = WebFactory.GetOrCreatePerRequestFactory();

			HttpContext.Current = testContext2;
#elif NET
			var testContext1 = new DefaultHttpContext();
			testContext1.Request.Scheme = "http";
			testContext1.Request.Host = new HostString("localhost");
			var testContext2 = new DefaultHttpContext();
			testContext2.Request.Scheme = "http";
			testContext2.Request.Host = new HostString("localhost");
			WebEnv.HttpContextAccessor = new HttpContextAccessor() { HttpContext = testContext1 };

			var factory = WebFactory.GetOrCreatePerRequestFactory();
			var factory2 = WebFactory.GetOrCreatePerRequestFactory();

			WebEnv.HttpContextAccessor = new HttpContextAccessor() { HttpContext = testContext2 };
#endif
			var factory3 = WebFactory.GetOrCreatePerRequestFactory(Factory);
			var factory4 = WebFactory.GetOrCreatePerRequestFactory(factory2);

			AssertNotNull(factory);
			Assert(object.ReferenceEquals(factory, testContext1.Items[WebFactory.FactoryKey]));
			Assert(object.ReferenceEquals(factory, factory2));

			Assert("first call returns the given factory", object.ReferenceEquals(Factory, factory3));
			Assert("second call returns the per-request factory over the given factory", object.ReferenceEquals(factory3, factory4));
		}

		public void TestGetOrCreatePerRequestFactory_NoContext()
		{
			var factory = WebFactory.GetOrCreatePerRequestFactory();
			var factory2 = WebFactory.GetOrCreatePerRequestFactory();

			var factory3 = WebFactory.GetOrCreatePerRequestFactory(Factory);
			var factory4 = WebFactory.GetOrCreatePerRequestFactory(factory2);

			AssertNotNull(factory);
			Assert(!object.ReferenceEquals(factory, factory2));

			Assert("first call returns the given factory", object.ReferenceEquals(Factory, factory3));
			Assert("second call returns the given factory", object.ReferenceEquals(factory2, factory4));
		}

		protected override void TearDown()
		{
#if NETFRAMEWORK
			HttpContext.Current = null;
#elif NET
			WebEnv.HttpContextAccessor = null;
#endif
			base.TearDown();
		}
	}
}

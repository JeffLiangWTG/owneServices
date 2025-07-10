using CargoWise.EntityFramework.Testing;

#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	sealed class HttpRequestBaseExtensionsTest : TestCaseWithFactory
	{
		public void TestDetermineConnectionType_Null()
		{
			TestDetermineConnectionTypeCore(null, ConnectionType.Undefined);
		}

		public void TestDetermineConnectionType_HttpConnection()
		{
			var httpContextBase = MockHttpContext.PrepareMockHttpContextWrapper(isSecureConnection: false, isRedirectedFromLoadBalancer: false);
			TestDetermineConnectionTypeCore(httpContextBase.Request, ConnectionType.HttpConnection);
		}

		public void TestDetermineConnectionType_HttpsConnection()
		{
			var httpContextBase = MockHttpContext.PrepareMockHttpContextWrapper(isSecureConnection: true, isRedirectedFromLoadBalancer: false);
			TestDetermineConnectionTypeCore(httpContextBase.Request, ConnectionType.HttpsConnection);
		}

		public void TestDetermineConnectionType_HttpsConnection_RedirectedByLoadBalancer()
		{
			var httpContextBase = MockHttpContext.PrepareMockHttpContextWrapper(isSecureConnection: false, isRedirectedFromLoadBalancer: true);
			TestDetermineConnectionTypeCore(httpContextBase.Request, ConnectionType.HttpsConnection);
		}

#if NETFRAMEWORK
		void TestDetermineConnectionTypeCore(HttpRequestBase request, ConnectionType expectedConnectionType )
		{
			AssertEquals(expectedConnectionType, request.DetermineConnectionType());
		}
#else
		void TestDetermineConnectionTypeCore(HttpRequest request, ConnectionType expectedConnectionType )
		{
			AssertEquals(expectedConnectionType, request.DetermineConnectionType());
		}
#endif
	}
}

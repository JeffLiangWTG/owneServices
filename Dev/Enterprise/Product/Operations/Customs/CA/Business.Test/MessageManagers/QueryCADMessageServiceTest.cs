using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(QueryCADMessageService))]
	sealed class QueryCADMessageServiceTest : TestCaseWithFactory
	{
		public void TestQueryCADMessage()
		{
			using (CACustomsDataRegistry.Instance.CARMAPIKey.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "Test"))
			using (CACustomsDataRegistry.Instance.CARMEndPoint.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://Test.com"))
			{
				var httpClient = new HttpClient(GetMockHttpMsgHandler(HttpStatusCode.OK, "Test Response Content"));
				var result = QueryCADMessageService.Instance.Value.QueryCADMessage("00581155", httpClient);
				AssertEquals("QueryUrl: http://Test.com?TransactionNumber=00581155\nx-api-key: Test", result.QueryContent);
				AssertEquals("Test Response Content", result.ResponseContent);

				httpClient = new HttpClient(GetMockHttpMsgHandler(HttpStatusCode.NotFound, "Test Response Content"));
				result = QueryCADMessageService.Instance.Value.QueryCADMessage("00581155", httpClient);
				AssertEquals("QueryUrl: http://Test.com?TransactionNumber=00581155\nx-api-key: Test", result.QueryContent);
				AssertEquals("Test Response Content", result.ResponseContent);
			}
		}

		HttpMessageHandler GetMockHttpMsgHandler(HttpStatusCode responseStatus, string responseContent)
		{
			var mockHttpMsgHandler = new Mock<HttpMessageHandler>();
			mockHttpMsgHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage { StatusCode = responseStatus, Content = new StringContent(responseContent) });

			return mockHttpMsgHandler.Object;
		}
	}
}

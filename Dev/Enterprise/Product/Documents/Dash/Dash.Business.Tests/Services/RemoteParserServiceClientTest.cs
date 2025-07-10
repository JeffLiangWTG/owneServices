using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Dash.Business.Services;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Moq;
using Moq.Protected;
using WTG.Foundation.Http;
namespace Enterprise.Dash.Business.Tests.Services
{
	public class RemoteParserServiceClientTest : TestCaseWithDocumentFactory
	{
		public void TestParse()
		{
			SetUp();
			DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EoQNIn4lnxcKPmSH2RPACA==");

			var eDoc = GetNewStorageDocsBaseWithParent(1);
			var dashDocument = GetDashDocument(eDoc.PK.ToGuid());
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);
			MasterFactory.Save();

			HttpRequestHeaders capturedHeaders = null;
			Uri capturedRequestUri = null;

			var httpHandlerMock = new Mock<HttpMessageHandler>();
			httpHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Callback((HttpRequestMessage request, CancellationToken _) =>
				{
					capturedHeaders = request.Headers;
					capturedRequestUri = request.RequestUri;
				})
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("OK")
				});

			var httpClient = new HttpClient(httpHandlerMock.Object);

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();
			httpClientFactoryMock
				.Setup(factory => factory.Create())
				.Returns(httpClient);

			var tokenManagerMock = new Mock<IDocumentParsingTokenManager>();

			tokenManagerMock
				.Setup(t => t.GetSystemToSystemTrustToken())
				.Returns("5ddbb2ac-d026-459e-9474-f5ba257553e7");

			tokenManagerMock
				.Setup(t => t.TryDecryptEDocsAuthToken("ZmFrZS1lbmNyeXB0ZWQtdG9rZW4", out It.Ref<EDocsAuthTokenDetails>.IsAny))
				.Returns((string token, out EDocsAuthTokenDetails details) =>
				{
					details = new EDocsAuthTokenDetails
					{
						EDIMessagePK = Guid.Parse("5ddbb2ac-d026-459e-9474-f5ba257553e7"),
						EDocLastEditTime = DateTime.UtcNow.AddMinutes(30)
					};
					return true;
				});

			var parser = new RemoteParserServiceClient(tokenManagerMock.Object, httpClientFactoryMock.Object);

			// Act
			var response = parser.Parse(eDoc).GetAwaiter().GetResult();
			AssertNotNull("Request URI should not be null", capturedRequestUri);
			var queryParams = ParseQueryString(capturedRequestUri.Query);

			// Assert
			AssertNotNull("Headers should not be null", capturedHeaders);
			AssertEquals("Bearer", capturedHeaders.Authorization.Scheme);
			AssertEquals("5ddbb2ac-d026-459e-9474-f5ba257553e7", capturedHeaders.Authorization.Parameter);
			AssertNotNull("Response content should not be null", response.Content);

			AssertEquals(dashDocument.PK.ToString(), queryParams["DocPK"]);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		StorageDocsBase GetNewStorageDocsBaseWithParent(int databaseNumber)
		{
			var factory = MasterFactory.GetFactory(databaseNumber);
			var eDoc = factory.NewWithParent(typeof(StorageFile));
			eDoc.ParentMain.SM_DB = databaseNumber;
			eDoc.ParentMain.SM_ParentFK = ZGuid.NewZGuid();
			eDoc.ParentMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			eDoc.SC_DocType = "CIV";
			eDoc.SC_DataType = "PDF";
			eDoc.SC_FileName = "test";

			return eDoc;
		}

		DashDocument GetDashDocument(Guid docId)
		{
			var dashDocument = MasterFactory.NewWithValidTestData<DashDocument>();
			dashDocument.DDD_DocID = docId;

			return dashDocument;
		}

		protected override void SetUp()
		{
			DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocManagerRegistry.Instance.DocumentParserUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://shipamax.com");
		}

		Dictionary<string, string> ParseQueryString(string query)
		{
			Dictionary<string, string> queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (string.IsNullOrEmpty(query))
			{
				return queryParams;
			}

			if (query.StartsWith("?"))
			{
				query = query.Substring(1);
			}

			string[] pairs = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string pair in pairs)
			{
				string[] kv = pair.Split(new[] { '=' }, 2, StringSplitOptions.None);
				string key = WebUtility.UrlDecode(kv[0]);
				string value = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : string.Empty;

				if (!string.IsNullOrEmpty(key))
				{
					queryParams[key] = value;
				}
			}

			return queryParams;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using WTG.Foundation.Http;

namespace Enterprise.Dash.Business.Services
{
	public class RemoteParserServiceClient : IRemoteParserServiceClient
	{
		#region Constructors

		public RemoteParserServiceClient(IDocumentParsingTokenManager documentParsingTokenManager, IHttpClientFactory httpClientFactory)
		{
			IDocumentParsingTokenManager = documentParsingTokenManager;
			HttpClientFactory = httpClientFactory;
		}

		#endregion

		readonly IDocumentParsingTokenManager IDocumentParsingTokenManager;

		readonly IHttpClientFactory HttpClientFactory;

		public async Task<HttpResponseMessage> Parse(StorageDocsBase doc)
		{
			var url = DocManagerRegistry.Instance.DocumentParserUrl.Value.TrimEnd().TrimEnd('/') + "/parse";

			var dashDocument = GetDashDocument(doc);
			var bodyContent = GenerateRequestBody(doc);
			var queryParams = GenerateQueryParams(dashDocument);

			if (queryParams != null && queryParams.Count != 0)
			{
				var queryString = string.Join("&", queryParams
				.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
				url += "?" + queryString;
			}

			var client = HttpClientFactory.Create();

			AddRequiredHeaders(client);

			var response = await client.PostAsync(url, bodyContent).ConfigureAwait(false);
			return response;
		}

		DashDocument GetDashDocument(StorageDocsBase doc)
		{
			var dashDocumentQuery = new ZQuery(DashDocumentSchema.DDD_DocID, doc.PK);
			return new BusinessObjectFactory().LoadTop1<DashDocument>(dashDocumentQuery);
		}

		protected static HttpContent GenerateRequestBody(StorageDocsBase linkedEDoc)
		{
			var fileContent = new StreamContent(linkedEDoc.GetSC_ImageDataReader());
			var fullName = linkedEDoc.GetFileNameOnlyWithExtension();
			var mimeType = MimeTypes.GetMimeType(fullName);
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

			var retriever = new EnterpriseInformationRetriever();
			var utilityData = linkedEDoc.EDocsParsingSupport?.UtilityData ?? string.Empty;
			var multipartFormDataContent = new MultipartFormDataContent
			{
				{ new StringContent(linkedEDoc.SC_DocType, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.DocType },
				{ new StringContent(linkedEDoc.DocType?.RT_ParseType ?? string.Empty, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.ParseType },
				{ new StringContent(linkedEDoc.SC_Date.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.DocTimestamp },
				{ new StringContent(GlowRegistry.Instance.GlowServiceUri, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.ResponseEndpointRoot },
				{ new StringContent(retriever.VersionNumber, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.Cw1Version },
				{ new StringContent(retriever.LicenceCode.Replace(" ", string.Empty), Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.CustomerId },
				{ new StringContent(mimeType, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.FileContentType },
				{ new StringContent(utilityData, Encoding.UTF8, MediaTypeNames.Text.Plain), ParserServiceConstants.MultiPartContentName.UtilityData },
				{ fileContent, (NoResString)"file", fullName },
			};
			return multipartFormDataContent;
		}

		Dictionary<string, string> GenerateQueryParams(DashDocument dashDocument)
		{
			var queryParams = new Dictionary<string, string>
			{
				{ ParserServiceConstants.QueryParamNames.DocPk, dashDocument?.PK.ToString() },
			};

			return queryParams;
		}

		void AddRequiredHeaders(HttpClient client)
		{
			var tokenManager = IDocumentParsingTokenManager;

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((NoResString)"Bearer", tokenManager.GetSystemToSystemTrustToken());
		}
	}
}

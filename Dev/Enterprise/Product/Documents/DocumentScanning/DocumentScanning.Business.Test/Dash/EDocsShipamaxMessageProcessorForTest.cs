using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	public sealed class EDocsShipamaxMessageProcessorForTest : EDocsShipamaxMessageProcessor
	{
		public EDocsShipamaxMessageProcessorForTest()
		{
		}

		public EDocsShipamaxMessageProcessorForTest(ILogger serviceLogger)
		: base(serviceLogger)
		{
		}

		internal HttpMessageHandler MessageHandler { get; set; }

		internal ShipamaxIntegrationTokenManager TokenManager { get; set; }

		internal Dictionary<string, string> QueryParams { get; set; }

		internal List<ZGuid> ExcludedPKsExposed => excludedPKs;

		protected override HttpClient GetHttpClient()
		{
			return MessageHandler == null ? base.GetHttpClient() : new HttpClient(MessageHandler);
		}

		protected override ShipamaxIntegrationTokenManager GetTokenManager()
		{
			return TokenManager ?? base.GetTokenManager();
		}

		protected override Dictionary<string, string> GenerateQueryParams(StorageDocsBase linkedEDoc)
		{
			return QueryParams ?? base.GenerateQueryParams(linkedEDoc);
		}

		protected override Dictionary<string, string> GenerateQueryParamsV2(StorageDocsBase linkedEDoc)
		{
			return QueryParams ?? base.GenerateQueryParamsV2(linkedEDoc);
		}

		protected override int ProcessShipamaxMessageGroup(EDocsShipamaxMessage[] messageGroup, CancellationToken token)
		{
			GroupBranches.Add(GlbBranch.CurrentBranch.PK);
			return base.ProcessShipamaxMessageGroup(messageGroup, token);
		}

		internal List<ZGuid> GroupBranches = new ();

		internal ZQuery QueryLoadingMessagesExposed => QueryForLoadingMessages;

		internal HttpContent GenerateRequestBodyContentExposed(StorageDocsBase linkedEDoc) => GenerateRequestBodyContent(linkedEDoc);

		internal HttpContent GenerateRequestBodyContentExposedV2(StorageDocsBase linkedEDoc) => GenerateRequestBodyContentV2(linkedEDoc);

		internal Dictionary<string, string> GenerateQueryParamsExposed(StorageDocsBase linkedEDoc) => GenerateQueryParams(linkedEDoc);

		internal Dictionary<string, string> GenerateQueryParamsExposedV2(StorageDocsBase linkedEDoc) => GenerateQueryParamsV2(linkedEDoc);

		internal bool HandleRequestResponseExposed(HttpStatusCode statusCode, EDocsShipamaxMessage shipamaxMessage) => HandleRequestResponse(statusCode, shipamaxMessage);

		internal bool CheckPreConditionsBeforeProcessingMessagesExposed() => CheckPreConditionsBeforeProcessingMessages();
	}
}

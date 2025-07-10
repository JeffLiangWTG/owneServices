using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.DocumentScanning.Business
{
	public class EDocsShipamaxMessageProcessor
	{
		#region Constructors

		public EDocsShipamaxMessageProcessor()
			: this(null)
		{
		}

		public EDocsShipamaxMessageProcessor(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		#endregion

		const int BatchSize = 100;

		protected readonly List<ZGuid> excludedPKs = new();

		Uri shipamaxParseRequestUrl;

		public ILogger ServiceLogger { get; }

		public virtual void ProcessMessages(CancellationToken token)
		{
			if (!CheckPreConditionsBeforeProcessingMessages())
			{
				return;
			}

			excludedPKs.Clear();
			var totalMessagesSent = 0;
			var servicePath = DocManagerRegistry.Instance.DocumentParserUrl.Value.TrimEnd().TrimEnd('/') + "/parse";

			if (!Uri.TryCreate(servicePath, UriKind.Absolute, out shipamaxParseRequestUrl))
			{
				ServiceLogger.Error(FormattableString.Invariant($"Document Ingestion Processing url is invalid. Please enter correct value in '{DocManagerRegistry.Instance.DocumentParserUrl.GetLocation()}'."));
				return;
			}

			ServiceLogger.Information((NoResString)"Task started successfully.");
			var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory { RefreshEnabled = false });

			while (true)
			{
				token.ThrowIfCancellationRequested();

				var batch = masterFactory.Load<EDocsShipamaxMessage>(QueryForLoadingMessages);
				if (batch.Length > 0)
				{
					ServiceLogger.Information(FormattableString.Invariant($"Loaded and started processing {batch.Length} document(s)."));

					var messagesSentInCurrentBatch = 0;
					var shipamaxMessageGroups = batch.GroupBy(x => x.EM_GB, (pk, g) => new { BranchPK = pk, Messages = g.ToArray() });
					foreach (var group in shipamaxMessageGroups)
					{
						token.ThrowIfCancellationRequested();

						using (DisposableEnvironment.ForBranch(group.BranchPK.ToGuid()))
						{
							messagesSentInCurrentBatch += ProcessShipamaxMessageGroup(group.Messages, token);
						}
					}

					totalMessagesSent += messagesSentInCurrentBatch;
					ServiceLogger.Information(FormattableString.Invariant($"{messagesSentInCurrentBatch} messages have been sent for parsing."));
					ZExceptionReporting.ProcessWithConcurrencyHandling(() => masterFactory.Save(), null);
				}
				else
				{
					ServiceLogger.Information(FormattableString.Invariant($"No documents are available to be parsed."));
					break;
				}
			}

			ServiceLogger.Information(FormattableString.Invariant($"Task was completed with {totalMessagesSent} messages sent."));
		}

		protected virtual int ProcessShipamaxMessageGroup(EDocsShipamaxMessage[] messageGroup, CancellationToken token)
		{
			var messagesSent = 0;
			foreach (var shipamaxMessage in messageGroup)
			{
				token.ThrowIfCancellationRequested();

				messagesSent += SendShipamaxParseRequest(shipamaxMessage, token) ? 1 : 0;
			}
			return messagesSent;
		}

		bool SendShipamaxParseRequest(EDocsShipamaxMessage shipamaxMessage, CancellationToken token)
		{
			var linkedEDoc = shipamaxMessage.LinkedEDoc;
			if (linkedEDoc == null)
			{
				var failureReason = FormattableString.Invariant($"Failed to load linked StorageDocs object '{shipamaxMessage.EM_LinkUniqueID}'.");
				LogWarningAndMarkMessageInactive(failureReason, shipamaxMessage);
				return false;
			}

			try
			{
				using var client = GetHttpClient();
				using var requestBodyContent = GenerateRequestBodyContentV2(linkedEDoc);

				if (DocManagerRegistry.Instance.EnableDSPServiceTaskCollectDebugData.Value)  // this is temporary code to assit FR, will be removed later
				{
					var dataContents = requestBodyContent as MultipartFormDataContent;
					var loggerTextSB = new StringBuilder();
					foreach (var dataContent in dataContents)
					{
						var name = dataContent.Headers.ContentDisposition.Name;
						var value = dataContent.ReadAsStringAsync().Result;
						if (name != "file") // ignore the file part which is big and useless for FR.
						{
							loggerTextSB.AppendLine(name + ": " + value);
						}
					}
					ServiceLogger.Information(loggerTextSB.ToString());
				}

				var requestUriWithQueryParams = new UriBuilder(shipamaxParseRequestUrl)
				{
					Query = string.Join("&", GenerateQueryParamsV2(linkedEDoc).Select(p => $"{p.Key}={p.Value}"))
				}.Uri;

				AddRequiredHeaders(client, shipamaxMessage);

				var response = client.PostAsync(requestUriWithQueryParams, requestBodyContent, token).ConfigureAwait(false).GetAwaiter().GetResult();
				return HandleRequestResponse(response.StatusCode, shipamaxMessage);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex.IsExceptionPresentIncludingInner<WebException>() ||
					ex is AuthenticationException ||
					ex is UriFormatException ||
					ex is InvalidOperationException && ex.Message.StartsWith((NoResString)"Failed to get access token"))
				{
					throw new HostedServiceException(ex.Message, ex);
				}

				var errorMessage = FormattableString.Invariant($"An exception occurred while processing document {linkedEDoc.SC_FileName}, PK: '{linkedEDoc.PK}'.");
				LogErrorAndMarkMessagePKExcluded(ex.Message, shipamaxMessage);
				ErrorReporter.ReportOnce("EDocsShipamaxMessageProcessor_RunningError", errorMessage, ex);
				return false;
			}
		}

		protected bool CheckPreConditionsBeforeProcessingMessages()
		{
			if (!EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				ServiceLogger.Error((NoResString)"Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");

				return false;
			}

			if (string.IsNullOrEmpty(DocManagerRegistry.Instance.DocumentParserUrl.Value))
			{
				ServiceLogger.Error(FormattableString.Invariant($"Failed to run service task. Please check if registry '{DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()}' has a valid url."));

				return false;
			}

			return true;
		}

		protected ZQuery QueryForLoadingMessages
		{
			get
			{
				var failedMessageFilter = new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Failed)
					.AddToFilter(EDIMessageSchema.EM_RetryCount, SQLComparisonOperator.LessThan, (byte)5);

				var messageStatusFilter = new ZQuery(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued)
					.AddToFilter(failedMessageFilter, JoinCondition.Or);

				var query = new ZDBOnlyQuery(typeof(EDocsShipamaxMessage)) { MaximumRows = BatchSize }
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ShipamaxIntegration)
					.AddToFilter(EDIMessageSchema.EM_IsActive, true)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal)
					.AddToFilter(messageStatusFilter);

				query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_MessageNum_EM_SystemCreateTimeUtc));
				query.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_SystemLastEditTimeUtc"));

				if (excludedPKs.Count > 0)
				{
					_ = query.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, excludedPKs);
				}

				return query;
			}
		}

		protected virtual HttpClient GetHttpClient()
		{
			return new HttpClient();
		}

		protected virtual ShipamaxIntegrationTokenManager GetTokenManager()
		{
			return new ShipamaxIntegrationTokenManager();
		}

		void AddRequiredHeaders(HttpClient client, EDocsShipamaxMessage shipamaxMessage)
		{
			client.DefaultRequestHeaders.Remove(ShipamaxIntegrationConstants.HeaderNames.DocToken);

			var tokenManager = GetTokenManager();
			client.DefaultRequestHeaders.Add(ShipamaxIntegrationConstants.HeaderNames.DocToken, tokenManager.GenerateEDocsAuthToken(shipamaxMessage.PK.ToGuid(), shipamaxMessage.LinkedEDoc));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((NoResString)"Bearer", tokenManager.GetSystemToSystemTrustToken());
		}

		protected virtual Dictionary<string, string> GenerateQueryParams(StorageDocsBase linkedEDoc)
		{
			const string docType = nameof(docType);
			const string docPk = nameof(docPk);
			const string docTimestamp = nameof(docTimestamp);
			const string responseEndpointRoot = nameof(responseEndpointRoot);
			const string cw1Version = nameof(cw1Version);
			const string customerId = nameof(customerId);
			const string docMainPk = nameof(docMainPk);
			const string parseType = nameof(parseType);

			var queryParams = new Dictionary<string, string>
			{
				{ docType, linkedEDoc.SC_DocType },
				{ parseType, linkedEDoc.DocType?.RT_ParseType ?? string.Empty },
				{ docPk, linkedEDoc.PK.ToString() },
				{ docMainPk, linkedEDoc.ParentMain?.PK.ToString() ?? Guid.Empty.ToString() },
				{ docTimestamp, linkedEDoc.SC_Date.ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture) },
				{ responseEndpointRoot, GlowRegistry.Instance.GlowServiceUri }
			};

			var retriever = new EnterpriseInformationRetriever();
			queryParams.Add(cw1Version, retriever.VersionNumber);
			queryParams.Add(customerId, retriever.LicenceCode);

			return queryParams;
		}

		protected virtual Dictionary<string, string> GenerateQueryParamsV2(StorageDocsBase linkedEDoc)
		{
			var queryParams = new Dictionary<string, string>
			{
				{ "docPk", linkedEDoc.PK.ToString() },
				{ "docMainPk", linkedEDoc.ParentMain?.PK.ToString() ?? Guid.Empty.ToString() }
			};

			return queryParams;
		}

		protected static HttpContent GenerateRequestBodyContent(StorageDocsBase linkedEDoc)
		{
			var fileContent = new StreamContent(linkedEDoc.GetSC_ImageDataReader());
			var fullName = linkedEDoc.GetFileNameOnlyWithExtension();
			var mimeType = MimeTypes.GetMimeType(fullName);
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

			return fileContent;
		}

		protected static HttpContent GenerateRequestBodyContentV2(StorageDocsBase linkedEDoc)
		{
			var fileContent = new StreamContent(linkedEDoc.GetSC_ImageDataReader());
			var fullName = linkedEDoc.GetFileNameOnlyWithExtension();
			var mimeType = MimeTypes.GetMimeType(fullName);
			fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

			var retriever = new EnterpriseInformationRetriever();
			var utilityData = linkedEDoc.EDocsParsingSupport?.UtilityData ?? string.Empty;
			var multipartFormDataContent = new MultipartFormDataContent
			{
				{ new StringContent(linkedEDoc.SC_DocType, Encoding.UTF8, MediaTypeNames.Text.Plain), "docType" },
				{ new StringContent(linkedEDoc.DocType?.RT_ParseType ?? string.Empty, Encoding.UTF8, MediaTypeNames.Text.Plain), "parseType" },
				{ new StringContent(linkedEDoc.SC_Date.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture), Encoding.UTF8, MediaTypeNames.Text.Plain), "docTimestamp" },
				{ new StringContent(GlowRegistry.Instance.GlowServiceUri, Encoding.UTF8, MediaTypeNames.Text.Plain), "responseEndpointRoot" },
				{ new StringContent(retriever.VersionNumber, Encoding.UTF8, MediaTypeNames.Text.Plain), "cw1Version" },
				{ new StringContent(retriever.LicenceCode.Replace(" ", string.Empty), Encoding.UTF8, MediaTypeNames.Text.Plain), "customerId" },
				{ new StringContent(mimeType, Encoding.UTF8, MediaTypeNames.Text.Plain), "fileContentType" },
				{ new StringContent(utilityData, Encoding.UTF8, MediaTypeNames.Text.Plain), "utilityData" },
				{ fileContent, (NoResString)"file", fullName },
			};
			return multipartFormDataContent;
		}

		protected bool HandleRequestResponse(HttpStatusCode statusCode, EDocsShipamaxMessage shipamaxMessage)
		{
			if (statusCode == HttpStatusCode.OK)
			{
				shipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
				return true;
			}

			var statusDetails = ParseStatusCodeForDetails(statusCode);

			if (statusCode == (HttpStatusCode)420 ||
				shipamaxMessage.EM_Status == EDIMessageStatusList.Codes.Failed && ++shipamaxMessage.EM_RetryCount >= 5)
			{
				shipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Error;
				LogErrorAndMarkMessagePKExcluded(statusDetails, shipamaxMessage);
				return false;
			}

			shipamaxMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
			LogFailureAndMarkMessagePKExcluded(statusDetails, shipamaxMessage);
			return false;
		}

		void LogWarningAndMarkMessageInactive(string failureReason, EDocsShipamaxMessage shipamaxMessage)
		{
			var warningMessage = FormattableString.Invariant($"Document {shipamaxMessage.LinkedEDoc?.SC_FileName}, PK: '{shipamaxMessage.LinkedEDoc?.PK}' has been marked as inactive. Failure reason: {failureReason}");
			shipamaxMessage.EM_IsActive = false;
			ServiceLogger.Warning(warningMessage);
		}

		void LogErrorAndMarkMessagePKExcluded(string failureReason, EDocsShipamaxMessage shipamaxMessage)
		{
			var errorMessage = FormattableString.Invariant($"Document {shipamaxMessage.LinkedEDoc?.SC_FileName}, PK: '{shipamaxMessage.LinkedEDoc?.PK}' can't be processed. Reason: {failureReason}");
			excludedPKs.Add(shipamaxMessage.PK);
			ServiceLogger.Error(errorMessage);
		}

		void LogFailureAndMarkMessagePKExcluded(string failureReason, EDocsShipamaxMessage shipamaxMessage)
		{
			var errorMessage = FormattableString.Invariant($"Document {shipamaxMessage.LinkedEDoc?.SC_FileName}, PK: '{shipamaxMessage.LinkedEDoc?.PK}' will be re-processed in next service task run. Failure reason: {failureReason}");
			excludedPKs.Add(shipamaxMessage.PK);
			ServiceLogger.Error(errorMessage);
		}

		static string ParseStatusCodeForDetails(HttpStatusCode statusCode)
		{
			string message;

			switch (statusCode)
			{
				case HttpStatusCode.Unauthorized:
					message = (NoResString)"Authorization header is missing, or token is invalid.";
					break;
				case (HttpStatusCode)422:
					message = (NoResString)"The request is not in the expected format. Please check if CargoWise application has been upgraded to latest version.";
					break;
				case (HttpStatusCode)420:
					message = (NoResString)"The document type provided is not supported for parsing.";
					break;
				case HttpStatusCode.InternalServerError:
					message = (NoResString)"Error occurred in Document Ingestion service.";
					break;
				case HttpStatusCode.NotFound:
					message = FormattableString.Invariant($"Please check if the url entered in registry '{DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()}' is valid.");
					break;
				default:
					message = FormattableString.Invariant($"An invalid status code '{statusCode}' was received from Document Ingestion service.");
					break;
			}

			return message;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.Business.Test
{
	public sealed class EDocsShipamaxMessageProcessorTest : TestCaseWithDocumentFactory
	{
		[TestDate(2023, 04, 02, 0, 0, 0)]
		public void TestQueryForLoadingMessages()
		{
			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var query = processor.QueryLoadingMessagesExposed;

			AssertEquals("EM_ApplicationCode = 'SPM' and EM_IsActive = 1 and EM_ReceiveTransmit = 'INT' and (EM_Status = 'QUE' or (EM_Status = 'FAL' and EM_RetryCount < '5'))", query.LiteralTextADO);
		}

		public void TestPreConditionsBeforeProcessingMessages_Success()
		{
			TestPreConditionsBeforeProcessingMessages(
				civParsing: true,
				pinParsing: false,
				"https://test.com",
				result: true,
				string.Empty);

			TestPreConditionsBeforeProcessingMessages(
				civParsing: false,
				pinParsing: true,
				"https://test.com",
				result: true,
				string.Empty);

			TestPreConditionsBeforeProcessingMessages(
				civParsing: true,
				pinParsing: true,
				"https://test.com",
				result: true,
				string.Empty);
		}

		public void TestPreConditionsBeforeProcessingMessages_ParsingTypes_Disabled()
		{
			TestPreConditionsBeforeProcessingMessages(
				civParsing: false,
				pinParsing: false,
				"https://test.com",
				result: false,
				"Error|Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types.");
		}

		public void TestPreConditionsBeforeProcessingMessages_DocParserUrl_NotSet()
		{
			TestPreConditionsBeforeProcessingMessages(
				civParsing: true,
				pinParsing: false,
				string.Empty,
				result: false,
				$"Error|Failed to run service task. Please check if registry '{DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()}' has a valid url.");
		}

		void TestPreConditionsBeforeProcessingMessages(bool civParsing, bool pinParsing, string docParserUrl, bool result, string log)
		{
			var logger = new TestServiceLogger();
			var processor = new EDocsShipamaxMessageProcessorForTest(logger);

			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, civParsing))
			using (DocManagerRegistry.Instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, pinParsing))
			using (DocManagerRegistry.Instance.DocumentParserUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docParserUrl))
			{
				var processedResult = processor.CheckPreConditionsBeforeProcessingMessagesExposed();

				AssertEquals($"Result should be {result}", result, processedResult);
				AssertEquals(log, logger.ToString().Trim());
			}

			logger.ClearLog();
		}

		public void TestHandleRequestResponse_StatusCode200()
		{
			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var shipamaxMessage = MasterFactory.New<EDocsShipamaxMessage>();

			AssertEquals("Precondition - EM_Status should QUE", "QUE", shipamaxMessage.EM_Status);
			AssertEquals("Precondition - EM_IsActive should be true", expected: true, shipamaxMessage.EM_IsActive);

			var result = processor.HandleRequestResponseExposed(HttpStatusCode.OK, shipamaxMessage);

			AssertEquals("Result should be true", expected: true, result);
			AssertEquals("Message status should updated to SNT", "SNT", shipamaxMessage.EM_Status);
		}

		public void TestHandleRequestResponse_StatusCode400()
		{
			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var shipamaxMessage = MasterFactory.New<EDocsShipamaxMessage>();

			AssertEquals("Precondition - EM_Status should QUE", "QUE", shipamaxMessage.EM_Status);
			AssertEquals("Precondition - EM_IsActive should be true", expected: true, shipamaxMessage.EM_IsActive);

			var result = processor.HandleRequestResponseExposed(HttpStatusCode.NotFound, shipamaxMessage);

			AssertEquals("Result should be false", expected: false, result);
			AssertEquals("Message status will be updated to Failed", "FAL", shipamaxMessage.EM_Status);
			AssertEquals("Message is still active", expected: true, shipamaxMessage.EM_IsActive);
			AssertEquals("List count should be 1", 1, processor.ExcludedPKsExposed.Count);
			AssertEquals("Message PK should be in the list", shipamaxMessage.PK, processor.ExcludedPKsExposed[0]);
		}

		public void TestHandleRequestResponse_OtherInvalidStatusCodes()
		{
			AssertHandlingInvalidStatusCodes((HttpStatusCode)422, "The request is not in the expected format. Please check if CargoWise application has been upgraded to latest version.");
			AssertHandlingInvalidStatusCodes(HttpStatusCode.InternalServerError, "Error occurred in Document Ingestion service.");
			AssertHandlingInvalidStatusCodes(HttpStatusCode.Unauthorized, "Authorization header is missing, or token is invalid.");
			AssertHandlingInvalidStatusCodes(HttpStatusCode.NotFound, $"Please check if the url entered in registry '{DocManagerRegistry.Instance.DocumentParserUrl.GetLocationInEnglish()}' is valid.");
			AssertHandlingInvalidStatusCodes(HttpStatusCode.GatewayTimeout, "An invalid status code 'GatewayTimeout' was received from Document Ingestion service.");
		}

		public void TestHandleRequestResponse_StatusCode420()
		{
			var logger = new TestServiceLogger();
			var processor = new EDocsShipamaxMessageProcessorForTest(logger);
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;

			AssertEquals("Precondition - EM_Status should QUE", "QUE", shipamaxMessage.EM_Status);
			AssertEquals("Precondition - EM_IsActive should be true", expected: true, shipamaxMessage.EM_IsActive);

			var result = processor.HandleRequestResponseExposed((HttpStatusCode)420, shipamaxMessage);

			AssertEquals("Result should be false", expected: false, result);
			AssertEquals("Message should be still active", expected: true, shipamaxMessage.EM_IsActive);
			AssertEquals("Message should be in error status", "ERR", shipamaxMessage.EM_Status);
			AssertEquals($"Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' can't be processed. Reason: The document type provided is not supported for parsing.", logger.ToString().Trim());
		}

		public void TestHandleRequestResponse_FailFiveTimes()
		{
			var logger = new TestServiceLogger();
			var processor = new EDocsShipamaxMessageProcessorForTest(logger);
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;

			for (int i = 0; i < 6; i++)
			{
				_ = processor.HandleRequestResponseExposed(HttpStatusCode.NotFound, shipamaxMessage);
				AssertEquals("The retry count should be less or equal to 5", i < 5 ? i : 5, shipamaxMessage.EM_RetryCount);
				AssertEquals("The status should be FAL until the fifth retry then it changes to ERR", i < 5 ? "FAL" : "ERR", shipamaxMessage.EM_Status);
			}
			AssertEquals($@"Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.
Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.
Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.
Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.
Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.
Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' can't be processed. Reason: Please check if the url entered in registry 'System -> DocManager -> Document Ingestion -> Parser Configuration -> Parser URL' is valid.", logger.ToString().Trim());
		}

		public void TestGenerateRequestBody()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);
			eDoc.SC_ImageData = new byte[] { 1, 2, 3 };
			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());

			var requestBodyBytes = processor.GenerateRequestBodyContentExposed(eDoc).ReadAsByteArrayAsync().Result;

			AssertNotNull("Body content should not be null", requestBodyBytes);
			AssertEquals("Contents should be same", new byte[] { 1, 2, 3 }, requestBodyBytes);
		}

		public void TestGenerateRequestBodyV2()
		{
			const string testUtilityData = "Test Utility Data";
			Registry.Business.GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowurl.com");

			var eDoc = GetNewStorageDocsBaseWithParent(1);
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);
			eDoc.SC_ImageData = new byte[] { 1, 2, 3 };
			AssertNotNull(eDoc.DocType);
			AssertEquals("CIV", eDoc.DocType.RT_ParseType);

			var eDocsParsingSupportMock = new Mock<IEDocsParsingSupport>();
			eDocsParsingSupportMock.Setup(x => x.UtilityData).Returns(testUtilityData);
			eDoc.EDocsParsingSupport = eDocsParsingSupportMock.Object;

			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var multiFormDataContent = processor.GenerateRequestBodyContentExposedV2(eDoc) as MultipartFormDataContent;
			var retriever = new EnterpriseInformationRetriever();

			var docTypeContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("docType"));
			AssertEquals(eDoc.SC_DocType, docTypeContent.ReadAsStringAsync().Result);

			var parseTypeContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("parseType"));
			AssertEquals(eDoc.DocType.RT_ParseType, parseTypeContent.ReadAsStringAsync().Result);

			var docTimestampContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("docTimestamp"));
			AssertEquals(eDoc.SC_Date.ToString("yyyy-MM-ddTHH:mm:ssZ"), docTimestampContent.ReadAsStringAsync().Result);

			var responseEndpointRootContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("responseEndpointRoot"));
			AssertEquals("https://glowurl.com/", responseEndpointRootContent.ReadAsStringAsync().Result);

			var customerIdContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("customerId"));
			AssertEquals(retriever.LicenceCode.Replace(" ", string.Empty), customerIdContent.ReadAsStringAsync().Result);

			var cw1VersionContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("cw1Version"));
			AssertEquals(retriever.VersionNumber, cw1VersionContent.ReadAsStringAsync().Result);

			var fileTypeContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("fileContentType"));
			AssertEquals("application/pdf", fileTypeContent.ReadAsStringAsync().Result);

			var utilityDataContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("utilityData"));
			AssertEquals(testUtilityData, utilityDataContent.ReadAsStringAsync().Result);

			var fileContent = multiFormDataContent.Single(x => x.Headers.ContentDisposition.Name.Equals("file"));
			AssertEquals([1, 2, 3], fileContent.ReadAsByteArrayAsync().Result);
		}

		public void TestGenerateQueryParams()
		{
			Registry.Business.GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowurl.com");
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);
			AssertNotNull(eDoc.DocType);
			AssertEquals("CIV", eDoc.DocType.RT_ParseType);

			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var queryParams = processor.GenerateQueryParamsExposed(eDoc);

			AssertEquals(eDoc.SC_DocType, queryParams["docType"]);
			AssertEquals(eDoc.DocType.RT_ParseType, queryParams["parseType"]);
			AssertEquals(eDoc.PK.ToString(), queryParams["docPk"]);
			AssertEquals(eDoc.ParentMain.PK.ToString(), queryParams["docMainPk"]);
			AssertEquals(eDoc.SC_Date.ToString("yyyy-MM-ddTHH:mm:ssK"), queryParams["docTimestamp"]);
			AssertEquals("https://glowurl.com/", queryParams["responseEndpointRoot"]);

			var retriever = new EnterpriseInformationRetriever();

			AssertEquals(retriever.LicenceCode, queryParams["customerId"]);
			AssertEquals(retriever.VersionNumber, queryParams["cw1Version"]);

			eDoc.SC_SM = ZGuid.Empty;
			queryParams = processor.GenerateQueryParamsExposed(eDoc);
			AssertEquals(ZGuid.Empty.ToString(), queryParams["docMainPk"]);
		}

		public void TestGenerateQueryParamsV2()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);

			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var queryParams = processor.GenerateQueryParamsExposedV2(eDoc);

			AssertEquals(eDoc.PK.ToString(), queryParams["docPk"]);
			AssertEquals(eDoc.ParentMain.PK.ToString(), queryParams["docMainPk"]);
		}

		public void TestQueryForLoadingMessagesWithFailedMessages()
		{
			// Add 4 eDocs
			PrepareEDocsWithMessages(2, 1);

			var eDoc1 = GetNewStorageDocsBaseWithParent(1);
			var eDoc2 = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();

			// Set eDoc1 to Failed parse status and retry count to less than 5
			eDoc1.ActiveShipamaxMessage.EM_Status = "FAL";
			eDoc1.ActiveShipamaxMessage.EM_RetryCount = 4;

			// Set eDoc2 to Failed parse status and retry count to 5
			eDoc2.ActiveShipamaxMessage.EM_Status = "FAL";
			eDoc2.ActiveShipamaxMessage.EM_RetryCount = 5;
			MasterFactory.Save();

			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger());
			var query = processor.QueryLoadingMessagesExposed;
			var messagesToSend = MasterFactory.Load<EDIMessage>(query).ToArray();
			AssertEquals("There should be 3 documents to send", 3, messagesToSend.Length);
		}

		public void TestRequestUri()
		{
			PrepareEDocsWithMessages(2, 1);

			MasterFactory.Save();

			Uri requestUri = null;
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Callback((HttpRequestMessage message, CancellationToken _) =>
				{
					requestUri = message.RequestUri;
				})
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Successful")
				});

			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger())
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object,
				QueryParams = new Dictionary<string, string>
				{
					{ "key1", "value1" },
					{ "key2", "value2" },
					{ "key3", "value3" }
				}
			};

			processor.ProcessMessages(new CancellationToken(false));

			AssertEquals("?key1=value1&key2=value2&key3=value3", requestUri.Query);
		}

		public void TestRequestHeaders()
		{
			DocManagerRegistry.Instance.EDocsAuthTokenEncryptionKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EoQNIn4lnxcKPmSH2RPACA==");

			var eDoc = GetNewStorageDocsBaseWithParent(1);
			eDoc.SC_Date = new ZDateTime(2023, 11, 27);

			MasterFactory.Save();

			HttpRequestHeaders actualHeaders = null;
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Callback((HttpRequestMessage message, CancellationToken _) =>
				{
					actualHeaders = message.Headers;
				})
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Successful")
				});

			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			tokenManagerMock
				.Setup(t => t.GetSystemToSystemTrustToken())
				.Returns("5ddbb2ac-d026-459e-9474-f5ba257553e7");

			var processor = new EDocsShipamaxMessageProcessorForTest(new TestServiceLogger())
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			processor.ProcessMessages(new CancellationToken(false));

			AssertNotNull("Actual headers should not be null", actualHeaders);
			AssertEquals("Bearer", actualHeaders.Authorization.Scheme);
			AssertEquals("5ddbb2ac-d026-459e-9474-f5ba257553e7", actualHeaders.Authorization.Parameter);

			var result = actualHeaders.TryGetValues(ShipamaxIntegrationConstants.HeaderNames.DocToken, out var values);
			var valuesList = values.ToList();

			AssertEquals("Customized header Doc-Token should exist and have a value", expected: true, result);
			AssertEquals("There should be only one value in Doc-Token header", 1, valuesList.Count);

			result = tokenManagerMock.Object.TryDecryptEDocsAuthToken(valuesList[0], out var tokenDetails);

			AssertEquals("Token stored in the header is able to be decrypted", expected: true, result);
			AssertNotNull("Decrypted EDocsAuthTokenDetails object should not be null", tokenDetails);
		}

		public void TestProcessMessages_Successful()
		{
			PrepareEDocsWithMessages(2, 1);
			PrepareEDocsWithMessages(2, 2);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Successful")
				});
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			processor.ProcessMessages(new CancellationToken(false));

			var expectedMessage = @"Information|Task started successfully.
Information|Loaded and started processing 4 document(s).
Information|4 messages have been sent for parsing.
Information|No documents are available to be parsed.
Information|Task was completed with 4 messages sent.";
			AssertEquals(expectedMessage, testLogger.ToString().Trim());

			var query = new ZDBOnlyQuery(typeof(EDocsShipamaxMessage)) { ReLoadExistingRows = true };
			_ = query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ShipamaxIntegration)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal);
			var processedMessages = MasterFactory.Load<EDocsShipamaxMessage>(query);

			AssertEquals("There should be 4 EDocsShipamaxMessage records", 4, processedMessages.Length);
			AssertEquals("All messages should be active", expected: false, processedMessages.Any(m => !m.EM_IsActive));
			AssertEquals("The status for All messages should be SNT", expected: false, processedMessages.Any(m => m.EM_Status != "SNT"));
		}

		public void TestProcessMessages_FailedToLoadLinkedStorageDocs()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();

			var shipamaxMessage = eDoc.ActiveShipamaxMessage;
			shipamaxMessage.EM_LinkUniqueID = ZGuid.NewZGuid();
			MasterFactory.Save();

			var testLogger = new TestServiceLogger();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger);
			processor.ProcessMessages(new CancellationToken(false));

			var expectedMessage = $@"Information|Task started successfully.
Information|Loaded and started processing 1 document(s).
Warning|Document , PK: '' has been marked as inactive. Failure reason: Failed to load linked StorageDocs object '{shipamaxMessage.EM_LinkUniqueID}'.
Information|0 messages have been sent for parsing.
Information|No documents are available to be parsed.
Information|Task was completed with 0 messages sent.";
			AssertEquals(expectedMessage, testLogger.ToString().Trim());

			var query = new ZDBOnlyQuery(typeof(EDocsShipamaxMessage)) { ReLoadExistingRows = true };
			_ = query.AddToFilter(EDIMessageSchema.PK, shipamaxMessage.PK);
			var updatedShipamaxMessage = MasterFactory.LoadTop1<EDocsShipamaxMessage>(query);

			AssertNotNull(updatedShipamaxMessage);
			AssertEquals("Updated message should be inactive", expected: false, updatedShipamaxMessage.EM_IsActive);
		}

		public void TestProcessMessages_Cancelled()
		{
			PrepareEDocsWithMessages(2, 1);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Callback((HttpRequestMessage _, CancellationToken _) =>
				{
					Thread.Sleep(TimeSpan.FromSeconds(45));
				})
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Successful")
				});
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			using var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(45));
			_ = AssertExceptionThrown<OperationCanceledException>(() => processor.ProcessMessages(cancellationTokenSource.Token));
		}

		public void TestProcessMessages_NoDeadLoop()
		{
			PrepareEDocsWithMessages(2, 1);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError
				});
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			using (var cancellationTokenSource = new CancellationTokenSource())
			{
				cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1));
				processor.ProcessMessages(cancellationTokenSource.Token);
				AssertEquals("Information|Task was completed with 0 messages sent.", testLogger[testLogger.Count - 1].Trim());
				AssertEquals("Service task ends not because of cancellation", expected: false, cancellationTokenSource.IsCancellationRequested);
				AssertEquals("List count should be 2", 2, processor.ExcludedPKsExposed.Count);
			}

			ErrorReporter.Clear();
		}

		public void TestProcessMessages_TrailingSpaceIsTrimmedIsHandled()
		{
			PrepareEDocsWithMessages(1, 1);

			using (DocManagerRegistry.Instance.DocumentParserUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://example.com/ "))
			{
				var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
					.ReturnsAsync(new HttpResponseMessage
					{
						StatusCode = HttpStatusCode.OK,
						Content = new StringContent("Successful")
					});
				var testLogger = new TestServiceLogger();
				var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
				var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
				{
					MessageHandler = httpMessageHandlerMock.Object,
					TokenManager = tokenManagerMock.Object
				};

				processor.ProcessMessages(new CancellationToken(false));

				var expectedMessage = @"Information|Task started successfully.
Information|Loaded and started processing 1 document(s).
Information|1 messages have been sent for parsing.
Information|No documents are available to be parsed.
Information|Task was completed with 1 messages sent.";

				AssertEquals(expectedMessage, testLogger.ToString().Trim());
			}
		}

		public void TestProcessMessages_WebExceptionIsWrappedAsHostServiceException()
		{
			PrepareEDocsWithMessages(1, 1);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Throws((() => new HttpRequestException("message", new WebException())));
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			var exception = AssertExceptionThrown<HostedServiceException>(() => processor.ProcessMessages(new CancellationToken(false)));

			AssertEquals("The inner exception should be HttpRequestException", expected: true, exception.InnerException is HttpRequestException);
			AssertEquals("The inner most exception should be WebException", expected: true, exception.InnerException.InnerException is WebException);
		}

		public void TestProcessMessages_AuthenticationExceptionIsWrappedAsHostServiceException()
		{
			PrepareEDocsWithMessages(1, 1);

			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			tokenManagerMock.Setup(m => m.GetSystemToSystemTrustToken())
				.Throws(new AuthenticationException());

			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				TokenManager = tokenManagerMock.Object
			};

			var exception = AssertExceptionThrown<HostedServiceException>(() => processor.ProcessMessages(new CancellationToken(false)));

			AssertEquals("The inner exception should be AuthenticationException", expected: true, exception.InnerException is AuthenticationException);
		}

		public void TestProcessMessages_AccessTokenExceptionIsWrappedAsHostServiceException()
		{
			PrepareEDocsWithMessages(1, 1);

			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			tokenManagerMock.Setup(m => m.GetSystemToSystemTrustToken())
				.Throws(new InvalidOperationException("Failed to get access token. invalid_client."));

			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				TokenManager = tokenManagerMock.Object
			};

			var exception = AssertExceptionThrown<HostedServiceException>(() => processor.ProcessMessages(new CancellationToken(false)));

			AssertEquals("The inner exception should be InvalidOperationException", expected: true, exception.InnerException is InvalidOperationException);
		}

		public void TestProcessMessages_UriFormatExceptionIsWrappedAsHostServiceException()
		{
			PrepareEDocsWithMessages(1, 1);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Throws<UriFormatException>();
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			var exception = AssertExceptionThrown<HostedServiceException>(() => processor.ProcessMessages(new CancellationToken(false)));

			AssertEquals("The inner exception should be UriFormatException", expected: true, exception.InnerException is UriFormatException);
		}

		public void TestProcessMessages_NonNetworkExceptionIsReported()
		{
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.Throws<InvalidOperationException>();
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			AssertNoExceptionThrown(() => processor.ProcessMessages(new CancellationToken(false)));
			AssertEquals("EDocsShipamaxMessageProcessor_RunningError", ErrorReporter.LastKeyReported);
			AssertEquals($"An exception occurred while processing document {eDoc.SC_FileName}, PK: '{eDoc.PK}'.", ErrorReporter.LastMessageReported);
			AssertEquals("List count should be 1", 1, processor.ExcludedPKsExposed.Count);
			AssertEquals("Message PK should be in the list", eDoc.ActiveShipamaxMessage.PK, processor.ExcludedPKsExposed[0]);
			ErrorReporter.Clear();
		}

		public void TestProcessMessages_GroupByBranches()
		{
			var company1 = MasterFactory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch1 = MasterFactory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			var company2 = MasterFactory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			var branch2 = MasterFactory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			MasterFactory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				PrepareEDocsWithMessages(2, 1);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				PrepareEDocsWithMessages(2, 1);
			}

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Successful")
				});
			var testLogger = new TestServiceLogger();
			var tokenManagerMock = new Mock<ShipamaxIntegrationTokenManager>();
			var processor = new EDocsShipamaxMessageProcessorForTest(testLogger)
			{
				MessageHandler = httpMessageHandlerMock.Object,
				TokenManager = tokenManagerMock.Object
			};

			processor.ProcessMessages(new CancellationToken(false));

			AssertContainsExactElementsInAnyOrder("Shoud be two groups with PK of branch1 and branch2", new List<ZGuid> { branch1.PK, branch2.PK }, processor.GroupBranches);
		}

		#region Implementation

		void AssertHandlingInvalidStatusCodes(HttpStatusCode statusCode, string failureReason)
		{
			var logger = new TestServiceLogger();
			var processor = new EDocsShipamaxMessageProcessorForTest(logger);
			var eDoc = GetNewStorageDocsBaseWithParent(1);
			MasterFactory.Save();
			var shipamaxMessage = eDoc.ActiveShipamaxMessage;

			AssertEquals("Precondition - EM_Status should QUE", "QUE", shipamaxMessage.EM_Status);
			AssertEquals("Precondition - EM_IsActive should be true", expected: true, shipamaxMessage.EM_IsActive);
			AssertEquals("List count should be 0", 0, processor.ExcludedPKsExposed.Count);

			var result = processor.HandleRequestResponseExposed(statusCode, shipamaxMessage);

			AssertEquals("Result should be false", expected: false, result);
			AssertEquals("Message status will be updated to Failed", "FAL", shipamaxMessage.EM_Status);
			AssertEquals("Message is still active", expected: true, shipamaxMessage.EM_IsActive);
			AssertEquals("List count should be 1", 1, processor.ExcludedPKsExposed.Count);
			AssertEquals("Message PK should be in the list", shipamaxMessage.PK, processor.ExcludedPKsExposed[0]);
			AssertEquals($"Error|Document {eDoc.SC_FileName}, PK: '{eDoc.PK}' will be re-processed in next service task run. Failure reason: {failureReason}", logger.ToString().Trim());

			ErrorReporter.Clear();
		}

		void PrepareEDocsWithMessages(int messagesCount, int dbNumber)
		{
			for (var count = 0; count < messagesCount; ++count)
			{
				GetNewStorageDocsBaseWithParent(dbNumber);
			}

			MasterFactory.Save();
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

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			if (!dbHelper.DatabaseExists(1))
			{
				dbHelper.CreateDatabase(1);
			}

			if (!dbHelper.DatabaseExists(2))
			{
				dbHelper.CreateDatabase(2);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			DropDatabase(1);
			DropDatabase(2);
		}

		protected override void SetUp()
		{
			DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocManagerRegistry.Instance.DocumentParserUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://shipamax.com");
		}

		void DropDatabase(int dbNumber)
		{
			if (dbHelper.DatabaseExists(dbNumber))
			{
				var dbName = dbHelper.GetDatabaseName(dbNumber);
				dbHelper.DropDatabase(dbName);
			}
		}

		readonly DocManagerDBHelperTestClass dbHelper = new ();

		#endregion
	}
}

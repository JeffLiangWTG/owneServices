using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	[TestedType(typeof(CertificateManagementServiceTask))]
	public class CertificateManagementServiceTaskTest : ServiceTaskTestCase<CertificateManagementServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		string NewTestGuid => Guid.NewGuid().ToString();

		public void TestTaskActiveByDefault()
		{
			var hostedServiceAttribute = GetHostedServiceAttributeOrFail();
			Assert("The task should be active by default.", hostedServiceAttribute.ActiveByDefault);
		}

		public void TestInitialSequence()
		{
			var operationId = NewTestGuid;

			mockCertificateManager
				.Setup(m => m.RequestCertificate(It.IsAny<string>()))
				.Returns(operationId);
			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns(("", "", ZBlob.Empty, "PRC"));

			var dummyCsr = $"Dummy CSR {operationId}";
			mockTokenConfigWriterService
				.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(dummyCsr));

			var task = CreateServiceTask();
			task.RunTask(cancellationTokenSource.Token);

			CombineAssertions(() =>
			{
				AssertContainsInOrder(
					"Logs are not as expected",
					logger.ToString(),
					"Information|Starting initial certificate request.",
					"Information|Certificate is currently being processed. Download of the certificate will be attempted after 0 seconds.",
					"Information|Certificate is still being processed after 3 retries. Download of the certificate will be attempted next scheduled run.",
					"Information|Skip sending redirect url due to no client info."
					);
				AssertContainsExactElementsInExactOrder(
					"PrepareNewCertificateRequests are not as expected",
					new[] { "{}", },
					capturedPrepareNewCertificateRequests.Select(Serialize));
				AssertContainsExactElementsInExactOrder(
					"SetOperationIdRequests are not as expected",
					new[]
					{
						$$"""
						{"Csr":"{{dummyCsr}}","OperationId":"{{operationId}}"}
						""",
					},
					capturedSetOperationIdRequests.Select(Serialize));
			});

			mockCertificateManager.Verify(m => m.RequestCertificate(dummyCsr), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(operationId), Times.Exactly(3));
			mockAuthenticationService.Verify(m => m.GetAccessToken(), Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		public void TestInitialSequence_CertificateInProcess()
		{
			//Arrange
			var operationId = NewTestGuid;
			var dummyCsr = $"Dummy CSR {operationId}";

			var startingRegistryItem = new SystemToSystemTrustInfo
			{
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "private key",
#pragma warning restore CS0618 // For unit test purposes only
				CertificateSigningRequest = dummyCsr,
				OperationId = operationId,
			};

			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns(("", "", ZBlob.Empty, "PRC"));

			var task = CreateServiceTask();

			//Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			logger.ClearLog();
			task.RunTask(cancellationTokenSource.Token);

			AssertContainsInOrder(
				"Logs are not as expected",
				logger.ToString(),
				"Information|Starting certificate download.",
				"Information|Certificate is currently being processed. Download of the certificate will be attempted after 0 seconds.",
				"Information|Certificate is still being processed after 3 retries. Download of the certificate will be attempted next scheduled run.",
				"Information|Skip sending redirect url due to no client info."
			);

			mockCertificateManager.Verify(m => m.RequestCertificate(It.IsAny<string>()), Times.Never);
			mockCertificateManager.Verify(m => m.DownloadCertificate(operationId), Times.Exactly(3));
			mockAuthenticationService.Verify(m => m.GetAccessToken(), Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		public void TestInitialSequence_CertificateAndRedirectUrlCompletedOnTheSameRun()
		{
			var oidcConfig = OIDCConfigHelper.GetOIDCConfig();
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testapp.com");

			var operationId = NewTestGuid;
			var clientId = NewTestGuid;
			var tenantId = NewTestGuid;
			var certificate = ZBlob.FromAscii("certificate");
			var dummyCsr = $"Dummy CSR {operationId}";

			mockCertificateManager
				.Setup(m => m.RequestCertificate(It.IsAny<string>()))
				.Returns(operationId);
			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns((tenantId, clientId, certificate, "COM"));

			mockTokenConfigWriterService
				.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(dummyCsr));

			mockAuthenticationService
				.Setup(service => service.GetAccessToken())
				.Returns("mocktoken");

			mockApiHelper.Setup(helper => helper.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns((string relativeUri, StringContent content, string token) =>
				{
					if (relativeUri == "application/oidcregister")
					{
						return new SystemToSystemTrustApiResponse(HttpStatusCode.OK, "clientId");
					}

					return new SystemToSystemTrustApiResponse(HttpStatusCode.OK, "");
				});

			var actualApplicationRedirectUrlProcessor = new ApplicationRedirectUrlProcessor();
			mockRedirectUrlProcessor
				.Setup(m => m.ProcessAsync(mockApiHelper.Object, mockAuthenticationService.Object, logger, cancellationTokenSource.Token))
				.Returns(actualApplicationRedirectUrlProcessor.ProcessAsync);
			var task = CreateServiceTask();
			task.RunTask(cancellationTokenSource.Token);

			//Assert
			var expectedLogs = """
				Information|Task started.
				Information|SystemToSystemTrust api endpoint: https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/
				Information|Starting initial certificate request.
				Debug|Requesting new certificate with password.
				Information|New certificate requested.
				Information|Starting certificate download.
				Information|New certificate acquired.
				Information|Successfully registered OIDC client ID: 'clientId'.
				Information|Start to send the redirect urls.
				Information|Redirect urls have been sent.
				Information|Task completed.

				""";
			CombineAssertions(() =>
			{
				AssertContainsExactLinesInExactOrder(expectedLogs, logger.ToString());
				AssertContainsExactElementsInExactOrder(
					"PrepareNewCertificateRequests are not as expected",
					new[] { "{}", },
					capturedPrepareNewCertificateRequests.Select(Serialize));
				AssertContainsExactElementsInExactOrder(
					"SetOperationIdRequests are not as expected",
					new[]
					{
						$$"""
						  {"Csr":"{{dummyCsr}}","OperationId":"{{operationId}}"}
						  """,
					},
					capturedSetOperationIdRequests.Select(Serialize));
				AssertContainsExactElementsInExactOrder(
					"SetNewCertificateCredentialsRequests are not as expected",
					new[]
					{
						$$"""
						  {"OperationId":"{{operationId}}","TenantId":"{{tenantId}}","ClientId":"{{clientId}}","Certificate":"{{Convert.ToBase64String(certificate)}}"}
						  """,
					},
					capturedSetNewCertificateCredentialsRequests.Select(Serialize));
			});

			mockCertificateManager.Verify(m => m.RequestCertificate(It.IsAny<string>()), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(It.IsAny<string>()), Times.Once);
			mockAuthenticationService.Verify(m => m.GetAccessTokenAsync(cancellationTokenSource.Token), Times.Exactly(2));
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockApiHelper.Verify(helper => helper.SystemToSystemTrustApiPost("application/oidcregister", It.IsAny<StringContent>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.Verify(helper => helper.SystemToSystemTrustApiPost("application/oidcredirecturls", It.IsAny<StringContent>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(mockApiHelper.Object, mockAuthenticationService.Object, logger, cancellationTokenSource.Token),
				Times.Once);
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_ClientIdNotMatchRegistry()
		{
			SetupCommonMocksAndAssertsForOIDCClientId("clientIdInRegistry", "clientId");
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_ClientIdMatchRegistry()
		{
			SetupCommonMocksAndAssertsForOIDCClientId("clientIdInRegistry", "clientIdInRegistry");
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_PostBadRequest()
		{
			SetupCommonMocksAndAssertsForOIDCClientId("clientIdInRegistry", "clientIdInRegistry", isBadRequest: true);
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_ClientIdAndClientIdInRegistryBothAreEmpty()
		{
			SetupCommonMocksAndAssertsForOIDCClientId(string.Empty, string.Empty);
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_ClientIdIsEmpty()
		{
			SetupCommonMocksAndAssertsForOIDCClientId(string.Empty, "clientIdInRegistry");
		}

		public void TestInitialSequence_CertificateAndOIDCClientIdCompletedOnTheSameRun_IsOIDCFederatedWithWTGFalse()
		{
			SetupCommonMocksAndAssertsForOIDCClientId("clientIdInRegistry", "clientIdInRegistry", isOIDCFederatedWithWTG: false);
		}

		public void TestInitialSequence_RetryForCertificate()
		{
			var operationId = NewTestGuid;
			var clientId = NewTestGuid;
			var tenantId = NewTestGuid;
			var certificate = ZBlob.FromAscii("certificate");
			var dummyCsr = $"Dummy CSR {operationId}";

			mockCertificateManager.Setup(m => m.RequestCertificate(It.IsAny<string>())).Returns(operationId);
			mockCertificateManager
				.SetupSequence(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns(("", "", certificate, "PRC"))
				.Returns((tenantId, clientId, ZBlob.Empty, "PRC"))
				.Returns((tenantId, clientId, certificate, "COM"));

			var startingRegistryItem = new SystemToSystemTrustInfo
			{
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "private key",
#pragma warning restore CS0618 // For unit test purposes only
				CertificateSigningRequest = dummyCsr,
				OperationId = operationId,
			};
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			mockApiHelper.Setup(helper => helper.SecondsDelayedBetweenRequests).Returns(TimeSpan.FromSeconds(2));

			var task = CreateServiceTask();
			task.RunTask(cancellationTokenSource.Token);

			var expectedLogs = """
				Information|Task started.
				Information|SystemToSystemTrust api endpoint: https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/
				Information|Starting certificate download.
				Information|Certificate is currently being processed. Download of the certificate will be attempted after 2 seconds.
				Information|Certificate is currently being processed. Download of the certificate will be attempted after 2 seconds.
				Information|New certificate acquired.
				Information|Skip registering OIDC because the OIDC is not enabled.
				Information|Task completed.

				""";
			CombineAssertions(() =>
			{
				AssertContainsExactLinesInExactOrder(expectedLogs, logger.ToString());
				AssertContainsExactElementsInExactOrder(
					"SetNewCertificateCredentialsRequests are not as expected",
					new[]
					{
						$$"""
						  {"OperationId":"{{operationId}}","TenantId":"{{tenantId}}","ClientId":"{{clientId}}","Certificate":"{{Convert.ToBase64String(certificate)}}"}
						  """,
					},
					capturedSetNewCertificateCredentialsRequests.Select(Serialize));
			});
			mockCertificateManager.Verify(m => m.RequestCertificate(It.IsAny<string>()), Times.Never);
			mockCertificateManager.Verify(m => m.DownloadCertificate(operationId), Times.Exactly(3));
			mockAuthenticationService.Verify(m => m.GetAccessToken(), Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(mockApiHelper.Object, mockAuthenticationService.Object, logger, cancellationTokenSource.Token),
				Times.Never);
		}

		public void TestRolloverSequence()
		{
			//Arrange
			var oldCertificate = ZBlob.FromAscii("certificate about to expire");
			var oldClientId = NewTestGuid;
			var oldTenantId = NewTestGuid;
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				Certificate = oldCertificate,
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "old private key",
#pragma warning restore CS0618 // For unit test purposes only
				ClientId = oldClientId,
				TenantId = oldTenantId
			};

			var operationId = NewTestGuid;
			var dummyCsr = $"Dummy CSR {operationId}";

			mockCertificateManager
				.Setup(m => m.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), null))
				.Returns(operationId);
			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns(("", "", ZBlob.Empty, "PRC"));

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, null));

			mockTokenConfigWriterService
				.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(dummyCsr));
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager
				.Setup(m => m.IsAboutToExpire(It.IsAny<byte[]>()))
				.Returns(true);
			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync("0");

			var task = CreateServiceTask();

			//Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			//Assert
			CombineAssertions(() =>
			{
				AssertContainsInOrder(
					"Logs are not as expected",
					logger.ToString(),
					"Information|Certificate expires in less than 2 months. Starting rollover certificate request.",
					"Information|Certificate is currently being processed. Download of the certificate will be attempted after 0 seconds.",
					"Information|Certificate is still being processed after 3 retries. Download of the certificate will be attempted next scheduled run.",
					"Information|Skip sending redirect url due to no client info."
				);
				AssertContainsExactElementsInExactOrder(
					"PrepareNewCertificateRequests are not as expected",
					new[] { "{}", },
					capturedPrepareNewCertificateRequests.Select(Serialize));
				AssertContainsExactElementsInExactOrder(
					"SetOperationIdRequests are not as expected",
					new[]
					{
						$$"""
						{"Csr":"{{dummyCsr}}","OperationId":"{{operationId}}"}
						""",
					},
					capturedSetOperationIdRequests.Select(Serialize));
			});
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			mockCertificateManager.Verify(m => m.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), null), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(It.IsAny<string>()), Times.Exactly(3));
			mockCertificateManager.Verify(m => m.IsExpired(oldCertificate), Times.Once);
			mockCertificateManager.Verify(m => m.IsAboutToExpire(oldCertificate), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		public void TestRolloverSequence_CertificateIsExpired()
		{
			//Arrange
			var oldCertificate = ZBlob.FromAscii("certificate is expired");
			var oldClientId = NewTestGuid;
			var oldTenantId = NewTestGuid;
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				Certificate = oldCertificate,
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "old private key",
#pragma warning restore CS0618 // For unit test purposes only
				ClientId = oldClientId,
				TenantId = oldTenantId
			};

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, null));
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(true);

			var task = CreateServiceTask();

			//Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			//Assert
			CombineAssertions(() =>
			{
				AssertContainsInOrder(
					"Logs are not as expected",
					logger.ToString(),
					"Warning|Reset the SystemToSystemTrustInfo because the certificate has expired."
				);
			});

			mockCertificateManager.Verify(m => m.IsExpired(oldCertificate), Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Never);
		}

		public void TestRolloverSequence_CertificateCompleted()
		{
			//Arrange
			var oldCertificate = ZBlob.FromAscii("old certificate");
			var oldClientId = NewTestGuid;
			var oldTenantId = NewTestGuid;

			var operationId = NewTestGuid;
			var dummyCsr = $"Dummy CSR {operationId}";

			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				Certificate = oldCertificate,
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "old private key",
				RolloverPrivateKey = "rollover private key",
#pragma warning restore CS0618 // For unit test purposes only
				CertificateSigningRequest = dummyCsr,
				ClientId = oldClientId,
				TenantId = oldTenantId,
				OperationId = operationId
			};

			var certificate = ZBlob.FromAscii("new certificate");
			var clientId = NewTestGuid;
			var tenantId = NewTestGuid;

			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns((tenantId, clientId, certificate, "COM"));

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, null));

			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager
				.Setup(m => m.IsAboutToExpire(It.IsAny<byte[]>()))
				.Returns(true);

			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync("0");

			const string firstRelevantLogEntry = "Information|Starting certificate download.";
			const string lastRelevantLogEntry = "Information|New certificate acquired.";

			var task = CreateServiceTask();

			//Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			logger.ClearLog();
			task.RunTask(cancellationTokenSource.Token);

			//Assert
			CombineAssertions(() =>
			{
				AssertContainsInOrder(
					"Logs are not as expected",
					logger.ToString(),
					firstRelevantLogEntry,
					lastRelevantLogEntry
				);
				AssertContainsExactElementsInExactOrder(
					"SetNewCertificateCredentialsRequests are not as expected",
					new[]
					{
						$$"""
						{"OperationId":"{{operationId}}","TenantId":"{{tenantId}}","ClientId":"{{clientId}}","Certificate":"{{Convert.ToBase64String(certificate)}}"}
						""",
					},
					capturedSetNewCertificateCredentialsRequests.Select(Serialize));
			});

			mockCertificateManager.Verify(m => m.RolloverCertificate(It.IsAny<string>(), It.IsAny<string>(), null), Times.Never);
			mockCertificateManager.Verify(m => m.DownloadCertificate(operationId), Times.Once);
			mockCertificateManager.Verify(m => m.IsExpired(It.IsAny<byte[]>()), Times.Once);
			mockCertificateManager.Verify(m => m.IsAboutToExpire(It.IsAny<byte[]>()), Times.Never);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.Never());
			mockAuthenticationService.Verify(m => m.GetAccessToken(), Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(mockApiHelper.Object, mockAuthenticationService.Object, logger, cancellationTokenSource.Token),
				Times.Never);
		}

		public void TestAutoHealIfApplicationDoesNotExistInAzure()
		{
			// Arrange
			var operationId = Guid.NewGuid().ToString();
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				ClientId = "invalidClientId",
				TenantId = "tenantId",
				OperationId = operationId,
				CertificateSigningRequest = "CSR",
#pragma warning disable CS0618 // For unit test purposes only
				RolloverPrivateKey = "rolloverPrivateKey",
				PrivateKey = "privateKey",
				LegacyPrivateKey = "legacyPrivateKey",
#pragma warning restore CS0618 // For unit test purposes only
				Certificate = new ZBlob(new byte[] { 1, 2, 3 }),
				LegacyCertificate = new ZBlob(new byte[] { 1 })
			};

			mockApiHelper
				.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, ""));
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync("0");

			var task = CreateServiceTask();

			// Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			// Assert
			var expectedLog = "Reset the SystemToSystemTrustInfo because the application with client id 'invalidClientId' does not exist.";
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains(expectedLog, logger.ToString());
				AssertContainsExactElementsInExactOrder(
					"ResetAccessTokenRequests should not have been called",
					Array.Empty<string>(),
					capturedResetAccessTokenRequests.Select(Serialize));
			});
			mockTokenConfigWriterService.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockCertificateManager.Verify(m => m.IsExpired(It.IsAny<byte[]>()), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(operationId), Times.Exactly(3));
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockTokenServicesFactory.Verify(m => m.GetTokenConfigWriterService(), Times.Once);
			mockTokenServicesFactory.VerifyNoOtherCalls();
			mockTokenServicesFactory.Invocations.Clear();

			// Re-arrange
			mockApiHelper
				.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.NotFound, ""));

			// Act
			task.RunTask(cancellationTokenSource.Token);

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertContains(expectedLog, logger.ToString());
				AssertContainsExactElementsInExactOrder(
					"ResetAccessTokenRequests are not as expected",
					new[] { "{}" },
					capturedResetAccessTokenRequests.Select(Serialize));
			});

			mockTokenConfigWriterService.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), It.IsAny<CancellationToken>()),
				Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet("application/invalidClientId", null), Times.Exactly(2));
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.AtLeastOnce);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce);
		}

		public void TestCertificateStillValid()
		{
			//Arrange
			var oldCertificate = ZBlob.FromAscii("valid certificate");
			var oldClientId = NewTestGuid;
			var oldTenantId = NewTestGuid;

			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				Certificate = oldCertificate,
#pragma warning disable CS0618 // For unit test purposes only
				PrivateKey = "old private key",
#pragma warning restore CS0618 // For unit test purposes only
				ClientId = oldClientId,
				TenantId = oldTenantId
			};

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, ""));
			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync("0");
			mockCertificateManager.Setup(m => m.IsAboutToExpire(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);

			var task = CreateServiceTask();

			//Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			logger.ClearLog();
			task.RunTask(cancellationTokenSource.Token);

			var taskFinishedLogEntry = """
				Information|Task started.
				Information|SystemToSystemTrust api endpoint: https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/
				Information|Certificate is valid and current.
				Information|Skip registering OIDC because the OIDC is not enabled.
				Information|Task completed.

				""";

			AssertContainsExactLinesInExactOrder(taskFinishedLogEntry, logger.ToString());
			mockCertificateManager.Verify(m => m.DownloadCertificate(It.IsAny<string>()), Times.Never);
			mockCertificateManager.Verify(m => m.IsExpired(oldCertificate), Times.Once);
			mockCertificateManager.Verify(m => m.IsAboutToExpire(oldCertificate), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.Never);
			mockAuthenticationService.Verify(m => m.GetAccessToken(), Times.Never);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), It.IsAny<CancellationToken>()),
				Times.Never);
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(mockApiHelper.Object, mockAuthenticationService.Object, logger, cancellationTokenSource.Token),
				Times.Never);
		}

		public void TestDoNotRunTaskDueToInvalidRegistrySettings()
		{
			var task = CreateServiceTask();

			var mockReg = new Mock<IProductRegistration>();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(true);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(true);

			using (ObjectFactory.Substitute(mockReg.Object))
			using (SystemDataRegistry.Instance.EnableCertificateManagementServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				task.RunTask(cancellationTokenSource.Token);
				AssertEquals("Warning|The registry setting 'System -> System-to-System Trust -> Enable System To System Trust Certificate Management Service Task' requires a value equal to 'True'.\r\nWarning|The registry setting 'Web and Visibility -> Web Component URLs -> CargoWise User Portal URL' has not been configured.\r\n", logger.ToString());
			}

			logger.ClearLog();
			RegistryItemDictionary.Instance.PurgeAll();

			using (ObjectFactory.Substitute(mockReg.Object))
			using (SystemDataRegistry.Instance.EnableCertificateManagementServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				task.RunTask(cancellationTokenSource.Token);
				AssertEquals("Warning|The registry setting 'System -> System-to-System Trust -> Enable System To System Trust Certificate Management Service Task' requires a value equal to 'True'.\r\n", logger.ToString());
			}

			logger.ClearLog();
			RegistryItemDictionary.Instance.PurgeAll();
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalDeveloperSystem()).Returns(false);
			mockReg.Setup(reg => reg.IsWiseTechGlobalInternalUATSystem()).Returns(false);

			using (ObjectFactory.Substitute(mockReg.Object))
			using (SystemDataRegistry.Instance.EnableCertificateManagementServiceTask.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				task.RunTask(cancellationTokenSource.Token);
				AssertEquals("Warning|The registry setting 'Web and Visibility -> Web Component URLs -> CargoWise User Portal URL' has not been configured.\r\n", logger.ToString());
			}
		}

		public void TestLogCertificateManagementException_SystemToSystemTrustCertificateManagementException()
		{
			LogCertificateManagementException(new SystemToSystemTrustCertificateManagementException("The api return a exception"));
		}

		public void TestLogCertificateManagementException_CertificateManagementException()
		{
			LogCertificateManagementException(new CertificateManagementException("There was a certificate management exception"));
		}

		public void TestLogCertificateManagementException_ArgumentNullException()
		{
			LogCertificateManagementException(new ArgumentNullException(null, "The argument was null"));
		}

		public void TestLogCertificateManagementException_InvalidOperationException()
		{
			LogCertificateManagementException(new InvalidOperationException("The operation was invalid"));
		}

		void LogCertificateManagementException(Exception ex)
		{
			var csr = NewTestGuid;
			mockCertificateManager
				.Setup(o => o.RequestCertificate(It.IsAny<string>()))
				.Throws(() => ex);
			mockTokenConfigWriterService.Setup(
				m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(csr));

			var task = CreateServiceTask();

			task.RunTask(cancellationTokenSource.Token);

			CombineAssertions(() =>
			{
				AssertContains("We log the exception", ex.Message, logger.ToString());
				AssertEquals("We don't report this exception", 0, ExceptionReporterTestListener.Instance.Count);
				AssertContainsExactElementsInExactOrder(
					"PrepareNewCertificateRequests are not as expected",
					new[] { "{}", },
					capturedPrepareNewCertificateRequests.Select(Serialize));
			});

			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockCertificateManager.Verify(m => m.RequestCertificate(csr), Times.Once);
		}

		public void TestShouldThrowUnExpectedException()
		{
			var csr = NewTestGuid;
			mockCertificateManager
				.Setup(m => m.RequestCertificate(It.IsAny<string>()))
				.Throws(new Exception("The api return a unhandled exception"));
			mockTokenConfigWriterService.Setup(
				m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(csr));

			var task = CreateServiceTask();
			AssertExceptionThrown<Exception>(() => task.RunTask(cancellationTokenSource.Token));

			AssertContainsExactElementsInExactOrder(
				"PrepareNewCertificateRequests are not as expected",
				new[] { "{}", },
				capturedPrepareNewCertificateRequests.Select(Serialize));

			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockCertificateManager.Verify(m => m.RequestCertificate(csr), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
		}

		public void TestShouldResetRegistryItemWhenDatabaseNumberIsDifferentFromLicenceInfo()
		{
			// Arrange
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				ClientId = "clientId",
				TenantId = "tenantId",
				OperationId = Guid.NewGuid().ToString(),
				CertificateSigningRequest = "CSR",
#pragma warning disable CS0618 // For unit test purposes only
				RolloverPrivateKey = "rolloverPrivateKey",
				PrivateKey = "privateKey",
				LegacyPrivateKey = "legacyPrivateKey",
#pragma warning restore CS0618 // For unit test purposes only
				Certificate = new ZBlob(new byte[] { 1, 2, 3 }),
				LegacyCertificate = new ZBlob(new byte[] { 1 })
			};

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, ""));
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync("100");

			var task = CreateServiceTask();

			// Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			// Assert
			CombineAssertions(() =>
			{
				AssertContains("Reset the SystemToSystemTrustInfo because the database number in application '100' does not match registry '0'.", logger.ToString());
				AssertContainsExactElementsInExactOrder(
					"ResetAccessTokenRequests are not as expected",
					new[] { "{}" },
					capturedResetAccessTokenRequests.Select(Serialize));
			});

			mockCertificateManager.Verify(m => m.IsExpired(It.IsAny<byte[]>()), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.ResetAccessTokenAsync(It.IsAny<ResetAccessTokenRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet("application/clientId", null), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.AtLeastOnce);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.Never);
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		public void TestStopTheRunWhenExceptionThrownDuringDatabaseNumberValidation()
		{
			// Arrange
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				ClientId = "clientId",
				TenantId = "tenantId",
				OperationId = Guid.NewGuid().ToString(),
				CertificateSigningRequest = "CSR",
#pragma warning disable CS0618 // For unit test purposes only
				RolloverPrivateKey = "rolloverPrivateKey",
				PrivateKey = "privateKey",
				LegacyPrivateKey = "legacyPrivateKey",
#pragma warning restore CS0618 // For unit test purposes only
				Certificate = new ZBlob(new byte[] { 1, 2, 3 }),
				LegacyCertificate = new ZBlob(new byte[] { 1 })
			};

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, ""));
			mockCertificateManager.Setup(m => m.IsExpired(It.IsAny<byte[]>())).Returns(false);
			mockCertificateManager
				.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>()))
				.ThrowsAsync(new SystemToSystemTrustCertificateManagementException("Request Failed"));

			var task = CreateServiceTask();

			// Act
			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			AssertContains("Error|Request Failed", logger.ToString());

			mockCertificateManager.Verify(m => m.IsExpired(It.IsAny<byte[]>()), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token), Times.Once);
			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet("application/clientId", null), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.AtLeastOnce);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.Never);
			mockRedirectUrlProcessor.Verify(
				m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
				Times.Never);
		}

		[ExpectNoExceptions]
		public void TestShouldNotLoadDatabaseNumberIfCertificateIsEmpty()
		{
			var startingRegistryItem = new SystemToSystemTrustInfo
			{
				ClientId = "clientId",
				TenantId = "tenantId",
				OperationId = Guid.NewGuid().ToString(),
#pragma warning disable CS0618 // For unit test
				PrivateKey = "privateKey",
#pragma warning restore CS0618 // For unit test
			};

			mockApiHelper.Setup(m => m.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>())).Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, ""));
			mockCertificateManager.Setup(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>())).Throws(new SystemToSystemTrustCertificateManagementException("Request Failed"));
			mockCertificateManager.Setup(m => m.DownloadCertificate(It.IsAny<string>())).Returns(("tenantId", "clientId", ZBlob.FromAscii("certificate"), "PRC"));

			var task = CreateServiceTask();

			SystemDataRegistry.Instance.SystemToSystemCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startingRegistryItem);
			task.RunTask(cancellationTokenSource.Token);

			mockApiHelper.Verify(m => m.SystemToSystemTrustApiGet("application/clientId", null), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(It.IsAny<string>()), Times.Once);
			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.AtLeastOnce);
			mockRedirectUrlProcessor.Verify(m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()), Times.Never);
			mockTokenConfigWriterService.Verify(m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), cancellationTokenSource.Token), Times.Once);
			mockCertificateManager.Verify(m => m.LoadDatabaseNumberByClientIdAsync(It.IsAny<CancellationToken>()), Times.Never);
		}

		void SetupCommonMocksAndAssertsForOIDCClientId(string clientIdInRegistry, string clientIdReturned, bool isBadRequest = false, bool isOIDCFederatedWithWTG = true)
		{
			var oidcConfig = OIDCConfigHelper.GetOIDCConfig();
			SystemDataRegistry.Instance.OIDCConfig.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oidcConfig);
			SystemDataRegistry.Instance.OIDCClientIDForWebApplications.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, clientIdInRegistry);

			if (!isOIDCFederatedWithWTG)
			{
				SystemDataRegistry.Instance.IsOIDCFederatedWithWTG.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			}

			var operationId = NewTestGuid;
			var clientId = NewTestGuid;
			var tenantId = NewTestGuid;
			var certificate = ZBlob.FromAscii("certificate");

			mockCertificateManager
				.Setup(m => m.RequestCertificate(It.IsAny<string>()))
				.Returns(operationId);
			mockCertificateManager
				.Setup(m => m.DownloadCertificate(It.IsAny<string>()))
				.Returns((tenantId, clientId, certificate, "COM"));
			mockAuthenticationService
				.Setup(service => service.GetAccessToken())
				.Returns("mocktoken");

			mockApiHelper.Setup(helper => helper.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns((string relativeUri, StringContent content, string token) =>
				{
					if (relativeUri == "application/oidcregister")
					{
						return isBadRequest
							? new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, "Error message")
							: new SystemToSystemTrustApiResponse(HttpStatusCode.OK, clientIdReturned);
					}

					return new SystemToSystemTrustApiResponse(HttpStatusCode.OK, "");
				});

			var dummyCsr = $"Dummy CSR {operationId}";
			mockTokenConfigWriterService
				.Setup(m => m.PrepareNewCertificateAsync(Capture.In(capturedPrepareNewCertificateRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new PrepareNewCertificateResponse(dummyCsr));

			var task = CreateServiceTask();
			task.RunTask(cancellationTokenSource.Token);

			string expectedLogMessage;
			if (isBadRequest)
			{
				expectedLogMessage = "Error|OIDC registration failed with error: Error message";
			}
			else if (!isOIDCFederatedWithWTG)
			{
				expectedLogMessage = "Information|Skip registering OIDC because the OIDC flow is not using WTG B2C for Identity Federation.";
			}
			else if (string.IsNullOrEmpty(clientIdReturned))
			{
				expectedLogMessage = "Information|OIDC application is not created in Azure yet. AuthorityUrl: 'https://test.com'.";
			}
			else
			{
				expectedLogMessage = clientIdInRegistry.Equals(clientIdReturned)
					? $"Information|OIDC client ID '{clientIdInRegistry}' is already registered and matches the value in the registry."
					: $"Information|Successfully registered OIDC client ID: '{clientIdReturned}'.";
			}

			var expectedLog = $"""
				Information|Task started.
				Information|SystemToSystemTrust api endpoint: https://myaccount-portal.cargowise.com/myaccount/api/SystemTrust/
				Information|Starting initial certificate request.
				Debug|Requesting new certificate with password.
				Information|New certificate requested.
				Information|Starting certificate download.
				Information|New certificate acquired.
				{expectedLogMessage}
				Information|Task completed.

				""";

			AssertContainsExactLinesInExactOrder(expectedLog, logger.ToString());

			mockCertificateManager.Verify(m => m.RequestCertificate(It.IsAny<string>()), Times.Once);
			mockCertificateManager.Verify(m => m.DownloadCertificate(It.IsAny<string>()), Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.PrepareNewCertificateAsync(It.IsAny<PrepareNewCertificateRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetOperationIdAsync(It.IsAny<SetOperationIdRequest>(), cancellationTokenSource.Token),
				Times.Once);
			mockTokenConfigWriterService.Verify(
				m => m.SetNewCertificateCredentialsAsync(It.IsAny<SetNewCertificateCredentialsRequest>(), cancellationTokenSource.Token),
				Times.Once);
			if (isOIDCFederatedWithWTG)
			{
				mockAuthenticationService.Verify(m => m.GetAccessTokenAsync(cancellationTokenSource.Token), Times.Once);
				mockApiHelper.Verify(helper => helper.SystemToSystemTrustApiPost("application/oidcregister", It.IsAny<StringContent>(), It.IsAny<string>()), Times.Once);
			}
			else
			{
				mockAuthenticationService.Verify(m => m.GetAccessTokenAsync(cancellationTokenSource.Token), Times.Never);
				mockApiHelper.Verify(helper => helper.SystemToSystemTrustApiPost("application/oidcregister", It.IsAny<StringContent>(), It.IsAny<string>()), Times.Never);
			}

			mockApiHelper.VerifyGet(m => m.SystemToSystemTrustApiEndpoint, Times.Once);
			mockApiHelper.VerifyGet(m => m.SecondsDelayedBetweenRequests, Times.AtLeastOnce());
			if (isBadRequest || !isOIDCFederatedWithWTG || string.IsNullOrEmpty(clientIdReturned))
			{
				mockRedirectUrlProcessor.Verify(
					m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
					Times.Never);
			}
			else
			{
				mockRedirectUrlProcessor.Verify(
					m => m.ProcessAsync(It.IsAny<ISystemToSystemTrustApiHelper>(), It.IsAny<IAuthenticationService>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()),
					Times.Once);
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
			capturedPrepareNewCertificateRequests = new List<PrepareNewCertificateRequest>();
			capturedSetNewCertificateCredentialsRequests = new List<SetNewCertificateCredentialsRequest>();
			capturedSetOperationIdRequests = new List<SetOperationIdRequest>();
			capturedResetAccessTokenRequests = new List<ResetAccessTokenRequest>();
			logger = new TestServiceLogger();

			mockApiHelper = new Mock<ISystemToSystemTrustApiHelper>();
			mockApiHelper.Setup(helper => helper.SystemToSystemTrustApiEndpoint).Returns(new SystemToSystemTrustApiHelper().SystemToSystemTrustApiEndpoint);
			mockCertificateManager = new Mock<ICertificateManager>();
			mockAuthenticationService = new Mock<IAuthenticationService>();
			mockRedirectUrlProcessor = new Mock<IApplicationRedirectUrlProcessor>();
			mockTokenConfigWriterService = new Mock<ITokenConfigWriterService>();
			mockTokenServicesFactory = new Mock<ITokenServicesFactory>();

			mockTokenConfigWriterService
				.Setup(m => m.SetOperationIdAsync(Capture.In(capturedSetOperationIdRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new SetOperationIdResponse());
			mockTokenConfigWriterService
				.Setup(m => m.SetNewCertificateCredentialsAsync(Capture.In(capturedSetNewCertificateCredentialsRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new SetNewCertificateCredentialsResponse());
			mockTokenConfigWriterService
				.Setup(m => m.ResetAccessTokenAsync(Capture.In(capturedResetAccessTokenRequests), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new ResetAccessTokenResponse());

			mockTokenServicesFactory
				.Setup(m => m.GetTokenConfigWriterService())
				.Returns(() => mockTokenConfigWriterService.Object);
		}

		protected override void TearDownCore()
		{
			cancellationTokenSource.Dispose();
			base.TearDownCore();
			mockApiHelper.VerifyNoOtherCalls();
			mockCertificateManager.VerifyNoOtherCalls();
			mockAuthenticationService.VerifyNoOtherCalls();
			mockRedirectUrlProcessor.VerifyNoOtherCalls();
			mockTokenConfigWriterService.VerifyNoOtherCalls();
			mockTokenServicesFactory.Verify(m => m.GetTokenConfigWriterService(), Times.AtMostOnce);
			mockTokenServicesFactory.VerifyNoOtherCalls();
		}

		string Serialize(object obj)
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
		}

		CertificateManagementServiceTask CreateServiceTask()
		{
			return new CertificateManagementServiceTask(
				mockCertificateManager.Object,
				mockApiHelper.Object,
				mockAuthenticationService.Object,
				mockRedirectUrlProcessor.Object,
				mockTokenServicesFactory.Object)
			{
				ServiceLogger = logger
			};
		}

		CancellationTokenSource cancellationTokenSource;
		IList<PrepareNewCertificateRequest> capturedPrepareNewCertificateRequests;
		IList<SetNewCertificateCredentialsRequest> capturedSetNewCertificateCredentialsRequests;
		IList<SetOperationIdRequest> capturedSetOperationIdRequests;
		IList<ResetAccessTokenRequest> capturedResetAccessTokenRequests;
		Mock<ISystemToSystemTrustApiHelper> mockApiHelper;
		Mock<ICertificateManager> mockCertificateManager;
		Mock<IAuthenticationService> mockAuthenticationService;
		Mock<IApplicationRedirectUrlProcessor> mockRedirectUrlProcessor;
		Mock<ITokenServicesFactory> mockTokenServicesFactory;
		Mock<ITokenConfigWriterService> mockTokenConfigWriterService;
		TestServiceLogger logger;
	}
}

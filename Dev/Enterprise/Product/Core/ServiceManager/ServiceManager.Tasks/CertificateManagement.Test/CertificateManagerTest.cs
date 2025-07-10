using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement.Test
{
	public class CertificateManagerTest : TransactionedTestCase
	{
		public void TestObjectFactory_IsRegistered_CreatesInstance()
		{
			// Arrange, Act
			var result = ObjectFactory.Get<ICertificateManager>();

			// Assert
			AssertNotNull(result);
			AssertType<CertificateManager>(result);
		}

		public void TestIsAboutToExpire_WithCertificateAboutToExpire_ReturnsTrue()
		{
			var manager = CreateCertificateManager();

			CombineAssertions(() =>
			{
				CheckIsAboutToExpire(manager, "last month", true, DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now.AddMonths(-2));
				CheckIsAboutToExpire(manager, "1 day", true, DateTimeOffset.Now.AddDays(1));
				CheckIsAboutToExpire(manager, "27 days", true, DateTimeOffset.Now.AddDays(27));
				CheckIsAboutToExpire(manager, "32 days", true, DateTimeOffset.Now.AddDays(32));
				CheckIsAboutToExpire(manager, "57 days", true, DateTimeOffset.Now.AddDays(57));
				CheckIsAboutToExpire(manager, "63 days", false, DateTimeOffset.Now.AddDays(63));
				CheckIsAboutToExpire(manager, "3 months", false, DateTimeOffset.Now.AddMonths(3));
				CheckIsAboutToExpire(manager, "1 year", false, DateTimeOffset.Now.AddYears(1));
			});
		}

		void CheckIsAboutToExpire(ICertificateManager manager, string message, bool expected, DateTimeOffset notAfter, DateTimeOffset? notBefore = null)
		{
			notBefore ??= DateTimeOffset.Now;
			var certificate = CreateCertificateData(notBefore.Value, notAfter);

			AssertEquals(message, expected, manager.IsAboutToExpire(certificate));
		}

		public void TestIsIsExpired_WithCertificateIsExpired()
		{
			var manager = CreateCertificateManager();

			CombineAssertions(() =>
			{
				CheckIsExpired(manager, "last month", true, DateTimeOffset.Now.AddMonths(-1), DateTimeOffset.Now.AddMonths(-2));
				CheckIsExpired(manager, "1 day", false, DateTimeOffset.Now.AddDays(1));
				CheckIsExpired(manager, "1 month", false, DateTimeOffset.Now.AddMonths(1));
				CheckIsExpired(manager, "1 year", false, DateTimeOffset.Now.AddYears(1));
			});
		}

		void CheckIsExpired(ICertificateManager manager, string message, bool expected, DateTimeOffset notAfter, DateTimeOffset? notBefore = null)
		{
			notBefore ??= DateTimeOffset.Now;
			var certificate = CreateCertificateData(notBefore.Value, notAfter);

			AssertEquals(message, expected, manager.IsExpired(certificate));
		}

		public void TestRequestCertificate_ParametersPassedToSystemToSystemTrustApi()
		{
			var csr = "test";
			var capturedCsr = string.Empty;

			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Callback((string url, StringContent content, string token) =>
				{
					var secureQueryString = new SecureQueryString(content.ReadAsStringAsync().Result);
					var requestInfo = JsonConvert.DeserializeObject<IdentityCertificateInitialRequest>(secureQueryString["IdentityCertificateInitialRequest"]);
					capturedCsr = requestInfo.CertificateSignRequest;
				})
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, string.Empty));

			var manager = CreateCertificateManager();

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act
				var result = manager.RequestCertificate(csr);

				// Assert
				AssertEquals(string.Empty, result);
				AssertEquals(csr, capturedCsr);
			}
		}

		public void TestRequestCertificate_WithCsr_ReturnsResponse()
		{
			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, string.Empty));

			var manager = CreateCertificateManager();

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act
				var result = manager.RequestCertificate("test");

				// Assert
				AssertEquals(string.Empty, result);
			}
		}

		public void TestRequestCertificate_WithNullCsr_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.RequestCertificate(null));
		}

		public void TestRequestCertificate_WithFailedRequest_ThrowsException()
		{
			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, string.Empty));

			var manager = CreateCertificateManager();

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act, Assert
				AssertExceptionThrown<CertificateManagementException>(() => manager.RequestCertificate("test"));
			}
		}

		public void TestRequestCertificate_WithInvalidProductRegistration_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			using (ObjectFactory.Substitute(GetMockProductRegistration(0)))
			{
				AssertExceptionThrown<InvalidOperationException>(() => manager.RequestCertificate("test"));
			}
		}

		public void TestRequestCertificate_WithApiException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act, Assert
				var result = AssertExceptionThrown<CertificateManagementException>(() => manager.RequestCertificate("test"));
				AssertType<SystemToSystemTrustCertificateManagementException>(result.InnerException);
			}
		}

		public void TestRegisterCertificate_ParametersPassedToSystemToSystemTrustApi()
		{
			var csr = "test";
			var capturedCsr = string.Empty;

			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Callback((string url, StringContent content, string token) =>
				{
					var queryString = content.ReadAsStringAsync().Result;
					var requestInfo = JsonConvert.DeserializeObject<IdentityCertificateRegisterRequest>(queryString);
					capturedCsr = requestInfo.CertificateSignRequest;
				})
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			var manager = CreateCertificateManager();
			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act
				var result = manager.RegisterCertificate(csr, "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor);

				// Assert
				AssertEquals(string.Empty, result);
				AssertEquals(csr, capturedCsr);
			}
		}

		public void TestRegisterCertificate_WithCsr_ReturnsResponse()
		{
			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			var manager = CreateCertificateManager();

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act
				var result = manager.RegisterCertificate("test", "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor);

				// Assert
				AssertEquals(string.Empty, result);
			}
		}

		public void TestRegisterCertificate_WithNullCsr_ThrowsException()
		{
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.RegisterCertificate(null, "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor));
		}

		public void TestRegisterCertificate_WithFailedRequest_ThrowsException()
		{
			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			var manager = CreateCertificateManager();

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act, Assert
				AssertExceptionThrown<CertificateManagementException>(() => manager.RegisterCertificate("test", "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor));
			}
		}

		public void TestRegisterCertificate_WithInvalidProductRegistration_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			using (ObjectFactory.Substitute(GetMockProductRegistration(0)))
			{
				AssertExceptionThrown<InvalidOperationException>(() => manager.RegisterCertificate("test", "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor));
			}
		}

		public void TestRegisterCertificate_WithApiException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act, Assert
				var result = AssertExceptionThrown<CertificateManagementException>(() => manager.RegisterCertificate("test", "module", "applicationDescription", CARootCodeDescriptionList.Codes.Adaptor));
				AssertType<SystemToSystemTrustCertificateManagementException>(result.InnerException);
			}
		}

		public void TestRolloverCertificate_WithNullClientId_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.RolloverCertificate(null, "csr"));
		}

		public void TestRolloverCertificate_WithNullCsr_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.RolloverCertificate("clientId", null));
		}

		public void TestRolloverCertificate_WithTokenException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();
			var testException = new InvalidOperationException("Failure for unit test");
			authenticationService.Setup(o => o.GetAccessToken()).Throws(testException);

			// Act
			var e = AssertExceptionThrown<InvalidOperationException>(() => manager.RolloverCertificate("clientId", "csr"));

			// Assert
			AssertEquals(testException, e);
		}

		public void TestRolloverCertificate_WithFailedRequest_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act, Assert
			AssertExceptionThrown<CertificateManagementException>(() => manager.RolloverCertificate("clientId", "csr"));
		}

		public void TestRolloverCertificate_WithApiException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act, Assert
			var result = AssertExceptionThrown<CertificateManagementException>(() => manager.RolloverCertificate("clientId", "csr"));
			AssertType<SystemToSystemTrustCertificateManagementException>(result.InnerException);
		}

		public void TestRolloverCertificate_WithSuccessfulRequest_ReturnsResponse()
		{
			// Arrange
			var manager = CreateCertificateManager();
			var expectedResult = "operationId";
			var requestBody = string.Empty;

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Callback((string _, StringContent content, string _) => requestBody = content.ReadAsStringAsync().Result)
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, expectedResult));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act
			var result = manager.RolloverCertificate("clientId", "csr", "dummy-root");

			// Assert
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty("Request body of rollover request should not be empty", requestBody);
				AssertEquals("Body of rollover request should have a valid caRootType attribute value", "dummy-root", JsonConvert.DeserializeObject<Dictionary<string, string>>(requestBody)["caRootType"]);
				AssertEquals(expectedResult, result);
			});
		}

		public void TestDownloadCertificate_WithNullOperationId_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.DownloadCertificate(null));
		}

		public void TestDownloadCertificate_WithFailedRequest_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, string.Empty));

			// Act, Assert
			AssertExceptionThrown<CertificateManagementException>(() => manager.DownloadCertificate("operationId"));
		}

		public void TestDownloadCertificate_WithApiException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));

			// Act, Assert
			var result = AssertExceptionThrown<CertificateManagementException>(() => manager.DownloadCertificate("operationId"));
			AssertType<SystemToSystemTrustCertificateManagementException>(result.InnerException);
		}

		public void TestDownloadCertificate_WithSuccessfulRequest_ReturnsResponse()
		{
			// Arrange
			var manager = CreateCertificateManager();
			var response = new IdentityCertificateResponse
			{
				ClientId = nameof(IdentityCertificateResponse.ClientId),
				Status = nameof(IdentityCertificateResponse.Status),
				TenantId = nameof(IdentityCertificateResponse.TenantId),
				CertificateData = Encoding.UTF8.GetBytes(nameof(IdentityCertificateResponse.CertificateData)),
				StatusCode = nameof(IdentityCertificateResponse.StatusCode)
			};

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(response)));

			// Act
			var result = manager.DownloadCertificate("operationId");

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(response.TenantId, result.TenantId);
				AssertEquals(response.ClientId, result.ClientId);
				AssertEquals(response.CertificateData, result.Certificate);
			});
		}

		public void TestDownloadCertificates_WithNullOperationId_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			// Act, Assert
			AssertExceptionThrown<ArgumentNullException>(() => manager.DownloadCertificates(null));
		}

		public void TestDownloadCertificates_WithFailedRequest_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act, Assert
			AssertExceptionThrown<CertificateManagementException>(() => manager.DownloadCertificates("clientId"));
		}

		public void TestDownloadCertificates_WithApiException_ThrowsException()
		{
			// Arrange
			var manager = CreateCertificateManager();

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act, Assert
			var result = AssertExceptionThrown<CertificateManagementException>(() => manager.DownloadCertificates("clientId"));
			AssertType<SystemToSystemTrustCertificateManagementException>(result.InnerException);
		}

		public void TestDownloadCertificates_WithSuccessfulRequest_ReturnsResponse()
		{
			// Arrange
			var manager = CreateCertificateManager();

			var certificate1 = new IdentityCertificateData
			{
				CertificateData = Encoding.UTF8.GetBytes(nameof(IdentityCertificateResponse.CertificateData))
			};

			var certificate2 = new IdentityCertificateData
			{
				CertificateData = Encoding.UTF8.GetBytes("test")
			};

			var response = new IdentityCertificateResponse
			{
				ClientId = nameof(IdentityCertificateResponse.ClientId),
				Status = nameof(IdentityCertificateResponse.Status),
				TenantId = nameof(IdentityCertificateResponse.TenantId),
				CertificateDataArray = new IdentityCertificateData[] { certificate1, certificate2 },
				StatusCode = nameof(IdentityCertificateResponse.StatusCode)
			};

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiGet(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(response)));
			authenticationService.Setup(o => o.GetAccessToken()).Returns("test token");

			// Act
			var result = manager.DownloadCertificates("clientId");

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(2, result.Count());
				AssertEquals(certificate1.CertificateData, result.First());
				AssertEquals(certificate2.CertificateData, result.Last());
			});
		}

		public void TestLoadClientIdByDatabaseNumberAsync_NoException()
		{
			// Arrange
			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.OK, "QDT"));
			authenticationService.Setup(o => o.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("test token");
			using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));

			var manager = CreateCertificateManager();
			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				// Act
				var result = manager.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token).Result;

				// Assert
				AssertEquals("QDT", result);
			}
			authenticationService.Verify(o => o.GetAccessTokenAsync(cancellationTokenSource.Token), Times.Once);
		}

		public void TestLoadDatabaseNumberByClientId_Exception()
		{
			var manager = CreateCertificateManager();

			var testException = new InvalidOperationException("Failure for unit test");
			authenticationService.Setup(o => o.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ThrowsAsync(testException);
			using var cancellationTokenSource = new CancellationTokenSource(delay: TimeSpan.FromSeconds(10));
			var e = AssertExceptionThrown<InvalidOperationException>(
				"Should throw when GetAccessTokenAsync throws",
				() => manager.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token).GetAwaiter().GetResult());

			AssertEquals(testException, e);

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Returns(new SystemToSystemTrustApiResponse(HttpStatusCode.BadRequest, "QDT"));
			authenticationService.Setup(o => o.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("test token");

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				AssertExceptionThrown<CertificateManagementException>(
					"Should throw CertificateManagementException when response is failed code",
					() => manager.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token).GetAwaiter().GetResult());
			}

			apiHelperMock
				.Setup(o => o.SystemToSystemTrustApiPost(It.IsAny<string>(), It.IsAny<StringContent>(), It.IsAny<string>()))
				.Throws(new SystemToSystemTrustCertificateManagementException(string.Empty));

			using (ObjectFactory.Substitute(GetMockProductRegistration()))
			{
				AssertExceptionThrown<CertificateManagementException>(
					"Should throw CertificateManagementException when MyAccount api return SystemToSystemTrustCertificateManagementException",
					() => manager.LoadDatabaseNumberByClientIdAsync(cancellationTokenSource.Token).GetAwaiter().GetResult());
			}
		}

		static byte[] CreateCertificateData(DateTimeOffset notBefore, DateTimeOffset notAfter)
		{
			using var rsa = RSA.Create(4096);
			var subjectName = Guid.NewGuid().ToString();
			var req = new CertificateRequest($"cn={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			using var cert = req.CreateSelfSigned(notBefore, notAfter);
			return cert.Export(X509ContentType.Cert);
		}

		protected override void SetUp()
		{
			apiHelperMock = new Mock<ISystemToSystemTrustApiHelper>();
			authenticationService = new Mock<IAuthenticationService>();
		}

		CertificateManager CreateCertificateManager()
		{
			return new CertificateManager(apiHelperMock.Object, authenticationService.Object);
		}

		static IProductRegistration GetMockProductRegistration(int databaseNumber = 1, string password = nameof(IProductRegistrationKey.Password))
		{
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationKeyMock.Setup(o => o.DatabaseNumber).Returns(databaseNumber);
			productRegistrationKeyMock.Setup(o => o.Password).Returns(password);

			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(o => o.Key).Returns(productRegistrationKeyMock.Object);

			return productRegistrationMock.Object;
		}

		Mock<ISystemToSystemTrustApiHelper> apiHelperMock;
		Mock<IAuthenticationService> authenticationService;
	}
}

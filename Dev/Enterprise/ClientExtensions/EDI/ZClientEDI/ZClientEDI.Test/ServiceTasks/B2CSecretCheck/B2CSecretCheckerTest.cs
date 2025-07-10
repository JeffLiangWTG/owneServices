using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.ServiceTasks.B2CSecretCheck;
using Enterprise.Registry.Business;
using Microsoft.Graph;
using Moq;
using NUnit.Framework;
using WTG.AzureApplicationIntegration;

namespace ZClientEDI.Test.ServiceTasks.B2CSecretCheck
{
	[TestedType(typeof(B2CSecretChecker))]
	public class B2CSecretCheckerTest : TransactionedTestCase
	{
		public void TestIsCheckerReady_WithoutAllRequiredData()
		{
			bool isReady;
			string notReadyMessage;
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var graphServiceMock = new Mock<IGraphService>();
				b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
				isReady = b2CSecretChecker.IsCheckerReady(out notReadyMessage);
				AssertEquals(false, isReady);

				var expectedMessage = @"The system does not have the private key for the System to System Trust token authentication.
Following registries don't have valid value.
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Tenant ID;
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Client ID;
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Secret Client ID
";
				AssertEquals(expectedMessage, notReadyMessage);
			}

			isReady = b2CSecretChecker.IsCheckerReady(out notReadyMessage);
			AssertEquals(true, isReady);
			AssertEquals(string.Empty, notReadyMessage);
		}

		public void TestIsCheckerReady_WithoutTenantId()
		{
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var graphServiceMock = new Mock<IGraphService>();
				b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
				var isReady = b2CSecretChecker.IsCheckerReady(out var notReadyMessage);
				AssertEquals(false, isReady);

				var expectedMessage = @"Following registries don't have valid value.
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Tenant ID
";
				AssertEquals(expectedMessage, notReadyMessage);
			}
		}

		public void TestIsCheckerReady_WithoutClientId()
		{
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var graphServiceMock = new Mock<IGraphService>();
				b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
				var isReady = b2CSecretChecker.IsCheckerReady(out var notReadyMessage);
				AssertEquals(false, isReady);

				var expectedMessage = @"Following registries don't have valid value.
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Client ID
";
				AssertEquals(expectedMessage, notReadyMessage);
			}
		}

		public void TestIsCheckerReady_WithoutSecretClientId()
		{
			using (EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var graphServiceMock = new Mock<IGraphService>();
				b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
				var isReady = b2CSecretChecker.IsCheckerReady(out var notReadyMessage);
				AssertEquals(false, isReady);

				var expectedMessage = @"Following registries don't have valid value.
WiseTech Global Client Extensions -> B2C Secret Check Management -> B2C Secret Check Management Secret Client ID
";
				AssertEquals(expectedMessage, notReadyMessage);
			}
		}

		public void TestIsCheckerReady_WithoutPrivateKey()
		{
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var graphServiceMock = new Mock<IGraphService>();
				b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
				var isReady = b2CSecretChecker.IsCheckerReady(out var notReadyMessage);
				AssertEquals(false, isReady);

				var expectedMessage = @"The system does not have the private key for the System to System Trust token authentication.
";
				AssertEquals(expectedMessage, notReadyMessage);
			}
		}

		public void TestGetExpiryDateWithAuthenticationFailedException()
		{
			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.GetMaxExpirationDateAsync(It.IsAny<string>())).Throws(new InvalidOperationException("Invalid Operation"));
			b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
			var exception = AssertExceptionThrown<InvalidOperationException>(() => b2CSecretChecker.GetExpiryDate());
			graphServiceMock.Verify(m => m.GetMaxExpirationDateAsync(EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.Value), Times.Once);
			AssertEquals("Invalid Operation", exception.Message);
		}

		public void TestGetExpiryDateWithEmptySecrets()
		{
			applicationContainingSecret.PasswordCredentials = new List<PasswordCredential>();
			var graphServiceMock = new Mock<IGraphService>();
			b2CSecretChecker = new B2CSecretChecker(graphServiceMock.Object);
			var expiryDate = b2CSecretChecker.GetExpiryDate();
			AssertEquals(DateTime.MinValue, expiryDate);
		}

		public void TestGetExpiryDate()
		{
			var expectedExpiryDate = DateTime.UtcNow.AddMonths(5);
			var secretClientId = Guid.NewGuid().ToString();
			using var tempOverride = EDIDataRegistry.Instance.B2CSecretCheckManagementSecretClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, secretClientId);
			var mockGraphService = new Mock<IGraphService>();
			mockGraphService.Setup(m => m.GetMaxExpirationDateAsync(It.IsAny<string>())).ReturnsAsync(expectedExpiryDate);
			b2CSecretChecker = new B2CSecretChecker(mockGraphService.Object);

			var expiryDate = b2CSecretChecker.GetExpiryDate();

			AssertEquals(expectedExpiryDate, expiryDate);
			mockGraphService.Verify(m => m.GetMaxExpirationDateAsync(secretClientId), Times.Once);
		}

		protected override void SetUp()
		{
			applicationContainingSecret = new Application() { Id = Guid.NewGuid().ToString() };

			var systemToSystemInfo = new SystemToSystemTrustInfo
			{
				Certificate = ZBlob.FromUTF8("dummy certificate")
			};

			systemToSystemCertificateSetTemporaryValue = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemInfo);
		}

		protected override void TearDown()
		{
			systemToSystemCertificateSetTemporaryValue.Dispose();
		}

		IDisposable systemToSystemCertificateSetTemporaryValue;

		B2CSecretChecker b2CSecretChecker;

		static Application applicationContainingSecret;
	}
}

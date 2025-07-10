using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Graph;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.AzureApplicationIntegration;
using ZClientEDI.Test.ServiceTasks.AzureApplicationProcessing.Test;

namespace ZClientEDI.Test.ServiceTasks.ApplicationProcessing.Test
{
	[TestedType(typeof(ApplicationProcessingServiceTask))]
	public class ApplicationProcessingServiceTaskTest : ServiceTaskTestCase<ApplicationProcessingServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTaskCode()
		{
			AssertEquals("AAP", ApplicationProcessingServiceTask.Code);
		}

		public void TestRunServiceTask_EmptyTenantId()
		{
			var application = CreateApplication();

			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var expectedMessage = @"Warning|Identity Application Processing Service Task cannot run because the following registries don't have valid value.
WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Tenant ID

";
			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals(log.ToString(), expectedMessage);
			}
		}

		public void TestRunServiceTask_EmptyClientId()
		{
			var application = CreateApplication();

			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var expectedMessage = @"Warning|Identity Application Processing Service Task cannot run because the following registries don't have valid value.
WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Client ID

";
			using (EDIDataRegistry.Instance.AzureApplicationManagementClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals(log.ToString(), expectedMessage);
			}
		}

		public void TestRunServiceTask_EmptyCertificate()
		{
			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var expectedMessage = @"Warning|Identity Application Processing Service Task cannot run because the following registries don't have valid value.
System -> System-to-System Trust -> System to System Certificate (hidden registry item)
";
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals(log.ToString(), expectedMessage);
			}
		}

		public void TestRunServiceTask_EmptyRegistries()
		{
			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var expectedMessage = @"Warning|Identity Application Processing Service Task cannot run because the following registries don't have valid value.
WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Tenant ID
WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Client ID
System -> System-to-System Trust -> System to System Certificate (hidden registry item)
";
			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (EDIDataRegistry.Instance.AzureApplicationManagementClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);
				AssertEquals(log.ToString(), expectedMessage);
			}
		}

		public void TestRunTask_RollbackApplicationWhenLicenseIsInactive()
		{
			var licenseEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var license = Factory.NewWithValidTestData<LicenceDatabase>();
			license.LD_IsActive = false;
			license.LD_LE = licenseEnterprise.PK;

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = Guid.NewGuid().ToString();
			application.IDA_LD = license.PK;
			application.Tenant.IDT_Name = "TestTenant";
			application.IDA_ApplicationName = "TestApp";

			var attachedCertificate = application.Certificates.AddNew();
			attachedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			attachedCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(30);
			attachedCertificate.ICE_CertificateThumbprint = "Thumbprint";
			attachedCertificate.ICE_ProcessingStatus = "COM";

			var redirectUrlWeb = application.RedirectUrls.AddNew();
			redirectUrlWeb.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrlWeb.IAR_RedirectUrl = "https://redirect.web.com";

			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			application.ReloadSafe();
			var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Removed 1 certificate(s) from Azure Application 'TestApp' on the tenant 'TestTenant'
Information|Synchronized redirect URLs for Azure Application 'TestApp' onto the tenant 'TestTenant'
Information|Rolled back Application 'TestApp' from the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
			CombineAssertions(() =>
			{
				graphServiceMock.Verify(g =>
						g.OverrideRedirectUrlsAsync(application.IDA_ClientID.ToString(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
						Times.Exactly(1));

				AssertEquals("IDA_IsRollback", true, application.IDA_IsRollback);
				AssertEquals("IDA_IsActive", false, application.IDA_IsActive);
				AssertEquals(expectedMessage, log.ToString());
			});
		}

		public void TestRunTask_RollbackApplicationWhenClientIdIsEmpty()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZString.Empty;
			application.IDA_ApplicationName = "TestApp";
			application.IDA_IsRollback = true;
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();

			var azureApplicationManagementCreator = new Mock<IAzureApplicationManagementCreator>();
			azureApplicationManagementCreator
				.Setup(a => a.CreateAzureApplicationManagement(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new AzureApplicationManagementForTest(graphServiceMock.Object));

			var serviceTask = CreateServiceTask(graphServiceMock.Object);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			application.ReloadSafe();
			var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
			CombineAssertions(() =>
			{
				AssertEquals("IDA_IsRollback", true, application.IDA_IsRollback);
				AssertEquals("IDA_IsActive", false, application.IDA_IsActive);
				AssertEquals(expectedMessage, log.ToString());
			});
		}

		public void TestRunTask_ReactivateApplicationWhenLicenseIsActive()
		{
			var licenseEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var license = Factory.NewWithValidTestData<LicenceDatabase>();
			license.LD_LE = licenseEnterprise.PK;

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = Guid.NewGuid().ToString();
			application.IDA_LD = license.PK;
			application.IDA_ApplicationName = "TestApp";
			application.Tenant.IDT_Name = "TestTenant";
			application.IDA_IsRollback = true;
			application.IDA_IsActive = false;
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var log = InitialiseAndRunTaskSchedule(serviceTask);

			application.ReloadSafe();
			var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Activate Application 'TestApp' on the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
			CombineAssertions(() =>
			{
				AssertEquals("IDA_IsRollback", false, application.IDA_IsRollback);
				AssertEquals("IDA_IsActive", true, application.IDA_IsActive);
				AssertEquals(expectedMessage, log.ToString());
			});
		}

		public void TestRunTask_ProcessApplicationFailed()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "E000001.CW1.FZ";
			application.IDA_ClientID = ZString.Empty;
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock
				.Setup(g => g.CreateApplicationAsync(It.IsAny<string>()))
				.Throws(new InvalidOperationException("Create application failed"));

			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(g => g.CreateApplicationAsync(application.IDA_ApplicationName), Times.Exactly(3));

				var logString = log.ToString();

				var expectedLog = "Failed to process application for {0}\r\nEnterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing.AzureApplicationManagementException: Create Application Error";

				var expectedLog2 = @"Information|Processed 1 application(s). 0 was successful and 1 failed";

				application.ReloadSafe();

				var emails = Env.OutgoingMailManager.EmailsCreated;

				CombineAssertions(() =>
				{
					AssertContains("Information|Identity Application Processing Service Task started", logString);
					AssertContains(string.Format(expectedLog, application.IDA_ApplicationName), logString);

					AssertContains(expectedLog2, logString);
					AssertContains("System.InvalidOperationException: Create application failed", logString);
					AssertNullOrEmpty(application.IDA_ClientID);

					AssertEquals(1, emails.Count);
					AssertEquals("FAL", application.IDA_ProcessingStatus);
					Assert("Subject", emails.All(x => x.Subject == "Identity Application Processing Service Task Process Application Error"));
					Assert("Body contains error", emails.All(x => x.Body.Contains("System.InvalidOperationException: Create application failed")));
					Assert("Body contains application name", emails.Any(x => x.Body.Contains(string.Format(expectedLog, application.IDA_ApplicationName))));
				});
			}
		}

		public void TestRunTask_CreateApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "E000001.CW1.FZ";
			application.Tenant.IDT_Name = "TestTenant";
			application.IDA_ClientID = ZString.Empty;
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.CreateApplicationAsync(It.IsAny<string>())).Returns(Task.FromResult(AppId));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(g => g.CreateApplicationAsync(application.IDA_ApplicationName), Times.Exactly(1));

				var expectedMessage = $@"Information|Identity Application Processing Service Task started
Information|Created the Azure Application 'E000001.CW1.FZ' with ClientId '{AppId}' on the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				application.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals(expectedMessage, log.ToString());
					AssertEquals(AppId, application.IDA_ClientID);
				});
			}
		}

		public void TestRunTask_DoNotProcessFailedApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZString.Empty;
			application.IDA_ProcessingStatus = "FAL";
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Processed 0 application(s). 0 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				CombineAssertions(() =>
				{
					AssertEquals(expectedMessage, log.ToString());
					graphServiceMock.VerifyNoOtherCalls();
				});
			}
		}

		public void TestRunTask_RemoveRevokedCertificates()
		{
			var application = CreateApplication();
			application.IDA_ClientID = "Client Id";

			var certificateWithThumbprint = application.Certificates.AddNew();
			certificateWithThumbprint.ICE_ProcessingStatus = "CAN";
			certificateWithThumbprint.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			certificateWithThumbprint.ICE_CertificateThumbprint = "Thumbprint";
			certificateWithThumbprint.ICE_IsCertificateRevoked = false;

			var certificateWithoutThumbprint = application.Certificates.AddNew();
			certificateWithoutThumbprint.ICE_ProcessingStatus = "CAN";
			certificateWithoutThumbprint.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);

			var revokedCertificate = application.Certificates.AddNew();
			revokedCertificate.ICE_ProcessingStatus = "CAN";
			revokedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			revokedCertificate.ICE_CertificateThumbprint = "Thumbprint";
			revokedCertificate.ICE_IsCertificateRevoked = true;

			var inactiveCertificate = application.Certificates.AddNew();
			inactiveCertificate.ICE_ProcessingStatus = "CAN";
			inactiveCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			inactiveCertificate.ICE_CertificateThumbprint = "Thumbprint";
			inactiveCertificate.ICE_IsActive = false;

			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Removed 1 certificate(s) from Azure Application 'E000001.CW1.FZ' on the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());
				graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync(application.IDA_ClientID, It.IsAny<string[]>()), Times.Exactly(1));

				certificateWithThumbprint.ReloadSafe();
				certificateWithoutThumbprint.ReloadSafe();
				revokedCertificate.ReloadSafe();
				inactiveCertificate.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals("certificateWithThumbprint", true, certificateWithThumbprint.ICE_IsActive);
					AssertEquals("certificateWithoutThumbprint", true, certificateWithoutThumbprint.ICE_IsActive);
					AssertEquals("revokedCertificate", false, revokedCertificate.ICE_IsActive);
					AssertEquals("inactiveCertificate", false, inactiveCertificate.ICE_IsActive);
				});
			}
		}

		public void TestProcessingApplications_ManageCertificates()
		{
			var application = CreateApplication();
			application.IDA_ClientID = "Client Id";

			var issuedCertificate = application.Certificates.AddNew();
			issuedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			issuedCertificate.ICE_CertificateThumbprint = "Thumbprint";
			issuedCertificate.ICE_ProcessingStatus = "PRC";

			var notIssuedYetCertificate = application.Certificates.AddNew();
			notIssuedYetCertificate.ICE_ProcessingStatus = "QUE";

			var revokedCertificate = application.Certificates.AddNew();
			revokedCertificate.ICE_ProcessingStatus = "CAN";
			revokedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			revokedCertificate.ICE_CertificateThumbprint = "Thumbprint";
			revokedCertificate.ICE_IsCertificateRevoked = true;

			var expiredCertificate = application.Certificates.AddNew();
			expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			expiredCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			expiredCertificate.ICE_CertificateThumbprint = "Thumbprint";
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()));
			graphServiceMock.Setup(a => a.AddCertificatesToApplicationAsync(It.IsAny<string>(), It.IsAny<X509Certificate2[]>()));

			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(g => g.AddCertificatesToApplicationAsync(application.IDA_ClientID, It.IsAny<X509Certificate2[]>()), Times.Exactly(1));
				graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync(application.IDA_ClientID, It.IsAny<string[]>()), Times.Exactly(1));

				var expectedMessage = @$"Information|Identity Application Processing Service Task started
Information|Added 1 certificate(s) to Azure Application 'E000001.CW1.FZ' on the tenant 'TestTenant'
Information|Removed 2 certificate(s) from Azure Application 'E000001.CW1.FZ' on the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());

				issuedCertificate.ReloadSafe();
				notIssuedYetCertificate.ReloadSafe();
				revokedCertificate.ReloadSafe();
				expiredCertificate.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals("COM", issuedCertificate.ICE_ProcessingStatus);
					AssertEquals("QUE", notIssuedYetCertificate.ICE_ProcessingStatus);
					AssertEquals("CAN", revokedCertificate.ICE_ProcessingStatus);
					AssertEquals(false, revokedCertificate.ICE_IsActive);
					AssertEquals(false, expiredCertificate.ICE_IsActive);
				});
			}
		}

		public void TestProcessApplication_Rollback()
		{
			var application = CreateApplication();
			application.IDA_ClientID = "Client Id";
			application.IDA_IsRollback = true;

			var issuedCertificate = application.Certificates.AddNew();
			issuedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			issuedCertificate.ICE_CertificateThumbprint = "Thumbprint1";
			issuedCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(30);
			issuedCertificate.ICE_ProcessingStatus = "PRC";

			var notIssuedYetCertificate = application.Certificates.AddNew();
			notIssuedYetCertificate.ICE_ProcessingStatus = "QUE";

			var revokedCertificate = application.Certificates.AddNew();
			revokedCertificate.ICE_ProcessingStatus = "CAN";
			revokedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			revokedCertificate.ICE_CertificateThumbprint = "Thumbprint2";
			revokedCertificate.ICE_IsCertificateRevoked = true;
			revokedCertificate.ICE_IsActive = false;

			var expiredCertificate = application.Certificates.AddNew();
			expiredCertificate.ICE_ProcessingStatus = "COM";
			expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			expiredCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			expiredCertificate.ICE_CertificateThumbprint = "Thumbprint3";

			var attachedCertificate = application.Certificates.AddNew();
			attachedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			attachedCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(30);
			attachedCertificate.ICE_CertificateThumbprint = "Thumbprint4";
			attachedCertificate.ICE_ProcessingStatus = "COM";

			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()));
			graphServiceMock.Setup(a => a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()));

			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(g => g.RemoveCertificatesFromApplicationAsync(application.IDA_ClientID, It.IsAny<string[]>()), Times.Exactly(1));
				graphServiceMock.Verify(g => g.OverrideRedirectUrlsAsync(application.IDA_ClientID, It.IsAny<string[]>(),
					It.IsAny<string[]>(), It.IsAny<string[]>()), Times.Exactly(1));

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Removed 3 certificate(s) from Azure Application 'E000001.CW1.FZ' on the tenant 'TestTenant'
Information|Synchronized redirect URLs for Azure Application 'E000001.CW1.FZ' onto the tenant 'TestTenant'
Information|Rolled back Application 'E000001.CW1.FZ' from the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());

				issuedCertificate.ReloadSafe();
				notIssuedYetCertificate.ReloadSafe();
				revokedCertificate.ReloadSafe();
				expiredCertificate.ReloadSafe();
				attachedCertificate.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals("Issued certificate status", "CAN", issuedCertificate.ICE_ProcessingStatus);
					AssertEquals("Issued certificate active", false, issuedCertificate.ICE_IsActive);

					AssertEquals("Revoked certificate status", "CAN", revokedCertificate.ICE_ProcessingStatus);
					AssertEquals("Revoked certificate active", false, revokedCertificate.ICE_IsActive);

					AssertEquals("Expired certificate status", "COM", expiredCertificate.ICE_ProcessingStatus);
					AssertEquals("Expired certificate active", false, expiredCertificate.ICE_IsActive);

					AssertEquals("Not issued yet certificate status", "CAN", notIssuedYetCertificate.ICE_ProcessingStatus);
					AssertEquals("Not issued yet certificate active", false, notIssuedYetCertificate.ICE_IsActive);

					AssertEquals("Attached certificate status", "CAN", attachedCertificate.ICE_ProcessingStatus);
					AssertEquals("Attached certificate active", false, attachedCertificate.ICE_IsActive);
				});
			}
		}

		[TestDate(2024, 5, 1, 12, 0, 0)]
		public void TestGithubActionSecretCheck()
		{
			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(-1).ToDateTime()))
			{
				var application = new Application
				{
					PasswordCredentials = new List<PasswordCredential> { new() { EndDateTime = ZDateTime.UtcNow.AddMonths(2).ToDateTime() } }
				};

				var graphServiceMock = new Mock<IGraphService>();
				graphServiceMock.Setup(a => a.GetMaxExpirationDateAsync(It.IsAny<string>())).Returns(Task.FromResult(application.PasswordCredentials.First().EndDateTime));

				var serviceTask = CreateServiceTask(graphServiceMock.Object);

				var log = InitialiseAndRunTaskSchedule(serviceTask);

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Processed 0 application(s). 0 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());

				var nextRunDate = EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.Value;
				AssertEquals("Next run date is updated.", ZDateTime.UtcNow.AddDays(7), nextRunDate);
			}
		}

		[TestDate(2024, 11, 1, 12, 0, 0)]
		public void TestGithubActionSecretCheck_ExpiredDate()
		{
			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var application = new Application()
				{
					PasswordCredentials = new List<PasswordCredential> { new() { EndDateTime = ZDateTime.UtcNow.AddDays(20).ToDateTime() } }
				};

				var graphServiceMock = new Mock<IGraphService>();
				graphServiceMock.Setup(a => a.GetMaxExpirationDateAsync(It.IsAny<string>())).Returns(Task.FromResult(application.PasswordCredentials.First().EndDateTime));
				var serviceTask = CreateServiceTask(graphServiceMock.Object);

				InitialiseAndRunTaskSchedule(serviceTask);

				AssertEquals("Should be one notification email created.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Github Action Secret Notification", Env.OutgoingMailManager.EmailsCreated.First().Subject);
				AssertEquals("Github Action Secret is about to expire or has already expired, please renew it. Expiration date: 21/11/2024", Env.OutgoingMailManager.EmailsCreated.First().Body);
				AssertEquals("Next run date should be updated.", ZDateTime.UtcNow.AddDays(7), EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.Value);
			}
		}

		public void TestNudgedApplications()
		{
			var clientId = Guid.NewGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestApp";
			application.IDA_ClientID = clientId;
			application.Tenant.IDT_Name = "TestTenant";
			application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged;

			var redirectUrlWeb = application.RedirectUrls.AddNew();
			redirectUrlWeb.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrlWeb.IAR_RedirectUrl = "https://redirect.web.com";

			var redirectUrlSinglePage = application.RedirectUrls.AddNew();
			redirectUrlSinglePage.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			redirectUrlSinglePage.IAR_RedirectUrl = "https://redirect.spa.com";

			var redirectUrlInstalledClient = application.RedirectUrls.AddNew();
			redirectUrlInstalledClient.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			redirectUrlInstalledClient.IAR_RedirectUrl = "https://redirect.installed.com";
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			var log = InitialiseAndRunTaskSchedule(serviceTask);

			graphServiceMock.Verify(g => g.OverrideRedirectUrlsAsync(clientId, new[] { "https://redirect.web.com" }, new[] { "https://redirect.spa.com" }, new[] { "https://redirect.installed.com" }), Times.Exactly(1));
			var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Synchronized redirect URLs for Azure Application 'TestApp' onto the tenant 'TestTenant'
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
			application.Reload();

			CombineAssertions(() =>
			{
				AssertEquals(expectedMessage, log.ToString());
				AssertEquals("The status is updated to scheduled", EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled, application.IDA_RedirectUrlStatus);
				AssertCloseEnough("The last sync date is updated to now", DateTime.UtcNow, application.IDA_RedirectUrlLastSyncTimeUtc.ToDateTime(), 10);
			});
		}

		public void TestScheduledApplicationsWithInterval()
		{
			var clientId1 = Guid.NewGuid().ToString();
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_ApplicationName = "TestApp1";
			application1.IDA_ClientID = clientId1;
			application1.IDA_RedirectUrlLastSyncTimeUtc = DateTime.UtcNow.AddDays(-2);

			var redirectUrl1 = application1.RedirectUrls.AddNew();
			redirectUrl1.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrl1.IAR_RedirectUrl = "https://redirect.web.com";

			var clientId2 = Guid.NewGuid().ToString();
			var lastSyncDate = ZDateTime.UtcNow.AddHours(-12);
			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_ApplicationName = "TestApp2";
			application2.IDA_ClientID = clientId2;
			application2.IDA_RedirectUrlLastSyncTimeUtc = lastSyncDate;

			var redirectUrl2 = application2.RedirectUrls.AddNew();
			redirectUrl2.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			redirectUrl2.IAR_RedirectUrl = "https://redirect.spa.com";
			Factory.Save();

			application1.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled;
			application2.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled;
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			InitialiseAndRunTaskSchedule(serviceTask);

			application1.Reload();
			application2.Reload();

			graphServiceMock.Verify(a => a.OverrideRedirectUrlsAsync(clientId1, new[] { "https://redirect.web.com" }, Array.Empty<string>(), Array.Empty<string>()), Times.Once);
			graphServiceMock.Verify(a => a.OverrideRedirectUrlsAsync(clientId2, It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()), Times.Never);

			CombineAssertions(() =>
			{
				AssertCloseEnough("The last sync date is updated to now", DateTime.UtcNow, application1.IDA_RedirectUrlLastSyncTimeUtc.ToDateTime(), 10);

				AssertEquals("The date should not be updated", lastSyncDate, application2.IDA_RedirectUrlLastSyncTimeUtc);
			});
		}

		public void TestHandleConcurrencyException()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_RedirectUrlLastSyncTimeUtc = ZDateTime.Now.AddDays(-10);
			application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled;

			var redirectUrl1 = application.RedirectUrls.AddNew();
			redirectUrl1.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrl1.IAR_RedirectUrl = "https://web.com";
			var redirectUrl2 = application.RedirectUrls.AddNew();
			redirectUrl2.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			redirectUrl2.IAR_RedirectUrl = "https://SinglePage.com";
			var redirectUrl3 = application.RedirectUrls.AddNew();
			redirectUrl3.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			redirectUrl3.IAR_RedirectUrl = "https://InstalledClient.com";
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.OverrideRedirectUrlsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string[]>(), It.IsAny<string[]>()));
			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedApplication = newFactory.Load<EdiIdentityApplication>(application.PK);
				loadedApplication.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged;
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);
				newFactory.Save();
			});

			var serviceTask = CreateServiceTask(graphServiceMock.Object);
			InitialiseAndRunTaskSchedule(serviceTask);

			application.Reload();
			AssertEquals("status should be updated", EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled, application.IDA_RedirectUrlStatus);
			AssertCloseEnough("last sync date should be updated", DateTime.UtcNow, application.IDA_RedirectUrlLastSyncTimeUtc.ToDateTime(), 10);
		}

		public void TestProcessCustomerApplication_ManageCertificates()
		{
			var application = CreateCustomerApplication();

			var issuedCertificate = application.Certificates.AddNew();
			issuedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			issuedCertificate.ICE_CertificateThumbprint = "Thumbprint";
			issuedCertificate.ICE_ProcessingStatus = "PRC";

			var notIssuedYetCertificate = application.Certificates.AddNew();
			notIssuedYetCertificate.ICE_ProcessingStatus = "QUE";

			var revokedCertificate = application.Certificates.AddNew();
			revokedCertificate.ICE_ProcessingStatus = "CAN";
			revokedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			revokedCertificate.ICE_IsCertificateRevoked = true;
			revokedCertificate.ICE_CertificateThumbprint = "Thumbprint";

			var expiredCertificate = application.Certificates.AddNew();
			expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			expiredCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			expiredCertificate.ICE_CertificateThumbprint = "Thumbprint";
			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.AddCertificatesToApplicationAsync(It.IsAny<string>(), It.IsAny<X509Certificate2[]>()));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(a => a.AddCertificatesToApplicationAsync(application.IDA_ClientID, It.IsAny<X509Certificate2[]>()), Times.Never());

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());

				issuedCertificate.ReloadSafe();
				notIssuedYetCertificate.ReloadSafe();
				revokedCertificate.ReloadSafe();
				expiredCertificate.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals("COM", issuedCertificate.ICE_ProcessingStatus);
					AssertEquals("QUE", notIssuedYetCertificate.ICE_ProcessingStatus);
					AssertEquals("CAN", revokedCertificate.ICE_ProcessingStatus);
					AssertEquals(false, revokedCertificate.ICE_IsActive);
					AssertEquals(false, expiredCertificate.ICE_IsActive);
				});
			}
		}

		public void TestProcessCustomerApplication_Rollback()
		{
			var application = CreateCustomerApplication();
			application.IDA_IsRollback = true;

			var issuedCertificate = application.Certificates.AddNew();
			issuedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			issuedCertificate.ICE_CertificateThumbprint = "Thumbprint1";
			issuedCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(30);
			issuedCertificate.ICE_ProcessingStatus = "PRC";

			var notIssuedYetCertificate = application.Certificates.AddNew();
			notIssuedYetCertificate.ICE_ProcessingStatus = "QUE";

			var revokedCertificate = application.Certificates.AddNew();
			revokedCertificate.ICE_ProcessingStatus = "CAN";
			revokedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			revokedCertificate.ICE_CertificateThumbprint = "Thumbprint2";
			revokedCertificate.ICE_IsCertificateRevoked = true;
			revokedCertificate.ICE_IsActive = false;

			var expiredCertificate = application.Certificates.AddNew();
			expiredCertificate.ICE_ProcessingStatus = "COM";
			expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			expiredCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			expiredCertificate.ICE_CertificateThumbprint = "Thumbprint3";

			var attachedCertificate = application.Certificates.AddNew();
			attachedCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			attachedCertificate.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddDays(30);
			attachedCertificate.ICE_CertificateThumbprint = "Thumbprint4";
			attachedCertificate.ICE_ProcessingStatus = "COM";

			Factory.Save();

			var graphServiceMock = new Mock<IGraphService>();
			graphServiceMock.Setup(a => a.RemoveCertificatesFromApplicationAsync(It.IsAny<string>(), It.IsAny<string[]>()));
			var serviceTask = CreateServiceTask(graphServiceMock.Object);

			using (EDIDataRegistry.Instance.GithubActionSecretNextCheckDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddHours(1).ToDateTime()))
			{
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				graphServiceMock.Verify(a => a.AddCertificatesToApplicationAsync(application.IDA_ClientID, It.IsAny<X509Certificate2[]>()), Times.Never());

				var expectedMessage = @"Information|Identity Application Processing Service Task started
Information|Processed 1 application(s). 1 was successful and 0 failed
Information|Checked the secret expiry date for Github Action
Information|Identity Application Processing Service Task finished
";
				AssertEquals(expectedMessage, log.ToString());

				issuedCertificate.ReloadSafe();
				notIssuedYetCertificate.ReloadSafe();
				revokedCertificate.ReloadSafe();
				expiredCertificate.ReloadSafe();
				attachedCertificate.ReloadSafe();

				CombineAssertions(() =>
				{
					AssertEquals("Issued certificate status", "CAN", issuedCertificate.ICE_ProcessingStatus);
					AssertEquals("Issued certificate active", false, issuedCertificate.ICE_IsActive);

					AssertEquals("Cancelled certificate status", "CAN", revokedCertificate.ICE_ProcessingStatus);
					AssertEquals("Cancelled certificate active", false, revokedCertificate.ICE_IsActive);

					AssertEquals("Expired certificate status", "COM", expiredCertificate.ICE_ProcessingStatus);
					AssertEquals("Expired certificate active", false, expiredCertificate.ICE_IsActive);

					AssertEquals("Not issued yet certificate status", "CAN", notIssuedYetCertificate.ICE_ProcessingStatus);
					AssertEquals("Not issued yet certificate active", false, notIssuedYetCertificate.ICE_IsActive);

					AssertEquals("Attached certificate status", "CAN", attachedCertificate.ICE_ProcessingStatus);
					AssertEquals("Attached certificate active", false, attachedCertificate.ICE_IsActive);
				});
			}
		}

		protected override void SetUpCore()
		{
			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			certProcessingNotificationGroupOverride = EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			azureApplicationManagementTenantIDOverride = EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Graph Tenant Id");
			azureApplicationManagementClientIDOverride = EDIDataRegistry.Instance.AzureApplicationManagementClientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Graph Client Id");

			var systemToSystemTrustInfo = new SystemToSystemTrustInfo
			{
				Certificate = Encoding.UTF8.GetBytes(Certificate),
			};

			systemToSystemCertificateOverride = SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, systemToSystemTrustInfo);
		}

		protected override void TearDownCore()
		{
			certProcessingNotificationGroupOverride.Dispose();
			azureApplicationManagementTenantIDOverride.Dispose();
			azureApplicationManagementClientIDOverride.Dispose();
			systemToSystemCertificateOverride.Dispose();
		}

		EdiIdentityApplication CreateApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.Tenant.IDT_Name = "TestTenant";
			application.IDA_ApplicationName = "E000001.CW1.FZ";
			Factory.Save();
			return application;
		}

		EdiIdentityApplication CreateCustomerApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "CW1Application";
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			var customerApplication = Factory.New<EdiIdentityApplication>();
			customerApplication.IDA_ApplicationName = "CustomerApp1";
			customerApplication.IDA_IDA_ParentApplication = application.PK;
			customerApplication.IDA_ClientID = ZGuid.NewZGuid().ToString();
			Factory.Save();
			return customerApplication;
		}

		ApplicationProcessingServiceTask CreateServiceTask(IGraphService graphService)
		{
			var logTest = new TestServiceLogger();

			var azureApplicationManagementCreator = new Mock<IAzureApplicationManagementCreator>();
			azureApplicationManagementCreator
				.Setup(a => a.CreateAzureApplicationManagement(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new AzureApplicationManagementForTest(graphService));

			var serviceTask = new ApplicationProcessingServiceTask(azureApplicationManagementCreator.Object, logTest);
			return serviceTask;
		}

		IDisposable certProcessingNotificationGroupOverride;
		IDisposable azureApplicationManagementTenantIDOverride;
		IDisposable azureApplicationManagementClientIDOverride;
		IDisposable systemToSystemCertificateOverride;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				EdiIdentityApplicationSchema.Constants.TableName,
				"Azure Application Initialization Queue",
				EdiIdentityApplicationSchema.Constants.IDA_ClientID + "="
			),
			new TaskNudgeInformationForTest(
			EdiIdentityApplicationSchema.Constants.TableName,
			"Nudged Applications Queue",
			EdiIdentityApplicationSchema.Constants.IDA_RedirectUrlStatus + "=" + EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged
			),
			new TaskNudgeInformationForTest(
			EdiIdentityApplicationSchema.Constants.TableName,
			"Rollback Applications Queue",
			EdiIdentityApplicationSchema.Constants.IDA_IsRollback + "=Y",
			EdiIdentityApplicationSchema.Constants.IDA_IsActive + "=Y"
			),
			new TaskNudgeInformationForTest(
				EdiIdentityCertificateSchema.Constants.TableName,
				"Processing Certificate Queue",
				EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=PRC",
				EdiIdentityCertificateSchema.Constants.ICE_IsActive + "=Y"
			),
			new TaskNudgeInformationForTest(
				EdiIdentityCertificateSchema.Constants.TableName,
				"Revoked Certificate Queue",
				EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=CAN",
				EdiIdentityCertificateSchema.Constants.ICE_IsActive + "=Y"
			),
			new TaskNudgeInformationForTest(
				LicenceDatabaseSchema.Constants.TableName,
				"Applications with Inactive Licenses Queue",
				LicenceDatabaseSchema.Constants.LD_IsActive + "=N",
				LicenceDatabaseSchema.Constants.LD_Product + "=CW1"
			)
		};

		public string AppId = Guid.NewGuid().ToString();

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";
	}
}

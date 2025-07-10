using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Amazon.ACMPCA.Model;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ServiceTasks.EDICertProcessing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.AWSCertificateIntegration;

namespace ZClientEDI.Test.ServiceTasks.EDICertProcessing.Test
{
	[TestedType(typeof(EDICertificateProcessingServiceTask))]
	public class EDICertProcessingServiceTaskTest : ServiceTaskTestCase<EDICertificateProcessingServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("30Minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("30Minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTaskCode()
		{
			AssertEquals("CPS", EDICertificateProcessingServiceTask.Code);
		}

		public void TestMandatoryRegistries()
		{
			awsPrivateCAListManagerOverride.Dispose();

			var serviceTask = new EDICertificateProcessingServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var expectedMessage = "Information|Certificate Processing for System to System Trust Service Task has not been started, please check the registry items under category WiseTech Global Client Extensions/AWS Private CA.\r\n";
			AssertEquals(expectedMessage, log.ToString());
		}

		public void TestRunTask_IssueCertificate()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateSigningRequest = Csr;
			certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;
			Factory.Save();

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			var expectedMessage = @"Information|Certificate Processing for System to System Trust Service Task started
Information|Processed 1 certificate request(s), 1 was successful and 0 failed
Information|Certificate Processing for System to System Trust Service Task finished
";
			AssertEquals(expectedMessage, log.ToString());

			certificate.ReloadSafe();

			CombineAssertions(() =>
			{
				AssertEquals("PRC", certificate.ICE_ProcessingStatus);
				AssertEquals("CN=NJG1", certificate.ICE_CertificateIssuedBy);
				AssertEquals("CN=NJG1", certificate.ICE_CertificateIssuedTo);
				AssertEquals("0153A7C4B112DE00D8E50756C5249D5789C64B03", certificate.ICE_CertificateThumbprint);
				AssertEquals(new DateTime(2024, 2, 24, 3, 31, 0), certificate.ICE_CertificateExpiryDate);
				AssertEquals(new DateTime(2023, 2, 24, 2, 31, 0), certificate.ICE_CertificateValidDate);
			});
		}

		public void TestIssueCertificatesWithDifferentArn()
		{
			var ediCertificate1 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediCertificate1.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			ediCertificate1.Application.IDA_ApplicationName = "TestApp";
			ediCertificate1.ICE_CertificateSigningRequest = Csr;
			ediCertificate1.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;

			var ediCertificate2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediCertificate2.ICE_CARoot = CARootCodeDescriptionList.Codes.Adaptor;
			ediCertificate2.Application.IDA_ApplicationName = "TestApp1";
			ediCertificate2.ICE_CertificateSigningRequest = Csr1;
			ediCertificate2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;
			ediCertificate2.ICE_CARoot = CARootCodeDescriptionList.Codes.Adaptor;

			Factory.Save();

			var logTest = new TestServiceLogger();
			mockAwsPcaManager.Protected()
				.Setup<string>("IssueCertificate", ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None)
				.Returns<IssueCertificateRequest, CancellationToken>((request, cancel) => request.CertificateAuthorityArn);

			mockAwsPcaManager.Protected()
				.Setup<string>("GetCertificate", ItExpr.IsAny<GetCertificateRequest>(), CancellationToken.None)
				.Returns<GetCertificateRequest, CancellationToken>((request, cancel) => request.CertificateArn == Arn ? Certificate : Certificate2);

			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			InitialiseAndRunTaskSchedule(serviceTask);

			ediCertificate1.ReloadSafe();
			ediCertificate2.ReloadSafe();

			var issuedCertificate1 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var issuedCertificate2 = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate2));

			CombineAssertions(() =>
			{
				AssertEquals("PRC", ediCertificate1.ICE_ProcessingStatus);
				AssertEquals(issuedCertificate1.RawData, ediCertificate1.ICE_CertificateData);
				AssertEquals("PRC", ediCertificate2.ICE_ProcessingStatus);
				AssertEquals(issuedCertificate2.RawData, ediCertificate2.ICE_CertificateData);
			});
		}

		public void TestRunTask_IssueCertificateFailed()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateSigningRequest = Csr;
			certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;
			certificate.Application.IDA_ApplicationName = "E000001.CW1.FZ";
			Factory.Save();

			mockAwsPcaManager.Protected()
				.Setup<string>("IssueCertificate", ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None)
				.Throws(new InvalidOperationException("Issue certificate failed"));

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			mockAwsPcaManager.Protected().Verify("IssueCertificate", Times.Exactly(3), ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None);

			var logString = log.ToString();
			certificate.ReloadSafe();
			var emails = Env.OutgoingMailManager.EmailsCreated;
			var queueCertEmail = emails.First(email => email.Subject == "Certificate Processing for System to System Trust Service Task Issue Certificate Error");

			var expectedMessage = "Failed to process certificate for application E000001.CW1.FZ\r\nEnterprise.Client.EDI.ServiceTasks.EDICertProcessing.AwsCertManagementServiceException: Issue Certificate Error";
			CombineAssertions(() =>
			{
				AssertContains(expectedMessage, logString);
				AssertContains("System.InvalidOperationException: Issue certificate failed", logString);
				AssertContains("Information|Processed 1 certificate request(s), 0 was successful and 1 failed", logString);
				AssertEquals("FAL", certificate.ICE_ProcessingStatus);
				Assert(certificate.ICE_CertificateData.IsEmpty);
				AssertEquals(1, emails.Count);
				AssertContains(expectedMessage, queueCertEmail.Body);
				AssertContains("System.InvalidOperationException: Issue certificate failed", queueCertEmail.Body);
			});
		}

		public void TestRunTask_DoesNotProcess_PRC_FAL_COM_EmptyCertificateWithCAN()
		{
			var ediIdentityCertificate1 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificate1.ICE_CertificateSigningRequest = Csr;
			ediIdentityCertificate1.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			ediIdentityCertificate1.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.PRC;

			var ediIdentityCertificate2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificate2.ICE_CertificateSigningRequest = Csr1;
			ediIdentityCertificate2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;

			var ediIdentityCertificate3 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificate3.ICE_CertificateSigningRequest = Csr2;
			ediIdentityCertificate3.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.FAL;

			var ediIdentityCertificate4 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			ediIdentityCertificate4.ICE_CertificateSigningRequest = Csr;
			ediIdentityCertificate4.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			ediIdentityCertificate4.ICE_IsCertificateRevoked = false;

			Factory.Save();

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			mockAwsPcaManager.Protected().Verify("IssueCertificate", Times.Exactly(0), ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None);

			var expectedMessage = @"Information|Certificate Processing for System to System Trust Service Task started
Information|Processed 0 certificate request(s), 0 was successful and 0 failed
Information|Certificate Processing for System to System Trust Service Task finished
";
			ediIdentityCertificate1.ReloadSafe();
			ediIdentityCertificate2.ReloadSafe();
			ediIdentityCertificate3.ReloadSafe();
			ediIdentityCertificate4.ReloadSafe();

			CombineAssertions(() =>
			{
				AssertEquals(expectedMessage, log.ToString());

				AssertEquals("PRC", ediIdentityCertificate1.ICE_ProcessingStatus);
				AssertEquals("COM", ediIdentityCertificate2.ICE_ProcessingStatus);
				AssertEquals("FAL", ediIdentityCertificate3.ICE_ProcessingStatus);
				AssertEquals("CAN", ediIdentityCertificate4.ICE_ProcessingStatus);
				AssertEquals("The certificate wont be processed as it's deactivated", false, ediIdentityCertificate4.ICE_IsCertificateRevoked);
			});
		}

		public void TestRunTask_RevokeCertificate()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_CertificateSigningRequest = Csr;
			certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			certificate.ICE_CertificateThumbprint = "Thumbprint1";
			certificate.ICE_IsActive = false;

			var certificateAlreadyRevoked = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificateAlreadyRevoked.ICE_CertificateSigningRequest = Csr;
			certificateAlreadyRevoked.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			certificateAlreadyRevoked.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificateAlreadyRevoked.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			certificateAlreadyRevoked.ICE_CertificateThumbprint = "Thumbprint2";
			certificateAlreadyRevoked.ICE_IsCertificateRevoked = true;

			var emptyCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			emptyCertificate.ICE_CertificateSigningRequest = Csr;
			emptyCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			emptyCertificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;

			Factory.Save();

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			var expectedMessage = @"Information|Certificate Processing for System to System Trust Service Task started
Information|Processed 1 certificate request(s), 1 was successful and 0 failed
Information|Certificate Processing for System to System Trust Service Task finished
";
			certificate.ReloadSafe();

			CombineAssertions(() =>
			{
				AssertEquals(expectedMessage, log.ToString());
				AssertEquals("CAN", certificate.ICE_ProcessingStatus);
				AssertEquals(true, certificate.ICE_IsCertificateRevoked);
			});
		}

		public void TestRunTask_RevokeCertificateFailed()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateSigningRequest = Csr;
			certificate.Application.IDA_ApplicationName = "E000001.CW1.FZ";
			certificate.ICE_CertificateData = Encoding.UTF8.GetBytes(Certificate);
			certificate.ICE_CertificateThumbprint = "Thumbprint";
			certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			Factory.Save();

			mockAwsPcaManager.Protected()
				.Setup<RevokeCertificateResponse>("RevokeCertificate", ItExpr.IsAny<RevokeCertificateRequest>(), CancellationToken.None)
				.Throws(new Exception());

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask);

			var expectedMessage = "Failed to process certificate for application E000001.CW1.FZ\r\nEnterprise.Client.EDI.ServiceTasks.EDICertProcessing.AwsCertManagementServiceException: Revoke Certificate Error";
			CombineAssertions(() =>
			{
				AssertContains("Information|Processed 1 certificate request(s), 0 was successful and 1 failed", log.ToString());
				AssertContains(expectedMessage, log.ToString());
			});

			certificate.ReloadSafe();

			var emails = Env.OutgoingMailManager.EmailsCreated;
			var cancelCertEmail = emails.First(email => email.Subject == "Certificate Processing for System to System Trust Service Task Revoke Certificate Error");

			CombineAssertions(() =>
			{
				AssertEquals("FAL", certificate.ICE_ProcessingStatus);
				AssertEquals(false, certificate.ICE_IsCertificateRevoked);
				AssertEquals(1, emails.Count);
				AssertContains(expectedMessage, cancelCertEmail.Body);
			});
		}

		public void TestRunTask_Process_FailedWithoutCARoot()
		{
			var certificate1 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate1.ICE_CertificateSigningRequest = Csr;
			certificate1.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;
			certificate1.Application.IDA_ApplicationName = "app1";

			var certificate2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate2.ICE_CertificateSigningRequest = Csr1;
			certificate2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			certificate2.Application.IDA_ApplicationName = "app2";
			certificate2.ICE_CertificateThumbprint = "Thumbprint2";

			Factory.Save();

			var logTest = new TestServiceLogger();
			var serviceTask = new EDICertificateProcessingServiceTask(awsCertManagementService, logTest);
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			var expectedMessage = "Failed to process certificate for application {0}\r\nEnterprise.Client.EDI.ServiceTasks.EDICertProcessing.AwsCertManagementServiceException: Unable to find the corresponding CA";

			CombineAssertions(() =>
			{
				AssertContains(string.Format(expectedMessage, "app1"), log);
				AssertContains(string.Format(expectedMessage, "app2"), log);
				AssertContains("Information|Processed 2 certificate request(s), 0 was successful and 2 failed", log);
			});

			certificate1.ReloadSafe();
			certificate2.ReloadSafe();

			var emails = Env.OutgoingMailManager.EmailsCreated;
			var expectedEmailSubject = "Certificate Processing for System to System Trust Service Task {0} Certificate Error";
			var queueCertEmail = emails.FirstOrDefault(e => e.Subject == string.Format(expectedEmailSubject, "Issue"));
			var canCertEmail = emails.FirstOrDefault(e => e.Subject == string.Format(expectedEmailSubject, "Revoke"));

			CombineAssertions(() =>
			{
				AssertEquals("FAL", certificate1.ICE_ProcessingStatus);
				AssertEquals("FAL", certificate2.ICE_ProcessingStatus);
				Assert(certificate1.ICE_CertificateData.IsEmpty);
				Assert(!certificate2.ICE_IsCertificateRevoked);
				AssertEquals(2, emails.Count);
				AssertContains(string.Format(expectedMessage, "app1"), queueCertEmail.Body);
				AssertContains(string.Format(expectedMessage, "app2"), canCertEmail.Body);
			});
		}

		protected override void SetUpCore()
		{
			var staffToNotification = Factory.NewWithValidTestData<GlbStaff>();
			staffToNotification.GS_EmailAddress = "test@wisetchglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staffToNotification);
			Factory.Save();

			var collection = new AWSPrivateCACollection();
			var awsPrivateCa1 = collection.AddNew();
			awsPrivateCa1.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			awsPrivateCa1.Arn = Arn;
			awsPrivateCa1.IsEnabled = true;
			awsPrivateCa1.AccessKey = AccessKey;
			awsPrivateCa1.SecretKey = SecretKey;

			var awsPrivateCa2 = collection.AddNew();
			awsPrivateCa2.IssuingCA = CARootCodeDescriptionList.Codes.Adaptor;
			awsPrivateCa2.Arn = Arn2;
			awsPrivateCa2.IsEnabled = true;
			awsPrivateCa2.AccessKey = AccessKey;
			awsPrivateCa2.SecretKey = SecretKey;

			certProcessingNotificationGroupOverride = EDIDataRegistry.Instance.CertProcessingNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			awsPrivateCAListManagerOverride = EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var revokeCertificateResponse = new RevokeCertificateResponse()
			{
				ContentLength = 3,
				ResponseMetadata = null,
				HttpStatusCode = HttpStatusCode.OK
			};

			mockAwsPcaManager = new Mock<AwsPcaManager>(AccessKey, SecretKey, "ap-southeast-2");
			mockAwsPcaManager.Protected()
				.Setup<string>("IssueCertificate", ItExpr.IsAny<IssueCertificateRequest>(), CancellationToken.None)
				.Returns(Arn);

			mockAwsPcaManager.Protected()
				.Setup<string>("GetCertificate", ItExpr.IsAny<GetCertificateRequest>(), CancellationToken.None)
				.Returns(Certificate);

			mockAwsPcaManager.Protected()
				.Setup<RevokeCertificateResponse>("RevokeCertificate", ItExpr.IsAny<RevokeCertificateRequest>(), CancellationToken.None)
				.Returns(revokeCertificateResponse);

			var caRootDictionary = new Dictionary<string, (AwsPcaManager mockAwsPcaManager, string ARN)>
			{
				{ CARootCodeDescriptionList.Codes.SystemToSystemTrust, (mockAwsPcaManager.Object, Arn) },
				{ CARootCodeDescriptionList.Codes.Adaptor, (mockAwsPcaManager.Object, Arn2) }
			};

			awsCertManagementService = new AwsCertManagementService(caRootDictionary);
		}

		protected override void TearDownCore()
		{
			certProcessingNotificationGroupOverride.Dispose();
			awsPrivateCAListManagerOverride.Dispose();
		}

		const string AccessKey = "GFDGSDGDFSGDFGDFGDFG";

		const string SecretKey = "pik4+se9HK6aoBDfb4nl1z3S1xqfJ+dSDGSDGfgg";

		const string Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:certificate-authority/84dc84fe-e734-4281-84e3-45fdgfdfg";

		const string Arn2 = "arn:aws:acm-pca:ap-southeast-2:079973481859:certificate-authority/ccc";

		Mock<AwsPcaManager> mockAwsPcaManager;
		AwsCertManagementService awsCertManagementService;

		IDisposable certProcessingNotificationGroupOverride;
		IDisposable awsPrivateCAListManagerOverride;

		const string Csr = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0B
-----END CERTIFICATE REQUEST-----";

		const string Csr1 = @"-----BEGIN CERTIFICATE REQUEST-----
MIICxTCCAa0CAQAwYzELMAkGA1UEBhMCQVUxDDAKBgNVBAgMA1NZRDEMMAoGA1UE
BwwDU1lEMQwwCgYDVQQKDANXVEcxDDAKBgNVBAsMA1dURzEcMBoGA1UEAwwTd2lz
ZXRlY2guZ2xvYmFsLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AOe/ylBndmB622NYlYkl8BglnvFLL8cTz7DQOQ5c4x9aQMujiW+XUgvEjyJ6+cUA
r8H9+tIn9165EgiZLyZcfUYXB4KZ0EHy4rr1SB322UBzES+/jquAD83TV5J1wTEn
VI43PXkgY6cUYQzany8R3CZUsxmWTSQoy545y96rSuVgTYI7u4PmT7nzCgUXwHIj
ZQCUFjCvwixQrbZZR/cLhQUfCGGOqK833ydZ6HHdMJXuZXDYPIoYLBkqKoDHEd9m
VN5fjDRPKg3C4pVdjmltGpaLtit63d6avFjrWU4BhjDh2kxDgQF6oF2UG84LolxH
uoo6CwYMJs5IKx6sOiYBnn0CAwEAAaAdMBsGCSqGSIb3DQEJBzEODAxpbktLS0s0
OjdVeT0wDQYJKoZIhvcNAQELBQADggEBAHbGvq92/o6iPbVApbs/dT8GgCvU7eMC
AYjGIqoVM4r668YO+tttEGp3jf3DbMYWNmFnZBQNcvvPUIeqL2uA56AJy8Mk8+aJ
sCYixb88nyfLvq9yO2+TS3YA0WCigNstsCF+nqXCTrpjSDtrvxCDaRPihwIPhkHG
xY6DbH5+n4rtjPCfzJ1yJdJhuC4dTXgmhDOYV8n3Ie6o0BUwd6137WMyRUH2xht6
LvKdupVccNEYTCjsdeRJgT9G8oN/M6YyTqOKq240ZzypLvo/Eur2LNztdCQyfkqR
BiyfM8iWi6CQZOMCLQ1IMGBrbVWscY9igyNBhLsxvVHBOHlRwqWPAQM=
-----END CERTIFICATE REQUEST-----";

		const string Csr2 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIDSDCCAjACAQAwgdcxCzAJBgNVBAYTAkNOMRAwDgYDVQQIDAdKaWFuZ1NVMRAw
DgYDVQQHDAdOYW5qaW5nMQwwCgYDVQQKDANXVEcxLDAqBgNVBAsMI1Jlc2VhcmNo
IGFuZCBEZXZlbG9wbWVudCBEZXBhcnRtZW50MTgwNgYDVQQDDC9XVEcsIFRlY2hu
b2xvZ3kgYW5kIERldmVsb3BtZW50IERlcGFydG1lbnQsIFdXRzEuMCwGCSqGSIb3
DQEJARYfd2VuZGluZy53YW5nQHdpc2V0ZWNoZ2xvYmFsLmNvbTCCASIwDQYJKoZI
hvcNAQEBBQADggEPADCCAQoCggEBALiaFVr6oQJC234yx1DZLqO9mAWb8G4R0JBA
K6V9CQ/d/pc+GpVNXB2+D2lDXfoa2uhR6a3lVgmx/lgzhjhDeglEo/5u+5TjfxJM
6iphUOg1/hirGl/4/eOm0PdWJhiRsfvoQfrZjDziEQTUWZI4dUQSmFyqnIihDzLj
RUk68fOKnHWVPPSYfzuiHqOh79HepxYED9OCxVskOAaJbDfICil0g6qMvLuYoYPP
7xKAajB3dvKX26YFxoGi8kvV0+zjIV0ucIa96gpwd2KI+0VjCFWJZglgrf/PquO+
Ij/uRd5aVHm6quI9pwJapz0xhPugGg3nnfY+ZqebQ8lvK9Qjg10CAwEAAaArMBIG
CSqGSIb3DQEJAjEFDANXVEcwFQYJKoZIhvcNAQkHMQgMBjEyMzQ1NjANBgkqhkiG
9w0BAQsFAAOCAQEAYMp+zirn6RvczoA/5hySZ1UlwIEYNA66MU7/7pjbvquVMJu/
9Ydg9HMmAibGOrBu2j7wbCTXfb5aKI+snpRIR1ZRNrjn4JvdhBPwGTFOqnRiPwhy
rzUmwmveQnvvUNWRcj4CT2y8pKso+8bv9oHYWbUnv9olSguamFAAKqFvuaxyKr1i
j6kFzsFfTkZbTOEmlwjd8l72Lqr9as9to2Wrd/vJE5sA+nrt6FIngIT9G4ujgVQb
ti1Luqu+XuBdDSDoUfDu74uBko2tRmJ0yRBF6BoK9oLnwn9robCKq9jJjSwrc4NS
scN71qV3pKjYYS1B4CjG8HjxZbvx7ms7dU800A==
-----END CERTIFICATE REQUEST-----";

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

		const string Certificate2 = @"-----BEGIN CERTIFICATE-----
MIIDgzCCAmugAwIBAgIUCP33gIMaTkVQN80o+mVvzf2IvMgwDQYJKoZIhvcNAQEL
BQAwUTELMAkGA1UEBhMCQ04xCzAJBgNVBAgMAkpTMQswCQYDVQQHDAJOSjEMMAoG
A1UECgwDV1RHMQwwCgYDVQQLDANOSkcxDDAKBgNVBAMMA0pheTAeFw0yMzA0Mjcw
NjAwMTlaFw0yMzA1MjcwNjAwMTlaMFExCzAJBgNVBAYTAkNOMQswCQYDVQQIDAJK
UzELMAkGA1UEBwwCTkoxDDAKBgNVBAoMA1dURzEMMAoGA1UECwwDTkpHMQwwCgYD
VQQDDANKYXkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCcuf4gxhYr
UCRl52pwUJmal84UffcegB6sla+HCXb/kHseHGLgZP0G8yInEKHaMO6Pa/ZHyOtx
bzBnO2lvCMzHfimg0gNyvg6Euto+/RMQ3WApXlug+ecrb6YtTnWB/8P4n8lBd2Jh
gt4Qp/vssqTMYoOeKkEAqI7RhfdNpviPQh8NqFPhNfX11kwlMI6GlWcX1qfIlkLu
fGAPGVx4ZKAzM25ASmdj77RGvFFoxeXi2ateBFCQQ2tLsHS741eqZw5UhVI74tuF
wk7ZriWCnhAwCn7CzNm1Bk7kkA1bfyuuZKS9oH9fWMwv2+qrhXBlrl/V7dU4j5LE
9Em5AHEy3LJRAgMBAAGjUzBRMB0GA1UdDgQWBBTo1MWjmpOSg/7y5ATNLHQ2YF02
7zAfBgNVHSMEGDAWgBTo1MWjmpOSg/7y5ATNLHQ2YF027zAPBgNVHRMBAf8EBTAD
AQH/MA0GCSqGSIb3DQEBCwUAA4IBAQB3h0jYTuIN7ELcbSiuF5L0Rx19XaI3XDOJ
3A6GBCF/6UOUKM8+aklQGAeUrySOvzVqe4xDiuHN9WZhLZJtSEDfwZDIJX7n9LE6
IB/lqhI/RL5JdGNJ/nBv1TEYpfdzo6CAhwfCCZMSYTYLuVWX3MbmCSsi8hSPbS2j
DTHOzf+IKwhfrVMJPdcvKNDXm1+b29X1gakcl5/X0ov9BSo1jjip+7HY2J8OXbCE
LyEr/Vq5280ljJ1RzKR43OAEH+XI/eBi1SBTncm3B+Qxn7tN4AjQ4EtriN41xSK6
6KUR9JRMYiMgwm9W/4XaPqYITba7/Q4KJm3YmiE0CxChUe9yDToM
-----END CERTIFICATE-----
";
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[]
		{
			new TaskNudgeInformationForTest(
				EdiIdentityCertificateSchema.Constants.TableName,
				"Issue Certificates Queue",
				EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=QUE"
			),
			new TaskNudgeInformationForTest(
				EdiIdentityCertificateSchema.Constants.TableName,
				"Revoke Certificates Queue",
				EdiIdentityCertificateSchema.Constants.ICE_ProcessingStatus + "=CAN"
			),
		};
	}
}

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Http;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityApplicationPermission.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZClientWebCargoWiseEDI;
using Enterprise.ZClientWebCargoWiseEDI.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WTG.OAuth2.Token.TestFramework;
using WTG.TrustedMessaging.MyAccount.Models;
using WTG.TrustedMessaging.MyAccount.Models.DistributedData;
using static Enterprise.Core.Constants;

namespace ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity
{
	[HttpContextEnabledTest]
	class SystemToSystemTrustControllerTest : TestCaseWithFactory
	{
		#region RequestCertificate

		public void TestRequestCertificate_HttpRequestWillNotRepeat()
		{
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";
			var certificate = InitializeCertificate(new ZDateTime("2023-10-24 01:23:46"));

			EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, certificate.Application.TenantID.ToString());

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = Csr1 };

			var logger = new NLogWrapperForTest(GetType());
			var response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.Ok}";
			AssertContains(message, logger.ToString());

			var operationId = response.Content.ReadAsStringAsync().Result;
			var newFactory = new BusinessObjectFactory();
			var newCertificate = newFactory.Load<EdiIdentityCertificate>(new ZGuid(operationId));

			certificate.ReloadSafe();

			CombineAssertions(() =>
			{
				AssertNotNull("newCertificate", newCertificate);
				AssertNotEquals("Should create a new certificate", certificate.PK, newCertificate.PK);
				AssertEquals(Csr1, newCertificate.ICE_CertificateSigningRequest);
				AssertEquals(EdiIdentityCertificateProcessingStatus.Codes.QUE, newCertificate.ICE_ProcessingStatus);
				AssertEquals("old and new certificate uses the same application", certificate.ICE_IDA, newCertificate.ICE_IDA);
			});
		}

		public void TestRequestCertificate_EmptyContent()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";
			var response = InvokeRequestCertificate(null, logger, uri);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The message is malformed.";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestRequestCertificate_MissingRequiredFields()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = "", Password = "password", CertificateSignRequest = "TestCSR" };

			var response = InvokeRequestCertificate(request, logger, uri);
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			request = new IdentityCertificateInitialRequest { DatabaseNumber = "0", Password = "password", CertificateSignRequest = "TestCSR" };
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			request = new IdentityCertificateInitialRequest { DatabaseNumber = "-100", Password = "password", CertificateSignRequest = "TestCSR" };
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			request = new IdentityCertificateInitialRequest { DatabaseNumber = "123", Password = "", CertificateSignRequest = "TestCSR" };
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			request = new IdentityCertificateInitialRequest { DatabaseNumber = "123", Password = "123", CertificateSignRequest = "" };
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestRequestCertificate_InvalidDatabase()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = "12112112", Password = "password", CertificateSignRequest = "TestCSR" };

			var response = InvokeRequestCertificate(request, logger, uri);
			var message = $"SystemToSystemTrustControllerTest: 12112112 | {NLogWrapper.Status.BadRequest}: The specified system is not found.";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			Database.LD_IsActive = false;
			Factory.Save();

			request.DatabaseNumber = Database.LD_DatabaseNumber.ToString();
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.BadRequest}: Database Not Active";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			Database.LD_IsActive = true;
			Database.LD_Status = "NON";
			Factory.Save();

			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.BadRequest}: Database Not Registered";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestRequestCertificate_InvalidPassword()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = Database.LD_Password, CertificateSignRequest = "TestCSR" };

			var response = InvokeRequestCertificate(request, logger, uri);
			var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.BadRequest}: Password Mismatch";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestRequestCertificate_ApplicationExistsAlready()
		{
			var ediIdentityTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			ediIdentityTenant.IDT_TenantId = "TestTenantId";
			EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestTenantId");

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_LD = Database.PK;
			application.IDA_IDT = ediIdentityTenant.PK;
			application.IDA_IsActive = false;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = Csr1 };

			var response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.BadRequest}: The Application Already Exists And Is Not Active";
			AssertContains(message, logger.ToString());

			application.IDA_IsActive = true;
			Factory.Save();

			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var operationId = response.Content.ReadAsStringAsync().Result;

			var certificate = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));

			CombineAssertions(() =>
			{
				AssertNotNull(certificate);
				AssertEquals(application.PK, certificate.ICE_IDA);
				AssertEquals(CARootCodeDescriptionList.Codes.SystemToSystemTrust, certificate.ICE_CARoot);
			});
		}

		public void TestRequestCertificate_InvalidCSR()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = "TestCSR" };

			var response = InvokeRequestCertificate(request, logger, uri);
			var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.BadRequest}: Invalid Certificate Sign Request";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestRequestCertificate_WithoutModule()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var requestData = new JObject();
			requestData.Add("databaseNumber", Database.LD_DatabaseNumber.ToString());
			requestData.Add("password", "123");
			requestData.Add("csr", csr);

			var response = InvokeRequestCertificateWithoutModule(requestData, logger, uri);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestRequestCertificate_WithDuplicateCSR()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = csr };

			var response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var message = $"SystemToSystemTrustControllerTest: 8000 | 200";
			AssertContains(message, logger.ToString());

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_DatabaseNumber = 123;
			database.LD_Password =
				"26-3F-EC-58-86-14-49-AA-CC-1C-32-8A-4A-FF-64-AF-F4-C6-2D-F4-A2-D5-0B-3F-20-7F-A8-9B-6E-24-2C-9A-A7-78-E7-A8-BA-EF-FE-F8-5B-6C-A6-D2-E7-DC-16-FF-0A-76-0D-59-C1-3C-23-8F-6B-CD-C3-2F-8C-E9-CC-62";
			database.LD_Status = DatabaseStatusList.Codes.REG;
			Factory.Save();
			request.DatabaseNumber = database.LD_DatabaseNumber.ToString();
			logger.ClearLog();
			response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("Cannot insert duplicate key row in object 'dbo.EdiIdentityCertificate' with unique index 'NR_UX__ICE_CertificateSigningRequestHash'.", logger.ToString());
		}

		public void TestRequestCertificate_QueryApplicationByDatabaseNumberAndTenant()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant1.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_ApplicationName = "TestApp1";
			application1.IDA_LD = Database.PK;
			application1.IDA_IDT = tenant1.PK;
			Factory.Save();
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant2.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_ApplicationName = "TestApp2";
			application2.IDA_LD = Database.PK;
			application2.IDA_IDT = tenant2.PK;
			Factory.Save();

			EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tenant2.IDT_TenantId.ToString());

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = csr };
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";
			var response = InvokeRequestCertificate(request, logger, uri);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var operationId = response.Content.ReadAsStringAsync().Result;
			var certificate = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));
			AssertEquals("The application should be TestApp2 instead of TestApp1.", application2.IDA_ApplicationName, certificate.Application.IDA_ApplicationName);
		}

		public void TestRequestCertificate()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = "123", CertificateSignRequest = csr };
			var ediIdentityTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			ediIdentityTenant.IDT_TenantId = "TestTenantId";
			Factory.Save();
			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestTenantId"))
			{
				var response = InvokeRequestCertificate(request, logger, uri);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.Ok}";
				AssertContains(message, logger.ToString());

				var operationId = response.Content.ReadAsStringAsync().Result;

				var certificate = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));
				var expectedApplicationName = $"{Database.EnterpriseID}.{Database.LD_Product}.{Database.DatabaseId}";
				var application = certificate.Application;
				CombineAssertions(() =>
				{
					AssertNotNull(certificate);
					AssertEquals(Database.PK, certificate.Application.IDA_LD);
					AssertNullOrEmpty(certificate.Application.IDA_ClientID);
					AssertEquals(expectedApplicationName, application.IDA_ApplicationName);
					AssertEquals(ediIdentityTenant.PK, application.IDA_IDT);
					AssertEquals(1, application.Permissions.Count);
					Assert(application.Permissions.Any(x => x.IAP_Scope == "*"));
				});

				AssertNotNull(certificate);
				AssertEquals(csr, certificate.ICE_CertificateSigningRequest);
				AssertEquals(CARootCodeDescriptionList.Codes.SystemToSystemTrust, certificate.ICE_CARoot);

				AssertEquals(operationId, certificate.PK.ToString());

				logger.ClearLog();
				response = InvokeRequestCertificate(request, logger, uri);
				operationId = response.Content.ReadAsStringAsync().Result;
				AssertEquals(operationId, certificate.PK.ToString());
			}
		}

		public void TestRequestCertificate_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/request";

			var request = new IdentityCertificateInitialRequest { DatabaseNumber = Database.LD_DatabaseNumber.ToString(), Password = Database.LD_Password, CertificateSignRequest = "TestCSR" };

			var response = InvokeRequestCertificate(request, logger, uri, needToThrowException: true);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Request Certificate Failed.", logger.ToString());
			var content = response.Content.ReadAsStringAsync().Result;
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Service internal error. Please contact the administrator.\"}]}", content);
		}

		public void TestRegisterApplication()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/register";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				tenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				existingCertificate.Application.IDA_IDT = tenant.PK;
				Factory.Save();

				var request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor", CARoot = CARootCodeDescriptionList.Codes.Adaptor };

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				var response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var message = $"SystemToSystemTrustControllerTest: {identityServer.ClientIdentifier} | {request.Module} | {request.ApplicationDescription} | 200";
				AssertContains(message, logger.ToString());

				var operationId = response.Content.ReadAsStringAsync().Result;

				var certificate = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));
				var expectedApplicationName = $"{Database.EnterpriseCode}.{Database.LD_ServerCode}.{request.Module}.{request.ApplicationDescription}";
				var application = certificate.Application;
				CombineAssertions(() =>
				{
					AssertNotNull(certificate);
					AssertEquals(CARootCodeDescriptionList.Codes.Adaptor, certificate.ICE_CARoot);
					Assert(certificate.Application.IDA_LD.IsEmpty);
					AssertNullOrEmpty(certificate.Application.IDA_ClientID);
					AssertEquals(expectedApplicationName, application.IDA_ApplicationName);
					AssertEquals(request.Module, application.IDA_ApplicationModule);
					AssertEquals(existingCertificate.Application.PK, application.IDA_IDA_ParentApplication);
					AssertEquals(1, application.Permissions.Count);
					Assert(application.Permissions.Any(x => x.IAP_Scope == "*"));
				});

				logger.ClearLog();

				request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr + 1, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor2", CARoot = CARootCodeDescriptionList.Codes.Adaptor };
				var existingApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
				existingApplication.IDA_ApplicationName = $"{Database.EnterpriseCode}.{Database.LD_ServerCode}.{request.Module}.{request.ApplicationDescription}";
				Factory.Save();
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);

				message = $"SystemToSystemTrustControllerTest: {identityServer.ClientIdentifier} | {request.Module} | {request.ApplicationDescription} | 200";
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				AssertContains(message, logger.ToString());

				existingApplication.ReloadSafe();

				AssertEquals(1, existingApplication.Certificates.Count);

				var certificate2 = existingApplication.Certificates[0];
				CombineAssertions(() =>
				{
					AssertNotNull(certificate2);
					AssertEquals(CARootCodeDescriptionList.Codes.Adaptor, certificate.ICE_CARoot);
				});
			}
		}

		public void TestRegisterCertificate_WithoutRequiredData()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/register";
			var message = "SystemToSystemTrustControllerTest: The required field is missing. | 400";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var request = new IdentityCertificateRegisterRequest { CertificateSignRequest = string.Empty, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor", CARoot = CARootCodeDescriptionList.Codes.Adaptor };
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, identityServer.ClientIdentifier, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains(message, logger.ToString());
				request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = string.Empty, ApplicationDescription = "TesteAdaptor", CARoot = CARootCodeDescriptionList.Codes.Adaptor };
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains(message, logger.ToString());
				request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = "eAdaptor", ApplicationDescription = string.Empty, CARoot = CARootCodeDescriptionList.Codes.Adaptor };
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains(message, logger.ToString());
				request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor", CARoot = string.Empty };
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains(message, logger.ToString());
			}
		}

		public void TestRegisterCertificate_WithDuplicateCSR()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/register";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				existingCertificate.Application.IDA_IDT = tenant.PK;
				Factory.Save();

				var request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor", CARoot = CARootCodeDescriptionList.Codes.Adaptor };

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				var response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var operationId1 = response.Content.ReadAsStringAsync().Result;

				var message = $"SystemToSystemTrustControllerTest: {identityServer.ClientIdentifier} | {request.Module} | {request.ApplicationDescription} | 200";
				AssertContains(message, logger.ToString());

				logger.ClearLog();
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var operationId2 = response.Content.ReadAsStringAsync().Result;
				AssertEquals(operationId1, operationId2);

				var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var database = Factory.NewWithValidTestData<LicenceDatabase>();
				var existingCertificate2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate2.Application.IDA_LD = database.PK;
				existingCertificate2.Application.IDA_ClientID = ZGuid.BrettsGuid.ToString();
				existingCertificate2.Application.IDA_IDT = tenant2.PK;
				Factory.Save();
				logger.ClearLog();
				token = GetAccessToken(authorityUrl, ZGuid.BrettsGuid.ToString(), tenant2.IDT_TenantId.ToString());
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Cannot insert duplicate key row in object 'dbo.EdiIdentityCertificate' with unique index 'NR_UX__ICE_CertificateSigningRequestHash'.", logger.ToString());
			}
		}

		public void TestRegisterApplication_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_IDT = tenant.PK;
			Factory.Save();
			var uri = "https://unit-testing/api/SystemTrust/certificate/register";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var request = new IdentityCertificateRegisterRequest { CertificateSignRequest = csr, Module = "eAdaptor", ApplicationDescription = "TesteAdaptor", CARoot = "EAP1" };
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				//No Token
				var response = InvokeRegisterApplication(request, string.Empty, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());
				//Request Data is Invalid
				logger.ClearLog();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, string.Empty, SystemDataRegistry.Instance.EDIClientID.Value);
				var collection = new AWSPrivateCACollection();
				var awsPrivateCaArn = collection.AddNew();
				awsPrivateCaArn.IsEnabled = false;
				awsPrivateCaArn.IssuingCA = CARootCodeDescriptionList.Codes.Adaptor;
				awsPrivateCaArn.Arn = "Arn";
				awsPrivateCaArn.AccessKey = "AccessKey";
				awsPrivateCaArn.SecretKey = "SecretKey";
				logger.ClearLog();
				using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
				{
					response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					message = $"SystemToSystemTrustControllerTest: Invalid Certificate Register Request Data";
					AssertContains(message, logger.ToString());
				}
				//ClientId is Empty
				logger.ClearLog();
				request.CARoot = CARootCodeDescriptionList.Codes.Adaptor;
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token client id is empty";
				AssertContains(message, logger.ToString());
				//Cannot find CW1 application
				logger.ClearLog();
				request.Module = "eAdaptor";
				token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenant.IDT_TenantId}' and client id '{identityServer.ClientIdentifier}'";
				AssertContains(message, logger.ToString());
				//Cannot find database
				logger.ClearLog();
				var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				application.IDA_LD = licenceDatabase.PK;
				token = GetAccessToken(authorityUrl, clientId, tenant.IDT_TenantId.ToString());
				response = InvokeRegisterApplication(request, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: Invalid licence info {clientId}";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestRolloverCertificate_NoAccessToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
			var response = InvokeRolloverCertificate(null, "", logger, uri);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());
		}

		public void TestRolloverCertificate_InvalidAccessToken()
		{
			var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			existingCertificate.Application.IDA_LD = Database.PK;
			existingCertificate.Application.IDA_ClientID = Guid.NewGuid().ToString();
			Factory.Save();

			var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = existingCertificate.Application.IDA_ClientID, };

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
			var response = InvokeRolloverCertificate(certificateRolloverRequest, "Invalid Access Token", logger, uri);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
			AssertContains(message, logger.ToString());
		}

		public void TestRolloverCertificate_InvalidClientId()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, S2STApplication.IDA_ClientID.ToString(), Tenant.IDT_TenantId.ToString());

				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = "", };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
				AssertContains(message, logger.ToString());

				certificateRolloverRequest.ClientId = "InvalidClientId";
				logger.ClearLog();
				InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Client Id";
				AssertContains(message, logger.ToString());

				certificateRolloverRequest.ClientId = Guid.NewGuid().ToString();
				logger.ClearLog();
				InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Client Id - The application with client id '{certificateRolloverRequest.ClientId}' isn't active or doesn't exist.";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestRolloverCertificate_EmptyCSR()
		{
			var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = "", ClientId = Guid.NewGuid().ToString(), };

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
			var response = InvokeRolloverCertificate(certificateRolloverRequest, "null", logger, uri, "", false);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
			AssertContains(message, logger.ToString());
		}

		public void TestRolloverCertificate_InvalidCSR()
		{
			var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = "testCSR", ClientId = Guid.NewGuid().ToString(), };

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
			var response = InvokeRolloverCertificate(certificateRolloverRequest, "null", logger, uri, "", false);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Certificate Sign Request";
			AssertContains(message, logger.ToString());
		}

		const string csr = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGA1UEBhMCQVUxETAPBgNVBAoMCGpheXd0Z0NBMRww
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

		const string ClientId = "46546646-e627-46fb-afe4-e5ea9928740e";

		const string TenantId = "804a70cf-4a61-4a08-908e-afdf43432443";

		readonly X509Certificate2 certificateData = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));

		public void TestRolloverCertificate()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_IDT = Tenant.PK;
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, Tenant.IDT_TenantId.ToString());
				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = identityServer.ClientIdentifier };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var expectedMessage = $"SystemToSystemTrustControllerTest: {certificateRolloverRequest.ClientId} | 200 Ok";
				AssertContains(expectedMessage, logger.ToString());

				var operationId = response.Content.ReadAsStringAsync().Result;
				var certificateRequest = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));

				CombineAssertions(() =>
				{
					AssertNotNull(certificateRequest);
					AssertEquals(csr, certificateRequest.ICE_CertificateSigningRequest);
					AssertEquals(existingCertificate.Application.PK, certificateRequest.ICE_IDA);
					AssertEquals(CARootCodeDescriptionList.Codes.SystemToSystemTrust, certificateRequest.ICE_CARoot);
				});

				logger.ClearLog();

				response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				var operationId1 = response.Content.ReadAsStringAsync().Result;
				AssertEquals(operationId, operationId1);

				var certificateRequests = Factory.Load<EdiIdentityCertificate>(new ZQuery(EdiIdentityCertificateSchema.ICE_CertificateSigningRequest, csr));
				AssertNotNull(certificateRequests);
				AssertEquals(1, certificateRequests.Length);
				AssertContains(expectedMessage, logger.ToString());
			}
		}

		public void TestRolloverCertificate_WithDuplicateCSR()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
				application.IDA_LD = Database.PK;
				var certificate = application.Certificates.AddNew();
				certificate.ICE_CertificateSigningRequest = csr;
				var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = database2.PK;
				existingCertificate.Application.IDA_IDT = Tenant.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, existingCertificate.Application.IDA_ClientID.ToString(), Tenant.IDT_TenantId.ToString());
				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = identityServer.ClientIdentifier };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("Cannot insert duplicate key row in object 'dbo.EdiIdentityCertificate' with unique index 'NR_UX__ICE_CertificateSigningRequestHash'.", logger.ToString());
				var result = response.Content.ReadAsStringAsync().Result;
				AssertContains("Error saving record. Please contact the administrator.", result);
			}
		}

		public void TestRolloverCertificate_UnActive()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				existingCertificate.ICE_IsActive = false;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = identityServer.GenerateIdentityToken();
				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = identityServer.ClientIdentifier };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, Encoding.Unicode.GetString(token), logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			}
		}

		public void TestRolloverCertificate_WithEAdaptor()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_IDT = Tenant.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, existingCertificate.Application.IDA_ClientID.ToString(), Tenant.IDT_TenantId.ToString());
				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = identityServer.ClientIdentifier, CaRootType = CARootCodeDescriptionList.Codes.Adaptor };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var expectedMessage = $"SystemToSystemTrustControllerTest: {certificateRolloverRequest.ClientId} | 200 Ok";
				AssertContains(expectedMessage, logger.ToString());

				var operationId = response.Content.ReadAsStringAsync().Result;
				var certificateRequest = Factory.Load<EdiIdentityCertificate>(new ZGuid(operationId));

				CombineAssertions(() =>
				{
					AssertNotNull(certificateRequest);
					AssertEquals(csr, certificateRequest.ICE_CertificateSigningRequest);
					AssertEquals(certificateRolloverRequest.CaRootType, certificateRequest.ICE_CARoot);
					AssertEquals(existingCertificate.Application.PK, certificateRequest.ICE_IDA);
				});
			}
		}

		public void TestRolloverCertificate_CaRootNotFound()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var existingCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				existingCertificate.Application.IDA_LD = Database.PK;
				existingCertificate.Application.IDA_ClientID = identityServer.ClientIdentifier;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var certificateRolloverRequest = new IdentityCertificateRolloverRequest { CertificateSignRequest = csr, ClientId = identityServer.ClientIdentifier, CaRootType = "Test" };

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/certificate/rollover";
				var response = InvokeRolloverCertificate(certificateRolloverRequest, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Ca Root";
				AssertContains(message, logger.ToString());
			}
		}

		#endregion

		#region DownloadCertificate

		public void TestDownloadCertificate_InvalidOperationId()
		{
			var logger = new NLogWrapperForTest(GetType());

			var operationId = Guid.NewGuid();
			var response = InvokeDownloadCertificate(operationId, logger);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: OperationId Is Invalid";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);

			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "QUE";
			certificate.ICE_IsActive = false;
			Factory.Save();

			logger.ClearLog();
			response = InvokeDownloadCertificate(certificate.PK, logger);
			message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: OperationId Is Invalid";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
		}

		public void TestDownloadCertificate()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "QUE";
			certificate.Application.IDA_ClientID = clientId;
			Factory.Save();

			var response = InvokeDownloadCertificate(certificate.PK, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content.ReadAsStringAsync().Result);
			AssertNotNull(certificateResponse);
			AssertEquals("The certificate is awaiting processing.", certificateResponse.Status);
			AssertEquals("QUE", certificateResponse.StatusCode);
			Assert(certificateResponse.CertificateData.IsNullOrEmpty());

			certificate.ICE_ProcessingStatus = "PRC";
			Factory.Save();

			logger.ClearLog();
			response = InvokeDownloadCertificate(certificate.PK, logger);
			certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content.ReadAsStringAsync().Result);
			AssertNotNull(certificateResponse);
			AssertEquals("The certificate is currently being processed.", certificateResponse.Status);
			AssertEquals("PRC", certificateResponse.StatusCode);
			Assert(certificateResponse.CertificateData.IsNullOrEmpty());

			certificate.ICE_ProcessingStatus = "COM";

			var certificateData = new byte[] { 1, 2, 3 };
			certificate.ICE_CertificateData = certificateData;
			Factory.Save();

			logger.ClearLog();
			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TenantId))
			{
				response = InvokeDownloadCertificate(certificate.PK, logger);
				certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content.ReadAsStringAsync().Result);
			}

			CombineAssertions(() =>
			{
				AssertNotNull(certificateResponse);
				AssertEquals("The certificate is completed.", certificateResponse.Status);
				AssertNotNull(certificateResponse.CertificateData);
				AssertEquals(certificateData, certificateResponse.CertificateData);
				AssertEquals("ClientId", clientId, certificateResponse.ClientId);
				AssertEquals("TenantId", TenantId, certificateResponse.TenantId);
				AssertEquals("StatusCode", "COM", certificateResponse.StatusCode);
			});
		}

		public void TestDownloadCertificate_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "QUE";
			certificate.Application.IDA_ClientID = clientId;
			Factory.Save();

			var response = InvokeDownloadCertificate(certificate.PK, logger, needToThrowException: true);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Building Certificate Response Failed.", logger.ToString());
			var content = response.Content.ReadAsStringAsync().Result;
			AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
		}

		public void TestDownloadCertificates_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());

			var clientId = Guid.NewGuid().ToString();
			var response = InvokeDownloadCertificateByClientId(clientId, string.Empty, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());

			logger.ClearLog();

			var response2 = RequestDownloadCertificates(clientId, logger);
			message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Client Id - The application with client id '{clientId}' isn't active or doesn't exist.";
			AssertContains(message, logger.ToString());
			AssertEquals(HttpStatusCode.BadRequest, response2.StatusCode);
		}

		public void TestDownloadCertificates_ClientId()
		{
			var clientId = ZGuid.BrettsGuid.ToString();

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_IDT = Tenant.PK;
			Factory.Save();

			var response = RequestDownloadCertificates(clientId, new NLogWrapperForTest(GetType()));
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertNotNull(certificateResponse);
				AssertEquals("ClientId", clientId, certificateResponse.ClientId);
				AssertEquals("TenantId", TenantId, certificateResponse.TenantId);
				AssertEquals(0, certificateResponse.CertificateDataArray.Length);
				AssertNull(certificateResponse.CertificateData);
			});

			var certificateData1 = new byte[] { 1, 2, 3 };
			var certificateData2 = new byte[] { 1, 2, 4 };
			var certificateData3 = new byte[] { 1, 2, 5 };

			var clientId2 = Guid.NewGuid().ToString();

			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_ClientID = clientId2;
			application2.IDA_IDT = Tenant.PK;
			var certificate = application2.Certificates.AddNew();
			certificate.ICE_ProcessingStatus = "COM";
			certificate.ICE_CertificateData = certificateData1;

			var certificate2 = application2.Certificates.AddNew();
			certificate2.ICE_CertificateData = certificateData2;
			certificate2.ICE_ProcessingStatus = "CAN";

			var certificate3 = application2.Certificates.AddNew();
			certificate3.ICE_CertificateData = certificateData3;
			certificate3.ICE_ProcessingStatus = "CAN";
			certificate3.ICE_IsActive = false;

			var certificate4 = application2.Certificates.AddNew();
			certificate4.ICE_ProcessingStatus = "QUE";
			Factory.Save();

			var response2 = RequestDownloadCertificates(clientId2, new NLogWrapperForTest(GetType()));
			AssertEquals(HttpStatusCode.OK, response2.StatusCode);
			var certificateResponse2 = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response2.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertNotNull(certificateResponse2);
				AssertEquals("ClientId", clientId2, certificateResponse2.ClientId);
				AssertEquals("TenantId", TenantId, certificateResponse2.TenantId);
				AssertEquals(1, certificateResponse2.CertificateDataArray.Length);
				AssertNull(certificateResponse2.CertificateData);
				var list = certificateResponse2.CertificateDataArray.ToList();
				AssertEquals(1, list.Count(a => a.CertificateData == certificate.ICE_CertificateData));
			});
		}

		public void TestDownloadCertificates_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());

			var clientId = Guid.NewGuid().ToString();

			var response = RequestDownloadCertificates(clientId, logger, needToThrowException: true);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Query Application From Token Failed.", logger.ToString());
			var content = response.Content.ReadAsStringAsync().Result;
			AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
		}

		#endregion

		#region sync certificate

		const string CertificateString = @"-----BEGIN CERTIFICATE-----
MIID2jCCAsKgAwIBAgIQR0RemLM7Lo27g7CJGUVSRzANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwNTA4MDYxNDAzWhcNMjQwNTA3MDcxNDAz
WjBjMQswCQYDVQQGEwJBVTEMMAoGA1UECAwDU1lEMQwwCgYDVQQHDANTWUQxDDAK
BgNVBAoMA1dURzEMMAoGA1UECwwDV1RHMRwwGgYDVQQDDBN3aXNldGVjaC5nbG9i
YWwuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA57/KUGd2YHrb
Y1iViSXwGCWe8UsvxxPPsNA5DlzjH1pAy6OJb5dSC8SPInr5xQCvwf360if3XrkS
CJkvJlx9RhcHgpnQQfLiuvVIHfbZQHMRL7+Oq4APzdNXknXBMSdUjjc9eSBjpxRh
DNqfLxHcJlSzGZZNJCjLnjnL3qtK5WBNgju7g+ZPufMKBRfAciNlAJQWMK/CLFCt
tllH9wuFBR8IYY6orzffJ1nocd0wle5lcNg8ihgsGSoqgMcR32ZU3l+MNE8qDcLi
lV2OaW0alou2K3rd3pq8WOtZTgGGMOHaTEOBAXqgXZQbzguiXEe6ijoLBgwmzkgr
Hqw6JgGefQIDAQABo3wwejAJBgNVHRMEAjAAMB8GA1UdIwQYMBaAFMUZFOQcnWCL
2XW0xLCOqDhZ7wLiMB0GA1UdDgQWBBRgVXKxahXMIirYYm0846gHnJeY5jAOBgNV
HQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMA0GCSqG
SIb3DQEBCwUAA4IBAQBcRs+X0iH/K5dOTQkZ5/v13huLOXb3QFExTJW2+tKmRYe1
e3CoQVE6LQJ+cCQ+Tl6UmbyeyTIqEuFpTmm6Yhl9agu41tlgiobP1+YQ/VMjasgh
Vhgr34KA09iVzpLsIlROdNW5Q5rfjRh1WuBAEPcABKKNFgaqVvS2BKzd/a6aXaId
Zu+YuRV232OXOUZP07DkaLhax6wfSf+tfkNLQLvoVNcJmcF6mNgzg2HULuvia77u
HPpykW02IbnSAr3jDVGHezVNLctFrpHpCWRlTUMulW56xc74ZiONSf+N/2WZJo0x
wNakFhJRZGZJpKCx8xrIO4D9y7jt0WGP7ZCvcXeQ
-----END CERTIFICATE-----";

		[TestRequiresAdministrativePrivileges("MockOpenIDIdentityServer Needs to start http listener")]
		public void TestSyncAllCertificates_TokenValidation()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var logger = new NLogWrapperForTest(GetType());
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				var response = InvokeSyncAllCertificate(0, 10, logger, true, "", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());
				logger.ClearLog();

				response = InvokeSyncAllCertificate(0, 10, logger, true, "invalidToken", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
				AssertContains(message, logger.ToString());
				logger.ClearLog();

				var azp = Guid.NewGuid().ToString();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				response = InvokeSyncAllCertificate(0, 10, logger, true, token, authorityUrl);

				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var syncResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

				CombineAssertions(() =>
				{
					AssertEquals("No data loaded.", 0, syncResponse.CreatedCertificates.Count);
					AssertEquals("No data loaded.", 0, syncResponse.DeletedCertificates.Count);
					AssertEquals("No data loaded.", 0, syncResponse.LastSequenceNumber);
					Assert("should stop the sync as no data loaded.", syncResponse.SyncEnded);
				});
			}
		}

		public void TestSyncAllCertificates()
		{
			var normalCertificatePks = new List<Guid>();
			var deletedCertificatePks = new List<Guid>();
			for (var i = 0; i < 5; i++)
			{
				var queuedCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				queuedCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.QUE;

				var processedCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				processedCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.PRC;

				Factory.Save(); // make sure the sequence numbers are in order

				var cw1Cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				cw1Cert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
				cw1Cert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
				cw1Cert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
				cw1Cert.ICE_IsActive = true;
				normalCertificatePks.Add(cw1Cert.PK.ToGuid());

				var nonCw1Cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				nonCw1Cert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
				nonCw1Cert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
				nonCw1Cert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
				nonCw1Cert.Application.IDA_LD = ZGuid.Empty;
				normalCertificatePks.Add(nonCw1Cert.PK.ToGuid());
				Factory.Save(); // make sure the sequence numbers are in order

				var cancelledCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				cancelledCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
				cancelledCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
				cancelledCertificate.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
				cancelledCertificate.ICE_IsActive = false;
				deletedCertificatePks.Add(cancelledCertificate.PK.ToGuid());

				var expiredCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
				expiredCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
				expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
				expiredCertificate.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
				expiredCertificate.ICE_IsActive = false;
				deletedCertificatePks.Add(expiredCertificate.PK.ToGuid());

				Factory.Save(); // make sure the sequence numbers are in order
			}

			var logger = new NLogWrapperForTest(GetType());

			var response = InvokeSyncAllCertificate(0, 10, logger, false);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var certificateResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertEquals(6, certificateResponse.CreatedCertificates.Count);
				AssertContainsExactElementsInAnyOrder(normalCertificatePks.Take(6), certificateResponse.CreatedCertificates.Select(cert => cert.PK));
				AssertEquals(4, certificateResponse.DeletedCertificates.Count);
				AssertContainsExactElementsInAnyOrder(deletedCertificatePks.Take(4), certificateResponse.DeletedCertificates);
				Assert("There are certificates not synced.", !certificateResponse.SyncEnded);
				AssertEquals("10 items match the condition.", 10, certificateResponse.CreatedCertificates.Count + certificateResponse.DeletedCertificates.Count);
			});

			var lastSeq = certificateResponse.LastSequenceNumber;
			response = InvokeSyncAllCertificate(lastSeq, 12, logger, false);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			certificateResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertEquals(4, certificateResponse.CreatedCertificates.Count);
				AssertContainsExactElementsInAnyOrder(normalCertificatePks.Skip(6), certificateResponse.CreatedCertificates.Select(cert => cert.PK));
				AssertEquals(6, certificateResponse.DeletedCertificates.Count);
				AssertContainsExactElementsInAnyOrder(deletedCertificatePks.Skip(4), certificateResponse.DeletedCertificates);
				Assert("All certificates are synced.", certificateResponse.SyncEnded);
				AssertEquals("only 10 matched items left.", 10, certificateResponse.CreatedCertificates.Count + certificateResponse.DeletedCertificates.Count);
			});
		}

		public void TestSyncAllCertificates_LimitedRowsInRegistry()
		{
			var limitedRows = 5;
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, limitedRows))
			{
				for (var i = 0; i < 5; i++)
				{
					var normalCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
					normalCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
					normalCertificate.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
					normalCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);

					var expiredCertificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
					expiredCertificate.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
					expiredCertificate.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
					expiredCertificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
					expiredCertificate.ICE_IsActive = false;
				}

				Factory.Save();

				var logger = new NLogWrapperForTest(GetType());

				var response = InvokeSyncAllCertificate(0, 10, logger, false);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var certificateResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

				CombineAssertions(() =>
				{
					AssertEquals(3, certificateResponse.CreatedCertificates.Count);
					AssertEquals("ICE_IsActive is false.", 2, certificateResponse.DeletedCertificates.Count);
					Assert("sync is not ended.", !certificateResponse.SyncEnded);
					AssertEquals("5 items return as the registry defines the max rows which is lower than the request parameter.", limitedRows,
						certificateResponse.CreatedCertificates.Count + certificateResponse.DeletedCertificates.Count);
				});
			}
		}

		public void TestSyncAllCertificates_LastSeqNumberShouldAtLeastEqualToQueryParameter()
		{
			var logger = new NLogWrapperForTest(GetType());
			var seqNum = 11;
			var response = InvokeSyncAllCertificate(seqNum, 10, logger, false);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var certificateResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertEquals("No data loaded.", 0, certificateResponse.CreatedCertificates.Count);
				AssertEquals("No data loaded.", 0, certificateResponse.DeletedCertificates.Count);
				AssertEquals("it equals the parameter in the request.", seqNum, certificateResponse.LastSequenceNumber);
			});
		}

		public void TestSyncAllCertificates_Categories()
		{
			var ediProdPk = Guid.NewGuid();
			foreach (var code in WiseTechGlobalInternalSystemCodes.AllCodes)
			{
				var leEdi = Factory.NewWithValidTestData<LicenceEnterprise>();
				leEdi.LE_EnterpriseCode = code;

				if (code == WiseTechGlobalInternalSystemCodes.EDI)
				{
					var ediProdCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
					ediProdCert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
					ediProdCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
					ediProdCert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
					ediProdCert.ICE_IsActive = true;
					ediProdCert.LicenseDatabase.LD_LE = leEdi.PK;
					ediProdCert.LicenseDatabase.LD_ServerCode = "SYD";
					ediProdPk = ediProdCert.PK.ToGuid();

					var ediOtherCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
					ediOtherCert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
					ediOtherCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
					ediOtherCert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
					ediOtherCert.ICE_IsActive = true;
					ediOtherCert.LicenseDatabase.LD_LE = leEdi.PK;
					ediOtherCert.LicenseDatabase.LD_ServerCode = "TST";
				}
				else
				{
					var internalCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
					internalCert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
					internalCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
					internalCert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
					internalCert.ICE_IsActive = true;
					internalCert.LicenseDatabase.LD_LE = leEdi.PK;
				}
			}

			var customCW1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			customCW1.LE_EnterpriseCode = "UP3";

			var productCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			productCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			productCert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			productCert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
			productCert.LicenseDatabase.LD_LE = customCW1.PK;
			productCert.LicenseDatabase.LD_LicenceType = DatabaseTypes.Codes.Production;

			var testCert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			testCert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			testCert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			testCert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
			productCert.LicenseDatabase.LD_LE = customCW1.PK;
			testCert.LicenseDatabase.LD_LicenceType = DatabaseTypes.Codes.Test;

			var nonCw1Cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			nonCw1Cert.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			nonCw1Cert.ICE_CertificateData = Encoding.UTF8.GetBytes(CertificateString);
			nonCw1Cert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			nonCw1Cert.Application.IDA_LD = ZGuid.Empty;

			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());

			var response = InvokeSyncAllCertificate(0, 20, logger, false);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);

			var certificateResponse = JsonConvert.DeserializeObject<CertificatesSyncResponse>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertEquals(10, certificateResponse.CreatedCertificates.Count);

				var ediProdCert = certificateResponse.CreatedCertificates.First(cert => cert.PK == ediProdPk);
				AssertEquals("ediprod should be Production.", CertificateCategory.Production, ediProdCert.Category);

				var prodCert = certificateResponse.CreatedCertificates.First(cert => cert.PK == productCert.PK);
				AssertEquals("Production cw1 should be Production.", CertificateCategory.Production, prodCert.Category);

				var tstCert = certificateResponse.CreatedCertificates.First(cert => cert.PK == testCert.PK);
				AssertEquals("test cw1 should be test.", CertificateCategory.Test, tstCert.Category);

				var unclassifiedCert = certificateResponse.CreatedCertificates.First(cert => cert.PK == nonCw1Cert.PK);
				AssertEquals("non cw1 should be unclassified.", CertificateCategory.Unclassified, unclassifiedCert.Category);

				var internalCerts = certificateResponse.CreatedCertificates.Where(cert => cert.PK != ediProdPk && cert.PK != productCert.PK && cert.PK != testCert.PK && cert.PK != nonCw1Cert.PK)
					.Select(cert => cert.Category)
					.Distinct();
				AssertEquals("all others are internal.", 1, internalCerts.Count());
				AssertEquals(CertificateCategory.Internal, internalCerts.First());
			});
		}

		#endregion

		#region sync LicenceDatabase

		[TestRequiresAdministrativePrivileges("MockOpenIDIdentityServer Needs to start http listener")]
		public void TestSyncLicenceDatabases_TokenValidation()
		{
			using var identityServer = new MockOpenIDIdentityServer();
			var logger = new NLogWrapperForTest(GetType());
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
			var response = InvokeSyncRoute("licenceDatabases", 10, Guid.Empty, logger, true, "", authorityUrl);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());
			logger.ClearLog();

			response = InvokeSyncRoute("licenceDatabases", 10, Guid.Empty, logger, true, "invalidToken", authorityUrl);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
			AssertContains(message, logger.ToString());
			logger.ClearLog();

			var azp = Guid.NewGuid().ToString();
			var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
			response = InvokeSyncRoute("licenceDatabases", 10, Guid.Empty, logger, true, token, authorityUrl);

			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var syncResponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<LicenceDatabaseModel>>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				// Added a new entry in the setup method
				AssertEquals(1, syncResponse.Items.Count);
			});
		}

		public void TestSyncLicenceDatabases()
		{
			for (var i = 0; i < 4; i++)
			{
				Factory.NewWithValidTestData<LicenceDatabase>();
			}
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(LicenceDatabase));
			var licenceDatabases = Factory.Load<LicenceDatabase>(query);
			var sortedLicenceDatabases = licenceDatabases.OrderBy(ld => ld.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();

			var logger = new NLogWrapperForTest(GetType());

			var response = InvokeSyncRoute("licenceDatabases", 2, sortedLicenceDatabases[0], logger, false);
			AssertSyncRoute<LicenceDatabaseModel>(response, sortedLicenceDatabases, 1);
			response = InvokeSyncRoute("licenceDatabases", 2, sortedLicenceDatabases[2], logger, false);
			AssertSyncRoute<LicenceDatabaseModel>(response, sortedLicenceDatabases, 3);
			response = InvokeSyncRoute("licenceDatabases", 2, sortedLicenceDatabases[4], logger, false);
			AssertSyncRouteWhenLastPKIsLatest<LicenceDatabaseModel>(response);
			response = InvokeSyncRoute("licenceDatabases", 10, Guid.Empty, logger, false);
			AssertSyncRouteWithExpectedCount<LicenceDatabaseModel>(response);
		}

		public void TestSyncLicenceDatabases_LimitedRowsInRegistry()
		{
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				for (var i = 0; i < 10; i++)
				{
					Factory.NewWithValidTestData<LicenceDatabase>();
				}
				Factory.Save();

				var logger = new NLogWrapperForTest(GetType());

				var response = InvokeSyncRoute("licenceDatabases", 10, Guid.Empty, logger, false);
				AssertSyncRouteWithExpectedCount<LicenceDatabaseModel>(response, false);
			}
		}

		#endregion

		#region sync EdiIdentityApplicationPermissions

		[TestRequiresAdministrativePrivileges("MockOpenIDIdentityServer Needs to start http listener")]
		public void TestSyncEdiIdentityApplicationPermissions_TokenValidation()
		{
			using var identityServer = new MockOpenIDIdentityServer();
			var logger = new NLogWrapperForTest(GetType());
			var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
			var response = InvokeSyncRoute("applicationPermissions", 10, Guid.Empty, logger, true, "", authorityUrl);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
			AssertContains(message, logger.ToString());
			logger.ClearLog();

			response = InvokeSyncRoute("applicationPermissions", 10, Guid.Empty, logger, true, "invalidToken", authorityUrl);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
			AssertContains(message, logger.ToString());
			logger.ClearLog();

			var azp = Guid.NewGuid().ToString();
			var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
			response = InvokeSyncRoute("applicationPermissions", 10, Guid.Empty, logger, true, token, authorityUrl);

			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var syncResponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<ApplicationPermissionModel>>(response.Content.ReadAsStringAsync().Result);

			CombineAssertions(() =>
			{
				AssertEquals(0, syncResponse.Items.Count);
			});
		}

		public void TestSyncEdiIdentityApplicationPermissions()
		{
			for (var i = 0; i < 5; i++)
			{
				Factory.NewWithValidTestData<EdiIdentityApplicationPermission>();
			}
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(EdiIdentityApplicationPermission));
			var applicationPermissions = Factory.Load<EdiIdentityApplicationPermission>(query);
			var sortedApplicationPermissions = applicationPermissions.OrderBy(ap => ap.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();

			var logger = new NLogWrapperForTest(GetType());
			var response = InvokeSyncRoute("applicationPermissions", 2, sortedApplicationPermissions[0], logger, false);
			AssertSyncRoute<ApplicationPermissionModel>(response, sortedApplicationPermissions, 1);
			response = InvokeSyncRoute("applicationPermissions", 2, sortedApplicationPermissions[2], logger, false);
			AssertSyncRoute<ApplicationPermissionModel>(response, sortedApplicationPermissions, 3);
			response = InvokeSyncRoute("applicationPermissions", 2, sortedApplicationPermissions[4], logger, false);
			AssertSyncRouteWhenLastPKIsLatest<ApplicationPermissionModel>(response);
			response = InvokeSyncRoute("applicationPermissions", 10, Guid.Empty, logger, false);
			AssertSyncRouteWithExpectedCount<ApplicationPermissionModel>(response);
		}

		public void TestSyncEdiIdentityApplicationPermissions_LimitedRowsInRegistry()
		{
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				for (var i = 0; i < 10; i++)
				{
					Factory.NewWithValidTestData<EdiIdentityApplicationPermission>();
				}
				Factory.Save();

				var logger = new NLogWrapperForTest(GetType());
				var response = InvokeSyncRoute("applicationPermissions", 10, Guid.Empty, logger, false);
				AssertSyncRouteWithExpectedCount<ApplicationPermissionModel>(response, false);
			}
		}

		#endregion

		#region Application

		public void TestSyncApplications()
		{
			var applications = new List<EdiIdentityApplication>() { S2STApplication };
			for (var i = 0; i < 5; i++)
			{
				var application = InitializeEdiIdentityApplication(i);
				applications.Add(application);
				Factory.Save();
			}

			var sortApplications = applications.OrderBy(x => x.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, S2STApplication.IDA_ClientID.ToString(), Tenant.IDT_TenantId.ToString());
				var response = InvokeSyncRoute("applications", 2, sortApplications[0], logger, true, token, authorityUrl);
				AssertSyncRoute<IdentityApplicationModel>(response, sortApplications, 1);
				response = InvokeSyncRoute("applications", 2, sortApplications[2], logger, true, token, authorityUrl);
				AssertSyncRoute<IdentityApplicationModel>(response, sortApplications, 3);
				response = InvokeSyncRoute("applications", 2, sortApplications[5], logger, true, token, authorityUrl);
				AssertSyncRouteWhenLastPKIsLatest<IdentityApplicationModel>(response);
				response = InvokeSyncRoute("applications", 10, Guid.Empty, logger, true, token, authorityUrl);
				AssertSyncRouteWithExpectedCount<IdentityApplicationModel>(response, true, 6);
			}
		}

		public void TestSyncApplications_LimitedRowsInRegistry()
		{
			var applications = new List<EdiIdentityApplication>();
			for (var i = 0; i < 6; i++)
			{
				var application = InitializeEdiIdentityApplication(i);
				applications.Add(application);
				Factory.Save();
			}

			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("applications", 6, Guid.Empty, logger, true, token, authorityUrl);
				AssertSyncRouteWithExpectedCount<IdentityApplicationModel>(response, false);
			}
		}

		public void TestSyncApplications_IncludeFKDataIfExist()
		{
			var parentApplication = InitializeEdiIdentityApplication(0);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var applicationWithFKData = InitializeEdiIdentityApplication(1);
			applicationWithFKData.IDA_OH_ParentOrg = orgHeader.PK;
			applicationWithFKData.IDA_IDA_ParentApplication = parentApplication.PK;
			applicationWithFKData.IDA_LD = Database.PK;
			var applicationWithoutFKData = InitializeEdiIdentityApplication(2);
			applicationWithoutFKData.IDA_OH_ParentOrg = ZGuid.Empty;
			applicationWithoutFKData.IDA_IDA_ParentApplication = ZGuid.Empty;
			applicationWithoutFKData.IDA_LD = ZGuid.Empty;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, S2STApplication.IDA_ClientID.ToString(), Tenant.IDT_TenantId.ToString());
				var response = InvokeSyncRoute("applications", 4, Guid.Empty, logger, true, token, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var applicationReponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<IdentityApplicationModel>>(response.Content.ReadAsStringAsync().Result);

				AssertEquals(4, applicationReponse.Items.Count);// An S2STApplication has already been created in the setup method, so count is 4.
				var applicationWithOrgResponse = applicationReponse.Items.FirstOrDefault(x => x.PK == applicationWithFKData.PK.ToGuid());
				AssertEquals(orgHeader.PK, applicationWithOrgResponse.ParentOrgPK);
				AssertEquals(parentApplication.PK, applicationWithOrgResponse.ParentApplicationPK);
				AssertEquals(Database.PK, applicationWithOrgResponse.LicenceDatabasePK);
				var applicationWithoutOrgResponse = applicationReponse.Items.FirstOrDefault(x => x.PK == applicationWithoutFKData.PK.ToGuid());
				AssertEquals(null, applicationWithoutOrgResponse.ParentOrgPK);
				AssertEquals(null, applicationWithoutOrgResponse.ParentApplicationPK);
				AssertEquals(null, applicationWithoutOrgResponse.LicenceDatabasePK);
			}
		}

		public void TestSyncApplications_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				//No Token
				var response = InvokeSyncRoute("applications", 6, Guid.Empty, logger, true, string.Empty, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());

				response = InvokeSyncRoute("applications", 6, Guid.Empty, logger, true, "1234", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestSyncApplications_WithException()
		{
			var applications = new List<EdiIdentityApplication>();
			for (int i = 0; i < 5; i++)
			{
				var application = InitializeEdiIdentityApplication(i);
				applications.Add(application);
				Factory.Save();
			}

			var sortApplications = applications.OrderBy(x => x.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("applications", 2, sortApplications[0], logger, true, token, authorityUrl, true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("SystemToSystemTrustControllerTest: /api/SystemTrust/sync/applications | 500 InternalError\\nSystem.InvalidOperationException: Building Application Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
			}
		}

		public void TestSyncApplications_IncludeModule()
		{
			var application = InitializeEdiIdentityApplication(1);
			application.IDA_ApplicationModule = "Test111";
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("applications", 2, Guid.Empty, logger, true, token, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var applicationReponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<IdentityApplicationModel>>(response.Content.ReadAsStringAsync().Result);
				AssertEquals(2, applicationReponse.Items.Count);
				var applicationReponseWithModule = applicationReponse.Items.FirstOrDefault(x => x.PK == application.PK.ToGuid());
				AssertEquals(application.IDA_ApplicationModule, applicationReponseWithModule.ApplicationModule);
			}
		}

		#endregion

		#region OrgHeader

		public void TestSyncOrganizations()
		{
			var ifNeedToInitialize = false;
			var orgHeaders = new List<EDIOrgHeader>();
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.OrderBy = OrgHeaderSchema.Constants.PK;
			query.MaximumRows = 5;

			orgHeaders = Factory.Load<EDIOrgHeader>(query).ToList();
			if (orgHeaders.Count == 0)
			{
				for (int i = 0; i < 5; i++)
				{
					var orgHeader = InitializeOrganization(i);
					orgHeaders.Add(orgHeader);
				}
				ifNeedToInitialize = true;
				Factory.Save();
			}

			var sortedOrgHeaders = orgHeaders.OrderBy(x => x.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);

				var response = InvokeSyncRoute("organizations", 2, sortedOrgHeaders[0], logger, true, token, authorityUrl);
				AssertSyncRoute<OrgHeaderModel>(response, sortedOrgHeaders, 1);

				response = InvokeSyncRoute("organizations", 2, sortedOrgHeaders[2], logger, true, token, authorityUrl);
				AssertSyncRoute<LicenceEnterpriseModel>(response, sortedOrgHeaders, 3);

				var response2 = new HttpResponseMessage();

				if (!ifNeedToInitialize)
				{
					query.OrderBy = OrgHeaderSchema.Constants.PK + OrderByClause.Descending;
					query.MaximumRows = 5;

					orgHeaders = Factory.Load<EDIOrgHeader>(query).ToList();
					response = InvokeSyncRoute("organizations", 2, orgHeaders[0].PK.ToGuid(), logger, true, token, authorityUrl);
					response2 = InvokeSyncRoute("organizations", 10, orgHeaders[4].PK.ToGuid(), logger, true, token, authorityUrl);
				}
				else
				{
					response = InvokeSyncRoute("organizations", 2, sortedOrgHeaders[4], logger, true, token, authorityUrl);
					response2 = InvokeSyncRoute("organizations", 10, orgHeaders[0].PK.ToGuid(), logger, true, token, authorityUrl);
				}
				AssertSyncRouteWhenLastPKIsLatest<OrgHeaderModel>(response);
				AssertSyncRouteWithExpectedCount<OrgHeaderModel>(response2, expectCount: 4);
			}
		}

		public void TestSyncOrganizations_LimitedRowsInRegistry()
		{
			var orgHeaders = new List<OrgHeader>();
			for (int i = 0; i < 6; i++)
			{
				var orgHeader = InitializeOrganization(i);
				orgHeaders.Add(orgHeader);
				Factory.Save();
			}

			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("organizations", 6, Guid.Empty, logger, true, token, authorityUrl);
				AssertSyncRouteWithExpectedCount<OrgHeaderModel>(response, false);
			}
		}

		public void TestSyncOrganizations_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				//No Token
				var response = InvokeSyncRoute("organizations", 6, Guid.Empty, logger, true, string.Empty, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());

				response = InvokeSyncRoute("organizations", 6, Guid.Empty, logger, true, "1234", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestSyncOrganizations_WithException()
		{
			var orgHeaders = new List<OrgHeader>();
			for (int i = 0; i < 6; i++)
			{
				var orgHeader = InitializeOrganization(i);
				orgHeaders.Add(orgHeader);
				Factory.Save();
			}

			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("organizations", 6, Guid.Empty, logger, true, token, authorityUrl, true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("SystemToSystemTrustControllerTest: /api/SystemTrust/sync/organizations | 500 InternalError\\nSystem.InvalidOperationException: Building OrgHeader Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		#region LicenceEnterprise

		public void TestSyncLicenceEnterprises()
		{
			var enterprises = new List<LicenceEnterprise>();

			for (int i = 0; i < 4; i++)
			{
				var enterprise = InitializeLicenceEnterprise(i);
				enterprises.Add(enterprise);
				Factory.Save();
			}
			enterprises.Add(Database.LicEnterprise);

			var sortedEnterprises = enterprises.OrderBy(x => x.PK.ToGuid(), new GuidAsSqlGuidComparer()).Select(x => x.PK.ToGuid()).ToList();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("licenceEnterprises", 2, sortedEnterprises[0], logger, true, token, authorityUrl);
				AssertSyncRoute<LicenceEnterpriseModel>(response, sortedEnterprises, 1);
				response = InvokeSyncRoute("licenceEnterprises", 2, sortedEnterprises[2], logger, true, token, authorityUrl);
				AssertSyncRoute<LicenceEnterpriseModel>(response, sortedEnterprises, 3);
				response = InvokeSyncRoute("licenceEnterprises", 2, sortedEnterprises[4], logger, true, token, authorityUrl);
				AssertSyncRouteWhenLastPKIsLatest<LicenceEnterpriseModel>(response);
				response = InvokeSyncRoute("licenceEnterprises", 10, Guid.Empty, logger, true, token, authorityUrl);
				AssertSyncRouteWithExpectedCount<LicenceEnterpriseModel>(response);
			}
		}

		public void TestSyncLicenceEnterprises_LimitedRowsInRegistry()
		{
			var enterprises = new List<LicenceEnterprise>();
			for (int i = 0; i < 6; i++)
			{
				var enterprise = InitializeLicenceEnterprise(i);
				enterprises.Add(enterprise);
			}
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("licenceEnterprises", 6, Guid.Empty, logger, true, token, authorityUrl);
				AssertSyncRouteWithExpectedCount<LicenceEnterpriseModel>(response, false);
			}
		}

		public void TestSyncLicenceEnterprises_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				//No Token
				var response = InvokeSyncRoute("licenceEnterprises", 6, Guid.Empty, logger, true, string.Empty, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());

				response = InvokeSyncRoute("licenceEnterprises", 6, Guid.Empty, logger, true, "1234", authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestSyncLicenceEnterprises_WithException()
		{
			var enterprises = new List<LicenceEnterprise>();
			for (int i = 0; i < 6; i++)
			{
				var enterprise = InitializeLicenceEnterprise(i);
				enterprises.Add(enterprise);
			}
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (EDIDataRegistry.Instance.MaxRowsForDistributedDataSyncRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var azp = identityServer.ClientIdentifier;
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, azp, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeSyncRoute("licenceEnterprises", 6, Guid.Empty, logger, true, token, authorityUrl, true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("SystemToSystemTrustControllerTest: /api/SystemTrust/sync/licenceEnterprises | 500 InternalError\\nSystem.InvalidOperationException: Building LicenceEnterprise Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		#region CheckIfApplicationExists

		public void TestCheckIfApplicationExists()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			Factory.Save();

			var response = InvokeCheckIfApplicationExists("clientIdDoesNotExist", logger);
			AssertEquals(HttpStatusCode.NotFound, response.StatusCode);

			response = InvokeCheckIfApplicationExists(clientId, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
		}

		public void TestCheckIfApplicationExists_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			Factory.Save();

			var response = InvokeCheckIfApplicationExists("clientIdDoesNotExist", logger, needToThrowException: true);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Checking Application Failed.", logger.ToString());
			var content = response.Content.ReadAsStringAsync().Result;
			AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
		}

		#endregion

		public void TestUseCorrectAuthorityUrl()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var controller = new SystemToSystemTrustControllerForTest(logger))
			{
				var authorityUrl = controller.GetAuthorityUrlForTest();
				AssertContains("Registry 'WiseTech Global Client Extensions -> Azure Application Management -> Azure Application Management Tenant ID' is not overridden.", logger.ToString());
			}

			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TenantId))
			using (var controller = new SystemToSystemTrustControllerForTest(logger))
			{
				var authorityUrl = controller.GetAuthorityUrlForTest();
				AssertEquals("https://login.microsoftonline.com/804a70cf-4a61-4a08-908e-afdf43432443/v2.0/", authorityUrl);
			}
		}

		#region UpdateApplicationRedirectUrls

		[TestDate(2024, 02, 29)]
		[TestUtcOffset(0, 0, 0)]
		public void TestUpdateApplicationRedirectUrls()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var now = ZDateTime.UtcNow;
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var ediIdentityApplication = Factory.New<EdiIdentityApplication>();
				ediIdentityApplication.IDA_ClientID = identityServer.ClientIdentifier;
				ediIdentityApplication.IDA_ApplicationName = "Quince.Test";
				ediIdentityApplication.IDA_IDT = tenant.PK;
				var existingEdiIdentityRedirectUrlList = new List<EdiIdentityRedirectUrl>();
				for (var i = 0; i < 4; i++)
				{
					var existingEdiIdentityRedirectUrl = InitializeEdiIdentityRedirectUrl(ediIdentityApplication.PK, i, now);
					existingEdiIdentityRedirectUrlList.Add(existingEdiIdentityRedirectUrl);
				}

				Factory.Save();

				AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged, ediIdentityApplication.IDA_RedirectUrlStatus);

				ediIdentityApplication.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.None;
				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				var identityRedirectUrlRequestArray = new IdentityRedirectUrlRequest[]
				{
					new IdentityRedirectUrlRequest() { ApplicationName = "Test0", RedirectUrlType = EdiIdentityRedirectType.Codes.Web, RedirectUrl = "http://www.test0.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test11", RedirectUrlType = EdiIdentityRedirectType.Codes.Web, RedirectUrl = "http://www.test1.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test2", RedirectUrlType = EdiIdentityRedirectType.Codes.InstalledClient, RedirectUrl = "http://www.test2.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test3", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage, RedirectUrl = "http://www.test3.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test4", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage, RedirectUrl = "http://www.test4.com" }
				};

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/application/redirecturls";
				var response = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				ediIdentityApplication.ReloadSafe();

				var logEntries = logger.LogEntries.ToList();

				AssertEquals(1, logEntries.Count);

				var deleteLogs = ediIdentityApplication.Logs
					.Find(a => a.SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode).ToArray();

				var deleteMessage1 = "Deleted : ApplicationName=Test3|RedirectUrl=http://www.test3.com|RedirectType=WEB";
				var deleteMessage2 = "Deleted : ApplicationName=Test2|RedirectUrl=http://www.test2.com|RedirectType=WEB";
				var deleteMessage3 = "Deleted : ApplicationName=Test1|RedirectUrl=http://www.test1.com|RedirectType=WEB";

				AssertEquals(3, deleteLogs.Length);
				AssertNotNull(deleteLogs.FirstOrDefault(a => a.SL_Reference == deleteMessage1));
				AssertNotNull(deleteLogs.FirstOrDefault(a => a.SL_Reference == deleteMessage2));
				AssertNotNull(deleteLogs.FirstOrDefault(a => a.SL_Reference == deleteMessage3));

				var edtLogs = ediIdentityApplication.Logs
					.Find(a => a.SL_SE_NKEvent == Events.EditedARecordCode).ToArray();

				var statusChangeMessage = "Status : NON to NUD.";
				AssertNotNull(edtLogs.FirstOrDefault(a => a.SL_Reference == statusChangeMessage));

				var addLogs = ediIdentityApplication.Logs
					.Find(a => a.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).ToArray();

				AssertEquals(9, addLogs.Length);

				var addNewRedirectUrlMessage1 =
					$"Added : ApplicationName={identityRedirectUrlRequestArray[1].ApplicationName}|RedirectUrl={identityRedirectUrlRequestArray[1].RedirectUrl}|RedirectType={identityRedirectUrlRequestArray[1].RedirectUrlType}";
				AssertNotNull(addLogs.FirstOrDefault(a => a.SL_Reference == addNewRedirectUrlMessage1));

				var addNewRedirectUrlMessage2 =
					$"Added : ApplicationName={identityRedirectUrlRequestArray[2].ApplicationName}|RedirectUrl={identityRedirectUrlRequestArray[2].RedirectUrl}|RedirectType={identityRedirectUrlRequestArray[2].RedirectUrlType}";
				AssertNotNull(addLogs.FirstOrDefault(a => a.SL_Reference == addNewRedirectUrlMessage2));

				var addNewRedirectUrlMessage3 =
					$"Added : ApplicationName={identityRedirectUrlRequestArray[3].ApplicationName}|RedirectUrl={identityRedirectUrlRequestArray[3].RedirectUrl}|RedirectType={identityRedirectUrlRequestArray[3].RedirectUrlType}";
				AssertNotNull(addLogs.FirstOrDefault(a => a.SL_Reference == addNewRedirectUrlMessage3));

				var addNewRedirectUrlMessage4 =
					$"Added : ApplicationName={identityRedirectUrlRequestArray[4].ApplicationName}|RedirectUrl={identityRedirectUrlRequestArray[4].RedirectUrl}|RedirectType={identityRedirectUrlRequestArray[4].RedirectUrlType}";
				AssertNotNull(addLogs.FirstOrDefault(a => a.SL_Reference == addNewRedirectUrlMessage4));

				var expectedMessage = $"SystemToSystemTrustControllerTest: {ediIdentityApplication.IDA_ClientID} | 200 Ok";
				AssertContains(expectedMessage, logEntries[0]);

				var newEdiIdentityRedirectUrlList = new BusinessObjectFactory().Load<EdiIdentityRedirectUrl>(new ZQuery(EdiIdentityRedirectUrlSchema.IAR_IDA, ediIdentityApplication.PK));
				AssertEquals(5, newEdiIdentityRedirectUrlList.Length);

				AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged, ediIdentityApplication.IDA_RedirectUrlStatus);

				AssertEquals(1, newEdiIdentityRedirectUrlList.Count(a => a.IAR_ApplicationName == identityRedirectUrlRequestArray[0].ApplicationName
																		 && a.IAR_RedirectType == identityRedirectUrlRequestArray[0].RedirectUrlType
																		 && a.IAR_RedirectUrl == identityRedirectUrlRequestArray[0].RedirectUrl));

				AssertEquals(1, newEdiIdentityRedirectUrlList.Count(a => a.IAR_ApplicationName == identityRedirectUrlRequestArray[1].ApplicationName &&
																		 a.IAR_RedirectType == identityRedirectUrlRequestArray[1].RedirectUrlType
																		 && a.IAR_RedirectUrl == identityRedirectUrlRequestArray[1].RedirectUrl));

				AssertEquals(1, newEdiIdentityRedirectUrlList.Count(a => a.IAR_ApplicationName == identityRedirectUrlRequestArray[2].ApplicationName
																		 && a.IAR_RedirectType == identityRedirectUrlRequestArray[2].RedirectUrlType
																		 && a.IAR_RedirectUrl == identityRedirectUrlRequestArray[2].RedirectUrl));

				AssertEquals(1, newEdiIdentityRedirectUrlList.Count(a => a.IAR_ApplicationName == identityRedirectUrlRequestArray[3].ApplicationName
																		 && a.IAR_RedirectType == identityRedirectUrlRequestArray[3].RedirectUrlType
																		 && a.IAR_RedirectUrl == identityRedirectUrlRequestArray[3].RedirectUrl));

				AssertEquals(1, newEdiIdentityRedirectUrlList.Count(a => a.IAR_ApplicationName == identityRedirectUrlRequestArray[4].ApplicationName
																		 && a.IAR_RedirectType == identityRedirectUrlRequestArray[4].RedirectUrlType
																		 && a.IAR_RedirectUrl == identityRedirectUrlRequestArray[4].RedirectUrl));
			}
		}

		[TestDate(2024, 02, 29)]
		[TestUtcOffset(0, 0, 0)]
		public void TestUpdateApplicationRedirectUrls_DoesNotTriggerChangingDuringEnumeration()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var now = ZDateTime.UtcNow;
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var ediIdentityApplication = Factory.New<EdiIdentityApplication>();
				ediIdentityApplication.IDA_ClientID = identityServer.ClientIdentifier;
				ediIdentityApplication.IDA_ApplicationName = "Quince.Test";
				ediIdentityApplication.IDA_IDT = tenant.PK;
				var existingEdiIdentityRedirectUrl = InitializeEdiIdentityRedirectUrl(ediIdentityApplication.PK, 1, now);

				Factory.Save();

				AssertEquals(1, ediIdentityApplication.RedirectUrls.Count);

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				var identityRedirectUrlRequestArray = Array.Empty<IdentityRedirectUrlRequest>();

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/application/redirecturls";
				var response = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);

				var redirectUrlReload = new BusinessObjectFactory().Load<EdiIdentityRedirectUrl>(existingEdiIdentityRedirectUrl.PK);
				AssertNull("The redirect url is removed.", redirectUrlReload);

				ediIdentityApplication.ReloadSafe();
				var deleteLogs = ediIdentityApplication.Logs
					.Find(a => a.SL_SE_NKEvent == Events.DeletedARecordInTheSystemCode).ToArray();

				var deleteMessage = "Deleted : ApplicationName=Test1|RedirectUrl=http://www.test1.com|RedirectType=WEB";

				AssertEquals(1, deleteLogs.Length);
				AssertNotNull(deleteLogs.First(a => a.SL_Reference == deleteMessage));
			}
		}

		[TestDate(2024, 02, 29)]
		[TestUtcOffset(0, 0, 0)]
		public void TestUpdateApplicationRedirectUrlsWithoutHttpSuccessCode()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var now = ZDateTime.UtcNow;
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var ediIdentityApplication = Factory.New<EdiIdentityApplication>();
				ediIdentityApplication.IDA_ClientID = identityServer.ClientIdentifier;
				ediIdentityApplication.IDA_ApplicationName = "Quince.Test";
				ediIdentityApplication.IDA_IDT = tenant.PK;
				var existingEdiIdentityRedirectUrlList = new List<EdiIdentityRedirectUrl>();
				for (var i = 0; i < 4; i++)
				{
					var existingEdiIdentityRedirectUrl = InitializeEdiIdentityRedirectUrl(ediIdentityApplication.PK, i, now);
					existingEdiIdentityRedirectUrlList.Add(existingEdiIdentityRedirectUrl);
				}

				Factory.Save();

				var identityRedirectUrlRequestArray = new IdentityRedirectUrlRequest[]
				{
					new IdentityRedirectUrlRequest() { ApplicationName = "Test0", RedirectUrlType = EdiIdentityRedirectType.Codes.Web, RedirectUrl = "http://www.test0.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test11", RedirectUrlType = EdiIdentityRedirectType.Codes.Web, RedirectUrl = "http://www.test1.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test2", RedirectUrlType = EdiIdentityRedirectType.Codes.InstalledClient, RedirectUrl = "http://www.test2.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test3", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage, RedirectUrl = "http://www.test3.com" },
					new IdentityRedirectUrlRequest() { ApplicationName = "Test4", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage, RedirectUrl = "http://www.test4.com" }
				};

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/application/redirecturls";
				var response = InvokeUpdateRedirectUrl(null, "", logger, uri);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());

				logger.ClearLog();
				var response2 = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, "Invalid Access Token", logger, uri);
				AssertEquals(HttpStatusCode.BadRequest, response2.StatusCode);
				var message2 = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
				AssertContains(message2, logger.ToString());

				logger.ClearLog();
				var token = GetAccessToken(authorityUrl, ZGuid.NewZGuid().ToString(), ZGuid.NewZGuid().ToString());
				var response3 = InvokeUpdateRedirectUrl(null, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response3.StatusCode);
				var message3 = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
				AssertContains(message3, logger.ToString());

				logger.ClearLog();
				token = GetAccessToken(authorityUrl, string.Empty, string.Empty);
				var response4 = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response4.StatusCode);
				var message4 = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token client id is empty";
				AssertContains(message4, logger.ToString());

				logger.ClearLog();
				token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, string.Empty);
				response4 = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response4.StatusCode);
				message4 = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token tenant id is empty";
				AssertContains(message4, logger.ToString());

				logger.ClearLog();
				var inValidTenantId = ZGuid.NewZGuid().ToString();
				token = GetAccessToken(authorityUrl, ediIdentityApplication.IDA_ClientID.ToString(), inValidTenantId);
				var response6 = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response6.StatusCode);
				var message6 = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{inValidTenantId}' and client id '{ediIdentityApplication.IDA_ClientID}'";
				AssertContains(message6, logger.ToString());
			}
		}

		public void TestUpdateApplicationRedirectUrls_WithException()
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
				var ediIdentityApplication = Factory.New<EdiIdentityApplication>();
				ediIdentityApplication.IDA_ClientID = identityServer.ClientIdentifier;
				ediIdentityApplication.IDA_ApplicationName = "Quince.Test";
				ediIdentityApplication.IDA_IDT = tenant.PK;

				Factory.Save();

				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				var identityRedirectUrlRequestArray = Array.Empty<IdentityRedirectUrlRequest>();

				var logger = new NLogWrapperForTest(GetType());
				var uri = "https://unit-testing/api/SystemTrust/application/redirecturls";
				var response = InvokeUpdateRedirectUrl(identityRedirectUrlRequestArray, token, logger, uri, authorityUrl, needToThrowException: true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Query Application From Token Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error saving record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		#region LoadDatabaseNumberByClientId

		public void TestLoadDatabaseNumberByClientId()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_LD = Database.PK;
			application.IDA_IDT = tenant.PK;
			Factory.Save();

			var uri = "https://unit-testing/api/SystemTrust/load/databasenumber";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, clientId, tenant.IDT_TenantId.ToString());

				var response1 = InvokeLoadDatabaseNumberByClientId(token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.OK, response1.StatusCode);

				var message = $"SystemToSystemTrustControllerTest: 8000 | {NLogWrapper.Status.Ok}";
				AssertContains(message, logger.ToString());

				var databaseNumber = response1.Content.ReadAsStringAsync().Result;
				AssertEquals(Database.LD_DatabaseNumber.ToString(), databaseNumber);
			}
		}

		public void TestLoadDatabaseNumberByClientId_WithError()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_IDT = tenant.PK;
			Factory.Save();
			var uri = "https://unit-testing/api/SystemTrust/load/databasenumber";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				//No Token
				var response = InvokeLoadDatabaseNumberByClientId(string.Empty, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());
				//ClientId is Empty
				logger.ClearLog();
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, string.Empty, SystemDataRegistry.Instance.EDIClientID.Value);
				response = InvokeLoadDatabaseNumberByClientId(token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token client id is empty";
				AssertContains(message, logger.ToString());
				//ClientId is Invalid
				logger.ClearLog();
				token = GetAccessToken(authorityUrl, identityServer.ClientIdentifier, tenant.IDT_TenantId.ToString());
				response = InvokeLoadDatabaseNumberByClientId(token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenant.IDT_TenantId}' and client id '{identityServer.ClientIdentifier}'";
				AssertContains(message, logger.ToString());
				//No Database
				logger.ClearLog();
				token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), tenant.IDT_TenantId.ToString());
				response = InvokeLoadDatabaseNumberByClientId(token, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid licence info {clientId}";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestLoadDatabaseNumber_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());
			var clientId = Guid.NewGuid().ToString();
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = clientId;
			application.IDA_LD = Database.PK;
			application.IDA_IDT = tenant.PK;
			Factory.Save();

			var uri = "https://unit-testing/api/SystemTrust/load/databasenumber";

			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, clientId, tenant.IDT_TenantId.ToString());

				var response = InvokeLoadDatabaseNumberByClientId(token, logger, uri, authorityUrl, needToThrowException: true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("/api/SystemTrust/load/databasenumber | 500 InternalError\\nSystem.InvalidOperationException: Query Application From Token Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		#region Lsn

		public void TestGetLsn()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/lsn";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, identityServer.ClientIdentifier, SystemDataRegistry.Instance.EDIClientID.Value);
				var response1 = InvokeGetLsn(token, logger, uri, authorityUrl);

				var message = $"api/SystemTrust/lsn | {NLogWrapper.Status.Ok}";
				AssertContains(message, logger.ToString());

				AssertEquals(HttpStatusCode.OK, response1.StatusCode);

				var lsn = response1.Content.ReadAsByteArrayAsync().Result;
				var lsnStr = BitConverter.ToString(lsn).Replace("-", "").ToLowerInvariant();
				AssertNotNullOrEmpty(lsnStr);
				AssertEquals(lsnStr, "0102030405");
			}
		}

		public void TestGetLsn_Failed()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/lsn";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				//No Token
				var response = InvokeGetLsn(string.Empty, logger, uri, authorityUrl);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
				AssertContains(message, logger.ToString());
			}
		}

		public void TestGetLsn_WithException()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/lsn";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, identityServer.ClientIdentifier, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeGetLsn(token, logger, uri, authorityUrl, needToThrowException: true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("SystemToSystemTrustControllerTest: /api/SystemTrust/lsn | 500 InternalError\\nSystem.InvalidOperationException: Gen Lsn Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error query record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		#region Oidc RedirectUrl

		readonly string oidcRedirectUrls = "https://unit-testing/api/SystemTrust/application/oidcredirecturls";

		public void TestOidcRedirectUrls_InvalidToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var response = InvokeUpdateOidcRedirectUrl(string.Empty, null, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
					AssertContains(message, logger.ToString());
				});

				logger.ClearLog();
				response = InvokeUpdateOidcRedirectUrl("invalidaccesstoken", null, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_MissingAuthorityUrlFromRequestBody()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				var token = GetAccessToken(authorityUrl, application.IDA_ClientID, s2stTenant.IDT_TenantId);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = ZGuid.NewZGuid().ToString(), AuthorityUrl = string.Empty };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("The required field is missing.", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_MissingClientIdFromRequestBody()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				var token = GetAccessToken(authorityUrl, application.IDA_ClientID, s2stTenant.IDT_TenantId);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = string.Empty, AuthorityUrl = "http://authority.com" };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("The required field is missing.", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_EmptyClientId()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, string.Empty, SystemDataRegistry.Instance.EDIClientID.Value);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = ZGuid.NewZGuid().ToString(), AuthorityUrl = "http://authority.com" };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token client id is empty";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_EmptyTenantId()
		{
			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, ZGuid.NewZGuid().ToString(), string.Empty);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = ZGuid.NewZGuid().ToString(), AuthorityUrl = "https://authorityurl.com" };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token tenant id is empty";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_InvalidClientId()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var clientId = ZGuid.NewZGuid().ToString();
				var tenantId = ZGuid.NewZGuid().ToString();
				var token = GetAccessToken(authorityUrl, clientId, tenantId);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = ZGuid.NewZGuid().ToString(), AuthorityUrl = "http://authority.com" };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenantId}' and client id '{clientId}'";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_InvalidTenantId()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var tenantId = ZGuid.NewZGuid().ToString();

				var token = GetAccessToken(authorityUrl, application.IDA_ClientID, tenantId);
				var requestBody = new IdentityOidcRedirectUrlRequest() { ClientId = ZGuid.NewZGuid().ToString(), AuthorityUrl = "http://authority.com" };
				var response = InvokeUpdateOidcRedirectUrl(token, requestBody, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenantId}' and client id '{application.IDA_ClientID}'";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_InvalidLicenceDatabaseOfTheTokenApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var listApplicationIds = new List<string>();
			foreach (var item in nonEnterpriseProducts)
			{
				var clientId = ZGuid.NewZGuid().ToString();
				var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licenceDatabase.LD_Product = item;
				var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
				application.IDA_ClientID = clientId;
				application.IDA_IDT = s2stTenant.PK;
				application.IDA_LD = licenceDatabase.PK;
				listApplicationIds.Add(clientId);
			}
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				foreach (var applicationId in listApplicationIds)
				{
					var token = GetAccessToken(authorityUrl, applicationId, s2stTenant.IDT_TenantId.ToString());

					var request = new IdentityOidcRedirectUrlRequest()
					{
						AuthorityUrl = "http://authority.com",
						ClientId = applicationId,
						RedirectUrls = new List<IdentityRedirectUrl>()
					};
					var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl);
					CombineAssertions(() =>
					{
						AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
						var responseString = response.Content.ReadAsStringAsync().Result;
						AssertContains("Invalid Access Token", responseString);
						var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid licence info {applicationId}";
						AssertContains(message, logger.ToString());
						logger.ClearLog();
					});
				}
			}
		}

		public void TestOidcRedirectUrls_InvalidAuthorityUrl()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			s2stTenant.IDT_AuthorityUrl = "http://authority.com";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var request = new IdentityOidcRedirectUrlRequest()
				{
					AuthorityUrl = "http://oidcauthority.com",
					ClientId = application.IDA_ClientID,
					RedirectUrls = new List<IdentityRedirectUrl>()
				};
				var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Authority URL", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Authority URL '{request.AuthorityUrl}'";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_NotSameLicenseDatabase_RejectWithInvalidOidcApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			s2stTenant.IDT_AuthorityUrl = "http://authority.com";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			oidcTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			oidcTenant.IDT_AuthorityUrl = "http://oidcauthority.com";
			var oidcLicenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var oidcApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			oidcApplication.IDA_ClientID = ZGuid.NewZGuid().ToString();
			oidcApplication.IDA_LD = oidcLicenceDatabase.PK;
			oidcApplication.IDA_IDT = oidcTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var request = new IdentityOidcRedirectUrlRequest()
				{
					AuthorityUrl = "http://oidcauthority.com",
					ClientId = oidcApplication.IDA_ClientID,
					RedirectUrls = new List<IdentityRedirectUrl>()
				};
				var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Client Id", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid OIDC application {oidcApplication.IDA_ClientID}";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_NotSameTenant_RejectWithInvalidOidcApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			s2stTenant.IDT_AuthorityUrl = "http://authority.com";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			oidcTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			oidcTenant.IDT_AuthorityUrl = "http://oidcauthority.com";
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var request = new IdentityOidcRedirectUrlRequest()
				{
					AuthorityUrl = "http://oidcauthority.com",
					ClientId = application.IDA_ClientID,
					RedirectUrls = new List<IdentityRedirectUrl>()
				};
				var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Client Id", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid OIDC application {application.IDA_ClientID}";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRedirectUrls_Successful()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			s2stTenant.IDT_AuthorityUrl = "http://authority.com";
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseNext;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			oidcTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			oidcTenant.IDT_AuthorityUrl = "http://oidcauthority.com";
			var oidcApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			oidcApplication.IDA_ClientID = ZGuid.NewZGuid().ToString();
			oidcApplication.IDA_LD = licenceDatabase.PK;
			oidcApplication.IDA_IDT = oidcTenant.PK;
			var redirectUrl1 = oidcApplication.RedirectUrls.AddNew();
			redirectUrl1.IAR_ApplicationName = "Test1";
			redirectUrl1.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrl1.IAR_RedirectUrl = "http://www.test1.com";
			var redirectUrl2 = oidcApplication.RedirectUrls.AddNew();
			redirectUrl2.IAR_ApplicationName = "Test2";
			redirectUrl2.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			redirectUrl2.IAR_RedirectUrl = "http://www.test2.com";
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var request = new IdentityOidcRedirectUrlRequest()
				{
					AuthorityUrl = "http://oidcauthority.com",
					ClientId = oidcApplication.IDA_ClientID,
					RedirectUrls = new List<IdentityRedirectUrl>()
					{
						new IdentityRedirectUrl() { ApplicationName = "Test1", RedirectUrl = "http://www.test1.com", RedirectUrlType = EdiIdentityRedirectType.Codes.Web },
						new IdentityRedirectUrl() { ApplicationName = "Test3", RedirectUrl = "http://www.test3.com", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage },
					}
				};
				var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
					AssertNull(response.Content);
					var message = $"SystemToSystemTrustControllerTest: {oidcApplication.IDA_ClientID} | {NLogWrapper.Status.Ok}";
					AssertContains(message, logger.ToString());

					var oidcApplicationReload = new BusinessObjectFactory() { RefreshEnabled = false }.Load<EdiIdentityApplication>(oidcApplication.PK);
					var expectedRedirectUrls = new string[]
					{
						JsonConvert.SerializeObject(new IdentityRedirectUrlRequest() { ApplicationName = "Test1", RedirectUrl = "http://www.test1.com", RedirectUrlType = EdiIdentityRedirectType.Codes.Web }),
						JsonConvert.SerializeObject(new IdentityRedirectUrlRequest() { ApplicationName = "Test3", RedirectUrl = "http://www.test3.com", RedirectUrlType = EdiIdentityRedirectType.Codes.SinglePage }),
					};

					var actualRedirectUrls = oidcApplicationReload.RedirectUrls.Select(a => JsonConvert.SerializeObject(new IdentityRedirectUrlRequest() { ApplicationName = a.IAR_ApplicationName, RedirectUrl = a.IAR_RedirectUrl, RedirectUrlType = a.IAR_RedirectType })).ToArray();
					AssertContainsExactElementsInAnyOrder(expectedRedirectUrls, actualRedirectUrls);
				});
			}
		}

		public void TestOidcRedirectUrls_WithException()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var listApplicationIds = new List<string>();
			foreach (var item in nonEnterpriseProducts)
			{
				var clientId = ZGuid.NewZGuid().ToString();
				var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licenceDatabase.LD_Product = item;
				var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
				application.IDA_ClientID = clientId;
				application.IDA_IDT = s2stTenant.PK;
				application.IDA_LD = licenceDatabase.PK;
				listApplicationIds.Add(clientId);
			}
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				foreach (var applicationId in listApplicationIds)
				{
					var token = GetAccessToken(authorityUrl, applicationId, s2stTenant.IDT_TenantId.ToString());

					var request = new IdentityOidcRedirectUrlRequest()
					{
						AuthorityUrl = "http://authority.com",
						ClientId = applicationId,
						RedirectUrls = new List<IdentityRedirectUrl>()
					};
					var response = InvokeUpdateOidcRedirectUrl(token, request, oidcRedirectUrls, logger, authorityUrl, needToThrowException: true);
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					AssertContains("SystemToSystemTrustControllerTest: 500 InternalError\\nSystem.InvalidOperationException: Query Application From Token Failed.", logger.ToString());
					var content = response.Content.ReadAsStringAsync().Result;
					AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error saving record. Please contact the administrator.\"}]}", content);
				}
			}
		}

		#endregion

		#region OIDC Application Register

		public void TestOidcRegister_InlivadRequestBody()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
				requestMessage.Content = new StringContent(JsonConvert.SerializeObject(string.Empty));
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "accessToken");
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				using (var controller = new SystemToSystemTrustControllerForTest(logger, true, authorityUrl))
				{
					var response = ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));

					CombineAssertions(() =>
					{
						AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
						var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: The required field is missing.";
						AssertContains(message, logger.ToString());
					});
				}
			}
		}

		public void TestOidcRegister_InvalidToken()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var response = InvokeOidcRegister(string.Empty, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access Token Not Provided";
					AssertContains(message, logger.ToString());
				});

				logger.ClearLog();
				response = InvokeOidcRegister("invalidaccesstoken", authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Access Token";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_EmptyClientId()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, string.Empty, SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token client id is empty";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_EmptyTenantId()
		{
			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = MockJwtTokenProvider.GenerateClientAccessToken(authorityUrl, ZGuid.NewZGuid().ToString(), SystemDataRegistry.Instance.EDIClientID.Value);
				var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Access token tenant id is empty";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_InvalidApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var clientId = ZGuid.NewZGuid().ToString();
				var tenantId = ZGuid.NewZGuid().ToString();

				var token = GetAccessToken(authorityUrl, clientId, tenantId);
				var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenantId}' and client id '{clientId}'";
					AssertContains(message, logger.ToString());
				});

				token = GetAccessToken(authorityUrl, application.IDA_ClientID, tenantId);
				response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: No application found from tenant id '{tenantId}' and client id '{application.IDA_ClientID}'";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_EmptyLicenceDatabase()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains("Invalid Access Token", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid licence info {application.IDA_ClientID}";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_InvalidLicenceDatabase()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();

			var listApplicationIds = new List<string>();
			foreach (var item in nonEnterpriseProducts)
			{
				var clientId = ZGuid.NewZGuid().ToString();
				var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
				licenceDatabase.LD_Product = item;
				var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
				application.IDA_ClientID = clientId;
				application.IDA_IDT = s2stTenant.PK;
				application.IDA_LD = licenceDatabase.PK;
				listApplicationIds.Add(clientId);
			}
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";

				foreach (var applicationId in listApplicationIds)
				{
					var token = GetAccessToken(authorityUrl, applicationId, s2stTenant.IDT_TenantId.ToString());
					var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
					CombineAssertions(() =>
					{
						AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
						var responseString = response.Content.ReadAsStringAsync().Result;
						AssertContains("Invalid Access Token", responseString);
						var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid licence info {applicationId}";
						AssertContains(message, logger.ToString());
						logger.ClearLog();
					});
				}
			}
		}

		static readonly string[] nonEnterpriseProducts =
		[
			ProductTypes.Codes.GLOW,
			ProductTypes.Codes.EHub,
			ProductTypes.Codes.BorderWise,
			ProductTypes.Codes.CargoSphere,
			ProductTypes.Codes.WiseTechAcademy,
			ProductTypes.Codes.Telematics,
			ProductTypes.Codes.Sapphire,
			ProductTypes.Codes.WTGInternal,
		];

		public void TestOidcRegister_InvalidAuthorityUrl()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var response = InvokeOidcRegister(token, authorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
					var responseString = response.Content.ReadAsStringAsync().Result;
					AssertContains($"Invalid Authority URL", responseString);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.BadRequest}: Invalid Authority URL '{authorityUrl}'";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_CreateApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestApplication";
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcAuthorityUrl = "https://unit-testing.com/oidc";
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_AuthorityUrl = oidcAuthorityUrl;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var response = InvokeOidcRegister(token, oidcAuthorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
					var result = response.Content.ReadAsStringAsync().Result;
					AssertEquals(string.Empty, result);

					var query = new ZQuery();
					query.AddToFilter(EdiIdentityApplicationSchema.IDA_IDT, tenant.PK);
					query.AddToFilter(EdiIdentityApplicationSchema.IDA_LD, licenceDatabase.PK);

					var oidcApplications = Factory.Load<EdiIdentityApplication>(query);
					AssertContainsExactElementsInAnyOrder(["TestApplication_OIDC"], oidcApplications.Select(x => x.IDA_ApplicationName));
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.Ok}: OIDC application is not created in Azure yet {application.IDA_ClientID}";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_ReturnCreatedApplication()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestApplication";
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcAuthorityUrl = "https://unit-testing.com/oidc";
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_AuthorityUrl = oidcAuthorityUrl;

			var oidcAppId = ZGuid.NewZGuid().ToString();
			var oidcApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			oidcApplication.IDA_IDT = tenant.PK;
			oidcApplication.IDA_LD = licenceDatabase.PK;
			oidcApplication.IDA_ApplicationName = "TestApplication_OIDC";
			oidcApplication.IDA_ClientID = oidcAppId;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var response = InvokeOidcRegister(token, oidcAuthorityUrl, uri, logger, authorityUrl);
				CombineAssertions(() =>
				{
					AssertEquals(HttpStatusCode.OK, response.StatusCode);
					var result = response.Content.ReadAsStringAsync().Result;
					AssertEquals(oidcAppId, result);
					var message = $"SystemToSystemTrustControllerTest: {NLogWrapper.Status.Ok}: {oidcApplication.IDA_ClientID} {application.IDA_ClientID}";
					AssertContains(message, logger.ToString());
				});
			}
		}

		public void TestOidcRegister_WithException()
		{
			var s2stTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			s2stTenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.CargoWiseOne;
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestApplication";
			application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			application.IDA_LD = licenceDatabase.PK;
			application.IDA_IDT = s2stTenant.PK;
			var oidcAuthorityUrl = "https://unit-testing.com/oidc";
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_AuthorityUrl = oidcAuthorityUrl;
			Factory.Save();

			var logger = new NLogWrapperForTest(GetType());
			var uri = "https://unit-testing/api/SystemTrust/application/oidcregister";
			using (var identityServer = new MockOpenIDIdentityServer())
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var token = GetAccessToken(authorityUrl, application.IDA_ClientID.ToString(), s2stTenant.IDT_TenantId.ToString());
				var response = InvokeOidcRegister(token, oidcAuthorityUrl, uri, logger, authorityUrl, needToThrowException: true);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				AssertContains("/api/SystemTrust/application/oidcregister | 500 InternalError\\nSystem.InvalidOperationException: Query Application From Token Failed.", logger.ToString());
				var content = response.Content.ReadAsStringAsync().Result;
				AssertEquals("{\"errors\":[{\"code\":\"500\",\"message\":\"Error saving record. Please contact the administrator.\"}]}", content);
			}
		}

		#endregion

		string GetAccessToken(string authorityUrl, string azp, string tid)
		{
			var claims = new List<Claim>
			{
				new Claim("azp", azp),
				new Claim("tid", tid)
			};
			var token = MockJwtTokenProvider.GenerateValidJwtIDToken(claims, authorityUrl, SystemDataRegistry.Instance.EDIClientID.Value, Guid.NewGuid().ToString());
			return token;
		}

		void AssertSyncRoute<T1>(HttpResponseMessage response, List<Guid> sortedList, int skip)
			where T1 : DistributedDataModel
		{
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var licenceEnterpriseDataSyncResponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<T1>>(response.Content.ReadAsStringAsync().Result);

			AssertEquals(2, licenceEnterpriseDataSyncResponse.Items.Count);
			var reponseEnterprises = licenceEnterpriseDataSyncResponse.Items.Select(x => x.PK).ToList();
			var expectEnterprisse = sortedList.Skip(skip).Take(2).ToList();
			AssertContainsExactElementsInExactOrder(reponseEnterprises, expectEnterprisse);
		}

		void AssertSyncRouteWhenLastPKIsLatest<T1>(HttpResponseMessage response)
			where T1 : DistributedDataModel
		{
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var licenceEnterpriseDataSyncResponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<T1>>(response.Content.ReadAsStringAsync().Result);
			AssertEquals(0, licenceEnterpriseDataSyncResponse.Items.Count);
		}

		void AssertSyncRouteWithExpectedCount<T1>(HttpResponseMessage response, bool needToAssertSyncEnded = true, int expectCount = 5)
			where T1 : DistributedDataModel
		{
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var licenceEnterpriseDataSyncResponse = JsonConvert.DeserializeObject<DistributedDataSyncResponse<T1>>(response.Content.ReadAsStringAsync().Result);
			AssertEquals(expectCount, licenceEnterpriseDataSyncResponse.Items.Count);
			if (needToAssertSyncEnded)
			{
				Assert(licenceEnterpriseDataSyncResponse.SyncEnded);
			}
		}

		HttpResponseMessage InvokeSyncRoute(string route, long maxRow, Guid lastSyncPK, NLogWrapper logger, bool validateAccessToken, string accessToken = "", string authorityUrl = "", bool needToThrowException = false)
		{
			var requestUrl = $"https://unit-testing/api/SystemTrust/sync/{route}?maxRows={maxRow}&lastSyncPK={lastSyncPK}";
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateAccessToken, authorityUrl, needToThrowException: needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage RequestDownloadCertificates(string clientId, NLogWrapperForTest logger, bool needToThrowException = false)
		{
			using (var identityServer = new MockOpenIDIdentityServer())
			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TenantId))
			{
				var authorityUrl = $"https://{MockIdentityServerBase.HostName}:{identityServer.Port}";
				var claims = new List<Claim>() { new Claim("tid", TenantId), new Claim("azp", S2STApplication.IDA_ClientID) };
				var token = MockJwtTokenProvider.GenerateValidJwtIDToken(claims, authorityUrl, SystemDataRegistry.Instance.EDIClientID.Value, Guid.NewGuid().ToString());

				return InvokeDownloadCertificateByClientId(clientId, token, logger, authorityUrl, needToThrowException: needToThrowException);
			}
		}

		EdiIdentityApplication InitializeEdiIdentityApplication(int index)
		{
			var ediIdentityApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			ediIdentityApplication.IDA_ApplicationName = $"Test{index}";
			return ediIdentityApplication;
		}

		EDIOrgHeader InitializeOrganization(int index)
		{
			var header = Factory.NewWithValidTestData<EDIOrgHeader>();
			return header;
		}

		LicenceEnterprise InitializeLicenceEnterprise(int index)
		{
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "QD" + index;
			return enterprise;
		}

		EdiIdentityRedirectUrl InitializeEdiIdentityRedirectUrl(ZGuid applicationPk, int index, ZDateTime updateTime)
		{
			var ediIdentityRedirectUrl = Factory.New<EdiIdentityRedirectUrl>();
			ediIdentityRedirectUrl.IAR_IDA = applicationPk;
			ediIdentityRedirectUrl.IAR_ApplicationName = $"Test{index}";
			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = $"http://www.test{index}.com";
			ediIdentityRedirectUrl.IAR_SystemCreateUser = $"T{index}";
			ediIdentityRedirectUrl.IAR_SystemCreateTimeUtc = updateTime;
			ediIdentityRedirectUrl.IAR_SystemLastEditUser = $"T{index}";
			ediIdentityRedirectUrl.IAR_SystemLastEditTimeUtc = updateTime;
			return ediIdentityRedirectUrl;
		}

		EdiIdentityCertificate InitializeCertificate(ZDateTime createTime)
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_TenantId = ZGuid.NewZGuid().ToString();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_LD = Database.PK;
			application.IDA_IDT = tenant.PK;
			var certificate = application.Certificates.AddNew();
			certificate.ICE_CertificateData = certificateData.RawData;
			certificate.ICE_CertificateSigningRequest = csr;
			certificate.ICE_CertificateExpiryDate = ZDateTime.Now;
			certificate.ICE_CertificateIssuedBy = "Test";
			certificate.ICE_CertificateIssuedTo = "Test";
			certificate.ICE_CertificateThumbprint = certificateData.Thumbprint;
			certificate.ICE_CertificateValidDate = ZDateTime.Now;
			certificate.ICE_IsCertificateRevoked = false;
			certificate.ICE_IsActive = true;
			certificate.ICE_SystemCreateTimeUtc = createTime;
			Factory.Save();
			return certificate;
		}

		#region Execute

		HttpResponseMessage Execute(HttpRequestMessage request, NLogWrapper logger, bool needToThrowException = false)
		{
			using (var controller = new SystemToSystemTrustControllerForTest(logger, needToThrowException: needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, request, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeRequestCertificate(IdentityCertificateInitialRequest requestInfo, NLogWrapper logger, string requestUri, bool needToThrowException = false)
		{
			string queryString;
			if (requestInfo == null)
			{
				queryString = string.Empty;
			}
			else
			{
				var qs = new SecureQueryString { [nameof(IdentityCertificateInitialRequest)] = JsonConvert.SerializeObject(requestInfo) };
				queryString = qs.ToString();
			}

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(queryString);
			return Execute(requestMessage, logger, needToThrowException: needToThrowException);
		}

		HttpResponseMessage InvokeRegisterApplication(IdentityCertificateRegisterRequest request, string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "", bool validateToken = true)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request));
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeRequestCertificateWithoutModule(JObject requestData, NLogWrapper logger, string requestUri)
		{
			var qs = new SecureQueryString { [nameof(IdentityCertificateInitialRequest)] = requestData.ToString() };
			var queryString = qs.ToString();

			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(queryString);
			return Execute(requestMessage, logger);
		}

		HttpResponseMessage InvokeRolloverCertificate(IdentityCertificateRolloverRequest request, string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "",
			bool validateToken = true)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request));
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeDownloadCertificate(ZGuid operationId, NLogWrapper logger, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://unit-testing/api/SystemTrust/certificate/{operationId}");
			return Execute(requestMessage, logger, needToThrowException: needToThrowException);
		}

		HttpResponseMessage InvokeDownloadCertificateByClientId(string clientId, string accessToken, NLogWrapper logger, string authorityUrl = "",
			bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://unit-testing/api/SystemTrust/certificates/{clientId}");
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeCheckIfApplicationExists(string clientId, NLogWrapper logger, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://unit-testing/api/SystemTrust/application/{clientId}");
			return Execute(requestMessage, logger, needToThrowException: needToThrowException);
		}

		HttpResponseMessage InvokeLoadDatabaseNumberByClientId(string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "",
			bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(string.Empty);
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeSyncAllCertificate(long seqNum, long maxRow, NLogWrapper logger, bool validateAccessToken, string accessToken = "", string authorityUrl = "")
		{
			var requestUrl = $"https://unit-testing/api/SystemTrust/sync/certificates?seqNum={seqNum}&maxRows={maxRow}";
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateAccessToken, authorityUrl))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeUpdateOidcRedirectUrl(string accessToken, IdentityOidcRedirectUrlRequest oidcRedirectUrlRequest, string requestUri, NLogWrapperForTest logger, string authorityUrl = "", bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(oidcRedirectUrlRequest));
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeUpdateRedirectUrl(IdentityRedirectUrlRequest[] redirectUrls, string accessToken, NLogWrapper logger, string requestUri, string authorityUrl = "",
			bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(redirectUrls));
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeGetLsn(string accessToken, NLogWrapperForTest logger, string requestUri, string authorityUrl = "",
			bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		HttpResponseMessage InvokeOidcRegister(string accessToken, string oidcAuthorityUrl, string requestUri, NLogWrapperForTest logger, string authorityUrl = "", bool validateToken = true, bool needToThrowException = false)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(new IdentityOidcApplicationRequest() { AuthorityUrl = oidcAuthorityUrl }));
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using (var controller = new SystemToSystemTrustControllerForTest(logger, validateToken, authorityUrl, needToThrowException))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(SystemToSystemTrustController));
			}
		}

		#endregion Execute

		protected override void SetUp()
		{
			base.SetUp();

			var product = "CW1";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			Database = licence.Database;
			Database.LD_DatabaseNumber = 8000;
			Database.LD_Product = product;
			Database.LD_Password =
				"DE-3E-CC-F6-D3-2B-3E-58-E3-E1-55-80-F5-77-67-95-73-D9-1D-CD-C7-52-37-9A-C8-1A-E9-24-1B-23-75-7D-E9-B2-93-AB-BE-0E-2B-58-A8-8D-8B-64-03-33-95-DB-72-C2-1A-9C-1B-3F-BB-1E-8F-FB-5D-BE-B0-71-74-12";
			Database.LD_IsActive = true;
			Database.LD_Status = "REG";

			Tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			Tenant.IDT_TenantId = TenantId;
			Tenant.IDT_AuthorityUrl = authorityUrl;

			S2STApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			S2STApplication.IDA_ClientID = ZGuid.NewZGuid().ToString();
			S2STApplication.IDA_IDT = Tenant.PK;

			Factory.Save();

			var collection = new AWSPrivateCACollection();
			var awsPrivateCaArn1 = collection.AddNew();
			awsPrivateCaArn1.IsEnabled = true;
			awsPrivateCaArn1.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			awsPrivateCaArn1.Arn = "Arn";
			awsPrivateCaArn1.AccessKey = "AccessKey";
			awsPrivateCaArn1.SecretKey = "SecretKey";

			var awsPrivateCaArn2 = collection.AddNew();
			awsPrivateCaArn2.IsEnabled = true;
			awsPrivateCaArn2.IssuingCA = CARootCodeDescriptionList.Codes.Adaptor;
			awsPrivateCaArn2.Arn = "Arn";
			awsPrivateCaArn2.AccessKey = "AccessKey";
			awsPrivateCaArn2.SecretKey = "SecretKey";

			EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		LicenceDatabase Database;
		EdiIdentityTenant Tenant;
		EdiIdentityApplication S2STApplication;
		const string authorityUrl = "https://authorityUrl.com";
	}

	// The SQL query will order Guid with a different logic than the Guid implementation
	// see this link for details:  https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/comparing-guid-and-uniqueidentifier-values 
	class GuidAsSqlGuidComparer : IComparer<Guid>
	{
		public int Compare(Guid x, Guid y)
		{
			var sqlX = new SqlGuid(x);
			var sqlY = new SqlGuid(y);

			return sqlX.CompareTo(sqlY);
		}
	}

	class SystemToSystemTrustControllerForTest : SystemToSystemTrustController
	{
		public SystemToSystemTrustControllerForTest(NLogWrapper logger, bool validateToken = true, string authorityUrl = null, bool needToThrowException = false) : base(logger)
		{
			this.validateToken = validateToken;
			this.needToThrowException = needToThrowException;
			if (authorityUrl != null)
			{
				SetAuthorityUrl(authorityUrl);
			}
		}

		readonly bool validateToken;

		readonly bool needToThrowException;

		void SetAuthorityUrl(string authorityUrl)
		{
			TrustHelper = new SystemToSystemTrustHelperForTest(authorityUrl);
		}

		protected override bool ValidateRequestBearerToken(HttpRequestMessage request, string routingPath, string sessionId, out IHttpActionResult actionResult, out JwtSecurityToken securityToken)
		{
			if (validateToken)
			{
				return base.ValidateRequestBearerToken(request, routingPath, sessionId, out actionResult, out securityToken);
			}

			actionResult = null;
			securityToken = null;
			return true;
		}

		public string GetAuthorityUrlForTest()
		{
			return TrustHelper.GetAuthorityUrl(Logger);
		}

		protected override byte[] QueryLsn()
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Gen Lsn Failed.");
			}

			return new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
		}

		protected override IHttpActionResult RequestCertificate(string session, string queryString, string routingPath)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Request Certificate Failed.");
			}
			return base.RequestCertificate(session, queryString, routingPath);
		}

		protected override bool CheckIfApplicationExistsInDb(string clientId)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Checking Application Failed.");
			}
			return base.CheckIfApplicationExistsInDb(clientId);
		}

		protected override IdentityCertificateResponse BuildIdentityCertificateResponse(EdiIdentityCertificate certificate)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Building Certificate Response Failed.");
			}
			return base.BuildIdentityCertificateResponse(certificate);
		}

		protected override EdiIdentityApplication QueryApplicationFromTenantIdAndClientId(string tenantId, string clientId)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Query Application Failed.");
			}
			return base.QueryApplicationFromTenantIdAndClientId(tenantId, clientId);
		}

		protected override IdentityApplicationModel BuildIdentityApplicationModel(EdiIdentityApplication application)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Building Application Failed.");
			}
			return base.BuildIdentityApplicationModel(application);
		}

		protected override OrgHeaderModel BuildOrgHeaderModel(OrgHeader orgHeader)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Building OrgHeader Failed.");
			}
			return base.BuildOrgHeaderModel(orgHeader);
		}

		protected override LicenceEnterpriseModel BuildLicenceEnterpriseModel(LicenceEnterprise enterprise)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Building LicenceEnterprise Failed.");
			}
			return base.BuildLicenceEnterpriseModel(enterprise);
		}

		protected override EdiIdentityApplication QueryApplicationFromToken(string sessionId, string routingPath, JwtSecurityToken token)
		{
			if (needToThrowException)
			{
				throw new InvalidOperationException("Query Application From Token Failed.");
			}
			return base.QueryApplicationFromToken(sessionId, routingPath, token);
		}
	}
}

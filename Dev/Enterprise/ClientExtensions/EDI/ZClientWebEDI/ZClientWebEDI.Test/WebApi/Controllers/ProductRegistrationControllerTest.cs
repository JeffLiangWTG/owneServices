using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Newtonsoft.Json;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class ProductRegistrationControllerTest : TestCaseWithFactory
	{
		#region V1
		[HttpContextEnabledTest]
		public void TestRegister()
		{
			var logger = new NLogWrapperForTest(GetType());
			var (encryptedData, signature) = CreateRequest();
			var result = CallRegister(encryptedData, signature, "ABU", logger);
			AssertEquals(HttpStatusCode.OK, result.StatusCode);
			var responseSig = result.Headers.GetValues("SIGNED").Single();
			var responseMessage = result.Content.ReadAsStringAsync().Result;
			var decryptResponseMessage = TestHelper.DecryptRegistrationResponse(responseMessage, responseSig);
			var registrationResponse = JsonConvert.DeserializeObject<ProductRegistrationResponse>(decryptResponseMessage);
			AssertEquals(8000, registrationResponse.DatabaseNumber);
			AssertEquals("https://myaccount-portal.cargowise.com/myaccount/api/", registrationResponse.MyAccountEndpointBaseUrl);
			var db = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, 8000));
			AssertEquals("http://www.cw1.com", db.TrustedSystem.ETS_SystemEndpointUrl);
			var notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single();
			AssertEquals(@"Product:ABU
SystemID:SYS0399481
LicenceType:TST
SystemEndpointUrl:http://www.cw1.com", notes.ST_NoteDataAsText);
			var expectedLogMessages = new string[] { "api/ProductRegistration/Register start","success | 200 Ok" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		[HttpContextEnabledTest]
		public void TestRegister_BadRequests()
		{
			var logger = new NLogWrapperForTest(GetType());
			var (encryptedData, signature) = CreateRequest();
			var result = CallRegister(encryptedData + "something bad", "", "ABU", logger);
			var content = result.Content.ReadAsStringAsync().Result;
			var data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Message Not Signed", data.Messages[0].Message);
			var expectedLogMessages = new string[] { "api/ProductRegistration/Register start","1001 Message Not Signed" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			result = CallRegister(encryptedData + "something bad", signature, "ABU", logger);
			content = result.Content.ReadAsStringAsync().Result;
			data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertStartsWith("base64", "The input is not a valid Base-64 string", data.Messages[0].Message);
			expectedLogMessages = new string[] { "api/ProductRegistration/Register start","The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			result = CallRegister(Convert.ToBase64String(Encoding.UTF8.GetBytes("something bad")), signature, "ABU", logger);
			content = result.Content.ReadAsStringAsync().Result;
			data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Signature Verification Failed", data.Messages[0].Message);
			expectedLogMessages = new string[] { "api/ProductRegistration/Register start","1001 Signature Verification Failed" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			Database.LD_Status = "REG";
			Factory.Save();
			result = CallRegister(encryptedData, signature, "ABU", logger);
			content = result.Content.ReadAsStringAsync().Result;
			data = JsonConvert.DeserializeObject<ErrorMessages>(content);
			AssertEquals("Database Registered Already", data.Messages[0].Message);
			expectedLogMessages = new string[] { "api/ProductRegistration/Register start","2003 Database Registered Already" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestAppendAdditionalInfo()
		{
			var logger = new NLogWrapperForTest(GetType());
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(Database);
			Factory.Save();
			var request = CreateAdditionalInfoRequest(securityKeyTestHelper, new LicenceDatabaseRegistrationAdditionalInfo()
			{ Product = Database.LD_Product, SystemId = Database.LD_TenantID, TenantId = "Tenant#002", InfoExpires = DateTime.UtcNow.AddMinutes(10), Address1 = "Addr 1", Address2 = "Addr 2", BusinessRegoNo = "ABN #123", City = "Sydney", OrgCountry = "AU", OrgName = "Org Name 123", Postcode = "2000", State = "NSW", EnterpriseID = "enterprise_id_1", EnterpriseCode = "enterprise_code_2", ServerCode = "server_code_3", CargowiseCompanyCode = "cargowise_company_code_4", DatabaseNumber = "database_number_5", });
			var response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var db = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, 8000));
			var notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description).Single();
			AssertEquals(@"Product:ABU
SystemID:SYS0399481
TenantID:Tenant#002
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:database_number_5", notes.ST_NoteDataAsText);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestAppendAdditionalInfo_Validation()
		{
			var logger = new NLogWrapperForTest(GetType());
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(Database);
			Factory.Save();
			var info = new LicenceDatabaseRegistrationAdditionalInfo()
			{ Product = Database.LD_Product, SystemId = Database.LD_TenantID, InfoExpires = DateTime.UtcNow.AddMinutes(10), Address1 = "Addr 1", Address2 = "Addr 2", BusinessRegoNo = "ABN #123", City = "Sydney", OrgName = "Org Name 123", Postcode = "2000", State = "NSW" };
			var request = CreateAdditionalInfoRequest(securityKeyTestHelper, info);
			var response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 No Country or Licencing Info" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			info.OrgCountry = "AU";
			request = CreateAdditionalInfoRequest(securityKeyTestHelper, info);
			response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			info.OrgCountry = string.Empty;
			info.EnterpriseCode = "DDD";
			info.CargowiseCompanyCode = "ABC";
			info.ServerCode = "SYD";
			request = CreateAdditionalInfoRequest(securityKeyTestHelper, info);
			response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestAppendAdditionalInfo_DeduplicateDatabases()
		{
			var logger = new NLogWrapperForTest(GetType());
			var licence1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var db1 = licence1.Database;
			db1.LD_TenantID = "";
			db1.LD_DatabaseNumber = 6000;
			db1.LD_Product = "ABU";
			db1.GetOrCreateTrustedSystem();
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CM2", "DB2");
			var db2 = licence2.Database;
			db2.LD_TenantID = "";
			db2.LD_DatabaseNumber = 7000;
			db2.LD_Product = "ABU";
			db2.GetOrCreateTrustedSystem();
			Factory.Save();
			void SetDuplicateOrgInfo(OrgHeader org)
			{
				org.OH_FullName = "Org Name 123";
				org.MainAddress.OA_Address1 = "Addr 1";
				org.MainAddress.OA_Address2 = "Addr 2";
				org.MainAddress.OA_City = "Sydney";
				org.MainAddress.OA_RN_NKCountryCode = "AU";
				org.MainAddress.OA_PostCode = "2000";
				org.MainAddress.State = "NSW";
			}

			SetDuplicateOrgInfo(db1.WebAccessOrg);
			SetDuplicateOrgInfo(db2.WebAccessOrg);
			SetDuplicateOrgInfo(Database.WebAccessOrg);
			Factory.Save();
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(Database);
			Factory.Save();
			var request = CreateAdditionalInfoRequest(securityKeyTestHelper, new LicenceDatabaseRegistrationAdditionalInfo()
			{ Product = Database.LD_Product, SystemId = Database.LD_TenantID, TenantId = "Tenant#002", InfoExpires = DateTime.UtcNow.AddMinutes(10), Address1 = "Addr 1", Address2 = "Addr 2", BusinessRegoNo = "ABN #123", City = "Sydney", OrgCountry = "AU", OrgName = "Org Name 123", Postcode = "2000", State = "NSW", EnterpriseID = "enterprise_id_1", EnterpriseCode = "enterprise_code_2", ServerCode = "server_code_3", CargowiseCompanyCode = "cargowise_company_code_4", DatabaseNumber = "database_number_5", });
			var response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200", "Tenant#002" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			var db = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, 8000));
			var notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description).Single();
			AssertEquals(@"Product:ABU
SystemID:SYS0399481
TenantID:Tenant#002
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:database_number_5", notes.ST_NoteDataAsText);
			AssertEquals("Duplicate Licence Databases Found", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(@"Potential duplicate databases: 6000,7000,8000
Please review the additional info. and set the Master Org. / merge databases manually.

Product:ABU
SystemID:SYS0399481
TenantID:Tenant#002
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:database_number_5

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", Env.OutgoingMailManager.EmailsCreated[0].Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			db2.LD_Product = "CW1";
			var billedUsage = Factory.NewWithValidTestData<EdiBilledUsage>();
			billedUsage.BU9_LD = Database.PK;
			Factory.Save();
			response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals("Licence Databases Merge Failed", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(@"Potential duplicate databases: 6000,8000
The database 8000 has billing records already, merge failed.
Please review the additional info. and set the Master Org. / merge databases manually.

Product:ABU
SystemID:SYS0399481
TenantID:Tenant#002
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:database_number_5

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", Env.OutgoingMailManager.EmailsCreated[0].Body);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			billedUsage.Delete();
			Factory.Save();
			response = CallAppendAdditionalInfo(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			AssertEquals(false, Env.OutgoingMailManager.EmailsCreated.Any());
			db = new BusinessObjectFactory().LoadTop1<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, 6000));
			AssertEquals("SYS0399481", db.LD_TenantID);
			AssertEquals("Production", db.TrustedSystem.ETS_SystemID);
			notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description).Single();
			AssertEquals(@"Product:ABU
SystemID:SYS0399481
TenantID:Tenant#002
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:database_number_5", notes.ST_NoteDataAsText);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		#endregion V1
		#region V2
		public void TestRegisterTrustedSystem()
		{
			var logger = new NLogWrapperForTest(GetType());
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			var productRegistrationConfig = Factory.New<EdiTrustedMessagingConfig>();
			productRegistrationConfig.ETM_Product = "CSP";
			productRegistrationConfig.ETM_CertificateType = CertificateTypeList.Codes.PreDeploymentCertificate;
			productRegistrationConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = "CSP";
			centralSystemConfig.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
			centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			Factory.Save();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var request = new TrustedSystemRegistrationInfo()
				{ Product = "CSP", SystemId = "production" };
				request.EncryptRegistrationKey(rsa.ExportCspBlob(false), productRegistrationConfig.GetCertificate());
				var response = CallRegisterTrustedSystem(request, logger);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var data = JsonConvert.DeserializeObject<TrustedSystemRegistrationResponse>(response.Content.ReadAsStringAsync().Result);
				var certs = data.Export(rsa);
				AssertCertificate(certs.RemoteCertificate, centralSystemConfig.GetCertificate().GetRSAPrivateKey());
				AssertCertificate(certs.LocalCertificate, rsa);
				var system = EdiTrustedSystem.Load(Factory, "CSP", "production");
				AssertNotNull(system);
				AssertNotNull(system.CertificateConfig);
				AssertCertificate(system.CertificateConfig.GetCertificate(), rsa);
				var log = system.Logs.Find(x => x.SL_SE_NKEvent == Events.CertificateReceived.Code).Single();
				AssertEquals(log.SL_Reference, $"|Thumbprint={certs.LocalCertificate.Thumbprint}");
				var expectedLogMessages = new string[] { "api/ProductRegistration/system start","success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestRegisterTrustedSystem_ExistingSystem_NoCerts()
		{
			var logger = new NLogWrapperForTest(GetType());
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			var productRegistrationConfig = Factory.New<EdiTrustedMessagingConfig>();
			productRegistrationConfig.ETM_Product = "CSP";
			productRegistrationConfig.ETM_CertificateType = CertificateTypeList.Codes.PreDeploymentCertificate;
			productRegistrationConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = "CSP";
			centralSystemConfig.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
			centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = "CSP";
			system.ETS_SystemID = "production";
			system.GetOrCreateCertificateConfig();
			Factory.Save();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var request = new TrustedSystemRegistrationInfo()
				{ Product = "CSP", SystemId = "production", };
				request.EncryptRegistrationKey(rsa.ExportCspBlob(false), productRegistrationConfig.GetCertificate());
				var response = CallRegisterTrustedSystem(request, logger);
				AssertEquals(HttpStatusCode.OK, response.StatusCode);
				var data = JsonConvert.DeserializeObject<TrustedSystemRegistrationResponse>(response.Content.ReadAsStringAsync().Result);
				var certs = data.Export(rsa);
				AssertCertificate(certs.RemoteCertificate, centralSystemConfig.GetCertificate().GetRSAPrivateKey());
				AssertCertificate(certs.LocalCertificate, rsa);
				system.CertificateConfig.Reload();
				AssertCertificate(system.CertificateConfig.GetCertificate(), rsa);
				var log = system.Logs.Find(x => x.SL_SE_NKEvent == Events.CertificateReceived.Code).Single();
				AssertEquals(log.SL_Reference, $"|Thumbprint={certs.LocalCertificate.Thumbprint}");
				var expectedLogMessages = new string[] { "api/ProductRegistration/system start", "success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestRegisterTrustedSystem_ExistingSystem_WithCerts()
		{
			var logger = new NLogWrapperForTest(GetType());
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
			var productRegistrationConfig = Factory.New<EdiTrustedMessagingConfig>();
			productRegistrationConfig.ETM_Product = "CSP";
			productRegistrationConfig.ETM_CertificateType = CertificateTypeList.Codes.PreDeploymentCertificate;
			productRegistrationConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
			centralSystemConfig.ETM_Product = "CSP";
			centralSystemConfig.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
			centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = "CSP";
			system.ETS_SystemID = "production";
			system.GetOrCreateCertificateConfig();
			system.CertificateConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Client.cer");
			Factory.Save();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				EDICertRequestForTest.SetOutput(rsa);
				var request = new TrustedSystemRegistrationInfo()
				{ Product = "CSP", SystemId = "production", };
				request.EncryptRegistrationKey(rsa.ExportCspBlob(false), productRegistrationConfig.GetCertificate());
				var response = CallRegisterTrustedSystem(request, logger);
				AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
				var expectedLogMessages = new string[] { "api/ProductRegistration/system start","The system_id has been already registered" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		public void TestRegisterTrustedSystem_Validation()
		{
			var logger = new NLogWrapperForTest(GetType());
			// Blank registration key
			var request = new TrustedSystemRegistrationInfo()
			{ Product = "CSP", SystemId = "production", RegistrationKey = string.Empty };
			var response = CallRegisterTrustedSystem(request, logger);
			AssertEquals("Registration key shouldn't be empty", HttpStatusCode.BadRequest, response.StatusCode);
			var expectedLogMessages = new string[] { "api/ProductRegistration/system start","2002 The required field is missing." };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			using (var rsa = new RSACryptoServiceProvider(2048))
			{
				// No PDC cert
				EDICertRequestForTest.SetOutput(rsa);
				request.RegistrationKey = "invalid_key";
				response = CallRegisterTrustedSystem(request, logger);
				AssertEquals("Product registration cert is missing", HttpStatusCode.InternalServerError, response.StatusCode);
				expectedLogMessages = new string[] { "api/ProductRegistration/system start","Registration Cert Not Available" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				// Invalid registration key
				var productRegistrationConfig = Factory.New<EdiTrustedMessagingConfig>();
				productRegistrationConfig.ETM_Product = "CSP";
				productRegistrationConfig.ETM_CertificateType = CertificateTypeList.Codes.PreDeploymentCertificate;
				productRegistrationConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
				Factory.Save();
				response = CallRegisterTrustedSystem(request, logger);
				AssertEquals("Registration key is invalid", HttpStatusCode.BadRequest, response.StatusCode);
				expectedLogMessages = new string[] { "api/ProductRegistration/system start","2002 The required field is missing." };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				// No CSC cert
				request.EncryptRegistrationKey(rsa.ExportCspBlob(false), productRegistrationConfig.GetCertificate());
				response = CallRegisterTrustedSystem(request, logger);
				AssertEquals("Central system cert is missing", HttpStatusCode.InternalServerError, response.StatusCode);
				expectedLogMessages = new string[] { "api/ProductRegistration/system start","Certificate Authority Not Available" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				// Missing CA service config
				var centralSystemConfig = Factory.New<EdiTrustedMessagingConfig>();
				centralSystemConfig.ETM_Product = "CSP";
				centralSystemConfig.ETM_CertificateType = CertificateTypeList.Codes.CentralSystemCertificate;
				centralSystemConfig.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Server.pfx");
				Factory.Save();
				response = CallRegisterTrustedSystem(request, logger);
				AssertEquals("CA service config is not complete", HttpStatusCode.InternalServerError, response.StatusCode);
				expectedLogMessages = new string[] { "api/ProductRegistration/system start","Certificate Authority Not Available" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
				logger.ClearLog();
				// Should pass validation
				EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123");
				response = CallRegisterTrustedSystem(request, logger);
				AssertEquals("Should pass validation", HttpStatusCode.OK, response.StatusCode);
				expectedLogMessages = new string[] { "api/ProductRegistration/system start","success | 200" };
				Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			}
		}

		void AssertCertificate(X509Certificate2 cert, RSA privateKey)
		{
			AssertNull(cert.GetRSAPrivateKey());
			var msg = Guid.NewGuid().ToString();
			var encryptedBytes = cert.GetRSAPublicKey().Encrypt(Encoding.UTF8.GetBytes(msg), RSAEncryptionPadding.Pkcs1);
			var decryptedMsg = Encoding.UTF8.GetString(privateKey.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1));
			AssertEquals(msg, decryptedMsg);
		}

		public void TestRegisterTenant()
		{
			var logger = new NLogWrapperForTest(GetType());
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(trustedSystem);
			Factory.Save();
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);
			var info = new TenantRegistrationInfo()
			{ Product = trustedSystem.ETS_Product, SystemId = trustedSystem.ETS_SystemID, TenantId = "DEMO", InfoExpires = DateTime.UtcNow.AddMinutes(10), LicenceType = "PRD", Address1 = "Addr 1", Address2 = "Addr 2", BusinessRegoNo = "ABN #123", City = "Sydney", OrgCountry = "AU", OrgName = "Org Name 123", Postcode = "2000", State = "NSW", EnterpriseID = "enterprise_id_1", EnterpriseCode = "enterprise_code_2", ServerCode = "server_code_3", CargowiseCompanyCode = "cargowise_company_code_4", DatabaseNumber = "", };
			var request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			var response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var query = new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, trustedSystem.PK);
			query.AddToFilter(new ZQuery(LicenceDatabaseSchema.LD_TenantID, "DEMO"));
			var db = Factory.LoadTop1<LicenceDatabase>(query);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<ProductRegistrationResponse>(content);
			AssertEquals(db.LD_DatabaseNumber, data.DatabaseNumber);
			AssertNotNull(db);
			AssertEquals("DEMO", db.LD_TenantID);
			AssertEquals("PRD", db.LD_LicenceType);
			var notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single();
			AssertEquals(@"Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:", notes.ST_NoteDataAsText);
			var expectedLogMessages = new string[] { $@"Request received", "Trusted message decrypted", "success | 200", $"{info.Product} | {info.SystemId} | {info.TenantId}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestRegisterTenant_ExistingTenant()
		{
			var logger = new NLogWrapperForTest(GetType());
			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(trustedSystem);
			Factory.Save();
			var info = new TenantRegistrationInfo()
			{ Product = trustedSystem.ETS_Product, SystemId = trustedSystem.ETS_SystemID, TenantId = "DEMO", InfoExpires = DateTime.UtcNow.AddMinutes(10), LicenceType = "PRD", OrgName = "Org Name 123", };
			var request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			var response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 No Country or Licencing Info", $"{info.Product} | {info.SystemId} | {info.TenantId}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestRegisterTenant_Validation()
		{
			var logger = new NLogWrapperForTest(GetType());
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(trustedSystem);
			var existingDb = Factory.NewWithValidTestData<LicenceDatabase>();
			existingDb.LD_ETS_TrustedSystem = trustedSystem.PK;
			existingDb.LD_TenantID = "DEMO";
			Factory.Save();
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);
			// Blank tenant_id
			var info = new TenantRegistrationInfo()
			{ Product = trustedSystem.ETS_Product, SystemId = trustedSystem.ETS_SystemID, OrgName = "Demo Company", LicenceType = "PRD", InfoExpires = DateTime.UtcNow.AddMinutes(10), OrgCountry = "AU", };
			var request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			var response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 The tenant_id is blank" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			// No country
			info.TenantId = "DEMO";
			info.OrgCountry = string.Empty;
			request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 No Country or Licencing Info" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			// Tenant_id has registered
			info.OrgCountry = "AU";
			request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.BadRequest, response.StatusCode);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "2003 The tenant_id has been already registered", $"{info.Product} | {info.SystemId} | {info.TenantId}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
			logger.ClearLog();
			// Should pass validation
			info.TenantId = "Test";
			info.OrgCountry = string.Empty;
			info.EnterpriseCode = "DDD";
			info.CargowiseCompanyCode = "ABC";
			info.ServerCode = "SYD";
			request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200", $"{info.Product} | {info.SystemId} | {info.TenantId}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		public void TestRegisterTenant_DuplicateDatabase()
		{
			var logger = new NLogWrapperForTest(GetType());
			var licence1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var db1 = licence1.Database;
			db1.LD_TenantID = "";
			db1.LD_DatabaseNumber = 7000;
			db1.LD_Product = "CSP";
			db1.WebAccessOrg.OH_FullName = "Org Name 123";
			db1.WebAccessOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			var securityKeyTestHelper = new SecurityKeyTestHelper();
			securityKeyTestHelper.SetSecretKey(trustedSystem);
			Factory.Save();
			var info = new TenantRegistrationInfo()
			{ Product = trustedSystem.ETS_Product, SystemId = trustedSystem.ETS_SystemID, TenantId = "DEMO", InfoExpires = DateTime.UtcNow.AddMinutes(10), LicenceType = "PRD", OrgName = "Org Name 123", OrgCountry = "AU", };
			var request = CreateRegisterTenantRequest(securityKeyTestHelper, info);
			var response = CallRegisterTenant(request, logger);
			AssertEquals(HttpStatusCode.OK, response.StatusCode);
			var newFactory = new BusinessObjectFactory();
			var db1InNewFactory = newFactory.Load<LicenceDatabase>(db1.PK);
			AssertEquals("DEMO", db1InNewFactory.LD_TenantID);
			AssertEquals(trustedSystem.PK, db1InNewFactory.LD_ETS_TrustedSystem);
			var content = TrustedMessageResponseHelper.ReadMessage(response, db1InNewFactory.GetOrCreateTrustedSystem());
			var data = JsonConvert.DeserializeObject<ProductRegistrationResponse>(content);
			AssertEquals(db1InNewFactory.LD_DatabaseNumber, data.DatabaseNumber);
			var notes = db1InNewFactory.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single();
			AssertEquals(@"Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:
Address2:
City:
Postcode:
State:
BusinessRegoNo:
EnterpriseID:
EnterpriseCode:
ServerCode:
CargowiseCompanyCode:
DatabaseNumber:", notes.ST_NoteDataAsText);
			var expectedLogMessages = new string[] { "Request received", "Trusted message decrypted", "success | 200", $"{info.Product} | {info.SystemId} | {info.TenantId}" };
			Assert(logger.ContainsLogMessages(expectedLogMessages, needCheckContaionSessionId: true));
		}

		#endregion V2
		#region Execute V1
		HttpResponseMessage Execute(HttpRequestMessage request, NLogWrapper logger)
		{
			using (ObjectFactory.Substitute<WTG.TrustedMessaging.ICertificatesProvider>(() => new CertificatesProviderForTest()))
			using (var controller = new ProductRegistrationController(logger))
			{
				return ControllerTestHelper.Execute(controller, request);
			}
		}

		HttpResponseMessage CallRegister(string content, string signature, string product, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://unit-testing/api/ProductRegistration/Register/{product}");
			requestMessage.Content = new StringContent(content);
			if (!string.IsNullOrEmpty(signature))
			{
				requestMessage.Headers.Add("SIGNED", signature);
			}

			return Execute(requestMessage, logger);
		}

		static TrustedRequestForTest CreateAdditionalInfoRequest(SecurityKeyTestHelper securityKeyTestHelper, LicenceDatabaseRegistrationAdditionalInfo additionalInfo)
		{
			var json = JsonConvert.SerializeObject(additionalInfo);
			var certProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(certProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var trustedRequest = new TrustedRequest()
			{ EncryptedContent = trustedMessage.EncryptedContent, Product = additionalInfo.Product, SystemId = additionalInfo.SystemId };
			return new TrustedRequestForTest()
			{ Request = trustedRequest, Signature = trustedMessage.Signature, IV = trustedMessage.IV };
		}

		static TrustedRequestForTest CreateRegisterTenantRequest(SecurityKeyTestHelper securityKeyTestHelper, TenantRegistrationInfo tenantInfo)
		{
			var json = JsonConvert.SerializeObject(tenantInfo);
			var certProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(certProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var trustedRequest = new TrustedRequest()
			{ EncryptedContent = trustedMessage.EncryptedContent, Product = tenantInfo.Product, SystemId = tenantInfo.SystemId };
			return new TrustedRequestForTest()
			{ Request = trustedRequest, Signature = trustedMessage.Signature, IV = trustedMessage.IV };
		}

		HttpResponseMessage CallAppendAdditionalInfo(TrustedRequestForTest request, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://unit-testing/api/ProductRegistration/AppendAdditionalInfo");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			return Execute(requestMessage, logger);
		}

		#endregion Execute V1
		#region Execute V2
		HttpResponseMessage CallRegisterTrustedSystem(TrustedSystemRegistrationInfo request, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://unit-testing/api/ProductRegistration/system");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
			using (var controller = new ProductRegistrationControllerForTest(logger))
			{
				return ControllerTestHelper.Execute(controller, requestMessage, typeof(ProductRegistrationController));
			}
		}

		HttpResponseMessage CallRegisterTenant(TrustedRequestForTest request, NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"http://unit-testing/api/ProductRegistration/tenant");
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request.Request), Encoding.UTF8, "application/json");
			requestMessage.Headers.Add("SIGNED", request.Signature);
			requestMessage.Headers.Add("WTG_I", request.IV);
			return Execute(requestMessage, logger);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			var product = "ABU";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "";
			db.GetOrCreateTrustedSystem().ETS_SystemID = "Production";
			Database = db;
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			TestHelper = new ProductRegistrationMessageEncryptorTestHelper();
		}

		class ProductRegistrationControllerForTest : ProductRegistrationController
		{
			public ProductRegistrationControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			protected override IEDICertRequest GetCertRequest() => new EDICertRequestForTest();
		}

		class EDICertRequestForTest : IEDICertRequest
		{
			public static void SetOutput(RSA rsa)
			{
				var subjectName = Guid.NewGuid().ToString();
				var req = new CertificateRequest($"cn={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				using (var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
				{
					Output = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
				}
			}

			static string Output { get; set; }

			public bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
			{
				output = Output;
				return true;
			}
		}

		ProductRegistrationMessageEncryptorTestHelper TestHelper;
		LicenceDatabase Database;
		const string DefaultProuct = "ABU";
		const string DefaultSystemId = "SYS0399481";
		(string encryptedData, string signature) CreateRequest(string productType = DefaultProuct, string systemID = DefaultSystemId, string systemEndpointUrl = "http://www.cw1.com")
		{
			var json = JsonConvert.SerializeObject(new LicenceDatabaseRegistration { Product = productType, SystemId = systemID, SystemEndpointUrl = systemEndpointUrl, LicenceType = "TST", });
			return TestHelper.EncryptRegistrationRequest(json);
		}
	}
}

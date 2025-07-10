using System;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.TrustedMessaging;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class MyAccountTrustedMessengerTest : TestCaseWithFactory
	{
		public void TestReadMessage()
		{
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var userInfo = new TrustedUserInfo()
			{ Product = product, SystemId = db.LD_TenantID, UserId = "U1001", FullName = "User 1", Email = "user1@test.com", InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(userInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequst = new TrustedRequest()
			{ Product = product, SystemId = db.LD_TenantID, EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				var info = result.Info;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.Successful, status);
				AssertEquals("Product", "SMF", info.Product);
				AssertEquals("System ID", "SYS0399481", info.SystemId);
				AssertEquals("User ID", "U1001", info.UserId);
				AssertEquals("Full name", "User 1", info.FullName);
				AssertEquals("Email", "user1@test.com", info.Email);
			}
		}

		public void TestReadMessage_InfoExpired()
		{
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var userInfo = new TrustedUserInfo()
			{ Product = product, SystemId = db.LD_TenantID, UserId = "U1001", FullName = "User 1", Email = "user1@test.com" };
			var json = JsonConvert.SerializeObject(userInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequst = new TrustedRequest()
			{ Product = product, SystemId = db.LD_TenantID, EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.InfoExpired, status);
			}

			userInfo.InfoExpires = ZDateTime.UtcNow.AddMinutes(-1).ToDateTime();
			json = JsonConvert.SerializeObject(userInfo);
			trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			trustedRequst.EncryptedContent = trustedMessage.EncryptedContent;
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.InfoExpired, status);
			}
		}

		public void TestReadMessage_CW1()
		{
			var product = "CW1";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var userInfo = new TrustedUserInfo()
			{ Product = product, SystemId = db.LD_DatabaseNumber.ToString(), UserId = "U1001", FullName = "User 1", Email = "user1@test.com", InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(userInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequst = new TrustedRequest()
			{ Product = product, SystemId = db.LD_DatabaseNumber.ToString(), EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				var info = result.Info;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.Successful, status);
				AssertEquals("Product", "CW1", info.Product);
				AssertEquals("System ID", "8000", info.SystemId);
				AssertEquals("User ID", "U1001", info.UserId);
				AssertEquals("Full name", "User 1", info.FullName);
				AssertEquals("Email", "user1@test.com", info.Email);
			}

			trustedRequst.SystemId = "Invalid Database Number";
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.InvalidSystemInfo, status);
			}
		}

		public void TestReadMessage_TrustedService()
		{
			var serviceCode = "DDD";
			var trustedServices = new CodeDescriptionBoolCollection { { serviceCode, (NoResString)"Demo Service", true } };
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedServices);
			var trustedService = Factory.New<EdiTrustedSystem>();
			trustedService.ETS_Product = serviceCode;
			securityKeyTestHelper.SetSecretKey(trustedService);
			Factory.Save();
			var serviceInfo = new CertificateInfo()
			{ Product = "DDD", SystemId = string.Empty, CertificateOwnerProduct = "CW1", CertificateOwnerSystemId = "1001", InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(serviceInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequst = new TrustedRequest()
			{ Product = serviceCode, SystemId = string.Empty, EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<CertificateInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				var info = result.Info;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.Successful, status);
				AssertEquals("Product", "DDD", info.Product);
				AssertEquals("System ID", string.Empty, info.SystemId);
				AssertEquals("Cert Owner Product", "CW1", info.CertificateOwnerProduct);
				AssertEquals("Cert Owner System Id", "1001", info.CertificateOwnerSystemId);
			}
		}

		public void TestReadMessage_CertificateMismatched()
		{
			var product = "CW1";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var userInfo = new TrustedUserInfo()
			{ Product = product, SystemId = db.LD_DatabaseNumber.ToString(), UserId = "U1001", FullName = "User 1", Email = "user1@test.com", InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(userInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage(json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequst = new TrustedRequest()
			{ Product = product, SystemId = db.LD_DatabaseNumber.ToString(), EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()
			{ IsServer = false }))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				var status = result.ProcessStatus;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.CertificateMismatched, status);
			}

			//If the ETS is null but LD is not null, we shall return CertificateMismatched,
			//The service task automatically retries when a CertificateMismatched response is received,
			db.TrustedSystem.Delete();
			db.LD_ETS_TrustedSystem = ZGuid.Empty;
			Factory.Save();
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()
			{ IsServer = false }))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequst, Factory);
				AssertEquals(MyAccountTrustedMessageProcessStatus.CertificateMismatched, result.ProcessStatus);
			}
		}

		readonly SecurityKeyTestHelper securityKeyTestHelper = new SecurityKeyTestHelper();
		[TestDate(2020, 01, 01)]
		public void TestReadMessage_InvalidJSONShouldReportError()
		{
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			securityKeyTestHelper.SetSecretKey(db);
			Factory.Save();
			var userInfo = new TrustedUserInfo()
			{ Product = product, SystemId = db.LD_TenantID, UserId = "U1001", FullName = "User 1", Email = "user1@test.com", InfoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime() };
			var json = JsonConvert.SerializeObject(userInfo);
			var clientCertProvider = new CertificatesProviderForTest()
			{ IsServer = false };
			var trustedMessage = new TrustedMessenger(clientCertProvider).CreateMessage("D" + json, securityKeyTestHelper.SecretKey);
			var requestMessage = new HttpRequestMessage();
			requestMessage.Headers.Add("SIGNED", trustedMessage.Signature);
			requestMessage.Headers.Add("WTG_I", trustedMessage.IV);
			var trustedRequest = new TrustedRequest()
			{ Product = product, SystemId = db.LD_TenantID, EncryptedContent = trustedMessage.EncryptedContent };
			using (ObjectFactory.Substitute<ICertificatesProvider>(() => new CertificatesProviderForTest()))
			{
				var result = new MyAccountTrustedMessenger<TrustedUserInfo>().ReadMessage(requestMessage, trustedRequest, Factory);
				var status = result.ProcessStatus;
				AssertEquals("Status", MyAccountTrustedMessageProcessStatus.Malformed, status);
				AssertEquals("Trusted Messenger JSON Parsing Exception", ErrorReporter.LastKeyReported);
				AssertEquals("Unexpected character encountered while parsing value: D. Path '', line 0, position 0. Message: D{\"user_id\":\"U1001\",\"full_name\":\"User 1\",\"user_email\":\"user1@test.com\",\"user_country\":null,\"product\":\"SMF\",\"system_id\":\"SYS0399481\",\"tenant_id\":null,\"info_expires\":\"2020-01-01T00:05:00Z\",\"info_timestamp\":\"0001-01-01T00:00:00\"}", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}
	}
}

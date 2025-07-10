using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.Licensing;
using Enterprise.ProductRegistration.Common;
using Moq;
using NLog;
using NLog.Targets;
using NUnit.Framework;

namespace CargoWise.ProductRegistration.Service.Test
{
	public class RegistrationControllerTest : TestCase
	{
		public void TestPostRegister()
		{
			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2014, 3, 3);
			dbKey.ServerName = "syd-wris-1";
			dbKey.DatabaseName = "UAT2";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;

			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";

			var dbMock = new DbRegisterResult();
			Populate(dbMock);
			dbMock.ReturnCode = (int)RegisterStatus.Success;
			dbMock.UtcNow = new DateTime(2014, 8, 22);

			var db = new Mock<IRegistrationRepository>();
			string passwordHash = null;
			Func<RegisterRequest, string, Task<DbRegisterResult>> mockRegister = (req, pwd) =>
			{
				passwordHash = pwd;
				return Task<DbRegisterResult>.FromResult(dbMock);
			};
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);
			db.Setup(m => m.Register(request, It.IsAny<string>())).Returns(mockRegister);

			const string expectedKey = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>syd-wris-1</ServerName>
    <DatabaseName>UAT2</DatabaseName>
    <DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated>
    <GroupId xsi:nil=""true"" />
  </DbUniqueKey>
  <IssueDate>2014-08-22T00:00:00</IssueDate>
  <ExpiryDate>2014-10-21T00:00:00</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;
			var doc = new XmlDocument();
			doc.LoadXml(response.Key);
			var generatedPassword = doc.GetElementsByTagName("Password")[0].InnerText;
			var expectedKeyWithPassword = expectedKey.Replace("{password}", SecurityElement.Escape(generatedPassword));
			var signedKey = MessageSigner.SignXml(expectedKeyWithPassword);
			AssertEquals((int)RegisterStatus.Success, response.Status);
			AssertXMLEquals("key", signedKey, response.Key);
			AssertEquals(passwordHash, SHA512Encryptor.Encrypt("19" + generatedPassword));

			request.ProductKey = "EDI-SYD";
			dbMock.ManualLicenceExpiry = new DateTime(2014, 8, 25);
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);
			doc = new XmlDocument();
			doc.LoadXml(response.Key);
			generatedPassword = doc.GetElementsByTagName("Password")[0].InnerText;
			expectedKeyWithPassword = expectedKey.Replace("{password}", SecurityElement.Escape(generatedPassword))
				.Replace("<ExpiryDate>2014-10-21", "<ExpiryDate>2014-08-25");
			signedKey = MessageSigner.SignXml(expectedKeyWithPassword);
			AssertXMLEquals("key", signedKey, response.Key);

			dbMock.ReturnCode = (int)RegisterStatus.ProductKeyNotFound;
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.ProductKeyNotFound, response.Status);
			AssertNull("RegistrationKey", response.Key);

			dbMock.ReturnCode = (int)RegisterStatus.ProductKeyUnavailable;
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.ProductKeyUnavailable, response.Status);
			AssertNull("RegistrationKey", response.Key);
		}

		public void TestPostRegisterCargoWiseNext_Licensed()
		{
			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2014, 3, 3);
			dbKey.ServerName = "syd-wris-1";
			dbKey.DatabaseName = "UAT2";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;
			request.ProductVersion = "24.11.1.1"; // Indicates a CW Next version

			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";
			dbStatus.Product = "CWN"; // They are licensed for CW Next

			var dbMock = new DbRegisterResult();
			Populate(dbMock);
			dbMock.ReturnCode = (int)RegisterStatus.Success;
			dbMock.UtcNow = new DateTime(2014, 8, 22);

			var db = new Mock<IRegistrationRepository>();
			string passwordHash = null;
			Func<RegisterRequest, string, Task<DbRegisterResult>> mockRegister = (req, pwd) =>
			{
				passwordHash = pwd;
				return Task<DbRegisterResult>.FromResult(dbMock);
			};
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);
			db.Setup(m => m.Register(request, It.IsAny<string>())).Returns(mockRegister);

			const string expectedKey = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>syd-wris-1</ServerName>
    <DatabaseName>UAT2</DatabaseName>
    <DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated>
    <GroupId xsi:nil=""true"" />
  </DbUniqueKey>
  <IssueDate>2014-08-22T00:00:00</IssueDate>
  <ExpiryDate>2014-10-21T00:00:00</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;
			var doc = new XmlDocument();
			doc.LoadXml(response.Key);
			var generatedPassword = doc.GetElementsByTagName("Password")[0].InnerText;
			var expectedKeyWithPassword = expectedKey.Replace("{password}", SecurityElement.Escape(generatedPassword));
			var signedKey = MessageSigner.SignXml(expectedKeyWithPassword);
			AssertEquals((int)RegisterStatus.Success, response.Status);
			AssertXMLEquals("key", signedKey, response.Key);
			AssertEquals(passwordHash, SHA512Encryptor.Encrypt("19" + generatedPassword));

			request.ProductKey = "EDI-SYD";
			dbMock.ManualLicenceExpiry = new DateTime(2014, 8, 25);
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);
			doc = new XmlDocument();
			doc.LoadXml(response.Key);
			generatedPassword = doc.GetElementsByTagName("Password")[0].InnerText;
			expectedKeyWithPassword = expectedKey.Replace("{password}", SecurityElement.Escape(generatedPassword))
				.Replace("<ExpiryDate>2014-10-21", "<ExpiryDate>2014-08-25");
			signedKey = MessageSigner.SignXml(expectedKeyWithPassword);
			AssertXMLEquals("key", signedKey, response.Key);

			dbMock.ReturnCode = (int)RegisterStatus.ProductKeyNotFound;
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.ProductKeyNotFound, response.Status);
			AssertNull("RegistrationKey", response.Key);

			dbMock.ReturnCode = (int)RegisterStatus.ProductKeyUnavailable;
			response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.ProductKeyUnavailable, response.Status);
			AssertNull("RegistrationKey", response.Key);
		}

		public void TestPostRegisterCargoWiseNext_Unlicensed()
		{
			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2014, 3, 3);
			dbKey.ServerName = "syd-wris-1";
			dbKey.DatabaseName = "UAT2";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;
			request.ProductVersion = "24.11.1.1";

			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";
			dbStatus.Product = "CW1";

			var db = new Mock<IRegistrationRepository>();
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.ProductNotLicensed, response.Status);
			AssertNullOrEmpty(response.Key);
			AssertEquals(Enterprise.ProductRegistration.Common.Constants.CargoWiseNextWrongProductUserErrorMessage, response.ErrorMsg);
		}

		public void TestPostRegisterCargoWise_NoVersionCheck()
		{
			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2025, 6, 1);
			dbKey.ServerName = "syd-wscw-1";
			dbKey.DatabaseName = "UAT2";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;
			request.ProductVersion = "25.4.7.167";

			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";
			dbStatus.Product = "CW1";

			var dbMock = new DbRegisterResult();
			Populate(dbMock);
			dbMock.ReturnCode = (int)RegisterStatus.Success;
			dbMock.UtcNow = new DateTime(2014, 8, 22);

			var db = new Mock<IRegistrationRepository>();
			string passwordHash = null;
			Func<RegisterRequest, string, Task<DbRegisterResult>> mockRegister = (req, pwd) =>
			{
				passwordHash = pwd;
				return Task<DbRegisterResult>.FromResult(dbMock);
			};
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);
			db.Setup(m => m.Register(request, It.IsAny<string>())).Returns(mockRegister);

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);
			AssertNotNull(response.Key);
		}

		public void TestIsCargoWiseNextUpgradeAllowed_Blocked()
		{
			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";
			dbStatus.Product = "CW1";

			var db = new Mock<IRegistrationRepository>();
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);

			var controller = new RegistrationController(db.Object);
			var response = controller.GetIsCargoWiseNextUpgradeAllowed("EDISYD").Result;
			AssertEquals((int)RegisterStatus.ProductNotLicensed, response.Status);
			AssertEquals(Enterprise.ProductRegistration.Common.Constants.CargoWiseNextWrongProductUserErrorMessage, response.ErrorMsg);
		}

		public void TestIsCargoWiseNextUpgradeAllowed_Allowed()
		{
			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = "";
			dbStatus.Product = "CWN";

			var db = new Mock<IRegistrationRepository>();
			db.Setup(m => m.GetStatus("EDISYD")).ReturnsAsync(dbStatus);

			var controller = new RegistrationController(db.Object);
			var response = controller.GetIsCargoWiseNextUpgradeAllowed("EDISYD").Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);
		}

		public void TestPostVerify()
		{
			const string keyXmlTemplate = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>syd-wris-1</ServerName>
    <DatabaseName>UAT2</DatabaseName>
    <DatabaseCreated>2014-03-03T00:00:00</DatabaseCreated>
    <GroupId>5763ca96-92f9-4f97-a3a8-3ec743ed1211</GroupId>
  </DbUniqueKey>
  <IssueDate>{IssueDate}</IssueDate>
  <ExpiryDate>{ExpiryDate}</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			// 2014-12-24T00:00:00
			// 2015-02-22T00:00:00

			const string XmlDateFormat = "yyyy-MM-ddTHH:mm:ss";
			DateTime utcNow = new DateTime(2014, 12, 24); // see Populate()
			string recentKeyXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.AddDays(-2).ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(58).ToString(XmlDateFormat));

			string keyExpiringSoonXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.AddDays(-50).ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(10).ToString(XmlDateFormat));

			string expectedKeyXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(60).ToString(XmlDateFormat));

			var dbKeyWithGroupId = new DatabaseUniqueKey();
			dbKeyWithGroupId.DatabaseCreated = new DateTime(2014, 3, 3);
			dbKeyWithGroupId.ServerName = "syd-wris-1";
			dbKeyWithGroupId.DatabaseName = "UAT2";
			dbKeyWithGroupId.GroupId = new Guid("5763CA96-92F9-4F97-A3A8-3EC743ED1211");

			var unsignedRequest = new VerifyRequest();
			unsignedRequest.UniqueKey = dbKeyWithGroupId;
			unsignedRequest.ProductVersion = "14.9.20.2";
			unsignedRequest.Key = recentKeyXml;

			var signedRequest = new VerifyRequest();
			signedRequest.UniqueKey = dbKeyWithGroupId;
			signedRequest.ProductVersion = "14.9.20.2";
			signedRequest.Key = MessageSigner.SignXml(recentKeyXml);

			var requestRequiringRenewal = new VerifyRequest();
			requestRequiringRenewal.UniqueKey = dbKeyWithGroupId;
			requestRequiringRenewal.ProductVersion = "14.9.20.2";
			requestRequiringRenewal.Key = MessageSigner.SignXml(keyExpiringSoonXml);

			var notXmlRequest = new VerifyRequest();
			notXmlRequest.UniqueKey = dbKeyWithGroupId;
			notXmlRequest.Key = "not xml";

			DbRegisterResult dbSuccess = new DbRegisterResult();
			Populate(dbSuccess);
			DbRegisterResult dbSuccessWithUpdate = new DbRegisterResult();
			dbSuccessWithUpdate.ReturnCode = (int)RegisterStatus.UniqueKeyUpdated;
			Populate(dbSuccessWithUpdate);
			DbRegisterResult dbFail = new DbRegisterResult();
			dbFail.ReturnCode = (int)RegisterStatus.ProductKeyNotFound;

			var passwordHash = SHA512Encryptor.Encrypt("19{password}");
			var mockSuccess = new Mock<IRegistrationRepository>();
			mockSuccess.Setup(m => m.Verify(19, passwordHash, It.IsAny<VerifyRequest>())).ReturnsAsync(dbSuccess);
			var mockPartialMatch = new Mock<IRegistrationRepository>();
			mockPartialMatch.Setup(m => m.Verify(19, passwordHash, It.IsAny<VerifyRequest>())).ReturnsAsync(dbSuccessWithUpdate);
			var mockFail = new Mock<IRegistrationRepository>();
			mockFail.Setup(m => m.Verify(19, passwordHash, It.IsAny<VerifyRequest>())).ReturnsAsync(dbFail);

			var controllerWithDbSuccess = new RegistrationController(mockSuccess.Object);
			var controllerWithDbPartialMatch = new RegistrationController(mockPartialMatch.Object);
			var controllerWithDbFail = new RegistrationController(mockFail.Object);

			var responseToUnsignedKey = controllerWithDbSuccess.PostVerify(unsignedRequest).Result;
			var responseToSignedKey = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			var responseToNotXmlKey = controllerWithDbSuccess.PostVerify(notXmlRequest).Result;
			var responseToFailKey = controllerWithDbFail.PostVerify(signedRequest).Result;
			var responseToPartialMatch = controllerWithDbPartialMatch.PostVerify(signedRequest).Result;
			var responseWithRenewal = controllerWithDbSuccess.PostVerify(requestRequiringRenewal).Result;

			AssertEquals((int)RegisterStatus.Success, responseToSignedKey.Status);
			AssertEquals((int)RegisterStatus.UniqueKeyUpdated, responseToPartialMatch.Status);
			AssertEquals((int)RegisterStatus.RequestInvalid, responseToUnsignedKey.Status);
			AssertEquals((int)RegisterStatus.RequestInvalid, responseToNotXmlKey.Status);
			AssertEquals((int)RegisterStatus.ProductKeyNotFound, responseToFailKey.Status);

			Assert("key empty " + responseToSignedKey.Key, string.IsNullOrEmpty(responseToSignedKey.Key));
			var expectedSignedXml = MessageSigner.SignXml(expectedKeyXml);
			AssertXMLEquals(expectedSignedXml, responseToPartialMatch.Key);
			Assert("key empty " + responseToUnsignedKey.Key, string.IsNullOrEmpty(responseToUnsignedKey.Key));
			Assert("key empty " + responseToNotXmlKey.Key, string.IsNullOrEmpty(responseToNotXmlKey.Key));
			Assert("key empty " + responseToFailKey.Key, string.IsNullOrEmpty(responseToFailKey.Key));
			AssertXMLEquals(expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("<DbType>PRD</DbType>", "<DbType>TST</DbType>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed DBType generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("<EnterpriseCode>EDI</EnterpriseCode>", "<EnterpriseCode>HYE</EnterpriseCode>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed EnterpriseCode generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("<ServerCode>SYD</ServerCode>", "<ServerCode>RIS</ServerCode>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed ServerCode generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("<DbSecurityMode>LCK</DbSecurityMode>", "<DbSecurityMode>OPN</DbSecurityMode>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed DbSecurityMode generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("<HostedLocation>SYD</HostedLocation>", "<HostedLocation>LON</HostedLocation>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed HostedLocation generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("</RegistrationKey>", "<ExpiredMessage>times up</ExpiredMessage></RegistrationKey>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed ExpiredMessage generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("</RegistrationKey>", "<ExpiryWeekMessage>times up in a week</ExpiryWeekMessage></RegistrationKey>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed ExpiryWeekMessage generates new key", expectedSignedXml, responseWithRenewal.Key);

			signedRequest.Key = MessageSigner.SignXml(recentKeyXml.Replace("</RegistrationKey>", "<ExpiryMonthMessage>times up in a month</ExpiryMonthMessage></RegistrationKey>"));
			responseWithRenewal = controllerWithDbSuccess.PostVerify(signedRequest).Result;
			AssertXMLEquals("changed ExpiryMonthMessage generates new key", expectedSignedXml, responseWithRenewal.Key);

			dbSuccess.CustomExpiredMessage = "msg1";
			dbSuccess.CustomExpiryWeekMessage = "msg2";
			dbSuccess.CustomExpiryMonthMessage = "msg3<>";
			var mockSuccessWithMsg = new Mock<IRegistrationRepository>();
			mockSuccessWithMsg.Setup(m => m.Verify(19, passwordHash, It.IsAny<VerifyRequest>())).ReturnsAsync(dbSuccess);
			var controllerWithDbSuccessAndMsg = new RegistrationController(mockSuccessWithMsg.Object);
			responseWithRenewal = controllerWithDbSuccessAndMsg.PostVerify(signedRequest).Result;
			Assert("ExpiredMessage " + responseWithRenewal.Key, responseWithRenewal.Key.Contains("<ExpiredMessage>msg1</ExpiredMessage>"));
			Assert("ExpiryWeekMessage " + responseWithRenewal.Key, responseWithRenewal.Key.Contains("<ExpiryWeekMessage>msg2</ExpiryWeekMessage>"));
			Assert("ExpiryMonthMessage " + responseWithRenewal.Key, responseWithRenewal.Key.Contains("<ExpiryMonthMessage>msg3&lt;&gt;</ExpiryMonthMessage>"));

			dbSuccess.ManualLicenceExpiry = new DateTime(2014, 12, 20);
			var mockManualExpiry = new Mock<IRegistrationRepository>();
			mockManualExpiry.Setup(m => m.Verify(19, passwordHash, It.IsAny<VerifyRequest>())).ReturnsAsync(dbSuccess);
			var controllerWithManualExpiry = new RegistrationController(mockManualExpiry.Object);
			responseWithRenewal = controllerWithManualExpiry.PostVerify(signedRequest).Result;
			AssertXMLContains("<ExpiryDate>2014-12-20T00:00:00</ExpiryDate>", responseWithRenewal.Key);
		}

		void Populate(DbRegisterResult db)
		{
			db.DatabaseNumber = 19;
			db.DatabaseSecurityMode = "LCK";
			db.DatabaseType = "PRD";
			db.HostedLocation = "SYD";
			db.EnterpriseCode = "EDI";
			db.ServerCode = "SYD";
			db.UtcNow = new DateTime(2014, 12, 24);
			db.BillingModel = "STL";
			db.IsInternalSystem = true;

			var info = new BillingTimeZoneInfo(db.UtcNow);
			db.CurrentBillingTimeZoneUtcOffset = info.CurrentBillingTimeZoneUtcOffset;
			db.NextBillingTimeZoneUtcOffset = info.NextBillingTimeZoneUtcOffset;
			db.NextUtcOffsetEffectiveTimeUtc = info.NextUtcOffsetEffectiveTimeUtc;
		}

		#region Logging

		public void TestLogging_NewRegistration()
		{
			memoryTarget.Logs.Clear();

			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2023, 3, 1);
			dbKey.ServerName = "sydco-spet-1";
			dbKey.DatabaseName = "OdysseyDev";
			dbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			dbKey.ConnectionServerName = "localhost";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;
			request.ProductVersion = "22.11.9.345";

			var dbStatus = new DbStatus();
			dbStatus.DatabaseNumber = 19;
			dbStatus.Status = string.Empty;

			var dbMock = new DbRegisterResult();
			Populate(dbMock);
			dbMock.ReturnCode = (int)RegisterStatus.Success;
			dbMock.UtcNow = new DateTime(2023, 3, 13);

			var db = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			string passwordHash = null;
			Func<RegisterRequest, string, Task<DbRegisterResult>> mockRegister = (req, pwd) =>
			{
				passwordHash = pwd;
				return Task<DbRegisterResult>.FromResult(dbMock);
			};
			db.Setup(x => x.GetStatus("EDISYD")).Returns(Task<DbStatus>.FromResult(dbStatus));
			db.Setup(x => x.Register(It.Is<RegisterRequest>(r => r == request), It.IsAny<string>())).Returns(mockRegister);

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;

			var expected = "Info | RegistrationController: New registration EDISYD 22.11.9.345 | 0 | 19 sydco-spet-1 OdysseyDev 03/01/2023 00:00:00 c85b44c8-cb13-4b81-be6b-ebda40cc665a localhost";

			var result = memoryTarget.Logs.FirstOrDefault();
			AssertNotNull(result);
			AssertEquals("Log content", expected, result.TrimEnd(' ', '|'));
		}

		public void TestLogging_NewRegistration_IncorrectKey()
		{
			memoryTarget.Logs.Clear();

			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2023, 3, 1);
			dbKey.ServerName = "sydco-spet-1";
			dbKey.DatabaseName = "OdysseyDev";
			dbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			dbKey.ConnectionServerName = "localhost";
			var request = new RegisterRequest();
			request.ProductKey = "EDISYD";
			request.UniqueKey = dbKey;
			request.ProductVersion = "22.11.9.345";

			DbStatus dbStatus = null;

			var dbMock = new DbRegisterResult();
			dbMock.ReturnCode = (int)RegisterStatus.ProductKeyNotFound;

			var db = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			db.Setup(x => x.GetStatus("EDISYD")).Returns(Task<DbStatus>.FromResult(dbStatus));

			var controller = new RegistrationController(db.Object);
			var response = controller.PostRegister(request).Result;

			var expected = "Info | RegistrationController: New registration EDISYD 22.11.9.345 | 1";
			var result = memoryTarget.Logs.FirstOrDefault();
			AssertNotNull(result);
			AssertEquals("Log content", expected, result.TrimEnd(' ', '|'));
		}

		public void TestLogging_Verify()
		{
			memoryTarget.Logs.Clear();

			const string keyXmlTemplate = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>sydco-spet-1</ServerName>
    <DatabaseName>OdysseyDev</DatabaseName>
    <DatabaseCreated>2023-03-01T00:00:00</DatabaseCreated>
    <GroupId>C85B44C8-CB13-4B81-BE6B-EBDA40CC665A</GroupId>
    <ConnectionServerName>localhost</ConnectionServerName>
  </DbUniqueKey>
  <IssueDate>{IssueDate}</IssueDate>
  <ExpiryDate>{ExpiryDate}</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			const string XmlDateFormat = "yyyy-MM-ddTHH:mm:ss";
			var utcNow = new DateTime(2023, 3, 13);
			string recentKeyXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.AddDays(-2).ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(58).ToString(XmlDateFormat));

			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2023, 3, 1);
			dbKey.ServerName = "sydco-spet-1";
			dbKey.DatabaseName = "OdysseyDev";
			dbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			dbKey.ConnectionServerName = "localhost";

			var signedRequest = new VerifyRequest();
			signedRequest.UniqueKey = dbKey;
			signedRequest.ProductVersion = "22.11.9.345";
			signedRequest.Key = MessageSigner.SignXml(recentKeyXml);

			DbRegisterResult dbSuccess = new DbRegisterResult();
			Populate(dbSuccess);
			dbSuccess.UtcNow = utcNow;

			var passwordHash = SHA512Encryptor.Encrypt("19{password}");
			var mockSuccess = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			mockSuccess.Setup(x => x.Verify(It.Is<int>(i => i == 19), It.Is<string>(s => s == passwordHash), It.IsAny<VerifyRequest>())).Returns(Task.FromResult(dbSuccess));

			var controller = new RegistrationController(mockSuccess.Object);

			var response = controller.PostVerify(signedRequest).Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);

			var expected = "Info | RegistrationController: Verify registration EDISYD 22.11.9.345 | 0 | 19 sydco-spet-1 OdysseyDev 03/01/2023 00:00:00 c85b44c8-cb13-4b81-be6b-ebda40cc665a localhost";
			var result = memoryTarget.Logs.FirstOrDefault();
			AssertNotNull(result);
			AssertEquals("Log content", expected, result.TrimEnd(' ', '|'));
		}

		public void TestLogging_Verify_Failure()
		{
			memoryTarget.Logs.Clear();

			const string keyXmlTemplate = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>sydco-spet-1</ServerName>
    <DatabaseName>OdysseyDev2</DatabaseName>
    <DatabaseCreated>2023-03-01T00:00:00</DatabaseCreated>
    <GroupId>C85B44C8-CB13-4B81-BE6B-EBDA40CC665A</GroupId>
    <ConnectionServerName>localhost</ConnectionServerName>
  </DbUniqueKey>
  <IssueDate>{IssueDate}</IssueDate>
  <ExpiryDate>{ExpiryDate}</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			const string XmlDateFormat = "yyyy-MM-ddTHH:mm:ss";
			var utcNow = new DateTime(2023, 3, 13);
			string recentKeyXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.AddDays(-2).ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(58).ToString(XmlDateFormat));

			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2023, 3, 1);
			dbKey.ServerName = "sydco-spet-1";
			dbKey.DatabaseName = "OdysseyDev2";
			dbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			dbKey.ConnectionServerName = "localhost";

			var existingDbKey = new DatabaseUniqueKey();
			existingDbKey.DatabaseCreated = new DateTime(2023, 3, 17);
			existingDbKey.ServerName = "sydco-spet-1";
			existingDbKey.DatabaseName = "OdysseyDev";
			existingDbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			existingDbKey.ConnectionServerName = "localhost";

			var signedRequest = new VerifyRequest();
			signedRequest.UniqueKey = dbKey;
			signedRequest.ProductVersion = "22.11.9.345";
			signedRequest.Key = MessageSigner.SignXml(recentKeyXml);

			DbRegisterResult dbFail = new DbRegisterResult();
			dbFail.UtcNow = utcNow;
			dbFail.ReturnCode = (int)RegisterStatus.UniqueKeyNotMatched;

			var passwordHash = SHA512Encryptor.Encrypt("19{password}");
			var mockFail = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			mockFail.Setup(x => x.Verify(It.Is<int>(i => i == 19), It.Is<string>(s => s == passwordHash), It.IsAny<VerifyRequest>())).Returns(Task.FromResult(dbFail));
			mockFail.Setup(x => x.GetDatabaseUniqueKey(It.Is<int>(i => i == 19))).Returns(Task.FromResult(existingDbKey));

			var controller = new RegistrationController(mockFail.Object);

			var response = controller.PostVerify(signedRequest).Result;
			AssertEquals((int)RegisterStatus.UniqueKeyNotMatched, response.Status);

			var expected = "Info | RegistrationController: Verify registration EDISYD 22.11.9.345 | 4 | 19 sydco-spet-1 OdysseyDev2 03/01/2023 00:00:00 c85b44c8-cb13-4b81-be6b-ebda40cc665a localhost | sydco-spet-1 OdysseyDev 03/17/2023 00:00:00 c85b44c8-cb13-4b81-be6b-ebda40cc665a localhost";
			var result = memoryTarget.Logs.FirstOrDefault();
			AssertNotNull(result);
			AssertEquals("Log content", expected, result.TrimEnd(' ', '|'));
		}

		public void TestLogging_Unregister()
		{
			memoryTarget.Logs.Clear();

			const string keyXmlTemplate = @"<?xml version=""1.0"" encoding=""utf-16""?>
<RegistrationKey xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <DatabaseNumber>19</DatabaseNumber>
  <DbUniqueKey>
    <ServerName>sydco-spet-1</ServerName>
    <DatabaseName>OdysseyDev</DatabaseName>
    <DatabaseCreated>2023-03-01T00:00:00</DatabaseCreated>
    <GroupId>C85B44C8-CB13-4B81-BE6B-EBDA40CC665A</GroupId>
    <ConnectionServerName>localhost</ConnectionServerName>
  </DbUniqueKey>
  <IssueDate>{IssueDate}</IssueDate>
  <ExpiryDate>{ExpiryDate}</ExpiryDate>
  <EnterpriseCode>EDI</EnterpriseCode>
  <ServerCode>SYD</ServerCode>
  <Password>{password}</Password>
  <DbType>PRD</DbType>
  <DbSecurityMode>LCK</DbSecurityMode>
  <HostedLocation>SYD</HostedLocation>
  <CurrentBillingTimeZoneUtcOffset>11</CurrentBillingTimeZoneUtcOffset>
  <NextBillingTimeZoneUtcOffset>10</NextBillingTimeZoneUtcOffset>
  <NextUtcOffsetEffectiveTimeUtc>2015-04-04T16:00:00</NextUtcOffsetEffectiveTimeUtc>
  <BillingModel>STL</BillingModel>
  <CanVerifyByNameOnly xsi:nil=""true"" />
  <IsInternalSystem>true</IsInternalSystem>
</RegistrationKey>";

			const string XmlDateFormat = "yyyy-MM-ddTHH:mm:ss";
			var utcNow = new DateTime(2023, 3, 13);
			string recentKeyXml = keyXmlTemplate
				.Replace("{IssueDate}", utcNow.AddDays(-2).ToString(XmlDateFormat))
				.Replace("{ExpiryDate}", utcNow.AddDays(58).ToString(XmlDateFormat));

			var dbKey = new DatabaseUniqueKey();
			dbKey.DatabaseCreated = new DateTime(2023, 3, 1);
			dbKey.ServerName = "sydco-spet-1";
			dbKey.DatabaseName = "OdysseyDev2";
			dbKey.GroupId = Guid.Parse("C85B44C8-CB13-4B81-BE6B-EBDA40CC665A");
			dbKey.ConnectionServerName = "localhost";

			var signedRequest = new UnregisterRequest();
			signedRequest.Key = MessageSigner.SignXml(recentKeyXml);

			var passwordHash = SHA512Encryptor.Encrypt("19{password}");
			var mockSuccess = new Mock<IRegistrationRepository>(MockBehavior.Strict);
			mockSuccess.Setup(x => x.Unregister(It.Is<int>(i => i == 19), It.Is<string>(s => s == passwordHash))).Returns(Task.FromResult((int)RegisterStatus.Success));

			var controller = new RegistrationController(mockSuccess.Object);

			var response = controller.PostUnregister(signedRequest).Result;
			AssertEquals((int)RegisterStatus.Success, response.Status);

			var expected = "Info | RegistrationController: Unregister EDISYD  | 0 | 19 sydco-spet-1 OdysseyDev 03/01/2023 00:00:00 c85b44c8-cb13-4b81-be6b-ebda40cc665a localhost";
			var result = memoryTarget.Logs.FirstOrDefault();
			AssertNotNull(result);
			AssertEquals("Log content", expected, result.TrimEnd(' ', '|'));
		}

		MemoryTarget memoryTarget;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			memoryTarget = new MemoryTarget();
			memoryTarget.Layout = "${level} | ${logger}: ${message} ${event-properties:product_version} | ${event-properties:status} | ${event-properties:database_number} ${event-properties:server_name} ${event-properties:database_name} ${event-properties:database_create_time} ${event-properties:group_id} ${event-properties:connection_server} | ${event-properties:registered_server_name} ${event-properties:registered_database_name} ${event-properties:registered_database_create_time} ${event-properties:registered_group_id} ${event-properties:registered_connection_server_name}";

			var config = LogManager.Configuration;
			if (config == null)
			{
				config = new NLog.Config.LoggingConfiguration();
				LogManager.Configuration = config;
			}
			config.AddRule(LogLevel.Info, LogLevel.Fatal, memoryTarget, "*");
			LogManager.ReconfigExistingLoggers();
		}
	}
}

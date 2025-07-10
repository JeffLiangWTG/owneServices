using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

namespace ZClientEDI.Test.Telematics.Tca
{
	public class RimEnrolmentRequestProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			TelematicsConfigurationRegistry.Instance.TcaRimUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://test");

			httpClientFactoryMock = new Mock<IHttpClientFactory>();
			eHubMessageSenderMock = new Mock<IEHubMessageSender>();
			httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			httpClient = new HttpClient(httpMessageHandlerMock.Object);
			httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(httpClient);
			processor = new RimEnrolmentRequestProcessor(httpClientFactoryMock.Object, eHubMessageSenderMock.Object, TimeSpan.FromMilliseconds(1000));

			testTime = new DateTimeOffset(2020, 11, 12, 01, 24, 55, 68, TimeSpan.FromHours(0));
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41243;
			clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_OH = org.PK;
			clientCompany.LCC_State = "VIC";
			clientCompany.LCC_Address1 = "Some Place";
			clientCompany.LCC_Phone = "94811111";
			clientCompany.LCC_PostCode = "2100";
			clientCompany.LCC_City = "Melbourne";
			clientDevice = Factory.New<ClientDeviceHeader>();
			clientDevice.CDH_Identifier = "1";
			clientDevice.CDH_EnterpriseCode = "DDD";
			clientDevice.CDH_ServerCode = "SYD";
			orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_OH = org.PK;
			orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			orgCusCode.OK_CustomsRegNo = "1234567890";

			Factory.Save();
		}

		public void TestSuccessfulRequestCreatesNewEnrolmentAndNotifiesRelevantClientSystem()
		{
			CombineAssertions(() =>
				{
					Test("WTG00000001", HttpStatusCode.OK, "2", "https://test/enrolment/WTG00000001", "12345", "NSW", "ABC123");
					Test("WTG00000002", HttpStatusCode.Created, "3", "https://test/enrolment/WTG00000002", "54321", "ACT", "ABC321");
					Test("WTG00000003", HttpStatusCode.Accepted, "4", "https://test/enrolment/WTG00000003", "12345678901", "VIC", "ABC123");
					Test("WTG00000004", HttpStatusCode.NonAuthoritativeInformation, "5", "https://test/enrolment/WTG00000004", "12345", "QLD", "WTG421");
					Test("WTG00000005", HttpStatusCode.NoContent, "6", "https://test/enrolment/WTG00000005", "12345", "WA", "SPC117");
					Test("WTG00000006", HttpStatusCode.ResetContent, "7", "https://test/enrolment/WTG00000006", "12345", "NT", "ABC123");
					Test("WTG00000007", HttpStatusCode.PartialContent, "8", "https://test/enrolment/WTG00000007", "12345", "TAS", "ABC123");
				});

			void Test(string enrolmentId, HttpStatusCode returnCode, string deviceIdentifier, string expectedUri, string vin, string registrationState, string registrationNumber)
			{
				// Arrange
				var newHttpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(newHttpClient);

				var expectedMessage = GetExpectedEnrolmentString(enrolmentId, deviceIdentifier, vin, registrationNumber, registrationState);
				var newclientDevice = Factory.New<ClientDeviceHeader>();
				newclientDevice.CDH_Identifier = deviceIdentifier;
				newclientDevice.CDH_EnterpriseCode = "DDD";
				newclientDevice.CDH_ServerCode = "SYD";
				httpMessageHandlerMock.Reset();
				eHubMessageSenderMock.Reset();
				var retrievedRequest = string.Empty;
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.Callback((HttpRequestMessage message, CancellationToken ct) =>
					{
						retrievedRequest = message.Content.ReadAsStringAsync().Result;
					})
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent("VeryNice!!!"),
					});
				eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

				// Act
				processor.ProcessEnrollmentRequest(Factory, newclientDevice, "OSOM", vin, registrationNumber, registrationState, "Under Dash", testTime, testTime, testTime);

				// Assert
				httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
					"SendAsync",
					Times.Once(),
					ItExpr.Is<HttpRequestMessage>(
						message =>
							message.Method == HttpMethod.Put &&
							message.RequestUri.AbsoluteUri == expectedUri),
					ItExpr.IsAny<CancellationToken>());
				var registration = Factory.Load<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId))
					.Single();
				AssertEquals(expectedMessage, retrievedRequest);
				AssertEquals(registration.TRR_LCC_ClientCompany, clientCompany.PK);
				AssertEquals(registration.TRR_CDH_ClientDeviceHeader, newclientDevice.PK);
				AssertEquals(registration.TRR_OK_OrgCusCode, orgCusCode.PK);
				AssertEquals(registration.TRR_VehicleRegistration, registrationNumber);
				AssertEquals(registration.TRR_VehicleRegistrationState, registrationState);
				AssertEquals(registration.TRR_VehicleIdentificationNumber, vin);
				AssertEquals(registration.TRR_InstallationDateTimeOffset, testTime);
			}
		}

		public void TestTcaFailureLoggedAndDoesNotCreateRegistrationOrSendNotification()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.Forbidden,
					Content = new StringContent("VeryNotNice!!!"),
				});
			eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

			// Act
			var result = processor.ProcessEnrollmentRequest(Factory, clientDevice, "OSOM", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

			// Assert
			httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put),
				ItExpr.IsAny<CancellationToken>());
			AssertEquals(false, result.status);
			AssertEquals($"TCA registration failure, status code: {HttpStatusCode.Forbidden} response: VeryNotNice!!!", result.message);
			AssertEquals(false, Factory.Exists(typeof(ClientTelRimRegistration), new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId)));
		}

		public void TestMissingRequiredFieldsOnClientCompany()
		{
			// Arrange
			CombineAssertions(
				() =>
				{
					clientCompany.LCC_Phone = string.Empty;
					Test();

					clientCompany.LCC_Phone = "94811111";
					clientCompany.LCC_Name = string.Empty;
					Test();

					clientCompany.LCC_Name = "Some Company";
					clientCompany.LCC_Address1 = string.Empty;
					Test();

					clientCompany.LCC_Address1 = "Some Place";
					clientCompany.LCC_City = string.Empty;
					Test();

					clientCompany.LCC_City = "Sydney";
					clientCompany.LCC_State = string.Empty;
					Test();

					clientCompany.LCC_State = "NSW";
					clientCompany.LCC_PostCode = string.Empty;
					Test();
				});

			void Test()
			{
				var enrolmentId = "WTG00000001";

				// Act
				var result = processor.ProcessEnrollmentRequest(Factory, clientDevice, "OSOM", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

				// Assert
				AssertEquals(false, result.status);
				AssertEquals("Client Company must be registered with all required fields: Name, Address, Phone, City, Postcode and State", result.message);
				AssertEquals(false, Factory.Exists(typeof(ClientTelRimRegistration), new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId)));
			}
		}

		public void TestInvalidAbn()
		{
			CombineAssertions(
				() =>
				{
					orgCusCode.OK_CustomsRegNo = ZString.Empty;
					Test();

					orgCusCode.OK_CustomsRegNo = "1234567890";
					orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.Diplomat;
					Test();
				});

			void Test()
			{
				var enrolmentId = "WTG00000001";

				// Act
				var result = processor.ProcessEnrollmentRequest(Factory, clientDevice, "OSOM", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

				// Assert
				AssertEquals(false, result.status);
				AssertEquals("Related Organization must be registered with an Australian Business Number", result.message);
				AssertEquals(false, Factory.Exists(typeof(ClientTelRimRegistration), new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId)));
			}
		}

		public void TestProcessorWithExistingRegistration()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			var existingEnrolment = CreateTelEnrolment(enrolmentId, "OSOM", testTime.AddHours(-10));
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("VeryNice!!!"),
				});
			eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

			// Act
			processor.ProcessEnrollmentRequest(Factory, clientDevice, "OSOM", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

			// Assert
			httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
				"SendAsync",
				Times.Never(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>());
			var registration = Factory.Load<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId))
				.Single();
			AssertEquals(registration.PK, existingEnrolment.PK);
		}

		public void TestProcessorWithNewScheme()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			CreateTelEnrolment(enrolmentId, "SPECTS", testTime.AddHours(-10));
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("VeryNice!!!"),
				});
			eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

			// Act
			processor.ProcessEnrollmentRequest(Factory, clientDevice, "OSOM", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

			// Assert
			var registration = Factory.Load<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.TRR_CDH_ClientDeviceHeader, clientDevice.PK));
			AssertEquals(2, registration.Length);
		}

		public void TestProcessorExistingSchemeOnClosedRegistration()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			var existingEnrolment = CreateTelEnrolment(enrolmentId, "SPECTS", new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)));
			existingEnrolment.TRR_EndTime = testTime.AddHours(-10);
			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("VeryNice!!!"),
				});
			eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

			// Act
			processor.ProcessEnrollmentRequest(Factory, clientDevice, "SPECTS", "12345", "ABC123", "VIC", "Under Dash", testTime, testTime, testTime);

			// Assert
			var registrations = Factory.Load<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.TRR_CDH_ClientDeviceHeader, clientDevice.PK));
			AssertEquals(2, registrations.Length);
			var currentRegistration = registrations.Single(registration => registration.TRR_EndTime == ZDateTimeOffset.Empty);
			AssertEquals(true, existingEnrolment.TRR_EndTime < currentRegistration.TRR_StartTime);
		}

		public void TestSuccessfulCancellationRequestClosesExistingEnrolmentAndNotifiesRelevantClientSystem()
		{
			CombineAssertions(
				() =>
				{
					Test("WTG00000001", HttpStatusCode.OK, "2", "https://test/enrolment/WTG00000001", "12345", "NSW", "ABC123");
					Test("WTG00000002", HttpStatusCode.Created, "3", "https://test/enrolment/WTG00000002", "54321", "ACT", "ABC321");
					Test("WTG00000003", HttpStatusCode.Accepted, "4", "https://test/enrolment/WTG00000003", "12345678901", "VIC", "ABC123");
					Test("WTG00000004", HttpStatusCode.NonAuthoritativeInformation, "5", "https://test/enrolment/WTG00000004", "12345", "QLD", "WTG421");
					Test("WTG00000005", HttpStatusCode.NoContent, "6", "https://test/enrolment/WTG00000005", "12345", "WA", "SPC117");
					Test("WTG00000006", HttpStatusCode.ResetContent, "7", "https://test/enrolment/WTG00000006", "12345", "NT", "ABC123");
					Test("WTG00000007", HttpStatusCode.PartialContent, "8", "https://test/enrolment/WTG00000007", "12345", "TAS", "ABC123");
				});

			void Test(string enrolmentId, HttpStatusCode returnCode, string deviceIdentifier, string expectedUri, string vin, string registrationState, string registrationNumber)
			{
				// Arrange
				var newHttpClient = new HttpClient(httpMessageHandlerMock.Object);
				httpClientFactoryMock.Setup(factory => factory.CreateNew(It.IsAny<HttpMessageHandler>(), It.IsAny<TimeSpan>())).Returns(newHttpClient);

				var newclientDevice = Factory.New<ClientDeviceHeader>();
				newclientDevice.CDH_Identifier = deviceIdentifier;
				newclientDevice.CDH_EnterpriseCode = "DDD";
				newclientDevice.CDH_ServerCode = "SYD";

				var existingRegistration = Factory.New<ClientTelRimRegistration>();
				existingRegistration.TRR_LCC_ClientCompany = clientCompany.PK;
				existingRegistration.TRR_CDH_ClientDeviceHeader = newclientDevice.PK;
				existingRegistration.TRR_OK_OrgCusCode = orgCusCode.PK;
				existingRegistration.TRR_EnrolmentScheme = "OSOM";
				existingRegistration.TRR_VehicleIdentificationNumber = vin;
				existingRegistration.TRR_VehicleRegistration = registrationNumber;
				existingRegistration.TRR_EnrolmentId = enrolmentId;
				existingRegistration.TRR_VehicleRegistrationState = registrationState;
				existingRegistration.TRR_StartTime = testTime;
				existingRegistration.TRR_InstallationDateTimeOffset = testTime;

				var cessationTime = testTime.AddHours(10);
				var expectedMessage = GetExpectedEnrolmentCancellationString(enrolmentId, deviceIdentifier, vin, registrationNumber, registrationState);

				Factory.Save();

				httpMessageHandlerMock.Reset();
				eHubMessageSenderMock.Reset();
				var retrievedRequest = string.Empty;
				httpMessageHandlerMock
					.Protected()
					.Setup<Task<HttpResponseMessage>>(
						"SendAsync",
						ItExpr.IsAny<HttpRequestMessage>(),
						ItExpr.IsAny<CancellationToken>())
					.Callback((HttpRequestMessage message, CancellationToken ct) =>
					{
						retrievedRequest = message.Content.ReadAsStringAsync().Result;
					})
					.ReturnsAsync(new HttpResponseMessage()
					{
						StatusCode = returnCode,
						Content = new StringContent("VeryNice!!!"),
					});
				eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

				// Act
				processor.ProcessEnrollmentCancellation(Factory, existingRegistration, cessationTime);

				// Assert
				httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
					"SendAsync",
					Times.Once(),
					ItExpr.Is<HttpRequestMessage>(
						message =>
							message.Method == HttpMethod.Put &&
							message.RequestUri.AbsoluteUri == expectedUri),
					ItExpr.IsAny<CancellationToken>());
				var registration = Factory.Load<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.TRR_EnrolmentId, enrolmentId))
					.Single();

				var retrievedRegistration = Factory.Load<ClientTelRimRegistration>(existingRegistration.PK);
				AssertEquals(cessationTime, retrievedRegistration.TRR_EndTime);
				AssertEquals(expectedMessage, retrievedRequest);
			}
		}

		public void TestTcaCancelEnrolmentFailureFailureLoggedAndDoesNotCloseRegistrationOrSendNotification()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			var newclientDevice = Factory.New<ClientDeviceHeader>();
			newclientDevice.CDH_Identifier = "1";
			newclientDevice.CDH_EnterpriseCode = "DDD";
			newclientDevice.CDH_ServerCode = "SYD";

			var existingRegistration = Factory.New<ClientTelRimRegistration>();
			existingRegistration.TRR_LCC_ClientCompany = clientCompany.PK;
			existingRegistration.TRR_CDH_ClientDeviceHeader = newclientDevice.PK;
			existingRegistration.TRR_OK_OrgCusCode = orgCusCode.PK;
			existingRegistration.TRR_EnrolmentScheme = "OSOM";
			existingRegistration.TRR_VehicleIdentificationNumber = "12345";
			existingRegistration.TRR_VehicleRegistration = "ABC123";
			existingRegistration.TRR_EnrolmentId = enrolmentId;
			existingRegistration.TRR_VehicleRegistrationState = "NSW";
			existingRegistration.TRR_StartTime = testTime;
			existingRegistration.TRR_InstallationDateTimeOffset = testTime;

			var cessationTime = testTime.AddHours(5);
			Factory.Save();

			httpMessageHandlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.Forbidden,
					Content = new StringContent("VeryNotNice!!!"),
				});
			eHubMessageSenderMock.Setup(sender => sender.Send(Factory, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()));

			// Act
			var result = processor.ProcessEnrollmentCancellation(Factory, existingRegistration, cessationTime);

			// Assert
			httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
				"SendAsync",
				Times.Once(),
				ItExpr.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put),
				ItExpr.IsAny<CancellationToken>());
			AssertEquals(false, result.status);
			AssertEquals($"TCA registration failure, status code: {HttpStatusCode.Forbidden} response: VeryNotNice!!!", result.message);
			var retrievedRegistration = Factory.Load<ClientTelRimRegistration>(existingRegistration.PK);
			AssertEquals(true, retrievedRegistration.TRR_EndTime.IsEmpty);
		}

		public void TestCancellationRequestRejectedForClosedRegistrations()
		{
			// Arrange
			var enrolmentId = "WTG00000001";
			var newclientDevice = Factory.New<ClientDeviceHeader>();
			newclientDevice.CDH_Identifier = "1";
			newclientDevice.CDH_EnterpriseCode = "DDD";
			newclientDevice.CDH_ServerCode = "SYD";

			var existingRegistration = Factory.New<ClientTelRimRegistration>();
			existingRegistration.TRR_LCC_ClientCompany = clientCompany.PK;
			existingRegistration.TRR_CDH_ClientDeviceHeader = newclientDevice.PK;
			existingRegistration.TRR_OK_OrgCusCode = orgCusCode.PK;
			existingRegistration.TRR_EnrolmentScheme = "OSOM";
			existingRegistration.TRR_VehicleIdentificationNumber = "12345";
			existingRegistration.TRR_VehicleRegistration = "ABC123";
			existingRegistration.TRR_EnrolmentId = enrolmentId;
			existingRegistration.TRR_VehicleRegistrationState = "NSW";
			existingRegistration.TRR_StartTime = testTime;
			existingRegistration.TRR_EndTime = testTime.AddHours(5);
			existingRegistration.TRR_InstallationDateTimeOffset = testTime;

			Factory.Save();

			// Act
			var result = processor.ProcessEnrollmentCancellation(Factory, existingRegistration, testTime.AddHours(5));

			// Assert
			httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>(
				"SendAsync",
				Times.Never(),
				ItExpr.Is<HttpRequestMessage>(message => message.Method == HttpMethod.Put),
				ItExpr.IsAny<CancellationToken>());
			AssertEquals(false, result.status);
			AssertEquals($"Registration WTG00000001 has already been closed", result.message);
		}

		ClientTelRimRegistration CreateTelEnrolment(string enrolmentId, string scheme, DateTimeOffset time)
		{
			var enrolment = Factory.New<ClientTelRimRegistration>();
			enrolment.TRR_EnrolmentScheme = scheme;
			enrolment.TRR_EnrolmentId = enrolmentId;
			enrolment.TRR_StartTime = time;
			enrolment.TRR_CDH_ClientDeviceHeader = clientDevice.PK;
			enrolment.TRR_LCC_ClientCompany = clientCompany.PK;
			enrolment.TRR_OK_OrgCusCode = orgCusCode.PK;
			enrolment.TRR_InstallationDateTimeOffset = testTime;
			Factory.Save();
			return enrolment;
		}

		string GetExpectedEnrolmentString(string enrolmentId, string deviceIdentity, string vin, string registrationNumber, string registrationState)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentForm xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07"">
	<application>
		<name>RIM</name>
		<version>1.02</version>
	</application>
	<identifier>{enrolmentId}</identifier>
	<enrolmentProcess>ASP</enrolmentProcess>
	<authoritySection>
		<authority>
			<authorityCode>NSW</authorityCode>
		</authority>
		<scheme>OSOM</scheme>
	</authoritySection>
	<statusCode>APPROVED</statusCode>
	<commencementDateTime>2020-11-12T01:24:55.068</commencementDateTime>
	<operatorSection>
		<operator>
			<identity>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
				<name>ABC Co</name>
			</identity>
			<postalAddress>
				<lineOne>Some Place</lineOne>
				<lineTwo />
				<locality>Melbourne</locality>
				<stateCode>VIC</stateCode>
				<postCode>2100</postCode>
			</postalAddress>
			<businessHoursPhone>94811111</businessHoursPhone>
		</operator>
		<primaryUnitInformation>
			<registration>
				<number>{registrationNumber}</number>
				<stateCode>{registrationState}</stateCode>
			</registration>
			<identity>
				<vin>{vin}</vin>
			</identity>
		</primaryUnitInformation>
	</operatorSection>
	<serviceProviderSection>
		<serviceProvider>
			<identity>
				<companyName>WiseTech Global</companyName>
				<abn>41 065 894 724</abn>
			</identity>
		</serviceProvider>
		<primaryUnitInstallation>
			<vehicleIdentity>
				<vin>{vin}</vin>
			</vehicleIdentity>
			<installedDevice>
				<deviceIdentity>
					<id>{deviceIdentity}</id>
					<type>IVU</type>
				</deviceIdentity>
				<installationDateTime>2020-11-12T01:24:55.068</installationDateTime>
				<deviceLocation>Under Dash</deviceLocation>
			</installedDevice>
		</primaryUnitInstallation>
		<issuedDateTime>2020-11-12T01:24:55.068</issuedDateTime>
	</serviceProviderSection>
	<approvalSection>
		<approved>true</approved>
		<issuedDateTime>2020-11-12T01:24:55.068</issuedDateTime>
	</approvalSection>
</tde-enr:enrolmentForm>";
		}

		string GetExpectedEnrolmentCancellationString(string enrolmentId, string deviceIdentity, string vin, string registrationNumber, string registrationState)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentForm xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07"">
	<application>
		<name>RIM</name>
		<version>1.02</version>
	</application>
	<identifier>{enrolmentId}</identifier>
	<enrolmentProcess>ASP</enrolmentProcess>
	<authoritySection>
		<authority>
			<authorityCode>NSW</authorityCode>
		</authority>
		<scheme>OSOM</scheme>
	</authoritySection>
	<statusCode>CANCELLED</statusCode>
	<commencementDateTime>2020-11-12T01:24:55.068</commencementDateTime>
	<cessationDateTime>2020-11-12T11:24:55.068</cessationDateTime>
	<operatorSection>
		<operator>
			<identity>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
				<name>ABC Co</name>
			</identity>
			<postalAddress>
				<lineOne>Some Place</lineOne>
				<lineTwo />
				<locality>Melbourne</locality>
				<stateCode>VIC</stateCode>
				<postCode>2100</postCode>
			</postalAddress>
			<businessHoursPhone>94811111</businessHoursPhone>
		</operator>
		<primaryUnitInformation>
			<registration>
				<number>{registrationNumber}</number>
				<stateCode>{registrationState}</stateCode>
			</registration>
			<identity>
				<vin>{vin}</vin>
			</identity>
		</primaryUnitInformation>
	</operatorSection>
	<serviceProviderSection>
		<serviceProvider>
			<identity>
				<companyName>WiseTech Global</companyName>
				<abn>41 065 894 724</abn>
			</identity>
		</serviceProvider>
		<primaryUnitInstallation>
			<vehicleIdentity>
				<vin>{vin}</vin>
			</vehicleIdentity>
			<installedDevice>
				<deviceIdentity>
					<id>{deviceIdentity}</id>
					<type>IVU</type>
				</deviceIdentity>
				<installationDateTime>2020-11-12T01:24:55.068</installationDateTime>
			</installedDevice>
		</primaryUnitInstallation>
		<issuedDateTime>2020-11-12T11:24:55.068</issuedDateTime>
	</serviceProviderSection>
	<approvalSection>
		<approved>true</approved>
		<issuedDateTime>2020-11-12T11:24:55.068</issuedDateTime>
	</approvalSection>
</tde-enr:enrolmentForm>";
		}

		Mock<IHttpClientFactory> httpClientFactoryMock;
		HttpClient httpClient;
		Mock<HttpMessageHandler> httpMessageHandlerMock;
		Mock<IEHubMessageSender> eHubMessageSenderMock;
		DateTimeOffset testTime;
		ClientCompany clientCompany;
		ClientDeviceHeader clientDevice;
		OrgCusCode orgCusCode;

		RimEnrolmentRequestProcessor processor;
	}
}

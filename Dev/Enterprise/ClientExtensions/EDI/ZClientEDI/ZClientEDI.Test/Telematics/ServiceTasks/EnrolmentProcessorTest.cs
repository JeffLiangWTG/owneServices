using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.MasterFiles.Business;

namespace ZClientEDI.Test.Telematics.ServiceTasks
{
	class EnrolmentProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			enrollmentProcessor = new EnrolmentProcessor();
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41243;
			clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			clientDevice = Factory.New<ClientDeviceHeader>();
			clientDevice.CDH_Identifier = "1";
			orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
		}

		ClientCompany clientCompany;
		ClientDeviceHeader clientDevice;
		OrgCusCode orgCusCode;
		IEnrolmentProcessor enrollmentProcessor;

		public void TestEntryTimeDuringReportingMonth()
		{
			// Arrange
			var time = new DateTimeOffset(2016, 9, 10, 10, 10, 10, 10, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var enrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", "ABC-123", "12345678901234567", TcaCommonXml.RegistrationStateEnum.NSW, time),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", "ABC-321", "12345678901234567", TcaCommonXml.RegistrationStateEnum.QLD, time.AddDays(-5), time.AddDays(10)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", "ABC-123", "76543210987654321", TcaCommonXml.RegistrationStateEnum.ACT, time, time.AddMonths(2)),
			};
			Factory.Save();

			ExpectedResultsTest(@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentReport xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07"">
	<application>
		<name>RIM</name>
		<version>1.02</version>
	</application>
	<reportPeriod>
		<startDateTime>2016-09-01T00:00:00</startDateTime>
		<endDateTime>2016-10-01T00:00:00</endDateTime>
	</reportPeriod>
	<serviceProvider>
		<identity>
			<companyName>WiseTech Global</companyName>
			<abn>41 065 894 724</abn>
		</identity>
	</serviceProvider>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>NSW</stateCode>
			</registration>
			<identity>
				<vin>12345678901234567</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>01</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<entryDateTime>2016-09-10T10:10:10.01</entryDateTime>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-321</number>
				<stateCode>QLD</stateCode>
			</registration>
			<identity>
				<vin>12345678901234567</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>03</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<entryDateTime>2016-09-05T10:10:10.01</entryDateTime>
		<exitDateTime>2016-09-20T10:10:10.01</exitDateTime>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>ACT</stateCode>
			</registration>
			<identity>
				<vin>76543210987654321</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>02</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<entryDateTime>2016-09-10T10:10:10.01</entryDateTime>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<issuedDateTime>0001-01-01T00:00:00</issuedDateTime>
</tde-enr:enrolmentReport>",
				enrollments,
				enrollmentPeriodStart,
				enrollmentPeriodEnd);
		}

		public void TestExitTimeDuringReportingMonth()
		{
			// Arrange
			var time = new DateTimeOffset(2016, 9, 12, 5, 3, 2, 1, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var enrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", "ABC-123", "12345678901234567", TcaCommonXml.RegistrationStateEnum.NSW, time, time.AddDays(10)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", "ABC-123", "76543210987654321", TcaCommonXml.RegistrationStateEnum.ACT, time.AddMonths(-5), time.AddDays(5)),
			};
			Factory.Save();

			ExpectedResultsTest(@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentReport xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07"">
	<application>
		<name>RIM</name>
		<version>1.02</version>
	</application>
	<reportPeriod>
		<startDateTime>2016-09-01T00:00:00</startDateTime>
		<endDateTime>2016-10-01T00:00:00</endDateTime>
	</reportPeriod>
	<serviceProvider>
		<identity>
			<companyName>WiseTech Global</companyName>
			<abn>41 065 894 724</abn>
		</identity>
	</serviceProvider>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>NSW</stateCode>
			</registration>
			<identity>
				<vin>12345678901234567</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>01</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<entryDateTime>2016-09-12T05:03:02.001</entryDateTime>
		<exitDateTime>2016-09-22T05:03:02.001</exitDateTime>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>ACT</stateCode>
			</registration>
			<identity>
				<vin>76543210987654321</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>02</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<exitDateTime>2016-09-17T05:03:02.001</exitDateTime>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<issuedDateTime>0001-01-01T00:00:00</issuedDateTime>
</tde-enr:enrolmentReport>",
				enrollments,
				enrollmentPeriodStart,
				enrollmentPeriodEnd);
		}

		public void TestNeitherEntryNotExitTimeDuringReportingMonth()
		{
			// Arrange
			var time = new DateTimeOffset(2020, 11, 17, 3, 2, 15, 25, TimeSpan.Zero);
			var enrollmentPeriodStart = new DateTimeOffset(time.Year, time.Month, 1, 0, 0, 0, TimeSpan.Zero);
			var enrollmentPeriodEnd = enrollmentPeriodStart.AddMonths(1);
			var enrollments = new[]
			{
				GetRegistration(clientCompany, clientDevice, orgCusCode, "01", "OSOM", "ABC-123", "12345678901234567", TcaCommonXml.RegistrationStateEnum.NSW, time.AddMonths(-3), time.AddMonths(3)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "02", "OSOM", "ABC-123", "76543210987654321", TcaCommonXml.RegistrationStateEnum.ACT, time.AddMonths(3), time.AddMonths(5)),
				GetRegistration(clientCompany, clientDevice, orgCusCode, "03", "OSOM", "ABC-321", "12345678901234567", TcaCommonXml.RegistrationStateEnum.QLD, time.AddMonths(-5), time.AddMonths(-3)),
			};
			Factory.Save();

			ExpectedResultsTest(
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentReport xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07"">
	<application>
		<name>RIM</name>
		<version>1.02</version>
	</application>
	<reportPeriod>
		<startDateTime>2020-11-01T00:00:00</startDateTime>
		<endDateTime>2020-12-01T00:00:00</endDateTime>
	</reportPeriod>
	<serviceProvider>
		<identity>
			<companyName>WiseTech Global</companyName>
			<abn>41 065 894 724</abn>
		</identity>
	</serviceProvider>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>NSW</stateCode>
			</registration>
			<identity>
				<vin>12345678901234567</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>01</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-123</number>
				<stateCode>ACT</stateCode>
			</registration>
			<identity>
				<vin>76543210987654321</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>02</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>ABC-321</number>
				<stateCode>QLD</stateCode>
			</registration>
			<identity>
				<vin>12345678901234567</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>03</enrolmentIdentifier>
			<scheme>OSOM</scheme>
			<operator>
				<companyName>ABC Co</companyName>
				<abn>1234567890</abn>
			</operator>
		</enrolment>
		<installedDevice>
			<id>1</id>
		</installedDevice>
	</enrolmentSummary>
	<issuedDateTime>0001-01-01T00:00:00</issuedDateTime>
</tde-enr:enrolmentReport>",
				enrollments,
				enrollmentPeriodStart,
				enrollmentPeriodEnd);
		}

		public void ExpectedResultsTest(string expectedMessage, ClientTelRimRegistration[] enrollments, DateTimeOffset enrollmentPeriodStart, DateTimeOffset enrollmentPeriodEnd)
		{
			// Arrange
			// Act
			var message = enrollmentProcessor.Process(enrollments, enrollmentPeriodStart, enrollmentPeriodEnd);

			// Assert
			var result = TcaXmlSerializer.SerializeToTelematicsRimData(message, "http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07");
			AssertEquals(expectedMessage, result);
		}

		ClientTelRimRegistration GetRegistration(ClientCompany company, ClientDeviceHeader device, OrgCusCode code, string enrolmentId, string scheme, string reg, string vin, TcaCommonXml.RegistrationStateEnum state, DateTimeOffset startTime, DateTimeOffset? endTime = null)
		{
			var clientTelRimRegistration = Factory.New<ClientTelRimRegistration>();
			clientTelRimRegistration.TRR_LCC_ClientCompany = company.PK;
			clientTelRimRegistration.TRR_CDH_ClientDeviceHeader = device.PK;
			clientTelRimRegistration.TRR_OK_OrgCusCode = code.PK;
			clientTelRimRegistration.TRR_EnrolmentId = enrolmentId;
			clientTelRimRegistration.TRR_EnrolmentScheme = scheme;
			clientTelRimRegistration.TRR_StartTime = startTime;
			clientTelRimRegistration.TRR_VehicleRegistration = reg;
			clientTelRimRegistration.TRR_VehicleIdentificationNumber = vin;
			clientTelRimRegistration.TRR_VehicleRegistrationState = Enum.GetName(typeof(TcaCommonXml.RegistrationStateEnum), state);
			clientTelRimRegistration.TRR_InstallationDateTimeOffset = startTime;
			if (endTime != null)
			{
				clientTelRimRegistration.TRR_EndTime = (DateTimeOffset)endTime;
			}
			return clientTelRimRegistration;
		}
	}
}

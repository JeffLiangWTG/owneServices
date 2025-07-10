using System;

using Enterprise.Client.EDI.Telematics.Tca;

namespace ZClientEDI.Test.Telematics.Tca
{
	class EnrolmentReportTest : TcaXmlTestCase
	{
		public void TestEnrolmentReportXml()
		{
			CombineAssertions(
				() =>
				{
					var enrolmentReportTest1 = GenerateEnrolmentReportTestCase("RIM", "1.02", "XYZ0000001", "VIC", "SCHEME", "12341234", "43214321", "ABC123", "JJL012", "Wisetech global", "JJLAWSON");
					CheckXml(enrolmentReportTest1.xml, enrolmentReportTest1.message, "http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07");

					var enrolmentReportTest2 = GenerateEnrolmentReportTestCase("RIM", "2.03", "5543546", "NSW", "MSL", "11111111", "22222222", "WTG321", "ACF-025", "Google", "Apple");
					CheckXml(enrolmentReportTest2.xml, enrolmentReportTest2.message, "http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07");
				});
		}

		(string xml, EnrolmentReportType message) GenerateEnrolmentReportTestCase(string name, string version, string identifier, string authorityCode, string scheme, string abn1, string abn2, string regNumber, string vin, string provider, string subscriber)
		{
			string xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentReport xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment-report/2018-07"">
	<application>
		<name>{name}</name>
		<version>{version}</version>
	</application>
	<identifier>{identifier}</identifier>
	<authorityCode>{authorityCode}</authorityCode>
	<reportPeriod>
		<startDateTime>2019-11-01T00:00:00</startDateTime>
		<endDateTime>2019-11-30T23:59:59</endDateTime>
	</reportPeriod>
	<serviceProvider>
		<identity>
			<companyName>{provider}</companyName>
			<abn>{abn1}</abn>
		</identity>
	</serviceProvider>
	<enrolmentSummary>
		<vehicle>
			<registration>
				<number>{regNumber}</number>
				<stateCode>VIC</stateCode>
			</registration>
			<identity>
				<vin>{vin}</vin>
			</identity>
		</vehicle>
		<enrolment>
			<enrolmentIdentifier>{identifier}</enrolmentIdentifier>
			<scheme>{scheme}</scheme>
			<operator>
				<companyName>{subscriber}</companyName>
				<abn>{abn2}</abn>
			</operator>
		</enrolment>
		<entryDateTime>2019-11-28T04:14:42</entryDateTime>
		<exitDateTime>2019-11-28T04:14:42</exitDateTime>
		<installedDevice>
			<id>IVU123</id>
		</installedDevice>
	</enrolmentSummary>
	<issuedDateTime>2019-11-28T04:14:42</issuedDateTime>
</tde-enr:enrolmentReport>";

			var message = new EnrolmentReportType
			{
				application = new TcaCommonXml.ApplicationReferenceType
				{
					name = name,
					version = version,
				},
				identifier = identifier,
				authorityCode = authorityCode,
				reportPeriod = new EnrolmentReportType.DateTimePeriodType
				{
					startDateTime = new DateTime(2019, 11, 1, 0, 0, 0),
					endDateTime = new DateTime(2019, 11, 30, 23, 59, 59),
				},
				serviceProvider = new TcaCommonXml.ServiceProviderInformationType
				{
					identity = new TcaCommonXml.CompanyIdentificationType
					{
						abn = abn1,
						companyName = provider,
					},
				},
				enrolmentSummary = new[]
				{
					new EnrolmentReportType.EnrolmentSummaryType
					{
						installedDevice = new TcaCommonXml.DeviceIdentityType
						{
							id = "IVU123",
						},
						enrolment = new []
						{
							new EnrolmentReportType.EnrolmentReferenceType1
							{
								enrolmentIdentifier = identifier,
								scheme = scheme,
								@operator = new TcaCommonXml.OperatorIdentificationType
								{
									companyName = subscriber,
									abn = abn2,
								},
							},
						},
						entryDateTime = new DateTime(2019, 11, 28, 4, 14, 42),
						exitDateTime = new DateTime(2019, 11, 28, 4, 14, 42),
						entryDateTimeSpecified = true,
						exitDateTimeSpecified = true,
						Item = new TcaCommonXml.VehicleInformationType
						{
							identity = new TcaCommonXml.VehicleIdentityType
							{
								ItemElementName = TcaCommonXml.ItemChoiceType.vin,
								Item = vin,
							},
							registration = new TcaCommonXml.VehicleRegistrationType
							{
								number = regNumber,
								stateCode = TcaCommonXml.RegistrationStateEnum.VIC,
							},
						},
					},
				},
				issuedDateTime = new DateTime(2019, 11, 28, 4, 14, 42),
			};

			return (xml, message);
		}
	}
}

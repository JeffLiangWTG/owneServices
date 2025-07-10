using System;
using System.Globalization;

using Enterprise.Client.EDI.Telematics.Tca;

namespace ZClientEDI.Test.Telematics.Tca
{
	class EnrolmentFormTest : TcaXmlTestCase
	{
		public void TestEnrolmentFormXml()
		{
			CombineAssertions(
				() =>
				{
					var enrolmentFormTest1 = GenerateEnrolmentFormTestCase("RIM", "1.02", "XYZ0000001", "VIC", "SCHEME", "12341234", "43214321", "ABC123", "JJL012", "Wisetech global", "JJLAWSON", EnrolmentFormType.EnrolmentProcessEnum.ASP, EnrolmentFormType.EnrolmentStatusEnum.APPROVED, "123 Blah street", "Sydney", "2000", TcaCommonXml.StateEnum.NSW, TcaCommonXml.RegistrationStateEnum.VIC, "ABC123456", TcaCommonXml.DeviceTypeType.IVU, true, new DateTime(2019, 1, 2, 3, 4, 5));
					CheckXml(enrolmentFormTest1.xml, enrolmentFormTest1.message, "http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07");

					var enrolmentFormTest2 = GenerateEnrolmentFormTestCase("RIM", "2.03", "5543546", "NSW", "MSL", "11111111", "22222222", "WTG321", "ACF-025", "Google", "Apple", EnrolmentFormType.EnrolmentProcessEnum.AUTHORITY, EnrolmentFormType.EnrolmentStatusEnum.CANCELLED, "321 NotBlah street", "Melbourne", "3000", TcaCommonXml.StateEnum.VIC, TcaCommonXml.RegistrationStateEnum.NSW, "CDA3214512", TcaCommonXml.DeviceTypeType.TID, false, new DateTime(2019, 6, 5, 4, 3, 2));
					CheckXml(enrolmentFormTest2.xml, enrolmentFormTest2.message, "http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07");
				});
		}

		public void TestEnrolmentFormOrder()
		{
			// Arrange
			var testData = GenerateEnrolmentFormTestCase("RIM", "1.02", "TCA00000001", "NSW", "SCHEME", "23112936991", "12345678901", "ABC123", "ABCDEFG1234567890", "Transport Certification Australia", "Transport Operator Company", EnrolmentFormType.EnrolmentProcessEnum.ASP, EnrolmentFormType.EnrolmentStatusEnum.APPROVED, "123 Example Street", "Melbourne", "3000", TcaCommonXml.StateEnum.VIC, TcaCommonXml.RegistrationStateEnum.VIC, "ABC123456", TcaCommonXml.DeviceTypeType.IVU, true, new DateTime(2019, 11, 28, 1, 12, 28));
			var enrolmentForm = testData.message;

			// Act
			var serialized = TcaXmlSerializer.SerializeToTelematicsRimData(enrolmentForm, "http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07");

			// Assert
			AssertEquals(TcaProvidedXmlExample, serialized);
		}

		(string xml, EnrolmentFormType message) GenerateEnrolmentFormTestCase(
			string name,
			string version,
			string identifier,
			string authorityCode,
			string scheme,
			string abn1,
			string abn2,
			string regNumber,
			string vin,
			string provider,
			string subscriber,
			EnrolmentFormType.EnrolmentProcessEnum enrolmentProcess,
			EnrolmentFormType.EnrolmentStatusEnum enrolmentStatus,
			string address,
			string locality,
			string postcode,
			TcaCommonXml.StateEnum state,
			TcaCommonXml.RegistrationStateEnum registrationState,
			string deviceId,
			TcaCommonXml.DeviceTypeType deviceType,
			bool approved,
			DateTime someTime)
		{
			var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<tde-enr:enrolmentForm xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:tde-enr=""http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07"">
	<application>
		<name>{name}</name>
		<version>{version}</version>
	</application>
	<identifier>{identifier}</identifier>
	<enrolmentProcess>{Enum.GetName(typeof(EnrolmentFormType.EnrolmentProcessEnum), enrolmentProcess)}</enrolmentProcess>
	<authoritySection>
		<authority>
			<authorityCode>{authorityCode}</authorityCode>
		</authority>
		<scheme>{scheme}</scheme>
	</authoritySection>
	<statusCode>{Enum.GetName(typeof(EnrolmentFormType.EnrolmentStatusEnum), enrolmentStatus)}</statusCode>
	<commencementDateTime>{someTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</commencementDateTime>
	<operatorSection>
		<operator>
			<identity>
				<companyName>{subscriber}</companyName>
				<abn>{abn2}</abn>
				<name>{subscriber}</name>
			</identity>
			<postalAddress>
				<lineOne>{address}</lineOne>
				<locality>{locality}</locality>
				<stateCode>{Enum.GetName(typeof(TcaCommonXml.StateEnum), state)}</stateCode>
				<postCode>{postcode}</postCode>
			</postalAddress>
			<businessHoursPhone>1234567890</businessHoursPhone>
		</operator>
		<primaryUnitInformation>
			<registration>
				<number>{regNumber}</number>
				<stateCode>{Enum.GetName(typeof(TcaCommonXml.RegistrationStateEnum), registrationState)}</stateCode>
			</registration>
			<identity>
				<vin>{vin}</vin>
			</identity>
		</primaryUnitInformation>
	</operatorSection>
	<serviceProviderSection>
		<serviceProvider>
			<identity>
				<companyName>{provider}</companyName>
				<abn>{abn1}</abn>
			</identity>
		</serviceProvider>
		<primaryUnitInstallation>
			<vehicleIdentity>
				<vin>{vin}</vin>
			</vehicleIdentity>
			<installedDevice>
				<deviceIdentity>
					<id>{deviceId}</id>
					<type>{Enum.GetName(typeof(TcaCommonXml.DeviceTypeType), deviceType)}</type>
				</deviceIdentity>
				<installationDateTime>{someTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</installationDateTime>
				<deviceLocation>Under Dash</deviceLocation>
			</installedDevice>
		</primaryUnitInstallation>
		<issuedDateTime>{someTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</issuedDateTime>
	</serviceProviderSection>
	<approvalSection>
		<approved>{approved.ToString().ToLower()}</approved>
		<issuedDateTime>{someTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</issuedDateTime>
	</approvalSection>
</tde-enr:enrolmentForm>";

			var message = new EnrolmentFormType
			{
				application = new TcaCommonXml.ApplicationReferenceType
				{
					name = name,
					version = version,
				},
				identifier = identifier,
				enrolmentProcess = enrolmentProcess,
				authoritySection = new EnrolmentFormType.AuthoritySectionType
				{
					authority = new TcaCommonXml.AuthorityInformationType
					{
						authorityCode = authorityCode,
					},
					scheme = scheme,
				},
				statusCode = enrolmentStatus,
				commencementDateTime = someTime,
				operatorSection = new EnrolmentFormType.OperatorSectionType
				{
					@operator = new EnrolmentFormType.OperatorInformationType
					{
						identity = new TcaCommonXml.OperatorIdentificationType
						{
							abn = abn2,
							name = subscriber,
							companyName = subscriber,
						},
						postalAddress = new TcaCommonXml.AddressType
						{
							lineOne = address,
							locality = locality,
							postCode = postcode,
							stateCode = state,
						},
						businessHoursPhone = "1234567890",
					},
					primaryUnitInformation = new TcaCommonXml.PrimaryUnitInformationType
					{
						identity = new TcaCommonXml.VehicleIdentityType
						{
							ItemElementName = TcaCommonXml.ItemChoiceType.vin,
							Item = vin,
						},
						registration = new TcaCommonXml.VehicleRegistrationType
						{
							number = regNumber,
							stateCode = registrationState
						},
					},
				},
				serviceProviderSection = new TcaCommonXml.ServiceProviderSectionType
				{
					serviceProvider = new TcaCommonXml.ServiceProviderInformationType
					{
						identity = new TcaCommonXml.CompanyIdentificationType
						{
							abn = abn1,
							companyName = provider,
						},
					},
					primaryUnitInstallation = new TcaCommonXml.PrimaryUnitInstallationType
					{
						installedDevice = new[]
							{
								new TcaCommonXml.DeviceInstallationType
								{
									deviceIdentity = new TcaCommonXml.DeviceIdentityType
									{
										id = deviceId,
										type = deviceType,
										typeSpecified = true,
									},
									deviceLocation = "Under Dash",
									installationDateTime = someTime,
								},
							},
						vehicleIdentity = new TcaCommonXml.VehicleIdentityType
						{
							ItemElementName = TcaCommonXml.ItemChoiceType.vin,
							Item = vin,
						},
					},
					issuedDateTime = someTime,
				},
				approvalSection = new EnrolmentFormType.ApprovalSectionType
				{
					approved = approved,
					issuedDateTime = someTime,
				},
			};

			return (xml, message);
		}

		const string TcaProvidedXmlExample = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<tde-enr:enrolmentForm xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:tde-enr=\"http://www.tca.gov.au/schemas/tde/core/enrolment/2018-07\">\r\n\t<application>\r\n\t\t<name>RIM</name>\r\n\t\t<version>1.02</version>\r\n\t</application>\r\n\t<identifier>TCA00000001</identifier>\r\n\t<enrolmentProcess>ASP</enrolmentProcess>\r\n\t<authoritySection>\r\n\t\t<authority>\r\n\t\t\t<authorityCode>NSW</authorityCode>\r\n\t\t</authority>\r\n\t\t<scheme>SCHEME</scheme>\r\n\t</authoritySection>\r\n\t<statusCode>APPROVED</statusCode>\r\n\t<commencementDateTime>2019-11-28T01:12:28</commencementDateTime>\r\n\t<operatorSection>\r\n\t\t<operator>\r\n\t\t\t<identity>\r\n\t\t\t\t<companyName>Transport Operator Company</companyName>\r\n\t\t\t\t<abn>12345678901</abn>\r\n\t\t\t\t<name>Transport Operator Company</name>\r\n\t\t\t</identity>\r\n\t\t\t<postalAddress>\r\n\t\t\t\t<lineOne>123 Example Street</lineOne>\r\n\t\t\t\t<locality>Melbourne</locality>\r\n\t\t\t\t<stateCode>VIC</stateCode>\r\n\t\t\t\t<postCode>3000</postCode>\r\n\t\t\t</postalAddress>\r\n\t\t\t<businessHoursPhone>1234567890</businessHoursPhone>\r\n\t\t</operator>\r\n\t\t<primaryUnitInformation>\r\n\t\t\t<registration>\r\n\t\t\t\t<number>ABC123</number>\r\n\t\t\t\t<stateCode>VIC</stateCode>\r\n\t\t\t</registration>\r\n\t\t\t<identity>\r\n\t\t\t\t<vin>ABCDEFG1234567890</vin>\r\n\t\t\t</identity>\r\n\t\t</primaryUnitInformation>\r\n\t</operatorSection>\r\n\t<serviceProviderSection>\r\n\t\t<serviceProvider>\r\n\t\t\t<identity>\r\n\t\t\t\t<companyName>Transport Certification Australia</companyName>\r\n\t\t\t\t<abn>23112936991</abn>\r\n\t\t\t</identity>\r\n\t\t</serviceProvider>\r\n\t\t<primaryUnitInstallation>\r\n\t\t\t<vehicleIdentity>\r\n\t\t\t\t<vin>ABCDEFG1234567890</vin>\r\n\t\t\t</vehicleIdentity>\r\n\t\t\t<installedDevice>\r\n\t\t\t\t<deviceIdentity>\r\n\t\t\t\t\t<id>ABC123456</id>\r\n\t\t\t\t\t<type>IVU</type>\r\n\t\t\t\t</deviceIdentity>\r\n\t\t\t\t<installationDateTime>2019-11-28T01:12:28</installationDateTime>\r\n\t\t\t\t<deviceLocation>Under Dash</deviceLocation>\r\n\t\t\t</installedDevice>\r\n\t\t</primaryUnitInstallation>\r\n\t\t<issuedDateTime>2019-11-28T01:12:28</issuedDateTime>\r\n\t</serviceProviderSection>\r\n\t<approvalSection>\r\n\t\t<approved>true</approved>\r\n\t\t<issuedDateTime>2019-11-28T01:12:28</issuedDateTime>\r\n\t</approvalSection>\r\n</tde-enr:enrolmentForm>";
	}
}

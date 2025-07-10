using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace ZClientEDI.Business.Test.Registry.Telematics
{
	[TestedType(typeof(TcaRimEnrollmentSchemeRegistryDataType))]
	class TcaRimEnrollmentSchemeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TcaRimEnrollmentSchemeRegistryDataType>
	{
		protected override TcaRimEnrollmentSchemeRegistryDataType GetNewDataType()
		{
			return new TcaRimEnrollmentSchemeRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return null; }
		}

		// Using custom EditorInfo
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new TcaRimEnrollmentSchemeCollection();
			sample.Add("AAA", (NoResString)"DESC A", true);
			sample.Add("BBB", (NoResString)"DESC B", false);

			var sample2 = new TcaRimEnrollmentSchemeCollection();
			sample.Add("CCC", (NoResString)"DESC C", true);
			sample.Add("DDD", (NoResString)"DESC D", false);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, DataType.Serialise(sample)),
				new ValidSampleAndBinaryValueInDB(sample2, DataType.Serialise(sample2)),
			};
		}
	}

	class TcaRimEnrollmentSchemeRegistryDataTypeFactoryTest : TestCaseWithFactory
	{
		public void TestValidationChecksForUnusedEntries()
		{
			// Arrange
			var registryDataType = new TcaRimEnrollmentSchemeRegistryDataType();
			var collection = new TcaRimEnrollmentSchemeCollection();
			var registration = GetNewClientTelRimRegistrationBusinessObject(Factory, "ASD");
			Factory.Save();

			// Act
			// Assert
			AssertExceptionThrown<RegistryValidationException>(
				() => registryDataType.ValidateBeforeRegistryFormSave(EDIDataRegistry.Instance.TcaRimEnrollmentSchemeDescriptions, collection, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		BusinessObject GetNewClientTelRimRegistrationBusinessObject(BusinessObjectFactory factory, string scheme)
		{
			var code = factory.NewWithValidTestData<OrgCusCode>();
			var org = BillingTestHelper.CreateOrganisation(factory, "DAB", "BAC", "MLB");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41244;
			var company = ClientCompany.FindOrCreate(factory, "BAC", db.PK, org.PK, "", "");
			var device = factory.New<ClientDeviceHeader>();
			device.CDH_Identifier = "2";

			return GetRegistration(factory, company, device, code, "0102030405060708090A0B0C", scheme);
		}

		ClientTelRimRegistration GetRegistration(BusinessObjectFactory factory, ClientCompany company, ClientDeviceHeader device, OrgCusCode code, string enrolmentId, string scheme)
		{
			var clientTelRimRegistration = factory.New<ClientTelRimRegistration>();
			clientTelRimRegistration.TRR_LCC_ClientCompany = company.PK;
			clientTelRimRegistration.TRR_CDH_ClientDeviceHeader = device.PK;
			clientTelRimRegistration.TRR_OK_OrgCusCode = code.PK;
			clientTelRimRegistration.TRR_EnrolmentId = enrolmentId;
			clientTelRimRegistration.TRR_EnrolmentScheme = scheme;
			clientTelRimRegistration.TRR_StartTime = ZDateTimeOffset.UtcNow;
			clientTelRimRegistration.TRR_VehicleIdentificationNumber = "12345678901234567";
			clientTelRimRegistration.TRR_VehicleRegistration = "ABC-123";
			clientTelRimRegistration.TRR_InstallationDateTimeOffset = ZDateTimeOffset.UtcNow;
			return clientTelRimRegistration;
		}
	}
}

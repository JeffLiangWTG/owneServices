using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	internal class ClientDeviceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIdentifierUniqueForDevices()
		{
			var device1 = Factory.New<ClientDeviceHeader>();
			device1.CDH_Identifier = "1";
			AssertNoErrors(device1.CDH_IdentifierInfo);

			var device2 = Factory.New<ClientDeviceHeader>();
			device2.CDH_Identifier = "1";
			AssertHasErrors(device2.CDH_IdentifierInfo);

			device1.RunPreSaveValidation();
			device2.RunPreSaveValidation();
			AssertHasErrors(device1.CDH_IdentifierInfo);
			AssertHasErrors(device2.CDH_IdentifierInfo);
		}

		public void TestIdentifierCannotConflictWithNumberFountain()
		{
			// Arrange
			var device1 = Factory.New<ClientDeviceHeader>();
			var device2 = Factory.New<ClientDeviceHeader>();
			var device3 = Factory.New<ClientDeviceHeader>();

			// Act
			device1.CDH_Identifier = "TD12345";
			device2.CDH_Identifier = "00001TD";
			device3.CDH_Identifier = "TDD0001";

			// Assert
			AssertHasErrors(device1.CDH_IdentifierInfo);
			AssertNoErrors(device2.CDH_IdentifierInfo);
			AssertNoErrors(device3.CDH_IdentifierInfo);
		}

		public void TestModelIdUniqueForTemplates()
		{
			var device1 = Factory.New<ClientDeviceHeader>();
			device1.CDH_ModelID = "RAK";

			var device2 = Factory.New<ClientDeviceHeader>();
			device2.CDH_ModelID = "RAK";
			AssertNoErrors(device2.CDH_ModelIDInfo);

			var device3 = Factory.New<ClientDeviceHeader>();
			device3.CDH_IsTemplate = true;
			device3.CDH_ModelID = "RAK";
			AssertNoErrors(device3.CDH_ModelIDInfo);

			var device4 = Factory.New<ClientDeviceHeader>();
			device4.CDH_IsTemplate = true;
			device4.CDH_ModelID = "RAK";
			AssertHasErrors(device4.CDH_ModelIDInfo);

			device1.RunPreSaveValidation();
			device2.RunPreSaveValidation();
			device3.RunPreSaveValidation();
			device4.RunPreSaveValidation();
			AssertNoErrors(device1.CDH_ModelIDInfo);
			AssertNoErrors(device2.CDH_ModelIDInfo);
			AssertHasErrors(device3.CDH_ModelIDInfo);
			AssertHasErrors(device4.CDH_ModelIDInfo);
		}

		public void TestWarningIfAssignedWithoutSOF()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_ModelID = "RAK";
			device.CDH_EnterpriseCode = "AAA";
			device.CDH_ServerCode = "AAA";

			device.RunPreSaveValidation();
			AssertHasWarning(device.CDH_ServerCodeInfo, "This device has no hardware identifier. It will not be visible on client systems.");
			AssertHasWarning(device.CDH_EnterpriseCodeInfo, "This device has no hardware identifier. It will not be visible on client systems.");
		}

		public void TestNoWarningIfUnassignedWithoutHardwareId()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_ModelID = "RAK";

			var component = device.Components.AddNew();

			var identification = component.Identifiers.AddNew();

			device.RunPreSaveValidation();
			AssertNoWarnings(device.CDH_ServerCodeInfo);
			AssertNoWarnings(device.CDH_EnterpriseCodeInfo);
		}

		public void TestNoWarningIfAssignedWithHardwareId()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_ModelID = "RAK";
			device.CDH_EnterpriseCode = "AAA";
			device.CDH_ServerCode = "AAA";

			device.CDH_DeviceIdentifier = "boo";

			device.RunPreSaveValidation();
			AssertNoWarnings(device.CDH_ServerCodeInfo);
			AssertNoWarnings(device.CDH_EnterpriseCodeInfo);
		}

		public void TestNoWarningIfUnassignedWithHardwareId()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_ModelID = "RAK";
			device.CDH_DeviceIdentifier = "boo";

			device.RunPreSaveValidation();
			AssertNoWarnings(device.CDH_ServerCodeInfo);
			AssertNoWarnings(device.CDH_EnterpriseCodeInfo);
		}

		public void TestNoErrorIfNotAssignedToEnterprise()
		{
			var device = Factory.New<ClientDeviceHeader>();

			device.RunPreSaveValidation();
			AssertNoErrors(device.CDH_ServerCodeInfo);
			AssertNoErrors(device.CDH_EnterpriseCodeInfo);
		}

		public void TestErrorIfAssignedToInvalidEnterprise()
		{
			var device = Factory.New<ClientDeviceHeader>();

			device.CDH_EnterpriseCode = "BBB";

			device.RunPreSaveValidation();
			AssertHasErrors(device.CDH_EnterpriseCodeInfo);
			AssertHasError(device.CDH_EnterpriseCodeInfo, "Enter a valid Enterprise Code.");
		}

		public void TestErrorIfAssignedToInvalidServerCode()
		{
			var device = Factory.New<ClientDeviceHeader>();

			device.CDH_ServerCode = "BBB";

			device.RunPreSaveValidation();
			AssertHasErrors(device.CDH_ServerCodeInfo);
			AssertHasError(device.CDH_ServerCodeInfo, "Enter a valid Server Code.");
		}

		public void TestNoErrorIfAssignedToValidEnterpriseAndServerCode()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var ent = Factory.New<LicenceEnterprise>();
			ent.LE_OH = org.PK;
			ent.LE_EnterpriseCode = "BBB";

			var svr = Factory.New<LicenceDatabase>();
			svr.LD_LE = ent.PK;
			svr.LD_ServerCode = "AAA";

			Factory.Save();

			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_EnterpriseCode = "BBB";
			device.CDH_ServerCode = "AAA";

			device.RunPreSaveValidation();

			AssertNoErrors(device.CDH_EnterpriseCodeInfo);
			AssertNoErrors(device.CDH_ServerCodeInfo);
		}
	}
}

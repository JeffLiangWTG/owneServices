using System;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SRRErrorNotificationOptionsRegistryDataType))]
	sealed class SRRErrorNotificationOptionsRegistryDataTypeTest : RegistryDataTypeTestCase<SRRErrorNotificationOptionsRegistryDataType>
	{
		protected override object GetNullRepresentation()
		{
			return StringRegistryDataTypeTest.GetNullStringRepresentation();
		}

		protected override SRRErrorNotificationOptionsRegistryDataType GetNewDataType()
		{
			return new SRRErrorNotificationOptionsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB("DEF", DataType.Serialise("DEF")),
				new ValidSampleAndBinaryValueInDB("ROL", DataType.Serialise("ROL")),
				new ValidSampleAndBinaryValueInDB("GRP", DataType.Serialise("GRP"))
			};
		}

		public void TestValidateBeforeRegistryFormSave_ErrorNotificationStaffRoles()
		{
			var roles = StaffRolesNotificationHelper.GetRoles();
			StaffRolesNotificationHelper.SetAllBoolsTo(roles, false);
			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var options = SystemDataRegistry.Instance.SRRErrorNotificationOptions;
			var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(options, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("The Registry item[System -> Reports -> Error Notification Staff Roles] must be configured before enabling Send to staff roles", exception.Message);

			StaffRolesNotificationHelper.SetAllBoolsTo(roles, true);
			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);
			AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(options, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidateBeforeRegistryFormSave_ErrorNotificationGroups()
		{
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			var options = SystemDataRegistry.Instance.SRRErrorNotificationOptions;
			var exception = AssertExceptionThrown<RegistryValidationException>(() => DataType.ValidateBeforeRegistryFormSave(options, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP, Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("The Registry item[System -> Reports -> Error Notification Groups] must be configured before enabling Send to notification group", exception.Message);

			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			AssertNoExceptionThrown(() => DataType.ValidateBeforeRegistryFormSave(options, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP, Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

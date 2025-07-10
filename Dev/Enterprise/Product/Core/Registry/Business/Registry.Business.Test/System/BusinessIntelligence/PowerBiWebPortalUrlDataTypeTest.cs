using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PowerBiWebPortalUrlDataType))]
	sealed class PowerBiWebPortalUrlDataTypeTest : StringRegistryDataTypeTest
	{
		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}
		protected override StringRegistryDataType GetNewDataType()
		{
			return new PowerBiWebPortalUrlDataType();
		}

		public void TestSlashRemovedForPbiUrlWhenGivenWithSlash()
		{
			var proposedValue = "url///";
			var expectedValue = "url";

			var returnedValue = PowerBiWebPortalUrlDataType.RemoveSlash(proposedValue);

			AssertEquals("Trailing slashes should have been removed", expectedValue, returnedValue);
		}

		public void TestNoChangesForPbiUrlWhenGivenWithoutSlash()
		{
			var proposedValue = "url";
			var expectedValue = "url";

			var returnedValue = PowerBiWebPortalUrlDataType.RemoveSlash(proposedValue);

			AssertEquals("Url should remain the same", expectedValue, returnedValue);
		}

		public void TestCannotSetThisItemWhenPrerequisiteHasNotBeenSet()
		{
			var dataType = GetNewDataType();
			var registryItem = new StringRegistryItem("BiPowerBiWebPortalUrl", null, null, null, RegistryStorageFlags.System);
			SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			AssertExceptionThrown(typeof(RegistryValidationException), "\"Power BI Web Portal URL\" cannot be set when \"Analysis Server\" has not been set.", () => dataType.Validate(registryItem, "TestPowerBiWebPortalUrl", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public override void TestGetSetValidValues()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" });
			SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");
			SystemDataRegistry.Instance.BiAuditServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAuditServer");
			base.TestGetSetValidValues();
		}
	}
}

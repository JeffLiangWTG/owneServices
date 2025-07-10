using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AnalysisServerDataType))]
	sealed class AnalysisServerDataTypeTest : StringRegistryDataTypeTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
		}

		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new AnalysisServerDataType();
		}

		public void TestCannotRemoveThisItemWhenPostrequisiteHasValue()
		{
			var credential = new BiReportCredential
			{
				Domain = "TestDomain",
				UserName = "TestUserName",
				Password = "P$ssword"
			};

			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, credential);
			SystemDataRegistry.Instance.BiAnalysisServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");
			SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestBiPowerBiWebPortalUrl");

			var dataType = new AnalysisServerDataType();
			var registryItem = new StringRegistryItem("BiAnalysisServer", null, null, null, RegistryStorageFlags.System);
			AssertExceptionThrown(typeof(RegistryValidationException), "\"Analysis Server\" value cannot be removed when \"Power Bi Web Portal URL\" has a value specified.", () => dataType.Validate(registryItem, "", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCannotSetThisItemWhenPrerequisiteHasNotBeenSet()
		{
			var dataType = new AnalysisServerDataType();
			var registryItem = new StringRegistryItem("BiAnalysisServer", null, null, null, RegistryStorageFlags.System);

			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestDataWarehouseServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential());

			AssertExceptionThrown(typeof(RegistryValidationException), "\"Analysis Server\" cannot be set if \"Report User Credentials\" hasn't been set.", () => dataType.Validate(registryItem, "TestValue", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public override void TestGetSetValidValues()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestAnalysisServer");
			SystemDataRegistry.Instance.BiReportUserCredential.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BiReportCredential { Domain = "test", UserName = "test", Password = "test" });
			base.TestGetSetValidValues();
		}
	}
}

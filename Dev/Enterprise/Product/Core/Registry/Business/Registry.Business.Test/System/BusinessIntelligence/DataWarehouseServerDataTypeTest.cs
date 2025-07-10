using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DataWarehouseServerDataType))]
	sealed class DataWarehouseServerDataTypeTest : StringRegistryDataTypeTest
	{
		protected override object[] GetInvalidSamples()
		{
			return Array.Empty<object>();
		}
		protected override StringRegistryDataType GetNewDataType()
		{
			return new DataWarehouseServerDataType();
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

			var dataType = new DataWarehouseServerDataType();
			var registryItem = new StringRegistryItem("BiDataWarehouseServer", null, null, null, RegistryStorageFlags.System);

			AssertExceptionThrown(typeof(RegistryValidationException), "\"Data Warehouse Server\" value cannot be removed when \"Report User Credentials\" has a value specified.", () => dataType.Validate(registryItem, "", Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

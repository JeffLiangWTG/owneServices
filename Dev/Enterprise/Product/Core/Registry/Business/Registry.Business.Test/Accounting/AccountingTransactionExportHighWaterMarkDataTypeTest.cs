using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AccountingTransactionExportHighWaterMarkDataType))]
	sealed class AccountingTransactionExportHighWaterMarkDataTypeTest : RegistryDataTypeTestCase<AccountingTransactionExportHighWaterMarkDataType>
	{
		protected override AccountingTransactionExportHighWaterMarkDataType GetNewDataType()
		{
			return new AccountingTransactionExportHighWaterMarkDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[] { new ValidSampleAndBinaryValueInDB(DateTime.MinValue, GetNewDataType().Serialise(DateTime.MinValue)) };
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { DateTime.Today };
		}

		public void TestValidation()
		{
			var dataType = new AccountingTransactionExportHighWaterMarkDataType();
			var registryItem = new DateTimeRegistryItem("", null, null, null, RegistryStorageFlags.Company, DateTime.MinValue);
			AssertExceptionThrown(typeof(RegistryValidationException), "Only the default value can be set", () => dataType.Validate(registryItem, DateTime.Now, Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

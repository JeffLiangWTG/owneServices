using System;
using System.Text;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ServiceUrlRegistryDataType))]
	sealed class ServiceUrlRegistryDataTypeTest : RegistryDataTypeTestCase<ServiceUrlRegistryDataType>
	{
		protected override ServiceUrlRegistryDataType GetNewDataType()
		{
			return new ServiceUrlRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB("http://www.google.com.au/", Encoding.Unicode.GetBytes("http://www.google.com.au/")),
				new ValidSampleAndBinaryValueInDB(string.Empty, Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString)),
			};
		}

		public void TestValidate_Url()
		{
			var registryItem = new ServiceUrlRegistryItem("dummy", null, null, null, RegistryStorageFlags.System, string.Empty);
			var dataType = (ServiceUrlRegistryDataType)registryItem.DataType;
			var exceptionMessage = "The URL is invalid, please input a valid URL that must be HTTPS or HTTP.";

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, " ", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "https://", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "https://.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "https://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "http://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ftp://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

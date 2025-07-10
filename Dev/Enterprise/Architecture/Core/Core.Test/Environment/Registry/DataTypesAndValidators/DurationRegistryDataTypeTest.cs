using System;
using System.Text;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DurationRegistryDataType))]
	sealed class DurationRegistryDataTypeTest : StringRegistryDataTypeTest
	{
		public void TestDurationFormat()
		{
			var dataType = new DurationRegistryDataType();
			var registryItem = new StringRegistryItem("dummy", null, null, null, dataType, RegistryStorageFlags.System);
			var exceptionMessage = @"Incorrect duration format.
Duration should be in HH:mm format.
e.g 08:00, 37:05 and 99:59";

			AssertNoExceptionThrown(exceptionMessage, () => dataType.Validate(registryItem, "38:32", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(exceptionMessage, () => dataType.Validate(registryItem, "99:59", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "fds", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "99:60", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "100:23", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "12", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		#region implementation

		protected override StringRegistryDataType GetNewDataType()
		{
			return new DurationRegistryDataType(0, 5);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("38:20", Encoding.Unicode.GetBytes("38:20")),
				new ValidSampleAndBinaryValueInDB("12:34", Encoding.Unicode.GetBytes("12:34"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "123:60" };
		}

		#endregion
	}
}

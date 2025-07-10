using System;
using CargoWise.Types;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NativeXMLSupportTillDateDataType))]
	sealed class NativeXMLSupportTillDateDataTypeTest : RegistryDataTypeTestCase<NativeXMLSupportTillDateDataType>
	{
		protected override NativeXMLSupportTillDateDataType GetNewDataType()
		{
			return new NativeXMLSupportTillDateDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(DateTime.MinValue, GetNewDataType().Serialise(DateTime.MinValue)),
				new ValidSampleAndBinaryValueInDB(DateTime.MinValue + TimeSpan.FromDays(69), GetNewDataType().Serialise(DateTime.MinValue + TimeSpan.FromDays(69)))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { DateTime.Today.AddMonths(4) };
		}

		public void TestValidation()
		{
			var dataType = new NativeXMLSupportTillDateDataType();
			var registryItem = new DateTimeRegistryItem("", null, null, null, RegistryStorageFlags.System, DateTime.MinValue);
			AssertExceptionThrown(typeof(RegistryValidationException), "Date cannot be greater than 3 months in the future", () => dataType.Validate(registryItem, ZDateTime.UtcNow.Date.AddMonths(4).ToDateTime(), Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

[TestedType(typeof(SecurityIdentifierRegistryDataType))]
sealed class SecurityIdentifierRegistryDataTypeTest : RegistryDataTypeTestCase<SecurityIdentifierRegistryDataType>
{
	const string InvalidValue = "Invalid Value";
	public void TestSetInvalidValues()
	{
		var item = new RegistryItemImpl("InvalidItem", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", DataType, RegistryStorageFlags.All);
		var e = AssertExceptionThrown<RegistryValidationException>(() => item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, InvalidValue));
		AssertEquals($"AD name '{InvalidValue}' not found.", e.Message);
	}

	public void TestGetInvalidValues()
	{
		var binaryValue = new StringRegistryDataType().Serialise(InvalidValue);
		var e = AssertExceptionThrown<RegistryValidationException>(() => GetNewDataType().Deserialise(binaryValue));
		AssertEquals($"AD name '{InvalidValue}' not found.", e.Message);
	}

	protected override SecurityIdentifierRegistryDataType GetNewDataType()
	{
		return new SecurityIdentifierRegistryDataType();
	}

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var validValues = new[] { "BUILTIN\\Administrators", "Everyone", string.Empty, null, };
		return validValues.Select(v => new ValidSampleAndBinaryValueInDB(v, new StringRegistryDataType().Serialise(v))).ToArray();
	}

	protected override object GetNullRepresentation()
	{
		return StringRegistryDataTypeTest.GetNullStringRepresentation();
	}
}

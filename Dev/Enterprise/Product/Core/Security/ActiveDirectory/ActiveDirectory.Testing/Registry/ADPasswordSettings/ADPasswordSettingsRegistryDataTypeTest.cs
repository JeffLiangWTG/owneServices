using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADPasswordSettingsRegistryDataType))]
	class ADPasswordSettingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ADPasswordSettingsRegistryDataType>
	{
		protected override ADPasswordSettingsRegistryDataType GetNewDataType() => new ADPasswordSettingsRegistryDataType(new ADPasswordSettingsRegistryBusinessObject());

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new[]
{
				new ValidSampleAndBinaryValueInDB(new ADPasswordSettingsRegistryBusinessObject() { Dummy = false }, System.Array.Empty<byte>()),
				new ValidSampleAndBinaryValueInDB(new ADPasswordSettingsRegistryBusinessObject() { Dummy = true }, System.Array.Empty<byte>())
			};
		}

		protected override string ExpectedEditorName => "ADPasswordSettingsRegistryEditor";
	}
}

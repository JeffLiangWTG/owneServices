using CargoWise.ResourceStrings.Cache;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ParameterizedStringDataType))]
	sealed class ParameterizedStringDataTypeTest : RegistryDataTypeTestCase<ParameterizedStringDataType>
	{
		protected override ParameterizedStringDataType GetNewDataType()
		{
			var defaultValue = ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value"));
			var registryItem = new ParameterizedStringRegistryItem("test", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, RegistryOptions.Default, defaultValue, ResString.GetMultilingualString("p", "Parameter"));
			return (ParameterizedStringDataType)registryItem.DataType;
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var stringDataType = new StringRegistryDataType();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")), stringDataType.Serialise("Test {0}")),
				new ValidSampleAndBinaryValueInDB(ResString.GetMultilingualString(CustomizableDataResourceStrings.GetCustomizableDataKey("R!test", "{0} something"), "{0} something", ResString.GetMultilingualString("y", "Value")), stringDataType.Serialise("{0} something")),
				new ValidSampleAndBinaryValueInDB(ResString.GetMultilingualString(CustomizableDataResourceStrings.GetCustomizableDataKey("R!test", "Nothing"), "Nothing", ResString.GetMultilingualString("y", "Value")), stringDataType.Serialise("Nothing")),
			};
		}
	}
}

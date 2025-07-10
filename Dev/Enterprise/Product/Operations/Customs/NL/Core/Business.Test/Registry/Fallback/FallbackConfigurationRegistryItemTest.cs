using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.FallbackConfigurationRegistryItem;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(FallbackConfigurationRegistryItem))]
sealed class FallbackConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<FallbackConfiguration>
{
	protected override ZArchitecture.Environment.StronglyTypedRegistryItem<FallbackConfiguration, FallbackConfiguration> GetNewRegistryItem()
	{
		return new FallbackConfigurationRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System | RegistryStorageFlags.Company);
	}
}

[TestedType(typeof(FallbackConfigurationRegistryDataType))]
sealed class FallbackConfigurationsRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FallbackConfigurationRegistryDataType>
{
	protected override string ExpectedEditorName => "FallbackRegistryItemEditor";

	protected override FallbackConfigurationRegistryDataType GetNewDataType()
	{
		return new FallbackConfigurationRegistryDataType();
	}

	[TestDate(2024, 12, 18, 00, 00, 00)]
	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var fallbackConfig1 = new FallbackConfiguration();
		fallbackConfig1.Start = new ZDateTime(2024, 12, 18);

		var fallbackConfig2 = new FallbackConfiguration();
		fallbackConfig2.Start = new ZDateTime(2024, 12, 19);

		return
		[
			new ValidSampleAndBinaryValueInDB(fallbackConfig1, new FallbackConfigurationRegistryDataType().Serialise(fallbackConfig1)),
			new ValidSampleAndBinaryValueInDB(fallbackConfig2, new FallbackConfigurationRegistryDataType().Serialise(fallbackConfig2))
		];
	}
}

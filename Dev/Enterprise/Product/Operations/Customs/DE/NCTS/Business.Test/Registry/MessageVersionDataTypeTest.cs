using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing;

[TestedType(typeof(NctsFallbackConfigurationRegistryDataType))]
sealed class NctsFallbackConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NctsFallbackConfigurationRegistryDataType>
{
	protected override NctsFallbackConfigurationRegistryDataType GetNewDataType() => new();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var nctsFallbackConfiguration1 = new NctsFallbackConfiguration { Start = ZDateTime.Today, CustomsIncidentNumber = "1234567890" };
		var nctsFallbackConfiguration2 = new NctsFallbackConfiguration { Start = ZDateTime.Today.AddHours(6).AddMinutes(30).AddSeconds(10), CustomsIncidentNumber = "12345678901234567890" };

		return
		[
			new(nctsFallbackConfiguration1, DataType.Serialise(nctsFallbackConfiguration1)),
			new(nctsFallbackConfiguration2, DataType.Serialise(nctsFallbackConfiguration2)),
		];
	}

	protected override string ExpectedEditorName => "NctsFallbackConfigurationRegistryItemEditor";
}

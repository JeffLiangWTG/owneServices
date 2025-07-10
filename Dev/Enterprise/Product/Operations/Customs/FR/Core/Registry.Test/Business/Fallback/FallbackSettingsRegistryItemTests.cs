using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Customs.FR.Registry.FallbackSettingsRegistryItem;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(FallbackSettingsRegistryItem))]
	class FallbackSettingsRegistryItemTests : StronglyTypedRegistryItemTestCase<FallbackSettings>
	{
		protected override ZArchitecture.Environment.StronglyTypedRegistryItem<FallbackSettings, FallbackSettings> GetNewRegistryItem()
		{
			return new FallbackSettingsRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(FallbackSettingsRegistryDataType))]
	class FallbackSettingsRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FallbackSettingsRegistryDataType>
	{
		protected override string ExpectedEditorName => "FallbackRegistryItemEditor";

		protected override FallbackSettingsRegistryDataType GetNewDataType()
		{
			return new FallbackSettingsRegistryDataType();
		}

		[TestDate(2020, 03, 8, 17, 0, 0)]
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var fallbackSetting1 = new FallbackSettings();
			fallbackSetting1.Start = new ZDateTime(2020, 03, 17);

			var fallbackSetting2 = new FallbackSettings();
			fallbackSetting2.Start = fallbackSetting1.Start.AddDays(1);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(fallbackSetting1, new FallbackSettingsRegistryDataType().Serialise(fallbackSetting1)),
				new ValidSampleAndBinaryValueInDB(fallbackSetting2, new FallbackSettingsRegistryDataType().Serialise(fallbackSetting2))
			};
		}
	}
}

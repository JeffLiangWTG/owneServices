using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing.Business
{
	[TestedType(typeof(CDSDUCRAutomationRegistryItem))]
	public class CDSDUCRAutomationSettingsRegistryItemTest :
		StronglyTypedRegistryItemTestCase<CDSDUCRAutomationSettings>
	{
		protected override ZArchitecture.Environment.StronglyTypedRegistryItem<CDSDUCRAutomationSettings,
			CDSDUCRAutomationSettings> GetNewRegistryItem()
		{
			return new CDSDUCRAutomationRegistryItem("CDSDUCRAutomation", null, null, null,
				Integration.RegistryStorageFlags.System, Integration.RegistryOptions.Default,
				new CDSDUCRAutomationSettings());
		}
	}

	[TestedType(typeof(CDSDUCRAutomationRegistryDataType))]
	class CDSDUCRAutomationSettingsRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CDSDUCRAutomationRegistryDataType>
	{
		protected override string ExpectedEditorName => "CDSUCRAutomationRegistryItemEditor";

		protected override CDSDUCRAutomationRegistryDataType GetNewDataType()
		{
			return new CDSDUCRAutomationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var settings1 = new CDSDUCRAutomationSettings
			{
				CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
			};
			var settings2 = new CDSDUCRAutomationSettings
			{
				CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference
			};
			var settings3 = new CDSDUCRAutomationSettings
			{
				CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports
			};
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(settings1, new CDSDUCRAutomationRegistryDataType().Serialise(settings1)),
				new ValidSampleAndBinaryValueInDB(settings2, new CDSDUCRAutomationRegistryDataType().Serialise(settings2)),
				new ValidSampleAndBinaryValueInDB(settings2, new CDSDUCRAutomationRegistryDataType().Serialise(settings2))
			};
		}
	}
}

using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Registry.Testing.Business
{
	[TestedType(typeof(CDSDUCRAutomationSettings))]
	public class CDSDUCRautomationSettingsTest : RegistryBusinessObjectTemplateTestCase<CDSDUCRAutomationSettings>
	{
		protected override bool RequiresFactory { get { return true; } }

		protected override bool RequiresFallbackLevel { get { return true; } }

		protected override CDSDUCRAutomationSettings GetBusinessObjectToClone()
		{
			return new CDSDUCRAutomationSettings(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
		}

		protected override CDSDUCRAutomationSettings GetBusinessObjectToSerialise()
		{
			return new CDSDUCRAutomationSettings(Factory);
		}

		public void TestCDSDUCRAutomationSelectedSettings()
		{
			var settings = new CDSDUCRAutomationSettings();
			IRegistryDataType dummyDataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CDSDUCRAutomationSettings));

			settings.CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports;
			ZBlob serialisedValue = dummyDataType.Serialise(settings);
			CDSDUCRAutomationSettings deserialisedBusinessObject = (CDSDUCRAutomationSettings)dummyDataType.Deserialise(serialisedValue);
			AssertEquals(settings.CDSDUCRAutomation, deserialisedBusinessObject.CDSDUCRAutomation);

			settings.CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference;
			serialisedValue = dummyDataType.Serialise(settings);
			deserialisedBusinessObject = (CDSDUCRAutomationSettings)dummyDataType.Deserialise(serialisedValue);
			AssertEquals(settings.CDSDUCRAutomation, deserialisedBusinessObject.CDSDUCRAutomation);

			settings.CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields;
			serialisedValue = dummyDataType.Serialise(settings);
			deserialisedBusinessObject = (CDSDUCRAutomationSettings)dummyDataType.Deserialise(serialisedValue);
			AssertEquals(settings.CDSDUCRAutomation, deserialisedBusinessObject.CDSDUCRAutomation);
		}

		public void TestValidateCDSDUCRAutomationSettings()
		{
			var settings = new CDSDUCRAutomationSettings();
			settings.CDSDUCRAutomation = string.Empty;
			AssertHasErrors(settings.CDSDUCRAutomationInfo);

			settings.CDSDUCRAutomation = "DUMMY";
			AssertHasErrors(settings.CDSDUCRAutomationInfo);

			settings.CDSDUCRAutomationInfo.ClearAllNotifications();
			settings.CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports;
			AssertNoMessageErrors(settings.CDSDUCRAutomationInfo);
		}
	}
}

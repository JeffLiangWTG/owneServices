using System.IO;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADConfig))]
	class ADConfigNonPersistentObjectTestCase : NonPersistentBusinessObjectTestCase
	{
	}

	[TestedType(typeof(ADConfig))]
	class ADConfigRegistryBusinessObjectTemplateTest : RegistryBusinessObjectTemplateTestCase<ADConfig>
	{
		protected override ADConfig GetBusinessObjectToClone() => ADConfig.DefaultValue;

		protected override ADConfig GetBusinessObjectToSerialise() => ADConfig.DefaultValue;

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;
	}

	class ADConfigTest : TestCaseWithFactoryAndMocks
	{
		public void TestReadFromXml()
		{
			var config = new ADConfig();
			var textReader = new StringReader(xml);
			var xmlReader = XmlTextReader.Create(textReader);
			config.Read(xmlReader);

			AssertEquals(true, config.IsADIntegrationEnabled);
			AssertEquals(EntitiesToSync.UsersAndGroups, config.EntitiesToSync);
			AssertEquals(true, config.IsSingleSignOn);
			AssertEquals(SyncMode.ADIsMaster, config.SyncMode);
			AssertEquals(SyncDirection.TwoWay, config.SyncDirection);
			AssertEquals(SyncDirection.TwoWay, config.SyncDirectionGroup);
		}

		public void TestReadFromXml_WithSyncMode()
		{
			var config = new ADConfig();
			var textReader = new StringReader(xmlWithSyncMode);
			var xmlReader = XmlTextReader.Create(textReader);
			config.Read(xmlReader);

			AssertEquals(true, config.IsADIntegrationEnabled);
			AssertEquals(EntitiesToSync.UsersOnly, config.EntitiesToSync);
			AssertEquals(false, config.IsSingleSignOn);
			AssertEquals(SyncMode.EnterpriseIsMaster, config.SyncMode);
			AssertEquals(SyncDirection.TwoWay, config.SyncDirection);
			AssertEquals(SyncDirection.TwoWay, config.SyncDirectionGroup);
		}

		public void TestReadFromXml_WithSyncDirection()
		{
			var config = new ADConfig();
			var textReader = new StringReader(xmlWithSyncDirection);
			var xmlReader = XmlTextReader.Create(textReader);
			config.Read(xmlReader);

			AssertEquals(true, config.IsADIntegrationEnabled);
			AssertEquals(EntitiesToSync.UsersAndGroups, config.EntitiesToSync);
			AssertEquals(true, config.IsSingleSignOn);
			AssertEquals(SyncMode.ADIsMaster, config.SyncMode);
			AssertEquals(SyncDirection.OneWay, config.SyncDirection);
			AssertEquals(SyncDirection.OneWay, config.SyncDirectionGroup);
		}

		public void TestReadFromXml_WithGroupSyncModeAndDirection()
		{
			var config = new ADConfig();
			var textReader = new StringReader(xmlWithGroupSyncDirection);
			var xmlReader = XmlTextReader.Create(textReader);
			config.Read(xmlReader);

			AssertEquals(true, config.IsADIntegrationEnabled);
			AssertEquals(EntitiesToSync.UsersAndGroups, config.EntitiesToSync);
			AssertEquals(true, config.IsSingleSignOn);
			AssertEquals(SyncMode.ADIsMaster, config.SyncMode);
			AssertEquals(SyncDirection.OneWay, config.SyncDirection);
			AssertEquals(SyncDirection.TwoWay, config.SyncDirectionGroup);
		}

		const string xml =
@"<?xml version=""1.0"" ?>
<ADConfig>
	<IsADIntegrationEnabled>Y</IsADIntegrationEnabled>
	<EntitiesToSyncCode>ALL</EntitiesToSyncCode>
	<IsSingleSignOn>Y</IsSingleSignOn>
</ADConfig>";

		const string xmlWithSyncMode =
@"<?xml version=""1.0"" ?>
<ADConfig>
	<IsADIntegrationEnabled>Y</IsADIntegrationEnabled>
	<EntitiesToSyncCode>USR</EntitiesToSyncCode>
	<SyncModeCode>CW1</SyncModeCode>
	<IsSingleSignOn>N</IsSingleSignOn>
</ADConfig>";

		const string xmlWithSyncDirection =
@"<?xml version=""1.0"" ?>
<ADConfig>
	<IsADIntegrationEnabled>Y</IsADIntegrationEnabled>
	<EntitiesToSyncCode>ALL</EntitiesToSyncCode>
	<SyncModeCode>AD</SyncModeCode>
	<IsSingleSignOn>Y</IsSingleSignOn>
	<SyncDirectionCode>1WAY</SyncDirectionCode>
</ADConfig>";

		const string xmlWithGroupSyncDirection =
			@"<?xml version=""1.0"" ?>
<ADConfig>
	<IsADIntegrationEnabled>Y</IsADIntegrationEnabled>
	<EntitiesToSyncCode>ALL</EntitiesToSyncCode>
	<SyncModeCode>AD</SyncModeCode>
	<IsSingleSignOn>Y</IsSingleSignOn>
	<SyncDirectionCode>1WAY</SyncDirectionCode>
	<SyncDirectionGroupCode>2WAY</SyncDirectionGroupCode>
</ADConfig>";

		public void TestPropertiesShouldBeReadOnlyWhenADDisabled()
		{
			var config = new ADConfig();
			SetADEnabledAndAssertInfosReadonly(config, false, true);
			SetADEnabledAndAssertInfosReadonly(config, true, false);
		}

		void SetADEnabledAndAssertInfosReadonly(ADConfig config, bool adEnabled, bool infosReadonly)
		{
			config.IsADIntegrationEnabled = adEnabled;
			Assert("AD Integration should always be writable", !config.IsADIntegrationEnabledInfo.ReadOnly);

			AssertEquals(infosReadonly, config.EntitiesToSyncCodeInfo.ReadOnly);
			AssertEquals(infosReadonly, config.IsSingleSignOnInfo.ReadOnly);
			AssertEquals(infosReadonly, config.SyncModeCodeInfo.ReadOnly);
			AssertEquals(infosReadonly, config.SyncDirectionCodeInfo.ReadOnly);
			AssertEquals(infosReadonly, config.SyncDirectionGroupCodeInfo.ReadOnly);
		}

		public void TestSetADEnabledDisablesSingleSignOn()
		{
			var config = new ADConfig();

			config.IsADIntegrationEnabled = true;
			config.IsSingleSignOn = true;

			config.IsADIntegrationEnabled = false;
			AssertEquals("IsSingleSignOn should be false when IsADIIntegrationEnabled is set to false", false, config.IsSingleSignOn);

			config.IsADIntegrationEnabled = true;
			AssertEquals("IsSingleSignOn should remain false when IsADIIntegrationEnabled re-enabled", false, config.IsSingleSignOn);
		}

		public void TestEquals()
		{
			AssertEquals(ADConfig.DefaultValue, ADConfig.DefaultValue);
			AssertEquals(
				new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersOnly, IsSingleSignOn = false, SyncMode = SyncMode.EnterpriseIsMaster, SyncDirection = SyncDirection.OneWay, SyncDirectionGroup = SyncDirection.TwoWay },
				new ADConfig { IsADIntegrationEnabled = true, EntitiesToSync = EntitiesToSync.UsersOnly, IsSingleSignOn = false, SyncMode = SyncMode.EnterpriseIsMaster, SyncDirection = SyncDirection.OneWay, SyncDirectionGroup = SyncDirection.TwoWay });
		}

		public void TestSetEntitiesToSyncCode()
		{
			var adConfig = new ADConfig();

			adConfig.EntitiesToSyncCode = "ALL";
			AssertEquals(EntitiesToSync.UsersAndGroups, adConfig.EntitiesToSync);
			Assert(!adConfig.EntitiesToSyncCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.EntitiesToSyncCode = "USR";
			AssertEquals(EntitiesToSync.UsersOnly, adConfig.EntitiesToSync);
			Assert(!adConfig.EntitiesToSyncCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.EntitiesToSyncCode = "XYZ";
			Assert(adConfig.EntitiesToSyncCodeInfo.HasErrors());
			Assert(adConfig.EntitiesToSyncCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);
			AssertEquals(EntitiesToSync.UsersAndGroups, adConfig.EntitiesToSync); // default value

			//can recover
			adConfig.EntitiesToSyncCode = "USR";
			AssertEquals(EntitiesToSync.UsersOnly, adConfig.EntitiesToSync);
			Assert(!adConfig.EntitiesToSyncCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);
		}

		public void TestSetSyncModeCode()
		{
			var adConfig = new ADConfig();

			adConfig.SyncModeCode = "AD";
			AssertEquals(SyncMode.ADIsMaster, adConfig.SyncMode);
			Assert(!adConfig.SyncModeCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncModeCode = "CW1";
			AssertEquals(SyncMode.EnterpriseIsMaster, adConfig.SyncMode);
			Assert(!adConfig.SyncModeCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncModeCode = "XXX";
			Assert(adConfig.SyncModeCodeInfo.HasErrors());
			Assert(adConfig.SyncModeCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);
			AssertEquals(SyncMode.ADIsMaster, adConfig.SyncMode);

			//can recover
			adConfig.SyncModeCode = "CW1";
			AssertEquals(SyncMode.EnterpriseIsMaster, adConfig.SyncMode);
			Assert(!adConfig.SyncModeCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncModeCode = string.Empty;
			Assert(adConfig.SyncModeCodeInfo.HasErrors());
			Assert(adConfig.SyncModeCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);
			AssertEquals(SyncMode.ADIsMaster, adConfig.SyncMode);
		}
		public void TestSetSyncDirectionCode()
		{
			var adConfig = new ADConfig();

			adConfig.SyncDirectionCode = "2WAY";
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirection);
			Assert(!adConfig.SyncDirectionCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionCode = "1WAY";
			AssertEquals(SyncDirection.OneWay, adConfig.SyncDirection);
			Assert(!adConfig.SyncDirectionCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionCode = string.Empty;
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirection);
			Assert(adConfig.SyncDirectionCodeInfo.HasErrors());
			Assert(adConfig.SyncDirectionCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);

			//can recover
			adConfig.SyncDirectionCode = "1WAY";
			AssertEquals(SyncDirection.OneWay, adConfig.SyncDirection);
			Assert(!adConfig.SyncDirectionCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionCode = "XXX";
			Assert(adConfig.SyncDirectionCodeInfo.HasErrors());
			Assert(adConfig.SyncDirectionCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirection);
		}

		public void TestSetSyncDirectionGroupCode()
		{
			var adConfig = new ADConfig();

			adConfig.SyncDirectionGroupCode = "2WAY";
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirectionGroup);
			Assert(!adConfig.SyncDirectionGroupCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionGroupCode = "1WAY";
			AssertEquals(SyncDirection.OneWay, adConfig.SyncDirectionGroup);
			Assert(!adConfig.SyncDirectionGroupCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionGroupCode = string.Empty;
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirectionGroup);
			Assert(adConfig.SyncDirectionGroupCodeInfo.HasErrors());
			Assert(adConfig.SyncDirectionGroupCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);

			//can recover
			adConfig.SyncDirectionGroupCode = "1WAY";
			AssertEquals(SyncDirection.OneWay, adConfig.SyncDirectionGroup);
			Assert(!adConfig.SyncDirectionGroupCodeInfo.HasErrors());
			Assert(!adConfig.HasErrors);

			adConfig.SyncDirectionGroupCode = "XXX";
			Assert(adConfig.SyncDirectionGroupCodeInfo.HasErrors());
			Assert(adConfig.SyncDirectionGroupCodeInfo.HasError("Invalid Selection."));
			Assert(adConfig.HasErrors);
			AssertEquals(SyncDirection.TwoWay, adConfig.SyncDirectionGroup);
		}

		public void TestGetSyncModeFromCode()
		{
			AssertEquals(2, new SyncModeList().Count);
			AssertEquals(SyncMode.ADIsMaster, ADConfig.GetSyncModeFromCode(SyncModeList.Codes.ActiveDirectory));
			AssertEquals(SyncMode.EnterpriseIsMaster, ADConfig.GetSyncModeFromCode(SyncModeList.Codes.CargoWise));
			AssertEquals(null, ADConfig.GetSyncModeFromCode("RandomCode"));
		}
	}
}

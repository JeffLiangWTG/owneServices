using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using SharedGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowRegistry))]
	sealed class GlowRegistryTest : RegistryItemSetTestCaseWithFactory<GlowRegistry>
	{
		public void TestEnableAdvancedDataAutomationWizard()
		{
			TestRegistryItem(
				ItemSet.EnableAdvancedDataAutomationWizard,
				"EnableAdvancedDataAutomationWizard",
				GlowRegistry.Categories.GLOW,
				"Advanced Data Automation Wizard Enabled",
				@"Change to 'Yes' to enable the Advanced Data Automation Wizard on supported modules and entities ONLY.

Enabling this functionality requires WiseTech Global to set up relevant services.If these services have not been set up, this registry item will automatically be disabled when you next login.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				true);
		}

		public void TestEnableGridWideProposedValues()
		{
			TestRegistryItem(
				ItemSet.EnableGridWideProposedValues,
				"GlowEnableGridWideProposedValues",
				GlowRegistry.Categories.GLOW,
				"Grid-wide Proposed Values Enabled",
				@"Change to 'No' to disable grid-wide proposed values in GLOW. When disabled, data grids will only have proposed values enabled on the row currently being edited.

The web portal's application pool will need to be restarted for changes to take effect.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestEnableNewSignalRClient()
		{
			TestRegistryItem(
				ItemSet.EnableNewSignalRClient,
				"GlowEnableNewSignalRClient",
				GlowRegistry.Categories.GLOW,
				"Enable New ASP.NET Core SignalR Client",
				"Change to 'Yes' to enable the new ASP.NET Core SignalR client.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestSuppressTrailingZeroes()
		{
			TestRegistryItem(
				ItemSet.SuppressTrailingZeroes,
				"GlowSuppressTrailingZeroes",
				GlowRegistry.Categories.GLOW,
				"Suppress Trailing Zeros",
				"Change to ‘Yes’ to suppress trailing zeros.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestEnableServiceWorker()
		{
			TestRegistryItem(
				ItemSet.EnableServiceWorker,
				"GlowEnableServiceWorker",
				GlowRegistry.Categories.GLOW,
				"Service Worker Enabled",
				"Change to 'No' to disable the GLOW web portal service worker. The web portal's application pool will need to be restarted for changes to take effect.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestGlowServiceUri()
		{
			TestRegistryItem(
				ItemSet.GlowServiceUriRegistryItem,
				"GlowServiceUri",
				GlowRegistry.Categories.GLOW_Services,
				"GLOW Service URL",
				"The URL to the GLOW service",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty,
				"https://localhost/");

			ItemSet.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://foo.com/bar");
			AssertEquals("https://foo.com/bar/", ItemSet.GlowServiceUri);
			ItemSet.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://foo.com/bar/");
			AssertEquals("https://foo.com/bar/", ItemSet.GlowServiceUri);
			AssertEquals("https://foo.com/bar/", ObjectFactory.Get<IGlowRegistry>().GlowServiceUri);
		}

		public void TestGlowServiceExternalUri()
		{
			AssertEquals("/", ItemSet.GlowServiceExternalUri);

			TestRegistryItem(
				ItemSet.GlowServiceExternalUriRegistryItem,
				"GlowServiceExternalUri",
				GlowRegistry.Categories.GLOW_Services,
				"External User GLOW Service URL",
				"The URL to the GLOW service in installations where external contact users have a different Service URL to internal staff users.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty,
				"https://localhost/");

			ItemSet.GlowServiceExternalUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://foo.com/bar");
			AssertEquals("https://foo.com/bar/", ItemSet.GlowServiceExternalUri);
			ItemSet.GlowServiceExternalUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://foo.com/bar/");
			AssertEquals("https://foo.com/bar/", ItemSet.GlowServiceExternalUri);
		}

		public void TestEnableUnpublishedModulesTest_True() => EnableUnpublishedModulesTest(true);

		public void TestEnableUnpublishedModulesTest_False() => EnableUnpublishedModulesTest(false);

		void EnableUnpublishedModulesTest(bool value)
		{
			var productRegistrationMock = new Mock<IProductRegistration>();
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				TestRegistryItem(
					ItemSet.EnableUnpublishedModules,
					"GlowEnableUnpublishedModules",
					GlowRegistry.Categories.GLOW,
					"Enable Unpublished Modules",
					"Allows access to modules which have not been published.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					false);
			}
			ItemSet.EnableUnpublishedModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			AssertEquals(value, ItemSet.EnableUnpublishedModules.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		const string GlowRestrictedModulesOverrideRegistryName = "GlowRestrictedModulesOverride";

		public void TestGetRestrictedModulesOverride_NoRegistryValue()
		{
			GlowRegistryTestHelper.SetRestrictedModulesOverride();

			AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), GlowRegistry.GetRestrictedPortalsOverride());
		}

		public void TestGetRestrictedModulesOverride_ValueIsSet()
		{
			GlowRegistryTestHelper.SetRestrictedModulesOverride("HRM");

			AssertContainsExactElementsInAnyOrder(new string[] { "HRM" }, GlowRegistry.GetRestrictedPortalsOverride());
		}

		public void TestGetRestrictedModulesOverride_ValueIsCorrupt()
		{
			// Taken directly from my local db and then ruined
			const string SerialisedValue = "x";

			Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name=@name", p => p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name));

			// GLOW doesn't use compression here, we need UTF-8 stored as binary blob
			Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmData (SD_Name, SD_BinaryValue, SD_PK) VALUES (@name, CONVERT(varbinary(max), @value), NEWID())", p =>
			{
				p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name);
				p.AddParameter("@value", System.Data.SqlDbType.VarChar, SerialisedValue);
			});

			string[] results = null;
			AssertNoExceptionThrown(() => results = GlowRegistry.GetRestrictedPortalsOverride());
			AssertContainsExactElementsInAnyOrder(Array.Empty<string[]>(), results);
			AssertEquals("GlowRestrictedModulesOverride failed to be read by CW1", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetRestrictedModulesOverride_ValueIsCorrupt_WithValidXml()
		{
			// Taken directly from my local db and then ruined
			const string SerialisedValue = "<xml><NotTheRightThing>Hello</NotTheRightThing></xml>";

			Db.Connection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name=@name", p => p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name));

			// GLOW doesn't use compression here, we need UTF-8 stored as binary blob
			Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmData (SD_Name, SD_BinaryValue, SD_PK) VALUES (@name, CONVERT(varbinary(max), @value), NEWID())", p =>
			{
				p.AddParameterBasedOnDbColumn("@name", GlowRestrictedModulesOverrideRegistryName, StmDataSchema.SD_Name);
				p.AddParameter("@value", System.Data.SqlDbType.VarChar, SerialisedValue);
			});

			string[] results = null;
			AssertNoExceptionThrown(() => results = GlowRegistry.GetRestrictedPortalsOverride());
			AssertContainsExactElementsInAnyOrder(Array.Empty<string[]>(), results);
			AssertEquals("GlowRestrictedModulesOverride failed to be read by CW1", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGlowPortalsRootUri()
		{
			TestRegistryItem(
				ItemSet.GlowPortalsUri,
				"GlowPortalsUri",
				GlowRegistry.Categories.GLOW_Services,
				"GLOW Portals Root URL",
				"The URL to GLOW Web Portals",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty,
				"https://localhost/");
		}

		public void TestGlowDefaultWebPortal()
		{
			TestRegistryItem(
				ItemSet.GlowDefaultWebPortal,
				"GlowDefaultWebPortal",
				GlowRegistry.Categories.GLOW_Services,
				"Default Web Portal",
				"The default web portal to use when no portal is specified in URL.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				expectedDefaultValue: string.Empty,
				testValueToSetAndRead: "NEO");

			var dataType = ItemSet.GlowDefaultWebPortal.DataType as StringRegistryDataType;
			AssertEquals(CharacterCase.Upper, dataType.CharacterCase);
			AssertEquals(3, dataType.MinLength);
			AssertEquals(3, dataType.MaxLength);
		}

		public void TestGlowPushServiceUrl()
		{
			TestRegistryItem(
				ItemSet.GlowPushServiceUrl,
				"GlowPushServiceUrl",
				GlowRegistry.Categories.GLOW_Services,
				"Push Service URL",
				String.Format("The URL to the service used for Push Notifications. This registry item is currently not used by {0}.", Core.Constants.ProductName),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				TextEditorType.TextBox,
				expectedDefaultValue: "http://localhost/Push/",
				testValueToSetAndRead: "https://localhost/");
		}
		public void TestEdiProdOAuthRootUriKey()
		{
			TestRegistryItem(
				ItemSet.EdiProdOAuthRootUriKey,
				"EdiProdOAuthRootUri",
				GlowRegistry.Categories.GLOW_Services,
				"ediProd OAuth Root URL",
				"The base URI for OAuth services provided by ediProd",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
				TextEditorType.TextBox,
				expectedDefaultValue: "https://myaccount-portal.cargowise.com/myaccount/oauth",
				testValueToSetAndRead: "https://localhost/oauth");
		}

		public void TestGlowAuthenticationEncryptionKey()
		{
			TestGenericRegistryItem(
				ItemSet.GlowAuthenticationEncryptionKey,
				"GlowAuthEncryptionKey",
				GlowRegistry.Categories.GLOW,
				"Authentication Encryption Key",
				"Key used for encryption of the GLOW authentication cookie. The key should be 32 bytes long.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestGlowAuthenticationHmacKey()
		{
			TestGenericRegistryItem(
				ItemSet.GlowAuthenticationHmacKey,
				"GlowAuthHmacKey",
				GlowRegistry.Categories.GLOW,
				"Authentication HMAC Key",
				"Key used for integrity checks of the GLOW authentication cookie. The key should be 64 bytes long.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport);
		}

		public void TestGlowAuthenticationHmacKey_EditorInfo()
		{
			var registryItem = GlowRegistry.Instance.GlowAuthenticationHmacKey;
			AssertNull(registryItem.EditorInfo);
		}

		public void TestGlowAuthenticationHmacKey_DataType()
		{
			var registryItem = GlowRegistry.Instance.GlowAuthenticationHmacKey;
			AssertType(typeof(BinaryKeyRegistryDataType), registryItem.DataType);

			var dataType = (BinaryKeyRegistryDataType)registryItem.DataType;
			AssertEquals(64, dataType.KeySize); // 64-bit key for SHA256 HMAC
		}

		public void TestGlowAuthenticationHmacKey_DataType_CorrectKeySizeInDescription()
		{
			var registryItem = GlowRegistry.Instance.GlowAuthenticationHmacKey;
			var dataType = (BinaryKeyRegistryDataType)registryItem.DataType;
			var keySize = dataType.KeySize;

			var bytesDescription = string.Format("{0} bytes", keySize);
			Assert(registryItem.Hint.IndexOf(bytesDescription, StringComparison.InvariantCultureIgnoreCase) >= 0);
		}

		public void TestPreserveGlowTheme()
		{
			TestGenericRegistryItem(
				ItemSet.PreserveGlowTheme,
				"PreserveGlowTheme",
				GlowRegistry.Categories.GLOW,
				"Preserve Glow Theme",
				@"If current database is the test database, change to 'Yes' to preserve current GLOW theme in 'Copy Production To Test'. Change to 'No' to use the production database theme.

If current database is the production database, this registry has no effect.

NOTE: 'Copy Production To Test' is a function in Database Backup And Restore tool. It is used to create a test database which can be used like a sandbox environment for testing features or training.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestGlowEnableDevelopmentModeKey()
		{
			var item = ItemSet.GlowEnableDevelopmentModeKey;
			AssertEquals("Name", "GlowEnableDevelopmentMode", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "Enable Development Mode", item.Caption);
			AssertEquals("Hint", "Change to ‘Yes’ to enable development mode on the platform builder. Any change, from ‘Yes’ or ‘No’, will require the web server to restart.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestGlowIndexCdcFilterByUpdateMask()
		{
			var item = ItemSet.GlowIndexCdcFilterByUpdateMask;
			AssertEquals("Name", "GlowIndexCdcFilterByUpdateMask", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer CDC Filter By Update Mask", item.Caption);
			AssertEquals("Hint", "GLOW Indexer gets changed data by filtering update mask when SQL Change Data Capture is in use.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", true, item.DefaultValue);
		}

		public void TestGlowNumberOfAdditionalIndexers()
		{
			var item = ItemSet.GlowNumberOfAdditionalIndexers;
			AssertEquals("Name", "GlowNumberOfAdditionalIndexers", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Parallel Indexers", item.Caption);
			AssertEquals("Hint", "Number of additional Indexers that perform index rebuilds.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, item.Options);
			AssertEquals("Default", 1, item.DefaultValue);
		}

		public void TestGlowIndexParameterizeQueryConstants()
		{
			var item = ItemSet.GlowIndexParameterizeQueryConstants;
			AssertEquals("Name", "GlowIndexParameterizeQueryConstants", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Parameterize Query Constants", item.Caption);
			AssertEquals("Hint", "GLOW Indexer parameterizes query constants.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestGlowIndexSyncUseHttp()
		{
			var item = ItemSet.GlowUseHttpInterCluster;
			AssertEquals("Name", "GlowUseHttpInterCluster", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW inter-node communication uses HTTP", item.Caption);
			AssertEquals("Hint", "GLOW inter-node communication uses HTTP.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestGlowIndexSearchUsageCollector()
		{
			TestRegistryItem(
				ItemSet.GlowIndexSearchUsageCollector,
				"GlowIndexSearchUsageCollector",
				GlowRegistry.Categories.GLOW_Services,
				"CargoWise Index Search Usage Collector",
				"CargoWise Index Search Usage Collector.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				false);
		}

		public void TestGlowIndexUsePersistentConnection()
		{
			var item = ItemSet.GlowIndexUsePersistentConnection;
			AssertEquals("Name", "GlowIndexUsePersistentConnection", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Use Persistent Connection", item.Caption);
			AssertEquals("Hint", "GLOW Indexer uses a persistent database connection.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", true, item.DefaultValue);
		}

		public void TestGlowConfigurationTmplEnabled()
		{
			var item = ItemSet.GlowConfigurationTmplEnabled;
			AssertEquals("Name", "GlowConfigurationTmplEnabled", item.Name);
			AssertEquals("Category", "GLOW", item.Category);
			AssertEquals("Caption", "Configuration Templates Enabled", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestForceImportable()
		{
			var item = ItemSet.ForceImportable;
			AssertEquals("Name", "ForceImportable", item.Name);
			AssertEquals("Category", "GLOW", item.Category);
			AssertEquals("Caption", "Force Importable Enabled", item.Caption);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestGlowMaximumNumberOfTrackingResults()
		{
			TestRegistryItem(
				ItemSet.GlowMaximumNumberOfTrackingResults,
				"GlowMaximumNumberOfTrackingResults",
				GlowRegistry.Categories.GLOW_Services,
				"Maximum Number of Tracking Results",
				"The maximum number of tracking results returned by the Tracking Service.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				10);

			ItemSet.GlowMaximumNumberOfTrackingResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);
			AssertEquals("GlowMaximumNumberOfTrackingResults", 25, ItemSet.GlowMaximumNumberOfTrackingResults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestGlowExclusiveRegistryItems_AreUnderneathTopLevelItemNamedGlow()
		{
			CombineAssertions(() =>
			{
				foreach (var item in GetGlowExclusiveRegistryItems())
				{
					Assert(
						string.Format("Item [{0}] is Glow-exclusive and should be in the GLOW category or sub-categories", item.Name),
						item.Category == GlowRegistry.Categories.GLOW || item.Category.StartsWith(GlowRegistry.Categories.GLOW + "/"));
				}
			});
		}

		public void TestGlowExclusiveRegistryItems_AreForSupportOnly_Except_Exclusions()
		{
			var exclusions = new List<string>()
			{
				"PreserveGlowTheme",
				"EnableAdvancedDataAutomationWizard",
				"GlowSuppressTrailingZeroes",
				"GlowNeoEnableHyperlinksInDocumentMacros",
				SharedGlowRegistry.NeoEnableConversations,
				nameof(GlowRegistry.EndUserPolicyHyperlinks),
			};

			CombineAssertions(() =>
			{
				foreach (var item in GetGlowExclusiveRegistryItems().Where(x => !exclusions.Contains(x.Name)))
				{
					Assert(
						string.Format("Item [{0}] is GLOW-exclusive and should be set to IsOnlyEditableBySupportIfHosted or IsOnlyForSupport", item.Name),
						item.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted) || item.HasOption(RegistryOptions.IsOnlyForSupport));
				}
			});
		}

		public void TestAzureSignalRConnectionString()
		{
			TestRegistryItem(
				ItemSet.AzureSignalRConnectionString,
				"AzureSignalRConnectionString",
				GlowRegistry.Categories.GLOW_Services,
				"Azure SignalR Connection String",
				"Set the Azure SignalR connection string to enable live messages.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise,
				TextEditorType.TextBox,
				expectedDefaultValue: "",
				testValueToSetAndRead: "Endpoint=https://test.com;AccessKey=AAA;");
		}

		public void TestGlowIndexerStrategyPreference()
		{
			var item = ItemSet.GlowIndexerStrategyPreference;
			AssertEquals("Name", "GlowIndexerStrategyPreference", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Strategy Preference", item.Caption);
			AssertEquals("Hint", "GLOW Indexer will attempt to use the specified strategy where available. The possible options are: SQL Change Tracking, SQL Change Data Capture or BI Audit.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", "CDC", item.DefaultValue);
		}

		public void TestGlowIndexerStorageType_CWHosted()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.HostedLocationForTest = "SYD";
			registration.KeyForTest.EnterpriseCodeForTest = "HAO";

			AssertGlowIndexerStorageType(new[] { "Lucene", "LuceneClustered", "Elasticsearch" }, "LuceneClustered");
		}

		public void TestGlowIndexerStorageType_SelfHosted()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.HostedLocationForTest = LicenceConstants.NotHostedWithCargoWise;
			registration.KeyForTest.EnterpriseCodeForTest = "HAO";

			AssertGlowIndexerStorageType(new[] { "Lucene", "LuceneClustered" }, "Lucene");
		}

		public void TestGlowIndexerStorageType_Internal()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.HostedLocationForTest = LicenceConstants.NotHostedWithCargoWise;
			registration.KeyForTest.EnterpriseCodeForTest = WiseTechGlobalInternalSystemCodes.WTL;

			AssertGlowIndexerStorageType(new[] { "Lucene", "LuceneClustered", "Elasticsearch" }, "Lucene");
		}

		void AssertGlowIndexerStorageType(string[] expectedLookItems, string expectedDefaultValue)
		{
			var item = ItemSet.GlowIndexerStorageType;
			AssertEquals("Name", "GlowIndexerStorageType", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Storage Type", item.Caption);
			AssertEquals("Hint", "Set the storage type used by GLOW Indexer.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, item.Options);
			AssertEquals("Default", expectedDefaultValue, item.DefaultValue);
			AssertType("DataType", typeof(CodePairRegistryDataType), item.DataType);
			AssertArrayEqualsByElements("Storage Types", expectedLookItems, ((CodePairRegistryDataType)item.DataType).LookUpList.GetAllCodes());
		}

		public void TestGlowIndexerElasticServerUris()
		{
			var item = ItemSet.GlowIndexerElasticServerUris;
			AssertEquals("Name", "GlowIndexerElasticServerUris", item.Name);
			AssertEquals("Category", "GLOW/Services/Elasticsearch", item.Category);
			AssertEquals("Caption", "Glow Indexer Elasticsearch Server URIs", item.Caption);
			AssertEquals("Hint", "The URI list of Elasticsearch nodes.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", string.Empty, item.DefaultValue);
			AssertType("DataType", typeof(StringRegistryDataType), item.DataType);
		}

		public void TestGlowIndexerElasticUsername()
		{
			var item = ItemSet.GlowIndexerElasticUsername;
			AssertEquals("Name", "GlowIndexerElasticUsername", item.Name);
			AssertEquals("Category", "GLOW/Services/Elasticsearch", item.Category);
			AssertEquals("Caption", "Glow Indexer Elasticsearch Username", item.Caption);
			AssertEquals("Hint", "Set the user name GLOW Indexer uses to access Elasticsearch.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", string.Empty, item.DefaultValue);
			AssertType("DataType", typeof(StringRegistryDataType), item.DataType);
		}

		public void TestGlowIndexerElasticPassword()
		{
			var item = ItemSet.GlowIndexerElasticPassword;
			AssertEquals("Name", "GlowIndexerElasticPassword", item.Name);
			AssertEquals("Category", "GLOW/Services/Elasticsearch", item.Category);
			AssertEquals("Caption", "Glow Indexer Elasticsearch Password", item.Caption);
			AssertEquals("Hint", "Set the password GLOW Indexer uses to access Elasticsearch.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", string.Empty, item.DefaultValue);
			AssertType("DataType", typeof(StringRegistryDataType), item.DataType);
		}

		public void TestGlowIndexerSplitQueries()
		{
			var item = ItemSet.GlowIndexerSplitQueries;
			AssertEquals("Name", "GlowIndexerSplitQueries", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Should Split Queries", item.Caption);
			AssertEquals("Hint", "GLOW Indexer should split queries when the current service is a 64-bit process.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", true, item.DefaultValue);
		}

		public void TestGlowShowUnpublishedControlTowerWidgets()
		{
			var item = ItemSet.GlowShowUnpublishedControlTowerWidgets;
			AssertEquals("Name", "GlowShowUnpublishedControlTowerWidgets", item.Name);
			AssertEquals("Category", "GLOW/Control Tower", item.Category);
			AssertEquals("Caption", "Show unpublished Control Tower widgets", item.Caption);
			AssertEquals("Hint", "Show unpublished Control Tower widgets.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", false, item.DefaultValue);
		}

		public void TestGlowIndexerStorageSizeMB()
		{
			var item = ItemSet.GlowIndexerStorageSizeMB;
			AssertEquals("Name", "GlowIndexerStorageSizeMB", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Storage Size(MB)", item.Caption);
			AssertEquals("Hint", "Set the Glow Indexer storage size(MB).", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", 300, item.DefaultValue);
		}

		public void TestGlowIndexerRamSizeMB()
		{
			var item = ItemSet.GlowIndexerRamSizeMB;
			AssertEquals("Name", "GlowIndexerRamSizeMB", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "GLOW Indexer Ram Size(MB)", item.Caption);
			AssertEquals("Hint", "Set the Glow Indexer ram size(MB).", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", 100, item.DefaultValue);
		}

		public void TestGlowPermittedPrimaryIndexerAddresses()
		{
			var item = ItemSet.GlowPermittedPrimaryIndexerAddresses;
			AssertEquals("Name", "GlowPermittedPrimaryIndexerAddresses", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "Servers which can be primary Glow Indexers", item.Caption);
			AssertEquals("Hint", "DNS names or IP addresses of servers which can be primary Glow Indexers.", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, item.Options);
		}

		public void TestGlowEntityIndexNotUpdatedThresholdInMinutes()
		{
			var item = ItemSet.EntityIndexNotUpdatedThresholdInMinutes;
			AssertEquals("Name", "EntityIndexNotUpdatedThresholdInMinutes", item.Name);
			AssertEquals("Category", "GLOW/Services", item.Category);
			AssertEquals("Caption", "Entity Index Not Updated Threshold(Minutes)", item.Caption);
			AssertEquals("Hint", "Set the Entity Index Not Updated Threshold(Minutes).", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("Default", 0, item.DefaultValue);
		}

		public void TestNexusLogonDiscoveryEndpoint()
		{
			TestRegistryItem(
				ItemSet.NexusLogonDiscoveryEndpoint,
				SharedGlowRegistry.NexusLogonDiscoveryEndpointKey,
				GlowRegistry.Categories.GLOW_Neo,
				"Logon OIDC Discovery Document URL",
				"The URL for the metadata used to handle Neo external logons",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				expectedDefaultValue: string.Empty,
				testValueToSetAndRead: "https://idp.com/.well-known/openid-configuration");
		}

		public void TestNexusLogonClientID()
		{
			TestRegistryItem(
				ItemSet.NexusLogonClientID,
				SharedGlowRegistry.NexusLogonClientIDKey,
				GlowRegistry.Categories.GLOW_Neo,
				"Logon ClientID",
				"Set the ClientID used for Neo authentication requests",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				expectedDefaultValue: string.Empty,
				testValueToSetAndRead: "e8385215-e048-4446-a5a9-abd6bfa6ea5f");
		}

		public void TestNexusLogonClientSecret()
		{
			TestRegistryItem(
				ItemSet.NexusLogonClientSecret,
				SharedGlowRegistry.NexusLogonClientSecretKey,
				GlowRegistry.Categories.GLOW_Neo,
				"Logon Client Secret",
				"Set the Client Secret used to validate Neo authentication requests",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.IsPasswordVisibleForControllerUser,
				TextEditorType.Password,
				expectedDefaultValue: string.Empty,
				testValueToSetAndRead: "07ebc136-53dd-4934-ba29-2f514d66a6a0");
		}

		public void TestNeoEnableConversations()
		{
			TestRegistryItem(
				ItemSet.NeoEnableConversations,
				SharedGlowRegistry.NeoEnableConversations,
				GlowRegistry.Categories.GLOW_Neo,
				"Enable eConversations",
				"Set to Yes to enable the eConversation tab for Shipments, Bookings, and Orders.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestNeoConversationsEmailAddress_NoRegistryValue()
		{
			TestRegistryItem(
				ItemSet.NeoConversationsEmailAddress,
				"GlowNeoConversationsEmailAddress",
				GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox,
				"Neo eConversations Email Address",
				"This is the 'From' address that will appear on eConversation message notification emails.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				string.Empty,
				"");
		}

		public void TestNeoConversationsEmailAddress_ValidEmail()
		{
			TestRegistryItem(
				ItemSet.NeoConversationsEmailAddress,
				"GlowNeoConversationsEmailAddress",
				GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox,
				"Neo eConversations Email Address",
				"This is the 'From' address that will appear on eConversation message notification emails.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				string.Empty,
				"neoconversation@domain.com");
		}

		public void TestNeoConversationsEmailAddress_InvalidEmail()
		{
			AssertExceptionThrown<RegistryValidationException>(() => TestRegistryItem(
				ItemSet.NeoConversationsEmailAddress,
				"GlowNeoConversationsEmailAddress",
				GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox,
				"Neo eConversations Email Address",
				"This is the 'From' address that will appear on eConversation message notification emails.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				string.Empty,
				"NeoEConv"));
		}

		public void TestNeoDefaultFormFlowUrlRegistryItem()
		{
			TestRegistryItem(
				ItemSet.NeoDefaultFormFlowUrlRegistryItem,
				"GlowNeoDefaultFormFlowUrlRegistryItem",
				GlowRegistry.Categories.GLOW_Neo,
				"Neo Default Form-Flow URL",
				"The Neo Default Form-Flow URL, relative to the Glow Portal Root URL",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				"NEO/Desktop#/formFlow/default");
		}

		public void TestGetNeoDefaultFormFlowUrl()
		{
			ItemSet.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Portals");
			var dummy = Factory.New<DummyBusinessObject>();
			var defaultFormFlowUrl = GlowRegistry.GetNeoDefaultFormFlowUrl(dummy);

			AssertEquals($"https://localhost/Portals/NEO/Desktop#/formFlow/default/IDummyBizo/{dummy.PK}", defaultFormFlowUrl);
		}

		public void TestNeoConversationsMailbox()
		{
			MailboxSettingsTest.AssertMailbox(ItemSet.NeoConversationsMailBox, "GlowNeoConversations", GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox, RegistryOptions.PreserveTestValue);
		}

		public void TestEnableVerboseModeOnEmailProcessors()
		{
			TestRegistryItem(
				ItemSet.NeoEnableVerboseModeOnEmailProcessors,
				"GlowNeoEnableVerboseModeOnEmailProcessors",
				GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox,
				"Enable Verbose Mode on Email Processors",
				"This provides additional logging info to help troubleshooting email processors issues.",
				RegistryStorageFlags.System,
				false);
		}

		public void TestNeoEnableHyperlinksInDocumentMacros()
		{
			TestRegistryItem(
				ItemSet.NeoEnableHyperlinksInDocumentMacros,
				"GlowNeoEnableHyperlinksInDocumentMacros",
				GlowRegistry.Categories.GLOW_Neo,
				"Enable Neo Hyperlinks in Document and Report Macros",
				"This allows documents and reports with the GetTrackingUrl macro to generate Neo hyperlinks instead of WebTracker.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestNeoConversationsAttachmentDocType()
		{
			TestGenericRegistryItem(
				ItemSet.NeoConversationsAttachmentDocType,
				"GlowNeoConversationsAttachmentDocType",
				GlowRegistry.Categories.GLOW_Neo_ConversationsMailbox,
				(NoResString)"Document Type for eConversation attachments",
				(NoResString)"Select the Document Type that will be used when eConversations are added to eDocs.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				Core.Constants.RefDocTypes.MiscellaneousDocument);

			var editorInfo = (CodeFindBoxRegistryEditorInfo)ItemSet.NeoConversationsAttachmentDocType.EditorInfo;
			AssertEquals(ModuleIDs.RefDocType, editorInfo.ModuleID);
		}

		public void TestGlowEnableResourceStringsFullAccess()
		{
			TestRegistryItem(
				ItemSet.GlowEnableResourceStringsFullAccess,
				"GlowEnableResourceStringsFullAccess",
				GlowRegistry.Categories.GLOW,
				"Enable Resource Strings Full Access",
				"Allows access to write translation feedback for English resource strings.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);

			ItemSet.GlowEnableResourceStringsFullAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("GlowEnableResourceStringsFullAccess", true, ItemSet.GlowEnableResourceStringsFullAccess.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEnableSecurityGroupsForContactsInGLOW()
		{
			TestRegistryItem(
				ItemSet.EnableSecurityGroupsForContactsInGLOW,
				"EnableSecurityGroupsForContactsInGLOW",
				GlowRegistry.Categories.GLOW,
				"Enable Security Groups for Contacts in GLOW",
				"When set to YES, then GLOW rights discovery will be via the new contact groups, managed via the GLOW SSM portal. Management of GLOW security rights will no longer be possible via the Organization Contact record in CargoWiseOne.\r\nWhen set to NO, then GLOW rights discovery will be via legacy rights discovery method, managed via the Organization Contact record in CargoWiseOne.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);

			ItemSet.EnableSecurityGroupsForContactsInGLOW.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("EnableSecurityGroupsForContactsInGLOW", false, ItemSet.EnableSecurityGroupsForContactsInGLOW.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEndUserPolicyHyperlinks()
		{
			TestGenericRegistryItem(
				ItemSet.EndUserPolicyHyperlinks,
				"EndUserPolicyHyperlinks",
				GlowRegistry.Categories.GLOW,
				"End User Policy Hyperlinks",
				"Add hyperlinks to any relevant policies that affect end user access to GLOW based web portals. Typical policies include Terms of Use, Privacy Notice, Legal Information, etc.  Configured hyperlinks will be displayed to end users in key locations such as the cookie acceptance form, login pages, and the App menu.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue);

			AssertEquals("EndUserPolicyHyperlinks default value is empty list", 0, ItemSet.EndUserPolicyHyperlinks.DefaultValue.Count);
		}

		public void TestGlowExternalUserPortalsUri()
		{
			TestRegistryItem(
				ItemSet.GlowExternalUserPortalsUri,
				"GlowExternalUserPortalsUri",
				GlowRegistry.Categories.GLOW,
				"External User GLOW Portals Root URL",
				"In installations where external contact users have a different URL to internal staff users, this URL describes the external user facing URL. This approach may be used to distinguish internet activity from external users, from intranet traffic from internal users, for security reasons. When configured, this URL will also be used for the password reset link sent to Organization Contacts from the CW1 \"Send Password Set/Reset Email\" for CW1 Web Portal users.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
				TextEditorType.TextBox,
				string.Empty,
				"https://localhost/");
		}

		public void TestIndexSearchabilityPeriodDefault()
		{
			TestRegistryItem(
				ItemSet.IndexSearchabilityPeriodDefault,
				"IndexSearchabilityPeriodDefault",
				GlowRegistry.Categories.GLOW_Services,
				"Indexing Service Searchability Period Default",
				"Age of records to be included in the index. 1 to 36 months.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
				36, 1, 36);

			ItemSet.IndexSearchabilityPeriodDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 25);
			AssertEquals("IndexSearchabilityPeriodDefault", 25, ItemSet.IndexSearchabilityPeriodDefault.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestIndexSearchabilityPeriodOverrides()
		{
			TestGenericRegistryItem(
				ItemSet.IndexSearchabilityPeriodOverrides,
				"IndexSearchabilityPeriodOverrides",
				GlowRegistry.Categories.GLOW_Services,
				"Indexing Service Searchability Period Overrides",
				"Age of records to be included in the index. 0 means include all records regardless of age.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted);

			AssertEquals("IndexSearchabilityPeriodOverrides default list has two values", 3, ItemSet.IndexSearchabilityPeriodOverrides.DefaultValue.Count);
			var defaultValue = ItemSet.IndexSearchabilityPeriodOverrides.DefaultValue[0];
			AssertEquals("HVLVConsignment", defaultValue.Table);
			AssertEquals(3, defaultValue.DurationInMonths);

			defaultValue = ItemSet.IndexSearchabilityPeriodOverrides.DefaultValue[1];
			AssertEquals("ProcessHeader", defaultValue.Table);
			AssertEquals(1, defaultValue.DurationInMonths);

			defaultValue = ItemSet.IndexSearchabilityPeriodOverrides.DefaultValue[2];
			AssertEquals("ProcessTasks", defaultValue.Table);
			AssertEquals(3, defaultValue.DurationInMonths);
		}

		public void TestGlowContentSecurityPolicyMode()
		{
			var item = ItemSet.GlowContentSecurityPolicyMode;
			AssertEquals("Name", SharedGlowRegistry.ContentSecurityPolicyModeKey, item.Name);
			AssertEquals("Category", GlowRegistry.Categories.GLOW, item.Category);
			AssertEquals("Caption", "GLOW Content Security Policy Mode", item.Caption);
			AssertEquals("Hint", @"This setting controls the Content Security Policy (CSP) for GLOW.
 
CSP is an added layer of security that helps to detect and mitigate Cross-Site Scripting (XSS), click-jacking and other code injection attacks that result from the execution of malicious content in the trusted web page context. These attacks can form the basis for data theft, site defacement, and malware distribution. When a resource is requested by a web page - such as a script, image, CSS etc - the browser will block it from loading if it violates the CSP.
 
The following modes are available to control the behavior of the browser when it attempts to load a resource that violates the CSP:
 
Disabled - CSP will be disabled
 
Report Only - any CSP violation will be reported; the resource will be loaded
 
Enforce and Report - any CSP violation be reported; the resource will not be loaded
 
Enforce Only - any CSP violation will prevent the resource from being loaded without reporting it", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController, item.Options);
			AssertEquals("Default", "EnforceAndReport", item.DefaultValue);
			AssertArrayEqualsByElements(
				"ContentSecurityPolicy Mode",
				new[] { "Disabled", "ReportOnly", "EnforceAndReport", "EnforceOnly" },
				((CodePairRegistryDataType)item.DataType).LookUpList.GetAllCodes());
		}

		public void TestNeoGtmEnabled()
		{
			TestRegistryItem(
				ItemSet.NeoGtmEnabled,
				"NeoGtmEnabled",
				GlowRegistry.Categories.GLOW_Neo,
				"Enable Neo GTM",
				"Set to Yes to enable the Neo GTM link in the Neo portal.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestNeoGtmEndpointURL()
		{
			TestRegistryItem(
				ItemSet.NeoGtmEndpointURL,
				"NeoGtmEndpointURL",
				GlowRegistry.Categories.GLOW_Neo,
				"Neo GTM Endpoint URL",
				"The URL of Neo GTM endpoint",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				string.Empty,
				"https://localhost/");
		}

		IEnumerable<IRegistryItem> GetGlowExclusiveRegistryItems()
		{
			return new RegistryItemSetLocator()
				.GetAllRegistryItems()
				.Where(item => item.Category == GlowRegistry.Categories.GLOW_Services ||
					item.Category == GlowRegistry.Categories.GLOW_Neo ||
					item.Category == GlowRegistry.Categories.GLOW);
		}

		public void TestModuleAllowGlowIndexingSearch()
		{
			using (ItemSet.GlowUseIndexingForModuleList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
			{
				AssertEquals(false, ItemSet.IsGlowIndexSearchAllowedForModule("GlbStaff"));
			}

			using (ItemSet.GlowUseIndexingForModuleList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "GlbStaff" }))
			{
				AssertEquals(true, ItemSet.IsGlowIndexSearchAllowedForModule("GlbStaff"));
				AssertEquals(false, ItemSet.IsGlowIndexSearchAllowedForModule("OtherModule"));
			}
		}

		public void TestGlowUseIndexingServiceForGlobalSearch()
		{
			TestRegistryItem(
				ItemSet.GlowUseIndexingServiceForGlobalSearch,
				"GlowUseIndexingServiceForGlobalSearch",
				GlowRegistry.Categories.GLOW_Services,
				"Indexing Service for Global Search",
				"When enabled, the CargoWise Search bar can be used to search for records throughout the system.  When disabled, the CargoWise Search will only return modules within the system.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				true);
		}

		public void TestGlowEnableTaskIndexing()
		{
			TestRegistryItem(
				ItemSet.GlowEnableTaskIndexing,
				"GlowEnableTaskIndexing",
				GlowRegistry.Categories.GLOW,
				"Enable Task Indexing",
				"Enable Task List Index",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				false);
		}

		public void TestGlowUseIndexingForModuleList()
		{
			TestRegistryItem(
				ItemSet.GlowUseIndexingForModuleList,
				"GlowUseIndexingForModuleList",
				GlowRegistry.Categories.GLOW_Services,
				"GLOW Use Indexing For Module List",
				@$"The modules that will use the GLOW Indexing Service to augment search queries in {Core.Constants.ProductName}.

Beta:
When enabling a module that is still in Beta, module searches may have limited functionality.
Please report any issues that are encountered.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted,
				Array.Empty<string>(),
				typeof(GlowUseIndexingForModuleListEditorInfo));
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				return new[]
				{
					"GlowNeoConversationsIMAPSecureConnection",
					"GlowNeoConversationsMailboxPassword",
					"GlowNeoConversationsMailboxUserName",
					"GlowNeoConversationsMailServer",
					"GlowNeoConversationsMailServerPort",
					"GlowNeoConversationsPOP3SecureConnection",
					"GlowNeoConversationsMailRetrievalProtocol",
				};
			}
		}
	}
}

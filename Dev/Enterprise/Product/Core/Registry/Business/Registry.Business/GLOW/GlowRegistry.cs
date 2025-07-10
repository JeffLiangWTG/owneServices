using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.CW1.Resources;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using SharedGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.Registry.Business
{
	public sealed class GlowRegistry : RegistryItemSet, IGlowRegistry
	{
		public static GlowRegistry Instance
		{
			get { return instance ?? (instance = new GlowRegistry()); }
		}

		[ThreadStatic]
		static GlowRegistry instance;

		public override bool IsForProductivityWise => true;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString GLOW => ResString.GetMultilingualString("GLOW", "GLOW");
			public static MultilingualString GLOW_Services => CombineCategories(GLOW, ResString.GetMultilingualString("Services", "Services"));
			public static MultilingualString GLOW_Services_Elastic => CombineCategories(GLOW_Services, ResString.GetMultilingualString("Elasticsearch", "Elasticsearch"));
			public static MultilingualString GLOW_Neo => CombineCategories(GLOW, ResString.GetMultilingualString("Neo", "Neo"));
			public static MultilingualString GLOW_Neo_ConversationsMailbox => CombineCategories(GLOW_Neo, ResString.GetMultilingualString("ConversationsMailbox", "Neo eConversations Mailbox"));
			public static MultilingualString GLOW_Control_Tower => CombineCategories(GLOW, ResString.GetMultilingualString("ControlTower", "Control Tower"));
		}

		#region SuppressResourceStringsCheckRegion

		internal static string GetRootGlowUriDefaultValue(GlowUriType uriType, string suffix)
		{
			var productRegistration = ObjectFactory.Get<IProductRegistration>();
			var registrationKey = productRegistration.Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var databaseCode = registrationKey.ServerCode;

			if (string.IsNullOrWhiteSpace(enterpriseCode) || string.IsNullOrWhiteSpace(databaseCode))
			{
				return "ERROR: Enterprise Code or Database Code is empty!";
			}
			else
			{
				return SharedGlowRegistry.CalculateGlowUri(enterpriseCode, databaseCode, productRegistration.IsWiseTechGlobalInternalSystem(), uriType, suffix).AbsoluteUri;
			}
		}

		public bool IsGlowIndexSearchAllowedForModule(string module)
		{
			return GlowUseIndexingForModuleList.Value.Contains(module);
		}

		public int MaximumNumberOfModuleFiltersSearchResults => GlowMaximumNumberOfModuleFiltersSearchResults.Value;

		public bool IndexSearchUsageCollector => GlowIndexSearchUsageCollector.Value;

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string GlowServiceUri => GetServiceUri(GlowServiceUriRegistryItem);

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string GlowServiceExternalUri => GetServiceUri(GlowServiceExternalUriRegistryItem);

		static string GetServiceUri(StringRegistryItem registryItem)
		{
			var value = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (!value.EndsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				value += "/";
			}

			return value;
		}

		public static string[] GetRestrictedPortalsOverride()
		{
			var item = new BusinessObjectFactory().LoadTop1<StmData>(new ZQuery(StmDataSchema.SD_Name, "GlowRestrictedModulesOverride"));
			if (item == null)
			{
				return Array.Empty<string>();
			}

			using var stream = item.GetSD_BinaryValueReader();
			try
			{
				return (string[])new System.Runtime.Serialization.DataContractSerializer(typeof(string[])).ReadObject(stream);
			}
			catch (Exception ex) when (ex is XmlException || ex is System.Runtime.Serialization.SerializationException)
			{
				ErrorReporter.ReportOnce("GlowRestrictedModulesOverride failed to be read by CW1", ex);
				return Array.Empty<string>();
			}
		}

		public StringRegistryItem GlowServiceUriRegistryItem
		{
			get
			{
				return GetItem("GlowServiceUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowServiceUri",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("4E4AC396-A079-4C3A-81E5-651311F3EA71", "GLOW Service URL"),
						ResString.GetMultilingualString("480FAE48-9ECA-482E-A8DC-73D15B036598", "The URL to the GLOW service"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue);
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public StringRegistryItem GlowServiceExternalUriRegistryItem
		{
			get
			{
				return GetItem("GlowServiceExternalUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowServiceExternalUri",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("4c38a22e-8380-463c-83be-3eb8acd31b90", "External User GLOW Service URL"),
						ResString.GetMultilingualString("84f131a0-a327-4a73-ab4e-003a507722da", "The URL to the GLOW service in installations where external contact users have a different Service URL to internal staff users."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue);
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public BooleanRegistryItem EnableUnpublishedModules
		{
			get
			{
				return GetItem("GlowEnableUnpublishedModules", delegate
				{
					var item = new BooleanRegistryItem(
						"GlowEnableUnpublishedModules",
						Categories.GLOW,
						(NoResString)"Enable Unpublished Modules",
						(NoResString)"Allows access to modules which have not been published.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
					return item;
				});
			}
		}

		public StringRegistryItem GlowPortalsUri
		{
			get
			{
				return GetItem("GlowPortalsUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowPortalsUri",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("842ECD2D-1C79-4DCF-90B4-9AE606400049", "GLOW Portals Root URL"),
						ResString.GetMultilingualString("E0908FCE-1789-42BC-86B5-603E52DA429B", "The URL to GLOW Web Portals"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue);
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public StringRegistryItem GlowDefaultWebPortal
		{
			get
			{
				return GetItem("GlowDefaultWebPortal", delegate
				{
					var item = new StringRegistryItem(
						"GlowDefaultWebPortal",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("0E139A58-1C24-40DC-A256-B5ECE47578F6", "Default Web Portal"),
						ResString.GetMultilingualString("45327A84-1A3E-48EE-A631-405382C06305", "The default web portal to use when no portal is specified in URL."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						string.Empty);
					item.DataType = new StringRegistryDataType(CharacterCase.Upper, 3, 3);
					return item;
				});
			}
		}

		public StringRegistryItem GlowPushServiceUrl
		{
			get
			{
				return GetItem("GlowPushServiceUrl", delegate
				{
					var sri = new StringRegistryItem(
						"GlowPushServiceUrl",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("E8158695-F2E1-44D8-B190-D75A79E6FE7A", "Push Service URL"),
						ResString.GetMultilingualString("E75CA01D-1DB1-479A-9FE3-02EA52889EE0", "The URL to the service used for Push Notifications. This registry item is currently not used by {0}.", Core.Constants.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						"http://localhost/Push/");
					sri.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return sri;
				});
			}
		}

		public StringRegistryItem EdiProdOAuthRootUriKey
		{
			get
			{
				return GetItem(SharedGlowRegistry.EdiProdOAuthRootUriKey, delegate
				{
					var sri = new StringRegistryItem(
						SharedGlowRegistry.EdiProdOAuthRootUriKey,
						Categories.GLOW_Services,
						(NoResString)"ediProd OAuth Root URL",
						(NoResString)"The base URI for OAuth services provided by ediProd",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsValueMandatory,
						"https://myaccount-portal.cargowise.com/myaccount/oauth");
					sri.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return sri;
				});
			}
		}

		public BooleanRegistryItem EnableAdvancedDataAutomationWizard
		{
			get
			{
				return GetItem("EnableAdvancedDataAutomationWizard", delegate
				{
					return new BooleanRegistryItem(
						"EnableAdvancedDataAutomationWizard",
						Categories.GLOW,
						ResString.GetMultilingualString("81547F79-76ED-486F-9C2F-B22C6F13A076", "Advanced Data Automation Wizard Enabled"),
						ResString.GetMultilingualString("5AE6CDF5-9F79-47C2-95FF-AE4E3236405D", @"Change to 'Yes' to enable the Advanced Data Automation Wizard on supported modules and entities ONLY.

Enabling this functionality requires WiseTech Global to set up relevant services.If these services have not been set up, this registry item will automatically be disabled when you next login."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableGridWideProposedValues
		{
			get
			{
				const string key = "GlowEnableGridWideProposedValues";
				return GetItem(key, delegate
				{
					return new BooleanRegistryItem(
						key,
						Categories.GLOW,
						(NoResString)"Grid-wide Proposed Values Enabled",
						(NoResString)@"Change to 'No' to disable grid-wide proposed values in GLOW. When disabled, data grids will only have proposed values enabled on the row currently being edited.

The web portal's application pool will need to be restarted for changes to take effect.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableNewSignalRClient
		{
			get
			{
				const string key = "GlowEnableNewSignalRClient";
				return GetItem(key, delegate
				{
					return new BooleanRegistryItem(
						key,
						Categories.GLOW,
						(NoResString)"Enable New ASP.NET Core SignalR Client",
						(NoResString)"Change to 'Yes' to enable the new ASP.NET Core SignalR client.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem SuppressTrailingZeroes
		{
			get
			{
				const string key = "GlowSuppressTrailingZeroes";
				return GetItem(key, delegate
				{
					return new BooleanRegistryItem(
						key,
						Categories.GLOW,
						ResString.GetMultilingualString("F4AC454B-2286-4AC5-8268-CB8B219385BA", "Suppress Trailing Zeros"),
						ResString.GetMultilingualString("7609316F-F5EC-4513-B841-C6DA7DDFCA2A", "Change to ‘Yes’ to suppress trailing zeros."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableServiceWorker
		{
			get
			{
				const string key = "GlowEnableServiceWorker";
				return GetItem(key, delegate
				{
					return new BooleanRegistryItem(
						key,
						Categories.GLOW,
						(NoResString)"Service Worker Enabled",
						(NoResString)"Change to 'No' to disable the GLOW web portal service worker. The web portal's application pool will need to be restarted for changes to take effect.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public StringRegistryItem GlowAuthenticationEncryptionKey
		{
			get
			{
				return GetItem("GlowAuthEncryptionKey", delegate
				{
					var result = new StringRegistryItem(
						"GlowAuthEncryptionKey",
						Categories.GLOW,
						(NoResString)"Authentication Encryption Key",
						(NoResString)"Key used for encryption of the GLOW authentication cookie. The key should be 32 bytes long.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
					result.DataType = new BinaryKeyRegistryDataType(keySize: 32);
					return result;
				});
			}
		}

		public StringRegistryItem GlowAuthenticationHmacKey
		{
			get
			{
				return GetItem("GlowAuthHmacKey", delegate
				{
					var result = new StringRegistryItem(
						"GlowAuthHmacKey",
						Categories.GLOW,
						(NoResString)"Authentication HMAC Key",
						(NoResString)"Key used for integrity checks of the GLOW authentication cookie. The key should be 64 bytes long.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
					result.DataType = new BinaryKeyRegistryDataType(keySize: 64);
					return result;
				});
			}
		}

		public BooleanRegistryItem PreserveGlowTheme
		{
			get
			{
				const string key = "PreserveGlowTheme";
				return GetItem(key, delegate
				{
					return new BooleanRegistryItem(
						key,
						Categories.GLOW,
						ResString.GetMultilingualString("2277B66F-9FF5-44ED-A966-58004929B8E9", "Preserve Glow Theme"),
						ResString.GetMultilingualString("A0D8715F-BC55-4F34-84F2-29C47C6F77EB", @"If current database is the test database, change to 'Yes' to preserve current GLOW theme in 'Copy Production To Test'. Change to 'No' to use the production database theme.

If current database is the production database, this registry has no effect.

NOTE: 'Copy Production To Test' is a function in Database Backup And Restore tool. It is used to create a test database which can be used like a sandbox environment for testing features or training."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowEnableDevelopmentModeKey
		{
			get
			{
				return GetItem("GlowEnableDevelopmentMode", () =>
				{
					return new BooleanRegistryItem(
						"GlowEnableDevelopmentMode",
						Categories.GLOW_Services,
						(NoResString)"Enable Development Mode",
						(NoResString)"Change to ‘Yes’ to enable development mode on the platform builder. Any change, from ‘Yes’ or ‘No’, will require the web server to restart.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false
					);
				});
			}
		}

		public BooleanRegistryItem GlowIndexCdcFilterByUpdateMask
		{
			get
			{
				return GetItem("GlowIndexCdcFilterByUpdateMask", delegate
				{
					return new BooleanRegistryItem(
						"GlowIndexCdcFilterByUpdateMask",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer CDC Filter By Update Mask",
						(NoResString)"GLOW Indexer gets changed data by filtering update mask when SQL Change Data Capture is in use.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public IntRegistryItem GlowNumberOfAdditionalIndexers
		{
			get
			{
				return GetItem("GlowNumberOfAdditionalIndexers", delegate
				{
					return new IntRegistryItem(
						"GlowNumberOfAdditionalIndexers",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("4F8BE54C-28F2-4653-82DF-B2309DC12C80", "GLOW Indexer Parallel Indexers"),
						ResString.GetMultilingualString("F6B296B8-572C-41C6-9ECB-3D6CA8C21307", "Number of additional Indexers that perform index rebuilds."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1);
				});
			}
		}

		public BooleanRegistryItem GlowIndexParameterizeQueryConstants
		{
			get
			{
				return GetItem("GlowIndexParameterizeQueryConstants", delegate
				{
					return new BooleanRegistryItem(
						"GlowIndexParameterizeQueryConstants",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("FEDE01FC-CAD5-4CB0-8A5D-C436DB9DEC83", "GLOW Indexer Parameterize Query Constants"),
						ResString.GetMultilingualString("02B54BAF-4C88-499A-A3BB-2FA10455AC4C", "GLOW Indexer parameterizes query constants."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowUseHttpInterCluster
		{
			get
			{
				return GetItem("GlowUseHttpInterCluster", delegate
				{
					return new BooleanRegistryItem(
						"GlowUseHttpInterCluster",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("FCC52858-A9DC-4F06-A497-783E0647A8B2", "GLOW inter-node communication uses HTTP"),
						ResString.GetMultilingualString("AA4075B5-FB8B-4978-BB7A-5B316E20F878", "GLOW inter-node communication uses HTTP."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowIndexSearchUsageCollector
		{
			get
			{
				return GetItem("GlowIndexSearchUsageCollector", delegate
				{
					return new BooleanRegistryItem(
						"GlowIndexSearchUsageCollector",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("1E5834A8-91E4-420C-9778-0D0D4AAB2225", "CargoWise Index Search Usage Collector"),
						ResString.GetMultilingualString("42ABD871-94E4-4832-82DD-180B56480169", "CargoWise Index Search Usage Collector."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowIndexUsePersistentConnection
		{
			get
			{
				return GetItem("GlowIndexUsePersistentConnection", delegate
				{
					return new BooleanRegistryItem(
						"GlowIndexUsePersistentConnection",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer Use Persistent Connection",
						(NoResString)"GLOW Indexer uses a persistent database connection.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem HealthCheckTrustsLocalNetwork
		{
			get
			{
				return GetItem("GlowHealthCheckTrustsLocalNetwork", () =>
				{
					return new BooleanRegistryItem(
						"GlowHealthCheckTrustsLocalNetwork",
						Categories.GLOW_Services,
						(NoResString)"Health Check Trusts Local Network",
						(NoResString)"Change to 'Yes' to allow health check requests from the same private IP address range as the web server. Requests with a 'Forwarded' or 'X-Forwarded-For' header will still be denied.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false
					);
				});
			}
		}

		public IntRegistryItem GlowMaximumNumberOfTrackingResults
		{
			get
			{
				return GetItem("GlowMaximumNumberOfTrackingResults", () =>
				{
					return new IntRegistryItem(
						"GlowMaximumNumberOfTrackingResults",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("8D00B8FC-4B59-4C17-A3A6-69B722EED4F4", "Maximum Number of Tracking Results"),
						ResString.GetMultilingualString("20E981ED-8237-4AD9-878A-8519D72422C5", "The maximum number of tracking results returned by the Tracking Service."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 10
					);
				});
			}
		}

		public BooleanRegistryItem GlowConfigurationTmplEnabled
		{
			get
			{
				return GetItem("GlowConfigurationTmplEnabled", delegate
				{
					return new BooleanRegistryItem(
						"GlowConfigurationTmplEnabled",
						Categories.GLOW,
						(NoResString)"Configuration Templates Enabled",
						(NoResString)"Set to true to enable configuration templates.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem ForceImportable
		{
			get
			{
				return GetItem("ForceImportable", delegate
				{
					// this setting will default to true until we sort out import restrictions satisfactorily
					return new BooleanRegistryItem(
						"ForceImportable",
						Categories.GLOW,
						(NoResString)"Force Importable Enabled",
						(NoResString)@"Change to 'Yes' to force enable import on all modules and entities; disregarding whether the entities have been marked as importable by development.

Enabling this functionality requires WiseTech Global to set up relevant services.If these services have not been set up, this registry item will automatically be disabled when you next login.

'Advanced Data Automation Wizard Enabled' will still need to be enabled, in addition to this registry setting, in order to import using the ADAW on unsupported tables.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowUseIndexingServiceForGlobalSearch
		{
			get
			{
				return GetItem("GlowUseIndexingServiceForGlobalSearch", delegate
				{
					return new BooleanRegistryItem(
						"GlowUseIndexingServiceForGlobalSearch",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("67EFA24E-3C23-41D7-A0B3-7D005DB7DF84", "Indexing Service for Global Search"),
						ResString.GetMultilingualString("D42C7C9A-12A1-4841-92B8-B427E40126E3", "When enabled, the CargoWise Search bar can be used to search for records throughout the system.  When disabled, the CargoWise Search will only return modules within the system."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public StringArrayRegistryItem GlowUseIndexingForModuleList
		{
			get
			{
				return GetItem("GlowUseIndexingForModuleList", () =>
					new StringArrayRegistryItem(
						"GlowUseIndexingForModuleList",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("7bb37a60-5626-42bf-a421-bd55dbb2757d", "GLOW Use Indexing For Module List"),
						ResString.GetMultilingualString("74c9d205-afe9-42f4-b4f0-882d771c2faa", @"The modules that will use the GLOW Indexing Service to augment search queries in {0}.

Beta:
When enabling a module that is still in Beta, module searches may have limited functionality.
Please report any issues that are encountered.", Core.Constants.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						Array.Empty<string>(),
						new GlowUseIndexingForModuleListEditorInfo()
					));
			}
		}

		public IntRegistryItem GlowMaximumNumberOfModuleFiltersSearchResults
		{
			get
			{
				return GetItem("GlowMaximumNumberOfModuleFiltersSearchResults", () =>
				{
					return new IntRegistryItem(
						"GlowMaximumNumberOfModuleFiltersSearchResults",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("4428f514-bab0-476f-95b3-4d4d1b83f172", "Maximum Number Of Module Filters Search Results"),
						ResString.GetMultilingualString("cc08762d-ad6e-4a02-9cf4-adf7e818bed7", "The maximum number of module filters search results returned by the Indexing Service."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 200,
						minValue: 1,
						maxValue: 1000
					);
				});
			}
		}

		public StringRegistryItem AzureSignalRConnectionString
		{
			get
			{
				return GetItem("AzureSignalRConnectionString", delegate
				{
					return new StringRegistryItem(
						"AzureSignalRConnectionString",
						Categories.GLOW_Services,
						(NoResString)"Azure SignalR Connection String",
						(NoResString)"Set the Azure SignalR connection string to enable live messages.",
						new AzureSignalRConnectionStringDataType(),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForCargoWise);
				});
			}
		}

		ICodeDescriptionPairListProvider GlowIndexerStrategyPreferenceListProvider
		{
			get
			{
				if (glowIndexerStrategyPreferenceListProvider == null)
				{
					glowIndexerStrategyPreferenceListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var strategyList = new CodeDescriptionPairList();
						strategyList.AddPair(Core.Constants.IndexerStrategy.CT, ResString.GetMultilingualString("DEA41252-8D26-4A05-AEA2-5C050FDE4530", "SQL Change Tracking"));
						strategyList.AddPair(Core.Constants.IndexerStrategy.CDC, ResString.GetMultilingualString("164317F4-CC0C-434D-877C-BFDF7412C390", "SQL Change Data Capture"));
						strategyList.AddPair(Core.Constants.IndexerStrategy.Audit, ResString.GetMultilingualString("0EF8A574-D88F-4377-9ACB-A22786356042", "SQL Audit Table"));
						strategyList.DefaultCode = Core.Constants.IndexerStrategy.CDC;
						return strategyList;
					});
				}
				return glowIndexerStrategyPreferenceListProvider;
			}
		}
		ICodeDescriptionPairListProvider glowIndexerStrategyPreferenceListProvider;

		public CodePairRegistryItem GlowIndexerStrategyPreference
		{
			get
			{
				return GetItem("GlowIndexerStrategyPreference", delegate
				{
					return new CodePairRegistryItem(
						"GlowIndexerStrategyPreference",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer Strategy Preference",
						(NoResString)"GLOW Indexer will attempt to use the specified strategy where available. The possible options are: SQL Change Tracking, SQL Change Data Capture or BI Audit.",
						GlowIndexerStrategyPreferenceListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Core.Constants.IndexerStrategy.CDC);
				});
			}
		}

		public CodePairRegistryItem GlowIndexerStorageType
		{
			get
			{
				return GetItem("GlowIndexerStorageType", delegate
				{
					return new CodePairRegistryItem(
						"GlowIndexerStorageType",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("9f64031b-d348-4c1e-b679-420648ccf820", "GLOW Indexer Storage Type"),
						ResString.GetMultilingualString("a3cc3f6f-acda-49c8-b9b9-10b4397b37c8", "Set the storage type used by GLOW Indexer."),
						GlowIndexerStorageListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						EnvProxy.IsHostedWithCargowise ? "LuceneClustered" : "Lucene");
				});
			}
		}

		ICodeDescriptionPairListProvider GlowIndexerStorageListProvider
		{
			get
			{
				if (glowIndexerStorageListProvider == null)
				{
					glowIndexerStorageListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair("Lucene", (NoResString)"Lucene");
						list.AddPair("LuceneClustered", (NoResString)"Lucene (Clustered)");

						if (EnvProxy.IsHostedWithCargowise
							|| ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem())
						{
							list.AddPair("Elasticsearch", (NoResString)"Elasticsearch");
						}

						return list;
					});
				}

				return glowIndexerStorageListProvider;
			}
		}
		ICodeDescriptionPairListProvider glowIndexerStorageListProvider;

		public StringRegistryItem GlowIndexerElasticServerUris
		{
			get
			{
				return GetItem("GlowIndexerElasticServerUris", delegate
				{
					return new StringRegistryItem(
						"GlowIndexerElasticServerUris",
						Categories.GLOW_Services_Elastic,
						(NoResString)"Glow Indexer Elasticsearch Server URIs",
						(NoResString)"The URI list of Elasticsearch nodes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem GlowIndexerElasticUsername
		{
			get
			{
				return GetItem("GlowIndexerElasticUsername", delegate
				{
					return new StringRegistryItem(
						"GlowIndexerElasticUsername",
						Categories.GLOW_Services_Elastic,
						(NoResString)"Glow Indexer Elasticsearch Username",
						(NoResString)"Set the user name GLOW Indexer uses to access Elasticsearch.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem GlowIndexerElasticPassword
		{
			get
			{
				return GetItem("GlowIndexerElasticPassword", delegate
				{
					var item = new StringRegistryItem(
						"GlowIndexerElasticPassword",
						Categories.GLOW_Services_Elastic,
						(NoResString)"Glow Indexer Elasticsearch Password",
						(NoResString)"Set the password GLOW Indexer uses to access Elasticsearch.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return item;
				});
			}
		}

		public BooleanRegistryItem GlowIndexerSplitQueries
		{
			get
			{
				return GetItem("GlowIndexerSplitQueries", delegate
				{
					return new BooleanRegistryItem(
						"GlowIndexerSplitQueries",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer Should Split Queries",
						(NoResString)"GLOW Indexer should split queries when the current service is a 64-bit process.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public IntRegistryItem GlowIndexerStorageSizeMB
		{
			get
			{
				return GetItem("GlowIndexerStorageSizeMB", delegate
				{
					return new IntRegistryItem(
						"GlowIndexerStorageSizeMB",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer Storage Size(MB)",
						(NoResString)"Set the Glow Indexer storage size(MB).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						300,
						1,
						int.MaxValue);
				});
			}
		}

		public IntRegistryItem GlowIndexerRamSizeMB
		{
			get
			{
				return GetItem("GlowIndexerRamSizeMB", delegate
				{
					return new IntRegistryItem(
						"GlowIndexerRamSizeMB",
						Categories.GLOW_Services,
						(NoResString)"GLOW Indexer Ram Size(MB)",
						(NoResString)"Set the Glow Indexer ram size(MB).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100,
						1,
						int.MaxValue);
				});
			}
		}

		public BooleanRegistryItem GlowShowUnpublishedControlTowerWidgets
		{
			get
			{
				return GetItem("GlowShowUnpublishedControlTowerWidgets", delegate
				{
					return new BooleanRegistryItem(
						"GlowShowUnpublishedControlTowerWidgets",
						Categories.GLOW_Control_Tower,
						(NoResString)"Show unpublished Control Tower widgets",
						(NoResString)"Show unpublished Control Tower widgets.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public JsonStringArrayRegistryItem GlowPermittedPrimaryIndexerAddresses
		{
			get
			{
				return GetItem("GlowPermittedPrimaryIndexerAddresses", delegate
				{
					return new JsonStringArrayRegistryItem(
						"GlowPermittedPrimaryIndexerAddresses",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("53b503b0-711e-483f-9e58-0e9ce69ce173", "Servers which can be primary Glow Indexers"),
						ResString.GetMultilingualString("9d79c2ed-b7cc-4bed-aa78-58e9a9a20cff", "DNS names or IP addresses of servers which can be primary Glow Indexers."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted);
				});
			}
		}

		public IndexShardRegistryItem GlowIndexerShards
		{
			get
			{
				var defaultList = new IndexShardList();
				foreach(var item in DefaultIndexShard.DefaultIndexShardMap)
				{
					defaultList.Add(new IndexShard() { IndexTableName = item.Key, ShardIndex = item.Value, IsOverridden = false });
				}
				return GetItem("GlowIndexerShards", delegate
				{
					var item = new IndexShardRegistryItem(
						"GlowIndexerShards",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("78546b0b-bc36-459e-806f-12793d0104f4", "GLOW Indexer Shards"),
						ResString.GetMultilingualString("7a845fab-1cd7-4b09-8978-cf6580bf432e", "Set shards for groups of tables. Shard 0 is the default shard."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultList);
					return item;
				});
			}
		}

		public IntRegistryItem EntityIndexNotUpdatedThresholdInMinutes
		{
			get
			{
				return GetItem("EntityIndexNotUpdatedThresholdInMinutes", delegate
				{
					return new IntRegistryItem(
						"EntityIndexNotUpdatedThresholdInMinutes",
						Categories.GLOW_Services,
						(NoResString)"Entity Index Not Updated Threshold(Minutes)",
						(NoResString)"Set the Entity Index Not Updated Threshold(Minutes).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						0,
						1,
						int.MaxValue);
				});
			}
		}

		public StringRegistryItem NexusLogonDiscoveryEndpoint
		{
			get
			{
				return GetItem(SharedGlowRegistry.NexusLogonDiscoveryEndpointKey, delegate
				{
					var sri = new StringRegistryItem(
						SharedGlowRegistry.NexusLogonDiscoveryEndpointKey,
						Categories.GLOW_Neo,
						(NoResString)"Logon OIDC Discovery Document URL",
						(NoResString)"The URL for the metadata used to handle Neo external logons",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport)
					{
						DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false }
					};
					return sri;
				});
			}
		}

		public StringRegistryItem NexusLogonClientID
		{
			get
			{
				return GetItem(SharedGlowRegistry.NexusLogonClientIDKey, delegate
				{
					return new StringRegistryItem(
						SharedGlowRegistry.NexusLogonClientIDKey,
						Categories.GLOW_Neo,
						(NoResString)"Logon ClientID",
						(NoResString)"Set the ClientID used for Neo authentication requests",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public StringRegistryItem NexusLogonClientSecret
		{
			get
			{
				return GetItem(SharedGlowRegistry.NexusLogonClientSecretKey, delegate
				{
					return new StringRegistryItem(
						SharedGlowRegistry.NexusLogonClientSecretKey,
						Categories.GLOW_Neo,
						(NoResString)"Logon Client Secret",
						(NoResString)"Set the Client Secret used to validate Neo authentication requests",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsPasswordVisibleForControllerUser)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
					};
				});
			}
		}

		public BooleanRegistryItem NeoEnableConversations
		{
			get
			{
				return GetItem(SharedGlowRegistry.NeoEnableConversations, delegate
				{
					return new BooleanRegistryItem(
						SharedGlowRegistry.NeoEnableConversations,
						Categories.GLOW_Neo,
						ResString.GetMultilingualString("1d80ad65-0975-4120-a10f-9c9faa79a509", "Enable eConversations"),
						ResString.GetMultilingualString("c83d19eb-00d7-46b5-bd61-8d1687e8abe9", "Set to Yes to enable the eConversation tab for Shipments, Bookings, and Orders."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public StringRegistryItem NeoConversationsEmailAddress
		{
			get
			{
				return GetItem("GlowNeoConversationsEmailAddress", delegate
				{
					return new StringRegistryItem(
						"GlowNeoConversationsEmailAddress",
						Categories.GLOW_Neo_ConversationsMailbox,
						ResString.GetMultilingualString("692b5e80-1d55-49fd-b8dc-ecb1cfd2fe81", "Neo eConversations Email Address"),
						ResString.GetMultilingualString("7379faac-d71d-456f-bc1e-ae85288cc7c8", "This is the 'From' address that will appear on eConversation message notification emails."),
						new EmailStringRegistryDataType(),
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
			}
		}

		public BooleanRegistryItem NeoEnableVerboseModeOnEmailProcessors
		{
			get
			{
				return GetItem("GlowNeoEnableVerboseModeOnEmailProcessors", delegate
				{
					return new BooleanRegistryItem(
						"GlowNeoEnableVerboseModeOnEmailProcessors",
						Categories.GLOW_Neo_ConversationsMailbox,
						ResString.GetMultilingualString("e6d8c59d-65da-4bb8-a9c1-6ae892819750", "Enable Verbose Mode on Email Processors"),
						ResString.GetMultilingualString("a3635bab-db8a-4b87-816b-1b53e048198d", "This provides additional logging info to help troubleshooting email processors issues."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public StringRegistryItem NeoConversationsAttachmentDocType
		{
			get
			{
				return GetItem("GlowNeoConversationsAttachmentDocType", delegate
				{
					return new StringRegistryItem(
						"GlowNeoConversationsAttachmentDocType",
						Categories.GLOW_Neo_ConversationsMailbox,
						ResString.GetMultilingualString("7d84cfde-09b0-4c3b-acf1-886e1e3da820", "Document Type for eConversation attachments"),
						ResString.GetMultilingualString("58508ad7-637f-4a15-8bea-dede0d310e45", "Select the Document Type that will be used when eConversations are added to eDocs."),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						Core.Constants.RefDocTypes.MiscellaneousDocument)
					{
						EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, GetDocTypeCollection)
					};
				});
			}
		}

		IBusinessObjectCollection GetDocTypeCollection(BusinessObjectFactory factory)
		{
			var query = new ZQuery(RefDocTypeSchema.RT_ReferenceType, "ALL");
			query.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.Equal, "SCL");

			return (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefDocTypeCollection>(), new object[] { factory, query });
		}

		public OAuthMailboxSettings NeoConversationsMailBox => neoConversationsMailBox ??= new OAuthMailboxSettings("GlowNeoConversations", Categories.GLOW_Neo_ConversationsMailbox, GetMailBoxItem);
		OAuthMailboxSettings neoConversationsMailBox;

		IRegistryItem GetMailBoxItem(string key, MailboxSettings.CreateMailBoxItemDelegate createMailBoxItemDelegate)
		{
			return GetItem(key, () => createMailBoxItemDelegate());
		}

		protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
		{
			return NeoConversationsMailBox.GetAllItems();
		}

		public StringRegistryItem NeoDefaultFormFlowUrlRegistryItem
		{
			get
			{
				return GetItem("GlowNeoDefaultFormFlowUrlRegistryItem", delegate
				{
					var item = new StringRegistryItem(
						"GlowNeoDefaultFormFlowUrlRegistryItem",
						Categories.GLOW_Neo,
						(NoResString)"Neo Default Form-Flow URL",
						(NoResString)"The Neo Default Form-Flow URL, relative to the Glow Portal Root URL",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"NEO/Desktop#/formFlow/default");
					return item;
				});
			}
		}

		public static string GetNeoDefaultFormFlowUrl(BusinessObject bizo)
		{
			var portalsUri = Instance.GlowPortalsUri.Value.TrimEnd('/');
			var formFlowUri = Instance.NeoDefaultFormFlowUrlRegistryItem.Value.Trim('/');
			var glowInterface = GlowDataDefinitionReference.FromType(bizo.GetType()).DataDefinitionName;

			return FormattableString.Invariant($"{portalsUri}/{formFlowUri}/{glowInterface}/{bizo.PK}");
		}

		public BooleanRegistryItem NeoEnableHyperlinksInDocumentMacros
		{
			get
			{
				return GetItem("GlowNeoEnableHyperlinksInDocumentMacros", delegate
				{
					return new BooleanRegistryItem(
						"GlowNeoEnableHyperlinksInDocumentMacros",
						Categories.GLOW_Neo,
						ResString.GetMultilingualString("303b41e1-f56b-4fce-91a7-7f7146b48283", "Enable Neo Hyperlinks in Document and Report Macros"),
						ResString.GetMultilingualString("62eec56d-7dd7-439d-a008-2deefa402514", "This allows documents and reports with the {0} macro to generate Neo hyperlinks instead of WebTracker.", "GetTrackingUrl"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem GlowEnableResourceStringsFullAccess
		{
			get
			{
				return GetItem("GlowEnableResourceStringsFullAccess", delegate
				{
					return new BooleanRegistryItem(
						"GlowEnableResourceStringsFullAccess",
						Categories.GLOW,
						(NoResString)"Enable Resource Strings Full Access",
						(NoResString)"Allows access to write translation feedback for English resource strings.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableSecurityGroupsForContactsInGLOW
		{
			get
			{
				return GetItem("EnableSecurityGroupsForContactsInGLOW", delegate
				{
					return new BooleanRegistryItem(
						"EnableSecurityGroupsForContactsInGLOW",
						Categories.GLOW,
						(NoResString)"Enable Security Groups for Contacts in GLOW",
						(NoResString)@"When set to YES, then GLOW rights discovery will be via the new contact groups, managed via the GLOW SSM portal. Management of GLOW security rights will no longer be possible via the Organization Contact record in CargoWiseOne.
When set to NO, then GLOW rights discovery will be via legacy rights discovery method, managed via the Organization Contact record in CargoWiseOne."
,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public HyperlinkListRegistryItem EndUserPolicyHyperlinks
		{
			get
			{
				return GetItem("EndUserPolicyHyperlinks", delegate
				{
					return new HyperlinkListRegistryItem(
						"EndUserPolicyHyperlinks",
						Categories.GLOW,
						ResString.GetMultilingualString("7cdfc74b-842e-4335-bc96-9614c78ae1e7", "End User Policy Hyperlinks"),
						ResString.GetMultilingualString("2951e56f-be5f-4eec-8360-cbe694cd8084", "Add hyperlinks to any relevant policies that affect end user access to GLOW based web portals. Typical policies include Terms of Use, Privacy Notice, Legal Information, etc.  Configured hyperlinks will be displayed to end users in key locations such as the cookie acceptance form, login pages, and the App menu."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue
					);
				});
			}
		}

		public StringRegistryItem GlowExternalUserPortalsUri
		{
			get
			{
				return GetItem("GlowExternalUserPortalsUri", delegate
				{
					var item = new StringRegistryItem(
						"GlowExternalUserPortalsUri",
						Categories.GLOW,
						ResString.GetMultilingualString("AF8AB6C5-CFC9-4242-932A-CFF2BE520214", "External User GLOW Portals Root URL"),
						ResString.GetMultilingualString("D2A1B50A-8712-48A4-96F1-C5CFC22DCA2B", "In installations where external contact users have a different URL to internal staff users, this URL describes the external user facing URL. This approach may be used to distinguish internet activity from external users, from intranet traffic from internal users, for security reasons. When configured, this URL will also be used for the password reset link sent to Organization Contacts from the CW1 \"Send Password Set/Reset Email\" for CW1 Web Portal users."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue);
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public IntRegistryItem IndexSearchabilityPeriodDefault
		{
			get
			{
				return GetItem("IndexSearchabilityPeriodDefault", delegate
				{
					var item = new IntRegistryItem(
						"IndexSearchabilityPeriodDefault",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("362C1843-2A3E-4AFC-819E-AAD9791BC3E4", "Indexing Service Searchability Period Default"),
						ResString.GetMultilingualString("DBF22CED-52DB-41B8-AC61-4224316C47B6", "Age of records to be included in the index. 1 to 36 months."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						36, 1, 36);
					return item;
				});
			}
		}

		public IndexDurationRegistryItem IndexSearchabilityPeriodOverrides
		{
			get
			{
				return GetItem("IndexSearchabilityPeriodOverrides", delegate
				{
					var item = new IndexDurationRegistryItem(
						"IndexSearchabilityPeriodOverrides",
						Categories.GLOW_Services,
						ResString.GetMultilingualString("508BD995-ED06-43B6-A7D1-C7EEF9F1FC33", "Indexing Service Searchability Period Overrides"),
						ResString.GetMultilingualString("D866C1D5-ED2D-477F-8422-DABF171C7A8F", "Age of records to be included in the index. 0 means include all records regardless of age."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyEditableBySupportIfHosted,
						[
							new IndexDuration() { Table = HVLVConsignmentSchema.Constants.TableName, DurationInMonths = 3 },
							new IndexDuration() { Table = ProcessHeaderSchema.Constants.TableName, DurationInMonths = 1 },
							new IndexDuration() { Table = ProcessTasksSchema.Constants.TableName, DurationInMonths = 3 },
						]);
					return item;
				});
			}
		}

		ICodeDescriptionPairListProvider GlowContentSecurityPolicyModeListProvider
		{
			get
			{
				if (glowContentSecurityPolicyModeListProvider == null)
				{
					glowContentSecurityPolicyModeListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var modeList = new CodeDescriptionPairList();
						modeList.AddPair(nameof(ContentSecurityPolicyMode.Disabled), ResString.GetMultilingualString("644D8A32-E0F1-483A-891A-660AB2BF45ED", "Disabled"));
						modeList.AddPair(nameof(ContentSecurityPolicyMode.ReportOnly), ResString.GetMultilingualString("FA9A6785-FCA3-4449-85D9-32E05DAC0B26", "Report Only"));
						modeList.AddPair(nameof(ContentSecurityPolicyMode.EnforceAndReport), ResString.GetMultilingualString("C5E277EB-8821-44D2-BB52-3DA13F470428", "Enforce And Report"));
						modeList.AddPair(nameof(ContentSecurityPolicyMode.EnforceOnly), ResString.GetMultilingualString("F56EFA67-0C97-4F0F-9A12-178FE12DCC5E", "Enforce Only"));
						modeList.DefaultCode = SharedGlowRegistry.ContentSecurityPolicyModeDefaultMode.ToString();
						return modeList;
					});
				}
				return glowContentSecurityPolicyModeListProvider;
			}
		}
		ICodeDescriptionPairListProvider glowContentSecurityPolicyModeListProvider;

		public CodePairRegistryItem GlowContentSecurityPolicyMode
		{
			get
			{
				return GetItem(SharedGlowRegistry.ContentSecurityPolicyModeKey, delegate
				{
					return new CodePairRegistryItem(
						SharedGlowRegistry.ContentSecurityPolicyModeKey,
						Categories.GLOW,
						ResString.GetMultilingualString("A9DB7778-3594-40DC-982A-F71D847B214D", "GLOW Content Security Policy Mode"),
						ResString.GetMultilingualString("660D60F3-B4A9-43FD-A737-FCF9758445C7", @"This setting controls the Content Security Policy (CSP) for GLOW.
 
CSP is an added layer of security that helps to detect and mitigate Cross-Site Scripting (XSS), click-jacking and other code injection attacks that result from the execution of malicious content in the trusted web page context. These attacks can form the basis for data theft, site defacement, and malware distribution. When a resource is requested by a web page - such as a script, image, CSS etc - the browser will block it from loading if it violates the CSP.
 
The following modes are available to control the behavior of the browser when it attempts to load a resource that violates the CSP:
 
Disabled - CSP will be disabled
 
Report Only - any CSP violation will be reported; the resource will be loaded
 
Enforce and Report - any CSP violation be reported; the resource will not be loaded
 
Enforce Only - any CSP violation will prevent the resource from being loaded without reporting it"),
						GlowContentSecurityPolicyModeListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						SharedGlowRegistry.ContentSecurityPolicyModeDefaultMode.ToString());
				});
			}
		}

		public BooleanRegistryItem NeoGtmEnabled
		{
			get
			{
				return GetItem("NeoGtmEnabled", () =>
				{
					return new BooleanRegistryItem(
						"NeoGtmEnabled",
						Categories.GLOW_Neo,
						(NoResString)"Enable Neo GTM",
						(NoResString)"Set to Yes to enable the Neo GTM link in the Neo portal.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false
					);
				});
			}
		}

		public StringRegistryItem NeoGtmEndpointURL
		{
			get
			{
				return GetItem("NeoGtmEndpointURL", delegate
				{
					var item = new StringRegistryItem(
						"NeoGtmEndpointURL",
						Categories.GLOW_Neo,
						(NoResString)"Neo GTM Endpoint URL",
						(NoResString)"The URL of Neo GTM endpoint",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
					item.DataType = new UriRegistryDataType(Uri.UriSchemeHttps) { AllowAutoProtocolPrefixing = false };
					return item;
				});
			}
		}

		public BooleanRegistryItem GlowEnableTaskIndexing
		{
			get
			{
				return GetItem("GlowEnableTaskIndexing", delegate
				{
					var item = new BooleanRegistryItem(
						"GlowEnableTaskIndexing",
						Categories.GLOW,
						ResString.GetMultilingualString("c1af36ee-0951-4f03-bafe-d5d7c697c9a4", "Enable Task Indexing"),
						ResString.GetMultilingualString("1f997894-7c6f-4927-9b1b-d037d5cf3778", "Enable Task List Index"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
					return item;
				});
			}
		}

		#endregion
	}
}

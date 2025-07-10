using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eHub;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class eHubMessagingRegistry : RegistryItemSet
	{
		#region Singleton Instance

		[ThreadStatic]
		static eHubMessagingRegistry instance;

		public static eHubMessagingRegistry Instance
		{
			get
			{
				return instance ?? (instance = new eHubMessagingRegistry());
			}
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : eServicesRegistry.Categories
		{
			public static MultilingualString eServices_CustomsServiceBureau { get { return CombineCategories(eServices, ResString.GetMultilingualString("1df5702e-69a9-46b3-84a5-4e78021d9d09", "Customs Service Bureau")); } }
			public static MultilingualString eServices_CustomsServiceBureau_AU { get { return CombineCategories(eServices_CustomsServiceBureau, ResString.GetMultilingualString("57798270-5e00-4506-b2c5-1cc54ffc5694", "AU")); } }
			public static MultilingualString eServices_CustomsServiceBureau_US { get { return CombineCategories(eServices_CustomsServiceBureau, ResString.GetMultilingualString("f0445c72-a49a-482b-b031-3d78b4a7542d", "US")); } }
			public static MultilingualString eServices_CustomsServiceBureau_HK { get { return CombineCategories(eServices_CustomsServiceBureau, ResString.GetMultilingualString("24aae886-7db3-493b-9846-e8b4db08f169", "HK")); } }
			public static MultilingualString eServices_CustomsServiceBureau_CA { get { return CombineCategories(eServices_CustomsServiceBureau, ResString.GetMultilingualString("7B3211AA-DA8A-4FCD-BE1C-AC8218053F65", "CA")); } }
			public static MultilingualString eServices_eHub_HealthCheck { get { return CombineCategories(eServices_eHub, HealthCheckConstants.HealthCheck); } }
			public static MultilingualString eServices_eHub_FailedEDIInterchange { get { return CombineCategories(eServices_eHub_HealthCheck, HealthCheckConstants.FailedEDIInterchange); } }
			public static MultilingualString eServices_eHub_FailedEDIInterchange_Hidden { get { return CombineCategories(eServices_eHub_FailedEDIInterchange, HealthCheckConstants.Hidden); } }
		}

		#endregion

		public static class eHubGateway
		{
			public const string ServerName = "ehubgateway.wisegrid.net";
			public const string TestServerName = "ehub-ausyd-test.wisegrid.net";
		}

		#region Purge Settings

		public PurgeSettingsRegistryItem PurgeSettingsItem
		{
			get
			{
				return GetItem("PurgeSettings", () =>
					new PurgeSettingsRegistryItem(
						"PurgeSettings",
						Categories.eServices,
						ResString.GetMultilingualString("5805bd9c-eb50-4afe-a381-5979ea7f19ea", "Message Purge Settings"),
						ResString.GetMultilingualString("5bfae43a-1c50-49ed-b6ae-315f82f0e6c9", "These settings specify what the EPG - Message Purge Service Task will do when it runs. For each application code and its Message Sub Types, you can select them to be purged by the service task and also specify how many months to keep the messages before they are purged."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						GetDefaultPurgeSettings(registryDataType)));
			}
		}

		readonly PurgeSettingsRegistryDataType registryDataType = new PurgeSettingsRegistryDataType();

		public static PurgeSettings GetDefaultPurgeSettings()
		{
			return GetDefaultPurgeSettings(new PurgeSettingsRegistryDataType());
		}

		static PurgeSettings GetDefaultPurgeSettings(PurgeSettingsRegistryDataType registryDataType)
		{
			var result = new PurgeSettings(registryDataType);
			result.ApplicationCodes.AddRange(EDIMessagePurgeSettings.GetAllPurgeSettings());

			return result;
		}

		#endregion

		#region eHub

		#region SuppressResourceStringsCheckRegion

		#region XML Service Verbose Logging

		public BooleanRegistryItem XMLServiceVerboseLogging
		{
			get
			{
				return GetItem("XMLServiceVerboseLogging", delegate
				{
					return new BooleanRegistryItem(
						"XMLServiceVerboseLogging",
						Categories.eServices_eHub,
						(NoResString)"XML Service Verbose Logging",
						(NoResString)"XML Service Verbose Logging.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region eHub Gateway Server Address List

		/// <summary>
		/// URI for EHub Web Service
		/// </summary>
		public eHubGatewayRegistryItem eHubGatewayServerAddressList
		{
			get
			{
				return GetItem("EHubGatewayServerAddress", delegate
				{
					var caption = (NoResString)"eHub Gateway Server Address";
					var hint = (NoResString)"This is the address of the gateway server. If this value is changed, all eHub passwords will automatically be reset which will cause the eHub service tasks to re-register with eHub.";
					var result = new eHubGatewayRegistryItem(
						"EHubGatewayServerAddress",
						Categories.eServices_eHub,
						caption,
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						eHubGateway.ServerName,
						isProduction: true);
					result.OnUpdateAction = OnUpdateForEHubGatewayServerAddress;
					return result;
				});
			}
		}

		public eHubGatewayRegistryItem eHubTestGatewayServerAddressList
		{
			get
			{
				return GetItem("EHubTestGatewayServerAddress", delegate
				{
					var caption = (NoResString)"eHub Test Gateway Server Address";
					var hint = (NoResString)"This is the address of the gateway server for performing system testing using non-production endpoints.";
					var result = new eHubGatewayRegistryItem(
						"EHubTestGatewayServerAddress",
						Categories.eServices_eHub,
						caption,
						hint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						eHubGateway.TestServerName,
						isProduction: false);
					result.OnUpdateAction = OnUpdateForEHubGatewayServerAddress;
					return result;
				});
			}
		}

		public BooleanRegistryItem eHubSendInterchangesToTestGateway
		{
			get
			{
				return GetItem("EHubSendInterchangesToTestGateway", delegate
				{
					var result = new BooleanRegistryItem(
						"EHubSendInterchangesToTestGateway",
						Categories.eServices_eHub,
						ResString.GetMultilingualString("541336C4-0CDA-4F53-AEE8-544F8527E50F", "Send Interchanges To The eHub Test Gateway"),
						ResString.GetMultilingualString("68D2D270-2740-49EA-8126-BD6BF50D4FDE", "On non-production systems, a value of true for this setting will result in interchanges being sent to the eHub test gateway instead of the production gateway."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalUATSystem() || ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalDeveloperSystem());
					result.OnUpdateAction = OnUpdateForEHubGatewayServerAddress;
					return result;
				});
			}
		}

		public BooleanRegistryItem eHubEnableReceiveFromProductionGateway
		{
			get
			{
				return GetItem("EHubEnableReceiveFromProductionGateway", delegate
				{
					return new BooleanRegistryItem(
						"EHubEnableReceiveFromProductionGateway",
						eServicesRegistry.Categories.eServices_eHub,
						(NoResString)"Enable Receive From The eHub Production Gateway",
						(NoResString)"A value of true for this setting will enable the EHI service task, which is responsible for retrieving messages from the eHub production gateway.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						!ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalUATSystem() && !ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalDeveloperSystem());
				});
			}
		}

		void OnUpdateForEHubGatewayServerAddress(Guid companyPK, Guid branchPK, Guid departmentPK, object registryitem)
		{
			CreateInterchangeToSendEHINudgeURLToEHub(EHINudgeURL.Value); // send the EHI Nudge URL to the eHub which serves at the new EHubGatewayServerAddress
		}

		#endregion

		#region ScavengingTaskSettings

		/// <summary>
		/// ScavengingTaskSettings
		/// </summary>
		public ScavengingSettingCollectionRegistryItem ScavengingTaskSettings
		{
			get
			{
				return GetItem("ScavengingTaskSettings", delegate
				{
					var result = new ScavengingSettingCollectionRegistryItem(
						"ScavengingTaskSettings",
						Categories.eServices_eHub,
						(NoResString)"Scavenging Task Settings",
						(NoResString)"Scavenging Task Settings",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue);
					result.IsExcludedFromCwOnlyNonCachedTest = true;
					return result;
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Customs Service Bureau

		#region AU
		#region SuppressResourceStringsCheckRegion
		#region Suppress re-sends

		public BooleanRegistryItem AUSuppressResends
		{
			get
			{
				return GetItem("AUSuppressResends", delegate
				{
					return new BooleanRegistryItem(
						"AUSuppressResends",
						Categories.eServices_CustomsServiceBureau_AU,
						ResString.GetMultilingualString("474D8DE2-BDA8-4775-81EF-C18E092A8DE3", "Suppress Resends"),
						ResString.GetMultilingualString("8F7305FB-C9DE-456E-B18B-2C9FE13D8026", "Select 'No' if you want the normal re-sending of interchanges to be reinstated when AU Customs Messaging via eHub is enabled"),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Suppress CONTRL Acknowledgements

		public BooleanRegistryItem AUSuppressCONTRLAcknowledgements
		{
			get
			{
				return GetItem("AUSuppressCONTRLAcknowledgements", delegate
				{
					return new BooleanRegistryItem(
						"AUSuppressCONTRLAcknowledgements",
						Categories.eServices_CustomsServiceBureau_AU,
						ResString.GetMultilingualString("DEB8321E-7E08-4629-BFC8-3979E92AD7C2", "Suppress sending CONTRL Acknowledgements"),
						ResString.GetMultilingualString("2B3D92D0-6C89-43A1-BC61-BC20B869C89A", "Select 'Yes' if you want to suppress the sending of the CONTRL Acknowledgement message in response to a Customs Messaging received via eHub.\r\nNote that the use of this feature requires that Customs set not to expect CONTRL messages for your user site.\r\nDo not set this to 'Yes' if Customs have not made the necessary adjustment to your user site."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#endregion
		#endregion

		#region CA
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem SendCAViaEHub
		{
			get
			{
				return GetItem("SendCAViaEHub", delegate
				{
					return new BooleanRegistryItem(
						"SendCAViaEHub",
						Categories.eServices_CustomsServiceBureau_CA,
						(NoResString)"Send Via eHub",
						(NoResString)"Select 'Yes' if you want to enable CA Customs Messaging via eHub",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						true);// Note, if this default is changed to true then CA EHubInterchangeProcessorServiceTask will load at all sites, as do other CA service task
				});
			}
		}

		#endregion
		#endregion

		#region HK

		public BooleanRegistryItem SendHKISACEViaEHub
		{
			get
			{
				return GetItem("SendHKISACEViaEHub", delegate
				{
					return new BooleanRegistryItem(
						"SendHKISACEViaEHub",
						Categories.eServices_CustomsServiceBureau_HK,
						(NoResString)"Send ISAC Via eHub",
						(NoResString)"Select 'Yes' if you want to enable HK Customs Messaging via eHub",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region InterfaceConnector / NativeXMLConnector

		public bool HasInterfaceConnector
		{
			get
			{
				var enabledUntil = InterfaceConnectorTemporarilyEnabledUntilItem.Value.EnabledUntil;
				return !enabledUntil.IsEmpty && enabledUntil >= ZDateTime.Now.Date;
			}
		}

		public InterfaceConnectorTemporarilyEnabledUntilRegistryItem InterfaceConnectorTemporarilyEnabledUntilItem
		{
			get
			{
				return GetItem("InterfaceConnectorTemporarilyEnabledUntil", delegate
				{
					return new InterfaceConnectorTemporarilyEnabledUntilRegistryItem(
						"InterfaceConnectorTemporarilyEnabledUntil",
						Categories.eServices,
						(NoResString)"Interface Connector temporarily enabled until",
						(NoResString)"Enter a date to temporarily enable InterfaceConnector until:-",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
				});
			}
		}

		public bool HasNativeXMLConnector
		{
			get { return HasNativeXMLConnectorItem.Value; }
			set { HasNativeXMLConnectorItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public BooleanRegistryItem HasNativeXMLConnectorItem
		{
			get
			{
				return GetItem("HasNativeXMLConnector", delegate
				{
					return new BooleanRegistryItem("HasNativeXMLConnector", Categories.eServices_NativeXML, (NoResString)"NativeXMLConnector",
						(NoResString)"This is to enable NativeXMLConnector.",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region eHub Health Check Jobs

		public GuidRegistryItem eHubFailedEDIInterchangeNotificationGroup
		{
			get
			{
				return GetItem("eHubFailedEDIInterchangeNotificationGroup",
					() => new GuidRegistryItem("eHubFailedEDIInterchangeNotificationGroup",
						Categories.eServices_eHub_FailedEDIInterchange,
						HealthCheckConstants.EmailRecipients, HealthCheckConstants.EmailRecipientsDescription,
						RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public NotificationFrequencyRegistryItem eHubFailedEDIInterchangeNotificationFrequency
		{
			get
			{
				return GetItem("eHubFailedEDIInterchangeNotificationFrequency",
					() => new NotificationFrequencyRegistryItem("eHubFailedEDIInterchangeNotificationFrequency",
						Categories.eServices_eHub_FailedEDIInterchange, HealthCheckConstants.NotificationFrequency,
						HealthCheckConstants.NotificationFrequencyDescription,
						RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, new NotificationFrequency())
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public DateTimeRegistryItem eHubLastCheckedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("eHubLastCheckedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("eHubLastCheckedTimeForFailedEDIInterchange",
						Categories.eServices_eHub_FailedEDIInterchange_Hidden,
						(NoResString)"LastCheckedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last checked time point for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public DateTimeRegistryItem eHubLastReportedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("eHubLastReportedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("eHubLastReportedTimeForFailedEDIInterchange",
						Categories.eServices_eHub_FailedEDIInterchange_Hidden,
						(NoResString)"LastReportedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last reported time point of Periodically Setting for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public StringRegistryItem eHubCountFailedEDIInterchangesInPeriodically
		{
			get
			{
				return GetItem("eHubCountFailedEDIInterchangesInPeriodically",
					() => new StringRegistryItem("eHubCountFailedEDIInterchangesInPeriodically",
						Categories.eServices_eHub_FailedEDIInterchange_Hidden,
						(NoResString)"CountFailedEDIInterchangesInPeriodically",
						(NoResString)"Readonly. Count failed EDIInterchanges in Periodically",
						RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.NotCached)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		#endregion

		#region eHubOutboundSendLimitsRule

		/// <summary>
		/// eHubOutboundSendLimitsRule
		/// </summary>
		public OutboundSendLimitsRegistryItem eHubOutboundSendLimitsRule
		{
			get
			{
				return GetItem("eHubOutboundSendLimitsRule", delegate
				{
					var result = new OutboundSendLimitsRegistryItem(
						"eHubOutboundSendLimitsRule",
						Categories.eServices_eHub,
						(NoResString)"Outbound Send Limits",
						(NoResString)"These values determine the amount of data sent during each call to the web service.\n\nCount Boundary: Limit that cannot be exceeded.\nSize Threshold: Limit that can be exceeded to accommodate the sending of at least one EDI Interchange.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		#region Pending Items

		public IntRegistryItem eHubOutboundPendingItemsBatchSize
		{
			get
			{
				return GetItem("eHubOutboundPendingItemsBatchSize", delegate
				{
					var result = new IntRegistryItem(
						"eHubOutboundPendingItemsBatchSize",
						Categories.eServices_eHub,
						(NoResString)"Outbound Pending Items Batch Size",
						(NoResString)"The number of pending items that are retrieved from the database at a time per company and recipient to be sent.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						defaultValue: 1000,
						minValue: 1,
						maxValue: int.MaxValue);
					return result;
				});
			}
		}

		public IntRegistryItem eHubOutboundPendingItemsSearchLimit
		{
			get
			{
				return GetItem("eHubOutboundPendingItemsSearchLimit", delegate
				{
					var result = new IntRegistryItem(
						"eHubOutboundPendingItemsSearchLimit",
						Categories.eServices_eHub,
						(NoResString)"Outbound Pending Items Search Limit",
						(NoResString)"The maximum number of pending items that we will search through in order to establish the list of branch-recipient pair groups that we need to send interchanges for in this run.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						defaultValue: 100000,
						minValue: 1,
						maxValue: int.MaxValue);
					return result;
				});
			}
		}

		#endregion

		#region EHINudgeURL

		/// <summary>
		/// EHINudgeURL
		/// Validation of this registry item needs to be added before making it editable to "non developer"
		/// </summary>
		public StringRegistryItem EHINudgeURL
		{
			get
			{
				return GetItem("EHINudgeURL", delegate
				{
					var result = new StringRegistryItem(
						"EHINudgeURL",
						Categories.eServices_eHub,
						ResString.GetMultilingualString("{388758F0-D18B-43DF-93EC-5A6379431AFA}", "EHI Nudge URL"),
						ResString.GetMultilingualString("{32C0F27D-3597-4512-A138-7631B9B6E67E}", @"This is the URL which is used to nudge the service task EHI for this current system. Format: {0}", "http(s)://sample.url/Services/NudgeServiceTask"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
					result.OnUpdateAction = OnUpdateForEHINudgeURL;
					return result;
				});
			}
		}

		void OnUpdateForEHINudgeURL(Guid companyPK, Guid branchPK, Guid departmentPK, object registryitem)
		{
			var url = (string)registryitem;
			CreateInterchangeToSendEHINudgeURLToEHub(url);
		}

		void CreateInterchangeToSendEHINudgeURLToEHub(string nudgeURL)
		{
			IEDIInterchange interchange = Factory.New<IEDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.eHub;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.EHubRegistryUpdate;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_From = ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier;
			interchange.EI_To = "eHub";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_BodyText = string.Format(CultureInfo.InvariantCulture, (NoResString)@"<eHubRegistryUpdate xmlns=""{0}""><EHINudgeURL>{1}</EHINudgeURL></eHubRegistryUpdate>", EDIInterchangeTypeList.Descriptions.EHubRegistryUpdate, nudgeURL);
			Factory.Save();
		}

		#endregion

		#region OutageStartTime

		public DateTimeRegistryItem EHIOutageStartTime
		{
			get
			{
				return GetItem("EHIOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"EHIOutageStartTime",
						Categories.eServices_eHub,
						(NoResString)"EHI Service Task Outage Start Time",
						(NoResString)"The time the current outage for the EHI service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem EHOOutageStartTime
		{
			get
			{
				return GetItem("EHOOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"EHOOutageStartTime",
						Categories.eServices_eHub,
						(NoResString)"EHO Service Task Outage Start Time",
						(NoResString)"The time the current outage for the EHO service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public IntRegistryItem eHubOutageExpiryTimeInMinutes
		{
			get
			{
				return GetItem("eHubOutageExpiryTimeInMinutes", delegate
				{
					var result = new IntRegistryItem(
						"eHubOutageExpiryTimeInMinutes",
						Categories.eServices_eHub,
						(NoResString)"eHub Outage Expiry Time in Minutes",
						(NoResString)"The number of minutes until we will start reporting errors if the connection to the eHub Gateway remains offline",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						30);
					return result;
				});
			}
		}

		#endregion

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory { NameForDebugging = "eHubMessagingRegistry" };
				}

				return factory;
			}
		}

		BusinessObjectFactory factory;
	}
}

using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.eServices
{
	public sealed class eAdaptorRegistry : RegistryItemSet
	{
		#region Singleton Instance

		[ThreadStatic]
		static eAdaptorRegistry instance;

		public static eAdaptorRegistry Instance
		{
			get
			{
				return instance ?? (instance = new eAdaptorRegistry());
			}
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : eServicesRegistry.Categories
		{
			public static MultilingualString eServices_UniversalXML_AutomaticUpdateonImport { get { return CombineCategories(eServices_UniversalXML, ResString.GetMultilingualString("9d1a09a6-a994-41b5-b445-f9ba9d364a8b", "Automatic Update on Import")); } }
			public static MultilingualString eServices_eAdaptor_Inbound { get { return CombineCategories(eServices_eAdaptor, ResString.GetMultilingualString("2F986BB9-6A6B-419F-B8C6-DD2A1CC5F91A", "Inbound")); } }
			public static MultilingualString eServices_eAdaptor_Outbound { get { return CombineCategories(eServices_eAdaptor, ResString.GetMultilingualString("910AFD2F-6F16-465C-9AD7-287C3FB8506D", "Outbound")); } }
			public static MultilingualString eServices_eAdaptor_HealthCheck { get { return CombineCategories(eServices_eAdaptor, HealthCheckConstants.HealthCheck); } }
			public static MultilingualString eServices_eAdaptor_FailedEDIInterchange { get { return CombineCategories(eServices_eAdaptor_HealthCheck, HealthCheckConstants.FailedEDIInterchange); } }
			public static MultilingualString eServices_eAdaptor_FailedEDIInterchange_Hidden { get { return CombineCategories(eServices_eAdaptor_FailedEDIInterchange, HealthCheckConstants.Hidden); } }
		}

		#endregion

		#region CanSendCargoImpMessagesThroughEAdaptor

		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public BooleanRegistryItem CanSendCargoImpMessagesThroughEAdaptor
		{
			get
			{
				return GetItem("CanSendCargoImpMessagesThroughEAdapter", delegate
				{
					return new BooleanRegistryItem(
						"CanSendCargoImpMessagesThroughEAdapter",
						Categories.eServices_eAdaptor_Outbound,
						(NoResString)"User can send FHL or FWB messages through eAdaptor.",
						(NoResString)"Setting this registry item to 'Yes' will allow current system send FHL or FWB messages through eAdaptor.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						CanSendCargoImpMessagesThroughEAdaptorDefaultValue(ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode));
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		static bool CanSendCargoImpMessagesThroughEAdaptorDefaultValue(string licenceEnterpriseCode)
		{
			return licenceEnterpriseCode == "GEO";
		}

		#endregion

		#endregion

		#region Universal XML Items

		public BooleanRegistryItem UniversalXMLUseCombinedReferenceAndPartyIDMatch
		{
			get
			{
				return GetItem("UniversalXMLUseCombinedReferenceAndPartyIDMatch", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUseCombinedReferenceAndPartyIDMatch",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("9fda5bf6-e33a-40df-ba0e-9dedcb68d020", "Use Combined Reference And Party ID Matching"),
						ResString.GetMultilingualString("4956d343-aa97-4a3e-a297-882c2bdadacb", "Setting this registry item to 'Yes' will turn on Combined Reference and Party ID matching in Universal XML. PLEASE NOTE: Multiple Reference matching will be disabled for modules where this functionality is present."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						CanSendCargoImpMessagesThroughEAdaptorDefaultValue(ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode));
				});
			}
		}

		public BooleanRegistryItem UniversalXMLInactiveDepartmentFailsMessage
		{
			get
			{
				return GetItem("UniversalXMLInactiveDepartmentFailsMessage", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLInactiveDepartmentFailsMessage",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("B01A3B8D-C865-4322-844D-D89E46F45070", "Use of Inactive Department Causes Message Rejection"),
						ResString.GetMultilingualString("E78CD907-BBD6-47AC-89B8-05D24D7BEC11", "When enabled, the message will be rejected if the department specified in XML message is inactive."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLEnableVerboseLogging
		{
			get
			{
				return GetItem("UniversalXMLEnableVerboseLogging", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLEnableVerboseLogging",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("16de7b34-4e17-4da8-b145-630669f7d69c", "Enable Verbose Logging"),
						ResString.GetMultilingualString("78046e79-3610-4c08-ae7e-15f10bc3621c", "Turns on verbose logging on Universal XML Imports to show additional information about matching and more specific information about which rows were updated."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLTimestampsInProcessingLogs
		{
			get
			{
				return GetItem("UniversalXMLTimestampsInProcessingLogs", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLTimestampsInProcessingLogs",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("0F88A90C-15B3-44F1-9971-9D96729D0796", "Enable Timestamps in Processing Logs"),
						ResString.GetMultilingualString("F87CD0E6-7BD2-4805-B3CE-8516FA35A234", "Message processing logs will be prefixed with timestamps. This is visible in the EDI Message Notes tab and the response message from the eAdaptor Inbound Web Service."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UseBrokerageDataFirstWhenExportUniversalXML
		{
			get
			{
				return GetItem("UseBrokerageDataFirstWhenExportUniversalXML", delegate
				{
					return new BooleanRegistryItem(
						"UseBrokerageDataFirstWhenExportUniversalXML",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("8A851597-D2C7-42A4-907C-2EFFB4E6739E", "Use Brokerage Data First"),
						ResString.GetMultilingualString("609E6C94-F788-471D-B567-B1B41FB9A63E", "This registry governs which data is considered to be the most accurate and therefore which data gets placed in the exported Universal Shipment XML. If this registry item is set to 'yes' then most data from the brokerage tab (customs data) will take precedence. If it is set to 'no' then most shipment data will take precedence. This does not apply to packages."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UseDefaultingOfDataWhenImportingUniversalXML
		{
			get
			{
				return GetItem("UseDefaultingOfDataWhenImportingUniversalXML", delegate
				{
					return new BooleanRegistryItem(
						"UseDefaultingOfDataWhenImportingUniversalXML",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("9703770D-9F09-48C2-B0D3-C03EA0BFAC3B", "Enable Data Defaulting"),
						ResString.GetMultilingualString("175DCD2E-D2BB-44A0-A2D9-9662306B7050", "This registry governs whether the system will default data when importing an Universal Shipment XML like manual keying of the data (Customs Only)."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public IntRegistryItem StmQueueStateUpdateBatchSize
		{
			get
			{
				return GetItem("StmQueueStateUpdateBatchSize", delegate
				{
					return new IntRegistryItem(
						name: "StmQueueStateUpdateBatchSize",
						Categories.eServices_UniversalXML,
						(NoResString)"Queue State Update Batch Size",
						(NoResString)"Maximum number of UPDATE commands that can be executed together while updating Queue State rows",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						defaultValue: 500);
				});
			}
		}

		public BooleanRegistryItem AllowParallelUMI
		{
			get
			{
				return GetItem("AllowParallelUMI", delegate
				{
					return new BooleanRegistryItem(
						"AllowParallelUMI",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("551BDB3C-520E-4CF9-A80A-452C468728BB", "Process Inbound Messages in Parallel"),
						ResString.GetMultilingualString("9CF651B8-115F-4AA9-A9F9-8942C7C4F841", "This registry item controls when parallelism is used by the UMI service task. It can be turned on for systems with heavy load in order to increase the throughput of the service task. This functionality places additional load overall on the Service Task infrastructure. It should only be enabled where there are regular backlogs in processing, and there is available server capacity. This registry item will be removed upon final release of this functionality as it will be made to automatically engage itself when a backlog presents."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public IntRegistryItem RetryAttemptsOnUniversalXMLProcessingRecoverableErrors
		{
			get
			{
				return GetItem("RetryAttemptsOnUniversalXMLProcessingRecoverableErrors", delegate
				{
					return new IntRegistryItem(
						name: "RetryAttemptsOnUniversalXMLProcessingRecoverableErrors",
						category: Categories.eServices_UniversalXML,
						caption: ResString.GetMultilingualString("203368d6-7bd9-4f36-be80-1b1e2c50fd8c", "Retry Attempts On Universal XML Processing Recoverable Errors"),
						hint: ResString.GetMultilingualString("4d97e926-5bd3-44bc-addd-ecb541c02ab5", "The number of retry attempts Universal XML Processing will make in case of an error."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: 10,
						minValue: 10,
						maxValue: 10000);
				});
			}
		}

		public InterchangeSenderProxyUsersRegistryItem InterchangeSenderProxyUsers
		{
			get
			{
				return GetItem("InterchangeSenderProxyUsers", delegate
				{
					return new InterchangeSenderProxyUsersRegistryItem(
						"InterchangeSenderProxyUsers",
						Categories.eServices,
						ResString.GetMultilingualString("65909d47-e035-43ce-b571-18d10e83a095", "Interchange Sender Proxy Users"),
						ResString.GetMultilingualString("58d5681f-2c54-492d-b2c7-37df965d6e21", "This setting maps EDI Interchange sender code to a corresponding Staff Proxy user. When an EDI message is processed by UMI/NMI service task, the system checks the sender code and assigns the associated Staff code as the processing user. If no match is found, the default system user is used instead."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						GetDefaultInterchangeSenderProxyUsers()
					);
				});
			}
		}

		InterchangeSenderProxyUserCollection GetDefaultInterchangeSenderProxyUsers()
		{
			var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = "InterchangeSenderProxyFactory", RefreshEnabled = false };
			factory.SuspendValidation();
			return new InterchangeSenderProxyUserCollection(factory);
		}

		public BooleanRegistryItem UniversalXMLExtendedTransactionProtectionEnabled
		{
			get
			{
				return GetItem("UniversalXMLExtendedTransactionProtectionEnabled", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLExtendedTransactionProtectionEnabled",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("39996E91-9819-418C-9C54-423F326B5E1B", "Enable Extended Transaction Protection"),
						ResString.GetMultilingualString("FE5DA479-293D-4494-BCD3-860697BEBED2", "When enabled, all updates to the database for a Universal XML import will be wrapped in the one transaction."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public BooleanRegistryItem FileNameAttachToNotesEnabled
		{
			get
			{
				return GetItem("FileNameAttachToNotesEnabled", delegate
				{
					return new BooleanRegistryItem(
						"FileNameAttachToNotesEnabled",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("169b32ac-55d2-419a-b3e0-9b9bb33ca900", "Enable File Name Attach To Notes"),
						ResString.GetMultilingualString("a5b2bd6f-27a3-49c5-ad95-f661d28098b5", "When enabled, file name element in request will create a note entry in EDI message."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem SupportDocDataInUniversalShipmentXML =>
			GetItem("SupportDocDataInUniversalShipmentXML",
				() => new BooleanRegistryItem(
					"SupportDocDataInUniversalShipmentXML",
					Categories.eServices_UniversalXML,
					(NoResString)"Support Doc Data In USXML",
					(NoResString)"Do not turn this registry on unless explicitly permitted by WTG CEO.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false
				)
			);

		public BooleanRegistryItem UniversalXMLAlwaysIncludeJobCostingInUniversalShipment
		{
			get
			{
				return GetItem("UniversalXMLAlwaysIncludeJobCostingInUniversalShipment", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLAlwaysIncludeJobCostingInUniversalShipment",
						Categories.eServices_UniversalXML,
						(NoResString)"Always Include JobCosting/ConsolCost in Universal Shipment",
						(NoResString)"By default, the JobCosting and ConsolCosts element is not included when exporting a Universal Shipment unless you are sending using the [ORP - Organization Proxy] Recipient Role. This is to stop you sending your job profit information to third parties. Setting this registry item to 'Yes' will make the JobCosting and ConsolCosts element be included no matter who the recipient is.\r\n\r\nTHIS REGISTRY ITEM IS DEVELOPER ONLY AND WILL BE REMOVED IN THE FUTURE.\r\n\r\nPlease do not turn this on for a client without discussing with Henry Ye or Ben Govett first.",
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public IntRegistryItem MessagesPerBatch
		{
			get
			{
				return GetItem("MessagesPerBatch", delegate
				{
					return new IntRegistryItem(
						"MessagesPerBatch",
						Categories.eServices_UniversalXML,
						(NoResString)"XML Messages Per Batch (UMK/UMQ)",
						(NoResString)"The number of messages in each batch (per save) of XML Message services tasks (UMK and UMQ).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						50);
				});
			}
		}

		public IntRegistryItem MessagesPerExecution
		{
			get
			{
				return GetItem("MessagesPerExecution", delegate
				{
					return new IntRegistryItem(
						"MessagesPerExecution",
						Categories.eServices_UniversalXML,
						(NoResString)"XML Messages Per Execution (UMK/UMQ)",
						(NoResString)"The number of messages in each execution of the XML Messages service tasks (UMK and UMQ) before the service task restarts.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						5000);
				});
			}
		}

		public IntRegistryItem UMIMessagesPerBatch
		{
			get
			{
				return GetItem("UMIMessagesPerBatch", delegate
				{
					return new IntRegistryItem(
						"UMIMessagesPerBatch",
						Categories.eServices_UniversalXML,
						(NoResString)"XML Messages Per Batch (UMI)",
						(NoResString)"The number of messages in each batch (per save) of UMI service task.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						2000);
				});
			}
		}

		public IntRegistryItem UMIMessagesPerExecution
		{
			get
			{
				return GetItem("UMIMessagesPerExecution", delegate
				{
					return new IntRegistryItem(
						"UMIMessagesPerExecution",
						Categories.eServices_UniversalXML,
						(NoResString)"XML Messages Per Execution (UMI)",
						(NoResString)"The number of messages in each execution of the UMI service task before it restarts.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue,
						50000);
				});
			}
		}

		public IntRegistryItem MessageQueueCapacity
		{
			get
			{
				return GetItem("MessageQueueCapacity", delegate
				{
					return new IntRegistryItem(
						"MessageQueueCapacity",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("1B389BCC-CDAE-499C-9E35-BF684D8DF3EB", "XML Message UMQ Queue Max Backlog Size"),
						ResString.GetMultilingualString("9E18BB7D-C50A-4A80-B322-FEB5B63B049A", "The number of messages that can be queued in the {0} table. If you have a large number of UMQ service task runners and message back logs, this can be increased to ensure each UMQ runner has messages to process. This should be increased incrementally to achieve balance between keeping messages queued for UMQ and managing a larger {0} table.", "StmQueueState"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue,
						20000);
				});
			}
		}

		public IntRegistryItem ValidationRuleMacroCacheInSeconds
		{
			get
			{
				return GetItem("ValidationRuleMacroCacheInSeconds", delegate
				{
					return new IntRegistryItem(
						"ValidationRuleMacroCacheInSeconds",
						Categories.eServices_UniversalXML,
						(NoResString)"Validation Rule Macro Cache In Seconds",
						(NoResString)"The number of seconds the universal validation rule macros will be cached in the system before reloading from the database.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						300);
				});
			}
		}

		public IntRegistryItem UMKPreEnqueuerMaxBacklogSize
		{
			get
			{
				return GetItem("UMKPreEnqueuerMaxBacklogSize", delegate
				{
					return new IntRegistryItem(
						"UMKPreEnqueuerMaxBacklogSize",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("EB8E466A-5345-4419-B28E-CA0AD47589BE", "XML Message UMK Key Generation Max Backlog Size"),
						ResString.GetMultilingualString("B60BA199-9B41-4175-BBEF-5BE911A34201", "The maximum number of {0} rows added by the UMK service task before it stops calculating keys. If this capacity is reached and you have a large number of UMQ service task runners, this value can be increased to ensure there are enough messages ready for UMQ to process. This should be increased incrementally to achieve balance between keeping messages queued for UMQ and managing a larger {0} table.", "StmQueueState"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 10000);
				});
			}
		}

		public IntRegistryItem UMKPreEnqueuerIncreasePerformanceLevelDurationInMillis
		{
			get
			{
				return GetItem("UMKPreEnqueuerIncreasePerformanceLevelDurationInMillis", delegate
				{
					return new IntRegistryItem(
						"UMKPreEnqueuerIncreasePerformanceLevelDurationInMillis",
						Categories.eServices_UniversalXML,
						(NoResString)"Performence level increase duration",
						(NoResString)"How long getting a batch should take before we think it is broken.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						defaultValue: (int)TimeSpan.FromSeconds(20).TotalMilliseconds);
				});
			}
		}

		public IntRegistryItem UMKPreEnqueuerResetPerformanceLevelDurationInMillis
		{
			get
			{
				return GetItem("UMKPreEnqueuerResetPerformanceLevelDurationInMillis", delegate
				{
					return new IntRegistryItem(
						"UMKPreEnqueuerResetPerformanceLevelDurationInMillis",
						Categories.eServices_UniversalXML,
						(NoResString)"Performence level increase duration",
						(NoResString)"How long getting a batch ought to take.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						defaultValue: (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
				});
			}
		}

		public IntRegistryItem ParallelUMIQueueHistoryInHours
		{
			get
			{
				return GetItem("ParallelUMIQueueHistoryInHours", delegate
				{
					return new IntRegistryItem(
						"ParallelUMIQueueHistoryInHours",
						Categories.eServices_UniversalXML,
						(NoResString)"Parallel UMI Queue History in Hours",
						(NoResString)"How many hours should StmQueueState rows with the status PRS be persisted.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 0);
				});
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem ExtendedUniversalLogging
		{
			get
			{
				return GetItem("ExtendedUniversalLogging", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem("ExtendedUniversalLogging",
						Categories.eServices_UniversalXML,
						ResString.GetMultilingualString("77ffe78f-d9bb-4486-99c3-306a9cd0d02c", "Extended Logging Options"),
						ResString.GetMultilingualString("48d381ca-d2f6-4c9c-a454-94d856dcbd90", "Additional log toggles for monitoring in the UMI, UMQ and UMK service tasks."),
						RegistryStorageFlags.System,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("3276ab13-e405-4c60-b473-315977fb04a5", "Enable Logs"), true, true),
						new CodeDescriptionBoolDisallowNewCollection()
						{
							{ ExtendedUniversalLoggingKeys.UMIFirstMessageLoad, ResString.GetMultilingualString("9828e407-5c1b-4af1-9d23-43a5d4655209", "UMI - First message load"), false },
							{ ExtendedUniversalLoggingKeys.UMIMessageAtFront, ResString.GetMultilingualString("fd4da05a-2602-44bf-96c2-a4f522b65ca7", "UMI - Messages at front of queue"), false },
							{ ExtendedUniversalLoggingKeys.UMIChainStatistics, ResString.GetMultilingualString("1f3b69e9-5566-49bb-8819-72e97adb9a91", "UMI - Chain statistics"), false },
							{ ExtendedUniversalLoggingKeys.UMIShowOldest, ResString.GetMultilingualString("ff3654c5-7f2e-4121-b510-1516c4835afe", "UMI - Show oldest message"), false },
							{ ExtendedUniversalLoggingKeys.UMIAllLoads, ResString.GetMultilingualString("8e49e343-8b72-4ef2-9a7f-f1517899cc32", "UMI - All message loads"), false },
							{ ExtendedUniversalLoggingKeys.UMKAllLoads, ResString.GetMultilingualString("572ad440-b0f7-49e5-ba46-c6353c5f531e", "UMK - All message loads"), false },
							{ ExtendedUniversalLoggingKeys.UMKLocksTaken, ResString.GetMultilingualString("c413fa53-d280-4597-86d5-d5e0f1041032", "UMK - Locks taken"), false },
							{ ExtendedUniversalLoggingKeys.UMQAllLoads, ResString.GetMultilingualString("5e7aaef7-5233-495d-aece-278163188057", "UMQ - All message loads"), false },
							{ ExtendedUniversalLoggingKeys.UMQLocksTaken, ResString.GetMultilingualString("b822a82a-368c-43a2-bb46-c6957e0fb59d", "UMQ - Locks taken"), false },
						});
				});
			}
		}

		public static class ExtendedUniversalLoggingKeys
		{
			public const string UMIFirstMessageLoad = "IFM";
			public const string UMIMessageAtFront = "IMF";
			public const string UMIChainStatistics = "ICS";
			public const string UMIShowOldest = "ISO";
			public const string UMIAllLoads = "IAL";
			public const string UMKAllLoads = "KAL";
			public const string UMKLocksTaken = "KLT";
			public const string UMQAllLoads = "QAL";
			public const string UMQLocksTaken = "QLT";
		}

		#endregion

		#endregion

		#region Universal XML Update on Import

		public BooleanRegistryItem UniversalXMLUpdateConsolDuringAutomaticImport
		{
			get
			{
				return GetItem("UniversalXMLUpdateConsolDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUpdateConsolDuringAutomaticImport",
						Categories.eServices_UniversalXML_AutomaticUpdateonImport,
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872F6", "Update Consol During Automatic Import"),
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872F7", "Set this to \"Yes\" to update a consol's details during an automatic Universal XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLUpdateConsolShipmentDuringAutomaticImport
		{
			get
			{
				return GetItem("UniversalXMLUpdateConsolShipmenstDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUpdateConsolShipmenstDuringAutomaticImport",
						Categories.eServices_UniversalXML_AutomaticUpdateonImport,
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872F8", "Update Consol's Shipments During Automatic Import"),
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872F9", "Set this to \"Yes\" to update a consol's shipment's details during an automatic Universal XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLUpdateConsolContainersDuringAutomaticImport
		{
			get
			{
				return GetItem("UniversalXMLUpdateConsolContainersDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUpdateConsolContainersDuringAutomaticImport",
						Categories.eServices_UniversalXML_AutomaticUpdateonImport,
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G1", "Update Consol's Containers During Automatic Import"),
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G2", "Set this to \"Yes\" to update a consol's container's details during an automatic Universal XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLUpdateConsolRoutingDuringAutomaticImport
		{
			get
			{
				return GetItem("UniversalXMLUpdateConsolRoutingDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUpdateConsolRoutingDuringAutomaticImport",
						Categories.eServices_UniversalXML_AutomaticUpdateonImport,
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G3", "Update Consol's Routing Information During Automatic Import"),
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G4", "Set this to \"Yes\" to update a consol's routing information during an automatic Universal XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem UniversalXMLUpdateShipmentDuringAutomaticImport
		{
			get
			{
				return GetItem("UniversalXMLUpdateShipmentDuringAutomaticImport", delegate
				{
					return new BooleanRegistryItem(
						"UniversalXMLUpdateShipmentDuringAutomaticImport",
						Categories.eServices_UniversalXML_AutomaticUpdateonImport,
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G5", "Update Shipment During Automatic Import"),
						ResString.GetMultilingualString("E8D934BB-6ACF-40AA-97EA-16AFCF3872G6", "Set this to \"Yes\" to update a shipment's details during an automatic Universal XML import."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region eAdaptor

		#region Inbound

		#region eAdaptor Inbound Authentications
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public StringRegistryItem eAdaptorInboundAuthentications
		{
			get
			{
				return GetItem("eAdapterInboundAuthentications", delegate
				{
					var result = new StringRegistryItem(
						"eAdapterInboundAuthentications",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Inbound,
						ResString.GetMultilingualString("6cab8c44-3132-4aa4-8b06-d53d2de0afbc", "Basic Authentication"),
						ResString.GetMultilingualString("895c6fce-e8df-4aa0-82b8-8a9f27f1c4a6", "Basic Authentication."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyForClassic_eAdaptor,
						null);

					result.EditorInfo = new AuthenticationRegistryItemEditorInfo(true);
					return result;
				});
			}
		}

		#endregion

		#region eAdaptor Next Inbound OAuth Authority URLs

		public StringArrayRegistryItem eAdaptorInboundOAuthAuthorityUrls
		{
			get
			{
				return GetItem("eAdaptorInboundOAuthAuthorityUrls", delegate
				{
					return new StringArrayRegistryItem(
						"eAdaptorInboundOAuthAuthorityUrls",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Inbound,
						(NoResString)"OAuth Authority URLs",
						(NoResString)"Authority URLs used to validate access tokens to eAdaptor Next web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext | RegistryOptions.PreserveTestValue,
						Array.Empty<string>());
				});
			}
		}

		#endregion

		#region eAdaptor Next Inbound OAuth Client IDs

		public StringArrayRegistryItem eAdaptorInboundOAuthClientIDs
		{
			get
			{
				return GetItem("eAdaptorInboundOAuthClientIDs", delegate
				{
					return new StringArrayRegistryItem(
						"eAdaptorInboundOAuthClientIDs",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Inbound,
						(NoResString)"OAuth Client IDs",
						(NoResString)"Valid access tokens granted to client ids supplied here will be authorized to use eAdaptor Next web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext | RegistryOptions.PreserveTestValue,
						Array.Empty<string>());
				});
			}
		}

		#endregion

		#region Inbound eAdaptor Service Url

		public StringRegistryItem InboundAdaptorServiceUrl
		{
			get
			{
				return GetItem("InboundAdapterServiceUrl", delegate
				{
					return new StringRegistryItem(
						"InboundAdapterServiceUrl",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Inbound,
						ResString.GetMultilingualString("6cassc44-3132-4c54-8b06-d53d2de0axxc", "Service URL"),
						ResString.GetMultilingualString("89ss6fce-e8df-4540-82b8-8a9f27f1cxx6", "Service URL."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						null);
				});
			}
		}

		#endregion

		#region Message User Context Tracing

		public BooleanRegistryItem MessageUserContextTracingEnabled
		{
			get
			{
				return GetItem("MessageUserContextTracingEnabled", delegate
				{
					return new BooleanRegistryItem(
						"MessageUserContextTracingEnabled",
						Categories.eServices_eAdaptor_Inbound,
						ResString.GetMultilingualString("4CEB4CC1-EF90-4C57-A2AB-E7810F58F641", "Unexpected Context Switch Detection Enabled"),
						ResString.GetMultilingualString("BE1E118A-3745-4FDE-8DA3-CC488F08B685", "Enables User Context switch detection during import of Universal XML messages. Unexpected context switches can cause incorrect Workflow templates to be applied. This functionality will also attempt to switch the context back to the appropriate message context during the application of Workflow."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Outbound

		#region Communications Protocol

		public CodePairRegistryItem OutboundCommunicationsProtocol
		{
			get
			{
				return GetItem("OutboundCommunicationsProtocol", delegate
				{
					var lookupListProvider = new eAdaptorOutboundProtocolListProvider();
					return new CodePairRegistryItem(
						"OutboundCommunicationsProtocol",
						Categories.eServices_eAdaptor_Outbound,
						(NoResString)"Communications Protocol",
						(NoResString)"This registry item determines the application layer protocol used for outbound communications via eAdaptor.",
						lookupListProvider,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						eAdaptorOutboundProtocolList.Codes.SOAP);
				});
			}
		}

		#endregion

		public BooleanRegistryItem IgnoreUnknownSSLCertificate
		{
			get
			{
				return GetItem("IgnoreUnknownSSLCertificate", delegate
				{
					return new BooleanRegistryItem(
						"IgnoreUnknownSSLCertificate",
						Categories.eServices_eAdaptor_Outbound,
						(NoResString)"Ignore Unknown Certificate",
						(NoResString)"Ignore unknown and self-signed certificates.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext | RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#region eAdaptor Next Outbound Authentication

		/// <summary>
		/// eAdaptorNextOutbound
		/// </summary>
		public eAdaptorNextOutboundConfigRegistryItem eAdaptorNextOutbound
		{
			get
			{
				return GetItem("eAdaptorNextOutbound", delegate
				{
					return new eAdaptorNextOutboundConfigRegistryItem(
						"eAdaptorNextOutbound",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
						(NoResString)"OAuth Authentications",
						(NoResString)"OAuth Authentications",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext,
						Business.eAdaptorNextOutboundConfig.DefaultValue);
				});
			}
		}

		#endregion

		#region eAdaptor Outbound Password
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public StringRegistryItem eAdaptorOutboundPassword
		{
			get
			{
				return GetItem("eAdapterOutboundPassword", delegate
				{
					var rand = new Random();
					string randomPassword = rand.Next(999999).ToString(CultureInfo.InvariantCulture).PadLeft(6, '0');

					var result = new StringRegistryItem(
						"eAdapterOutboundPassword",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
						ResString.GetMultilingualString("9a85149a-c3f9-4wwb-a271-5e235b992b59", "Password"),
						ResString.GetMultilingualString("ddd63b45-2af7-473c-b68f-9b2f0c106f6a", "Passwords are cached. To cause immediate effect, please restart the Process Controller in Service Task."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsOnlyForClassic_eAdaptor,
						randomPassword);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#endregion

		#region Pending Items

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public IntRegistryItem eAdaptorOutboundPendingItemsBatchSize
		{
			get
			{
				return GetItem("eAdapterOutboundPendingItemsBatchSize", delegate
				{
					var result = new IntRegistryItem(
						"eAdapterOutboundPendingItemsBatchSize",
						Categories.eServices_eAdaptor_Outbound,
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public IntRegistryItem eAdaptorOutboundPendingItemsSearchLimit
		{
			get
			{
				return GetItem("eAdapterOutboundPendingItemsSearchLimit", delegate
				{
					var result = new IntRegistryItem(
						"eAdapterOutboundPendingItemsSearchLimit",
						Categories.eServices_eAdaptor_Outbound,
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

		public IntRegistryItem OutboundTimeout
		{
			get
			{
				return GetItem("OutboundTimeout", delegate
				{
					return new IntRegistryItem(
						"OutboundTimeout",
						Categories.eServices_eAdaptor_Outbound,
						(NoResString)"REST Timeout",
						(NoResString)"REST Timeout duration in seconds.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyFor_eAdaptorNext,
						60);
				});
			}
		}

		#region eAdaptorOutboundSendLimitsRule

		/// <summary>
		/// eAdaptorOutboundSendLimitsRule
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule")]
		public OutboundSendLimitsRegistryItem eAdaptorOutboundSendLimitsRule
		{
			get
			{
				return GetItem("eAdapterOutboundSendLimitsRule", delegate
				{
					var result = new OutboundSendLimitsRegistryItem(
						"eAdapterOutboundSendLimitsRule",
						Categories.eServices_eAdaptor_Outbound,
						ResString.GetMultilingualString("87FAC607-E11F-477B-A8D6-679B13571C3D", "Send Limits"),
						ResString.GetMultilingualString("991EA880-9650-490E-B357-D696A5F92751", "These values determine the amount of data sent during each call to the web service.\r\n\r\nCount Limit: cannot be exceeded per request.\r\nSize Limit: can be exceeded to accommodate the sending of at least one EDI Interchange per request.\r\n\r\nIf the count or size limit is exceeded, EDI Interchanges will be sent in multiple requests."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		#endregion

		#region Outbound eAdaptor Service Url

		public StringRegistryItem OutboundAdapterServiceUrl
		{
			get
			{
				return GetItem("OutboundAdapterServiceUrl", delegate
				{
					return new StringRegistryItem(
						"OutboundAdapterServiceUrl",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
						ResString.GetMultilingualString("6cab8c44-3132-4c54-8b06-d53d2de0axxc", "Service URL"),
						ResString.GetMultilingualString("895c6fce-e8df-4540-82b8-8a9f27f1cxx6", "Service URL."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						null);
				});
			}
		}

		#endregion

		#region Outbound eAdaptor Connectivity Test Url

		public StringRegistryItem OutboundAdaptorConnectivityTestUrl
		{
			get
			{
				return GetItem("OutboundAdaptorConnectivityTestUrl", delegate
				{
					return new StringRegistryItem(
						"OutboundAdaptorConnectivityTestUrl",
						eAdaptorRegistry.Categories.eServices_eAdaptor_Outbound,
						ResString.GetMultilingualString("92160506-CB70-4B2B-8460-40F2A508D329", "Connectivity Testing URL"),
						ResString.GetMultilingualString("7E468344-7CB0-40E3-BA2F-0EB8088E2700", "Used for testing internet connectivity if the Service URL cannot be reached."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://www.wisetechglobal.com/");
				});
			}
		}

		#endregion

		#endregion

		#region eAdaptor Health Check Jobs

		public GuidRegistryItem eAdaptorFailedEDIInterchangeNotificationGroup
		{
			get
			{
				return GetItem("eAdaptorFailedEDIInterchangeNotificationGroup",
					() => new GuidRegistryItem("eAdaptorFailedEDIInterchangeNotificationGroup",
						Categories.eServices_eAdaptor_FailedEDIInterchange,
						HealthCheckConstants.EmailRecipients, HealthCheckConstants.EmailRecipientsDescription,
						RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.PreserveTestValue,
						Core.Constants.Groups.PostMastersGroupPK)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public NotificationFrequencyRegistryItem eAdaptorFailedEDIInterchangeNotificationFrequency
		{
			get
			{
				return GetItem("eAdaptorFailedEDIInterchangeNotificationFrequency",
					() => new NotificationFrequencyRegistryItem("eAdaptorFailedEDIInterchangeNotificationFrequency",
						Categories.eServices_eAdaptor_FailedEDIInterchange, HealthCheckConstants.NotificationFrequency,
						HealthCheckConstants.NotificationFrequencyDescription,
						RegistryStorageFlags.System, RegistryOptions.Default, new NotificationFrequency()
						{
							Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
							TimeInterval = 15
						}));
			}
		}

		public DateTimeRegistryItem eAdaptorLastCheckedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("eAdaptorLastCheckedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("eAdaptorLastCheckedTimeForFailedEDIInterchange",
						Categories.eServices_eAdaptor_FailedEDIInterchange_Hidden,
						(NoResString)"LastCheckedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last checked time point for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public DateTimeRegistryItem eAdaptorLastReportedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("eAdaptorLastReportedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("eAdaptorLastReportedTimeForFailedEDIInterchange",
						Categories.eServices_eAdaptor_FailedEDIInterchange_Hidden,
						(NoResString)"LastReportedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last reported time point of Periodically Setting for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public StringRegistryItem eAdaptorCountFailedEDIInterchangesInPeriodically
		{
			get
			{
				return GetItem("eAdaptorCountFailedEDIInterchangesInPeriodically",
					() => new StringRegistryItem("eAdaptorCountFailedEDIInterchangesInPeriodically",
						Categories.eServices_eAdaptor_FailedEDIInterchange_Hidden,
						(NoResString)"CountFailedEDIInterchangesInPeriodically",
						(NoResString)"Readonly. Count failed EDIInterchanges in Periodically",
						RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.NotCached)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		#endregion
		#endregion

		#region UseDate2012_11NamespaceAndFormat

		public bool UseDate2012_11NamespaceAndFormat
		{
			get
			{
				var kickoffDate = Date2012_11NamespaceAndFormatKicksIn.Value;
				// ===========================================================================================================
				// This will be uncommented and the registry item made non developer only when 2012_11 namespace work is done.
				// ===========================================================================================================
				//if (kickoffDate == DateTime.MinValue) // Equivalent to ZDateTime.IsEmpty
				//{
				//  kickoffDate = GetDate2012_11NamespaceAndFormatKickoffDate();
				//  Date2012_11NamespaceAndFormatKicksIn.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, kickoffDate);
				//}
				return kickoffDate < ZDateTime.Now;
			}
		}

		//DateTime GetDate2012_11NamespaceAndFormatKickoffDate()
		//{
		//  var currentValue = Date2012_11NamespaceAndFormatKicksIn.Value;
		//  if (currentValue == DateTime.MinValue)
		//  {
		//    var lastMessage = GetLastUDMOrNDMTypeEDIMessage();
		//    currentValue = lastMessage == null ? new DateTime(2012, 3, 1) : ZDateTime.Today.AddMonths(6).ToDateTime();
		//  }

		//  return currentValue;
		//}

		//IEDIMessage GetLastUDMOrNDMTypeEDIMessage()
		//{
		//  var applicationCodeFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.NativeDataMessaging);
		//  applicationCodeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);

		//  var query = new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.Today.AddMonths(-6));
		//  query.AddToFilter(applicationCodeFilter);

		//  query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";

		//  return new BusinessObjectFactory().LoadTop1<IEDIMessage>(query);
		//}

		#region SuppressResourceStringsCheckRegion

#if DEBUG
		internal
#endif
		DateTimeRegistryItem Date2012_11NamespaceAndFormatKicksIn
		{
			get
			{
				return GetItem("Date2012_11NamespaceAndFormatKicksIn", delegate
				{
					return new DateTimeRegistryItem(
						"Date2012_11NamespaceAndFormatKicksIn",
						Categories.eServices_UniversalXML,
						(NoResString)"Date 2012/11 Namespace is used in Universal XML.",
						(NoResString)("The current Universal XML uses a new schema under the namespace http://www.cargowise.com/Schemas/Universal/2012/11. This date setting is used to determine when your " + Core.Constants.ProductName + " system will switch across to use this new schema by default for all Universal XML exports."),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers | RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#endregion

		sealed class eAdaptorOutboundProtocolListProvider : ICodeDescriptionPairListProvider
		{
			public CodeDescriptionPairList CodeDescriptionPairList => new eAdaptorOutboundProtocolList();
		}
	}
}

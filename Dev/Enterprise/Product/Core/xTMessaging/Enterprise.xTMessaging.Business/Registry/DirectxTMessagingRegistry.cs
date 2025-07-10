using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.xTMessaging.Business
{
	public sealed class DirectxTMessagingRegistry : RegistryItemSet
	{
		#region Singleton Instance

		[ThreadStatic]
		static DirectxTMessagingRegistry instance;

		public static DirectxTMessagingRegistry Instance
		{
			get { return instance ?? (instance = new DirectxTMessagingRegistry()); }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString eServices_DirectxTMessagingRegistry { get { return CombineCategories(eServices, ResString.GetMultilingualString("59C08FE9-B634-40AD-872C-7BC931B7E1BD", "xT")); } }
			public static MultilingualString eServices_DirectxT_HealthCheck { get { return CombineCategories(eServices_DirectxTMessagingRegistry, HealthCheckConstants.HealthCheck); } }
			public static MultilingualString eServices_DirectxT_FailedEDIInterchange { get { return CombineCategories(eServices_DirectxT_HealthCheck, HealthCheckConstants.FailedEDIInterchange); } }
			public static MultilingualString eServices_DirectxT_FailedEDIInterchange_Hidden { get { return CombineCategories(eServices_DirectxT_FailedEDIInterchange, HealthCheckConstants.Hidden); } }
		}

		public CodePairRegistryItem ConnectionToXTServer
		{
			get
			{
				return GetItem("ConnectionToXTServer", delegate
				{
					var isProdSystem = false;
					var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;

					if (registrationKey != null)
					{
						isProdSystem = registrationKey.DatabaseType == DatabaseTypes.Codes.Production;
					}

					var hint = (NoResString)@"This registry is used for selecting to which xT server CW1 will connect.

By default, this registry value is set to the database instance of the licence type. BE AWARE that if you change it to the xT does not match the licence type, MESSAGES WILL BE SENT TO A DIFFERENT SERVER THAN DEFAULT.

By default, the LCL (Local xT Server) option uses 127.0.0.1:61001 as the connection URL, or configure via 'xT Local Developer Address'. You must also configure the 'xT Local Developer Certificate' registry to use this option.";

					return new CodePairRegistryItem(
						"ConnectionToXTServer",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"Connection To XT Server (Production/Test/Local)",
						hint,
						new CodeDescriptionPairListProvider(() => new ConnectionToXTServerOptions()),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						isProdSystem ? ConnectionToXTServerOptions.XtProduction.Code : ConnectionToXTServerOptions.XtTest.Code);
				});
			}
		}

		public StringRegistryItem XTLocalDeveloperAddress
			=> GetItem("XTLocalDeveloperAddress",
				() =>
					new StringRegistryItem(
						"XTLocalDeveloperAddress",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Local Developer Address",
						(NoResString)@"Your local xT gRPC endpoint to connect to.

By default, 127.0.0.1:61001 is used. To connect to a different address (perhaps if you have multiple local xT instances), you can update this registry.
Note: this can be configured to non-local addresses (eg: some.other.xt.server.sand.wtg.zone), but please be careful!",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"127.0.0.1:61001")
			);

		public StringRegistryItem XTLocalDeveloperCertificate
			=> GetItem("XTLocalDeveloperCertificate",
				() =>
					new StringRegistryItem(
						"XTLocalDeveloperCertificate",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Local Developer Certificate",
						(NoResString)@"Your local xT 'Server Loopback Certificate'.

Copy+Paste the exported cer file into this registry to use the LCL (Local xT Server) option of 'Connection To XT Server' registry.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"")
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					}
			);

		public IntRegistryItem XTServerOutageExpiryTimeInMinutes
		{
			get
			{
				return GetItem("XTServerOutageExpiryTimeInMinutes", delegate
				{
					var result = new IntRegistryItem(
						"XTServerOutageExpiryTimeInMinutes",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Server Outage Expiry Time in Minutes",
						(NoResString)"The number of minutes until we will start reporting errors if the connection to the xT Server remains offline",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						30);

					return result;
				});
			}
		}

		public IntRegistryItem XTServerMessagesMaxReceivingRetryCount
		{
			get
			{
				return GetItem("XTServerMessagesMaxReceivingRetryCount", delegate
				{
					var result = new IntRegistryItem(
						"XTServerMessagesMaxReceivingRetryCount",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Server Messages Max Receiving Retry Count",
						(NoResString)"The number of attempts of message has to be retrieved from the xT server, before being rejected",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						3);

					return result;
				});
			}
		}

		public DateTimeRegistryItem XTIOutageStartTime
		{
			get
			{
				return GetItem("XTIOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"XTIOutageStartTime",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"XTI Service Task Outage Start Time",
						(NoResString)"The time the current outage for the XTI service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem XTOOutageStartTime
		{
			get
			{
				return GetItem("XTOOutageStartTime", delegate
				{
					return new DateTimeRegistryItem(
						"XTOOutageStartTime",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"XTO Service Task Outage Start Time",
						(NoResString)"The time the current outage for the XTO service task started",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						false);
				});
			}
		}

		public IntRegistryItem XTServerMessageTimeoutInSeconds
		{
			get
			{
				return GetItem("XTServerMessageTimeoutInSeconds", delegate
				{
					var result = new IntRegistryItem(
						"XTServerMessageTimeoutInSeconds",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Server Message Timeout in Seconds",
						(NoResString)"The number of seconds until we timeout on Log In and Log Out messages sent to the xT Server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						120);

					return result;
				});
			}
		}

		public IntRegistryItem XTServerMessageChunkSizeWhenSending
		{
			get
			{
				return GetItem("XTServerMessageChunkSizeWhenSending", delegate
				{
					var result = new IntRegistryItem(
						"XTServerMessageChunkSizeWhenSending",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Server Message Chunk Size When Sending",
						(NoResString)"The size (in kilobytes) of the chunk size used when sending message content to xT",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						32);

					return result;
				});
			}
		}

		public IntRegistryItem XTIdleConnectionKeepAliveInSeconds
		{
			get
			{
				return GetItem("XTIdleConnectionKeepAliveInSeconds", delegate
				{
					var result = new IntRegistryItem(
						"XTIdleConnectionKeepAliveInSeconds",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Idle Connection Keep Alive in Seconds",
						(NoResString)"The number of seconds to retain an idle connection to the xT Server for",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						60);

					return result;
				});
			}
		}

		public IntRegistryItem XTIdleConnectionRetryPauseInSeconds
		{
			get
			{
				return GetItem("XTIdleConnectionRetryPauseInSeconds", delegate
				{
					var result = new IntRegistryItem(
						"XTIdleConnectionRetryPauseInSeconds",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Idle Connection Retry Pause In Seconds",
						(NoResString)"The number of seconds to pause between Send or Receive retries on an idle connection to the xT Server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						15);

					return result;
				});
			}
		}

		public BooleanRegistryItem EnableXTIServiceTask
		{
			get
			{
				return GetItem("EnableXTIServiceTask", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableXTIServiceTask",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Enable XTI Service Task",
						(NoResString)"The XTI service task will be enabled or disabled based on this setting.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);

					return result;
				});
			}
		}

		public IntRegistryItem InterchangeCountPerBatchOnSending
		{
			get
			{
				return GetItem("InterchangeCountPerBatchOnSending", delegate
				{
					var result = new IntRegistryItem(
						"InterchangeCountPerBatchOnSending",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"Number of interchanges per batch when sending",
						(NoResString)"Number of interchanges to be processed per batch when sending interchanges to xT Server. Note: increasing beyond 20-30 per batch could result in mass failures.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						15);

					return result;
				});
			}
		}

		public IntRegistryItem InterchangeCountPerBatchOnReceiving
		{
			get
			{
				return GetItem("InterchangeCountPerBatchOnReceiving", delegate
				{
					var result = new IntRegistryItem(
						"InterchangeCountPerBatchOnReceiving",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"Number of interchanges per batch when receiving",
						(NoResString)"Number of interchanges to be processed per batch when receiving interchanges from xT Server.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100);

					return result;
				});
			}
		}

		public IntRegistryItem XTSendingRetryLimit
		{
			get
			{
				return GetItem("XTSendingRetryLimit", delegate
				{
					var result = new IntRegistryItem(
						"XTSendingRetryLimit",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"Interchanges will no longer be attempted to process after this amount of retries.",
						(NoResString)"Interchanges will no longer be attempted to process after this amount of retries.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						5);

					return result;
				});
			}
		}

		public BooleanRegistryItem EnableXTINudge
		{
			get
			{
				return GetItem("EnableXTINudge", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableXTINudge",
						Categories.eServices_DirectxTMessagingRegistry,
						(NoResString)"xT Enable Nudging of XTI Service Task",
						(NoResString)"Nudging of the XTI service task will be enabled or disabled based on this setting.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);

					return result;
				});
			}
		}

		#region xT Health Check Jobs

		public GuidRegistryItem xTFailedEDIInterchangeNotificationGroup
		{
			get
			{
				return GetItem("xTFailedEDIInterchangeNotificationGroup",
					() => new GuidRegistryItem("xTFailedEDIInterchangeNotificationGroup",
						Categories.eServices_DirectxT_FailedEDIInterchange, HealthCheckConstants.EmailRecipients,
						HealthCheckConstants.EmailRecipientsDescriptionXT,
						RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					});
			}
		}

		public NotificationFrequencyRegistryItem xTFailedEDIInterchangeNotificationFrequency
		{
			get
			{
				return GetItem("xTFailedEDIInterchangeNotificationFrequency",
					() => new NotificationFrequencyRegistryItem("xTFailedEDIInterchangeNotificationFrequency",
						Categories.eServices_DirectxT_FailedEDIInterchange, HealthCheckConstants.NotificationFrequency,
						HealthCheckConstants.NotificationFrequencyDescription,
						RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, new NotificationFrequency())
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public DateTimeRegistryItem xTLastCheckedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("xTLastCheckedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("xTLastCheckedTimeForFailedEDIInterchange",
						Categories.eServices_DirectxT_FailedEDIInterchange_Hidden,
						(NoResString)"LastCheckedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last checked time point for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public DateTimeRegistryItem xTLastReportedTimeForFailedEDIInterchange
		{
			get
			{
				return GetItem("xTLastReportedTimeForFailedEDIInterchange",
					() => new DateTimeRegistryItem("xTLastReportedTimeForFailedEDIInterchange",
						Categories.eServices_DirectxT_FailedEDIInterchange_Hidden,
						(NoResString)"LastReportedTimeForFailedEDIInterchange",
						(NoResString)"Readonly. The last reported time point of Periodically Setting for failed EDIInterchanges",
						RegistryStorageFlags.System, RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		public StringRegistryItem xTCountFailedEDIInterchangesInPeriodically
		{
			get
			{
				return GetItem("xTCountFailedEDIInterchangesInPeriodically",
					() => new StringRegistryItem("xTCountFailedEDIInterchangesInPeriodically",
						Categories.eServices_DirectxT_FailedEDIInterchange_Hidden,
						(NoResString)"CountFailedEDIInterchangesInPeriodically",
						(NoResString)"Readonly. Count failed EDIInterchanges in Periodically",
						RegistryStorageFlags.Company, RegistryOptions.IsHidden | RegistryOptions.NotCached)
					{
						IsExcludedFromCwOnlyNonCachedTest = true
					});
			}
		}

		#endregion
	}

	public sealed class ConnectionToXTServerOptions : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string XtProduction = "PRD";
			public const string XtTest = "TST";
			public const string XtLocal = "LCL";
		}

		public static CodeDescriptionPair XtProduction => new CodeDescriptionPair(Codes.XtProduction, (NoResString)"xT Production Server");

		public static CodeDescriptionPair XtTest => new CodeDescriptionPair(Codes.XtTest, (NoResString)"xT Test Server");
		public static CodeDescriptionPair XtLocal => new CodeDescriptionPair(Codes.XtLocal, (NoResString)"Local xT Server");

		public ConnectionToXTServerOptions()
		{
			Add(XtProduction);
			Add(XtTest);
			Add(XtLocal);
		}
	}
}

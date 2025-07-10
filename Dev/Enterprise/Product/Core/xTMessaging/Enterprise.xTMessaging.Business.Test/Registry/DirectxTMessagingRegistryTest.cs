using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.xTMessaging.Business.Test
{
	[TestedType(typeof(DirectxTMessagingRegistry))]
	class DirectxTMessagingRegistryTest : RegistryItemSetTestCaseWithFactory<DirectxTMessagingRegistry>
	{
		public void TestXTServerOutageExpiryTimeInMinutes()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTServerOutageExpiryTimeInMinutes,
					"XTServerOutageExpiryTimeInMinutes",
						"eServices/xT",
						"xT Server Outage Expiry Time in Minutes",
						"The number of minutes until we will start reporting errors if the connection to the xT Server remains offline",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						30);
			});
		}

		public void TestXTIOutageStartTime()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTIOutageStartTime,
					"XTIOutageStartTime",
						"eServices/xT",
						"XTI Service Task Outage Start Time",
						"The time the current outage for the XTI service task started",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue);
			});
		}

		public void TestXTServerMessagesMaxReceivingRetryCount()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTServerMessagesMaxReceivingRetryCount,
					"XTServerMessagesMaxReceivingRetryCount",
					"eServices/xT",
					"xT Server Messages Max Receiving Retry Count",
					"The number of attempts of message has to be retrieved from the xT server, before being rejected",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					3);
			});
		}

		public void TestXTOOutageStartTime()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTOOutageStartTime,
					"XTOOutageStartTime",
						"eServices/xT",
						"XTO Service Task Outage Start Time",
						"The time the current outage for the XTO service task started",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue);
			});
		}

		public void TestXTServerMessageTimeoutInSeconds()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTServerMessageTimeoutInSeconds,
					"XTServerMessageTimeoutInSeconds",
						"eServices/xT",
						"xT Server Message Timeout in Seconds",
						"The number of seconds until we timeout on Log In and Log Out messages sent to the xT Server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						120);
			});
		}

		public void TestXTServerMessageChunkSizeWhenSending()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTServerMessageChunkSizeWhenSending,
					"XTServerMessageChunkSizeWhenSending",
						"eServices/xT",
						"xT Server Message Chunk Size When Sending",
						"The size (in kilobytes) of the chunk size used when sending message content to xT",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						32);
			});
		}

		public void TestXTIdleConnectionKeepAliveInSeconds()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTIdleConnectionKeepAliveInSeconds,
					"XTIdleConnectionKeepAliveInSeconds",
						"eServices/xT",
						"xT Idle Connection Keep Alive in Seconds",
						"The number of seconds to retain an idle connection to the xT Server for",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						60);
			});
		}

		public void TestXTIdleConnectionRetryPauseInSeconds()
		{
			CombineAssertions(() =>
			{
				TestRegistryItem(ItemSet.XTIdleConnectionRetryPauseInSeconds,
					"XTIdleConnectionRetryPauseInSeconds",
						"eServices/xT",
						"xT Idle Connection Retry Pause In Seconds",
						"The number of seconds to pause between Send or Receive retries on an idle connection to the xT Server",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						15);
			});
		}

		public void TestConnectionToXTServerProd()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.ConnectionToXTServer,
					"ConnectionToXTServer",
					"eServices/xT",
					"Connection To XT Server (Production/Test/Local)",
					@"This registry is used for selecting to which xT server CW1 will connect.

By default, this registry value is set to the database instance of the licence type. BE AWARE that if you change it to the xT does not match the licence type, MESSAGES WILL BE SENT TO A DIFFERENT SERVER THAN DEFAULT.

By default, the LCL (Local xT Server) option uses 127.0.0.1:61001 as the connection URL, or configure via 'xT Local Developer Address'. You must also configure the 'xT Local Developer Certificate' registry to use this option.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					ConnectionToXTServerOptions.XtProduction.Code);
			});
		}

		public void TestConnectionToXTServerTest()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.ConnectionToXTServer,
					"ConnectionToXTServer",
			"eServices/xT",
			"Connection To XT Server (Production/Test/Local)",
			@"This registry is used for selecting to which xT server CW1 will connect.

By default, this registry value is set to the database instance of the licence type. BE AWARE that if you change it to the xT does not match the licence type, MESSAGES WILL BE SENT TO A DIFFERENT SERVER THAN DEFAULT.

By default, the LCL (Local xT Server) option uses 127.0.0.1:61001 as the connection URL, or configure via 'xT Local Developer Address'. You must also configure the 'xT Local Developer Certificate' registry to use this option.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
					ConnectionToXTServerOptions.XtTest.Code);
			});
		}

		public void TestConnectionToXTServer_LookupList()
		{
			var editorInfo = (ComboBoxRegistryEditorInfo)ItemSet.ConnectionToXTServer.EditorInfo;
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					ConnectionToXTServerOptions.XtProduction.Code,
					ConnectionToXTServerOptions.XtTest.Code,
					ConnectionToXTServerOptions.XtLocal.Code,
				},
				editorInfo.LookUpList.GetAllCodes()
			);
		}

		public void TestXTLocalDeveloperAddress()
		{
			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.XTLocalDeveloperAddress,
					"XTLocalDeveloperAddress",
					"eServices/xT",
					"xT Local Developer Address",
					@"Your local xT gRPC endpoint to connect to.

By default, 127.0.0.1:61001 is used. To connect to a different address (perhaps if you have multiple local xT instances), you can update this registry.
Note: this can be configured to non-local addresses (eg: some.other.xt.server.sand.wtg.zone), but please be careful!",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					"127.0.0.1:61001");
			});
		}

		public void TestEnableXTIServiceTask()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			CombineAssertions(() =>
			{
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				TestRegistryItem(ItemSet.EnableXTIServiceTask,
					"EnableXTIServiceTask",
					"eServices/xT",
					"xT Enable XTI Service Task",
					"The XTI service task will be enabled or disabled based on this setting.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true);

				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				TestRegistryItem(ItemSet.EnableXTIServiceTask,
					"EnableXTIServiceTask",
					"eServices/xT",
					"xT Enable XTI Service Task",
					"The XTI service task will be enabled or disabled based on this setting.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true);
			});
		}

		public void TestEnableXTINudge()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			CombineAssertions(() =>
			{
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				TestRegistryItem(ItemSet.EnableXTINudge,
					"EnableXTINudge",
					"eServices/xT",
					"xT Enable Nudging of XTI Service Task",
					"Nudging of the XTI service task will be enabled or disabled based on this setting.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true);
			});
		}

		public void TestInterchangeCountOnSendingPerBatch()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			CombineAssertions(() =>
			{
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				TestRegistryItem(ItemSet.InterchangeCountPerBatchOnSending,
					"InterchangeCountPerBatchOnSending",
					"eServices/xT",
					"Number of interchanges per batch when sending",
					"Number of interchanges to be processed per batch when sending interchanges to xT Server. Note: increasing beyond 20-30 per batch could result in mass failures.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					15);

				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				TestRegistryItem(ItemSet.InterchangeCountPerBatchOnSending,
					"InterchangeCountPerBatchOnSending",
					"eServices/xT",
					"Number of interchanges per batch when sending",
					"Number of interchanges to be processed per batch when sending interchanges to xT Server. Note: increasing beyond 20-30 per batch could result in mass failures.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					15);
			});
		}

		public void TestInterchangeCountOnReceivingPerBatch()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			CombineAssertions(() =>
			{
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				TestRegistryItem(ItemSet.InterchangeCountPerBatchOnReceiving,
					"InterchangeCountPerBatchOnReceiving",
					"eServices/xT",
					"Number of interchanges per batch when receiving",
					"Number of interchanges to be processed per batch when receiving interchanges from xT Server.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100);

				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				TestRegistryItem(ItemSet.InterchangeCountPerBatchOnReceiving,
					"InterchangeCountPerBatchOnReceiving",
					"eServices/xT",
					"Number of interchanges per batch when receiving",
					"Number of interchanges to be processed per batch when receiving interchanges from xT Server.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					100);
			});
		}

		public void TestXTSendingRetryLimit()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();

			CombineAssertions(() =>
			{
				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
				TestRegistryItem(ItemSet.XTSendingRetryLimit,
					"XTSendingRetryLimit",
					"eServices/xT",
					"Interchanges will no longer be attempted to process after this amount of retries.",
					"Interchanges will no longer be attempted to process after this amount of retries.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					5);

				reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
				TestRegistryItem(ItemSet.XTSendingRetryLimit,
					"XTSendingRetryLimit",
					"eServices/xT",
					"Interchanges will no longer be attempted to process after this amount of retries.",
					"Interchanges will no longer be attempted to process after this amount of retries.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					5);
			});
		}

		public void TestHealthCheckNotificationGroup()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.xTFailedEDIInterchangeNotificationGroup,
					"xTFailedEDIInterchangeNotificationGroup",
					"eServices/xT/Health Check/Failed EDI Interchange",
					"Email Recipients",
					@$"The staff group that will be notified when EDI Interchange fail. The fallback level rule applied when: 
	- If the xT Error Notification Group doesn't contain a valid Email address for this company, it will look for Email addresses for this {Core.Constants.ProductName} System.
	- If the xT Error Notification Group doesn't contain a valid Email address for this {Core.Constants.ProductName} System, it will look for System Notification Group ([System] > [Registry] > [Notification] > [Company Notification Group]).
	- If the System Notification Group doesn't contain a valid Email address, the notification will send to all the staff listed in the {Core.Constants.ProductName} System.",
					RegistryStorageFlags.Company | RegistryStorageFlags.System,
					RegistryOptions.IsValueMandatory);
			});
		}

		public void TestHealthCheckNotificationFrequency()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.xTFailedEDIInterchangeNotificationFrequency,
					"xTFailedEDIInterchangeNotificationFrequency",
					"eServices/xT/Health Check/Failed EDI Interchange",
					"Notification Frequency",
					"Configure how often notifications will be sent out, notifications are only sent when there are failed interchanges to report on.",
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue);
			});
		}

		public void TestHealthCheckLastCheckedTime()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.xTLastCheckedTimeForFailedEDIInterchange,
					"xTLastCheckedTimeForFailedEDIInterchange",
					"eServices/xT/Health Check/Failed EDI Interchange/HIDDEN",
					"LastCheckedTimeForFailedEDIInterchange",
					"Readonly. The last checked time point for failed EDIInterchanges",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden | RegistryOptions.NotCached);
			});
		}

		public void TestHealthCheckLastReportedTime()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.xTLastReportedTimeForFailedEDIInterchange,
					"xTLastReportedTimeForFailedEDIInterchange",
					"eServices/xT/Health Check/Failed EDI Interchange/HIDDEN",
					"LastReportedTimeForFailedEDIInterchange",
					"Readonly. The last reported time point of Periodically Setting for failed EDIInterchanges",
					RegistryStorageFlags.System,
					RegistryOptions.IsHidden | RegistryOptions.NotCached);
			});
		}

		public void TestHealthCheckCountFailedEDIInterchangesInPeriodically()
		{
			var reg = ObjectFactory.Get<IProductRegistration>();
			reg.ResetKeyToDefault();
			reg.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			CombineAssertions(() =>
			{
				TestGenericRegistryItem(ItemSet.xTCountFailedEDIInterchangesInPeriodically,
					"xTCountFailedEDIInterchangesInPeriodically",
					"eServices/xT/Health Check/Failed EDI Interchange/HIDDEN",
					"CountFailedEDIInterchangesInPeriodically",
					"Readonly. Count failed EDIInterchanges in Periodically",
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden | RegistryOptions.NotCached);
			});
		}
	}
}

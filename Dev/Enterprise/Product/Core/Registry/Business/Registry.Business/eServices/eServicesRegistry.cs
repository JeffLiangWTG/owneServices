using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.eHub;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class eServicesRegistry : RegistryItemSet
	{
		#region Singleton Instance

		[ThreadStatic]
		static eServicesRegistry instance;

		public static eServicesRegistry Instance
		{
			get { return instance ?? (instance = new eServicesRegistry()); }
		}

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString eServices_UniversalXML { get { return CombineCategories(eServices, ResString.GetMultilingualString("e3d051bc-6616-4c76-ba35-8b855e70ef4e", "Universal XML")); } }
			public static MultilingualString eServices_UniversalXML_InboundMessageNotifications { get { return CombineCategories(eServices_UniversalXML, ResString.GetMultilingualString("9bf88b36-d4c4-4464-926c-e2812b6bba32", "Inbound Message Notifications")); } }
			public static MultilingualString eServices_eHub { get { return CombineCategories(eServices, ResString.GetMultilingualString("b1006f77-0a07-49d8-9b9a-ef971266b96f", "eHub")); } }
			public static MultilingualString eServices_eAdaptor { get { return CombineCategories(eServices, ResString.GetMultilingualString("0ff7fac0-53d7-4449-907a-96b8cb13f4b3", "eAdaptor")); } }
			public static MultilingualString eServices_EDIClient { get { return CombineCategories(eServices, ResString.GetMultilingualString("fdd4cb5f-4208-4670-acfa-c54c5de05a3a", "EDI Client")); } }
		}

		#endregion

		#region Inbound Message Notifications

		public GuidRegistryItem ImportRejectedNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("InboundMessageRejectedNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						"InboundMessageRejectedNotificationGroup",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("FE4807F2-1C39-4E51-A71C-3896570A9251", "Import Rejected Notification Group"),
						ResString.GetMultilingualString("D6FC631E-0983-43D4-B1D0-1FD99758835E", "The staff group that will receive notification about rejected import."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem ImportEventRejectedNotificationGroup
		{
			get
			{
				return GetItem("InboundEventMessageRejectedNotificationGroup", delegate
				{
					var result = new GuidRegistryItem("InboundEventMessageRejectedNotificationGroup",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("0AF03330-1143-449F-1432-F06F598EC0B0", "Import Rejected Notification Group (For Universal Events)"),
						ResString.GetMultilingualString("B19ECC2C-62DD-4132-8EB8-2A7C1EE6CA75", @"The staff group that will receive notification about rejected Universal Event imports. This will default to Import Rejected Notification Group if empty."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, ImportRejectedNotificationGroup.Value);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem ImportDiscardedNotificationGroup
		{
			get
			{
				return GetItem<GuidRegistryItem>("InboundMessageDiscardedNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"InboundMessageDiscardedNotificationGroup",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("4C3DC013-60A8-4036-BD01-716FE4F39685", "Import Discarded Notification Group"),
						ResString.GetMultilingualString("976083F5-E64D-45B7-AC7A-30211A98C368", "The staff group that will receive notification about discarded import."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public InboundMessageNotificationsRegistryItem ImportWithErrorsNotificationConfiguration
		{
			get
			{
				var rule = new InboundMessageNotificationsRule();

				return GetItem("InboundMessageProcessedWithErrorsNotificationConfiguration", delegate
				{
					return new InboundMessageNotificationsRegistryItem("InboundMessageProcessedWithErrorsNotificationConfiguration",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("0AF03330-8591-449F-83F6-F06F598EC0B0", "Inbound Message Processed With Errors Notification Configuration"),
						ResString.GetMultilingualString("B19ECC2C-62DD-43C2-8EB8-2A7C1EE6CA75", @"Use the following registry to configure recipients for inbound message processed with errors notifications."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, rule);
				});
			}
		}

		public InboundMessageNotificationsRegistryItem ImportEventWithErrorsNotificationConfiguration
		{
			get
			{
				return GetItem("InboundEventProcessedWithErrorsNotificationConfiguration", delegate
				{
					return new InboundMessageNotificationsRegistryItem("InboundEventProcessedWithErrorsNotificationConfiguration",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("0AF03330-1143-449F-83F6-F06F598EC0B0", "Inbound Universal Event Processed With Errors Notification Configuration"),
						ResString.GetMultilingualString("B19ECC2C-62DD-3321-8EB8-2A7C1EE6CA75", @"Use the following registry to configure recipients for inbound Universal Event message processed with errors notifications. This will default to Inbound Message configuration if empty. "),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, ImportWithErrorsNotificationConfiguration.Value);
				});
			}
		}

		public InboundMessageNotificationsRegistryItem ImportWithWarningNotificationConfiguration
		{
			get
			{
				var rule = new InboundMessageNotificationsRule();

				return GetItem("InboundMessageProcessedWithWarningsNotificationConfiguration", delegate
				{
					return new InboundMessageNotificationsRegistryItem("InboundMessageProcessedWithWarningsNotificationConfiguration",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("9D821342-6184-4065-8A05-EA1DF476674D", "Inbound Message Processed With Warnings Notification Configuration"),
						ResString.GetMultilingualString("D3D50829-9314-4751-B1E8-4EBACB0A11A6", @"Use the following registry to configure recipients for inbound message processed with warnings notifications."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, rule);
				});
			}
		}

		public InboundMessageNotificationsRegistryItem ImportOKNotificationConfiguration
		{
			get
			{
				var rule = new InboundMessageNotificationsRule();

				return GetItem("InboundMessageProcessedOKNotificationConfiguration", delegate
				{
					return new InboundMessageNotificationsRegistryItem("InboundMessageProcessedOKNotificationConfiguration",
						Categories.eServices_UniversalXML_InboundMessageNotifications,
						ResString.GetMultilingualString("A3181170-3F6B-4B68-B489-D8686B20BCAC", "Inbound Message Processed OK Notification Configuration"),
						ResString.GetMultilingualString("485630B8-74AE-4A9C-BB4C-E9914CE2F417", @"Use the following registry to configure recipients for inbound message processed OK notifications."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, rule);
				});
			}
		}

		#endregion

		#region BillingTransactionsTransformationSettings

		public BillingTransactionsTransformationSettingsRegistryItem BillingTransactionsTransformationSettings
		{
			get
			{
				return GetItem("BillingTransactionsTransformationSettings", delegate
				{
					var result = new BillingTransactionsTransformationSettingsRegistryItem(
						"BillingTransactionsTransformationSettings",
						Categories.eServices,
						(NoResString)"BT Transformation Settings",
						(NoResString)"This registry item is used by BillingTransaction online database transformation to store the current fix. It's displayed for information purposes only and should not generally be changed.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers);
					result.IsExcludedFromCwOnlyNonCachedTest = true;
					return result;
				});
			}
		}

		#endregion

	}
}

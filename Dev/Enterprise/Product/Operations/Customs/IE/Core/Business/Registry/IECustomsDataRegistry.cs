using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Business
{
	public sealed class IECustomsDataRegistry : RegistryItemSet
	{
		#region Construction

		public static IECustomsDataRegistry Instance => instance ?? (instance = new IECustomsDataRegistry());
		[ThreadStatic]
		static IECustomsDataRegistry instance;

		IECustomsDataRegistry() { }

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Ireland;
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Ireland => CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("D8A0001D-468A-4BFC-8483-08F0482DFDB9", "Ireland"));
			public static MultilingualString Customs_Ireland_Notifications => CombineCategories(Customs_Ireland, ResString.GetMultilingualString("44099A15-44A5-4427-A937-7D17E29C6D2D", "Notifications"));
			public static MultilingualString Customs_Ireland_Notifications_Export => CombineCategories(Customs_Ireland_Notifications, ResString.GetMultilingualString("0BDC14AC-38BC-4BA9-B0C5-B43613A0FFC0", "Export"));
			public static MultilingualString Customs_Ireland_Notifications_Import => CombineCategories(Customs_Ireland_Notifications, ResString.GetMultilingualString("E1959BFC-AF78-451F-B7A6-690167C8F586", "Import"));
			public static MultilingualString Customs_Ireland_Notifications_Ncts => CombineCategories(Customs_Ireland_Notifications, ResString.GetMultilingualString("76D15E9A-9220-4C64-A2A6-8E8068249710", "NCTS"));
			public static MultilingualString Customs_Ireland_Notifications_CustomsAndExciseReport => CombineCategories(Customs_Ireland_Notifications, ResString.GetMultilingualString("D9EC830B-847D-4326-82E3-DE924718B064", "Customs and Excise Report"));
			public static MultilingualString Customs_Ireland_Notifications_PBNId => CombineCategories(Customs_Ireland_Notifications, ResString.GetMultilingualString("FFCCF3D6-27FA-4527-9D43-D47F00A0B7FC", "PBN ID"));
		}

		public BooleanRegistryItem IsDirectSendToCustomsForImportEnabled
		{
			get
			{
				return GetItem("IsDirectSendToCustomsForImportEnabled", delegate
				{
					return new BooleanRegistryItem(
						name: "IsDirectSendToCustomsForImportEnabled",
						category: Categories.Customs_Ireland,
						caption: (NoResString)"Enable 'Send To Customs' for Imports",
						hint: (NoResString)"If turned on, the 'Send To Customs' menu item under the 'Brokerage' menu will be displayed for Import Declarations.",
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.IsOnlyForDevelopers,
						defaultValue: false);
				});
			}
		}

		public BooleanRegistryItem IsUCC6EnabledForImport
		{
			get
			{
				return GetItem("IsUCC6EnabledForImport", delegate
				{
					return new BooleanRegistryItem(
						name: "IsUCC6EnabledForImport",
						category: Categories.Customs_Ireland,
						caption: (NoResString)"Enable Submit Type V2 (UCC6) for Imports",
						hint: (NoResString)"If turned on, the Submit Type V2 (UCC6) will be displayed for Import Declarations.",
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.IsOnlyForDevelopers,
						defaultValue: false);
				});
			}
		}

		public IntRegistryItem MessageProcessingNoOfDays
		{
			get
			{
				return GetItem("IEMessageProcessingNoOfDays", delegate
				{
					return new IntRegistryItem(
						name: "IEMessageProcessingNoOfDays",
						category: Categories.Customs_Ireland,
						caption: (NoResString)"Message Processing No Of Days",
						hint: (NoResString)"System will only process messages that have been created in the last no of days.",
					storage: RegistryStorageFlags.System,
						defaultValue: 14);
				});
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> SendImportAcknowledgements
		{
			get
			{
				return GetItem(
					key: "SendImportAcknowledgements",
					createItemDelegate: () => new GroupNotificationRegistryItem<GroupNotification>(
						name: "SendImportAcknowledgements",
						category: Categories.Customs_Ireland_Notifications_Import,
						caption: ResString.GetMultilingualString("BB0CFB3C-76BD-4083-AA9F-3E8008EE037C", "Send Import Acknowledgements To"),
						hint: ResString.GetMultilingualString("DB618C27-2219-4E23-A2BC-EA6042BBA79C", "Send Import acknowledgements to staff member, nominated group or both"),
						storage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						options: RegistryOptions.Default,
						defaultValue: new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
					)
				);
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> SendCustomsAndExciseReportAcknowledgements
		{
			get
			{
				return GetItem(
					key: "SendCustomsAndExciseReportAcknowledgements",
					createItemDelegate: () => new GroupNotificationRegistryItem<GroupNotification>(
						name: "SendCustomsAndExciseReportAcknowledgements",
						category: Categories.Customs_Ireland_Notifications_CustomsAndExciseReport,
						caption: ResString.GetMultilingualString("8E30D9DF-32F4-48FA-B12E-E92CB4462EAB", "Send Customs and Excise Report Acknowledgements To"),
						hint: ResString.GetMultilingualString("E0F22016-3974-408F-B879-12E87866150B", "Send Customs and Excise Report acknowledgements to staff member, nominated group or both"),
						storage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						options: RegistryOptions.Default,
						defaultValue: new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
					)
				);
			}
		}

		public GroupNotificationRegistryItem<GroupNotification> SendPreBoardingNotificationIds
		{
			get
			{
				return GetItem(
					key: "SendPreBoardingNotificationIds",
					createItemDelegate: () => new GroupNotificationRegistryItem<GroupNotification>(
						name: "SendPreBoardingNotificationIds",
						category: Categories.Customs_Ireland_Notifications_PBNId,
						caption: ResString.GetMultilingualString("99BE9929-AD0E-4C2B-A227-F614C5BFBB21", "Send PBN Ids To"),
						hint: ResString.GetMultilingualString("6379FADA-1695-4CF9-9144-8CD9E2BEFEED", "Send Pre-Boarding Notification Ids to staff member, nominated group or both"),
						storage: RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						options: RegistryOptions.Default,
						defaultValue: new GroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty)
					)
				);
			}
		}
	}
}

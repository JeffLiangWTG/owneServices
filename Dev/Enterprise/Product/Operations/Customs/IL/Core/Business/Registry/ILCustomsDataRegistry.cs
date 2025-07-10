using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.IL;

namespace Enterprise.Customs.IL.Business
{
	public sealed class ILCustomsDataRegistry : RegistryItemSet, IILCustomsDataRegistry
	{
		#region Construction

		public static ILCustomsDataRegistry Instance
			=> instance ?? (instance = new ILCustomsDataRegistry());

		[ThreadStatic]
		static ILCustomsDataRegistry instance;

		ILCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Israel
				=> CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("1C2F228D-BEA6-48FA-8189-784FFEE929AD", "Israel"));
			public static MultilingualString Customs_Israel_Manifest
				=> CombineCategories(Customs_Israel, ResString.GetMultilingualString("5892CDA4-816E-4714-8835-AD6DC15C4898", "Manifest"));
			public static MultilingualString Customs_Israel_DeliveryOrder
				=> CombineCategories(Customs_Israel, ResString.GetMultilingualString("B82AD105-1B15-4720-833D-DE64BD9844B6", "Delivery Order"));
			public static MultilingualString Customs_Israel_GatePassMovements
				=> CombineCategories(Customs_Israel, ResString.GetMultilingualString("F2EC0ABA-EB09-4938-8656-799B96ECD00D", "Gate pass Movements"));
			public static MultilingualString Customs_Israel_IIG
				=> CombineCategories(Customs_Israel, ResString.GetMultilingualString("C3D8DEDD-76EE-4E4F-BDF0-0FF487E00A21", "IIG"));
			public static MultilingualString Customs_Israel_DigitalSignature
				=> CombineCategories(Customs_Israel, ResString.GetMultilingualString("D6B76D27-C5C4-47EF-8D10-B0DB10C46ACB", "Digital Signature"));
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public ManifestGroupNotificationRegistryItem ILMANGroupNotification
		{
			get
			{
				return GetItem("ILMANGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"ILMANGroupNotification",
						Categories.Customs_Israel_Manifest,
						ResString.GetMultilingualString("55730951-2B26-4D8E-A275-5E3E287A74A2", "Notification Group"),
						ResString.GetMultilingualString("D6E06BC3-96D9-486E-B420-3097605A15D5", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					return result;
				}
				);
			}
		}

		public CodePairRegistryItem ILEnableILManifest
		{
			get
			{
				return GetItem("ILEnableILManifest", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() => new ILManifestRegistryOptions());

					var result = new CodePairRegistryItem(
						name: "ILEnableILManifest",
						category: Categories.Customs_Israel_Manifest,
						caption: ResString.GetMultilingualString("57DC969E-624E-47E4-AC89-EFA62C3F9AA5", "Enable IL Manifest"),
						hint: ResString.GetMultilingualString("71FCD8F1-6969-489B-8FD5-C1B09AF9E6C6", "Enable IL Manifest"),
						lookUpList: listProvider,
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.IsOnlyForDevelopers,
						defaultValue: ILManifestRegistryOptions.Codes.NONE);

					result.EditorInfo = new ComboBoxRegistryEditorInfo(listProvider, false);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableILDeliveryOrder
		{
			get
			{
				return GetItem("EnableILDeliveryOrder", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableILDeliveryOrder",
						Categories.Customs_Israel_DeliveryOrder,
						ResString.GetMultilingualString("C5CC76A6-698B-4E3B-A7A1-4D239AD5612E", "Enable IL Delivery Order"),
						ResString.GetMultilingualString("096D41B6-9D09-4FAE-B3A9-37F754C800B7", "Enable IL Delivery Order?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		public ManifestGroupNotificationRegistryItem ILDELGroupNotification
		{
			get
			{
				return GetItem("ILDELGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"ILDELGroupNotification",
						Categories.Customs_Israel_DeliveryOrder,
						ResString.GetMultilingualString("3084669F-B25C-46FB-9E6C-A4A5B8BF244A", "Notification Group"),
						ResString.GetMultilingualString("0FF00E1A-3D5E-458D-AA4F-18E6253A4ADD", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					return result;
				}
				);
			}
		}

		public BooleanRegistryItem EnableILGatePassMovements
		{
			get
			{
				return GetItem("EnableILGatePassMovements", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableILGatePassMovements",
						Categories.Customs_Israel_GatePassMovements,
						ResString.GetMultilingualString("49CA03B8-B063-4792-9D28-7021DD744441", "Enable IL Gate pass Movements"),
						ResString.GetMultilingualString("781EA947-E01A-448C-8EE3-199823189479", "Enable IL Gate pass Movements?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		public ManifestGroupNotificationRegistryItem ILGPMGroupNotification
		{
			get
			{
				return GetItem("ILGPMGroupNotification", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"ILGPMGroupNotification",
						Categories.Customs_Israel_GatePassMovements,
						ResString.GetMultilingualString("B970C324-2D2C-4748-921C-3131BDE09DE9", "Notification Group"),
						ResString.GetMultilingualString("EAF4D869-C5FC-4C5F-844E-8EE286CA6D3B", "Group to receive Manifest Messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ManifestGroupNotification.Default);

					return result;
				}
				);
			}
		}

		public BooleanRegistryItem EnablePullASyncMessage
		{
			get
			{
				return GetItem("EnablePullASyncMessage", delegate
				{
					var result = new BooleanRegistryItem(
						"EnablePullASyncMessage",
						Categories.Customs_Israel_IIG,
						ResString.GetMultilingualString("A470361E-EAF5-425B-A6BD-8338BEE8F6FE", "Enable Pull A-Sync Message"),
						ResString.GetMultilingualString("D60B905F-F56F-4314-B8A8-31E29D91A66B", "Enable Pull A-Sync Message?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		public DCAParametersRegistryItem DCAParametersForASyncMessage
		{
			get
			{
				return GetItem("DCAParametersForASyncMessage", delegate
				{
					var result = new DCAParametersRegistryItem(
						"DCAParametersForASyncMessage",
						Categories.Customs_Israel_IIG,
						ResString.GetMultilingualString("2ED3A1AE-B533-43E1-84E7-45162FD3061F", "DCA Parameters for A-Sync Message"),
						ResString.GetMultilingualString("3E6FD67E-0060-4770-AE1C-595900CF3BA6", "DCA Parameters for A-Sync Message"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
					return result;
				});
			}
		}

		public CodePairRegistryItem PersonalDigitalSignatureFallbackConfiguration
		{
			get
			{
				return GetItem("PersonalDigitalSignatureFallbackConfiguration", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() => new DigitalSignatureFallbackList());

					var result = new CodePairRegistryItem(
						name: "PersonalDigitalSignatureFallbackConfiguration",
						category: Categories.Customs_Israel_DigitalSignature,
						caption: ResString.GetMultilingualString("5C492561-B276-40B1-A17D-85AB2CF4A534", "Personal Digital Signature Fallback Configuration"),
						hint: ResString.GetMultilingualString("644EED4F-5911-4577-8725-E0EE52A89037", "Select the method by which the system will locate a valid digital signature certificate when a personal signature is necessary"),
						lookUpList: listProvider,
						storage: RegistryStorageFlags.Company,
						defaultValue: DigitalSignatureFallbackList.Codes.StaffOnly);

					result.EditorInfo = new ComboBoxRegistryEditorInfo(listProvider, false);
					return result;
				});
			}
		}

		public BooleanRegistryItem SendILCustomsMessagesWithoutDigitalSignature
		{
			get
			{
				return GetItem("SendILCustomsMessagesWithoutDigitalSignature", delegate
				{
					var result = new BooleanRegistryItem(
						"SendILCustomsMessagesWithoutDigitalSignature",
						Categories.Customs_Israel_DigitalSignature,
						ResString.GetMultilingualString("651300E5-14D8-4BA9-BBA8-D7AAAFCCFAEB", "Send IL Customs messages without digital signature"),
						ResString.GetMultilingualString("31188158-08EF-442E-9FDF-8928EA6BB90F", "Send IL Customs messages without digital signature?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);

					return result;
				});
			}
		}

		#region IILCustomsDataRegistry
		IRegistryItem IILCustomsDataRegistry.ILEnableILManifest => ILEnableILManifest;
		IRegistryItem IILCustomsDataRegistry.SendILCustomsMessagesWithoutDigitalSignature => SendILCustomsMessagesWithoutDigitalSignature;
		#endregion

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = CountryFilterPKs.Israel;
		}
	}
}

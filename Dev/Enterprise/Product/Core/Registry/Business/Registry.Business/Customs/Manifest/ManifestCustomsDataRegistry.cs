using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.Manifest
{
	public sealed class ManifestCustomsDataRegistry : RegistryItemSet
	{
		ManifestCustomsDataRegistry() { }

		BusinessObjectFactory fFactory;
		BusinessObjectFactory Factory => fFactory ?? (fFactory = new BusinessObjectFactory { NameForDebugging = "ManifestCustomsDataRegistry" });

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Manifest => CombineCategories(Customs, ResString.GetMultilingualString("239775EA-BC7B-4292-9560-3F15216FB612", "Manifest"));
			public static MultilingualString Customs_Manifest_Testing => CombineCategories(Customs_Manifest, ResString.GetMultilingualString("173F201B-8363-463A-AEF0-5BA8577E9774", "Testing"));
			public static MultilingualString Customs_UnitedStatesofAmerica => CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("United States of America", "United States of America"));
			public static MultilingualString Customs_UnitedStatesofAmerica_AirAMS => CombineCategories(Customs_UnitedStatesofAmerica, ResString.GetMultilingualString("1FAAA1B8-1823-45C7-BE06-F5642DF835B4", "Air-AMS"));
		}

		#endregion

		#region Manifest Job Number Customization

		public BillCustomisationRegistryItem ManifestJobNumberCustomization
		{
			get
			{
				return GetItem(
					"ManifestJobNumberCustomization",
					() => new BillCustomisationRegistryItem(
									"ManifestJobNumberCustomization",
							Categories.Customs_Manifest,
									ResString.GetMultilingualString("EFCC1FAC-A84A-4BFF-89E1-0BA05E61E9EA", "Manifest Job Number Customization"),
									ResString.GetMultilingualString("8DEA0953-1B03-4AF4-BA0D-C64C51030D0B", "Override this value to customize how Manifest Job Numbers are formatted"),
									RegistryStorageFlags.All,
									new ManifestJobNumberCustomisationRegistryDataType()));
			}
		}

		#endregion

		#region Global Manifest submission responses

		CodeDescriptionPairList SendNotificationsList => Factory.GetCachedValue("Enterprise.Registry.Business.Customs.Manifest.ManifestCustomsDataRegistry|SendNotificationsList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.EmailTo.NoEmails, ResString.GetMultilingualString("ManifestCustomsDataRegistry.SendNotificationsList|NoEmails", "No Emails"));
			result.AddPair(Constants.EmailTo.NominatedGroup, ResString.GetMultilingualString("ManifestCustomsDataRegistry.SendNotificationsList|NominatedGroup", "Email Nominated Group"));
			result.AddPair(Constants.EmailTo.StaffMemberAndNominatedGroup, ResString.GetMultilingualString("ManifestCustomsDataRegistry.SendNotificationsList|StaffMemberAndNominatedGroup", "Email Staff Member and Nominated Group"));
			return result;
		});

		public CodePairRegistryItem SendSuccessNotifications =>
			GetItem(
				"SendSuccessNotifications",
				() => new CodePairRegistryItem(
					"SendSuccessNotifications",
					Categories.Customs_Manifest,
					ResString.GetMultilingualString("C8603CE3-3124-4133-801F-29D9117B1342", "Send success notifications"),
					null,
					new CodeDescriptionPairListProvider(() => SendNotificationsList),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					Constants.EmailTo.StaffMemberAndNominatedGroup
				)
				{
					CountryFilterPKs = CountryFilterPKs.Singapore
				}
			);

		public CodePairRegistryItem SendErrorNotifications =>
			GetItem(
				"SendErrorNotifications",
				() => new CodePairRegistryItem(
					"SendErrorNotifications",
					Categories.Customs_Manifest,
					ResString.GetMultilingualString("4FE66EAA-8B71-423A-ACA9-13D160C661A8", "Send error notifications"),
					null,
					new CodeDescriptionPairListProvider(() => SendNotificationsList),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					Constants.EmailTo.StaffMemberAndNominatedGroup
				)
				{
					CountryFilterPKs = CountryFilterPKs.Singapore
				}
			);

		public GuidRegistryItem GroupToSendSucceedNotification =>
			(GuidRegistryItem)GetItem(
				"GroupToSendSucceedNotification",
				() =>
				{
					IRegistryItem result = new GuidRegistryItem(
						"GroupToSendSucceedNotification",
						Categories.Customs_Manifest,
						ResString.GetMultilingualString("8FC154AE-8757-44C2-94FC-59649FADD046", "Group to send Global Manifest Universal Event success response notification to"),
						null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Singapore;
					return result;
				});

		public GuidRegistryItem GroupToSendErrorNotification =>
			(GuidRegistryItem)GetItem(
				"GroupToSendErrorNotification",
				() =>
				{
					IRegistryItem result = new GuidRegistryItem(
						"GroupToSendErrorNotification",
						Categories.Customs_Manifest,
						ResString.GetMultilingualString("7974BD3B-F9A4-4150-87FA-E80AD4E4B2AF", "Group to send Global Manifest Universal Event failure response notification to"),
						null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Singapore;
					return result;
				});

		#endregion

		#region Add Excise Value to Duty Amount

		public BooleanRegistryItem AddExciseValueToDutyAmount => GetItem(
			"AddExciseValueToDutyAmount", () => new BooleanRegistryItem(
				"AddExciseValueToDutyAmount",
				Categories.Customs_Manifest,
				ResString.GetMultilingualString("D15F7CF3-7627-4C17-AD1D-E60C21C8D539", "Add Excise Value to Duty Amount"),
				ResString.GetMultilingualString("5736CE06-641E-40DA-9C4C-E8760889FAD2", "Add any Excise values to the Global Manifest Duty to Customs Declaration Synchronization"),
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				true
			));

		#endregion

		#region Maximum Searchable Global Manifest Bills
		public IntRegistryItem MaximumSearchableManifestBills =>
			(IntRegistryItem)GetItem(
				"MaximumSearchableManifestBills",
				() =>
				{
					IRegistryItem result = new IntRegistryItem(
						"MaximumSearchableManifestBills",
						Categories.Customs_Manifest,
						ResString.GetMultilingualString("0A4001B7-DA36-493C-83FB-E45A12F097D2", "Maximum Searchable Global Manifest Bills"),
						ResString.GetMultilingualString("87B22688-AE65-4D89-A70C-55E9E781936A", "Maximum number of search rows in the Global Manifest Bill search grid"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						5000,
						1,
						15000
					);
					return result;
				});

		#endregion

		#region Pre Boarding Notification Manifest

		public BooleanRegistryItem PreBoardingNotificationManifestEnabled => GetItem(
			"PreBoardingNotificationManifestEnabled", () => new BooleanRegistryItem(
				new BooleanCountryDependingRegistryItem(
							"PreBoardingNotificationManifestEnabled",
							Categories.Customs_Manifest,
							(NoResString)"Enable Pre-boarding Notification Manifest",
							(NoResString)"Set this to enable Pre-boarding Notification Manifest module.",
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							false,
							Array.Empty<string>()
							//Enabled by default for the UnitedKingdom when module is ready
							//new string[] { Core.Constants.CountryCodes.UnitedKingdom }
							))
			{
				CountryFilterPKs = CountryFilterPKs.Ireland
			});

		#endregion

		#region SuppressResourceStringsCheckRegion

		#region Enable Manifest Consol Decoupling

		public BooleanRegistryItem EnableManifestConsolDecoupling
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableManifestConsolDecoupling", () => new BooleanRegistryItem("EnableManifestConsolDecoupling",
					Categories.Customs_Manifest,
					(NoResString)"Enable Manifest Consol Decoupling",
					(NoResString)"Enable Decoupling of Manifests from their associated Consol",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region Enable FDM Message

		public BooleanRegistryItem EnableFDMMessage
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableFDMMessage", () => new BooleanRegistryItem("EnableFDMMessage",
					Categories.Customs_UnitedStatesofAmerica_AirAMS,
					(NoResString)"Enable FDM Message",
					(NoResString)"Enable Sending of FDM messages",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region Enable MAWB Messaging

		public BooleanRegistryItem EnableMAWBMessaging
		{
			get
			{
				return GetItem<BooleanRegistryItem>("EnableMAWBMessaging", () => new BooleanRegistryItem("EnableMAWBMessaging",
					Categories.Customs_UnitedStatesofAmerica_AirAMS,
					(NoResString)"Enable MAWB Messaging",
					(NoResString)"Enable Sending of messages from the MAWB",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		#endregion

		#region Enable non-HVLV Air-AMS Email Notification
		public ManifestGroupNotificationRegistryItem USAirAMSGroupNotification
		{
			get
			{
				return GetItem<ManifestGroupNotificationRegistryItem>("USAirAMSGroupNotification", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"USAirAMSGroupNotification",
						Categories.Customs_UnitedStatesofAmerica_AirAMS,
						ResString.GetMultilingualString("b3258820-094a-47da-a13c-47211e763827", "Notifications Group"),
						ResString.GetMultilingualString("0f3fca94-346e-4598-b903-633ec272a23d", "Group to receive Air AMS messages sent by Customs. if 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new ManifestGroupNotification(GroupNotification.StaffMemberOrNominatedGroup, Constants.Groups.PostMastersGroupPK, false));
				});
			}
		}

		#endregion

		#region Enable HVLV Air-AMS Email Notification
		public ManifestGroupNotificationRegistryItem USHVLVAirAMSGroupNotification
		{
			get
			{
				return GetItem<ManifestGroupNotificationRegistryItem>("USHVLVAirAMSGroupNotification", delegate
				{
					return new ManifestGroupNotificationRegistryItem(
						"USHVLVAirAMSGroupNotification",
						Categories.Customs_UnitedStatesofAmerica_AirAMS,
						ResString.GetMultilingualString("f9ff7f4c-21d3-4c94-b66d-06bd6f8143a6", "HVLV Notifications"),
						ResString.GetMultilingualString("bf7eba1a-2d75-43aa-a935-18485830eda2", "Group to receive Air AMS messages sent by Customs for bills that originate from HVL shipments. If 'Send Error Only' ticked, Only when an error occurs then send the notification email."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new ManifestGroupNotification(GroupNotification.StaffMemberOrNominatedGroup, Constants.Groups.PostMastersGroupPK, false));
				});
			}
		}

		#endregion

		#endregion

		#region Implementation

		public static ManifestCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new ManifestCustomsDataRegistry()); }
		}

		[ThreadStatic]
		static ManifestCustomsDataRegistry instance;

		#endregion
	}
}

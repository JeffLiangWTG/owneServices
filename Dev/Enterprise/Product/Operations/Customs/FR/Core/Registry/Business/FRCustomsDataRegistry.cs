using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Registry
{
	public sealed class FRCustomsDataRegistry : RegistryItemSet
	{
		#region Construction
		public static FRCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new FRCustomsDataRegistry()); }
		}

		public static class Schema
		{
			public const int CorrelationIDMaxLength = 10;
			public const int StatementNumberMaxLength = 25;
		}

		[ThreadStatic]
		static FRCustomsDataRegistry instance;

		FRCustomsDataRegistry()
		{
		}

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments;
		}

		#endregion

		#region Implementation

		public override bool IsForProductivityWise => false;

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_France { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("193F8B98-4CD1-41B8-9981-C5A32B1B5DD5", "France")); } }
			public static MultilingualString Customs_France_Fallback { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("D8D3CBD7-FD63-4FF2-90F9-DED32D06DD62", "Fallback")); } }
			public static MultilingualString Customs_France_CorrelationID { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("49746CC5-524E-4892-80F7-F0980193D784", "Correlation ID")); } }
			public static MultilingualString Customs_France_FRCustomsFallbackEntryNumber { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("527D57E1-79ED-4FF4-9B61-4B5B45EAC477", "FR Customs Fallback Entry Number")); } }
			public static MultilingualString Customs_France_Liquidation { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("34421BF1-947C-4CE0-9C50-8CCD2D033750", "Liquidation")); } }
			public static MultilingualString Customs_France_Notifications { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("1702CE06-00FB-4477-A25D-A3755CCC396E", "Notifications")); } }
			public static MultilingualString Customs_France_FRVATReport { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("CBD4C0EC-855D-481A-AF0B-25972160E1EE", "FR VAT Report")); } }
			public static MultilingualString Customs_France_AutoSendMessages { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("75E596C7-B962-4606-968E-EC7944AC7DDA", "Auto Send Messages")); } }
			public static MultilingualString Customs_France_ExitControlSystem { get { return CombineCategories(Customs_France, ResString.GetMultilingualString("3F2AE97A-3600-417D-9880-B01EA722D8AB", "Exit Control System")); } }
		}

		#endregion

		#region RecipientID

		public StringRegistryItem RecipientID
		{
			get
			{
				return GetItem("FRRecipientID", delegate
				{
					var result = new StringRegistryItem(
						"FRRecipientID",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("E5DF2B18-A216-4372-BDDE-20994D6A88DF", "Recipient ID"),
						ResString.GetMultilingualString("DDA7EDD4-A028-4752-B3B6-3AB7454BE4EF", "Recipient ID for Routing via eHub"),
						RegistryStorageFlags.System,
						"EASYLO2TEST_EAD"
					);
					return result;
				});
			}
		}

		#endregion

		#region CreditCheck

		public BooleanRegistryItem CheckWhenSendingAnArrivedValidee
		{
			get
			{
				return GetItem("CheckWhenSendingAnArrivedValidee", delegate
				{
					var result = new BooleanRegistryItem(
						"CheckWhenSendingAnArrivedValidee",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("e5d87d1d-edee-4a6a-9d73-f20278ea03bf", "Check when sending an arrived validation"),
						ResString.GetMultilingualString("9237e8a9-9b49-4aeb-ab61-4724b3084cff", "Perform the credit check on declarations that do not have an entry number when sending a 'validate' entry."),
						RegistryStorageFlags.Company,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CheckWhenChangingAnAnticipateToAValidee
		{
			get
			{
				return GetItem("CheckWhenChangingAnAnticipateToAValidee", delegate
				{
					var result = new BooleanRegistryItem(
						"CheckWhenChangingAnAnticipateToAValidee",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("7c9e2cd4-85e6-440e-a6c1-9ccb4bb4b914", "Check when changing an anticipate to a validation"),
						ResString.GetMultilingualString("c85b3af6-fd43-4fc3-80b0-88b71f89fd0f", "Perform the credit check on declarations that are 'anticipated' when sending a 'validate' entry."),
						RegistryStorageFlags.Company,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CheckAtEverySubmission
		{
			get
			{
				return GetItem("CheckAtEverySubmission", delegate
				{
					var result = new BooleanRegistryItem(
						"CheckAtEverySubmission",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("d4d1cf9d-c789-4ea4-93ee-1ead6f87495a", "Always check at every submission"),
						ResString.GetMultilingualString("4920afcd-097e-43b9-95ac-98f9789b416e", "Always check the credit standing before sending a message. Overrides all other settings in this category."),
						RegistryStorageFlags.Company,
						false);
					return result;
				});
			}
		}

		#endregion

		#region Various entries

		public IntRegistryItem IncrementalThresholdForChecking
		{
			get
			{
				return GetItem("IncrementalThresholdForChecking", delegate
				{
					var result = new IntRegistryItem(
						"IncrementalThresholdForChecking",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("ba250ba4-1825-467d-9754-6f10286e1a8b", "Incremental threshold for checking"),
						ResString.GetMultilingualString("d0452dee-4cd3-4256-8314-e23725d4ac78", "Perform the credit check on declarations that have an entry number and whose estimated duty/VAT at the time of submission is greater than the already recorded fees by this percentage. Set to a negative number to suppress the check."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						10);
					return result;
				});
			}
		}

		public BillCustomisationRegistryItem CorrelationIDCustomisation
		{
			get
			{
				return GetItem("CorrelationIDCustomisation", () => new BillCustomisationRegistryItem(
					"CorrelationIDCustomisation",
					Categories.Customs_France_CorrelationID,
					ResString.GetMultilingualString("245EB47D-E70B-4CC6-9963-B6D3F91AD9C6", "Correlation ID Customization"),
					ResString.GetMultilingualString("91C21CED-54D8-41D4-A8B4-70A2F2F3F84F", "Override this value to customize how Correlation ID are formatted"),
					RegistryStorageFlags.All,
					RegistryOptions.IsOnlyForSupport,
					new CorrelationIDCustomisationRegistryDataType()
				));
			}
		}

		public StringRegistryItem CINSenderID
		{
			get
			{
				return GetItem("FRCINSenderID", delegate
				{
					var result = new StringRegistryItem(
						"FRCINSenderID",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("922A7F81-F9F8-4945-A8EF-913010845DCB", "CIN Sender ID"),
						ResString.GetMultilingualString("172A56FE-3453-48EE-8893-87C122FD2418", "CIN Sender ID"),
						RegistryStorageFlags.Company,
						"PUT-CIN-ID-HERE"
					);
					return result;
				});
			}
		}

		public BillCustomisationRegistryItem FRCustomsFallbackEntryNumberCustomisation
		{
			get
			{
				return GetItem("FRCustomsFallbackEntryNumberCustomisation", () => new BillCustomisationRegistryItem(
					"FRCustomsFallbackEntryNumberCustomisation",
					Categories.Customs_France_FRCustomsFallbackEntryNumber,
					ResString.GetMultilingualString("915E0718-FBD5-4B5D-A5DF-222A1AAAFC29", "FR Customs Fallback Entry Number Customization	"),
					ResString.GetMultilingualString("D65AEAC0-BF53-4FDE-BC5C-508CFBC6145A", "Override this value to customize how FR Customs Fallback Entry Number are formatted"),
					RegistryStorageFlags.All,
					new FRCustomsFallbackEntryNumberCustomisationRegistryDataType()
				));
			}
		}

		public BillCustomisationRegistryItem FRStatementNumberCustomisation
		{
			get
			{
				return GetItem("FRStatementNumberCustomisation", () => new BillCustomisationRegistryItem(
					"FRStatementNumberCustomisation",
					Categories.Customs_France_Liquidation,
					ResString.GetMultilingualString("9B79F701-EAC2-4F35-8BEB-9E6A9ADE171B", "Statement Number Customization"),
					ResString.GetMultilingualString("8D0879D9-703B-4392-8114-1C5CB7884AAF", "Override this value to customize how Statement Job Numbers are formatted"),
					RegistryStorageFlags.Company,
					new StatementNumberCustomisationRegistryDataType()
				));
			}
		}

		#endregion

		#region DeltaD

		public IntRegistryItem NbDaysWaitBeforeSendingDeltaDStep2
		{
			get
			{
				return GetItem("FRNbDaysWaitBeforeSendingDeltaDStep2", delegate
				{
					var result = new IntRegistryItem(
						"FRNbDaysWaitBeforeSendingDeltaDStep2",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("71013430-E457-4784-94B9-A2708E7A0896", "Days to wait after BAE before automatically sending Delta G2 Second Step message"),
						ResString.GetMultilingualString("E6C2EA2C-57D2-4DF0-AA8A-C7817C32D710", "Days to wait after BAE before automatically sending Delta G2 Second Step message"),
						RegistryStorageFlags.Company,
						0
					);
					return result;
				});
			}
		}
		#endregion

		#region Fallback

		public static ZBool DeltaGFallbackIsActive
		{
			get
			{
				var deltaGFallbackDate = (FallbackSettings)Instance.DeltaGMode.Value;
				return (!deltaGFallbackDate.Start.IsEmpty
					&& deltaGFallbackDate.Start.IsInThePast()
					&& !deltaGFallbackDate.End.IsInThePast()
					);
			}
		}

		public static ZDateTime DeltaGFallbackInvocationDate => ((FallbackSettings)Instance.DeltaGMode.Value).Start;

		public static ZBool DeltaIFallbackIsActive
		{
			get
			{
				var deltaIFallbackDate = (FallbackSettings)Instance.DeltaIMode.Value;
				return (!deltaIFallbackDate.Start.IsEmpty
					&& deltaIFallbackDate.Start.IsInThePast()
					&& !deltaIFallbackDate.End.IsInThePast()
					);
			}
		}

		public static ZBool DeltaTFallbackIsActive
		{
			get
			{
				var deltaTFallbackDate = (FallbackSettings)Instance.DeltaTMode.Value;
				return (!deltaTFallbackDate.Start.IsEmpty
					&& deltaTFallbackDate.Start.IsInThePast()
					&& !deltaTFallbackDate.End.IsInThePast()
					);
			}
		}

		public IntRegistryItem FallbackTimerInMinutes
		{
			get
			{
				return GetItem("FRFallbackTimerInMinutes", delegate
				{
					return new IntRegistryItem(
						"FRFallbackTimerInMinutes",
						Categories.Customs_France_Fallback,
						ResString.GetMultilingualString("8ebad0ce-f2a3-4951-88c0-5db90a818512", "Fallback timer in minutes"),
						ResString.GetMultilingualString("8ebad0ce-f2a3-4951-88c0-5db90a818512", "Fallback timer in minutes"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						30);
				});
			}
		}
		public IntRegistryItem RegularisationTimerInMinutes
		{
			get
			{
				return GetItem("FRRegularisationTimerInMinutes", delegate
				{
					return new IntRegistryItem(
						"FRRegularisationTimerInMinutes",
						Categories.Customs_France_Fallback,
						ResString.GetMultilingualString("d57a0cf4-4d93-475a-8e23-bf72ff0412d4", "Regularization timer in minutes"),
						ResString.GetMultilingualString("d57a0cf4-4d93-475a-8e23-bf72ff0412d4", "Regularization timer in minutes"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						5);
				});
			}
		}

		#region Fallback Configuration
		/// <summary>
		/// PK of default group, e.g. postmasters groups
		/// </summary>
		public IRegistryItem CustomsFallbackConfiguration
		{
			get
			{
				return GetItem("CustomsFallbackConfiguration", delegate
				{
					var result = new FallbackSettingsRegistryItem(
						"CustomsFallbackConfiguration",
						Categories.Customs_France_Fallback,
						ResString.GetMultilingualString("4A66B578-0280-4B14-B53E-EFA4AD88BEF0", "Fallback Configuration (All)"),
						ResString.GetMultilingualString("798790BC-B400-4F59-AF0E-40D8AD5527E7", "Fallback Configuration (All)"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company
						);
					result.DataType = new FallbackSettingsRegistryItem.FallbackSettingsRegistryDataType();
					return result;
				});
			}
		}

		public IRegistryItem GetModeItem(ZString subItem, ResourceString humanName, IRegistryItem parentNotificationsItem)
		{
			var identifyingKey = parentNotificationsItem.Name + subItem;
			var parentNotificationsItemMultilingual = (IMultilingualRegistryItem)parentNotificationsItem;
			return GetItem(identifyingKey, delegate
			{
				return new RegistryItemImplWithDynamicDefaultValue
					(
						identifyingKey,
						CombineCategories(parentNotificationsItemMultilingual.CategoryMultilingual, ResString.GetMultilingualString("240d7496-6145-40f2-8a86-0cd7902e0a83", "Fallback Configuration (Per System)")),
						humanName,
						ResString.GetMultilingualString("2a57169b-9553-4b4c-b784-87a2d0593e2a", "Group for {0}", humanName),
						new FallbackSettingsRegistryItem.FallbackSettingsRegistryDataType(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => parentNotificationsItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK)
					);
			});
		}

		public IRegistryItem DeltaGMode
		{
			get { return GetModeItem("DeltaGMode", ResString.GetMultilingualString("6f8a5f24-c10d-44dd-8c7f-2311ee070794", "Delta G"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem DeltaIMode
		{
			get { return GetModeItem("DeltaIMode", ResString.GetMultilingualString("067041f0-b262-4c36-8f72-c74b944f05d3", "Delta I"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem DeltaTMode
		{
			get { return GetModeItem("DeltaTMode", ResString.GetMultilingualString("91730511-88bd-489d-9017-dab71b2dc0ee", "Delta T"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem GammaMode
		{
			get { return GetModeItem("GammaMode", ResString.GetMultilingualString("947a0554-3554-4751-95b9-c93c16475d25", "Gamma"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem DeltaXMode
		{
			get { return GetModeItem("DeltaXMode", ResString.GetMultilingualString("f5316941-100d-4414-abc9-61b2dfc153ae", "Delta X"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem ICSMode
		{
			get { return GetModeItem("ICSMode", ResString.GetMultilingualString("915be8df-6447-4417-9291-cf9571a1bbbc", "ICS"), CustomsFallbackConfiguration); }
		}
		public IRegistryItem ECSMode
		{
			get { return GetModeItem("ECSMode", ResString.GetMultilingualString("051b6442-ede3-46b5-abc5-6d2b7c2abbd6", "ECS"), CustomsFallbackConfiguration); }
		}

		#endregion

		#endregion

		#region Notifications

		public GuidRegistryItem FallbackRegularisationReportNotificationGroup
		{
			get
			{
				return GetItem("FallbackRegularisationReportNotificationGroup", () =>
				{
					var result = new GuidRegistryItem(
						"FallbackRegularisationReportNotificationGroup",
						FRCustomsDataRegistry.Categories.Customs_France_Notifications,
						ResString.GetMultilingualString("8f58edb8-e303-4c36-8bdf-25e5b7df8148", "Fallback Regularization Report Notification group"),
						ResString.GetMultilingualString("6040ce38-7f5a-47ab-9fda-63dcc0b56c55", "Fallback regularization report notification settings."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory,
						System.Guid.Empty);
					result.DataType = new NotificationGroupGuidRegistryDataType();
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments;
					return result;
				});
			}
		}

		public GuidRegistryItem BondedWarehouseNotificationGroup
		{
			get
			{
				return GetItem("BondedWarehouseNotificationGroup", () => new GuidRegistryItem(
					"BondedWarehouseNotificationGroup",
					Categories.Customs_France_Notifications,
					ResString.GetMultilingualString("5c0b7523-0bca-4ffd-b133-f24c82d1ea1c", "Bonded Warehouse Notification group"),
					ResString.GetMultilingualString("b113ca94-5703-408e-9f64-8f5f6eb70b0d", "Bonded Warehouse movements notification settings."),
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.IsValueMandatory,
					Guid.Empty)
				{
					DataType = new NotificationGroupGuidRegistryDataType(),
					CountryFilterPKs = CountryFilterPKs.FranceAndOverseasDepartments
				});
			}
		}

		public ManifestGroupNotificationRegistryItem DCGResponseNotificationGroup
		{
			get
			{
				return GetItem("DCGResponseNotificationGroup", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"DCGResponseNotificationGroup",
						Categories.Customs_France_Notifications,
						ResString.GetMultilingualString("9f1b01a4-fb09-4de1-8555-f5720acf68a8", "Liquidation (DCG) Responses"),
						ResString.GetMultilingualString("ec7468ac-88d8-4711-9910-db725c7d9a86", "Liquidation (DCG) response notification settings."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
						ManifestGroupNotification.Default);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments;
					return result;
				});
			}
		}

		public ManifestGroupNotificationRegistryItem DeltaTResponseNotificationGroup
		{
			get
			{
				return GetItem("DeltaTResponseNotificationGroup", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"DeltaTResponseNotificationGroup",
						Categories.Customs_France_Notifications,
						ResString.GetMultilingualString("ec62d40e-0297-4e19-83f6-14690a6d2438", "Transit (NCTS) Response"),
						ResString.GetMultilingualString("9b2426c6-dfb1-43bf-bce7-4847ee858b63", "Transit (NCTS) Response notification settings."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
						ManifestGroupNotification.Default);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments;
					return result;
				});
			}
		}

		public ManifestGroupNotificationRegistryItem DeltaGResponseNotificationGroup
		{
			get
			{
				return GetItem("DeltaGResponseNotificationGroup", delegate
				{
					var result = new ManifestGroupNotificationRegistryItem(
						"DeltaGResponseNotificationGroup",
						Categories.Customs_France_Notifications,
						ResString.GetMultilingualString("05ea0710-5c67-4eb0-9659-faf3b18ec789", "Delta G Response"),
						ResString.GetMultilingualString("0f271028-e71b-4243-ae93-00d1b22cfcfc", "Delta G Response notification settings."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter | RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory,
						ManifestGroupNotification.Default);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.FranceAndOverseasDepartments;
					return result;
				});
			}
		}

		#endregion

		#region FR VAT Report

		public DateTimeRegistryItem FRVATReportLastRunTime
		{
			get
			{
				return GetItem("FRVATReportLastRunTime", delegate
				{
					var result = new DateTimeRegistryItem(
						"FRVATReportLastRunTime",
						FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
						ResString.GetMultilingualString("6C814BF7-8577-44C6-A8B5-5B904466E6A8", "FR VAT report last run time"),
						ResString.GetMultilingualString("8C90CB69-4278-4EDC-B2C3-540871E4059B", "Indicate the last time the FR VAT Report has been running."),
						RegistryStorageFlags.Company
						);
					return result;
				});
			}
		}

		public IntRegistryItem FRVATReportStaggeringFactor
		{
			get
			{
				return GetItem("FRVATReportStaggeringFactor", delegate
				{
					var result = new IntRegistryItem(
						"FRVATReportStaggeringFactor",
						FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
						ResString.GetMultilingualString("0FB2186F-C41D-41FA-BC10-CA9C0FFACD70", "FR VAT report staggering factor"),
						ResString.GetMultilingualString("51D6FE95-B75F-4D62-BEF2-FF1E71A4FAC1", "Staggering factor for France VAT report."),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						1, 0, int.MaxValue);
					return result;
				});
			}
		}

		public IntRegistryItem FRVATReportDeadlineDayOfTheMonth
		{
			get
			{
				return GetItem("FRVATReportDeadlineDayOfTheMonth", delegate
				{
					var result = new IntRegistryItem(
						"FRVATReportDeadlineDayOfTheMonth",
						FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
						ResString.GetMultilingualString("298AE9FD-C1CA-4018-B5D1-E6F8A19430F6", "FR VAT report deadline day of the month"),
						ResString.GetMultilingualString("0C0A72D4-8739-4BDC-9759-E62FF2C1F39B", "Deadline day of the month to perform the VAT Report for France."),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						23, 3, 31);
					return result;
				});
			}
		}

		public BooleanRegistryItem RunFRVATReportInUAT
		{
			get
			{
				return GetItem("RunFRVATReportInUAT", delegate
				{
					var result = new BooleanRegistryItem(
						"RunFRVATReportInUAT",
						FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
						ResString.GetMultilingualString("DD65B9C5-BA30-440C-ACE8-F599EA7227B5", "Should the service task FRV be allowed to run in UAT?"),
						ResString.GetMultilingualString("DD65B9C5-BA30-440C-ACE8-F599EA7227B5", "Should the service task FRV be allowed to run in UAT?"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
					return result;
				});
			}
		}

		public CodePairRegistryItem FRVATReportConfiguration
		{
			get
			{
				return GetItem("FRVATReportConfiguration", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(VATReportConfigurationLookups.ConfigurationLookups);
						return list;
					});

					var result = new CodePairRegistryItem(
						"FRVATReportConfiguration",
						FRCustomsDataRegistry.Categories.Customs_France_FRVATReport,
						ResString.GetMultilingualString("07FB778F-F077-4B39-B129-96F7033E1009", "FR VAT Report configuration name"),
						ResString.GetMultilingualString("AC247B4C-7DD7-4697-8297-56DB207BC644", "Configuration that applies when executing the automatic FR VAT report."),
						listProvider,
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory,
						null);

					result.EditorInfo = new ComboBoxRegistryEditorInfo(listProvider, false);
					return result;
				});
			}
		}

		public static Guid FRVATReportPK = new Guid("614f1f22-156e-4099-a8e0-97b562bad075");

		#endregion

		#region Delta I/E

		public BooleanRegistryItem EnableDeltaIEForImports
		{
			get
			{
				return GetItem("EnableDeltaIEForImports", delegate
				{
					return new BooleanRegistryItem(
						"EnableDeltaIEForImports",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("001EF0FD-22E4-47D4-BE84-7DB80EDA0391", "Enable Delta I/E for Imports"),
						ResString.GetMultilingualString("E862BA51-4A95-4643-A7EE-91D166B02EF3", "Enable Delta I/E for Imports?"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						false
					);
				});
			}
		}

		public BooleanRegistryItem EnableDeltaIEForExports
		{
			get
			{
				return GetItem("EnableDeltaIEForExports", delegate
				{
					return new BooleanRegistryItem(
						"EnableDeltaIEForExports",
						FRCustomsDataRegistry.Categories.Customs_France,
						ResString.GetMultilingualString("EAADFC9E-9B60-4FBA-A395-76E6359D3586", "Enable Delta I/E for Exports"),
						ResString.GetMultilingualString("4225D8A0-A4BD-4895-8B32-6B380194E99E", "Enable Delta I/E for Exports?"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						false
					);
				});
			}
		}

		#endregion

		#region Auto Send Messages

		public TriggerPointsConfigurationRegistryItem TriggerPointsConfiguration
		{
			get
			{
				return GetItem("FRTriggerPointsConfiguration", delegate
				{
					var result = new TriggerPointsConfigurationRegistryItem(
						"FRTriggerPointsConfiguration",
						FRCustomsDataRegistry.Categories.Customs_France_AutoSendMessages,
						ResString.GetMultilingualString("B84399A7-76B1-4510-B300-C6325B175745", "Trigger points for automated validation"),
						ResString.GetMultilingualString("460D953D-D614-4BC3-889C-2D6B0632F438", "Set here the events expected to trigger the auto validation of messages to Customs"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						new TriggerPointsConfiguration()
						{
							ImportTriggerPoint = TriggerPointsCodeList.Codes.NUL,
							ExportTriggerPoint = TriggerPointsCodeList.Codes.NUL
						}
					);
					return result;
				});
			}
		}

		public AutomatedModificationRegistryItem FRAutomatedModification
		{
			get
			{
				return GetItem("AutomatedModification", delegate
				{
					var result = new AutomatedModificationRegistryItem(
						"AutomatedModification",
						FRCustomsDataRegistry.Categories.Customs_France_AutoSendMessages,
						ResString.GetMultilingualString("411B848A-FFEC-423C-8293-60D221389E5D", "Date for Duty automated modification."),
						ResString.GetMultilingualString("F9D95851-F0FD-4121-9E08-00B0FCF891B9", "Choose here to enable automated modification of the time at which to advance the date for duty of declaration 24h later."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						new AutomatedModification()
					);
					return result;
				});
			}
		}
		#endregion

		#region Exit Control System

		public StringRegistryItem DocumentTypeAsAlternateProofOfExit
		{
			get
			{
				return GetItem("FRDocumentTypeAsAlternateProofOfExit", () => new StringRegistryItem(
					"FRDocumentTypeAsAlternateProofOfExit",
					Categories.Customs_France_ExitControlSystem,
					ResString.GetMultilingualString("0E4EBBF4-E4C9-44F7-98D9-5D3A136BE351", "Document Type as Alternate Proof of Exit"),
					ResString.GetMultilingualString("7D00F281-17C7-4044-8EBD-C27F69BAB822", "Select a document type as Alternate Proof of Exit."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					string.Empty
				)
				{
					EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.RefDocType, factory => new RefDocTypeCollection(factory))
				});
			}
		}

		#endregion

		#region SAD

		public BooleanRegistryItem SADGenerationOnConfirmedExitEnabled
		{
			get
			{
				return GetItem("SADGenerationOnConfirmedExitEnabled", delegate
				{
					var result = new BooleanRegistryItem(
						"SADGenerationOnConfirmedExitEnabled",
						Categories.Customs_France,
						ResString.GetMultilingualString("8A4ED857-1C51-492D-93A5-3D6643B292A4", "SAD Generation on Confirmed Exit Enabled"),
						ResString.GetMultilingualString("4CBE3654-7FA2-4F01-A5BE-6159D92CB365", "Set to YES to enable SAD Generation on Confirmed Exit."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true
					);
					return result;
				});
			}
		}

		#endregion SAD
	}
}

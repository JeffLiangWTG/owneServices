using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client
{
	public sealed class JASDataRegistry : RegistryItemSet
	{
		JASDataRegistry()
		{
		}

		public static JASDataRegistry Instance
		{
			get { return instance ?? (instance = new JASDataRegistry()); }
		}
		[ThreadStatic]
		static JASDataRegistry instance;

		public override bool IsForProductivityWise => false;

		const string Category = "JAS Client Extensions";

		#region JXC Messaging

		#region JXCOutgoingDirectoryName

		public ZString JXCOutgoingDirectoryName
		{
			get { return new ZString(JXCOutgoingDirectoryNameItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)JXCOutgoingDirectoryNameItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		StringRegistryItem JXCOutgoingDirectoryNameItem
		{
			get
			{
				return GetItem("JXCOutgoingDirectoryName", delegate
				{
					string @default = string.Format("{0}{1}JXC{1}JCVI{1}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), Path.DirectorySeparatorChar);
					StringRegistryItem result = new StringRegistryItem("JXCOutgoingDirectoryName", (NoResString)JXCCategory, (NoResString)"Outgoing JXC Messages Dump Directory", null, RegistryStorageFlags.Company, @default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region JXCIncomingDirectoryNames

		public ZString JXCIncomingDirectoryNames
		{
			get { return new ZString(JXCIncomingDirectoryNamesItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)JXCIncomingDirectoryNamesItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		StringRegistryItem JXCIncomingDirectoryNamesItem
		{
			get
			{
				return GetItem("JXCIncomingDirectoryNames", delegate
				{
					string @default = string.Format("{0}{1}JXC{1}", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), Path.DirectorySeparatorChar);
					string hint = "Please enter the parent folder of the incoming messages directory. You could specify more than one directory by entering multiple lines.";
					StringRegistryItem result = new StringRegistryItem("JXCIncomingDirectoryNames", (NoResString)JXCCategory, (NoResString)"Incoming JXC Messages Directories", (NoResString)hint, RegistryStorageFlags.Company, @default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region JXCMessageArchiveDirectoryName

		public ZString JXCMessagesArchiveDirectoryName
		{
			get { return new ZString(JXCMessagesArchiveDirectoryNameItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)JXCMessagesArchiveDirectoryNameItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		StringRegistryItem JXCMessagesArchiveDirectoryNameItem
		{
			get
			{
				return GetItem("JXCMessagesArchiveDirectoryName", delegate
				{
					StringRegistryItem result = new StringRegistryItem("JXCMessagesArchiveDirectoryName", (NoResString)JXCCategory, (NoResString)"JXC Messages Archive Directory", (NoResString)"Outgoing and incoming JXC messages are archived into this directory", RegistryStorageFlags.Company);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region EnableAutoJXCMessaging

		public ZBool EnableAutoJXCMessaging
		{
			get { return EnableAutoJXCMessagingItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EnableAutoJXCMessagingItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (bool)value); }
		}

		BooleanRegistryItem EnableAutoJXCMessagingItem
		{
			get
			{
				return GetItem("EnableAutoJXCMessaging", delegate
				{
					return new BooleanRegistryItem("EnableAutoJXCMessaging", (NoResString)JXCCategory, (NoResString)"Enable Automatic JXC Messaging", (NoResString)"This enables automatic exporting of JXC messages when certain events occurred (I.e. Printing of MAWB)", RegistryStorageFlags.Company, true);
				});
			}
		}

		#endregion

		#region QueryUserForDirectoryOnManualJXCExport

		public ZBool QueryUserForDirectoryOnManualJXCExport
		{
			get { return QueryUserForDirectoryOnManualJXCExportItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)QueryUserForDirectoryOnManualJXCExportItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (bool)value); }
		}

		BooleanRegistryItem QueryUserForDirectoryOnManualJXCExportItem
		{
			get
			{
				return GetItem("QueryUserForDirectoryOnManualJXCExport", delegate
				{
					return new BooleanRegistryItem("QueryUserForDirectoryOnManualJXCExport", (NoResString)JXCCategory, (NoResString)"Query User For Directory On Manual JXC Export", (NoResString)"This enables user to select a directory where the JXC message is going to be exported to. If this is disabled, export files will go to the default export directory.", RegistryStorageFlags.Company, true);
				});
			}
		}

		#endregion

		#region DefaultChargeCodeForAccrualsImport

		public GuidRegistryItem DefaultChargeCodeForAccrualsImportItem
		{
			get
			{
				return GetItem("DefaultChargeCodeForAccrualsImport", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("DefaultChargeCodeForAccrualsImport", (NoResString)JXCCategory, (NoResString)"Default Charge Code for Accruals Import", (NoResString)"Please enter a default charge code for importing accruals when charge code cannot be matched", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
					return result;
				});
			}
		}

		#endregion

		#region EnableAutoUpdateOnImport

		public ZBool EnableAutoUpdateOnImport
		{
			get { return EnableAutoUpdateOnImportItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EnableAutoUpdateOnImportItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (bool)value); }
		}

		BooleanRegistryItem EnableAutoUpdateOnImportItem
		{
			get
			{
				return GetItem("EnableAutoUpdateOnImport", delegate
				{
					string hint = "If this is enabled, existing consols and/or shipments will be updated during import. If this is disabled (default behaviour), JXC Messages for existing consols and/or shipments will be ignored.";
					return new BooleanRegistryItem("EnableAutoUpdateOnImport", (NoResString)JXCCategory, (NoResString)"Enable Automatic Update on JXC Messaging Import", (NoResString)hint, RegistryStorageFlags.Company, false);
				});
			}
		}

		#endregion

		#region ProfitSplitDueDestinationPercentage

		public ZInt ProfitSplitDueDestinationPercentage
		{
			get { return ProfitSplitDueDestinationPercentageItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ProfitSplitDueDestinationPercentageItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)value); }
		}

		IntRegistryItem ProfitSplitDueDestinationPercentageItem
		{
			get
			{
				return GetItem("ProfitSplitDueDestinationPercentage", delegate
				{
					return new IntRegistryItem("ProfitSplitDueDestinationPercentage", (NoResString)JXCCategory, (NoResString)"Profit Split Due Destination Percentage. This is used when exporting JXC Air Profit Share messages.", null, new NumericRegistryEditorInfo(0), RegistryStorageFlags.System, RegistryOptions.Default, 50, 0, 100);
				});
			}
		}

		#endregion

		#region DisableGsumMessaging

		public ZBool DisableGsumMessaging
		{
			get { return DisableGsumMessagingItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)DisableGsumMessagingItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (bool)value); }
		}

		BooleanRegistryItem DisableGsumMessagingItem
		{
			get
			{
				return GetItem("DisableGsumMessaging", delegate
				{
					return new BooleanRegistryItem("DisableGsumMessaging", (NoResString)JXCCategory, (NoResString)"Disable GSUM Messaging", (NoResString)"This is a developer-only trigger to disable GSUM messaging for troubleshooting / emergency purposes", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForDevelopers, false);
				});
			}
		}

		#endregion

		#region IncludeProfitShareCharges

		public ZBool IncludeProfitShareCharges
		{
			get { return IncludeProfitShareChargesItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)IncludeProfitShareChargesItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (bool)value); }
		}

		BooleanRegistryItem IncludeProfitShareChargesItem
		{
			get
			{
				return GetItem("IncludeProfitShareCharges", delegate
				{
					return new BooleanRegistryItem("IncludeProfitShareCharges", (NoResString)JXCCategory, (NoResString)"Include Profit Share Charges In Ocean Shipment Message", null, RegistryStorageFlags.Company, false);
				});
			}
		}

		#endregion

		const string JXCCategory = Category + "/JXC";

		#endregion

		#region Notification Groups

		#region JXCAirMessagingNotificationGroup

		public ZGuid JXCAirMessagingNotificationGroupPK
		{
			get { return new ZGuid(JXCAirMessagingNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { JXCAirMessagingNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		internal GuidRegistryItem JXCAirMessagingNotificationGroupItem
		{
			get
			{
				return GetItem("JXCAirMessagingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JXCAirMessagingNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"JXC Air Messaging", (NoResString)"Please enter the group to send JXC Air Messaging notifications to", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region JXCOceanMessagingNotificationGroup

		public ZGuid JXCOceanMessagingNotificationGroupPK
		{
			get { return new ZGuid(JXCOceanMessagingNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { JXCOceanMessagingNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		internal GuidRegistryItem JXCOceanMessagingNotificationGroupItem
		{
			get
			{
				return GetItem("JXCOceanMessagingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JXCOceanMessagingNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"JXC Ocean Messaging", (NoResString)"Please enter the group to send JXC Ocean Messaging notifications to", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region JXCFinancialMessagingNotificationGroup

		public ZGuid JXCFinancialMessagingNotificationGroupPK
		{
			get { return new ZGuid(JXCFinancialMessagingNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { JXCFinancialMessagingNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		internal GuidRegistryItem JXCFinancialMessagingNotificationGroupItem
		{
			get
			{
				return GetItem("JXCFinancialMessagingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JXCFinancialMessagingNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"JXC Financial Messaging", (NoResString)"Please enter the group to send JXC Financial Messaging notifications to", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region JXCMiscMessagingNotificationGroup

		public ZGuid JXCMiscMessagingNotificationGroupPK
		{
			get { return new ZGuid(JXCMiscMessagingNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { JXCMiscMessagingNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		internal GuidRegistryItem JXCMiscMessagingNotificationGroupItem
		{
			get
			{
				return GetItem("JXCMiscMessagingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JXCMiscMessagingNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"JXC Miscellaneous Messaging", (NoResString)"Please enter the group to send JXC Miscellaneous Messaging notifications to", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region JXCGeneralNotificationGroup

		public ZGuid JXCGeneralNotificationGroupPK
		{
			get { return new ZGuid(JXCGeneralNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { JXCGeneralNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		GuidRegistryItem JXCGeneralNotificationGroupItem
		{
			get
			{
				return GetItem("JXCGeneralNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JXCGeneralNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"JXC General Issues", (NoResString)"Please enter the group to send JXC General Issues notifications to. Any issues which are not related to a specific message type will be sent to this group.", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region AccountingNotificationGroup

		public ZGuid AccountingNotificationGroupPK
		{
			get { return new ZGuid(AccountingNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { AccountingNotificationGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		GuidRegistryItem AccountingNotificationGroupItem
		{
			get
			{
				return GetItem("AccountingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("AccountingNotificationGroup", (NoResString)NotificationGroupsCategory, (NoResString)"Accounting", (NoResString)"Please enter the group to send Accounting notifications to", RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		public ZGuid GetNotificationGroupPKFromMessageCategory(JXCConstants.MessageCategories messageCategory)
		{
			return GetNotificationGroupFromMessageCategory(messageCategory).GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		internal GuidRegistryItem GetNotificationGroupFromMessageCategory(JXCConstants.MessageCategories messageCategory)
		{
			GuidRegistryItem result;
			switch (messageCategory)
			{
				case JXCConstants.MessageCategories.Air:
					result = JXCAirMessagingNotificationGroupItem;
					break;

				case JXCConstants.MessageCategories.Ocean:
					result = JXCOceanMessagingNotificationGroupItem;
					break;

				case JXCConstants.MessageCategories.Financial:
					result = JXCFinancialMessagingNotificationGroupItem;
					break;

				case JXCConstants.MessageCategories.Miscellaneous:
					result = JXCMiscMessagingNotificationGroupItem;
					break;

				default:
					result = JXCGeneralNotificationGroupItem;
					break;
			}
			return result;
		}

		const string NotificationGroupsCategory = Category + "/Notification Groups";

		#endregion

		#region JASWW Organisation

		public JASOrgHeader GetJASWWOrganisation(BusinessObjectFactory factory)
		{
			return factory.Load<JASOrgHeader>(JASWWOrganisationPK);
		}

		public ZGuid JASWWOrganisationPK
		{
			get { return new ZGuid(JASWWOrganisationItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
		}

		internal GuidRegistryItem JASWWOrganisationItem
		{
			get
			{
				return GetItem("JASWWOrganisation", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JASWWOrganisation", (NoResString)Category, (NoResString)"JASWW Organisation", (NoResString)"JASWW Organisation where Netting and JXC Messages are sent to. This organisation is also used for mapping things like Container Type, JAS SSL Code, etc...", RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion

		#region Account Matching

		public bool UseJobInvoiceNumberAsPreMatchingInvoiceRef
		{
			get { return UseJobInvoiceNumberAsPreMatchingInvoiceRefItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)UseJobInvoiceNumberAsPreMatchingInvoiceRefItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		BooleanRegistryItem UseJobInvoiceNumberAsPreMatchingInvoiceRefItem
		{
			get
			{
				return GetItem("UseJobInvoiceNumberAsPreMatchingInvoiceRef", delegate
				{
					return new BooleanRegistryItem("UseJobInvoiceNumberAsPreMatchingInvoiceRef", (NoResString)NettingCategory, (NoResString)"Use Job Invoice Number as Pre-Matching Invoice Ref", (NoResString)"If this is set to false, the system will use the auto-generated transaction number as the Invoice Ref", RegistryStorageFlags.Company, true);
				});
			}
		}

		public int NettingPaymentTerms
		{
			get { return NettingPaymentTermsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)NettingPaymentTermsItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public IntRegistryItem NettingPaymentTermsItem
		{
			get
			{
				return GetItem("NettingPaymentTerms", delegate
				{
					return new IntRegistryItem("NettingPaymentTerms", (NoResString)NettingCategory, (NoResString)"Netting Payment Terms",
						(NoResString)"The date of shipment departure will drive the aging of the invoice. Please specify the number of days after EOM of the Departure Date to calculate the Maturity Date",
						RegistryStorageFlags.System, 45);
				});
			}
		}

		#region Netting Cycle List

		public CodeDescriptionPairListRegistryItem NettingCycleListItem
		{
			get
			{
				return GetItem("NettingCycleList", delegate
				{
					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem("NettingCycleList", (NoResString)NettingCategory,
						(NoResString)"Netting Cycle list for Pre-Matching export",
						(NoResString)"Please enter the Netting Cycle Date list in \"dd-MMM-yy\" format. I.e. 05-JAN-05",
						9, RegistryStorageFlags.System, false, DefaultNettingCycleList);
					result.EditorInfo = new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, (NoResString)"Netting Cycle Date");
					result.DataType = new NettingCycleListRegistryDataType();
					return result;
				});
			}
		}

		CodeDescriptionPairList DefaultNettingCycleList
		{
			get
			{
				if (fDefaultNettingCycleList == null)
				{
					fDefaultNettingCycleList = new CodeDescriptionPairList();
					fDefaultNettingCycleList.AddPair("10-DEC-04");
					fDefaultNettingCycleList.AddPair("11-JAN-05");
					fDefaultNettingCycleList.AddPair("08-FEB-05");
					fDefaultNettingCycleList.AddPair("09-MAR-05");
					fDefaultNettingCycleList.AddPair("11-APR-05");
					fDefaultNettingCycleList.AddPair("11-MAY-05");
					fDefaultNettingCycleList.AddPair("10-JUN-05");
					fDefaultNettingCycleList.AddPair("11-JUL-05");
					fDefaultNettingCycleList.AddPair("10-AUG-05");
					fDefaultNettingCycleList.AddPair("12-SEP-05");
					fDefaultNettingCycleList.AddPair("11-OCT-05");
					fDefaultNettingCycleList.AddPair("10-NOV-05");
					fDefaultNettingCycleList.AddPair("09-DEC-05");
				}
				return fDefaultNettingCycleList;
			}
		}

		CodeDescriptionPairList fDefaultNettingCycleList;

		#endregion

		const string NettingCategory = Category + "/Netting";

		#endregion

		#region Cognos

		#region Mode Mapping

		public CognosModeMapping CognosModeMapping
		{
			get { return CognosModeMappingItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public CognosModeMappingRegistryItem CognosModeMappingItem
		{
			get
			{
				return GetItem("CognosModeMapping", delegate
				{
					return new CognosModeMappingRegistryItem(CognosCategory);
				});
			}
		}

		#endregion

		#region Cognos Sales Group

		public ZGuid CognosSalesGroup
		{
			get { return new ZGuid(CognosSalesGroupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { CognosSalesGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		GuidRegistryItem CognosSalesGroupItem
		{
			get
			{
				return GetItem("CognosSalesGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CognosSalesGroup", (NoResString)CognosCategory, (NoResString)"Cognos Sales Group for Headcount Statistics", (NoResString)"Please enter the group to be used when exporting Headcount for Sales records", RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Cognos Administration Group

		public ZGuid CognosAdminGroup
		{
			get { return new ZGuid(CognosAdminGroupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { CognosAdminGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		GuidRegistryItem CognosAdminGroupItem
		{
			get
			{
				return GetItem("CognosAdminGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CognosAdminGroup", (NoResString)CognosCategory, (NoResString)"Cognos Administration Group for Headcount Statistics", (NoResString)"Please enter the group to be used when exporting Headcount for Administration records", RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Cognos Shipment Control Group

		public ZGuid CognosShipmentControlGroup
		{
			get { return new ZGuid(CognosShipmentControlGroupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { CognosShipmentControlGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		GuidRegistryItem CognosShipmentControlGroupItem
		{
			get
			{
				return GetItem("CognosShipmentControlGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CognosShipmentControlGroup", (NoResString)CognosCategory, (NoResString)"Cognos Shipment Control Group for Headcount Statistics", (NoResString)"Please enter the group to be used when exporting Headcount for Shipment Control records", RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		const string CognosCategory = Category + "/Cognos";

		#endregion

		#region BOLBondNum

		public ZString BOLBondNum
		{
			get { return new ZString(BOLBondNumItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)BOLBondNumItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		StringRegistryItem BOLBondNumItem
		{
			get
			{
				return GetItem("BOLBondNum", delegate
				{
					return new StringRegistryItem("BOLBondNum", (NoResString)BOLCategory, (NoResString)"BOND Number", (NoResString)@"The text entered in this field will print on the document after “BILL OF LADING”", RegistryStorageFlags.Company, "- BOND # 990404");
				});
			}
		}

		const string BOLCategory = Category + "/JAS Bill of Lading";

		#endregion
	}
}

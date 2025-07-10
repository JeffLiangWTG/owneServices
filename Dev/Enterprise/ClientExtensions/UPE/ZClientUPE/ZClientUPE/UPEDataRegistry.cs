using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE
{
	public sealed partial class UPEDataRegistry : RegistryItemSet
	{
		public static UPEDataRegistry Instance
		{
			get { return instance ?? (instance = new UPEDataRegistry()); }
		}
		[ThreadStatic]
		static UPEDataRegistry instance;

		public override bool IsForProductivityWise => false;

		Guid companyPK
		{
			get
			{
				return (Env.CurrentCompany == null) ? Guid.Empty : Env.CurrentCompany.PK;
			}
		}

		#region Enable UPE Customisations

		public bool EnableUPECustomisations
		{
			get { return EnableUPECustomisationsItem.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EnableUPECustomisationsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}

		public BooleanRegistryItem EnableUPECustomisationsItem
		{
			get
			{
				return GetItem("EnableUPECustomisations", delegate
				{
					return new BooleanRegistryItem(
						"EnableUPECustomisations",
						(NoResString)Category,
						(NoResString)"Enable UPE Customisations",
						(NoResString)"Set this value to 'Yes' to enable UPE Customisations.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public Guid BranchToUseForUPECustomisations
		{
			get { return BranchToUseForUPECustomisationsItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BranchToUseForUPECustomisationsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}

		public GuidRegistryItem BranchToUseForUPECustomisationsItem
		{
			get
			{
				return GetItem("BISIUploadBranchToUseItem", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("BISIUploadBranchToUseItem",
						(NoResString)Category,
						(NoResString)"Branch to use for UPE Customisations",
						(NoResString)"Required for each company with UPE customisations enabled, time zone of selected branch will be used",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
					return result;
				});
			}
		}

		#endregion

		#region GSSi

		const string GSSiCategory = Category + "/GSSi";
		const string RefundEnquiryCategory = Category + "/Refund Enquiry";

		#region Branch Code IDs

		public ZString GetBuildingIDByPortOfArrival(ZGuid portPK)
		{
			ZString result = ZString.Empty;
			if (BranchCodeIDs.Value.FindByPort(portPK) != null)
			{
				result = BranchCodeIDs.Value.FindByPort(portPK).BuildingID;
			}
			return result;
		}

		internal UPEBranchIDsRegistryItem BranchCodeIDs
		{
			get
			{
				return GetItem("BranchCodeIDs", delegate
				{
					return new UPEBranchIDsRegistryItem(
						(NoResString)"BranchCodeIDs",
						(NoResString)"Branch Code IDs",
						null,
						RegistryStorageFlags.System,
						new UPEBranchIDsRegistryObjectCollection(),
						new MultilingualString[] { (NoResString)GSSiCategory });
				});
			}
		}

		#endregion

		#region Default Building ID

		public ZString DefaultBuildingID
		{
			get { return DefaultBuildingIDItem.Value; }
			set { ((IRegistryItem)DefaultBuildingIDItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal StringRegistryItem DefaultBuildingIDItem
		{
			get
			{
				return GetItem("UPEDefaultBuildingIDItem", delegate
				{
					return new StringRegistryItem(
						"UPEDefaultBuildingIDItem",
						(NoResString)GSSiCategory,
						(NoResString)"Default Building ID",
						null,
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#endregion

		#region DogHitXRayNotificationGroup

		public Guid DogHitXRayNotificationGroup
		{
			get { return DogHitXRayNotificationGroupItem.Value; }
			set { DogHitXRayNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem DogHitXRayNotificationGroupItem
		{
			get
			{
				return GetItem("DogHitXRayNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DogHitXRayNotificationGroup",
						(NoResString)Category,
						(NoResString)"Dog Hit or X-Ray Notification Group",
						(NoResString)"The staff group code for staff that will receive Dog Hit or X-Ray Hold warning emails",
						RegistryStorageFlags.System,
						DefaultNotificationGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region PartPaymentNotificationGroup

		public Guid PartPaymentNotificationGroup
		{
			get { return (Guid)PartPaymentNotificationGroupItem.Value; }
			set { PartPaymentNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IRegistryItem PartPaymentNotificationGroupItem
		{
			get
			{
				return GetItem("PartPaymentNotificationGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"PartPaymentNotificationGroup",
						(NoResString)Category,
						(NoResString)"Part Payment Notification Group",
						(NoResString)"The staff group code for staff that will receive part payment emails",
						RegistryStorageFlags.System,
						DefaultNotificationGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Refund Enquiry

		#region AtFault Groups
		public UPEGlbGroupsRegistryItem AtFaultGroupsItem
		{
			get
			{
				return GetItem("AtFaultGroups", delegate
				{
					UPEGlbGroupsRegistryObjectCollection collection = new UPEGlbGroupsRegistryObjectCollection();
					UPEGlbGroupsRegistryObject defaultValue = collection.AddNew();
					defaultValue.Group = Core.Constants.Groups.AllPK;
					return new UPEGlbGroupsRegistryItem(
						"AtFaultGroups",
						(NoResString)"At Fault Groups to Filter",
						(NoResString)"At Fault Groups to Filter",
						RegistryStorageFlags.System,
						collection,
						(NoResString)RefundEnquiryCategory);
				});
			}
		}
		#endregion

		#region RefundNotificationGroup

		public Guid RefundNotificationGroup
		{
			get { return (Guid)RefundNotificationGroupItem.Value; }
			set { RefundNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public IRegistryItem RefundNotificationGroupItem
		{
			get
			{
				return GetItem("RefundNotificationGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"RefundNotificationGroup",
						(NoResString)RefundEnquiryCategory,
						(NoResString)"Refund Notification Group",
						(NoResString)"The staff group code for staff that will receive refund emails",
						RegistryStorageFlags.System,
						DefaultNotificationGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region CreditNotificationGroup
		public Guid CreditNotificationGroup
		{
			get { return (Guid)CreditNotificationGroupItem.Value; }
			set { CreditNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IRegistryItem CreditNotificationGroupItem
		{
			get
			{
				return GetItem("CreditNotificationGroup", delegate
				{
					IRegistryItem result = new GuidRegistryItem(
						"CreditNotificationGroup",
						(NoResString)Category,
						(NoResString)"Credit Notification Group",
						(NoResString)"The staff group code for staff that will receive Electronic Credit Note, Entry Print, Tax Invoice, Commercial Invoice",
						RegistryStorageFlags.System,
						DefaultNotificationGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}
		#endregion

		#endregion

		#region MAWBFirstMovedToClassificationNotificationGroup
		public Guid MAWBFirstMovedToClassificationNotificationGroup
		{
			get { return MAWBFirstMovedToClassificationNotificationGroupItem.Value; }
			set { MAWBFirstMovedToClassificationNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public GuidRegistryItem MAWBFirstMovedToClassificationNotificationGroupItem
		{
			get
			{
				return GetItem("MAWBFirstMovedToClassificationNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"MAWBFirstMovedToClassificationNotificationGroup",
						(NoResString)Category,
						(NoResString)"MAWB First Moved To CLS Notification Group",
						(NoResString)"The staff group code for staff that will receive MAWB moved to CLS queue emails",
						RegistryStorageFlags.System,
						DefaultNotificationGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}
		#endregion

		#region ManualbillNotificationGroup

		public Guid ManualbillNotificationGroup
		{
			get { return ManualbillNotificationGroupItem.Value; }
			set { ManualbillNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem ManualbillNotificationGroupItem
		{
			get
			{
				return GetItem("ManualbillNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ManualbillNotificationGroup",
						(NoResString)Category,
						(NoResString)"Manual Bill Notification Group",
						(NoResString)"The staff group code for staff that will receive Manual Bill request notification emails",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DefaultBISIWarningReportGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region SFTP Server Timeout

		public ZInt SftpServerTimeout
		{
			get { return SftpServerTimeoutItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)SftpServerTimeoutItem).SetValue(companyPK, Guid.Empty, Guid.Empty, (int)value); }
		}

		internal IntRegistryItem SftpServerTimeoutItem
		{
			get
			{
				return GetItem("SftpServerTimeout", delegate
				{
					return new IntRegistryItem(
						"SftpServerTimeout",
						(NoResString)Category,
						(NoResString)"SFTP Server Timeout",
						(NoResString)"Timeout in seconds applied to ZU1-4 service tasks",
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						3600);
				});
			}
		}

		public Guid SftpServerTimeoutItemNotificationGroup
		{
			get { return SftpServerTimeoutItemNotificationGroupItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { SftpServerTimeoutItemNotificationGroupItem.SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem SftpServerTimeoutItemNotificationGroupItem
		{
			get
			{
				return GetItem("SftpServerTimeoutNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SftpServerTimeoutNotificationGroup",
						(NoResString)Category,
						(NoResString)"SFTP Server Timeout Notification Group",
						null,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region BISI

		#region Common

		#region BISISftpServerAddress

		public ZString BISISftpServerAddress
		{
			get { return BISISftpServerAddressItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISISftpServerAddressItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISISftpServerAddressItem
		{
			get
			{
				return GetItem("BISIFtpServerAddress", delegate
				{
					return new StringRegistryItem("BISIFtpServerAddress", (NoResString)BISICategory, (NoResString)"SFTP Server Address", null, RegistryStorageFlags.Company | RegistryStorageFlags.System, "10.162.32.93");
				});
			}
		}

		#endregion

		#region BISISfTpServerPort

		public ZInt BISISftpServerPort
		{
			get { return BISISftpServerPortItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISISftpServerPortItem).SetValue(companyPK, Guid.Empty, Guid.Empty, (int)value); }
		}

		internal IntRegistryItem BISISftpServerPortItem
		{
			get
			{
				return GetItem("BISIFtpServerPort", delegate
				{
					return new IntRegistryItem("BISIFtpServerPort", (NoResString)BISICategory, (NoResString)"SFTP Server Port", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, 22);
				});
			}
		}

		#endregion

		#region BISISftpServerUsername

		public ZString BISISftpServerUsername
		{
			get { return BISISftpServerUsernameItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISISftpServerUsernameItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISISftpServerUsernameItem
		{
			get
			{
				return GetItem("BISIFtpServerUsername", delegate
				{
					return new StringRegistryItem("BISIFtpServerUsername", (NoResString)BISICategory, (NoResString)"SFTP Server Username", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, "anonymous");
				});
			}
		}

		#endregion

		#region BISISftpServerPassword

		public ZString BISISftpServerPassword
		{
			get { return BISISftpServerPasswordItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISISftpServerPasswordItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISISftpServerPasswordItem
		{
			get
			{
				return GetItem("BISIFtpServerPassword", delegate
				{
					StringRegistryItem result = new StringRegistryItem("BISIFtpServerPassword", (NoResString)BISICategory, (NoResString)"SFTP Server Password", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, "ice@ups.com");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#endregion

		#region BillingNotificationGroup

		public ZString BillingNotificationGroup
		{
			get { return BillingNotificationGroupItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BillingNotificationGroupItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BillingNotificationGroupItem
		{
			get
			{
				return GetItem("BillingNotificationGroup", delegate
				{
					return new StringRegistryItem(
						"BillingNotificationGroup",
						(NoResString)BISICategory,
						(NoResString)"Billing Notification Group",
						(NoResString)"The staff group code for staff that will receive billing related failure emails",
						new StringRegistryDataType(1, GlbGroupSchema.GG_Code.MaxLength),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"BIS");
				});
			}
		}

		#endregion

		#region BISIUploadWarningHWM

		public ZDateTime BISIUploadWarningHWM
		{
			get { return new ZDateTime(BISIUploadWarningHWMItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)BISIUploadWarningHWMItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWarningHWMItem
		{
			get
			{
				return GetItem("BISIUploadWarningHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWarningHWM", (NoResString)BISIUploadCategory, (NoResString)"Time Stamp of Upload Warning Last Run", null,
						LongFormat, RegistryStorageFlags.System, RegistryOptions.Default, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region WarningReportNotificationGroup

		public Guid WarningReportNotificationGroup
		{
			get { return WarningReportNotificationGroupItem.Value; }
			set { WarningReportNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem WarningReportNotificationGroupItem
		{
			get
			{
				return GetItem("WarningReportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"WarningReportNotificationGroup",
						(NoResString)BISIUploadCategory,
						(NoResString)"Warning Report Notification Group",
						(NoResString)"The staff group code for staff that will receive BISI upload warning and notification emails",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DefaultBISIWarningReportGroupPK);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region InterchangeReportNotificationGroup
		public Guid InterchangeReportNotificationGroup
		{
			get { return InterchangeReportNotificationGroupItem.Value; }
			set { InterchangeReportNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem InterchangeReportNotificationGroupItem
		{
			get
			{
				return GetItem("InterchangeReportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"InterchangeReportNotificationGroup",
						(NoResString)BISIUploadCategory,
						(NoResString)"Interchange Report Notification Group",
						(NoResString)"The staff group code for staff that will receive Interchange Report notification emails",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DefaultBISIWarningReportGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}
		#endregion

		#endregion

		#region Upload

		#region BISIUploadDirectory
		public ZString BISIUploadDirectory
		{
			get { return BISIUploadDirectoryItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISIUploadDirectoryItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISIUploadDirectoryItem
		{
			get
			{
				return GetItem("BISIUploadDirectory", delegate
				{
					return new StringRegistryItem("BISIUploadDirectory", (NoResString)BISIUploadCategory, (NoResString)"Upload Directory", null, RegistryStorageFlags.Company | RegistryStorageFlags.System, "ivrpfile");
				});
			}
		}
		#endregion

		#region BISIUploadFilename

		public ZString BISIUploadFilename
		{
			get { return BISIUploadFilenameItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISIUploadFilenameItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISIUploadFilenameItem
		{
			get
			{
				return GetItem("BISIUploadFilename", delegate
				{
					return new StringRegistryItem("BISIUploadFilename", (NoResString)BISIUploadCategory, (NoResString)"Upload File Name", null, new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.TextBox), RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.Default, "IVRPUPLD.DAT");
				});
			}
		}

		#endregion

		#region ForceAllXPLDsToBeUploadedEveryTime

		public bool ForceAllXPLDsToBeUploadedEveryTime
		{
			get { return ForceAllXPLDsToBeUploadedEveryTimeItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)ForceAllXPLDsToBeUploadedEveryTimeItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem ForceAllXPLDsToBeUploadedEveryTimeItem
		{
			get
			{
				return GetItem("ForceAllXPLDsToBeUploadedEveryTimeItem", delegate
				{
					return new BooleanRegistryItem("ForceAllXPLDsToBeUploadedEveryTimeItem", (NoResString)BISIUploadCategory,
						(NoResString)"Force All XPLDs To Be Uploaded Every Time", null, RegistryStorageFlags.System, true);
				});
			}
		}

		#endregion

		#region BISIUploadCompletedHWM

		public ZDateTime BISIUploadCompletedHWM
		{
			get
			{
				var item = BISIUploadCompletedHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadCompletedHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadCompletedHWMItem
		{
			get
			{
				return GetItem("BISIUploadCompletedHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadCompletedHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Completed HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 9, 0, 0), true);
				});
			}
		}

		#endregion

		#region BISIUploadEverydayHWM

		public ZDateTime BISIUploadEverydayHWM
		{
			get
			{
				var item = BISIUploadEverydayHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadEverydayHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadEverydayHWMItem
		{
			get
			{
				return GetItem("BISIUploadEverydayHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadEverydayHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Everyday HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadEverydayDateOfArrivalHWM

		public ZDateTime BISIUploadEverydayDateOfArrivalHWM
		{
			get
			{
				var item = BISIUploadEverydayDateOfArrivalHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadEverydayDateOfArrivalHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadEverydayDateOfArrivalHWMItem
		{
			get
			{
				return GetItem("BISIUploadEverydayDateOfArrivalHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadEverydayDateOfArrivalHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Everyday DateOfArrival HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadWorkingDayMetroHWM

		public ZDateTime BISIUploadWorkingDayMetroHWM
		{
			get
			{
				var item = BISIUploadWorkingDayMetroHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadWorkingDayMetroHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayMetroHWMItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayMetroHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayMetroHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Working Day Metro HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadWorkingDayOtherHWM

		public ZDateTime BISIUploadWorkingDayOtherHWM
		{
			get
			{
				var item = BISIUploadWorkingDayOtherHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadWorkingDayOtherHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayOtherHWMItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayOtherHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayOtherHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Working Day Other HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadWorkingDayDateOfArrivalMetroHWM

		public ZDateTime BISIUploadWorkingDayDateOfArrivalMetroHWM
		{
			get
			{
				var item = BISIUploadWorkingDayDateOfArrivalMetroHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadWorkingDayDateOfArrivalMetroHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayDateOfArrivalMetroHWMItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayDateOfArrivalMetroHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayDateOfArrivalMetroHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Working Day DateOfArrival Metro HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadWorkingDayDateOfArrivalOtherHWM

		public ZDateTime BISIUploadWorkingDayDateOfArrivalOtherHWM
		{
			get
			{
				var item = BISIUploadWorkingDayDateOfArrivalOtherHWMItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				return new ZDateTime(item);
			}
			set { ((IRegistryItem)BISIUploadWorkingDayDateOfArrivalOtherHWMItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayDateOfArrivalOtherHWMItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayDateOfArrivalOtherHWM", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayDateOfArrivalOtherHWM", (NoResString)BISIUploadCategory, (NoResString)"BISI Upload Working Day DateOfArrival Other HWM", null,
						LongFormat, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadCurrentBatchNumber

		public ZInt BISIUploadCurrentBatchNumber
		{
			get { return BISIUploadCurrentBatchNumberItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { BISIUploadCurrentBatchNumberItem.SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem BISIUploadCurrentBatchNumberItem
		{
			get
			{
				return GetItem("BISIUploadCurrentBatchNumber", delegate
				{
					return new IntRegistryItem("BISIUploadCurrentBatchNumber", (NoResString)BISIUploadCategory, (NoResString)"Current Batch Number", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, 1);
				});
			}
		}

		#endregion

		#region BISIUploadArchiveDirectory

		public ZString BISIUploadArchiveDirectory
		{
			get { return BISIUploadArchiveDirectoryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISIUploadArchiveDirectoryItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISIUploadArchiveDirectoryItem
		{
			get
			{
				return GetItem("BISIUploadArchiveDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("BISIUploadArchiveDirectory", (NoResString)BISIUploadCategory, (NoResString)"Archive Directory", null, RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region BISIUploadTimeBuffer

		public ZInt BISIUploadTimeBuffer
		{
			get { return BISIUploadTimeBufferItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { BISIUploadTimeBufferItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem BISIUploadTimeBufferItem
		{
			get
			{
				return GetItem("BISIUploadTimeBuffer", delegate
				{
					return new IntRegistryItem("BISIUploadTimeBuffer", (NoResString)BISIUploadCategory, (NoResString)"Upload Time Buffer (in hours)", (NoResString)"This determines the maximum number of shipments to be processed in a single batch.", new NumericRegistryEditorInfo(0), RegistryStorageFlags.System, RegistryOptions.Default, 8, 1, 48);
				});
			}
		}

		#endregion

		#region Upload Time

		DateTimeRegistryEditorInfo TimeFormat
		{
			get { return timeFormat ?? (timeFormat = new DateTimeRegistryEditorInfo(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time)); }
		}
		DateTimeRegistryEditorInfo timeFormat;

		public ZDateTime BISIUploadWorkingDayMetroExportTime
		{
			get { return new ZDateTime(BISIUploadWorkingDayMetroExportTimeItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)BISIUploadWorkingDayMetroExportTimeItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayMetroExportTimeItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayMetroExportTime", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayMetroExportTime",
						(NoResString)(BISIUploadCategory + "/Working Day Upload Time"),
						(NoResString)"Metro Export Time",
						(NoResString)"Time of day to run working day metro export & date of arrival metro export",
						TimeFormat, RegistryStorageFlags.System, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 10, 30, 0), false);
				});
			}
		}

		public ZDateTime BISIUploadWorkingDayOtherExportTime
		{
			get { return new ZDateTime(BISIUploadWorkingDayOtherExportTimeItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)BISIUploadWorkingDayOtherExportTimeItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayOtherExportTimeItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayOtherExportTime", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayOtherExportTime",
						(NoResString)(BISIUploadCategory + "/Working Day Upload Time"),
						(NoResString)"Other Export Time",
						(NoResString)"Time of day to run working day other export & date of arrival other export",
						TimeFormat, RegistryStorageFlags.System, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 16, 30, 0), false);
				});
			}
		}

		public ZDateTime BISIUploadWorkingDayMetroAndOtherExportTime
		{
			get { return new ZDateTime(BISIUploadWorkingDayMetroAndOtherExportTimeItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)BISIUploadWorkingDayMetroAndOtherExportTimeItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem BISIUploadWorkingDayMetroAndOtherExportTimeItem
		{
			get
			{
				return GetItem("BISIUploadWorkingDayMetroAndOtherExportTime", delegate
				{
					return new DateTimeRegistryItem("BISIUploadWorkingDayMetroAndOtherExportTime",
						(NoResString)(BISIUploadCategory + "/Working Day Upload Time"),
						(NoResString)"Other Plus Metro Export Time",
						(NoResString)"Time of day to run working day metro & other export",
						TimeFormat, RegistryStorageFlags.System, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 22, 00, 0), false);
				});
			}
		}

		#endregion

		#region BISIUploadLoadProcessQueueLogBatchSize

		public ZInt BISIUploadLoadProcessQueueLogBatchSize
		{
			get { return BISIUploadLoadProcessQueueLogBatchSizeItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { BISIUploadLoadProcessQueueLogBatchSizeItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem BISIUploadLoadProcessQueueLogBatchSizeItem
		{
			get
			{
				return GetItem("BISIUploadLoadProcessQueueLogBatchSize", delegate
				{
					return new IntRegistryItem(
						(NoResString)"BISIUploadLoadProcessQueueLogBatchSize",
						(NoResString)BISIUploadCategory,
						(NoResString)"Load ProcessQueueLog Batch Size",
						(NoResString)"Load ProcessQueueLog Batch Size",
						RegistryStorageFlags.System,
						100
						);
				});
			}
		}

		#endregion

		#region XPLD Categories

		#region XPLD For Non-Working Days Date of Arrival Passed

		public ReadOnlyCodeDescriptionPairList XPLDForNonWorkingDaysDateOfArrivalPassed
		{
			get { return XPLDForNonWorkingDaysDateOfArrivalPassedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { XPLDForNonWorkingDaysDateOfArrivalPassedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem XPLDForNonWorkingDaysDateOfArrivalPassedItem
		{
			get
			{
				return GetItem("XPLDForNonWorkingDaysDateOfArrivalPassed", delegate
				{
					CodeDescriptionPairList xPLDForUploadList = new CodeDescriptionPairList();
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, string.Empty);
					return new CodeDescriptionPairListRegistryItem(
						"XPLDForNonWorkingDaysDateOfArrivalPassed",
						(NoResString)(BISIUploadCategory + "/XPLD"),
						(NoResString)"XPLD's for everyday if date >= date of arrival",
						(NoResString)"XPLD's entered in this list will be uploaded everyday once the current date is greater or equal to the date of arrival of a shipment.",
						3,
						new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper),
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						xPLDForUploadList,
						false);
				});
			}
		}

		#endregion

		#region XPLD For Working Days

		public ReadOnlyCodeDescriptionPairList XPLDForWorkingDays
		{
			get { return XPLDForWorkingDaysItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { XPLDForWorkingDaysItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem XPLDForWorkingDaysItem
		{
			get
			{
				return GetItem("XPLDForWorkingDays", delegate
				{
					CodeDescriptionPairList xPLDForUploadList = new CodeDescriptionPairList();
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.BK_CertificateOfOriginRequired);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes._34_Missort);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.AS_RefusedShippedTooLate);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.FE_DutyTaxRefused);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.RU_AlternateBroker);
					xPLDForUploadList.AddPair(ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient);

					return new CodeDescriptionPairListRegistryItem(
						"XPLDForWorkingDays",
						(NoResString)(BISIUploadCategory + "/XPLD"),
						(NoResString)"XPLD's for working days",
						(NoResString)"XPLD's entered in this list will be uploaded every working day.",
						3,
						new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper),
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						xPLDForUploadList,
						false);
				});
			}
		}

		#endregion

		#region XPLD For Working Days Date of Arrival Passed

		public ReadOnlyCodeDescriptionPairList XPLDForWorkingDaysDateOfArrivalPassed
		{
			get { return XPLDForWorkingDaysDateOfArrivalPassedItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { XPLDForWorkingDaysDateOfArrivalPassedItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem XPLDForWorkingDaysDateOfArrivalPassedItem
		{
			get
			{
				return GetItem("XPLDForWorkingDaysDateOfArrivalPassed", delegate
				{
					CodeDescriptionPairList xPLDList = new CodeDescriptionPairList();
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.SS_CustomsHold);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold);
					xPLDList.AddPair(ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments);

					return new CodeDescriptionPairListRegistryItem(
						"XPLDForWorkingDaysDateOfArrivalPassed",
						(NoResString)(BISIUploadCategory + "/XPLD"),
						(NoResString)"XPLD's for working days if date >= date of arrival",
						(NoResString)"XPLD's entered in this list will be uploaded on working days once the current date is greater or equal to the date of arrival of a shipment.",
						3,
						new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper),
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						xPLDList,
						false);
				});
			}
		}

		#endregion

		#region FlightNumbersForUploadOnDayOfArrivalPlusOne

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] FlightNumbersForUploadOnDayOfArrivalPlusOne
		{
			get { return ConvertToZStringArray(FlightNumbersForUploadOnDayOfArrivalPlusOneItem.Value); }
			set { FlightNumbersForUploadOnDayOfArrivalPlusOneItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		internal StringRegistryItem FlightNumbersForUploadOnDayOfArrivalPlusOneItem
		{
			get
			{
				return GetItem("FlightNumbersForUploadOnDayOfArrivalPlusOneItem", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"FlightNumbersForUploadOnDayOfArrivalPlusOneItem",
						(NoResString)(BISIUploadCategory + "/XPLD"),
						(NoResString)"Flight Numbers for Upload on Day of Arrival (DoA) + 1",
						(NoResString)"Flight Number(s) entered in this list will be used in conjunction with the following 2  registries only:\r\n - XPLD's for everday if date >=Date of arrival\r\n - XPLD's for working days if date >=Date of arrival\r\nIf the Flight number is in this list, the Date of Arrival used will be the Flight Date of Arrival + 1 day",
						RegistryStorageFlags.System);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region BISIOBCTaxCertificateNumber

		public ZInt BISIOBCTaxCertificateNumber
		{
			get { return BISIOBCTaxCertificateNumberItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { BISIOBCTaxCertificateNumberItem.SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}
		public IntRegistryItem BISIOBCTaxCertificateNumberItem
		{
			get
			{
				return GetItem("BISIOBCTaxCertificateNumber", delegate
				{
					return new IntRegistryItem("BISIOBCTaxCertificateNumber", (NoResString)BISIUploadCategory, (NoResString)"OBC Tax Certificate Number", (NoResString)"Please enter the next OBC Tax Certificate Number", new NumericRegistryEditorInfo(0), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForController, 1, 1, 9999999);
				});
			}
		}

		#endregion

		#region BISIUploadNoOfRecordsPerSave

		public ZInt BISIUploadNoOfRecordsPerSave
		{
			get { return BISIUploadNoOfRecordsPerSaveItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty); }
			set { BISIUploadNoOfRecordsPerSaveItem.SetValue(companyPK, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem BISIUploadNoOfRecordsPerSaveItem
		{
			get
			{
				return GetItem("BISIUploadNoOfRecPerSave", delegate
				{
					return new IntRegistryItem("BISIUploadNoOfRecPerSave", (NoResString)BISIUploadCategory, (NoResString)"No of Records Per Save", (NoResString)"This determines the number of records per factory save.", new NumericRegistryEditorInfo(0), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, 500);
				});
			}
		}

		#endregion
		#endregion

		#region Download

		#region XPLD

		const string XPLDChaseQueueValidationCategory = BISIDownloadCategory + "/XPLD/Chase Queue Validation";

		#region ChaseQueueValidationTotalLocalChargesThreshold

		public ZDecimal ChaseQueueValidationTotalLocalChargesThreshold
		{
			get { return ChaseQueueValidationTotalLocalChargesThresholdItem.Value; }
			set { ChaseQueueValidationTotalLocalChargesThresholdItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		DecimalRegistryItem ChaseQueueValidationTotalLocalChargesThresholdItem
		{
			get
			{
				return GetItem("ChaseQueueValidationTotalLocalChargesThresholdItem", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem(
						"ChaseQueueValidationTotalLocalChargesThresholdItem",
						(NoResString)XPLDChaseQueueValidationCategory,
						(NoResString)"Total Local Charges Threshold for Chase Queue Validation",
						(NoResString)"If a shipment's total local charges is higher than this threshold then it will be excluded from the \"Chase Queue\" validation.",
						RegistryStorageFlags.System,
						1000m);

					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#region ChaseQueueValidationServiceLevels

		public ServiceLevelRegistryItem ChaseQueueValidationServiceLevelRegistryItem
		{
			get
			{
				return GetItem("ChaseQueueValidationServiceLevelRegistryItem", delegate
				{
					return new ServiceLevelRegistryItem(
						"ChaseQueueValidationServiceLevelRegistryItem",
						(NoResString)XPLDChaseQueueValidationCategory,
						(NoResString)"Service Levels for Chase Queue Validation",
						(NoResString)"Please select the Service Levels that will be included in the Chase Queue Validation",
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region ChaseQueueValidationServiceLevelRegistryItem

		public ChaseQueueValidationRegistryItem ChaseQueueValidationRegistryItem
		{
			get
			{
				return GetItem("ChaseQueueValidationRegistryItem", delegate
				{
					return new ChaseQueueValidationRegistryItem(
						"ChaseQueueValidationRegistryItem",
						XPLDChaseQueueValidationCategory,
						"BISI Download Times",
						"Please select the days and the times that will be included in the Chase Queue Validation. If you want to set up Chase Queue Validation till the end of the day - just leave 'Time To' field blank");
				});
			}
		}

		#endregion

		#endregion

		#region CODFilesParentFolder

		public ZString CODFilesParentFolder
		{
			get { return CODFilesParentFolderItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)CODFilesParentFolderItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem CODFilesParentFolderItem
		{
			get
			{
				return GetItem("CODFilesParentFolder", delegate
				{
					return new StringRegistryItem("CODFilesParentFolder", (NoResString)BISIDownloadCategory, (NoResString)"SFTP Parent Folder for COD Files", null, RegistryStorageFlags.System, "BISI/PROD");
				});
			}
		}

		#endregion

		#region BISIDownloadArchiveDirectory

		public ZString BISIDownloadArchiveDirectory
		{
			get { return BISIDownloadArchiveDirectoryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)BISIDownloadArchiveDirectoryItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem BISIDownloadArchiveDirectoryItem
		{
			get
			{
				return GetItem("BISIDownloadArchiveDirectory", delegate
				{
					return new StringRegistryItem("BISIDownloadArchiveDirectory", (NoResString)BISIDownloadCategory, (NoResString)"Archive Directory", null, RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		const string BISIDownloadCategory = Category + "/BISI/Download";

		#endregion

		#endregion

		#region Entry Print

		#region EntryPrintSftpServerAddress

		public ZString EntryPrintSftpServerAddress
		{
			get { return EntryPrintSftpServerAddressItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerAddressItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpServerAddressItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerAddress", delegate
				{
					return new StringRegistryItem("EntryPrintFtpServerAddress", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Address", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, string.Empty);
				});
			}
		}

		#endregion

		#region EntryPrintSftpServerPort

		public ZInt EntryPrintSftpServerPort
		{
			get { return EntryPrintSftpServerPortItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerPortItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, (int)value); }
		}

		internal IntRegistryItem EntryPrintSftpServerPortItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerPort", delegate
				{
					return new IntRegistryItem("EntryPrintFtpServerPort", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Port", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, 22);
				});
			}
		}

		#endregion

		#region EntryPrintSftpServerUsername

		public ZString EntryPrintSftpServerUsername
		{
			get { return EntryPrintSftpServerUsernameItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerUsernameItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpServerUsernameItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerUsername", delegate
				{
					return new StringRegistryItem("EntryPrintFtpServerUsername", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Username", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, "anonymous");
				});
			}
		}

		#endregion

		#region EntryPrintSftpServerPassword

		public ZString EntryPrintSftpServerPassword
		{
			get { return EntryPrintSftpServerPasswordItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerPasswordItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpServerPasswordItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerPassword", delegate
				{
					StringRegistryItem result = new StringRegistryItem("EntryPrintFtpServerPassword", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Password", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, "ice@ups.com");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#endregion

		#region EntryPrintSftpServerUploadDirectory

		public ZString EntryPrintSftpServerUploadDirectory
		{
			get { return EntryPrintSftpServerUploadDirectoryItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerUploadDirectoryItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpServerUploadDirectoryItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerUploadDirectory", delegate
				{
					return new StringRegistryItem("EntryPrintFtpServerUploadDirectory", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Upload Directory", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, string.Empty);
				});
			}
		}

		#endregion

		#region EntryPrintSftpServerUploadFilename

		public ZString EntryPrintSftpServerUploadFilename
		{
			get { return EntryPrintSftpServerUploadFilenameItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpServerUploadFilenameItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpServerUploadFilenameItem
		{
			get
			{
				return GetItem("EntryPrintFtpServerUploadFilename", delegate
				{
					return new StringRegistryItem("EntryPrintFtpServerUploadFilename", (NoResString)EntryPrintCategory, (NoResString)"SFTP Server Upload File Name", null, new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.TextBox), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, "AU_CUSTOMS_ENTRIES.txt");
				});
			}
		}

		#endregion

		#region EntryPrintSftpArchiveDirectory

		public ZString EntryPrintSftpArchiveDirectory
		{
			get { return EntryPrintSftpArchiveDirectoryItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)EntryPrintSftpArchiveDirectoryItem).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem EntryPrintSftpArchiveDirectoryItem
		{
			get
			{
				return GetItem("EntryPrintFtpArchiveDirectory", delegate
				{
					return new StringRegistryItem("EntryPrintFtpArchiveDirectory", (NoResString)EntryPrintCategory, (NoResString)"SFTP Archive Directory", null, RegistryStorageFlags.System | RegistryStorageFlags.Company, string.Empty);
				});
			}
		}

		#endregion

		#region EntryPrintMaximumDeclarationsToSendPerDayItem

		public ZInt EntryPrintMaximumDeclarationsToSendPerDay
		{
			get { return EntryPrintMaximumDeclarationsToSendPerDayItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { EntryPrintMaximumDeclarationsToSendPerDayItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem EntryPrintMaximumDeclarationsToSendPerDayItem
		{
			get
			{
				return GetItem("EntryPrintMaximumDeclarationsToSendPerDay", delegate
				{
					return new IntRegistryItem("EntryPrintMaximumDeclarationsToSendPerDay", (NoResString)EntryPrintCategory, (NoResString)"Maximum declarations per day", (NoResString)"The maximum number of declarations to send entry prints for at a time so that the system doesn't get overloaded", RegistryStorageFlags.System | RegistryStorageFlags.Company, 1000);
				});
			}
		}

		#endregion

		readonly string EntryPrintCategory = Category + "/Entry Print Upload";

		#endregion

		#region GSSi SFTP

		#region GSSiSftpServerAddress

		public ZString GSSiSftpServerAddress
		{
			get { return GSSiSftpServerAddressItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)GSSiSftpServerAddressItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem GSSiSftpServerAddressItem
		{
			get
			{
				return GetItem("GSSiFtpServerAddress", delegate
				{
					return new StringRegistryItem("GSSiFtpServerAddress", (NoResString)GSSiCategory, (NoResString)"SFTP Server Address", null, RegistryStorageFlags.System, string.Empty);
				});
			}
		}

		#endregion

		#region GSSiSftpServerPort

		public ZInt GSSiSftpServerPort
		{
			get { return GSSiSftpServerPortItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)GSSiSftpServerPortItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)value); }
		}

		internal IntRegistryItem GSSiSftpServerPortItem
		{
			get
			{
				return GetItem("GSSiFtpServerPort", delegate
				{
					return new IntRegistryItem("GSSiFtpServerPort", (NoResString)GSSiCategory, (NoResString)"SFTP Server Port", null, RegistryStorageFlags.System, 1022);
				});
			}
		}

		#endregion

		#region GSSiSftpServerUsername

		public ZString GSSiSftpServerUsername
		{
			get { return GSSiSftpServerUsernameItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)GSSiSftpServerUsernameItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem GSSiSftpServerUsernameItem
		{
			get
			{
				return GetItem("GSSiFtpServerUsername", delegate
				{
					return new StringRegistryItem("GSSiFtpServerUsername", (NoResString)GSSiCategory, (NoResString)"SFTP Server Username", null, RegistryStorageFlags.System, "wtg0717");
				});
			}
		}

		#endregion

		#region GSSiSftpServerPassword

		public ZString GSSiSftpServerPassword
		{
			get { return GSSiSftpServerPasswordItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)GSSiSftpServerPasswordItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem GSSiSftpServerPasswordItem
		{
			get
			{
				return GetItem("GSSiFtpServerPassword", delegate
				{
					StringRegistryItem result = new StringRegistryItem("GSSiFtpServerPassword", (NoResString)GSSiCategory, (NoResString)"SFTP Server Password", null, RegistryStorageFlags.System, "Rtb21BT");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					return result;
				});
			}
		}

		#endregion

		#region GSSiSftpServerUploadDirectory

		public ZString GSSiSftpServerUploadDirectory
		{
			get { return GSSiSftpServerUploadDirectoryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)GSSiSftpServerUploadDirectoryItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem GSSiSftpServerUploadDirectoryItem
		{
			get
			{
				return GetItem("GSSiFtpServerUploadDirectory", delegate
				{
					return new StringRegistryItem("GSSiFtpServerUploadDirectory", (NoResString)GSSiCategory, (NoResString)"SFTP Server Upload Directory", null, RegistryStorageFlags.System, "GSS");
				});
			}
		}

		#endregion

		#endregion

		#region UPS Charges

		#region ContactFeeAmount

		public DecimalRegistryItem ContactFeeAmount
		{
			get
			{
				return GetItem("UPEContactFeeAmount", delegate
				{
					return new DecimalRegistryItem("UPEContactFeeAmount", (NoResString)PreReleaseCategory, (NoResString)"Contact Fee Amount", (NoResString)"Contact Fee Amount",
						new NumericRegistryEditorInfo(2), RegistryStorageFlags.System, RegistryOptions.Default,
						0m, 0, 999999);
				});
			}
		}

		#endregion

		#region SecurityFeeAmount

		public DecimalRegistryItem SecurityFeeAmount
		{
			get
			{
				return GetItem("SecurityFeeAmount", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("SecurityFeeAmount", (NoResString)ChargesCategory, (NoResString)"Security Fee Amount", null, RegistryStorageFlags.System, 9.95m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#region Charge Per Line

		public DecimalRegistryItem EntryLineChargeCappedAmount
		{
			get
			{
				return GetItem("UPEEntryLineChargeCappedAmount", delegate
				{
					return new DecimalRegistryItem("UPEEntryLineChargeCappedAmount", (NoResString)ChargePerLineCategory, (NoResString)"Capped Amount",
						(NoResString)"The Capped Chargeable Amount Per Entry", new NumericRegistryEditorInfo(2), RegistryStorageFlags.System, RegistryOptions.Default, 399m, 0, 999999);
				});
			}
		}

		public DecimalRegistryItem EntryLineChargeBaseAmount
		{
			get
			{
				return GetItem("UPEEntryLineChargeBaseAmount", delegate
				{
					return new DecimalRegistryItem("UPEEntryLineChargeBaseAmount", (NoResString)ChargePerLineCategory, (NoResString)"Base Charge",
						(NoResString)"Base Charge Per Entry", new NumericRegistryEditorInfo(2), RegistryStorageFlags.System, RegistryOptions.Default, 0m, 0, 999999);
				});
			}
		}

		public DecimalRegistryItem PerLineChargeAmount
		{
			get
			{
				return GetItem("UPEPerLineChargeAmount", delegate
				{
					return new DecimalRegistryItem("UPEPerLineChargeAmount", (NoResString)ChargePerLineCategory, (NoResString)"Charge Per Line",
						(NoResString)"Charge Per Entry Line",
						new NumericRegistryEditorInfo(2), RegistryStorageFlags.System, RegistryOptions.Default, 4m, 0, 999999);
				});
			}
		}

		public IntRegistryItem LinesExemptedFromLineCharge
		{
			get
			{
				return GetItem("UPELinesExemptedFromLineCharge", delegate
				{
					return new IntRegistryItem("UPELinesExemptedFromLineCharge", (NoResString)ChargePerLineCategory,
						(NoResString)$"Lines Exempted From '{PerLineChargeAmount.Caption}'", (NoResString)$"Number of Entry Lines Exempted from '{PerLineChargeAmount.Caption}'.\n\nFor Example:\nCustoms Entry has 10 Entry Lines.\nRegistry value = 5.\n\n'{PerLineChargeAmount.Caption}' will apply to the 6th line and every additional line thereafter.", RegistryStorageFlags.System, 5);
				});
			}
		}

		public BooleanRegistryItem PreReleaseChargeEnabled
		{
			get
			{
				return GetItem("UPEPreReleaseChargeEnabled", delegate
				{
					return new BooleanRegistryItem(
						"UPEPreReleaseChargeEnabled",
						(NoResString)PreReleaseCategory,
						(NoResString)"Enable Pre-Release Charge Calculation",
						(NoResString)"Enables Pre-Release Charge Calculation.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ZBool.False);
				});
			}
		}
		#endregion

		#region TerminalFee (ITF Charge)

		// TODO: Settable in registry for pre-CMR, post-CMR this needs to be calculated / stored against the organisation

		public DecimalRegistryItem TerminalFeeAmount
		{
			get
			{
				return GetItem("TerminalFeeAmount", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("TerminalFeeAmount", (NoResString)ChargesCategory, (NoResString)"Terminal Fee Amount (ITF)", (NoResString)"Terminal Fee Amount (ITF), excluding GST", RegistryStorageFlags.System, 45.0m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#region Alternate Broker Storage Fee

		public DecimalRegistryItem AlternateBrokerStorageFeePerDay
		{
			get
			{
				return GetItem("AlternateBrokerStorageFeePerDay", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("AlternateBrokerStorageFeePerDay", (NoResString)ChargesCategory, (NoResString)"Alternate Broker Storage Fee Per Day", (NoResString)"Alternate Broker storage fee per day, excluding GST", RegistryStorageFlags.System, 35.0m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		const string ChargesCategory = Category + "/Charges";
		const string PreReleaseCategory = ChargesCategory + "/Pre-Release Charge";
		const string ChargePerLineCategory = PreReleaseCategory + "/Entry Line Charge";

		#endregion

		#region ClassifierStaffGroupCode

		public StringRegistryItem ClassifierStaffGroupCode
		{
			get
			{
				return GetItem("ClassifierStaffGroupCode", delegate
				{
					return new StringRegistryItem(
						"ClassifierStaffGroupCode",
						(NoResString)Category,
						(NoResString)"Classifier Staff Group",
						(NoResString)"The staff group code that defines the classifiers",
						new StringRegistryDataType(1, GlbGroupSchema.GG_Code.MaxLength),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						"CLS");
				});
			}
		}

		#endregion

		#region COD Thresholds

		#region COD Confirm Payment Threshold

		public ZDecimal CODConfirmPaymentThreshold
		{
			get { return CODConfirmPaymentThresholdItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)CODConfirmPaymentThresholdItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (decimal)value); }
		}

		internal DecimalRegistryItem CODConfirmPaymentThresholdItem
		{
			get
			{
				return GetItem("ConfirmPaymentThreshold", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("ConfirmPaymentThreshold", (NoResString)CODCategory, (NoResString)"COD Confirm Payment Threshold", null, RegistryStorageFlags.System, 5000m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#region COD Auto Release at Upload Threshold

		public ZDecimal CODAutoReleaseAtUploadThreshold
		{
			get { return CODAutoReleaseAtUploadThresholdItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)CODAutoReleaseAtUploadThresholdItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (decimal)value); }
		}

		internal DecimalRegistryItem CODAutoReleaseAtUploadThresholdItem
		{
			get
			{
				return GetItem("AutoReleaseAtUploadThreshold", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("AutoReleaseAtUploadThreshold", (NoResString)CODCategory, (NoResString)"COD AutoRelease at Upload Threshold", null, RegistryStorageFlags.System, 100m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#region COD Auto Release and Chase Threshold

		public ZDecimal CODAutoReleaseAndChaseThreshold
		{
			get { return CODAutoReleaseAndChaseThresholdItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ((IRegistryItem)CODAutoReleaseAndChaseThresholdItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (decimal)value); }
		}

		public DecimalRegistryItem CODAutoReleaseAndChaseThresholdItem
		{
			get
			{
				return GetItem("AutoReleaseAndChaseThreshold", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem("AutoReleaseAndChaseThreshold", (NoResString)CODCategory, (NoResString)"COD AutoRelease and Chase Threshold", null, RegistryStorageFlags.System, 250m);
					result.EditorInfo = new NumericRegistryEditorInfo(2);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region Document Printing and Delivery

		#region PrintBatchMaxCount

		public IntRegistryItem PrintBatchMaxCount
		{
			get
			{
				return GetItem("PrintBatchMaxCount", delegate
				{
					return new IntRegistryItem(
						"PrintBatchMaxCount",
						(NoResString)DocumentPrintingAndDeliveryCategory,
						(NoResString)"Maximum Documents in Print Batch",
						(NoResString)"Maximum Documents in Print Batch",
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						200);
				});
			}
		}

		#endregion

		#region DocumentAutoDeliveryNotificationGroup

		public StringRegistryItem DocumentAutoDeliveryNotificationGroup
		{
			get
			{
				return GetItem("UPEDocumentAutoDeliveryNotificationGroup", delegate
				{
					return new StringRegistryItem(
						"UPEDocumentAutoDeliveryNotificationGroup",
						(NoResString)DocumentPrintingAndDeliveryCategory,
						(NoResString)"Document Auto-Delivery Notification Group",
						(NoResString)"The staff group code for staff that will receive document automatic delivery failure emails",
						new StringRegistryDataType(1, GlbGroupSchema.GG_Code.MaxLength),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						"IVN");
				});
			}
		}

		#endregion

		#region UPS Contact Fax

		public StringRegistryItem UPSContactFax
		{
			get
			{
				return GetItem("ShipmentHeldLetterUPSContactFax", delegate
				{
					return new StringRegistryItem(
						"ShipmentHeldLetterUPSContactFax",
						(NoResString)DocumentPrintingAndDeliveryCategory,
						(NoResString)"UPS Contact Fax",
						(NoResString)"Defines the UPS contact Fax number that is placed on the Customer Notification Postcard and Letter of Authority documents when delivered",
						new StringRegistryDataType(1, 64),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		#endregion

		#region UPSContactEmail

		public StringRegistryItem UPSContactEmail
		{
			get
			{
				return GetItem("ShipmentHeldLetterUPSContactEmail", delegate
				{
					return new StringRegistryItem(
						"ShipmentHeldLetterUPSContactEmail",
						(NoResString)DocumentPrintingAndDeliveryCategory,
						(NoResString)"UPS Contact Email",
						(NoResString)"Defines the UPS contact Email address that is placed on the Customer Notification Postcard document when delivered",
						new StringRegistryDataType(1, 64),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		#endregion

		readonly string DocumentPrintingAndDeliveryCategory = Category + "/Document Printing and Delivery";

		#endregion

		#region Invoice

		#region Tax Invoice

		#region TaxInvoiceCommentsText1

		public StringRegistryItem TaxInvoiceCommentsText1
		{
			get
			{
				return GetItem("TaxInvoiceCommentsText1", delegate
				{
					return new StringRegistryItem(
						"TaxInvoiceCommentsText1",
						(NoResString)TaxInvoiceCategory,
						(NoResString)"Tax Invoice Comments Normal",
						(NoResString)"Defines the text that appears under the charges on the first page of the tax invoice",
						new StringRegistryDataType(1, 5000),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		#endregion

		#region TaxInvoiceCommentsText2

		public StringRegistryItem TaxInvoiceCommentsText2
		{
			get
			{
				return GetItem("TaxInvoiceCommentsText2", delegate
				{
					return new StringRegistryItem(
						"TaxInvoiceCommentsText2",
						(NoResString)TaxInvoiceCategory,
						(NoResString)"Tax Invoice Comments Italic",
						(NoResString)"Defines the text that appears under the charges on the first page of the tax invoice",
						new StringRegistryDataType(1, 5000),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		#endregion

		#region TaxInvoiceFooterText

		public StringRegistryItem TaxInvoiceFooterText
		{
			get
			{
				return GetItem("TaxInvoiceFooterText", delegate
				{
					return new StringRegistryItem(
						"TaxInvoiceFooterText",
						(NoResString)TaxInvoiceCategory,
						(NoResString)"Tax Invoice Footer Text",
						(NoResString)"Defines the text that appears at the bottom of the second page of the invoice",
						new StringRegistryDataType(1, 5000),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						string.Empty);
				});
			}
		}

		#endregion

		#region TaxInvoiceImage

		public ImageRegistryItem TaxInvoiceImage
		{
			get
			{
				return GetItem("UPETaxInvoiceImage", delegate
				{
					return new ImageRegistryItem(
						"UPETaxInvoiceImage",
						(NoResString)TaxInvoiceCategory,
						(NoResString)"Tax Invoice Image",
						(NoResString)"Defines the image that appears on the invoice",
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						null);
				});
			}
		}

		#endregion

		readonly string TaxInvoiceCategory = Category + "/Tax Invoice";

		#endregion

		#endregion

		#region MatchingActivitiesHWM

		public ZDateTime MatchingActivitiesHWM
		{
			get { return new ZDateTime(MatchingActivitiesHWMItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)MatchingActivitiesHWMItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem MatchingActivitiesHWMItem
		{
			get
			{
				return GetItem("MatchingActivitiesHWM", delegate
				{
					return new DateTimeRegistryItem("MatchingActivitiesHWM", (NoResString)MatchingActivitiesCategory, (NoResString)"Time Stamp of Matching Activities Last Run", null,
						LongFormat, RegistryStorageFlags.System, RegistryOptions.Default, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		readonly string MatchingActivitiesCategory = Category + "/Matching Activities";

		#endregion

		#region SAC Screening

		const string UPSScreeningCategory = Category + "/Screening";

		#region AllowMultipleLevel1LoadsForMasterBill

		public bool AllowMultipleLevel1LoadsForMasterBill
		{
			get => AllowMultipleLevel1LoadsForMasterBillItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)AllowMultipleLevel1LoadsForMasterBillItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem AllowMultipleLevel1LoadsForMasterBillItem
		{
			get
			{
				return GetItem("AllowMultipleLevel1LoadsForMasterBill", delegate
				{
					var result = new BooleanRegistryItem("AllowMultipleLevel1LoadsForMasterBill",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Allow Multiple Level 1 Loads For Master Bill",
						(NoResString)"Allows the Level 1 Data import to load multiple level 1 files onto an existing Master Bill",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
					return result;
				});
			}
		}

		#endregion

		#region AllowBillUpdatesDuringMulitpleLevel1Loads

		public bool AllowBillUpdatesDuringMulitpleLevel1Loads
		{
			get => AllowBillUpdatesDuringMulitpleLevel1LoadsItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)AllowBillUpdatesDuringMulitpleLevel1LoadsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem AllowBillUpdatesDuringMulitpleLevel1LoadsItem
		{
			get
			{
				return GetItem("AllowBillUpdatesDuringMulitpleLevel1Loads", delegate
				{
					var result = new BooleanRegistryItem("AllowBillUpdatesDuringMulitpleLevel1Loads",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Allow Bills to be Updated for Multiple Level 1 Loads",
						(NoResString)"Allows the Level 1 Data import to update Bills for an existing Master Bill when the Master Bill is used for multiple loads",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						false);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
					return result;
				});
			}
		}

		#endregion

		#region ShipmentReferenceNumberRecyclePeriod

		public int ShipmentReferenceNumberRecyclePeriod
		{
			get => ShipmentReferenceNumberRecyclePeriodItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)ShipmentReferenceNumberRecyclePeriodItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal IntRegistryItem ShipmentReferenceNumberRecyclePeriodItem
		{
			get
			{
				return GetItem("ShipmentReferenceNumberRecyclePeriod", delegate
				{
					var result = new IntRegistryItem("ShipmentReferenceNumberRecyclePeriod",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Shipment Reference Number Recycle Window in months",
						(NoResString)"The number of months after which a Shipment Reference Number may be re-used",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						12);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
					return result;
				});
			}
		}

		#endregion

		#region StopImportOfBillIfMatchingBillFound

		public bool StopImportOfBillIfMatchingBillFound
		{
			get => StopImportOfBillIfMatchingBillFoundItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)StopImportOfBillIfMatchingBillFoundItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem StopImportOfBillIfMatchingBillFoundItem
		{
			get
			{
				return GetItem("StopImportOfBillIfMatchingBillFound", delegate
				{
					var result = new BooleanRegistryItem("StopImportOfBillIfMatchingBillFound",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Stops the import of the Bill if a matching Bill is found",
						(NoResString)"Enables the checking of Bills in the system against the Shipment Reference Number",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
					return result;
				});
			}
		}

		#endregion

		#region Enable Freight Auto Rating in Level 1 Import

		public bool EnableFreightAutoRatingInLevelOneImport
		{
			get => EnableFreightAutoRatingInLevelOneImportItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)EnableFreightAutoRatingInLevelOneImportItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem EnableFreightAutoRatingInLevelOneImportItem
		{
			get
			{
				return GetItem("EnableFreightAutoRatingInLevelOneImport", delegate
				{
					var result = new BooleanRegistryItem("EnableFreightAutoRatingInLevelOneImport",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Enable Freight Auto Rating in Level 1 Import",
						(NoResString)"Allows the Level 1 Data import to calculate freight and insurance value based on freight auto-rating system.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true);
					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
					return result;
				});
			}
		}

		#endregion

		#region EnableDecisionSupportImportShipments

		public bool EnableDecisionSupportImportShipments
		{
			get => EnableDecisionSupportImportShipmentsItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)EnableDecisionSupportImportShipmentsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem EnableDecisionSupportImportShipmentsItem
		{
			get
			{
				var result = GetItem("EnableDecisionSupportImportShipmentsItem", () =>
					new BooleanRegistryItem("EnableDecisionSupportImportShipmentsItem",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Enable Decision Support for Import Shipments",
						(NoResString)"Enables the Level 1 Import Decision Provider to determine if Global Manifest and Customs shipments are created for the imported level 1 file",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true));
				result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
				return result;
			}
		}

		#endregion

		#region EnableDecisionSupportExportShipments

		public bool EnableDecisionSupportExportShipments
		{
			get => EnableDecisionSupportExportShipmentsItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)EnableDecisionSupportExportShipmentsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem EnableDecisionSupportExportShipmentsItem
		{
			get
			{
				var result = GetItem("EnableDecisionSupportExportShipmentsItem", () =>
					new BooleanRegistryItem("EnableDecisionSupportExportShipmentsItem",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Enable Decision Support for Export Shipments",
						(NoResString)"Enables the Level 1 Import Decision Provider to determine if Global Manifest and Customs shipments are created for the imported level 1 file",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						true));
				result.CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia;
				return result;
			}
		}

		#endregion

		#region Default Level 1 Carrier Agent

		public ShippingAgentRegistryItem DefaultLevelOneImportCarrierAgentItem
		{
			get
			{
				return GetItem("DefaultLevelOneImportCarrierAgent", delegate
				{
					return new ShippingAgentRegistryItem(
						"DefaultLevelOneImportCarrierAgent",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Default Level 1 Carrier Agent for an Import shipment",
						(NoResString)"Defaults the Carrier Agent field on the Level 1 screen for an Import shipment")
					{
						CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia,
					};
				});
			}
		}

		public ShippingAgentRegistryItem DefaultLevelOneExportCarrierAgentItem
		{
			get
			{
				return GetItem("DefaultLevelOneExportCarrierAgent", delegate
				{
					return new ShippingAgentRegistryItem(
						"DefaultLevelOneExportCarrierAgent",
						(NoResString)UPSScreeningCategory,
						(NoResString)"Default Level 1 Carrier Agent for an Export shipment",
						(NoResString)"Defaults the Carrier Agent field on the Level 1 screen for an Export shipment")
					{
						CountryFilterPKs = ActiveCompaniesCountriesExcludingAustralia,
					};
				});
			}
		}

		#endregion

		#region AU Screening

		const string UPSScreeningAUCategory = UPSScreeningCategory + "/AU";

		#region StopPhrasesForConsignorName

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsignorName
		{
			get { return ConvertToZStringArray(StopPhrasesForConsignorNameItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)StopPhrasesForConsignorNameItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsignorNameItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsignorName", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsignorName",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignor Name",
						(NoResString)GetStopPhrasesHint("Consignor Name"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForConsignorAddress

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsignorAddress
		{
			get { return ConvertToZStringArray(StopPhrasesForConsignorAddressItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForConsignorAddressItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsignorAddressItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsignorAddress", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsignorAddress",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignor Address",
						(NoResString)GetStopPhrasesHint("Consignor Address line 1 or 2"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForConsignorAccountNum

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsignorAccountNum
		{
			get { return ConvertToZStringArray(StopPhrasesForConsignorAccountNumItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForConsignorAccountNumItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsignorAccountNumItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsignorAccountNum", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsignorAccountNum",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignor Account #",
						(NoResString)GetStopPhrasesHint("Consignor Account # of the level 1 record"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForConsigneeName

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsigneeName
		{
			get { return ConvertToZStringArray(StopPhrasesForConsigneeNameItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForConsigneeNameItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsigneeNameItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsigneeName", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsigneeName",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignee Name",
						(NoResString)GetStopPhrasesHint("Consignee Name"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForConsigneeAddress

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsigneeAddress
		{
			get { return ConvertToZStringArray(StopPhrasesForConsigneeAddressItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForConsigneeAddressItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsigneeAddressItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsigneeAddress", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsigneeAddress",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignee Address",
						(NoResString)GetStopPhrasesHint("Consignee Address line 1 or 2"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForConsigneeAccountNum

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForConsigneeAccountNum
		{
			get { return ConvertToZStringArray(StopPhrasesForConsigneeAccountNumItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForConsigneeAccountNumItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForConsigneeAccountNumItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForConsigneeAccountNum", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForConsigneeAccountNum",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Consignee Account #",
						(NoResString)GetStopPhrasesHint("Consignee Account # of the level 1 record"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region StopPhrasesForGoodsDescription

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] StopPhrasesForGoodsDescription
		{
			get { return ConvertToZStringArray(StopPhrasesForGoodsDescriptionItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { StopPhrasesForGoodsDescriptionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem StopPhrasesForGoodsDescriptionItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForGoodsDescription", delegate
				{
					StringWriter defaultValue = new StringWriter();
					defaultValue.WriteLine("CONSOL");
					defaultValue.WriteLine("CONSOLE");
					defaultValue.WriteLine("GIFT");
					defaultValue.WriteLine("XMAS");
					defaultValue.WriteLine("CHRISTMAS");
					defaultValue.WriteLine("TBA");
					defaultValue.WriteLine("UNKNOWN");
					defaultValue.WriteLine("PRESENT");
					defaultValue.WriteLine("NCV");
					defaultValue.WriteLine("INVOICES");
					defaultValue.WriteLine("SOUVINER");
					defaultValue.WriteLine("PERSONAL HYGIENE");
					defaultValue.WriteLine("DITTO");
					defaultValue.WriteLine("NON HAZARDOUS");
					defaultValue.WriteLine("USED");
					defaultValue.WriteLine("SAMPLE");
					defaultValue.WriteLine("PERSONAL");
					defaultValue.WriteLine("ETC");
					defaultValue.WriteLine("OLD");
					defaultValue.WriteLine("SAMPLE");

					StringRegistryItem result = new StringRegistryItem("UPEStopPhrasesForGoodsDescription",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Goods Description",
						(NoResString)GetStopPhrasesHint("Goods Description"),
						RegistryStorageFlags.System,
						defaultValue.GetStringBuilder().ToString());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region QuarantineStopPhrasesForGoodsDescription

		[SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Underlying Registry Architecture currently requires array")]
		public string[] QuarantineStopPhrasesForGoodsDescription
		{
			get { return ConvertToZStringArray(QuarantineStopPhrasesForGoodsDescriptionItem.Value); }
			set { QuarantineStopPhrasesForGoodsDescriptionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value)); }
		}

		StringRegistryItem QuarantineStopPhrasesForGoodsDescriptionItem
		{
			get
			{
				return GetItem("UPEQuarantineStopPhrasesForGoodsDescription", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEQuarantineStopPhrasesForGoodsDescription",
						(NoResString)UPSScreeningAUCategory,
						(NoResString)"Quarantine Stop Words",
						(NoResString)GetStopPhrasesHint("Goods Description"),
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region NonDocumentScreeningValueRanges

		public IReadOnlyList<KeyValuePair<decimal, decimal>> NonDocumentScreeningValueRanges
		{
			get
			{
				List<KeyValuePair<decimal, decimal>> result = new List<KeyValuePair<decimal, decimal>>();
				string[] ranges = NonDocumentScreeningValueRangesItem.Value.Split(';');
				foreach (string range in ranges)
				{
					int separatorIndex = range.IndexOf("-");
					if (separatorIndex != -1)
					{
						decimal from, to;
						if (decimal.TryParse(range.Substring(0, separatorIndex), out from) &&
							decimal.TryParse(range.Substring(separatorIndex + 1), out to))
						{
							result.Add(new KeyValuePair<decimal, decimal>(from, to));
						}
					}
				}
				return result;
			}
		}

		public StringRegistryItem NonDocumentScreeningValueRangesItem
		{
			get
			{
				return GetItem("NonDocumentScreeningValueRanges", () => new StringRegistryItem(
					"NonDocumentScreeningValueRanges",
					(NoResString)UPSScreeningAUCategory,
					(NoResString)"Screening Goods Value Ranges",
					(NoResString)"Ranges of Goods Value from which screening will identify cargo report jobs, which will cause them to move to Intervention. For example, '0.00-1.00;995.00-999.00'",
					RegistryStorageFlags.System, 0m));
			}
		}

		#endregion

		static string GetStopPhrasesHint(string field)
		{
			return "Stop words or phrases that when identified in the '" + field + "' of a cargo report, will move the job to Intervention";
		}

		#endregion

		#region SG Screening

		const string UPSScreeningSGCategory = UPSScreeningCategory + "/SG";

		#region StopPhrasesForSGGoodsDescription

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGGoodsDescription
		{
			get => ConvertToZStringArray(StopPhrasesForSGGoodsDescriptionItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGGoodsDescriptionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGGoodsDescriptionItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGGoodsDescription", () => new StringRegistryItem(
					"UPEStopPhrasesForSGGoodsDescription",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Goods Description Stop Words",
					(NoResString)GetSGStopPhrasesHint("Packline Goods Description"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPostcodeRangesForSGFreeTradeZones

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPostcodeRangesForSGFreeTradeZones
		{
			get => ConvertToZStringArray(StopPostcodeRangesForSGFreeTradeZonesItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPostcodeRangesForSGFreeTradeZonesItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPostcodeRangesForSGFreeTradeZonesItem
		{
			get
			{
				return GetItem("UPEStopPostcodeRangesForSGFreeTradeZones", delegate
				{
					var defaultValue = new StringWriter(CultureInfo.InvariantCulture);
					defaultValue.WriteLine("48000 to 52000");
					defaultValue.WriteLine("81000 to 81999");

					return new StringRegistryItem("UPEStopPostcodeRangesForSGFreeTradeZones",
						(NoResString)UPSScreeningSGCategory,
						(NoResString)"Postcode Ranges for Free Trade Zone filtering",
						(NoResString)GetSGStopPhrasesHint("Consignee Postcode"),
						RegistryStorageFlags.System,
						defaultValue.GetStringBuilder().ToString())
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
				});
			}
		}

		#endregion

		#region SGFallBackIncotermForLevel1

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] SGFallBackIncotermForLevel1
		{
			get => ConvertToZStringArray(SGFallBackIncotermForLevel1Item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => SGFallBackIncotermForLevel1Item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem SGFallBackIncotermForLevel1Item
		{
			get
			{
				return GetItem("UPESGFallBackIncotermForLevel1", delegate
				{
					var defaultValue = new StringWriter(CultureInfo.InvariantCulture);
					defaultValue.WriteLine("FOB");

					return new StringRegistryItem("UPESGFallBackIncotermForLevel1",
						(NoResString)UPSScreeningSGCategory,
						(NoResString)"Fallback Incoterm for Level 1",
						(NoResString)"Default Incoterm to be used when the billing term is missing from the Level 1 file being imported",
						RegistryStorageFlags.System,
						defaultValue.GetStringBuilder().ToString())
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
					};
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsigneeAccountNum

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsigneeAccountNum
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsigneeAccountNumItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsigneeAccountNumItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsigneeAccountNumItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsigneeAccountItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsigneeAccountItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignee Account #",
					(NoResString)GetSGStopPhrasesHint("Consignee Account #"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsigneeAddress

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsigneeAddress
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsigneeAddressItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsigneeAddressItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsigneeAddressItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsigneeAddressItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsigneeAddressItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignee Address",
					(NoResString)GetSGStopPhrasesHint("Consignee Address"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsigneeName

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsigneeName
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsigneeNameItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsigneeNameItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsigneeNameItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsigneeNameItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsigneeNameItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignee Name",
					(NoResString)GetSGStopPhrasesHint("Consignee Name"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsignorAccountNum

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsignorAccountNum
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsignorAccountNumItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsignorAccountNumItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsignorAccountNumItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsignorAccountItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsignorAccountItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignor Account #",
					(NoResString)GetSGStopPhrasesHint("Consignor Account #"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsignorAddress

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsignorAddress
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsignorAddressItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsignorAddressItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsignorAddressItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsignorAddressItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsignorAddressItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignor Address",
					(NoResString)GetSGStopPhrasesHint("Consignor Address"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region StopPhrasesForSGConsignorName

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public string[] StopPhrasesForSGConsignorName
		{
			get => ConvertToZStringArray(StopPhrasesForSGConsignorNameItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			set => StopPhrasesForSGConsignorNameItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToString(value));
		}

		internal StringRegistryItem StopPhrasesForSGConsignorNameItem
		{
			get
			{
				return GetItem("UPEStopPhrasesForSGConsignorNameItem", () => new StringRegistryItem(
					"UPEStopPhrasesForSGConsignorNameItem",
					(NoResString)UPSScreeningSGCategory,
					(NoResString)"Filter Consignor Name",
					(NoResString)GetSGStopPhrasesHint("Consignor Name"),
					RegistryStorageFlags.System)
				{
					EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo)
				});
			}
		}

		#endregion

		#region FilterSGTranshipments

		public bool FilterSGTranshipments
		{
			get => FilterSGTranshipmentsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)FilterSGTranshipmentsItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem FilterSGTranshipmentsItem
		{
			get
			{
				return GetItem("FilterSGTranshipmentsItem", () =>
					new BooleanRegistryItem("FilterSGTranshipmentsItem",
						(NoResString)UPSScreeningSGCategory,
						(NoResString)"Filter Transhipments",
						(NoResString)"Stops Level 1 shipments that are transhipments being imported",
						RegistryStorageFlags.System,
						true));
			}
		}

		#endregion

		#region EnableAutoPopulateCycleDetailsToImportGlobalManifestBills

		public bool EnableAutoPopulateCycleDetailsToImportGlobalManifestBills
		{
			get => EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
			set => ((IRegistryItem)EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem).SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		internal BooleanRegistryItem EnableAutoPopulateCycleDetailsToImportGlobalManifestBillsItem
		{
			get
			{
				return GetItem("EnableAutoPopulateCycleDetailsToImportGlobalManifestBills", () =>
					new BooleanRegistryItem(
						"EnableAutoPopulateCycleDetailsToImportGlobalManifestBills",
						(NoResString)UPSScreeningSGCategory,
						(NoResString)"Auto Populate Cycle details to Import Global Manifest Bills",
						(NoResString)"Enable the auto population of cycle details entered on the Level 1 screen against low value non TradeNet screened shipments.",
						RegistryStorageFlags.Company,
						true)
					{
						CountryFilterPKs = CountryFilterPKs.Singapore
					});
			}
		}

		#endregion

		static string GetSGStopPhrasesHint(string field)
		{
			return "Stop words or phrases that when identified in the '" + field + "' of a global manifest, will move the job to TradeNet";
		}

		#endregion

		#endregion

		#region Auto Queue Movement

		#region CusHAWB Queue Movement based on CARST Message

		public CusHAWBAutoQueueMovementCollection SortedCusHAWBAutoQueueMovements
		{
			get
			{
				if (fSortedCusHAWBAutoQueueMovements == null)
				{
					fSortedCusHAWBAutoQueueMovements = CusHAWBAutoQueueMovementRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
					fSortedCusHAWBAutoQueueMovements.Sort(CusHAWBAutoQueueMovement.Schema.Priority, ListSortDirection.Ascending);
				}
				return fSortedCusHAWBAutoQueueMovements;
			}
		}

		internal CusHAWBAutoQueueMovementRegistryItem CusHAWBAutoQueueMovementRegistryItem
		{
			get
			{
				return GetItem("CusHAWBAutoQueueMovementRegistryItem", delegate
				{
					string hint = "Cargo Report Queue Movement based on the Free Text Segments of the received CARST Message.\r\n" +
									"Queue is moved in the order of priority (1 being the highest priority).\r\n" +
									"Segment Name has to be exactly as how it appears in the message.";
					CusHAWBAutoQueueMovementRegistryItem result = new CusHAWBAutoQueueMovementRegistryItem(
						"CusHAWBAutoQueueMovementRegistryItem",
						AutoQueueMovementCategory,
						"Cargo Report Queue Movement",
						hint,
						DefaultCusHAWBAutoQueueMovements);
					result.ValueSet += new EventHandler(fCusHAWBAutoQueueMovementRegistryItem_ValueSet);
					return result;
				});
			}
		}

		internal CusHAWBAutoQueueMovementCollection DefaultCusHAWBAutoQueueMovements
		{
			get
			{
				if (fDefaultCusHAWBAutoQueueMovements == null)
				{
					fDefaultCusHAWBAutoQueueMovements = new CusHAWBAutoQueueMovementCollection();
					AddQuarantineCusHAWBQueueMovements(fDefaultCusHAWBAutoQueueMovements);
					AddShipperConsigneeInsufficientDetailsCusHAWBQueueMovements(fDefaultCusHAWBAutoQueueMovements);
				}
				return fDefaultCusHAWBAutoQueueMovements;
			}
		}

		void AddQuarantineCusHAWBQueueMovements(CusHAWBAutoQueueMovementCollection collection)
		{
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "QUARANTINE ACTION HOLD", 1));
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "CONDITIONAL RELEASE", 2));
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "DOCO ASSESSMENT", 3));
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "PENDING QUARANTINE ACTION", 4));
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "INSPECTION", 5));
			collection.Add(CreateNewQuarantineCusHAWBQueueMovement("QUARANTINE CARGO REPORT EVALUATION COMPLETE", "NO", 6));
		}

		void AddShipperConsigneeInsufficientDetailsCusHAWBQueueMovements(CusHAWBAutoQueueMovementCollection collection)
		{
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNEE ADDRESS", 7));
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNEE NAME", 8));
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNOR ADDRESS", 9));
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "DEFICIENT CONSIGNOR NAME", 10));
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "FULL CONSIGNEE NAME AND ADDRESS DETAILS ARE REQUIRED", 11));
			collection.Add(CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement("ACS/QUARANTINE IMPEDIMENT DETAILS", "FULL CONSIGNOR NAME AND ADDRESS DETAILS ARE REQUIRED", 12));
		}

		CusHAWBAutoQueueMovement CreateNewShipperConsigneeInsufficientDetailsCusHAWBQueueMovement(ZString freeTextSegmentName, ZString freeTextSegmentValue, ZInt priority)
		{
			CusHAWBAutoQueueMovement result = new CusHAWBAutoQueueMovement();
			result.FreeTextSegmentName = freeTextSegmentName;
			result.FreeTextSegmentValue = freeTextSegmentValue;
			result.Queue.QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Hold;
			result.Queue.Status = ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient;
			result.Queue.SubStatus = StatusCodeDescriptionPairList.Codes.TF_IncorrectAddressOrNeedPhoneNumber;
			result.Priority = priority;
			return result;
		}

		CusHAWBAutoQueueMovement CreateNewQuarantineCusHAWBQueueMovement(ZString freeTextSegmentName, ZString freeTextSegmentValue, ZInt priority)
		{
			CusHAWBAutoQueueMovement result = new CusHAWBAutoQueueMovement();
			result.FreeTextSegmentName = freeTextSegmentName;
			result.FreeTextSegmentValue = freeTextSegmentValue;
			result.Queue.QueueName = CargoReportQueueCodeDescriptionPairList.Codes.Quarantine;
			result.Queue.Status = ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold;
			result.Queue.SubStatus = StatusCodeDescriptionPairList.EmptyStatus;
			result.Priority = priority;
			return result;
		}

		void fCusHAWBAutoQueueMovementRegistryItem_ValueSet(object sender, EventArgs e)
		{
			ResetSortedCusHAWBAutoQueueMovements();
		}

		void ResetSortedCusHAWBAutoQueueMovements()
		{
			fSortedCusHAWBAutoQueueMovements = null;
		}

		CusHAWBAutoQueueMovementCollection fSortedCusHAWBAutoQueueMovements;
		CusHAWBAutoQueueMovementCollection fDefaultCusHAWBAutoQueueMovements;

		#endregion

		readonly string AutoQueueMovementCategory = Category + "/Auto Queue Movement";

		#endregion

		#region Process Queue Processor

		#region ProcessQueueProcessorHWM

		public ZDateTime ProcessQueueProcessorHWM
		{
			get { return new ZDateTime(ProcessQueueProcessorHWMItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ((IRegistryItem)ProcessQueueProcessorHWMItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToDateTime()); }
		}

		internal DateTimeRegistryItem ProcessQueueProcessorHWMItem
		{
			get
			{
				return GetItem("ProcessQueueProcessorHWM", delegate
				{
					return new DateTimeRegistryItem("ProcessQueueProcessorHWM", (NoResString)ProcessQueueProcessorCategory, (NoResString)"Time Stamp of UPS Queue Processor Last Run", null,
						LongFormat, RegistryStorageFlags.System, RegistryOptions.NotCached, new DateTime(2006, 3, 1, 0, 1, 0), false);
				});
			}
		}

		#endregion

		#region ProcessQueueProcessorTimeBuffer

		public ZInt ProcessQueueProcessorTimeBuffer
		{
			get { return ProcessQueueProcessorTimeBufferItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ProcessQueueProcessorTimeBufferItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem ProcessQueueProcessorTimeBufferItem
		{
			get
			{
				return GetItem("ProcessQueueProcessorTimeBuffer", delegate
				{
					return new IntRegistryItem("ProcessQueueProcessorTimeBuffer", (NoResString)ProcessQueueProcessorCategory, (NoResString)"UPS Queue Processor Time Buffer (in hours)", (NoResString)"This determines the maximum number of queue logs to be processed in a single batch.", new NumericRegistryEditorInfo(0), RegistryStorageFlags.System, RegistryOptions.Default, 24, 1, 168);
				});
			}
		}

		#endregion

		#region ProcessQueueProcessorRunTimeOffSet

		public ZInt ProcessQueueProcessorRunTimeOffSet
		{
			get { return ProcessQueueProcessorRunTimeOffSetItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { ProcessQueueProcessorRunTimeOffSetItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem ProcessQueueProcessorRunTimeOffSetItem
		{
			get
			{
				return GetItem("ProcessQueueProcessorRunTimeOffSet", delegate
				{
					return new IntRegistryItem("ProcessQueueProcessorRunTimeOffSet", (NoResString)ProcessQueueProcessorCategory, (NoResString)"UPS Queue Processor Run Time Offset (in seconds)", (NoResString)"UPS Queue Processor Run Time Offset (in seconds). If value is set to 60 seconds, service task will look for logs with timestamp < current time - 60 seconds.", new NumericRegistryEditorInfo(0), RegistryStorageFlags.System, RegistryOptions.Default, 60);
				});
			}
		}

		#endregion

		readonly string ProcessQueueProcessorCategory = Category + "/UPS Queue Processor";

		#endregion

		#region Document Image Importer

		#region DocumentImagingRepository

		public DirectoryInfo DocumentImagingRepository
		{
			get
			{
				DirectoryInfo result = new DirectoryInfo(Env.TempPath);
				string folder = new ZString(DocumentImagingRepositoryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
				{
					result = new DirectoryInfo(folder);
				}
				return result;
			}
			set { ((IRegistryItem)DocumentImagingRepositoryItem).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem DocumentImagingRepositoryItem
		{
			get
			{
				return GetItem("UPEDocumentImagingRepository", delegate
				{
					StringRegistryItem result = new StringRegistryItem("UPEDocumentImagingRepository", (NoResString)DocumentImageImporterCategory, (NoResString)"Document Image Repository", (NoResString)"Location where document images are picked up by the Document Imager Importer batch processor",
						RegistryStorageFlags.System, RegistryOptions.Default, Env.TempPath);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region DocumentImagingImageTypes

		public DocumentImageTypeRegistryItem DocumentImagingImageTypes
		{
			get
			{
				return GetItem("UPEDocumentImagingImageTypes", delegate
				{
					return new DocumentImageTypeRegistryItem(
						"UPEDocumentImagingImageTypes",
						DocumentImageImporterCategory,
						"Document Image Types",
						"The document image types that are imported from DIS. You must restart the batch processor for changes to take effect. An empty 'UPS Code' works as a catch-all for when the image index file's DocType is unknown.",
						GetDefaultDocumentImageTypes());
				});
			}
		}

		DocumentImageTypeCollection GetDefaultDocumentImageTypes()
		{
			DocumentImageTypeCollection result = new DocumentImageTypeCollection();
			result.SuspendValidation();
			try
			{
				result.AddNew(string.Empty, "MSC", "UPS document", false, false); // UPS to fix their document imaging system to allow for document types
																				  //Result.AddNew("CI", "UCI", "Commercial Invoice", true, false);
																				  //Result.AddNew("OT", "UOT", "Other Documents", false, true);
																				  //Result.AddNew("IP", "UIP", "Import Permit", true, false);
																				  //Result.AddNew("ON", "UON", "One-Time NAFTA/CO", false, true);
																				  //Result.AddNew("SW", "USW", "Shipment Waybill", false, false);
																				  //Result.AddNew("TD", "UTD", "Declaration", false, true);
																				  //Result.AddNew("CO", "UCO", "Certificate of origin", false, false);
																				  //Result.AddNew("PA", "UPA", "Power of attorney", false, true);
																				  //Result.AddNew("SD", "USD", "SED Documents", false, true);
																				  //Result.AddNew("PK", "UPK", "Packing List", false, false);
																				  //Result.AddNew("PI", "UPI", "Poor Image Quality", false, true);
			}
			finally
			{
				result.ResumeValidation();
			}
			return result;
		}

		#endregion

		#region DocumentImagingNotificationGroup

		public GuidRegistryItem DocumentImagingNotificationGroup
		{
			get
			{
				return GetItem("UPEDocumentImagingNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"UPEDocumentImagingNotificationGroup",
						(NoResString)DocumentImageImporterCategory,
						(NoResString)"Document Imaging Notification Group",
						(NoResString)"Notification group that document imaging emails are sent from",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DefaultNotificationGroupPK);
				});
			}
		}

		#endregion

		#region DocumentImagingNotificationsCache

		public StringRegistryItem DocumentImagingNotificationsCache
		{
			get
			{
				return GetItem("UPEDocumentImagingNotificationsCache", delegate
				{
					return new StringRegistryItem("UPEDocumentImagingNotificationsCache", (NoResString)DocumentImageImporterCategory, (NoResString)"Document Imaging Notification Cache", (NoResString)"Location where notifications are stored persistently during import to make the system ROBUST", RegistryStorageFlags.System, RegistryOptions.Default | RegistryOptions.IsOnlyForDevelopers, string.Empty);
				});
			}
		}

		#endregion

		readonly string DocumentImageImporterCategory = Category + "/Document Image Importer";

		#endregion

		#region SMS

		public IntRegistryItem SMSShipmentLoadDeadlineBeforeArrivalInMinutes
		{
			get
			{
				return GetItem("UPESMSShipmentLoadDeadlineBeforeArrivalInMinutes", delegate
				{
					return new IntRegistryItem("UPESMSShipmentLoadDeadlineBeforeArrivalInMinutes",
						(NoResString)SMSCategory,
						(NoResString)"Shipment Load Deadline Before Arrival",
						(NoResString)"Number of minutes before a master scheduled to arrival is SMS'd due to it not being loaded into the system",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						180);
				});
			}
		}

		public StringRegistryItem SMSEmailAddressSuffix
		{
			get
			{
				return GetItem("UPESMSEmailAddressSuffix", delegate
				{
					return new StringRegistryItem("UPESMSEmailAddressSuffix", (NoResString)SMSCategory, (NoResString)"SMS Email Address Suffix", (NoResString)"Recipient email address of the SMS provider that is appended to the user's mobile phone number.", RegistryStorageFlags.System, RegistryOptions.Default, ".fwd@messagenet.com.au");
				});
			}
		}

		public StringRegistryItem SMSNotificationGroup
		{
			get
			{
				return GetItem("UPESMSNotificationGroup", delegate
				{
					return new StringRegistryItem(
						"UPESMSNotificationGroup",
						(NoResString)SMSCategory,
						(NoResString)"SMS Notification Group",
						(NoResString)"The staff group code that will be sent flight notification SMS messages via their mobile phone numbers. The message is sent in the body of the email, with an empty subject. The provider must support a minimum of 160 characters. Allow up to 20 minutes for any changes to recipients of the SMS to take effect.",
						new StringRegistryDataType(1, GlbGroupSchema.GG_Code.MaxLength),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.NotCached,
						"SMS");
				});
			}
		}

		const string SMSCategory = Category + "/SMS";

		#endregion

		#region Dashboard Refresh
		public IntRegistryItem DashboardRefreshCycleTimeInMinutes
		{
			get
			{
				return GetItem("UPEDashboardRefreshCycleTimeInMinutes", delegate
				{
					return new IntRegistryItem("UPEDashboardRefreshCycleTimeInMinutes",
						(NoResString)Category,
						(NoResString)"Dashboard Automatic Refresh Time",
						(NoResString)"Number of minutes between consecutive automatic dashboard refresh",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5,
						1,
						30);
				});
			}
		}

		#endregion

		#region Helpers

		static readonly Guid DefaultNotificationGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");
		static readonly Guid DefaultBISIWarningReportGroupPK = new Guid("5952B279-2F88-44EB-8A9B-44D332EB6E27");

		string ConvertToString(string[] words)
		{
			StringWriter result = new StringWriter();

			foreach (ZString word in words)
			{
				if (!word.IsEmpty)
				{
					result.WriteLine(word.Trim());
				}
			}

			return result.GetStringBuilder().ToString().Trim();
		}

		DateTimeRegistryEditorInfo LongFormat
		{
			get { return longFormat ?? (longFormat = new DateTimeRegistryEditorInfo(Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long)); }
		}
		DateTimeRegistryEditorInfo longFormat;

		string[] ConvertToZStringArray(ZString word)
		{
			List<string> result = new List<string>();

			foreach (ZString splitWord in word.Replace("\r\n", "\n").Split('\n'))
			{
				if (!splitWord.IsEmpty)
				{
					result.Add(splitWord.Trim());
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Reports

		#region COD Manifest Report Zone Related Party

		public GuidRegistryItem CODManifestReportZoneRelatedPartyItem
		{
			get
			{
				return GetItem("CODManifestReportZoneRelatedPartyItem", delegate
				{
					return new GuidRegistryItem(
						"CODManifestReportZoneRelatedPartyItem",
						(NoResString)ReportCategory,
						(NoResString)"Zone Related Party",
						(NoResString)"The related party which the report will use in order to populate the Zone drop-down box on the filter",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Core.Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#endregion

		#region Implementation

		const string Category = "UPS Client Extensions";
		const string BISICategory = Category + "/BISI";
		const string BISIUploadCategory = Category + "/BISI/Upload";
		const string CODCategory = Category + "/COD";
		const string ReportCategory = Category + "/Reports";

		IEnumerable<Guid> activeCompaniesExcludingAustralia;
		IEnumerable<Guid> ActiveCompaniesCountriesExcludingAustralia
			=> activeCompaniesExcludingAustralia ?? (activeCompaniesExcludingAustralia = GlbCompany.GetActiveCompanies().Where(x => x.GC_IsActive && x.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Australia).Select(x => x.Country.PK.ToGuid()).Distinct().ToArray());

		#endregion
	}
}

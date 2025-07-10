using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.MFI
{
	internal sealed class MFIDataRegistry : RegistryItemSet
	{
		#region Instance

		public static MFIDataRegistry Instance
		{
			get { return instance ?? (instance = new MFIDataRegistry()); }
		}
		[ThreadStatic]
		static MFIDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region MFI New Zealand Specific

		#region Invoice Letterhead

		public Image InvoiceLetterhead
		{
			get { return InvoiceLetterheadItem.Value; }
			set { InvoiceLetterheadItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, value); }
		}

		public
 ImageRegistryItem InvoiceLetterheadItem
		{
			get
			{
				return GetItem<ImageRegistryItem>("InvoiceLetterhead", delegate
				{
					ImageRegistryItem result = new ImageRegistryItem(
						new NZVisibleImageRegistryItem(
						"InvoiceLetterhead",
						MainFreightNZCategory,
						"Invoice Letterhead",
						"This letterhead will be used when printing on pre-printed stationery.",
						new ImageRegistryDataType(),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch));

					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.NewZealand;
					return result;
				});
			}
		}

		#endregion

		#region Statement Letterhead

		public Image StatementLetterhead
		{
			get { return StatementLetterheadItem.Value; }
			set { StatementLetterheadItem.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, value); }
		}

		public
 ImageRegistryItem StatementLetterheadItem
		{
			get
			{
				return GetItem<ImageRegistryItem>("StatementLetterhead", delegate
				{
					ImageRegistryItem result = new ImageRegistryItem(
						new NZVisibleImageRegistryItem(
						"StatementLetterhead",
						MainFreightNZCategory,
						"Statement Letterhead",
						"This letterhead will be used when the statement is emailed or faxed.",
						new ImageRegistryDataType(),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch));

					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.NewZealand;
					return result;
				});
			}
		}

		#endregion

		#region New Zealand Specific Registry Item

		class NZVisibleImageRegistryItem : RegistryItemImpl
		{
			public NZVisibleImageRegistryItem(string name, string category, string caption, string hint, IRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, (NoResString)category, (NoResString)caption, (NoResString)hint, dataType, storage)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return (base.IsVisible(companyPK, branchPK, departmentPK, department, registryItemVisibility) && MFIConstants.NZ.ClientSpecificCondition);
			}
		}

		#endregion

		#endregion

		#region CaroTrans

		#region CaroTransAgents

		public Guid[] CaroTransAgents
		{
			get { return CaroTransAgentsItem.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { CaroTransAgentsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region CaroTransNextExportRun

		public ZDateTime CaroTransNextExportRun
		{
			get { return (CaroTransNextExportRunItem.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(CaroTransNextExportRunItem.Value); }
			set { CaroTransNextExportRunItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (value.IsValid ? value.ToDateTime() : DateTime.MinValue)); }
		}

		#endregion

		#region CaroTransTrackEmailAddress

		public ZString CaroTransTrackEmailAddress
		{
			get { return new ZString(CaroTransTrackEmailAddressItem.Value); }
			set { CaroTransTrackEmailAddressItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region CaroTransHighWaterMark

		public ZDateTime CaroTransHighWaterMark
		{
			get { return (CaroTransHighWaterMarkItem.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(CaroTransHighWaterMarkItem.Value); }
			set { CaroTransHighWaterMarkItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (value.IsValid ? value.ToDateTime() : DateTime.MinValue)); }
		}

		#endregion

		#region CaroTransExportFileExtension

		public ReadOnlyCodeDescriptionPairList CaroTransExportFileExtensions
		{
			get { return CaroTransExportFileExtensionItem.Value; }
			set { CaroTransExportFileExtensionItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

#endregion

		#region AutoeDocAllocation

		#region AutoeDocAllocationNotificationGroup

		public Guid AutoeDocAllocationNotificationGroup
		{
			get { return AutoeDocAllocationNotificationGroupItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { AutoeDocAllocationNotificationGroupItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, value); }
		}

		#endregion

		#region AutoeDocAllocationSourceDirectory

		public ZString AutoeDocAllocationSourceDirectory
		{
			get { return new ZString(AutoeDocAllocationSourceDirectoryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { AutoeDocAllocationSourceDirectoryItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region AutoeDocAllocationHoldDirectory

		public ZString AutoeDocAllocationHoldDirectory
		{
			get { return new ZString(AutoeDocAllocationHoldDirectoryItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
			set { AutoeDocAllocationHoldDirectoryItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion

		#region AutoeDocAllocationHoldPeriod

		public ZInt AutoeDocAllocationHoldPeriod
		{
			get { return AutoeDocAllocationHoldPeriodItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
			set { AutoeDocAllocationHoldPeriodItem.SetValue(CurrentCompanyPKGuid, Guid.Empty, Guid.Empty, (int)value); }
		}

		#endregion

		#endregion

		#region Implementation

		const string Category = "MFI Client Extensions";
		const string CaroTransCategory = Category + "/CaroTrans Tracking Export";
		const string MainFreightNZCategory = Category + "/MFI New Zealand";
		const string AutoeDocCategory = Category + "/Auto eDoc Allocation";

		#region CaroTrans

		#region CaroTransAgentsItem

		public
 GuidArrayRegistryItem CaroTransAgentsItem
		{
			get
			{
				return GetItem<GuidArrayRegistryItem>("CaroTransAgents", delegate
				{
					GuidArrayRegistryItem result = new GuidArrayRegistryItem("CaroTransAgents", (NoResString)CaroTransCategory, (NoResString)"Agents", (NoResString)"Agents to export data for", RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.OrgHeaderCodeListEdit);
					return result;
				});
			}
		}

		#endregion

		#region CaroTransNextExportRunItem

		public
 DateTimeRegistryItem CaroTransNextExportRunItem
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("CaroTransDataExportNextRun", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem("CaroTransDataExportNextRun", (NoResString)CaroTransCategory, (NoResString)"Data Export Next Run", (NoResString)"The time that batch server will next export the data", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long);
					return result;
				});
			}
		}

		#endregion

		#region CaroTransTrackEmailAddressItem

		public
 StringRegistryItem CaroTransTrackEmailAddressItem
		{
			get
			{
				return GetItem<StringRegistryItem>("CaroTransTrackEmailAddress", delegate
				{
					return new StringRegistryItem("CaroTransTrackEmailAddress", (NoResString)CaroTransCategory, (NoResString)"Track Email Address", (NoResString)"Email address to send the exported shipments data to", RegistryStorageFlags.System, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region CaroTransHighWaterMarkItem

		public
 DateTimeRegistryItem CaroTransHighWaterMarkItem
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("CaroTransHighWaterMark", delegate
				{
					return new DateTimeRegistryItem("CaroTransHighWaterMark", (NoResString)CaroTransCategory, (NoResString)"High Water Mark",
						(NoResString)"The time from then on logs get considered to export shipments data",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long), RegistryStorageFlags.System, RegistryOptions.NotCached,
						SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value, false);
				});
			}
		}

		#endregion

		#region CaroTransExportFileExtensionItem

		public CodeDescriptionPairListRegistryItem CaroTransExportFileExtensionItem
		{
			get
			{
				return GetItem<CodeDescriptionPairListRegistryItem>("CaroTransImportCountryFileExtension", delegate
				{
					CodeDescriptionPairListRegistryItem result = new CodeDescriptionPairListRegistryItem(
						"CaroTransImportCountryFileExtension",
						(NoResString)CaroTransCategory,
						(NoResString)"Export File Extension",
						(NoResString)"Enter the Import Country/Region Specific File Extension of the Exported Data.\r\n(e.g. Import Country/Region Code = 'AU', File Extension = 'mfa')",
						2,
						RegistryStorageFlags.System,
						false,
						RegistryOptions.Default,
						new ReadOnlyCodeDescriptionPairList(),
						true
						);

					CodeDescriptionPairListEditorInfo editorInfo = (CodeDescriptionPairListEditorInfo)result.EditorInfo;
					editorInfo.SetCodeColumnCaption((NoResString)"Import Country/Region Code");
					editorInfo.SetDescriptionColumnCaption((NoResString)"File Extension");

					return result;
				});
			}
		}

		#endregion
		#endregion

		#region AutoeDocAllocation

		#region AutoeDocAllocationNotificationGroupItem
		public
 GuidRegistryItem AutoeDocAllocationNotificationGroupItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("AutoeDocAllocationNotificationGroupItem", delegate
{
	return new GuidRegistryItem("AutoeDocAllocationNotificationGroupItem", (NoResString)AutoeDocCategory, (NoResString)"Notification Group", (NoResString)"The Email Notification Group to which eDoc allocation rejection emails are to be sent.", new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup), RegistryStorageFlags.Company, RegistryOptions.NotCached, DefaultPostmasterGroupPK);
});
			}
		}

		internal static readonly Guid DefaultPostmasterGroupPK = new Guid("55896E13-12BE-4FD4-AC94-2956795A5BE2");

		#endregion

		#region AutoeDocAllocationSourceDirectoryItem
		public
 StringRegistryItem AutoeDocAllocationSourceDirectoryItem
		{
			get
			{
				return GetItem<StringRegistryItem>("AutoeDocAllocationSourceDirectoryItem", delegate
{
	return new StringRegistryItem("AutoeDocAllocationSourceDirectoryItem", (NoResString)AutoeDocCategory, (NoResString)"Source Directory", (NoResString)"Directory for storing files to process. The directory where files need to be put if they are to be processed by the Auto eDocs Allocation routine", new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser), RegistryStorageFlags.Company, RegistryOptions.Default, String.Empty);
});
			}
		}

		#endregion

		#region AutoeDocAllocationHoldDirectoryItem
		public
 StringRegistryItem AutoeDocAllocationHoldDirectoryItem
		{
			get
			{
				return GetItem<StringRegistryItem>("AutoeDocAllocationHoldDirectoryItem", delegate
{
	return new StringRegistryItem("AutoeDocAllocationHoldDirectoryItem", (NoResString)AutoeDocCategory, (NoResString)"Hold Directory", (NoResString)"Directory for storing unmatched files. The directory where files are moved when they have failed to find a match on the first run of the eDocs Allocation", new StringRegistryDataType(), new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser), RegistryStorageFlags.Company, RegistryOptions.Default, String.Empty);
});
			}
		}

		#endregion

		#region AutoeDocAllocationHoldPeriodItem
		public
 IntRegistryItem AutoeDocAllocationHoldPeriodItem
		{
			get
			{
				return GetItem<IntRegistryItem>("AutoeDocAllocationHoldPeriodItem", delegate
{
	return new IntRegistryItem("AutoeDocAllocationHoldPeriodItem", (NoResString)AutoeDocCategory, (NoResString)"Time limit for storing unmatched files", (NoResString)"The period, in days, a rejected file will be held before it is be deleted and emailed to the Auto eDocs Allocation notification group.", RegistryStorageFlags.Company, RegistryOptions.Default);
});
			}
		}

		#endregion

		Guid CurrentCompanyPKGuid
		{
			get { return GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		#endregion

		#endregion
	}
}

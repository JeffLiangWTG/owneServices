using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Wow
{
	public sealed class WowDataRegistry : RegistryItemSet
	{
		WowDataRegistry()
		{
		}

		#region Instance

		public static WowDataRegistry Instance
		{
			get { return instance ?? (instance = new WowDataRegistry()); }
		}
		[ThreadStatic]
		static WowDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Category

		const string Category = "Woolworths Client Extensions";
		const string ImportProductCategory = Category + "/Import of Products from Managing Imports";
		const string ImportOrderDecCategory = Category + "/Import Order Customs Declaration";
		const string CASSKIRKIntegrationCategory = Category + "/CASS KIRK Order Integration";
		const string ExportCommercialInvoiceDataCategory = Category + "/Export Commercial Invoice Data";
		const string TaskExecuteTimeCategory = Category + "/Task Execute Time";

		#endregion

		#region UnmatchedDataItemsAccount and related methods

		public ZGuid UnmatchedDataItemsAccount
		{
			get { return new ZGuid(UnmatchedDataItemsAccountRaw.Value); }
			set { UnmatchedDataItemsAccountRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsEmpty ? Guid.Empty : value.ToGuid()); }
		}

		public bool IsUnmatchedDataItemsAccountValid(BusinessObjectFactory factory)
		{
			OrgHeader org = (OrgHeader)factory.Load(typeof(OrgHeader), UnmatchedDataItemsAccount);
			return (org != null);
		}

		public OrgHeader LoadUnmatchDataItemsAccount(BusinessObjectFactory factory)
		{
			return (OrgHeader)factory.Load(typeof(OrgHeader), UnmatchedDataItemsAccount) ?? throw new ApplicationException("Could not find UnmatchedDataItemsAccount");
		}

		#endregion
		#region ManagingImportsOutputDirectory

		public ZString ManagingImportsOutputDirectory
		{
			get { return new ZString(ManagingImportsOutputDirectoryRaw.Value); }
			set { ManagingImportsOutputDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		#endregion
		#region FilterImportedEmailsBySpecificSubject

		public BooleanRegistryItem FilterEmailImportFilesBySpecificSubject
		{
			get
			{
				return GetItem("FilterEmailImportFilesBySpecificSubject", delegate
				{
					return new BooleanRegistryItem(
						"FilterEmailImportFilesBySpecificSubject",
						(NoResString)Category,
						(NoResString)"Filter Emails by Specific Subject",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		#region ContainerCustomsDeclLastCreated

		public ZDateTime ContainerCustomsDeclLastCreated
		{
			get { return new ZDateTime(ContainerCustomsDeclLastCreatedRaw.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty)); }
			set { ContainerCustomsDeclLastCreatedRaw.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, value.IsEmpty ? new DateTime() : value.ToDateTime()); }
		}

		#endregion

		#region EnableOrderNumberFountain

		public ZBool EnableOrderNumberFountain
		{
			get { return EnableOrderNumberFountainRaw.Value; }
			set { EnableOrderNumberFountainRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (bool)value); }
		}

		#endregion

		#region ServiceTasks

		#region DeclarationInvoice

		public ZString DeclarationInvoiceExportDirectory
		{
			get { return DeclarationInvoiceExportDirectoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { DeclarationInvoiceExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public ZDateTime DeclarationInvoiceExportLastRun
		{
			get { return new ZDateTime(DeclarationInvoiceExportLastRunRaw.Value); }
			set { DeclarationInvoiceExportLastRunRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.IsEmpty ? new DateTime() : value.ToDateTime()); }
		}

		#endregion

		#endregion

		#region TaskExecuteTime

		public TimeSpan DailyTaskExecuteTime
		{
			get { return ParseTimeSpan(DailyTaskExecuteTimeRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { DailyTaskExecuteTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, String.Format("{0}:{1}", value.Hours, value.Minutes)); }
		}

		public TimeSpan MidnightTaskExecuteTime
		{
			get { return ParseTimeSpan(MidnightTaskExecuteTimeRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { MidnightTaskExecuteTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, String.Format("{0}:{1}", value.Hours, value.Minutes)); }
		}

		TimeSpan ParseTimeSpan(string text)
		{
			int hours = -1;
			int minutes = -1;

			try
			{
				string[] timeParts = text.Trim().Split(':');
				if (timeParts.Length == 2)
				{
					int part1 = Convert.ToInt32(timeParts[0]);
					if (part1 >= 0 && part1 <= 23)
					{
						hours = part1;
					}

					int part2 = Convert.ToInt32(timeParts[1]);
					if (part2 >= 0 && part2 <= 59)
					{
						minutes = part2;
					}
				}
			}
			catch
			{
				throw new ArgumentException("Incorrect time format.");
			}

			if (hours == -1 || minutes == -1)
			{
				throw new ArgumentException("Incorrect time format.");
			}

			return new TimeSpan(hours, minutes, 0);
		}

		#endregion

		#region Implementation

		#region UnmatchedDataItemsAccountRaw

		internal GuidRegistryItem UnmatchedDataItemsAccountRaw
		{
			get
			{
				return GetItem("UnmatchedDataItemsAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("UnmatchedDataItemsAccount", (NoResString)Category, (NoResString)"Unmatched Data Items Account", null, RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion

		#region ManagingImportsOutputDirectoryRaw

		internal StringRegistryItem ManagingImportsOutputDirectoryRaw
		{
			get
			{
				return GetItem("ManagingImportsOutputDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("ManagingImportsOutputDirectory", (NoResString)ImportProductCategory, (NoResString)"Backup Directory", null, RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion
		#region Create Declaration from Order

		internal DateTimeRegistryItem ContainerCustomsDeclLastCreatedRaw
		{
			get
			{
				return GetItem("ContainerCustomsDeclLastCreatedRaw", delegate
				{
					return new DateTimeRegistryItem("ContainerCustomsDeclLastCreatedRaw", (NoResString)ImportOrderDecCategory, (NoResString)"Last Run Order Customs Declaration", null, RegistryStorageFlags.System | RegistryStorageFlags.Branch, RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers);
				});
			}
		}

		internal GuidRegistryItem DeclarationImporterRaw
		{
			get
			{
				return GetItem("WoolworthsJobDecImporter", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"WoolworthsJobDecImporter",
						(NoResString)ImportOrderDecCategory,
						(NoResString)"Declaration Importer",
						(NoResString)"Default Importer For Declaration created From Orders",
						RegistryStorageFlags.Branch, RegistryOptions.NotCached);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		public ZGuid DeclarationImporter
		{
			get { return DeclarationImporterRaw.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty); }
			set { DeclarationImporterRaw.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, value.ToGuid()); }
		}

		public ZGuid GetDeclarationImporter(GlbBranch branch)
		{
			return DeclarationImporterRaw.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
		}

		public void SetDeclarationImporter(GlbBranch branch, ZGuid value)
		{
			DeclarationImporterRaw.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, value.ToGuid());
		}

		#endregion

		#region ContainerCustomsDeclLastCreatedRaw

		internal BooleanRegistryItem EnableOrderNumberFountainRaw
		{
			get
			{
				return GetItem("WowEnableOrderNumberFountain", delegate
				{
					return new BooleanRegistryItem("WowEnableOrderNumberFountain", (NoResString)Category, (NoResString)"Enable Auto Order Numbers", null, RegistryStorageFlags.System, RegistryOptions.NotCached, false);
				});
			}
		}

		#endregion

		#region ServiceTasksRaw

		#region DeclarationInvoice

		internal StringRegistryItem DeclarationInvoiceExportDirectoryRaw
		{
			get
			{
				return GetItem("DeclarationInvoiceExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("DeclarationInvoiceExportDirectory",
						(NoResString)ExportCommercialInvoiceDataCategory,
						(NoResString)"Export Directory",
						(NoResString)"Directory to put exported commercial invoice files to.",
						RegistryStorageFlags.System,
						Env.TempPath);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		internal DateTimeRegistryItem DeclarationInvoiceExportLastRunRaw
		{
			get
			{
				return GetItem("DeclarationInvoiceExportLastRun", delegate
				{
					return new DateTimeRegistryItem("DeclarationInvoiceExportLastRun",
						(NoResString)ExportCommercialInvoiceDataCategory,
						(NoResString)"Last Run",
						(NoResString)"Last run for export.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						DateTime.MinValue,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region WoolworthsImporter

		public Guid WoolworthsImporter
		{
			get { return WOWImporterItem.Value; }
			set { WOWImporterItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal GuidRegistryItem WOWImporterItem
		{
			get
			{
				return GetItem("WoolworthsImporter", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"WoolworthsImporter",
						(NoResString)Category,
						(NoResString)"Woolworths Importer",
						(NoResString)"Default Importer For Woolworths Declaration",
						RegistryStorageFlags.System, RegistryOptions.NotCached);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					return result;
				});
			}
		}

		#endregion
		#region CASS / KIRK  Integration

		#region CASSKIRKDisplayOption

		internal BooleanRegistryItem CASSKIRKDisplayOptionRaw
		{
			get
			{
				return GetItem("CASSKIRKDisplayOptionRaw", delegate
				{
					return new BooleanRegistryItem("CASSKIRKDisplayOptionRaw", (NoResString)CASSKIRKIntegrationCategory, (NoResString)"Purchase Order Display Option", (NoResString)"Display option for specified branches to show/hide VPN and GTIN columns in purchase order document output", RegistryStorageFlags.Branch, RegistryOptions.NotCached, false);
				});
			}
		}

		internal ZBool CASSKIRKDisplayOption
		{
			get { return CASSKIRKDisplayOptionRaw.GetFallBackValueAtAllLevels(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty); }
			set { CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, value); }
		}

		internal ZBool GetCASSKIRKDisplayOption(GlbBranch branch)
		{
			return CASSKIRKDisplayOptionRaw.GetFallBackValueAtAllLevels(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
		}

		internal void SetCASSKIRKDisplayOption(GlbBranch branch, ZBool value)
		{
			CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, value);
		}

		#endregion

		#endregion

		#region TaskExecuteTime

		internal StringRegistryItem DailyTaskExecuteTimeRaw
		{
			get
			{
				return GetItem("DailyTaskExecuteTime", delegate
				{
					StringRegistryItem result = new StringRegistryItem("DailyTaskExecuteTime",
						(NoResString)TaskExecuteTimeCategory,
						(NoResString)"Daily Task Execute Time",
						(NoResString)"Daily Task Execute Time (hh:mm).",
						RegistryStorageFlags.System,
						"18:00");
					return result;
				});
			}
		}

		internal StringRegistryItem MidnightTaskExecuteTimeRaw
		{
			get
			{
				return GetItem("MidnightTaskExecuteTime", delegate
				{
					StringRegistryItem result = new StringRegistryItem("MidnightTaskExecuteTime",
						(NoResString)TaskExecuteTimeCategory,
						(NoResString)"Midnight Task Execute Time",
						(NoResString)"Midnight Task Execute Time (hh:mm).",
						RegistryStorageFlags.System,
						"00:00");
					return result;
				});
			}
		}

		#endregion

	}
}

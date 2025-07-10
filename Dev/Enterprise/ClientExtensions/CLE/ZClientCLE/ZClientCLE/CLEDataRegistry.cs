using System;
using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.CLE
{
	public sealed class CLEDataRegistry : RegistryItemSet
	{
		CLEDataRegistry()
		{
		}

		#region Instance
		public static CLEDataRegistry Instance
		{
			get { return instance ?? (instance = new CLEDataRegistry()); }
		}
		[ThreadStatic]
		static CLEDataRegistry instance;
		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "CLE Client Specific";
		const string MattelCategory = ARInvoiceExportCategory + "/Mattel AR Invoices Export";
		const string ContainerDatesImportCategory = Category + "/Container Dehire Upload";
		const string ARInvoiceExportCategory = Category + "/AR Invoices Export";
		const string OrdersImportCategory = Category + "/Orders Import";

		#region Mattel

		#region MattelEmailAddress

		public ZString MattelEmailAddress
		{
			get { return new ZString(MattelEmailAddressRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { MattelEmailAddressRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal IRegistryItem MattelEmailAddressRaw
		{
			get
			{
				return GetItem("MattelEmailAddress", delegate
				{
					return new StringRegistryItem("MattelEmailAddress", (NoResString)MattelCategory, (NoResString)"Mattel's Email Address", (NoResString)"Email Address the Data Export Files to be sent to", new EmailStringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.NotCached, "");
				});
			}
		}

		#endregion

		#region Clemenger EmailAddress

		public ZString ClemengerEmailAddress
		{
			get { return new ZString(ClemengerEmailAddressRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ClemengerEmailAddressRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal IRegistryItem ClemengerEmailAddressRaw
		{
			get
			{
				return GetItem("CLEFromEmailAddress", delegate
				{
					return new StringRegistryItem("CLEFromEmailAddress", (NoResString)MattelCategory, (NoResString)"Clemenger's Email Address", (NoResString)"Clemenger 's Email Address the Data Export Response to be sent to", new EmailStringRegistryDataType(), null, RegistryStorageFlags.System, RegistryOptions.NotCached, "");
				});
			}
		}

		#endregion

		#region MattelEmailSubject

		public ZString MattelEmailSubject
		{
			get { return new ZString(MattelEmailSubjectRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { MattelEmailSubjectRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal IRegistryItem MattelEmailSubjectRaw
		{
			get
			{
				return GetItem("MattelEmailSubject", delegate
				{
					return new StringRegistryItem("MattelEmailSubject", (NoResString)MattelCategory, (NoResString)"Email Subject", (NoResString)"Email Subject to be used when sending Output Files", RegistryStorageFlags.System, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region MattelMailboxNumber

		public ZString MattelMailboxNumber
		{
			get { return new ZString(MattelMailboxNumberRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { MattelMailboxNumberRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal IRegistryItem MattelMailboxNumberRaw
		{
			get
			{
				return GetItem("MattelMailboxNumber", delegate
				{
					return new StringRegistryItem("MattelMailboxNumber", (NoResString)MattelCategory, (NoResString)"Mattel MailBox Number", (NoResString)"Enter the Mattel MailBox Number including the separator and qualifier.\r\nFor Example: 'MATTEL:ZZZ'", RegistryStorageFlags.System, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region ClemengerMailboxNumber

		public ZString ClemengerMailboxNumber
		{
			get { return new ZString(ClemengerMailboxNumberRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ClemengerMailboxNumberRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal IRegistryItem ClemengerMailboxNumberRaw
		{
			get
			{
				return GetItem("ClemengerMailboxNumber", delegate
				{
					return new StringRegistryItem("ClemengerMailboxNumber", (NoResString)MattelCategory, (NoResString)"Clemenger MailBox Number", (NoResString)"Enter the Clemenger MailBox Number including the separator and qualifier.\r\nFor Example: 'CLEMENT:ZZ'", RegistryStorageFlags.System, RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region Mattel Debtor

		public ZGuid MattelDebtor
		{
			get { return MattelDebtorRaw.Value; }
			set { MattelDebtorRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		internal GuidRegistryItem MattelDebtorRaw
		{
			get
			{
				return GetItem("MattelDebtor", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("MattelDebtor", (NoResString)MattelCategory, (NoResString)"Mattel Debtor", (NoResString)"Invoices of which will to be exported", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Debtor);
					return result;
				});
			}
		}

		#endregion

		#region MattelHighWaterMark

		public ZDateTime ARInvoiceExportHighWaterMark
		{
			get { return (ARInvoiceExportHighWaterMarkRaw.Value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(ARInvoiceExportHighWaterMarkRaw.Value); }
			set { ARInvoiceExportHighWaterMarkRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (value.IsValid ? value.ToDateTime() : DateTime.MinValue)); }
		}

		internal DateTimeRegistryItem ARInvoiceExportHighWaterMarkRaw
		{
			get
			{
				return GetItem("MattelHighWaterMark", delegate
				{
					return new DateTimeRegistryItem("MattelHighWaterMark", (NoResString)ARInvoiceExportCategory, (NoResString)"High Water Mark", (NoResString)"The Time from then on logs get considered to Export Invoices Data. For Go Live, Set the Value as Starting Point for the First Data Export. Afterwards, the System Automatically Updates the Value.", new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long), RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value, false);
				});
			}
		}

		#endregion

		#endregion

		#region ContainerDatesImport

		public GuidRegistryItem ContainerUploadEmailNotificationGroup
		{
			get
			{
				return GetItem("ContainerUploadEmailNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ContainerUploadEmailNotificationGroup",
						(NoResString)ContainerDatesImportCategory,
						(NoResString)"Email Notification Group",
						(NoResString)"The staff group that will be receiving the Exception Report after Container De-Hire Upload.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Guid.Empty);
					return result;
				});
			}
		}

		#endregion

		#region Orders Data Import

		#region Maximum Number of Orders to Deliver at a time

		public int MaximumOrdersToDeliver
		{
			get { return MaximumOrdersToDeliverRaw.Value; }
			set { MaximumOrdersToDeliverRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal IntRegistryItem MaximumOrdersToDeliverRaw
		{
			get
			{
				return GetItem("MaximumOrdersToDeliver", delegate
				{
					return new IntRegistryItem(
						"MaximumOrdersToDeliver",
						(NoResString)OrdersImportCategory,
						(NoResString)"Maximum Number of Orders to Deliver at a time",
						(NoResString)"Set this value to adjust the maximum number of orders to deliver at a time",
						RegistryStorageFlags.System,
						100);
				});
			}
		}

		#endregion

		#region Order Data Import Switch Registry Item

		public ZString OrderImportDirectory
		{
			get { return SwitchOrderImportItem.Value.Directory; }
		}

		public ZGuid OrderImportNotifyGroupPK
		{
			get { return SwitchOrderImportItem.GetValueWithoutFallback(CurrentCompanyPK, Guid.Empty, Guid.Empty).GroupPK; }
		}

		public bool EnableOrdersDataImportInterface
		{
			get { return SwitchOrderImportItem.GetValueWithoutFallback(CurrentCompanyPK, Guid.Empty, Guid.Empty).EnableInterface; }
		}

		internal ServiceTaskDataTransferSwitchRegistryItem SwitchOrderImportItem
		{
			get
			{
				return GetItem("SwitchOrderImportItem", delegate
				{
					return new ServiceTaskDataTransferSwitchRegistryItem(
						"SwitchOrderImportItem",
						(NoResString)OrdersImportCategory,
						(NoResString)"Order Import Settings",
						null,
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#endregion

		Guid CurrentCompanyPK
		{
			get { return Env.CurrentCompany.PK; }
		}
	}
}

#region Test
#region TestUserVisibleRegistryItems
#endregion
#region TestMattelDebtor
#endregion
#region TestMattelDataExportFrequency
#endregion
#region TestMattelEmailAddress
#endregion
#region TestMattelEmailAddress
#endregion
#region TestMattelEmailSubject
#endregion
#region TestMattelMailboxNumber
#endregion
#region TestClemengerMailboxNumber
#endregion
#region TestMattelHighWaterMark
#endregion
#region Order Import Interface
#endregion
#endregion

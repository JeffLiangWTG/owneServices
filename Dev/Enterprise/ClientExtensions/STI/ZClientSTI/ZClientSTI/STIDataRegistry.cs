using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.STI
{
	public sealed class STIDataRegistry : RegistryItemSet
	{
		STIDataRegistry()
		{
		}

		#region Instance
		public static STIDataRegistry Instance
		{
			get { return fInstance ?? (fInstance = new STIDataRegistry()); }
		}
		[ThreadStatic]
		static STIDataRegistry fInstance;
		#endregion

		public override bool IsForProductivityWise => false;

		#region Constants

		const string Category = "Strang International Client Extensions";
		const string NavisionCategory = Category + "/Navision";

		#endregion

		#region OrganisationExportDirectory

		public ZString OrganisationExportDirectory
		{
			get { return new ZString(OrganisationExportDirectoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { OrganisationExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem OrganisationExportDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("OrganisationExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("OrganisationExportDirectory", (NoResString)NavisionCategory, (NoResString)"Organisation Export Directory", (NoResString)"Specify Path to export Organisation files for Navision", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region InvoiceHeaderExportDirectory

		public ZString InvoiceHeaderExportDirectory
		{
			get { return new ZString(InvoiceHeaderExportDirectoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { InvoiceHeaderExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem InvoiceHeaderExportDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("InvoiceHeaderExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("InvoiceHeaderExportDirectory", (NoResString)NavisionCategory, (NoResString)"Invoice Header Export Directory", (NoResString)"Specify Path to Export Invoice Header files for Navision", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region InvoiceLinesExportDirectory

		public ZString InvoiceLinesExportDirectory
		{
			get { return new ZString(InvoiceLinesExportDirectoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { InvoiceLinesExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem InvoiceLinesExportDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("InvoiceLinesExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("InvoiceLinesExportDirectory", (NoResString)NavisionCategory, (NoResString)"Invoice Lines Export Directory", (NoResString)"Specify Path to Export Invoice Line files for Navision", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region ShipmentExportDirectory

		public ZString ShipmentExportDirectory
		{
			get { return new ZString(ShipmentExportDirectoryRaw.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)); }
			set { ShipmentExportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem ShipmentExportDirectoryRaw
		{
			get
			{
				return GetItem<StringRegistryItem>("ShipmentExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("ShipmentExportDirectory", (NoResString)NavisionCategory, (NoResString)"Shipment Export Directory", (NoResString)"Specify Path to Export Shipment files for Navision", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region DataExportLastRun

		public ZDateTime DataExportHighWaterMark
		{
			get { return ConvertToZDateTime(DataExportHighWaterMarkRaw.Value); }
			set { DataExportHighWaterMarkRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ConvertToDateTime(value)); }
		}

		internal DateTimeRegistryItem DataExportHighWaterMarkRaw
		{
			get
			{
				return GetItem<DateTimeRegistryItem>("DataExportHighWaterMark", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
						"DataExportHighWaterMark",
						(NoResString)NavisionCategory,
						(NoResString)"Data Export High Water Mark",
						(NoResString)"The time from which the logs get considered to export data",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						SystemDataRegistry.Instance.SystemLogBatchProcessHighWaterMark.Value,
						false);
					return result;
				});
			}
		}

		#endregion

		#region ExportingOrganisationsForTheFirstTime

		public bool ExportingOrganisationsForTheFirstTime
		{
			get { return ExportingOrganisationsForTheFirstTimeRaw.Value; }
			set { ExportingOrganisationsForTheFirstTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem ExportingOrganisationsForTheFirstTimeRaw
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ExportingOrganisationsForTheFirstTime", delegate
				{
					return new BooleanRegistryItem("ExportingOrganisationsForTheFirstTime", (NoResString)NavisionCategory, (NoResString)"Should Export All Organisations", null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, true);
				});
			}
		}

		#endregion

		#region DisableOrganisationExport

		public bool DisableOrganisationExport
		{
			get { return DisableOrganisationExportRaw.Value; }
			set { DisableOrganisationExportRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem DisableOrganisationExportRaw
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DisableOrganisationExport", delegate
				{
					return new BooleanRegistryItem("DisableOrganisationExport", (NoResString)NavisionCategory, (NoResString)"Disable Organisation Export", null, RegistryStorageFlags.System, RegistryOptions.Default, false);
				});
			}
		}

		#endregion

		#region DisableShipmentExport

		public bool DisableShipmentExport
		{
			get { return DisableShipmentExportRaw.Value; }
			set { DisableShipmentExportRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem DisableShipmentExportRaw
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DisableShipmentExport", delegate
				{
					return new BooleanRegistryItem("DisableShipmentExport", (NoResString)NavisionCategory, (NoResString)"Disable Shipment Export", null, RegistryStorageFlags.System, RegistryOptions.Default, false);
				});
			}
		}

		#endregion

		#region DisableInvoiceExport

		public bool DisableInvoiceExport
		{
			get { return DisableInvoiceExportRaw.Value; }
			set { DisableInvoiceExportRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem DisableInvoiceExportRaw
		{
			get
			{
				return GetItem<BooleanRegistryItem>("DisableInvoiceExport", delegate
				{
					return new BooleanRegistryItem("DisableInvoiceExport", (NoResString)NavisionCategory, (NoResString)"Disable Invoice Export", null, RegistryStorageFlags.System, RegistryOptions.Default, false);
				});
			}
		}

		#endregion

		#region Implementation

		DateTime ConvertToDateTime(ZDateTime value)
		{
			return value.IsValid ? value.ToDateTime() : DateTime.MinValue;
		}

		ZDateTime ConvertToZDateTime(DateTime value)
		{
			return (value == DateTime.MinValue) ? ZDateTime.Empty : new ZDateTime(value);
		}

		#endregion
	}
}

using System;

using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.WCB
{
	public sealed class WCBDataRegistry : RegistryItemSet
	{
		WCBDataRegistry()
		{
		}

		#region Instance
		public static WCBDataRegistry Instance
		{
			get { return instance ?? (instance = new WCBDataRegistry()); }
		}
		[ThreadStatic]
		static WCBDataRegistry instance;
		#endregion

		public override bool IsForProductivityWise => false;

		#region ChryslerImporter
		public ZGuid ChryslerImporter
		{
			get { return new ZGuid(ChryslerImporterItem.Value); }
		}

		internal GuidRegistryItem ChryslerImporterItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("WCBChryslerImporter", delegate
				{
					return new GuidRegistryItem("WCBChryslerImporter", (NoResString)MercedeChryslerMotoVehicleInterfaceCategory,
						(NoResString)"Chrysler Importer", (NoResString)"Chrysler Importer for which to import declaration data.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached, Guid.Empty);
				});
			}
		}
		#endregion

		#region MercedesImporter
		public ZGuid MercedesImporter
		{
			get { return new ZGuid(MercedesImporterItem.Value); }
		}

		internal GuidRegistryItem MercedesImporterItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("WCBMercedesImporter", delegate
				{
					return new GuidRegistryItem("WCBMercedesImporter", (NoResString)MercedeChryslerMotoVehicleInterfaceCategory,
						(NoResString)"Mercedes Importer", (NoResString)"Mercedes Importer for which to import declaration data.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader),
						RegistryStorageFlags.System,
						RegistryOptions.NotCached, Guid.Empty);
				});
			}
		}
		#endregion

		#region MercedesSupplier
		public ZGuid MercedesSupplier
		{
			get { return new ZGuid(MercedesSupplierItem.Value); }
		}

		internal GuidRegistryItem MercedesSupplierItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("DaimlerSupplier", delegate
				{
					return new GuidRegistryItem("DaimlerSupplier", (NoResString)MercedeChryslerMotoVehicleInterfaceCategory,
						(NoResString)"Mercedes Supplier", (NoResString)"Supplier for which to import Mercedes declaration data.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader), RegistryStorageFlags.System,
						RegistryOptions.NotCached, Guid.Empty);
				});
			}
		}
		#endregion

		#region ChryslerSupplier
		public ZGuid ChryslerSupplier
		{
			get { return new ZGuid(ChryslerSupplierItem.Value); }
		}

		internal GuidRegistryItem ChryslerSupplierItem
		{
			get
			{
				return GetItem<GuidRegistryItem>("ChryslerSupplier", delegate
				{
					return new GuidRegistryItem("ChryslerSupplier", (NoResString)MercedeChryslerMotoVehicleInterfaceCategory,
						(NoResString)"Chrysler Supplier", (NoResString)"Supplier for which to import Chrysler declaration data.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader), RegistryStorageFlags.System, RegistryOptions.NotCached, Guid.Empty);
				});
			}
		}
		#endregion
		#region MercedesImportFileNamePrefix - Data Import
		public ZString MercedesImportFileNamePrefix
		{
			get { return MercedesImportFileNamePrefixItem.Value; }
		}

		internal StringRegistryItem MercedesImportFileNamePrefixItem
		{
			get
			{
				return GetItem<StringRegistryItem>("DaimlerFileNamePrefix", delegate
				{
					return new StringRegistryItem("DaimlerFileNamePrefix", (NoResString)MercedeChryslerImportInterfacesCategory,
						(NoResString)"Mercedes File Name Prefix", (NoResString)"Prefix for Mercedes import declaration data file name.",
						RegistryStorageFlags.System, "SWTTOWCB");
				});
			}
		}
		#endregion

		#region ChryslerImportFileNamePrefix - Data Import
		public ZString ChryslerImportFileNamePrefix
		{
			get { return ChryslerImportFileNamePrefixItem.Value; }
		}

		internal StringRegistryItem ChryslerImportFileNamePrefixItem
		{
			get
			{
				return GetItem<StringRegistryItem>("ChryslerFileNamePrefix", delegate
				{
					return new StringRegistryItem("ChryslerFileNamePrefix", (NoResString)MercedeChryslerImportInterfacesCategory,
						(NoResString)"Chrysler File Name Prefix", (NoResString)"Prefix for Chrysler import declaration data file name.",
						RegistryStorageFlags.System, "SWTTOCRD");
				});
			}
		}
		#endregion

		const string Category = "WCB Client Extensions";
		const string MercedeChryslerMotoVehicleInterfaceCategory = Category + @"/Mercedes Chrysler Motor Vehicle Interface";
		const string MercedeChryslerImportInterfacesCategory = MercedeChryslerMotoVehicleInterfaceCategory + @"/Import Interfaces";
	}
}

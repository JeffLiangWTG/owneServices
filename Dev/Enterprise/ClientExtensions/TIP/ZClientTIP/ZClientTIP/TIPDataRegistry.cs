using System;

using CargoWise.Types;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TIP
{
	public sealed class TIPDataRegistry : RegistryItemSet
	{
		#region Instance

		public static TIPDataRegistry Instance
		{
			get { return instance ?? (instance = new TIPDataRegistry()); }
		}
		[ThreadStatic]
		static TIPDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region ProductExportToCsv
		public OrgPartRelationRegistryItem OrganisationProductRegistryItem
		{
			get
			{
				return GetItem("TIPOrganisationProductRelationship", delegate
					{
						return new OrgPartRelationRegistryItem(
							"TIPOrganisationProductRelationship",
							ProductExportCategory,
							"Organisations",
							"Setup the organisations that require Product Data Export",
							RegistryStorageFlags.System
							);
					}
				);
			}
		}

		internal ServiceTaskDataTransferHighWaterMarkRegistryItem ProductExportRegistryItem
		{
			get
			{
				return GetItem("TIPProductExportRegistryItem", delegate
					{
						return new ServiceTaskDataTransferHighWaterMarkRegistryItem(
							"TIPProductExportRegistryItem",
							(NoResString)ProductExportCategory,
							(NoResString)"Automatic Data Export Settings",
							(NoResString)"Please fill into the fields provided:",
							RegistryStorageFlags.System, RegistryOptions.NotCached);
					}
				);
			}
		}

		public ZString ProductExportDirectory
		{
			get { return ProductExportRegistryItem.Value.Directory; }
		}

		public ZGuid ProductExportNotifyGroupPK
		{
			get { return ProductExportRegistryItem.Value.GroupPK; }
		}

		public ZDateTime HighWaterMark
		{
			get { return ProductExportRegistryItem.Value.LastRunDateTime; }
			set
			{
				DataTransferRegistryBusinessObject obj = ProductExportRegistryItem.Value;
				obj.LastRunDateTime = value;
				ProductExportRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, obj);
			}
		}

		#endregion

		const string Category = "Toll International Extensions";
		const string ProductExportCategory = Category + "/" + "Product Export to CSV File";
	}
}

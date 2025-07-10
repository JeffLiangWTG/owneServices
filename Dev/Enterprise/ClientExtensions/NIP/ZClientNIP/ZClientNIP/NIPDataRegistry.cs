using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.NIP
{
	public sealed class NIPDataRegistry : RegistryItemSet
	{
		NIPDataRegistry()
		{
		}

		#region Instance

		public static NIPDataRegistry Instance
		{
			get { return instance ?? (instance = new NIPDataRegistry()); }
		}
		[ThreadStatic]
		static NIPDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region Category

		const string Category = "NIP Client-Extensions";
		const string ImportConsolAndShipmentCategory = Category + "/Import of Consol + Shipment Data";

		#endregion

		#region Import Consol And Shipment Data

		public StringRegistryItem ConsolAndShipmentImportDirectory
		{
			get
			{
				return GetItem("ConsolAndShipmentImportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ConsolAndShipmentImportDirectory",
						(NoResString)ImportConsolAndShipmentCategory,
						(NoResString)"Directory",
						null,
						RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public GuidRegistryItem ConsolAndShipmentImportNotificationGroup
		{
			get
			{
				return GetItem("ConsolAndShipmentImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ConsolAndShipmentImportNotificationGroup",
						(NoResString)ImportConsolAndShipmentCategory,
						(NoResString)"Notification Group",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion
	}
}
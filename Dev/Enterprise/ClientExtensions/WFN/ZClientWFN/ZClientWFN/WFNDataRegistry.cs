
using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.WFN
{
	sealed class WFNDataRegistry : RegistryItemSet
	{
		WFNDataRegistry()
		{
		}

		#region Instance

		public static WFNDataRegistry Instance
		{
			get { return instance ?? (instance = new WFNDataRegistry()); }
		}
		[ThreadStatic]
		static WFNDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		#region DimercoXMLImportDirectory

		public ZString DimercoXMLImportDirectory
		{
			get { return new ZString(DimercoXMLImportDirectoryRaw.Value); }
			set { DimercoXMLImportDirectoryRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem DimercoXMLImportDirectoryRaw
		{
			get
			{
				return GetItem("DimercoXMLImportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DimercoXMLImportDirectory",
						(NoResString)DimercoCategory,
						(NoResString)"Dimerco XML Import Directory",
						(NoResString)"Import Directory for storing the Dimerco XML files",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region DimercoXMLImportNotificationGroup

		public GuidRegistryItem DimercoXMLImportNotificationGroup
		{
			get
			{
				return GetItem("DimercoXMLImportNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DimercoXMLImportNotificationGroup",
						(NoResString)DimercoCategory,
						(NoResString)"Dimerco XML Import Notification Group",
						(NoResString)"The staff group that will be notified about Dimerco XML Import.",
						RegistryStorageFlags.System,
						RegistryOptions.NotCached,
						Guid.Empty
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		const string Category = "WFN Client Extensions";
		const string ImportCategory = Category + @"/Import";
		const string DimercoCategory = ImportCategory + @"/Dimerco";
	}
}

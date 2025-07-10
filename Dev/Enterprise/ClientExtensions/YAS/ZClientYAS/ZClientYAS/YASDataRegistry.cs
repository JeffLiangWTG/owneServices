using System;

using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.YAS
{
	public sealed class YASDataRegistry : RegistryItemSet
	{
		YASDataRegistry() { }

		#region Instance
		public static YASDataRegistry Instance
		{
			get { return instance ?? (instance = new YASDataRegistry()); }
		}
		[ThreadStatic]
		static YASDataRegistry instance;
		#endregion

		public override bool IsForProductivityWise => false;

		#region Proof Of Delivery Interface

		public ZString PODImportFolder
		{
			get { return PODImportFolderItem.Value; }
			set { PODImportFolderItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public StringRegistryItem PODImportFolderItem
		{
			get
			{
				return GetItem("YASPODImportFolder", () =>
				{
					StringRegistryItem result = new StringRegistryItem("YASPODImportFolder", (NoResString)PODInterfaceCategory,
					(NoResString)"Proof Of Delivery Import Folder",
					(NoResString)"Enter a folder for the files to import.", RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		public ZGuid PODEmailNotificationGroup
		{
			get { return new ZGuid(PODEmailNotificationGroupItem.Value); }
			set { PODEmailNotificationGroupItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToGuid()); }
		}

		public GuidRegistryItem PODEmailNotificationGroupItem
		{
			get
			{
				return GetItem("YASPODEmailNotificationGroup", () =>
					new GuidRegistryItem("YASPODEmailNotificationGroup", (NoResString)PODInterfaceCategory, (NoResString)"Proof Of Delivery Email Notification Group",
					(NoResString)"Nominate a group that we be advised of critical issues with the Proof of Delivery imports.",
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup), RegistryStorageFlags.System, RegistryOptions.NotCached, Guid.Empty));
			}
		}

		#endregion

		#region Categories

		static string Category
		{
			get { return f_Category ?? (f_Category = "YAS Client Specific"); }
		}
		[ThreadStatic]
		static string f_Category;

		#endregion

		internal static readonly string PODInterfaceCategory = Category + "/Import of POD Data";
	}
}

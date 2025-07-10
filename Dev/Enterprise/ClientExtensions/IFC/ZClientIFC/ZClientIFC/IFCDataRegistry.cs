using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.IFC
{
	internal sealed class IFCDataRegistry : RegistryItemSet
	{
		#region Instance

		public static IFCDataRegistry Instance
		{
			get { return instance ?? (instance = new IFCDataRegistry()); }
		}
		[ThreadStatic]
		static IFCDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "IFC Client Extensions";

		#region FSC Export Directory

		public ZString FSCExportDirectory
		{
			get { return new ZString(FSCExportDirectoryRaw.Value); }
			set { ((IRegistryItem)FSCExportDirectoryRaw).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		StringRegistryItem FSCExportDirectoryRaw
		{
			get
			{
				return GetItem("FSCExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("FSCExportDirectory", (NoResString)Category, (NoResString)"FSC Export Directory", (NoResString)"This is the Directory where the FSC Files are Exported", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region FSC Run Time Interval

		public ZInt FSCRunTimeIntervalForBatchProcess
		{
			get { return new ZInt(FSCRunTimeIntervalForBatchProcessRaw.Value); }
			set { FSCRunTimeIntervalForBatchProcessRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (int)value); }
		}

		IntRegistryItem FSCRunTimeIntervalForBatchProcessRaw
		{
			get
			{
				return GetItem("RunTimeIntervalForBatchProcess", delegate
				{
					return new IntRegistryItem("RunTimeIntervalForBatchProcess", (NoResString)Category, (NoResString)"Run Time Interval", (NoResString)"The Number of minutes between batch exports", RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, 60);
				});
			}
		}

		#endregion
	}
}

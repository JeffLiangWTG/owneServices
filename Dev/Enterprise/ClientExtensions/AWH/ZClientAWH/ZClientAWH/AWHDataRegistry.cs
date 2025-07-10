using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.AWH
{
	public sealed class AWHDataRegistry : RegistryItemSet
	{
		#region Instance

		public static AWHDataRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AWHDataRegistry();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static AWHDataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "AWH Logistics Client Extensions";
		const string ARTransExportSubCategory = Category + "/AR Trans. Export";

		#region Registry

		#region ARTransOutputDirectory

		public ZString ARTransExportDirectory
		{
			get { return fARTransOutputDirectory.Value; }
			set { fARTransOutputDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem fARTransOutputDirectory
		{
			get
			{
				return GetItem("AWHARTransExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("AWHARTransExportDirectory", (NoResString)ARTransExportSubCategory, (NoResString)"Export Files Directory", (NoResString)"Directory that store the Export Files", RegistryStorageFlags.System, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region OutputFilePrefix

		public ZString ExportFilePrefix
		{
			get { return fExportFilePrefix.Value; }
			set { fExportFilePrefix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem fExportFilePrefix
		{
			get
			{
				return GetItem("AWHExportFilePrefix", delegate
				{
					StringRegistryItem result = new StringRegistryItem("AWHExportFilePrefix", (NoResString)ARTransExportSubCategory, (NoResString)"Export File Prefix", (NoResString)"Prefix for the Output File Name", RegistryStorageFlags.System);
					return result;
				});
			}
		}

		#endregion

		#region Branch

		public ReadOnlyCodeDescriptionPairList BranchList
		{
			get { return fBranchList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { fBranchList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem fBranchList
		{
			get
			{
				return GetItem("AWHBranchList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"AWHBranchList",
							(NoResString)ARTransExportSubCategory,
							(NoResString)"Branch Mapping",
							(NoResString)("Specify the " + BrandingFactory.Instance.ProductName + " Branch Code and its Equivalent AWH Branch Code"),
							3,
							RegistryStorageFlags.System,
							false,
							RegistryOptions.NotCached,
							DefaultBranchList);
				});
			}
		}

		CodeDescriptionPairList DefaultBranchList
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					list.AddPair(branch.GB_Code);
				}
				return list;
			}
		}

		#endregion

		#region Department

		public ReadOnlyCodeDescriptionPairList DepartmentList
		{
			get { return fDepartmentList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
			set { fDepartmentList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem fDepartmentList
		{
			get
			{
				return GetItem("AWHDeptList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"AWHDeptList",
							(NoResString)ARTransExportSubCategory,
							(NoResString)"Department Mapping",
							(NoResString)("Specify the " + BrandingFactory.Instance.ProductName + " Dept Code and its Equivalent AWH Department Code"),
							3,
							RegistryStorageFlags.System,
							false,
							RegistryOptions.NotCached,
							new CodeDescriptionPairList(),
							false);
				});
			}
		}

		#endregion

		#endregion
	}
}

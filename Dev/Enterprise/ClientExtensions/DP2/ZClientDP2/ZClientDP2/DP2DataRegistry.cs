using System;

using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DP2
{
	public sealed class DP2DataRegistry : RegistryItemSet
	{
		#region Instance

		public static DP2DataRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new DP2DataRegistry();
				}
				return instance;
			}
		}

		[ThreadStatic]
		static DP2DataRegistry instance;

		#endregion

		public override bool IsForProductivityWise => false;

		const string Category = "DP2 Logistics Client Extensions";
		internal const string ARTransExportSubCategory = Category + "/AR Trans. Export";

		#region Registry

		#region ARTransOutputDirectory

		public ZString ARTransExportDirectory
		{
			get { return ARTransExportDirectoryItem.Value; }
			set { ARTransExportDirectoryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem ARTransExportDirectoryItem
		{
			get
			{
				return GetItem("DP2ARTransExportDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem("DP2ARTransExportDirectory",
						(NoResString)ARTransExportSubCategory, (NoResString)"Export Files Directory",
						(NoResString)"Directory that store the Export Files", RegistryStorageFlags.Company, RegistryOptions.NotCached);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					return result;
				});
			}
		}

		#endregion

		#region OutputFilePrefix

		public ZString ExportFilePrefix
		{
			get { return ExportFilePrefixItem.Value; }
			set { ExportFilePrefixItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value.ToString()); }
		}

		internal StringRegistryItem ExportFilePrefixItem
		{
			get
			{
				return GetItem("DP2ExportFilePrefix", delegate
				{
					StringRegistryItem result = new StringRegistryItem("DP2ExportFilePrefix",
						(NoResString)ARTransExportSubCategory, (NoResString)"Export File Prefix",
						(NoResString)"Prefix for the Output File Name", RegistryStorageFlags.Company);
					return result;
				});
			}
		}

		#endregion

		#region Branch

		public ReadOnlyCodeDescriptionPairList BranchList
		{
			get { return BranchListItem.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty); }
			set { BranchListItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem BranchListItem
		{
			get
			{
				return GetItem("DP2BranchList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"DP2BranchList",
							(NoResString)ARTransExportSubCategory,
							(NoResString)"Branch Mapping",
							(NoResString)("Specify the " + BrandingFactory.Instance.ProductName + " Branch Code and its Equivalent DP2 Branch Code"),
							3,
							RegistryStorageFlags.Company | RegistryStorageFlags.System,
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
				foreach (GlbCompany company in GlbCompany.GetActiveCompanies())
				{
					foreach (GlbBranch branch in company.ActiveBranches)
					{
						list.AddPair(branch.GB_Code);
					}
				}

				return list;
			}
		}

		#endregion

		#region Department

		public ReadOnlyCodeDescriptionPairList DepartmentList
		{
			get { return DepartmentListItem.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty); }
			set { DepartmentListItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value); }
		}

		internal CodeDescriptionPairListRegistryItem DepartmentListItem
		{
			get
			{
				return GetItem("DP2DeptList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
							"DP2DeptList",
							(NoResString)ARTransExportSubCategory,
							(NoResString)"Department Mapping",
							(NoResString)("Specify the " + BrandingFactory.Instance.ProductName + " Dept Code and its Equivalent DP2 Department Code"),
							3,
							RegistryStorageFlags.Company | RegistryStorageFlags.System,
							false,
							RegistryOptions.NotCached,
							new CodeDescriptionPairList(),
							false);
				});
			}
		}

		#endregion

		#region Enable

		public bool EnableARTransactionExport
		{
			get { return EnableARTransactionExportItem.Value; }
			set { EnableARTransactionExportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value); }
		}

		public BooleanRegistryItem EnableARTransactionExportItem
		{
			get
			{
				return GetItem("DP2EnableARTransactionExport", delegate
				{
					return new BooleanRegistryItem(
						"DP2EnableARTransactionExport",
						(NoResString)ARTransExportSubCategory,
						(NoResString)"Enable AR Transaction Export",
						(NoResString)"Enable AR Transaction Export",
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						ZBool.False);
				});
			}
		}
		#endregion

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	public class RegistryFormTreeViewBuilder
	{
		#region Properties

		protected BusinessObjectFactory Factory;
		bool hideInactiveFallbacks;

		protected GlbCompanyCollection companiesForFilteringRegistryItems;
		protected GlbCompanyCollection companiesForCreatingFallbacks;
		GlbDepartmentCollection departmentsForCreatingFallbacks;
		readonly Lazy<Dictionary<IRegistryItem, RegistryItemTag>> registryItemsToDisplay;

		protected static string RegistryItemNotSelectedText
		{
			get { return Res.GetString("4d3558ab-9fd0-40f4-a635-d0d8f1409d62", "Please select a Registry item."); }
		}
		protected static string NoFallbackNodesText
		{
			get { return Res.GetString("7f4e9d07-e0c4-45d7-a5d8-a506ae40b2ae", "No fall backs found."); }
		}
		protected static string SystemText
		{
			get { return Res.GetString("e8b40e23-501d-43f5-9599-6f21e2fc3c12", "System"); }
		}
		protected static string CompanyText
		{
			get { return Res.GetString("41ee3999-5691-4401-97d8-6b154fbc2798", "Companies"); }
		}
		protected static string BranchText
		{
			get { return Res.GetString("e4311c26-34bb-4ccd-b38b-cee4931b9544", "Branches"); }
		}
		protected static string DepartmentText
		{
			get { return Res.GetString("1defef79-ffea-4863-91f8-83925b44930b", "Departments"); }
		}

		#endregion

		public RegistryFormTreeViewBuilder(BusinessObjectFactory factory)
			: this(factory, RegistryItemTreeViewBuilder.GetCompanies(factory, false), false)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual member used for testing only")]
		public RegistryFormTreeViewBuilder(BusinessObjectFactory factory, GlbCompanyCollection companiesForFilteringRegistryItems, bool hideInactiveFallbacks)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			this.companiesForFilteringRegistryItems = Argument.NotNull(companiesForFilteringRegistryItems, nameof(companiesForFilteringRegistryItems));
			SetHideInactiveFallbacks(hideInactiveFallbacks);
			registryItemsToDisplay = new Lazy<Dictionary<IRegistryItem, RegistryItemTag>>(GetVisibleItems);
		}

		public void SetHideInactiveFallbacks(bool hideInactive)
		{
			hideInactiveFallbacks = hideInactive;
			companiesForCreatingFallbacks = RegistryItemTreeViewBuilder.GetCompanies(Factory, hideInactive);
			departmentsForCreatingFallbacks = GetDepartments();
		}

		Dictionary<IRegistryItem, RegistryItemTag> GetVisibleItems()
		{
			return GetAllRegistryItems()
				.Select(item => new RegistryItemTag(item))
				.Where(tag => tag.CheckItemIsVisible(companiesForFilteringRegistryItems))
				.ToDictionary(item => item.RegistryItem);
		}

		internal IEnumerable<IRegistryItem> RegistryItemsToDisplay => registryItemsToDisplay.Value.Keys;

		protected virtual IEnumerable<IRegistryItem> GetAllRegistryItems()
		{
			using (Db.Connection.StartScalarCaching())
			{
				foreach (var item in new RegistryItemSetLocator().GetAllRegistryItems())
				{
					yield return item;
				}
			}
		}

		public bool UpdateFallbackTree(TreeView fallbackTree, TreeNode registryNode)
		{
			var item = registryNode.Tag as RegistryItemTag;
			fallbackTree.Nodes.Clear();

			if (item != null)
			{
				InsertSystemNodes(fallbackTree, item);
				InsertCompanyNodes(fallbackTree, item);

				if (fallbackTree.GetNodeCount(true) > 0)
				{
					return true;
				}
				else
				{
					fallbackTree.Nodes.Add(new FallbackTreeNode(NoFallbackNodesText));
					return false;
				}
			}
			else
			{
				fallbackTree.Nodes.Add(new FallbackTreeNode(RegistryItemNotSelectedText));
				return false;
			}
		}

		#region Creating Fallback Levels

		protected void InsertSystemNodes(TreeView fallbackTree, RegistryItemTag item)
		{
			var storage = item.Storage;
			if (storage.HasAnyFlag(RegistryStorageFlags.System, RegistryStorageFlags.SystemDepartment))
			{
				var appliesToThisLevel = storage.HasFlag(RegistryStorageFlags.System);
				var systemNode = new FallbackTreeNode(SystemText, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, appliesToThisLevel);

				fallbackTree.Nodes.Add(systemNode);

				if (storage.HasFlag(RegistryStorageFlags.SystemDepartment))
				{
					InsertDepartmentNodes(systemNode, ZGuid.Empty, ZGuid.Empty, item);
				}
			}
		}

		protected void InsertCompanyNodes(TreeView fallbackTree, RegistryItemTag item)
		{
			var storage = item.Storage;
			if (storage.HasAnyFlag(RegistryStorageFlags.Company, RegistryStorageFlags.CompanyDepartment, RegistryStorageFlags.Branch, RegistryStorageFlags.BranchDepartment))
			{
				var appliesToThisLevel = storage.HasFlag(RegistryStorageFlags.Company);
				var companyRootNode = new FallbackTreeNode(CompanyText);

				foreach (var company in companiesForCreatingFallbacks)
				{
					var companyNode = new FallbackTreeNode(company.HumanReadableNameForRegistry, company.PK, ZGuid.Empty, ZGuid.Empty, appliesToThisLevel);

					if (storage.HasFlag(RegistryStorageFlags.CompanyDepartment))
					{
						InsertDepartmentNodes(companyNode, company.PK, ZGuid.Empty, item);
					}

					InsertBranchNodes(companyNode, company.PK, item);

					if ((appliesToThisLevel && item.RegistryItem.IsVisible(company.PK.ToGuid(), Guid.Empty, Guid.Empty)) ||
						companyNode.GetNodeCount(false) > 0)
					{
						companyRootNode.Nodes.Add(companyNode);
					}
				}

				if (companyRootNode.GetNodeCount(false) > 0)
				{
					fallbackTree.Nodes.Add(companyRootNode);
				}
			}
		}

		protected void InsertBranchNodes(FallbackTreeNode companyNode, ZGuid companyPk, RegistryItemTag item)
		{
			var storage = item.Storage;
			if (storage.HasAnyFlag(RegistryStorageFlags.Branch, RegistryStorageFlags.BranchDepartment))
			{
				var appliesToThisLevel = storage.HasFlag(RegistryStorageFlags.Branch);
				var branchRootNode = new FallbackTreeNode(BranchText);

				foreach (GlbBranch branch in GetBranches(companyPk))
				{
					var branchNode = new FallbackTreeNode(branch.HumanReadableNameForRegistry, companyPk, branch.PK, ZGuid.Empty, appliesToThisLevel);

					if (storage.HasFlag(RegistryStorageFlags.BranchDepartment))
					{
						InsertDepartmentNodes(branchNode, companyPk, branch.PK, item);
					}

					if ((appliesToThisLevel && item.RegistryItem.IsVisible(Guid.Empty, branch.PK.ToGuid(), Guid.Empty)) ||
						(branchNode.GetNodeCount(false) > 0))
					{
						branchRootNode.Nodes.Add(branchNode);
					}
				}

				if (branchRootNode.GetNodeCount(false) > 0)
				{
					companyNode.Nodes.Add(branchRootNode);
				}
			}
		}

		protected void InsertDepartmentNodes(FallbackTreeNode parentNode, ZGuid companyPk, ZGuid branchPk, RegistryItemTag item)
		{
			var departmentRootNode = new FallbackTreeNode(DepartmentText);

			foreach (var department in departmentsForCreatingFallbacks)
			{
				var departmentNode = new FallbackTreeNode(department.GE_DescMultilingual, companyPk, branchPk, department.PK, true);

				var companyGuid = (companyPk.IsEmpty || !branchPk.IsEmpty) ? Guid.Empty : companyPk.ToGuid();
				var branchGuid = (branchPk.IsEmpty) ? Guid.Empty : branchPk.ToGuid();

				if (item.RegistryItem.IsVisible(companyGuid, branchGuid, department.PK.ToGuid(), department))
				{
					departmentRootNode.Nodes.Add(departmentNode);
				}
			}

			if (departmentRootNode.GetNodeCount(false) > 0)
			{
				parentNode.Nodes.Add(departmentRootNode);
			}
		}

		protected GlbBranchCollection GetBranches(ZGuid companyPk)
		{
			var filter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, companyPk);

			if (hideInactiveFallbacks)
			{
				filter.AddToFilter(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			}

			var branches = new GlbBranchCollection(Factory, filter);

			var propertyNameForSort = Env.Registry.ShowCodeAtCompanyAndBranchName
				? GlbBranchSchema.GB_Code.Name
				: GlbBranchSchema.GB_BranchName.Name;

			branches.Sort(propertyNameForSort, ListSortDirection.Ascending);
			branches.Load();

			return branches;
		}

		protected GlbDepartmentCollection GetDepartments()
		{
			GlbDepartmentCollection departments;
			departments = new GlbDepartmentCollection(Factory);
			departments.ApplySort(GlbDepartmentSchema.GE_Desc.Name, ListSortDirection.Ascending);

			if (hideInactiveFallbacks)
			{
				var query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
				departments.AdditionalFilter = query;
			}

			return departments;
		}

		#endregion
	}
}

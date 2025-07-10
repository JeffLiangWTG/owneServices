using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	static class RegistryItemTreeViewBuilder
	{
		#region Registry Items

		public static void AddItemsToTree(TreeNodeCollection baseCollection, IEnumerable<IRegistryItem> itemsToDisplay, Func<IRegistryItem, object> getTagItem = null)
		{
			getTagItem = getTagItem ?? (item => item);
			foreach (var item in itemsToDisplay)
			{
				var categories = item.Categories;
				var categoriesUntranslated = item.CategoriesUntranslated;
				for (var i = 0; i < categories.Length; ++i)
				{
					var category = categories[i];
					var categoryUntranslated = categoriesUntranslated[i];
					var itemNode = new TreeNode { Text = item.Caption, Tag = getTagItem(item), Name = item.Name };
					var categoryNode = GetOrCreateNodePath(baseCollection, category, categoryUntranslated);

					categoryNode.Add(itemNode);
				}
			}
		}

#if DEBUG
		internal
#endif
		static TreeNodeCollection GetOrCreateNodePath(TreeNodeCollection parentCategory, string category, string categoryUntranslated)
		{
			var (categorySplit, categoryUntranslatedSplit) = ConvertCategory(category,categoryUntranslated);

			for (var i = 0; i < categorySplit.Length; ++i)
			{
				var categoryName = categorySplit[i].Trim();
				var categoryNameUntranslated = categoryUntranslatedSplit[i].Trim();
				var nextCategory = parentCategory[categoryName];

				if (nextCategory == null)
				{
					nextCategory = new TreeNode
					{
						Name = categoryName,
						Text = categoryName,
						Tag = categoryNameUntranslated,
						Checked = true
					};

					parentCategory.Add(nextCategory);
				}

				parentCategory = nextCategory.Nodes;
			}

			return parentCategory;
		}

		internal static (string[] categorySplit, string[] categoryUntranslatedSplit) ConvertCategory(string category, string categoryUntranslated)
		{
			//Some other languages erroneously have /s in their names that should be \/. Forcibly convert them.
			category = category.Replace(@"\/", "&#92;");
			categoryUntranslated = categoryUntranslated.Replace(@"\/", "&#92;");
			var categorySlashCount = category.CountMatches('/');
			var categoryUntranslatedSlashCount = categoryUntranslated.CountMatches('/');
			if (categorySlashCount > categoryUntranslatedSlashCount)
			{
				for (var i = 0; i < categorySlashCount - categoryUntranslatedSlashCount; ++i)
				{
					category = category.Substring(0, category.LastIndexOf('/')) + "&#92;" +
						((category.LastIndexOf('/') == category.Length - 1) ? "" : category.Substring(category.LastIndexOf('/') + 1));
				}
			}

			//Now the amount of /s in both category and categoryUntranslated are the same, so we can continue safely.
			var categorySplit = category.Split('/').Select(c => c.Replace("&#92;", "/")).ToArray();
			var categoryUntranslatedSplit = categoryUntranslated.Split('/').Select(c => c.Replace("&#92;", "/")).ToArray();
			return (categorySplit, categoryUntranslatedSplit);
		}

		public static IEnumerable<IRegistryItem> GetRegistryItems(TreeNodeCollection root, bool requireChecked)
		{
			return GetLeafRegistryItemNodes(root, requireChecked).Select(node => node.Tag).OfType<IRegistryItem>();
		}

		public static IEnumerable<TreeNode> GetLeafRegistryItemNodes(TreeNodeCollection parent, bool requireChecked)
		{
			foreach (TreeNode node in parent)
			{
				if (node.Nodes.Count == 0 && (node.Checked || !requireChecked) && node.Tag is IRegistryItem)
				{
					yield return node;
				}
				else
				{
					foreach (var child in GetLeafRegistryItemNodes(node.Nodes, requireChecked))
					{
						yield return child;
					}
				}
			}
		}

		#endregion RegistryNodes

		#region Fallbacks

		public static bool IsSelectable(TreeNode node)
		{
			return node.Tag is IOverrideLevel;
		}

		public static void AddFallbackNodes(TreeNodeCollection root, BusinessObjectFactory factory, bool addDefaultLevel = true, bool hideInactiveFallbacks = false)
		{
			if (addDefaultLevel)
			{
				AddNode(root, new DefaultOverrideLevel());
			}

			AddNode(root, new SystemOverrideLevel(hideInactiveFallbacks));

			foreach (var company in GetCompanies(factory, hideInactive: hideInactiveFallbacks))
			{
				AddNode(root, new CompanyOverrideLevel(company, hideInactiveFallbacks));
			}
		}

		static void AddNode(TreeNodeCollection root, IOverrideLevel level)
		{
			var directParentNode = string.IsNullOrEmpty(level.PathRelativeToParent) ? root : GetOrCreateNodePath(root, level.PathRelativeToParent, level.PathRelativeToParent);
			var node = new TreeNode
			{
				Text = level.Description,
				Name = level.Description,
				Tag = level
			};

			directParentNode.Add(node);
			foreach (var subLevel in level.Children)
			{
				AddNode(node.Nodes, subLevel);
			}
		}

		public static List<IOverrideLevel> AddFallbackNodes(TreeView treeView, BusinessObjectFactory factory, bool addDefaultLevel = true, bool hideInactiveFallbacks = false)
		{
			var rootOverrideLevels = new List<IOverrideLevel>();
			treeView.AfterExpand -= TreeView_AfterExpand;
			treeView.AfterExpand += TreeView_AfterExpand;
			if (addDefaultLevel)
			{
				AddNodeForRoot(treeView.Nodes, new DefaultOverrideLevel(), rootOverrideLevels);
			}

			AddNodeForRoot(treeView.Nodes, new SystemOverrideLevel(hideInactiveFallbacks), rootOverrideLevels);

			foreach (var company in GetCompanies(factory, hideInactive: hideInactiveFallbacks))
			{
				AddNodeForRoot(treeView.Nodes, new CompanyOverrideLevel(company, hideInactiveFallbacks), rootOverrideLevels);
			}

			return rootOverrideLevels;
		}

		static void AddNodeForRoot(TreeNodeCollection root, IOverrideLevel level, List<IOverrideLevel> rootOverrideLevels)
		{
			rootOverrideLevels.Add(level);
			AddCurrentNodeAndPlaceHolderNodeForChildren(root, level);
		}

		static void TreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Nodes.Count == 1
				&& e.Node.FirstNode.Text == PlaceHolderNode
				&& e.Node.Tag is IOverrideLevel level
				&& level.Children.Any())
			{
				e.Node.Nodes.Clear();

				foreach (var subLevel in level.Children)
				{
					AddCurrentNodeAndPlaceHolderNodeForChildren(e.Node.Nodes, subLevel);
				}
			}
		}

		const string PlaceHolderNode = nameof(PlaceHolderNode);

		static void AddCurrentNodeAndPlaceHolderNodeForChildren(TreeNodeCollection parentNodes, IOverrideLevel level)
		{
			var node = new TreeNode
			{
				Text = level.Description,
				Name = level.Description,
				Tag = level
			};

			var directParentNode = string.IsNullOrEmpty(level.PathRelativeToParent) ? parentNodes : GetOrCreateNodePath(parentNodes, level.PathRelativeToParent, level.PathRelativeToParent);
			directParentNode.Add(node);

			if (level.Children.Any())
			{
				var placeHolderNode = new TreeNode
				{
					Text = PlaceHolderNode,
					Name = PlaceHolderNode,
				};

				node.Nodes.Add(placeHolderNode);
			}
		}

		#region Creating Registry Items
		static Overridable<Guid> demoCompanyGuid = new Overridable<Guid>(new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5"));

		public static GlbCompanyCollection GetCompanies(BusinessObjectFactory factory, bool hideInactive)
		{
			var companies = new GlbCompanyCollection(factory);

			var propertyNameForSort = Env.Registry.ShowCodeAtCompanyAndBranchName
				? GlbCompanySchema.GC_Code.Name
				: GlbCompanySchema.GC_Name.Name;

			companies.ApplySort(propertyNameForSort, ListSortDirection.Ascending);

			var additionalQuery = new ZQuery();
			if (companies.Count > 1)
			{
				// If there is more than one company in the system, we don't need to show registry items with
				// company filters that are only visible to the Demo Company
				additionalQuery.AddToFilter(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, demoCompanyGuid.Value));
			}

			if (hideInactive)
			{
				additionalQuery.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			}

			companies.AdditionalFilter = additionalQuery;

			factory.AddFetchHint(GlbBranchSchema.Instance, new ZQuery(GlbBranchSchema.GB_GC, companies.Select(c => c.PK)));

			return companies;
		}

#if DEBUG
		internal static IDisposable SetDemoCompanyGuid(Guid newGuid)
		{
			var previousValue = demoCompanyGuid;
			demoCompanyGuid.Value = newGuid;
			return new DisposableAction(() => demoCompanyGuid = previousValue);
		}
#endif

		#endregion

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Forms.Internal;

namespace Enterprise.Registry.GUI
{
	public class RegistryItemsTreeView : ZTreeView, ITreeViewWithExpandingNodes
	{
		public RegistryItemsTreeView()
		{
			DynamicUpdateOnExpandEnabled = true;
			TreeViewSearcher = new LazyTreeViewSearcher();
		}

		public void Initialise(IRegistry registry, GlbCompanyCollection companiesForFilteringRegistryItems, IRegistryItemVisibility registryItemVisibility = null)
		{
			this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
			companies = companiesForFilteringRegistryItems ?? throw new ArgumentNullException(nameof(companiesForFilteringRegistryItems));
			this.registryItemVisibility = registryItemVisibility;

			BeginUpdate();
			Nodes.Clear();
			AddCategories(Nodes, registry.GetSortedTopLevelCategories());
			InitialiseMenuItems();
			EndUpdate();
		}

		void InitialiseMenuItems()
		{
			ContextMenu = new ContextMenu();
			copyRegistryItemPathMenuItem = new ZMenuItem(
					ResString.GetMultilingualString("EFE2FD80-2E45-4DF2-A477-F2BF33BB4E11", "Copy Registry Path to Clipboard"),
					AddSelectedRegistryItemPathToClipboard);
			copyRegistryItemPathMenuItem.Enabled = false;
			ContextMenu.MenuItems.Add(copyRegistryItemPathMenuItem);
			ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			copyRegistryItemPathMenuItem.Enabled = SelectedNode != null;
		}

		public void ExpandNodeIfNeeded(TreeNode node)
		{
			if (registry == null)
			{
				throw new NotSupportedException($"{nameof(RegistryItemsTreeView)} has not been initialised");
			}

			if (!DynamicUpdateOnExpandEnabled)
			{
				return;
			}

			if (node.Tag is RegistryCategoryRef category)
			{
				BeginUpdate();

				node.Nodes.Clear();
				node.Tag = null;

				var content = registry.GetSortedContent(category.Key);

				AddItems(node.Nodes, content.Items);
				AddCategories(node.Nodes, content.Categories);

				EndUpdate();
			}
		}

		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand(e);

			ExpandNodeIfNeeded(e.Node);
		}

		internal void AddCategories(TreeNodeCollection collection, IEnumerable<RegistryCategoryRef> sortedCategories)
		{
			foreach (var category in sortedCategories)
			{
				var node = new TreeNode(category.DisplayText) { Tag = category };
				node.Nodes.Add("...");
				collection.Add(node);
			}
		}

		internal void AddItems(TreeNodeCollection collection, IEnumerable<IRegistryItem> items)
		{
			foreach (var tag in items.Select(i => new RegistryItemTag(i)).Where(tag => tag.CheckItemIsVisible(companies, registryItemVisibility)))
			{
				collection.Add(new TreeNode { Text = tag.Caption, Tag = tag, Name = tag.Name });
			}
		}

		void AddSelectedRegistryItemPathToClipboard(object sender, EventArgs e)
		{
			var currentNode = SelectedNode;
			if (currentNode != null)
			{
				var pathBuilder = new StringBuilder(currentNode.Text);
				while (currentNode.Parent != null)
				{
					currentNode = currentNode.Parent;
					pathBuilder.Insert(0, " > ").Insert(0, currentNode.Text);
				}
				AddTextToClipboard(pathBuilder.ToString());
			}
		}

		internal bool DynamicUpdateOnExpandEnabled
		{
			get;
			set;
		}

		internal virtual void AddTextToClipboard(string text) { SafeClipboard.SetText(text); }

		IRegistry registry;
		GlbCompanyCollection companies;
		ZMenuItem copyRegistryItemPathMenuItem;
		IRegistryItemVisibility registryItemVisibility;
	}
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Core.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ZModuleTreeView : ZTreeView
	{
		public ZModuleTreeView(IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}

		public ZModuleTreeView()
		{
			InitializeComponent();
		}

		#region GUI

#if DEBUG
		public new void OnAfterSelect(TreeViewEventArgs e)
		{
			base.OnAfterSelect(e);
		}
#endif

		Container components;

		protected void InitializeComponent()
		{
			components = new Container();
			this.HideSelection = false;
		}

		#endregion

		#region Populate Security Tree

		ModuleTree sourceModuleTree;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ModuleTree SourceModuleTree
		{
			get { return sourceModuleTree ?? ModuleTree.Tree; }
			set { sourceModuleTree = value; }
		}

		/// <summary>
		/// Loads Categories, Sections and Items nodes into the treeview
		/// </summary>
		public void Populate()
		{
			Nodes.Clear();
			PopulateModuleCategories(SourceModuleTree.Categories.Values);
			PopulateCustomModuleCategories();
			SelectedNode = Nodes[0];
		}

		void PopulateModuleCategories(IReadOnlyCollection<ModuleCategory> moduleCategories)
		{
			foreach (var category in moduleCategories)
			{
				var newCategoryNode = GetModuleCategoryNode(category);
				if (newCategoryNode != null)
				{
					Nodes.Add(newCategoryNode);
					newCategoryNode.Expand();
					PopulateModuleSections(newCategoryNode.Nodes, category.Sections.Values);
				}
			}
		}
		protected abstract TreeNode GetModuleCategoryNode(ModuleCategory category);

		protected void PopulateModuleSections(TreeNodeCollection nodes, IReadOnlyCollection<ModuleSection> moduleSections)
		{
			foreach (var section in moduleSections)
			{
				if (section.SecurityCheckpoint != null)
				{
					var newSectionNode = GetModuleSectionNode(section);
					if (newSectionNode != null)
					{
						nodes.Add(newSectionNode);
						PopulateModules(newSectionNode.Nodes, section.Modules.Values);
					}
				}
			}
		}
		protected abstract TreeNode GetModuleSectionNode(ModuleSection section);

		void PopulateModules(TreeNodeCollection nodes, IReadOnlyCollection<INamedModule> modules)
		{
			foreach (var module in modules)
			{
				var newModuleNode = GetModuleNode(module);
				if (newModuleNode != null)
				{
					nodes.Add(newModuleNode);
					PopulateModuleItems(newModuleNode, module);
				}
			}
		}
		protected abstract TreeNode GetModuleNode(INamedModule module);

		protected virtual void PopulateModuleItems(TreeNode newModuleNode, INamedModule module)
		{
		}

		protected virtual void PopulateCustomModuleCategories()
		{
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

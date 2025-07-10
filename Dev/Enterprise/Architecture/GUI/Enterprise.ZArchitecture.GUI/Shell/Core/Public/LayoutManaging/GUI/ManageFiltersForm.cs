using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ManageLayoutsForm : ZChildForm
	{
		#region Construction

		public ManageLayoutsForm(IModifyModuleAndGridLayout layoutManageable, bool shouldShowSaveColumnsCheckBox, bool shouldShowSaveGridColourCheckBox, bool shouldShowUserDefinedFilter, GridColourScheme gridLastUsedColourScheme = null)
		{
			this.layoutManageable = layoutManageable;
			this.gridLastUsedColourScheme = gridLastUsedColourScheme;
			this.ShouldShowUserDefinedFilter = shouldShowUserDefinedFilter;
			var imageListCache = ZFilterControlImages.FilterImageList;

			if (imageListCache != null)
			{
				SaveColumnsCheckBox.Image = imageListCache.Images["ColumnLayouts"];
			}

			SaveColumnsCheckBox.FlatStyle = FlatStyle.Standard;
			SaveColumnsCheckBox.Visible = shouldShowSaveColumnsCheckBox;
			SaveGridColourCheckBox.Visible = shouldShowSaveGridColourCheckBox;
			SaveGridColourNameTextBox.Visible = shouldShowSaveGridColourCheckBox;

			var resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageLayoutsForm));
			this.RenameLayoutButton.Image = ((Image)(resources.GetObject("RenameLayoutButton.Image")));
			this.editLocalLanguageValuesButton.Enabled = EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed;
		}

		readonly IModifyModuleAndGridLayout layoutManageable;
		readonly GridColourScheme gridLastUsedColourScheme;
		bool ShouldShowUserDefinedFilter { get; set; }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			AddLayoutsToFiltersTreeView();
			SelectFirstLayout();
			base.OnShown(e);
		}

		void AddLayoutsToFiltersTreeView(Dictionary<ZGuid, string> layoutsRenamed = null)
		{
			var myFilterLayoutsText = Res.GetString("2D9A4163-2B6C-4666-881D-04397CE535C3", "My Filter Layouts");
			var sharedFilterLayoutsText = Res.GetString("982A306A-369C-4927-AAFB-9D0D29F20F2F", "Shared Filter Layouts");

			var privateFilters = layoutManageable.GetLayouts(false).Where(x => !IsDeleted(x)).Where(x => x.IsUserDefinedFilter == ShouldShowUserDefinedFilter);

			var privateLayoutsNode = CalculateNode(privateFilters, myFilterLayoutsText, layoutsRenamed);

			var publishedFilters = layoutManageable.GetLayouts(true).Where(x => !IsDeleted(x)).Where(x => x.IsUserDefinedFilter == ShouldShowUserDefinedFilter);
			var publishedLayoutsNode = CalculateNode(publishedFilters, sharedFilterLayoutsText, layoutsRenamed);

			FiltersTreeView.Nodes.Clear();

			FiltersTreeView.Nodes.Add(privateLayoutsNode);
			FiltersTreeView.Nodes.Add(publishedLayoutsNode);

			var defaultGridLayout = layoutManageable.GetDefaultGridLayout();
			if (defaultGridLayout != null)
			{
				FiltersTreeView.Nodes.Add(new LayoutsTreeNode(Res.GetString("78620B5C-B3D0-430D-812C-1F061D119CAD", "Default"), defaultGridLayout, layoutManageable.Factory));
			}
		}

		LayoutsTreeNode CalculateNode(IEnumerable<StmModuleFilter> layoutsList, string mainNodeText, Dictionary<ZGuid, string> layoutsRenamed = null)
		{
			var tree = FilterLayoutsHelper.CalculateTree(layoutsList, false, null, layoutsRenamed);
			using (new DisposableList(tree))
			{
				var layouts = ConvertToLayoutsTreeNode(tree, layoutsRenamed);
				var layoutsNode = new LayoutsTreeNode(mainNodeText);
				layoutsNode.Nodes.AddRange(layouts.ToArray());

				return layoutsNode;
			}
		}

		List<LayoutsTreeNode> ConvertToLayoutsTreeNode(List<StripControl.ZFilterToolStripMenuItem> tree, Dictionary<ZGuid, string> layoutsRenamed = null)
		{
			var result = new List<LayoutsTreeNode>();
			foreach (var item in tree)
			{
				var filter = (StmModuleFilter)item.Tag;
				string filterName = null;
				if (item.Tag != null && !IsDeleted(filter))
				{
					layoutsRenamed?.TryGetValue(filter.PK, out filterName);
				}

				var treeNode = new LayoutsTreeNode(item.Text, filter, layoutManageable.Factory, filterName);
				if (item.HasDropDownItems)
				{
					var subTree = item.DropDownItems.Cast<StripControl.ZFilterToolStripMenuItem>().ToList();
					var dropDownItems = ConvertToLayoutsTreeNode(subTree, layoutsRenamed);

					treeNode.Nodes.AddRange(dropDownItems.ToArray());
				}

				if (IsLayoutSystemDefined(treeNode))
				{
					treeNode.ForeColor = Color.Gray;
				}

				result.Add(treeNode);
			}

			return result;
		}

		void SelectFirstLayout()
		{
			if (FiltersTreeView.Nodes.Count > 0 && FiltersTreeView.Nodes[0].Nodes.Count > 0)
			{
				FiltersTreeView.SelectedNode = FiltersTreeView.Nodes[0].Nodes[0];
			}
		}

		#endregion

		#region Expanded States

		void ReloadLayoutExpandedStates()
		{
			if (treeNodesExpandedStates.Count > 0)
			{
				ExpandTreeNodes(FiltersTreeView.Nodes);
			}
		}

		internal List<string> treeNodesExpandedStates = new List<string>();

		void FiltersTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			AddToExpandedStates(e.Node.FullPath);
		}

		void AddToExpandedStates(string nodePath)
		{
			if (!treeNodesExpandedStates.Contains(nodePath))
			{
				treeNodesExpandedStates.Add(nodePath);
			}
		}

		void FiltersTreeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			RemoveFromExpandedStates(e.Node);
		}

		void RemoveFromExpandedStates(TreeNode node)
		{
			if (treeNodesExpandedStates.Contains(node.FullPath))
			{
				treeNodesExpandedStates.Remove(node.FullPath);
			}
		}

		void AddAllNodesIntoExpandedStates(TreeNode topNode, ZString layoutName)
		{
			var split = layoutName.ToString().Split('/').Reverse();
			var path = topNode.FullPath;
			AddToExpandedStates(path);
			foreach (var level in split.Skip(1).Reverse())
			{
				path += @"\" + level;
				AddToExpandedStates(path);
			}
		}

		void ExpandTreeNodes(TreeNodeCollection treeNodeCollection)
		{
			foreach (TreeNode treeNode in treeNodeCollection)
			{
				if (treeNode.Nodes.Count > 0 && treeNodesExpandedStates.Any(s => s == treeNode.FullPath))
				{
					treeNode.Expand();
					ExpandTreeNodes(treeNode.Nodes);
				}
			}
		}

		#endregion

#region Selecting a User Layout

#if !WINZOR
		const int WM_SETREDRAW = 0xB;
		const int DISABLE_DRAWING = 0;
		const int ENABLE_DRAWING = 1;
#endif

		void FiltersTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
#if !WINZOR
			UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, DISABLE_DRAWING, 0);
#endif
			try
			{
				SuspendLayout();
				var item = e.Node as LayoutsTreeNode;
				var isSelected = item?.FilterName != null && !IsDeleted(item.Layout);
				SaveGridColourNameTextBox.Text = string.Empty;

				if (isSelected)
				{
					using (new SemaphoreManager(ChangingCheckBoxValueSemaphore))
					{
						SaveColumnsCheckBox.Checked = item.SaveColumnLayouts && !item.IsUserDefinedFilter;
						SaveColumnsCheckBox.ReadOnly = item.IsUserDefinedFilter;
						SaveGridColourCheckBox.Checked = item.SaveGridColourLayouts && !item.IsUserDefinedFilter;
						SaveGridColourCheckBox.ReadOnly = item.IsUserDefinedFilter;

						if (SaveGridColourCheckBox.Checked)
						{
							SaveGridColourNameTextBox.Text = item.GridColourLayoutName;
						}
					}

					BuildTreeViewForLayout(item.Layout as StmModuleFilter);

					if (IsSelectedLayoutSystemDefined)
					{
						isSelected = false;
					}
					editLocalLanguageValuesButton.Enabled = true;
				}
				else
				{
					editLocalLanguageValuesButton.Enabled = false;
					ClearTreeView();
					var node = TreeView.Nodes.Add(Res.GetString("92fef1fe-9458-485e-906f-9249693ed8b9", "<Select a filter layout to see the layout's content here>"));
					node.ForeColor = SystemColors.GrayText;
				}

				RenameLayoutButton.Enabled = isSelected;
				DeleteLayoutButton.Enabled = isSelected;
				SaveColumnsCheckBox.Enabled = isSelected;
				SaveGridColourCheckBox.Enabled = isSelected;
				SaveGridColourNameTextBox.Enabled = isSelected;
			}
			finally
			{
				ResumeLayout();
#if !WINZOR
				UnsafeNativeMethods.SendMessage(new HandleRef(this, this.Handle), WM_SETREDRAW, ENABLE_DRAWING, 0);
				Invalidate(true);
#endif
			}
		}

		bool IsDeleted(IGridLayoutStorage layout)
		{
			return DeletedLayouts.Contains(layout.PK);
		}

#endregion

		#region Building the Tree of module filters

		void ClearTreeView()
		{
			TreeView.Nodes.Clear();
		}

		void BuildTreeViewForLayout(StmModuleFilter layout)
		{
			ClearTreeView();

			if (layout != null)
			{
				foreach (var layoutDetails in layoutManageable.GetLayoutDetailTree(layout))
				{
					var parentNodeUniqueID = layoutDetails.ParentUniqueID;

					if (!string.IsNullOrEmpty(parentNodeUniqueID))
					{
						var parentNode = FindOrCreateParentNode(layoutDetails.ParentUniqueID);
						parentNode.Nodes.Add(layoutDetails.UniqueID);
					}
					else if (TreeView.Nodes.Find(layoutDetails.UniqueID, true).Length == 0)
					{
						TreeView.Nodes.Add(layoutDetails.UniqueID, layoutDetails.UniqueID);
					}
				}
			}

			TreeView.ExpandAll();
		}

		TreeNode FindOrCreateParentNode(string parentNodeUniqueID)
		{
			TreeNode result;

			var matchingNodes = TreeView.Nodes.Find(parentNodeUniqueID, false);

			if (matchingNodes.Length > 0)
			{
				result = matchingNodes[0];
			}
			else
			{
				result = TreeView.Nodes.Add(parentNodeUniqueID, parentNodeUniqueID);
				result.NodeFont = ParentNodeFont;
				result.Text = parentNodeUniqueID; //need to set text again to resize node, as its font has been changed to Bold
			}

			return result;
		}

		Font ParentNodeFont
		{
			get
			{
				if (categoryFont == null)
				{
					categoryFont = new Font("Arial", 8.0f, FontStyle.Bold);
				}
				return categoryFont;
			}
		}
		Font categoryFont;

		#endregion

		#region Rename Layout

		void RenameLayoutButton_Click(object sender, EventArgs e)
		{
			RenameSelectedLayout();
		}

		protected void RenameSelectedLayout()
		{
			var selectedLayout = SelectedLayoutItem;
			var shouldRename = true;

			if (selectedLayout != null)
			{
				if (!IsSelectedLayoutAllowedForRenaming)
				{
					var msg = Res.GetString("15824ac7-b7ec-46d8-80c4-a276d9f474fb", "The selected layout cannot be renamed.");
					Globals.Message.ShowError(msg);
					shouldRename = false;
				}
				else if (IsSelectedLayoutPublished && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed ||
					IsSelectedLayoutPublishedAcrossAllCompanies && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches)
				{
					ShowSecurityError();
					shouldRename = false;
				}
				else if (selectedLayout.IsUserDefinedFilter)
				{
					if (!IsAllowedToModifyUserDefinedFilter())
					{
						ShowSecurityError();
						shouldRename = false;
					}
					else
					{
						var layout = layoutManageable.FindLayout(selectedLayout.FilterName) as StmModuleFilter;

						if (layout != null && !UserDefinedFilterHelper.CheckIsOkayToAdjustUserDefinedFilter(selectedLayout.FilterName, layout.S9_FilterNameMultilingual, layoutManageable.Factory, UserDefinedFilterHelper.LayoutAdjustmentMode.Rename))
						{
							shouldRename = false;
						}
					}
				}

				if (shouldRename)
				{
					var newLayoutName = GetLayoutNameFromUser();
					if (!newLayoutName.IsEmpty)
					{
						var topNode = selectedLayout.Layout.IsPublished ? FiltersTreeView.Nodes[1] : FiltersTreeView.Nodes[0];
						var layout = selectedLayout.Layout;
						if (RemoveFromTreeNode(topNode, layout.PK))
						{
							InsertInSubTree(topNode.Nodes, (StmModuleFilter)layout, newLayoutName, new Stack<string>(newLayoutName.ToString().Split('/').Reverse()));

							SaveButton.Enabled = true;
							SaveCloseButton.Enabled = true;
						}
						else
						{
							throw new ArgumentException("The selected layout wasn't removed properly from the filters tree view. Please try again.");
						}
					}
				}
			}
		}

		void InsertInSubTree(TreeNodeCollection nodeCollection, StmModuleFilter filter, string filterName, Stack<string> names)
		{
			var currentLevelUntrimmed = names.Pop();
			var currentLevel = currentLevelUntrimmed.Trim();
			var item = FindItemInListByText(nodeCollection, currentLevel);
			var isLastLevel = names.Count == 0;

			if (item == null)
			{
				var index = GetNewItemInsertIndex(nodeCollection, currentLevel);
				nodeCollection.Insert(index, CreateTreeNode(isLastLevel, currentLevel, filter, filterName));
				FiltersTreeView.SelectedNode = nodeCollection.Cast<TreeNode>().ToArray()[index];

				AddAllNodesIntoExpandedStates(filter.S9_IsPublished ? FiltersTreeView.Nodes[1] : FiltersTreeView.Nodes[0], filterName);
				ReloadLayoutExpandedStates();
			}
			else if (isLastLevel)
			{
				((LayoutsTreeNode)item).UpdateProperties(item.Text, filter, filter.Factory, filterName); // Is Factory correct ? Remove comments if tests ok 
				FiltersTreeView.SelectedNode = item;
			}

			if (!isLastLevel)
			{
				var newTree = item ?? FindItemInListByText(nodeCollection, currentLevelUntrimmed);
				InsertInSubTree(newTree.Nodes, filter, filterName, names);
			}
		}

		LayoutsTreeNode CreateTreeNode(bool hasFilter, string text, StmModuleFilter filter, string filterName)
		{
			return hasFilter ? new LayoutsTreeNode(text, filter, filter.Factory, filterName) : new LayoutsTreeNode(text); // Is Factory correct ? Remove comments if tests ok 
		}

		TreeNode FindItemInListByText(TreeNodeCollection list, string text, bool searchAllChildren = false)
		{
			var nodes = list.Cast<TreeNode>().ToList();
			var found = nodes.FirstOrDefault(x => string.Compare(x.Text.Trim(), text.Trim(), StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(x.Text.Trim(), text.Trim() + " [+]", StringComparison.OrdinalIgnoreCase) == 0);

			if (searchAllChildren && found == null)
			{
				foreach (var node in nodes)
				{
					var foundinChildren = FindItemInListByText(node.Nodes, text, true);

					if (foundinChildren != null)
					{
						return foundinChildren;
					}
				}
			}

			return found;
		}

		int GetNewItemInsertIndex(TreeNodeCollection list, string nameAtCurrentLevel)
		{
			int i;

			for (i = 0; i < list.Count; i++)
			{
				var itemName = list[i].Text.Trim();
				var shouldFilterNameBeBeforeItemName = string.Compare(nameAtCurrentLevel, itemName, StringComparison.OrdinalIgnoreCase) <= 0;
				var isItemNameAFilter = itemName != FilterLayoutsHelper.MyFilterLayoutsText && itemName != FilterLayoutsHelper.SharedFilterLayoutsText;

				if (shouldFilterNameBeBeforeItemName && isItemNameAFilter)
				{
					break;
				}
			}

			return i;
		}

		ZString GetLayoutNameFromUser()
		{
			var result = RequestLayoutNameFromUser();

			if (!result.IsEmpty)
			{
				bool isExistingItemUserDefinedFilter;

				while (ItemExists(result, out isExistingItemUserDefinedFilter))
				{
					if (RequestRetryCancelFromUser(result, isExistingItemUserDefinedFilter) == DialogResult.Retry)
					{
						result = RequestLayoutNameFromUser();
					}
					else
					{
						result = "";
						break;
					}
				}
			}

			return result;
		}

		IEnumerable<LayoutsTreeNode> GetLayoutListRenamed(bool published, Dictionary<ZGuid, string> layoutsRenamed = null)
		{
			var layouts = layoutManageable.GetLayouts(published).Where(x => !IsDeleted(x));
			var res = new List<LayoutsTreeNode>();

			foreach (var layout in layouts)
			{
				string filterName = null;
				layoutsRenamed?.TryGetValue(layout.PK, out filterName);

				res.Add(new LayoutsTreeNode(layout.DisplayName, layout, layoutManageable.Factory, filterName));
			}

			return res;
		}

		bool ItemExists(ZString layoutName, out bool isExistingItemUserDefinedFilter)
		{
			var dictionary = PrepareRenamedLayoutsForSave();

			var privateFiltersRenamed = GetLayoutListRenamed(false, dictionary);
			if (ItemExists(layoutName, privateFiltersRenamed, out isExistingItemUserDefinedFilter))
			{
				return true;
			}

			var publishedFiltersRenamed = GetLayoutListRenamed(true, dictionary);
			return ItemExists(layoutName, publishedFiltersRenamed, out isExistingItemUserDefinedFilter);
		}

		bool ItemExists(ZString layoutName, IEnumerable<LayoutsTreeNode> layouts, out bool isExistingItemUserDefinedFilter)
		{
			isExistingItemUserDefinedFilter = false;
			var selectedItem = SelectedLayoutItem;

			foreach (var item in layouts)
			{
				if (item.FilterName != selectedItem.FilterName // its ok to rename to itself, eg. changing case
					&& TrimSpacesInPath(layoutName).EqualsIgnoringCase(TrimSpacesInPath(item.FilterName)))
				{
					isExistingItemUserDefinedFilter = item.IsUserDefinedFilter;
					var bothLayoutsPublishedOrUnpublished =
						(IsSelectedLayoutPublished && IsLayoutPublished(item)) ||
						(!IsSelectedLayoutPublished && !IsLayoutPublished(item));

					if (bothLayoutsPublishedOrUnpublished || isExistingItemUserDefinedFilter || selectedItem.IsUserDefinedFilter)
					{
						return true;
					}
				}
			}

			return false;
		}

		static readonly Regex trimWhiteSpaces = new Regex(@"(?<=/)\s+|\s+(?=/)", RegexOptions.Compiled);

		internal ZString TrimSpacesInPath(string path)
		{
			return trimWhiteSpaces.Replace(path, string.Empty);
		}

		static DialogResult RequestRetryCancelFromUser(ZString currentLayoutName, bool isExistingItemUserDefinedFilter)
		{
			var messageText = isExistingItemUserDefinedFilter
				? Res.GetString("86bcd00e-93e2-4cf5-a2e3-a4420ba37ef1", "The user-defined filter '{0}' already exists. Please enter a different name.", currentLayoutName)
				: Res.GetString("711fbf56-f90d-4102-9e3d-91d3f2939e08", "The filter layout '{0}' already exists. Please enter a different name.", currentLayoutName);

			return Globals.Message.Show(messageText, Res.GetString("c4cec0da-0038-40bc-97a1-08baf2a1e806", "Layout Exists"), MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
		}

		protected virtual ZString RequestLayoutNameFromUser()
		{
			var selectedItem = SelectedLayoutItem;
			var layoutsRenamed = PrepareRenamedLayoutsForSave();
			string currentLayoutName = null;
			layoutsRenamed?.TryGetValue(selectedItem.Layout.PK, out currentLayoutName);

			var inputMessage = selectedItem.IsUserDefinedFilter
				? Res.GetString("f0f5a03c-28eb-4220-9637-7a57e5ee3289", "Enter a new name for this user-defined filter:")
				: Res.GetString("d218bccd-2659-4348-99c2-dc4ce937dfe4", "Enter a new name for this filter layout:");
			ZString result = InputBox.Show(inputMessage, Res.GetString("44bca232-7790-4229-825e-1dcec1e27931", "Rename Layout"), currentLayoutName ?? selectedItem.FilterName, false);

			return result.SubstringSafe(0, StmModuleFilter.Schema.S9_FilterNameMaxLength);
		}

		LayoutsTreeNode SelectedLayoutItem => FiltersTreeView.SelectedNode as LayoutsTreeNode;
		internal event EventHandler UserDefinedFiltersChanged;

		#endregion

		#region Delete Layout

		void DeleteLayoutButton_Click(object sender, EventArgs e)
		{
			DeleteSelectedLayout();
		}

		protected void DeleteSelectedLayout()
		{
			var selectedLayout = SelectedLayoutItem;

			if (selectedLayout != null)
			{
				var deleteLayout = true;

				if (selectedLayout.IsUserDefinedFilter)
				{
					if (!IsAllowedToModifyUserDefinedFilter())
					{
						ShowSecurityError();
						deleteLayout = false;
					}
					else
					{
						var layout = layoutManageable.FindLayout(selectedLayout.FilterName) as StmModuleFilter;

						if (layout != null && !UserDefinedFilterHelper.CheckIsOkayToAdjustUserDefinedFilter(selectedLayout.FilterName, layout.S9_FilterNameMultilingual, layoutManageable.Factory, UserDefinedFilterHelper.LayoutAdjustmentMode.Delete))
						{
							deleteLayout = false;
						}
					}
				}
				else if (IsSelectedLayoutPublished || IsSelectedLayoutPublishedAcrossAllCompanies)
				{
					if (!EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed ||
						IsSelectedLayoutPublishedAcrossAllCompanies && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches)
					{
						ShowSecurityError();
						deleteLayout = false;
					}
					else
					{
						string msg;
						if (IsSelectedLayoutPublishedAcrossAllCompanies)
						{
							msg = Res.GetString("3c3f3dc3-045d-4932-bd9d-7a6ade90a8e7",
								"The selected layout is available across all companies.\r\n\r\nARE YOU SURE YOU WANT TO DELETE THIS FILTER LAYOUT FOR *ALL* COMPANIES?");
						}
						else
						{
							msg = Res.GetString("72391a68-7c2c-47e7-817d-e07d8c22977f",
								"The selected layout is available to all users.\r\n\r\nARE YOU SURE YOU WANT TO DELETE THIS FILTER LAYOUT FOR *ALL* USERS?");
						}

						var result = Globals.Message.Show(msg, Res.GetString("ee877d37-2f49-4de4-963a-0aa006636147", "Global Filter Layout Warning!"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, DialogResult.No);
						deleteLayout = (result == DialogResult.Yes);
					}
				}
				else if (!IsSelectedLayoutDeletionAllowed)
				{
					deleteLayout = false;
					var msg = Res.GetString("d71c9c3f-5a6a-4a30-8f16-9fc4f65ca3a2", "The selected layout cannot be deleted.");
					Globals.Message.ShowError(msg);
				}

				if (deleteLayout)
				{
					DeletedLayouts.Add(selectedLayout.Layout.PK);

					var topNode = selectedLayout.Layout.IsPublished ? FiltersTreeView.Nodes[1] : FiltersTreeView.Nodes[0];

					if (RemoveFromTreeNode(topNode, selectedLayout.Layout.PK))
					{
						ReloadLayoutExpandedStates();
						SaveButton.Enabled = true;
						SaveCloseButton.Enabled = true;

						// select top node
						SelectFirstLayout();
					}
					else
					{
						throw new ArgumentException("The selected layout wasn't removed properly from the filters tree view. Please try again.");
					}
				}
			}
		}

		bool RemoveFromTreeNode(TreeNode topNode, ZGuid selectedLayoutPK)
		{
			foreach (var node in topNode.Nodes)
			{
				var layoutNode = node as LayoutsTreeNode;
				if (layoutNode?.Layout?.PK == selectedLayoutPK)
				{
					if (layoutNode.Nodes.Count == 0)
					{
						var lastNodeToRemove = layoutNode as TreeNode;
						if (layoutNode.Parent.Nodes.Count == 1)
						{
							var parent = layoutNode.Parent;
							while (parent.Nodes.Count == 1 && parent.Parent != null && ((LayoutsTreeNode)parent)?.Layout == null) // && !newName.Contains(parent.FullPath) ?
							{
								RemoveFromExpandedStates(parent);
								lastNodeToRemove = parent;
								parent = parent.Parent;
							}
						}

						if (lastNodeToRemove.Parent.Nodes.Count == 1)
						{
							RemoveFromExpandedStates(lastNodeToRemove.Parent);
						}
						lastNodeToRemove.Remove();
					}
					else
					{
						// refresh right panel
						FiltersTreeView_AfterSelect(null, new TreeViewEventArgs(layoutNode));

						// remove attached layout
						layoutNode.UpdateProperties(layoutNode.Text, null, layoutManageable.Factory); // Is Factory correct ? remove comment if tests ok
					}
					return true;
				}
				else if (layoutNode.Nodes.Count > 0 && RemoveFromTreeNode(layoutNode, selectedLayoutPK))
				{
					return true;
				}
			}
			return false;
		}

		bool IsAllowedToModifyUserDefinedFilter()
		{
			return !IsSelectedLayoutPublished || EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed;
		}

		List<ZGuid> DeletedLayouts
		{
			get { return deletedLayouts ?? (deletedLayouts = new List<ZGuid>()); }
		}
		List<ZGuid> deletedLayouts;

		#endregion

		#region IsSelectedLayoutPublished

		bool IsSelectedLayoutPublished
		{
			get { return IsLayoutPublished(SelectedLayoutItem); }
		}

		bool IsLayoutPublished(LayoutsTreeNode layoutItem)
		{
			return layoutItem?.Layout?.IsPublished ?? false;
		}

		bool IsSelectedLayoutPublishedAcrossAllCompanies
		{
			get { return IsLayoutPublishedAcrossAllCompanies(SelectedLayoutItem); }
		}

		bool IsLayoutPublishedAcrossAllCompanies(LayoutsTreeNode layoutItem)
		{
			return layoutItem?.Layout?.IsPublishedAcrossAllCompanies ?? false;
		}

		#endregion

		#region IsSelectedLayoutSystemDefined

		bool IsSelectedLayoutSystemDefined
		{
			get { return IsLayoutSystemDefined(SelectedLayoutItem); }
		}

		bool IsLayoutSystemDefined(LayoutsTreeNode layoutItem)
		{
			return layoutItem?.Layout?.IsSystemDefined ?? false;
		}

		#endregion

		#region IsSelectedLayoutNotAllowedForRenaming

		bool IsSelectedLayoutAllowedForRenaming => SelectedLayoutItem?.Layout?.IsRenameAllowed ?? false;

		bool IsSelectedLayoutDeletionAllowed => SelectedLayoutItem?.Layout?.IsDeleteAllowed ?? false;

		#endregion

		#region ChangeSavingOfColumnLayouts

		void SaveColumnsCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!ChangingCheckBoxValueSemaphore.IsSuspended)
			{
				ChangeSelectedLayoutSavingOfColumnLayouts();
			}
		}

		void ChangeSelectedLayoutSavingOfColumnLayouts()
		{
			if (SelectedLayoutItem != null)
			{
				if (IsSelectedLayoutPublished && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed ||
					IsSelectedLayoutPublishedAcrossAllCompanies && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches)
				{
					UncheckSavingColumnsCheckBox();
					ShowSecurityError();
				}
				else
				{
					SelectedLayoutItem.SaveColumnLayouts = SaveColumnsCheckBox.Checked;
					SaveButton.Enabled = true;
					SaveCloseButton.Enabled = true;
				}
			}
		}

		void UncheckSavingColumnsCheckBox()
		{
			using (new SemaphoreManager(ChangingCheckBoxValueSemaphore))
			{
				SaveColumnsCheckBox.Checked = !SaveColumnsCheckBox.Checked;
			}
		}

		Semaphore ChangingCheckBoxValueSemaphore
		{
			get { return changingCheckBoxValueSemaphore ?? (changingCheckBoxValueSemaphore = new Semaphore()); }
		}
		Semaphore changingCheckBoxValueSemaphore;

		#endregion

		#region ChangeSavingOfGridColours

		void SaveGridColourCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!ChangingCheckBoxValueSemaphore.IsSuspended)
			{
				ChangeSelectedLayoutSavingOfGridColours();
			}
		}

		void ChangeSelectedLayoutSavingOfGridColours()
		{
			if (SelectedLayoutItem != null)
			{
				if (IsSelectedLayoutPublished && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed ||
					IsSelectedLayoutPublishedAcrossAllCompanies && !EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches)
				{
					UncheckSavingGridColoursCheckBox();
					ShowSecurityError();
				}
				else
				{
					SelectedLayoutItem.SaveGridColourLayouts = SaveGridColourCheckBox.Checked;
					SaveButton.Enabled = true;
					SaveCloseButton.Enabled = true;

					if (SaveGridColourCheckBox.Checked)
					{
						if (gridLastUsedColourScheme != null && string.IsNullOrEmpty(SelectedLayoutItem.GridColourLayoutName))
						{
							SaveGridColourNameTextBox.Text = gridLastUsedColourScheme.S9_FilterName;
						}
						else
						{
							SaveGridColourNameTextBox.Text = SelectedLayoutItem.GridColourLayoutName;
						}
					}
					else
					{
						SaveGridColourNameTextBox.Text = string.Empty;
					}
				}
			}
		}

		void UncheckSavingGridColoursCheckBox()
		{
			using (new SemaphoreManager(ChangingCheckBoxValueSemaphore))
			{
				SaveGridColourCheckBox.Checked = !SaveGridColourCheckBox.Checked;
			}
		}

		#endregion

		#region Save

		void SaveCloseButton_Click(object sender, EventArgs e)
		{
			try
			{
				SavePendingChanges();
				Close();
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveConcurrencyException(ex);
			}
			catch (ZSaveException ex)
			{
				HandleSaveException(ex);
			}
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			try
			{
				SavePendingChanges();
			}
			catch (ZSaveConcurrencyException ex)
			{
				HandleSaveConcurrencyException(ex);
			}
			catch (ZSaveException ex)
			{
				HandleSaveException(ex);
			}
		}

		void HandleSaveConcurrencyException(ZSaveConcurrencyException ex)
		{
			RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilter.Schema.TableName);
			RowFactory.ClearSpecificTableFromUberFactory(StmModuleFilterUserData.Schema.TableName);

			Globals.Message.ShowError(new ConcurrencyExceptionHandler(ex).UserFriendlyMessage);

			foreach (var control in this.Controls.Cast<Control>())
			{
				control.Enabled = false;
			}
			this.CancelChangesButton.Enabled = true;
		}

		void SavePendingChanges()
		{
			var layoutNamesChanged = PrepareRenamedLayoutsForSave();
			var saveColumnSettingsChanged = PrepareSaveColumnsChangeForSave();
			var gridLastUsedColourSchemePk = gridLastUsedColourScheme?.PK ?? ZGuid.Empty;

			new LayoutSaveManager().SaveLayoutChanges(layoutManageable, layoutNamesChanged, DeletedLayouts, saveColumnSettingsChanged, gridLastUsedColourSchemePk);
			UserDefinedFiltersChanged?.Invoke(this, null);
		}

		Dictionary<ZGuid, string> PrepareRenamedLayoutsForSave()
			=> LayoutNodes.Where(n => n.IsNameChanged).ToDictionary(n => n.Layout.PK, n => n.FilterName);

		IEnumerable<(ZGuid, ZBool, ZBool)> PrepareSaveColumnsChangeForSave()
			=> from node in LayoutNodes
						select (node.Layout.PK, node.SaveColumnLayouts, node.SaveGridColourLayouts);

		IEnumerable<LayoutsTreeNode> LayoutNodes
			=> FiltersTreeView.Nodes.Cast<TreeNode>()
				.SelectMany(node => node.SelectRecursive(child => child.Nodes.Cast<TreeNode>()))
				.OfType<LayoutsTreeNode>()
				.Where(node => node.Layout != null);

		#endregion

		#region Cancel

		void CancelChangesButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region Showing Error Messages

		void ShowSecurityError()
		{
			var msg = Res.GetString("a2619bec-e0e3-40cd-970c-c15c54a8a511", "The selected layout is available to all users and can only be modified by an administrator.");
			Globals.Message.ShowError(msg);
		}

		#endregion

		void editLocalLanguageValuesButton_Click(object sender, EventArgs e)
		{
			StmModuleFilter filter = null;
			if (SelectedLayoutItem != null)
			{
				filter = SelectedLayoutItem.Layout as StmModuleFilter;
			}
			if (filter != null && !filter.S9_IsPublished)
			{
				filter = null;
			}
			if (filter == null)
			{
				filter = layoutManageable.GetLayouts(true).FirstOrDefault();
			}
			if (filter == null)
			{
				Globals.Message.ShowWarning(Res.GetString("2ac79e2c-12fc-4ddb-89e5-2a3e80658976", "No published layouts found for the current module."));
			}
			else
			{
				ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(filter.S9_FilterNameInfo.CustomizableDataResourceStrings, filter.S9_FilterNameMultilingual as ResourceString, filter);
			}
		}

#if DEBUG
		public BusinessObjectFactory FactoryExposed => layoutManageable.Factory;
#endif
	}

	#region class LayoutsTreeNode

	internal class LayoutsTreeNode : TreeNode
	{
		public LayoutsTreeNode(string displayText) : base(displayText)
		{
		}

		public LayoutsTreeNode(string displayText, IGridLayoutStorage layout, BusinessObjectFactory factory, string filterLayoutName = null)
		{
			UpdateProperties(displayText, layout, factory, filterLayoutName);
		}

		public void UpdateProperties(string displayText, IGridLayoutStorage layout, BusinessObjectFactory factory, string filterLayoutName = null)
		{
			IsUserDefinedFilter = (layout as StmModuleFilter)?.IsUserDefinedFilter ?? false;
			Layout = layout;
			FilterName = filterLayoutName ?? layout?.ColumnLayoutName;
			if (!IsUserDefinedFilter)
			{
				Text = displayText;
			}
			SaveColumnLayouts = layout?.SaveColumnLayout ?? false;
			SaveGridColourLayouts = layout?.SaveGridColourLayout ?? false;
			if (SaveGridColourLayouts && layout != null && factory != null)
			{
				var colourLayout = factory.Load<StmModuleFilter>(layout.GridColourLayoutID);
				GridColourLayoutName = colourLayout?.S9_FilterName ?? string.Empty;
			}
		}

		public IGridLayoutStorage Layout { get; private set; }
		public ZBool SaveColumnLayouts { get; set; }
		public bool IsUserDefinedFilter { get; private set; }
		public ZBool SaveGridColourLayouts { get; set; }
		public string GridColourLayoutName { get; set; }

		public string FilterName
		{
			get { return filterName; }
			set
			{
				filterName = value;
				var text = filterName?.Split('/').Last();
				Text = StmModuleFilter.GetDisplayName(text, IsUserDefinedFilter);
			}
		}
		string filterName;

		public bool IsNameChanged
		{
			get { return Layout?.ColumnLayoutName != FilterName; }
		}
	}

	#endregion
}

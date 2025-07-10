using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryComparisonForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public RegistryComparisonForm()
		{
			InitializeComponent();
		}

		public new RegistryComparisonBusinessObject BusinessEntity { get { return (RegistryComparisonBusinessObject)base.BusinessEntity; } }
		protected readonly BusinessObjectFactory factory;
		readonly bool hideInactiveChildren;

		public RegistryComparisonForm(RegistryComparisonBusinessObject bizo, bool hideInactiveChildren = false)
			: base(bizo)
		{
			InitializeComponent();

			this.hideInactiveChildren = hideInactiveChildren;
			this.factory = new BusinessObjectFactory();

			baseLevelPanel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("85E3FBDE-E22C-43ED-A9AB-013AB6C11F80", "Value for {0} (Base)", bizo.BaseLevel.Description);
			overrideLevelPanel.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("F0760B3C-3021-4B77-83B1-F4FC6A6DF659", "Value for {0} (Override)", bizo.OverrideLevel.Description);

			AddItemsToTree(treeView, bizo.ItemsThatDifferBetweenBaseAndOverride);
			CheckAllChildren(treeView.Nodes, true);
			treeView.TreeViewNodeSorter = new RegistryTreeNodeComparer();
		}

		void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		static void AddItemsToTree(ZTreeView treeView, IReadOnlyCollection<IRegistryItem> itemsToAdd)
		{
			if (itemsToAdd.Any())
			{
				RegistryItemTreeViewBuilder.AddItemsToTree(treeView.Nodes, itemsToAdd);
			}
			else
			{
				treeView.Nodes.Add(Res.GetString("1CEF3C10-B521-4846-93D3-791C6748AC45", "No differences were found"));
			}
		}

#if DEBUG
		internal
#endif
		void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			UpdateDisplay();
		}

		protected void UpdateDisplay()
		{
			var registryItem = treeView.SelectedNode.Tag as IRegistryItem;
			if (registryItem != null)
			{
				SetDisplay(baseLevelPanel, BusinessEntity.BaseLevel, registryItem);
				SetDisplay(overrideLevelPanel, BusinessEntity.OverrideLevel, registryItem);
			}
		}

		internal const string ActivePluginControlName = "ActivePluginControl";
		void SetDisplay(ZGroupBox parentContainer, IOverrideLevel level, IRegistryItem registryItem)
		{
			var activePluginControl = parentContainer.Controls[ActivePluginControlName];
			if (activePluginControl != null && parentContainer.Contains(activePluginControl))
			{
				parentContainer.Controls.Remove(activePluginControl);
				activePluginControl.Dispose();
			}

			var editor = RegistryItemEditorFactory.NewEditor(registryItem, level.GetFallbackLevel(), registryItem.DataType, registryItem.EditorInfo, factory);

			if (editor != null)
			{
				activePluginControl = editor.NewWinFormsEditorPane();
				activePluginControl.Name = ActivePluginControlName;

				activePluginControl.Location = ControlDpiScalingHelper.NewScaledPoint(5, 15);
				editor.SetEditorPaneLayout(activePluginControl, parentContainer.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(20), parentContainer.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(20));
				BindingSource.SetBindingMember(activePluginControl, "");

				editor.SetValueFromEditorPane(activePluginControl, level.GetValueOf(registryItem));
				editor.EnableEditorPane(activePluginControl, false);
				parentContainer.Controls.Add(activePluginControl);
			}

			Refresh();
		}

		public void treeView_AfterCheck(object sender, TreeViewEventArgs args)
		{
			CheckAllChildren(args.Node.Nodes, args.Node.Checked);
		}

		void CheckAllChildren(TreeNodeCollection collection, bool checkedState)
		{
			foreach (TreeNode child in collection)
			{
				child.Checked = checkedState; //Event handler will manage the children
			}
		}

		public IEnumerable<IRegistryItem> CheckedRegistryItems
		{
			get { return RegistryItemTreeViewBuilder.GetRegistryItems(treeView.Nodes, true); }
		}

		protected void ShowApplyFormForAnotherLevel(IEnumerable<IRegistryItem> itemsToUse, IOverrideLevel overrideLevel)
		{
			var newLevel = GetLevelToApplyTo();

			if (newLevel != null)
			{
				var newComparisonObject = new RegistryComparisonBusinessObject(itemsToUse, newLevel, overrideLevel);
				var applyForm = new RegistryApplyForm(newComparisonObject, hideInactiveChildren);

				applyForm.Show();
				Close();
			}
		}

		protected IOverrideLevel GetLevelToApplyTo()
		{
			using (var form = new SelectOverrideLevelForm(factory, hideInactiveChildren))
			{
				return ZFormModaliser.ShowDialogWithoutDispose(form, this) == DialogResult.OK ? form.SelectedOveride : null;
			}
		}
	}
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using ResString = Enterprise.ZArchitecture.GUI.UserControls.ResString;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZTreeViewControl : ZUserControl
	{
		public ZTreeViewControl()
		{
			InitializeComponent();

			NameOfATreeElement = DefaultNameOfATreeElement;
			NameOfTreeElementsPlural = DefaultNameOfTreeElementsPlural;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupButtons();
				SetupTree();

				SetupReadOnlySettings();
			}
		}

		#region DataBinding

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				Tree.Model = GetNewTreeModelView();
				ExpandRequired();
			}
			else
			{
				Tree.Model = null;
			}
		}

		protected virtual void ExpandRequired()
		{
			Tree.ExpandAll();
		}

		protected virtual IZTreeModelView GetNewTreeModelView()
		{
			throw new NotImplementedException("Override and return you tree model view");
		}

		#endregion

		#region View

		public IZTreeModelView ModelView
		{
			get { return Tree.Model; }
		}

		protected virtual ZTreeViewAdv CreateNewTreeViewAdv()
		{
			return new ZTreeViewAdv();
		}

		#endregion

		#region Tree

		protected virtual void SetupTree()
		{
			Tree.RowDraw += TreeViewAdv_RowDraw;
		}

		void TreeViewAdv_RowDraw(object sender, TreeViewRowDrawEventArgs e)
		{
			var brush = GetRowBackgroundBrush(e.Node);
#if !WINZOR
			if (brush != null)
			{
				var y = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.RowRect.Y);
				var width = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.ClipRectangle.Width + Math.Abs((int)e.Graphics.Transform.OffsetX));
				var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.RowRect.Height);
				var rect = ControlDpiScalingHelper.NewScaledRectangle(0, y, width, height);
				e.Graphics.FillRectangle(brush, rect);
			}
#else
			e.Node.RowBackgroundBrush = brush;
#endif
		}

		protected virtual Brush GetRowBackgroundBrush(TreeNodeAdv node)
		{
			if (Tree.SelectedNodes.Any(selectedNode => selectedNode.Tag == node.Tag))
			{
				return SystemBrushes.Highlight;
			}

			return null;
		}

		#endregion

		#region Buttons

		bool showNewButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Show or hide the tree's New button.")]
		[DefaultValue(true)]
		public bool ShowNewButton
		{
			get { return showNewButton; }
			set
			{
				if (NewToolStripButton != null)
				{
					NewToolStripButton.Visible = value;
				}

				showNewButton = value;
			}
		}

		bool showEditButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Show or hide the tree's Edit button.")]
		[DefaultValue(true)]
		public bool ShowEditButton
		{
			get { return showEditButton; }
			set
			{
				if (editToolStripButton != null)
				{
					editToolStripButton.Visible = value;
				}

				showEditButton = value;
			}
		}

		bool showAttachButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Show or hide the tree's Attach button.")]
		[DefaultValue(true)]
		public bool ShowAttachButton
		{
			get { return showAttachButton; }
			set
			{
				if (AttachToolStripButton != null)
				{
					AttachToolStripButton.Visible = value;
				}

				showAttachButton = value;
			}
		}

		bool showDetachButton = true;
		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("Show or hide the tree's Detach button.")]
		[DefaultValue(true)]
		public bool ShowDetachButton
		{
			get { return showDetachButton; }
			set
			{
				if (detachToolStripButton != null)
				{
					detachToolStripButton.Visible = value;
				}

				showDetachButton = value;
			}
		}

		protected virtual void SetupButtons()
		{
			SetupDetachActivityButton();
			SetupAttachActivityButton();
			SetupEditActivityButton();
			SetupNewActivityButton();
		}

		#endregion

		#region Edit

		public ZToolStripButton EditToolStripButton
		{
			get { return editToolStripButton; }
		}

		protected virtual void SetupEditActivityButton()
		{
			editToolStripButton.Image = Icons.GetImage(IconTypes.EditButtonRest);
			editToolStripButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
		}

		void EditToolStripButton_Click(object sender, EventArgs e)
		{
			var selectedNodes = Tree.SelectedNodes;
			if (selectedNodes.Count > 0)
			{
				foreach (var node in selectedNodes)
				{
					ShowEditForm(((IZNode)node.Tag).BizObjForBinding);
				}

				OnAfterAllEdits();
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("9d244013-f878-4442-8b59-0ed9edd02a07", "Please select an item in the grid"), ResString.GetMultilingualString("bbb3aafb-0f7f-4876-bb69-31d2f94aa85a", "Open {0}", NameOfATreeElement.Caption), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		protected virtual void OnAfterAllEdits()
		{
		}

		void TreeViewAdv_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			var relationNode = e.Node.Tag as IZNode;
			ShowEditForm(relationNode.BizObjForBinding);
			e.Handled = true;
		}

		protected virtual void ShowEditForm(IBusiness bizObjToEdit)
		{
		}

		void TreeViewAdv_NodeMouseClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			var relationNode = e.Node.Tag as IZNode;
			SelectTreeNode(relationNode.BizObjForBinding);
			e.Handled = true;
		}

		protected virtual void SelectTreeNode(IBusiness bizObjToSelect)
		{
		}

		#endregion

		#region New

		public ZToolStripButton NewToolStripButton
		{
			get { return newToolStripButton; }
		}

		protected virtual void NewToolStripButton_Click(object sender, EventArgs e)
		{
		}

		protected virtual void SetupNewActivityButton()
		{
			NewToolStripButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			NewToolStripButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
		}

		#endregion

		#region Attach

		public ZToolStripButton AttachToolStripButton
		{
			get { return attachToolStripButton; }
		}

		protected virtual void AttachToolStripButton_Click(object sender, EventArgs e)
		{
		}

		protected virtual void SetupAttachActivityButton()
		{
			AttachToolStripButton.Image = Icons.GetImage(IconTypes.BlackWhite_Add);
			AttachToolStripButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
		}

		#endregion

		#region Detach

		public ZToolStripButton DetachToolStripButton
		{
			get { return detachToolStripButton; }
		}

		protected virtual void SetupDetachActivityButton()
		{
			detachToolStripButton.Image = Icons.GetImage(IconTypes.BlackWhite_Remove);
			detachToolStripButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
		}

		void DetachToolStripButton_Click(object sender, EventArgs e)
		{
			DetachSelectedElements();
		}

		void Tree_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				DetachSelectedElements();
			}
		}

		protected virtual void DetachSelectedElements()
		{
			var selectedNodes = Tree.SelectedNodes;
			if (selectedNodes.Count == 0)
			{
				Globals.Message.Show(ResString.GetMultilingualString("f5cb216e-b748-4b18-b88f-05417164dd83", "Please select an item in the grid."), ResString.GetMultilingualString("d536f6e8-6ee4-4e3e-8d16-d1ae794c2841", "Detach {0}", NameOfATreeElement.Caption), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				if (ModelView == null || ModelView.SecurityCheckpointForEdit.IsAllowed)
				{
					if (Globals.Message.Show(DialogMessageForDetachNode, DialogCaptionForDetachNode, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
					{
						foreach (var node in selectedNodes.ToArray())
						{
							((IZNode)node.Tag).RemoveParent();
						}
					}
				}
				else
				{
					ModelView.SecurityCheckpointForEdit.ShowError();
				}
			}
		}

		protected virtual string DialogMessageForDetachNode
		{
			get
			{
				return ResString.GetMultilingualString("95e7e039-f0f8-4f12-b6e0-84d4afe3de16", "Detach the selected {0}?", NameOfTreeElementsPlural.Caption);
			}
		}

		protected virtual string DialogCaptionForDetachNode
		{
			get
			{
				return ResString.GetMultilingualString("d536f6e8-6ee4-4e3e-8d16-d1ae794c2841", "Detach {0}", NameOfATreeElement.Caption);
			}
		}

		#endregion

		#region ReadOnly

		void SetupReadOnlySettings()
		{
			SetControlsReadOnlyIfInReadOnlyMode();
			var parentZForm = ParentForm as ZForm;
			if (parentZForm != null)
			{
				parentZForm.DisplayModeChanged += parentZForm_DisplayModeChanged;
			}
		}

		void parentZForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			SetControlsReadOnlyIfInReadOnlyMode();
		}

		void SetControlsReadOnlyIfInReadOnlyMode()
		{
			var form = ParentForm as ZForm;
			if (form != null && form.DisplayMode == ODisplayMode.ReadOnly)
			{
				SetControlsReadOnly();
			}
		}

		protected virtual void SetControlsReadOnly()
		{
			AttachToolStripButton.Enabled = false;
			editToolStripButton.Enabled = false;
			NewToolStripButton.Enabled = false;
			detachToolStripButton.Enabled = false;
			Tree.ReadOnly = true;
		}

		#endregion

		#region Messages

		#region NameOfATreeElement

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("The name of the things in the tree. e.g, Order or Shipment.")]
		public ResourceStringData NameOfATreeElement
		{
			get;
			set;
		}

		protected virtual ResourceStringData DefaultNameOfATreeElement
		{
			get { return null; }
		}

		#endregion

		#region NameOfTreeElementsPlural

		public ResourceStringData NameOfTreeElementsPlural
		{
			get;
			set;
		}

		protected virtual ResourceStringData DefaultNameOfTreeElementsPlural
		{
			get { return null; }
		}

		#endregion

		#endregion
	}
}

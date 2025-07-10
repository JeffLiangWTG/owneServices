using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class ZTreeViewAdv : TreeViewAdv
	{
		public ZTreeViewAdv()
		{
			LineDashStyle = DashStyle.Solid;
			EnableTreeRestructuring();
			EnableColumnSort();

#if DEBUG
#if !WINZOR
			var scrollBar = Controls.Find("_hScrollBar", false)[0];
			TypeDescriptor.AddAttributes(scrollBar, new[] { new SuppressDpiAwareBasherAttribute() });
#endif
#endif // DEBUG

			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.None);
		}

		readonly internal TranslationFeedbackManager translationFeedbackManager;

		#region Model

		[Browsable(false)]
		[DefaultValue(null)]
		public new IZTreeModelView Model
		{
			get { return (IZTreeModelView)base.Model; }
			set { base.Model = value; }
		}

		#endregion

		#region ElementType

		[TypeConverter(typeof(TypeTypeConverter))]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Message")]
		public virtual Type ElementType
		{
			get { return elementType; }
			set
			{
				if (elementType != value)
				{
					elementType = value;
					onDragDrop_GenericMethodInfo = null;

					if (this.IsDesignMode())
					{
						if (TypeNameHolder.IsTypeFakeAndNotExistsInSolution(value))
						{
							DesignTimeUI.ShowMessage(this, "Could not find the type '" + value.FullName + "'");
						}
					}
				}
			}
		}
		Type elementType;

		#endregion

		#region Restructure Tree

		void EnableTreeRestructuring()
		{
			ItemDrag += TreeViewAdv_ItemDrag;
		}

		#region TreeViewAdv_DragOver

		protected override void OnDragOver(DragEventArgs dragEvent)
		{
			base.OnDragOver(dragEvent);

			if (dragEvent.Data.GetDataPresent(typeof(TreeNodeAdv[])) && DropPosition.Node != null)
			{
				TreeNodeAdv[] nodes = dragEvent.Data.GetData(typeof(TreeNodeAdv[])) as TreeNodeAdv[];
				TreeNodeAdv parent = DropPosition.Node;
				if (DropPosition.Position != NodePosition.Inside)
				{
					parent = parent.Parent;
				}

				foreach (TreeNodeAdv node in nodes)
				{
					if (!CheckNodeParent(parent, node))
					{
						dragEvent.Effect = DragDropEffects.None;
#if WINZOR
						DropPosition.Node.DropPositionEffects.Add(DragDropEffects.None);
#endif
						return;
					}
				}

				dragEvent.Effect = dragEvent.AllowedEffect;
#if WINZOR
				DropPosition.Node.DropPositionEffects.Add(dragEvent.AllowedEffect);
#endif
			}
		}

		bool CheckNodeParent(TreeNodeAdv parent, TreeNodeAdv node)
		{
			while (parent != null)
			{
				if (node == parent)
				{
					return false;
				}
				else
				{
					parent = parent.Parent;
				}
			}
			return true;
		}

		#endregion

		#region DragDrop

		protected override void OnDragDrop(DragEventArgs dragEvent)
		{
			base.OnDragDrop(dragEvent);

			TreeNodeAdv[] nodes = (TreeNodeAdv[])dragEvent.Data.GetData(typeof(TreeNodeAdv[]));
			OnDragDrop_GenericMethodInfo.Invoke(this, new object[] { nodes, DropPosition });
		}

		MethodInfo OnDragDrop_GenericMethodInfo
		{
			get
			{
				if (onDragDrop_GenericMethodInfo == null)
				{
					onDragDrop_GenericMethodInfo = typeof(ZTreeViewAdv).GetMethod("OnNodeDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(ElementType);
				}

				return onDragDrop_GenericMethodInfo;
			}
		}
		MethodInfo onDragDrop_GenericMethodInfo;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
		void OnNodeDragDrop<T>(TreeNodeAdv[] nodes, DropPosition dropPosition)
			where T : class, IBusiness
		{
			if (Model != null && Model.SecurityCheckpointForEdit != null && !Model.SecurityCheckpointForEdit.IsAllowed)
			{
				Model.SecurityCheckpointForEdit.ShowError();
			}

			if (dropPosition.Node == null)
			{
				return;
			}

			BeginUpdate();

			ZNode<T> dropNode = dropPosition.Node.Tag as ZNode<T>;
			if (dropPosition.Position == NodePosition.Inside)
			{
				OnNodeDragDrop_Inside(nodes.Select(n => n.Tag as ZNode<T>).ToList(), dropNode);
			}
			else if (dropPosition.Position == NodePosition.Before)
			{
				var node = nodes.Length > 0 ? nodes[0].Tag as ZNode<T> : null;
				if (node != null)
				{
					OnNodeDragDrop_Before(node, dropNode);
				}
			}

			EndUpdate();

			var newNode = FindNodeByTag(dropPosition.Node.Tag);
			while (newNode != null)
			{
				newNode.Expand();
				newNode = newNode.Parent;
			}
		}

		protected virtual void OnNodeDragDrop_Inside<T>(List<ZNode<T>> nodes, ZNode<T> dropNode)
			where T : class, IBusiness
		{
			foreach (var node in nodes)
			{
				if (node.ParentNode == dropNode)
				{
					SwapNodeWithParent(node);
				}
				else
				{
					var nodeParent = node.ParentNode;
					if (
						!UnsetParentNode(node) ||
						!SetParentNodeIfValid(node, dropNode))
					{
						node.SetParentNode(nodeParent, false);
					}
				}
			}
		}

		void SwapNodeWithParent<T>(ZNode<T> child)
			where T : class, IBusiness
		{
			var parent = child.ParentNode;
			var grandparent = parent.ParentNode;

			if (
				!UnsetParentNode(child) ||
				!UnsetParentNode(parent) ||
				!SetParentNodeIfValid(child, grandparent) ||
				!SetParentNodeIfValid(parent, child))
			{
				// Don't change the order of the lines below! Setting parent of child first could potentially cause an infinite loop
				parent.SetParentNode(grandparent, false);
				child.SetParentNode(parent, false);
			}
		}

		protected virtual void OnNodeDragDrop_Before<T>(ZNode<T> node, ZNode<T> dropNode)
			where T : class, IBusiness
		{
			var nodeParent = node.ParentNode;
			var dropNodeParent = dropNode.ParentNode;

			if (
				!UnsetParentNode(node) ||
				!UnsetParentNode(dropNode) ||
				!SetParentNodeIfValid(node, dropNodeParent) ||
				!SetParentNodeIfValid(dropNode, node))
			{
				// Don't change the order of the lines below! Setting parent of node first could potentially cause an infinite loop
				dropNode.SetParentNode(dropNodeParent, false);
				node.SetParentNode(nodeParent, false);
			}
		}

		protected virtual bool SetParentNodeIfValid<T>(ZNode<T> child, ZNode<T> parent)
			where T : class, IBusiness
		{
			return child.SetParentNode(parent, true);
		}

		protected virtual bool UnsetParentNode<T>(ZNode<T> node)
			where T : class, IBusiness
		{
			return node.SetParentNode(null, false);
		}

		#endregion

		#region TreeViewAdv_ItemDrag

		void TreeViewAdv_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDropSelectedNodes(DragDropEffects.Move);
		}

		#endregion

		protected override bool AllowDropPositionBefore(TreeNodeAdv node)
		{
			return node.Parent == node.Tree.Root;
		}

		protected override bool AllowDropPositionAfter(TreeNodeAdv node)
		{
			return false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && translationFeedbackManager != null)
			{
				translationFeedbackManager.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Column Sort

		void EnableColumnSort()
		{
			ColumnClicked += ZTreeViewAdv_ColumnClicked;
		}

		void ZTreeViewAdv_ColumnClicked(object sender, TreeColumnEventArgs e)
		{
			if (translationFeedbackManager.InMode)
			{
				translationFeedbackManager.HandleClick();
				return;
			}
			var clicked = e.Column;
			var sortOrder = (clicked.SortOrder == System.Windows.Forms.SortOrder.Ascending) ? System.Windows.Forms.SortOrder.Descending : System.Windows.Forms.SortOrder.Ascending;
			SetSortColumn(e.Column, sortOrder);
		}

		public void SetSortColumn(TreeColumn column, System.Windows.Forms.SortOrder sortOrder)
		{
			column.SortOrder = sortOrder;
			var model = Model;
			if (model != null)
			{
				var sortProperties = GetSortProperties(column);
				if (SortPropertiesNeeded != null)
				{
					var args = new ZTreeViewSortEventArgs(column, sortOrder, sortProperties);
					SortPropertiesNeeded(this, args);

					sortProperties = args.SortProperties;
				}

				model.SortProperties = sortProperties.ToArray();
			}
		}

		public event EventHandler<ZTreeViewSortEventArgs> SortPropertiesNeeded;

		List<Tuple<string, System.Windows.Forms.SortOrder>> GetSortProperties(TreeColumn column)
		{
			var sortProperties = new List<Tuple<string, System.Windows.Forms.SortOrder>>();
			foreach (var node in NodeControls)
			{
				if (node.ParentColumn == column)
				{
					var bindableControl = node as BindableControl;
					if (bindableControl != null)
					{
						sortProperties.Add(Tuple.Create(bindableControl.DataPropertyName, column.SortOrder));
					}
				}
			}

			return sortProperties;
		}

		#endregion

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			Rectangle box = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(0, 0, this.Width, GetTreeColumnHeaderHeight());

			if (box.Contains(p))
			{
				int currentWidth = 0;

				foreach (TreeColumn column in this.Columns)
				{
					if (p.X < (currentWidth + column.Width))
					{
						return column;
					}
					else
					{
						currentWidth += column.Width;
					}
				}
			}

			return null;
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			int mouseOverColumn = ((TreeColumn)component).Index;
			int xAdjustment = 0;

			for (int column = 0; column < mouseOverColumn; column++)
			{
				xAdjustment += this.Columns[column].Width;
			}

			return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledRectangle(xAdjustment, 0, ((TreeColumn)component).Width, GetTreeColumnHeaderHeight());
		}

		public object GetCaptionedComponentData(object component)
		{
			return component;
		}

		int GetTreeColumnHeaderHeight()
		{
			return (Application.RenderWithVisualStyles) ? 20 : 17;
		}

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;

					if (value)
					{
						foreach (var nodeControl in NodeControls.OfType<InteractiveControl>())
						{
							if (nodeControl.EditEnabled)
							{
								DisabledNodeControlsForReadOnly.Add(nodeControl);
								nodeControl.EditEnabled = false;
							}
						}
					}
					else if (disabledNodeControlsForReadOnly != null)
					{
						foreach (var nodeControl in disabledNodeControlsForReadOnly)
						{
							nodeControl.EditEnabled = true;
						}
						disabledNodeControlsForReadOnly.Clear();
					}
				}
			}
		}
		bool readOnly;

		HashSet<InteractiveControl> DisabledNodeControlsForReadOnly
		{
			get { return disabledNodeControlsForReadOnly ?? (disabledNodeControlsForReadOnly = new HashSet<InteractiveControl>()); }
		}
		HashSet<InteractiveControl> disabledNodeControlsForReadOnly;

		#endregion
	}

	public class ZTreeViewSortEventArgs : EventArgs
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ZTreeViewSortEventArgs(TreeColumn column, System.Windows.Forms.SortOrder sortOrder, List<Tuple<string, System.Windows.Forms.SortOrder>> sortProperties)
		{
			Column = column;
			SortOrder = sortOrder;
			SortProperties = sortProperties;
		}

		public readonly TreeColumn Column;
		public readonly System.Windows.Forms.SortOrder SortOrder;

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public List<Tuple<string, System.Windows.Forms.SortOrder>> SortProperties;
	}
}

using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Notifications;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.DevTools
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ZNotificationsForm : Form // Notification discovery form is not part of Enterprise.
	{
		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion

		#region Auto

		System.ComponentModel.IContainer components;

		#endregion

		public ZNotificationsForm(IBusiness rootBizObjOrCollection)
		{
			InitializeComponent();

			this.RootBizObjOrCollection = rootBizObjOrCollection;
			this.CurrentNotificationType = null;

			BuildNotificationTree();
			ToggleOptionsPanel();
		}

#if DEBUG

		public ZNotificationsForm()
		{
			InitializeComponent();
		}

#endif

		readonly IBusiness RootBizObjOrCollection;
		INotificationType CurrentNotificationType;

		#region BuildNotificationTree

		void BuildNotificationTree()
		{
			NotificationsTree.BeginUpdate();

			TopNode.Nodes.Clear();
			BuildNotificationTree(RootBizObjOrCollection, TopNode);
			TopNode.ExpandAll();

			NotificationsTree.EndUpdate();
		}

		void BuildNotificationTree(IBusiness bizObjOrCollection, TreeNode currentNode)
		{
			if (bizObjOrCollection is IBusinessObjectCollection)
			{
				currentNode = AddCollectionNode((IBusinessObjectCollection)bizObjOrCollection, currentNode);
			}
			else // BusinessObject
			{
				var bizObj = (BusinessObject)bizObjOrCollection;
				currentNode = AddBizObjNode(bizObj, currentNode);

				AddRowLevelNotifications(bizObj, currentNode);
			}

			foreach (var child in bizObjOrCollection.Children)
			{
				BuildNotificationTree(child, currentNode);
			}
		}

		#region Adding Business Object & Collection Nodes

		TreeNode AddCollectionNode(IBusinessObjectCollection bizCollection, TreeNode nodeToAddTo)
		{
			var node = new TreeNode(bizCollection.GetType().Name);
			node.ForeColor = Color.Blue;

			if (bizCollection.HasChanges)
			{
				node.NodeFont = BoldNodeFont;
			}

			if (bizCollection.ReadOnly)
			{
				node.Text += " (ReadOnly)";
			}

			nodeToAddTo.Nodes.Add(node);

			return node;
		}

		TreeNode AddBizObjNode(BusinessObject bizObj, TreeNode nodeToAddTo)
		{
			var node = new TreeNode(bizObj.GetType().Name);
			node.ForeColor = Color.Blue;

			if (((IBusiness)bizObj).HasChangesNotIncludingChildren)
			{
				node.Text += " (HasChanges)";
				node.NodeFont = BoldNodeFont;
			}
			else if (bizObj.HasChanges)
			{
				node.NodeFont = BoldNodeFont;
			}

			if (bizObj.ReadOnly)
			{
				node.Text += " (ReadOnly)";
			}

			nodeToAddTo.Nodes.Add(node);
			AddNotifications(bizObj, node);

			return node;
		}

		Font BoldNodeFont
		{
			get
			{
				if (fBoldNoteFont == null)
				{
					fBoldNoteFont = new Font(OFont.NormalFontName, 8.25F, FontStyle.Bold);
				}
				return fBoldNoteFont;
			}
		}

		Font fBoldNoteFont;

		#endregion

		#region Adding Property & Notification Nodes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		void AddRowLevelNotifications(BusinessObject bizObj, TreeNode nodeToAddTo)
		{
			const string RowLevel = "(row level) ?? ";

			if (CurrentNotificationType == null)
			{
				foreach (var notification in bizObj.RowNotifications)
				{
					if (CurrentNotificationType == null || notification.Type == CurrentNotificationType)
					{
						AddNotification(nodeToAddTo, RowLevel + notification.Message, NotificationColorScheme.GetFontColor(notification.Type), NotificationIconScheme.Instance.GetImage(notification.Type));
					}
				}
			}
		}

		void AddNotifications(BusinessObject bizObj, TreeNode nodeToAddTo)
		{
			foreach (var info in bizObj.PropertiesWithNotifications)
			{
				var propertyNode = new TreeNode(info.Name);

				foreach (var notification in info.Notifications)
				{
					if (CurrentNotificationType == null || notification.Type == CurrentNotificationType)
					{
						AddNotification(propertyNode, notification.Message, NotificationColorScheme.GetFontColor(notification.Type), NotificationIconScheme.Instance.GetImage(notification.Type));
					}
				}

				if (propertyNode.Nodes.Count > 0)
				{
					nodeToAddTo.Nodes.Add(propertyNode);
				}
			}
		}

		void AddNotification(TreeNode nodeToAddTo, string message, Color textColor, Image notificationImage)
		{
			var node = new TreeNode(message);
			for (var i = 0; i < NotificationsTree.ImageList.Images.Count; i++)
			{
				if (NotificationsTree.ImageList.Images[i] == notificationImage)
				{
					node.ImageIndex = i;
					break;
				}
			}
			if (node.ImageIndex == -1)
			{
				NotificationsTree.ImageList.Images.Add(notificationImage);
				node.ImageIndex = NotificationsTree.ImageList.Images.Count - 1;
			}
			node.SelectedImageIndex = node.ImageIndex;
			node.ForeColor = textColor;
			nodeToAddTo.Nodes.Add(node);
		}

		struct NotificationImage
		{
			public const int Error = 2;
			public const int Warning = 3;
			public const int MessageError = 4;
		}

		#endregion

		#region Top Level Node

		TreeNode TopNode
		{
			get
			{
				if (fTopNode == null)
				{
					fTopNode = new TreeNode(Res.GetString("8e958327-570d-4df5-a785-d39092f875f3", "Editable Objects"));
					NotificationsTree.Nodes.Add(fTopNode);
				}
				return fTopNode;
			}
		}

		TreeNode fTopNode;

		#endregion

		#endregion

		#region Button Events

		void AllNotificationsRadio_CheckedChanged(object sender, EventArgs e)
		{
			CurrentNotificationType = null;
			BuildNotificationTree();
		}

		void ErrorsRadio_CheckedChanged(object sender, EventArgs e)
		{
			CurrentNotificationType = CargoWise.EntityFramework.NotificationType.Error;
			BuildNotificationTree();
		}

		void WarningsRadio_CheckedChanged(object sender, EventArgs e)
		{
			CurrentNotificationType = CargoWise.EntityFramework.NotificationType.Warning;
			BuildNotificationTree();
		}

		void MessageErrorsRadio_CheckedChanged(object sender, EventArgs e)
		{
			CurrentNotificationType = CargoWise.EntityFramework.NotificationType.MessageError;
			BuildNotificationTree();
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			BuildNotificationTree();
		}

		void ExpandButton_Click(object sender, EventArgs e)
		{
			NotificationsTree.BeginUpdate();
			TopNode.ExpandAll();
			NotificationsTree.EndUpdate();
		}

		void CollapseButton_Click(object sender, EventArgs e)
		{
			if (TopNode.Nodes.Count > 0)
			{
				NotificationsTree.BeginUpdate();
				TopNode.Nodes[0].Collapse();
				NotificationsTree.EndUpdate();
			}
		}

		#endregion

		#region Toggle Options Panel

		void ToggleOptionsPanel()
		{
			if (ToggleOptionsButton.Text == ">")
			{
				ToggleOptionsButton.Text = "<";
				OptionsPanel.Visible = true;
				ControlDpiScalingHelper.SetWidth(this, this.Width + OptionsPanel.Width, false);
				ControlDpiScalingHelper.SetWidth(MainPanel, MainPanel.Width - OptionsPanel.Width, false);
			}
			else
			{
				ToggleOptionsButton.Text = ">";
				OptionsPanel.Visible = false;
				ControlDpiScalingHelper.SetWidth(this, this.Width - OptionsPanel.Width, false);
				ControlDpiScalingHelper.SetWidth(MainPanel, MainPanel.Width + OptionsPanel.Width, false);
			}
		}

		void ToggleOptionsButton_Click(object sender, EventArgs e)
		{
			ToggleOptionsPanel();
		}

		#endregion

		#region Auto

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Form.OnVisibleChanged Override

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			// HACK: This eliminates the intermittent horizontal scroll bar from appearing
			var treeWith = NotificationsTree.Width;
			ControlDpiScalingHelper.SetWidth(ref NotificationsTree, 0, true);
			ControlDpiScalingHelper.SetWidth(ref NotificationsTree, treeWith, false);
		}

		#endregion
	}

	#region Image TreeView

	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	internal class ImageTreeView : TreeView
	{
		protected override void OnAfterCollapse(TreeViewEventArgs e)
		{
			base.OnAfterCollapse(e);
			e.Node.ImageIndex = 0;
		}

		protected override void OnAfterExpand(TreeViewEventArgs e)
		{
			base.OnAfterExpand(e);
			e.Node.ImageIndex = 1;
		}
	}

	#endregion
}

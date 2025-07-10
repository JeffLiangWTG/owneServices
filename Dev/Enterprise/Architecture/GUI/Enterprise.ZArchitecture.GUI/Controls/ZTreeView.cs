using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Forms.Internal;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ITreeViewWithExpandingNodes
	{
		void ExpandNodeIfNeeded(TreeNode node);
	}

	[SuppressBindingMemberBashingTest]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public class ZTreeView : KTreeView, IDynamicToolTip, ICaptionedComponents, IShouldntBeReadOnly, IHotkeyProvider
	{
		public ZTreeView()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);
			translationFeedbackManager = new TranslationFeedbackManager(this, TranslationFeedbackManager.ClickMode.None);
			DrawMode = TreeViewDrawMode.OwnerDrawText;
			SelectedNodeBackBrushWhenFocused = SystemBrushes.Highlight;
#if !WINZOR
			SelectedNodeBackBrushWhenUnfocused = BrushProvider.FromColor(Color.LightGray);
#endif

			Hotkeys.RegisterHotKey(Keys.Control | Keys.F, ShowFindForm, Res.GetString("a8cf6f35-30a3-4aee-844f-053a15446e42", "Show find form"));
		}

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				if (fReadOnly != value)
				{
					fReadOnly = value;
					UpdateControlDetails();
				}
			}
		}
		bool fReadOnly;

		public ITreeViewSearcher TreeViewSearcher { get; set; }

		public Brush SelectedNodeBackBrushWhenFocused { get; set; }

		public Brush SelectedNodeBackBrushWhenUnfocused { get; set; }

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				UnhookNodeEvents();

				translationFeedbackManager.Dispose();

				nodeNotifications.Clear();
				BroadcastNotificationChange();

				CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		void UnhookNodeEvents()
		{
			foreach (TreeNode node in Nodes)
			{
				UnhookNodeEvents(node);
			}
		}

		void UnhookNodeEvents(TreeNode node)
		{
			var bizONode = node as ZBusinessObjectTreeNode;
			if (bizONode != null)
			{
				bizONode.UnhookEvents();
			}

			foreach (TreeNode childNode in node.Nodes)
			{
				UnhookNodeEvents(childNode);
			}
		}

		#region Hotkeys

		public HotkeyRegister Hotkeys { get; } = new HotkeyRegister();
		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("52d1a40c-d817-46bd-bf0e-f17a1ab96366", "Tree");

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return Hotkeys.ProcessCmdKey(this, keyData) || base.ProcessCmdKey(ref msg, keyData);
		}

		TreeViewFindForm currentFindForm;

		public void ShowFindForm()
		{
			if (!FindFormIsOpen)
			{
				currentFindForm = new TreeViewFindForm(this);
				currentFindForm.Owner = FindForm();
				currentFindForm.Show();
			}
			else
			{
				currentFindForm.BringToFront();
				currentFindForm.Focus();
			}
		}

		public virtual void LoadRegistryNodes()
		{
		}

		[DefaultValue(false)]
		public bool FindFormIsOpen
		{
			get { return currentFindForm != null && !currentFindForm.IsDisposed; }
		}

#if DEBUG
		internal TreeViewFindForm CurrentFindForm { get { return currentFindForm; } }
#endif

		#endregion

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZTreeView>()
				.Property("ReadOnly", true, false)
				.Result;
		}

		#endregion

		#region Hide the non-functional horizontal scroll bar on show (MS bug)

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				var oldWidth = Width;
				ControlDpiScalingHelper.SetWidth(this, 0, true);
				ControlDpiScalingHelper.SetWidth(this, oldWidth, false);
			}
		}

		#endregion

		#region Notifications

		public IDisposable SuspendNotificationsChanged() => new SemaphoreManager(UpdateImageIndexSemaphore);
		internal bool IsNotificationsChangedSuspended => updateImageIndexSemaphore?.IsSuspended ?? false;

		Semaphore UpdateImageIndexSemaphore => updateImageIndexSemaphore ?? (updateImageIndexSemaphore = new Semaphore());
		Semaphore updateImageIndexSemaphore;

		public void UpdateNotifications()
		{
			nodeNotifications.Clear();
			suspendBroadcastNotificationChange = true;
			RefreshNodesNotification(Nodes);
			suspendBroadcastNotificationChange = false;
			BroadcastNotificationChange();
		}

		internal void UpdateNotifications(TreeNode node, INotificationType notificationType)
		{
			var nodeReference = nodeNotifications.Keys.FirstOrDefault(reference => reference.Target == node) ?? new WeakReference(node);
			nodeNotifications[nodeReference] = notificationType;

			BroadcastNotificationChange();
		}

		void RefreshNodesNotification(TreeNodeCollection nodes)
		{
			foreach (ZBusinessObjectTreeNode node in nodes)
			{
				node.UpdateImageIndex(true);
				RefreshNodesNotification(node.Nodes);
			}
		}

		void BroadcastNotificationChange()
		{
			if (!suspendBroadcastNotificationChange)
			{
				INotificationType highestSeverityNotification = null;
				foreach (var nodeNotification in nodeNotifications)
				{
					TreeNode key;
					if ((key = nodeNotification.Key.Target as TreeNode) != null && nodeNotification.Value != null && key.TreeView == this &&
						(highestSeverityNotification == null || nodeNotification.Value.Severity < highestSeverityNotification.Severity))
					{
						highestSeverityNotification = nodeNotification.Value;
					}
				}
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(highestSeverityNotification, this);
			}
		}

		readonly Dictionary<WeakReference, INotificationType> nodeNotifications = new Dictionary<WeakReference, INotificationType>();
		bool suspendBroadcastNotificationChange;

		#endregion

		#region Implementation

		void UpdateControlDetails()
		{
			if (ReadOnly)
			{
				BackColor = SystemColors.Control;
			}
			else
			{
				BackColor = SystemColors.Window;
			}
		}

#if !WINZOR

		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			base.OnDrawNode(e);

			if (e.Node == null || e.Bounds.Height == 0)
			{ // doing nothing because there is no regestry items to be drawn 
			}
			else if (UseDrawNodeFontOverride)
			{
				var isSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
				var isFocused = e.Node.TreeView.Focused;

				var font = e.Node.NodeFont ?? e.Node.TreeView.Font;
				var size = TextRenderer.MeasureText(e.Node.Text, font, e.Bounds.Size, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
				var textColor = isSelected ? SystemColors.HighlightText : e.Node.ForeColor;

				var backColor =
							!isSelected
									? BrushProvider.FromColor(e.Node.BackColor)
									: isFocused
											? SelectedNodeBackBrushWhenFocused
											: SelectedNodeBackBrushWhenUnfocused;

				e.Graphics.FillRectangle(backColor, e.Bounds.X, e.Bounds.Y, size.Width, e.Bounds.Height);
				TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds.Location, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
			}
		}

#endif

		protected virtual bool UseDrawNodeFontOverride
		{
			get { return true; }
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.Button == MouseButtons.Left)
			{
				if (HitNodeToDrag != null)
				{
					DoDragDrop(HitNodeToDrag);
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			HitNodeToDrag = GetNodeAt(e.X, e.Y) as ZBusinessObjectTreeNode;
			if (Nodes.Count > 0 && HitNodeToDrag == Nodes[0])
			{
				HitNodeToDrag = null;
			}
			base.OnMouseDown(e);

#if WINZOR
			if (HitNodeToDrag != null)
			{
				var objectsToMove = new List<BusinessObject>
				{
					HitNodeToDrag.BizO
				};
				var aDragDropEffects = DoDragDrop(objectsToMove, DragDropEffects.Move);
			}
#endif
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			HitNodeToDrag = null;

			if (TranslationFeedbackManager.InTranslationFeedbackMode())
			{
				if (translationFeedbackManager.InMode)
				{
					translationFeedbackManager.HandleClick();
				}
				cancelSelect = true;
			}
		}

		bool DoDragDrop(ZBusinessObjectTreeNode node)
		{
			var aDragDropEffects = DragDropEffects.None;
			try
			{
				var objectsToMove = new ArrayList();
				objectsToMove.Add(node.BizO);
				BeforeDragDrop();
				aDragDropEffects = DoDragDrop(objectsToMove, DragDropEffects.Move);
				AfterDragDrop();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
			}

			return (aDragDropEffects == DragDropEffects.Move);
		}

		protected virtual void BeforeDragDrop()
		{
		}

		protected virtual void AfterDragDrop()
		{
		}

		ZBusinessObjectTreeNode HitNodeToDrag;

#endregion

		#region IDynamicToolTip members

		public event QueryToolTipEventHandler QueryToolTip;
		void IDynamicToolTip.GetToolTip(ToolTipInfo info)
		{
			if (QueryToolTip != null)
			{
				QueryToolTip(this, info);
			}
		}

		#endregion

		#region ICaptionedComponents

		public object GetCaptionedComponentAt(Point p)
		{
			return GetNodeAt(p);
		}

		public Rectangle GetCaptionedComponentRect(object component)
		{
			return ((TreeNode)component).Bounds;
		}

		public object GetCaptionedComponentData(object component)
		{
			return component;
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			if (cancelSelect)
			{
				e.Cancel = true;
				cancelSelect = false;
			}
			else
			{
				base.OnBeforeSelect(e);
			}
		}

		bool cancelSelect;

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WindowsMessage.WM_COPY)
			{
				SetTextToClipboard(SelectedNode?.Text);
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		void SetTextToClipboard(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				SafeClipboard.SetText(text);
			}
		}

#endif

		#endregion

		readonly internal TranslationFeedbackManager translationFeedbackManager;
	}

	public class ZBusinessObjectTreeNode : TreeNode
	{
		public ZBusinessObjectTreeNode(BusinessObject bizO, string initialText, int imageIndex, int errorIndex, int messageErrorIndex, int warningIndex)
			: base(initialText, imageIndex, imageIndex)
		{
			this.BizO = bizO;
			this.normalImageIndex = imageIndex;
			this.errorImageIndex = errorIndex;
			this.messageErrorImageIndex = messageErrorIndex;
			this.warningImageIndex = warningIndex;
			Bind();
		}

		public ZBusinessObjectTreeNode(BusinessObject bizO, string initialText)
			: this(bizO, initialText, defaultImageIndex, defaultErrorIndex, defaultMessageErrorIndex, defaultWarningText)
		{
		}

		protected virtual void Bind()
		{
			if (BizO != null)
			{
				BizO.NotificationsChanged += UpdateImageIndex;
			}
		}

		public ZBusinessObjectTreeNode FindNodeForObject(BusinessObject businessObject)
		{
			ZBusinessObjectTreeNode result = null;
			if (businessObject == BizO)
			{
				result = this;
			}
			else
			{
				foreach (ZBusinessObjectTreeNode aNode in Nodes)
				{
					result = aNode.FindNodeForObject(businessObject);
					if (result != null)
					{
						break;
					}
				}
			}
			return result;
		}

		#region Image Indexes

		const int defaultImageIndex = 0;
		const int defaultErrorIndex = 1;
		const int defaultMessageErrorIndex = 2;
		const int defaultWarningText = 3;

		public int NormalImageIndex
		{
			get { return normalImageIndex; }
			set
			{
				if (ImageIndex == normalImageIndex)
				{
					ImageIndex = value;
				}
				normalImageIndex = value;
			}
		}
		protected int normalImageIndex;

		public int ErrorImageIndex
		{
			get { return errorImageIndex; }
			set
			{
				if (ImageIndex == errorImageIndex)
				{
					ImageIndex = value;
				}
				errorImageIndex = value;
			}
		}
		protected int errorImageIndex;

		public int MessageErrorImageIndex
		{
			get { return messageErrorImageIndex; }
			set
			{
				if (ImageIndex == messageErrorImageIndex)
				{
					ImageIndex = value;
				}
				messageErrorImageIndex = value;
			}
		}
		protected int messageErrorImageIndex;

		public int WarningImageIndex
		{
			get { return warningImageIndex; }
			set
			{
				if (ImageIndex == warningImageIndex)
				{
					ImageIndex = value;
				}
				warningImageIndex = value;
			}
		}
		protected int warningImageIndex;

		#endregion

		#region Implementation

		public readonly BusinessObject BizO;

		public void UpdateImageIndex(object sender, EventArgs e)
		{
			if (TreeView is ZTreeView tree && !tree.IsNotificationsChangedSuspended)
			{
				UpdateImageIndex(false);
			}
		}

		internal void UpdateImageIndex(bool forceNotification)
		{
			var highestSeverity = BizO.GetHighestSeverityNotificationType()?.Severity;
			if (highestSeverity == CargoWise.ComponentModel.NotificationType.Error.Severity)
			{
				SetImages(ErrorImageIndex, forceNotification);
			}
			else if (highestSeverity == CargoWise.EntityFramework.NotificationType.MessageError.Severity)
			{
				SetImages(MessageErrorImageIndex, forceNotification);
			}
			else if (highestSeverity == CargoWise.ComponentModel.NotificationType.Warning.Severity)
			{
				SetImages(WarningImageIndex, forceNotification);
			}
			else
			{
				SetImages(NormalImageIndex, forceNotification);
			}
		}

		void SetImages(int newIndex, bool forceNotification)
		{
			var imageIndexChanged = ImageIndex != newIndex || SelectedImageIndex != newIndex;

			if (imageIndexChanged)
			{
				ImageIndex = newIndex;
				SelectedImageIndex = newIndex;
			}

			if (imageIndexChanged || forceNotification)
			{
				var treeView = TreeView as ZTreeView;
				if (treeView != null && BizO != null)
				{
					treeView.UpdateNotifications(this, BizO.GetHighestSeverityNotificationType());
				}
			}
		}

		protected internal void UnhookEvents()
		{
			if (BizO != null)
			{
				BizO.NotificationsChanged -= UpdateImageIndex;
			}
		}

		#endregion
	}
}

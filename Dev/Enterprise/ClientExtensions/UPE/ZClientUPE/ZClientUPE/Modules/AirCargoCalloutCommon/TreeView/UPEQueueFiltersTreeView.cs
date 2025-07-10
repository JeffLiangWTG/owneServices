using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module
{
	[SuppressFilterStripControlBoundCheck]
	public class UPEQueueFilterStripTreeView : UPEQueueFiltersTreeView
	{
		public UPEQueueFilterStripTreeView(IQueueFilterBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected override bool IsAutoExpanded
		{
			get { return false; }
		}
	}

	[SuppressFilterStripControlBoundCheck]
	public class UPEQueueFiltersTreeView : ZTreeView
	{
		public UPEQueueFiltersTreeView()
		{
			CheckBoxes = true;
		}

		public UPEQueueFiltersTreeView(IQueueFilterBusinessObject filterBizObj)
			: this()
		{
			FilterBizObj = filterBizObj;
		}

		public IQueueFilterBusinessObject FilterBizObj
		{
			get { return fFilterBizObj; }
			set
			{
				fFilterBizObj = value;
				Nodes.Clear();

				if (fFilterBizObj != null)
				{
					QueueListTreeNode queueNamesNode = new QueueListTreeNode(this, FilterBizObj.QueueStatus, FilterBizObj.QueueNames_List);
					Nodes.Add(queueNamesNode);
					queueNamesNode.Expand();
				}
			}
		}

		IQueueFilterBusinessObject fFilterBizObj;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				foreach (TreeNode node in Nodes)
				{
					IDisposable disposable = node as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#region Expanding on Focus

		protected virtual bool IsAutoExpanded
		{
			get { return true; }
		}

#if !WINZOR
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == WindowsMessage.WM_NCMOUSELEAVE || m.Msg == WindowsMessage.WM_NCMOUSEMOVE)
			{
				UpdateExpanded();
			}
		}
#endif

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			UpdateExpanded();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			UpdateExpanded();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			UpdateExpanded();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			UpdateExpanded();
		}

		void UpdateExpanded()
		{
			if (IsAutoExpanded)
			{
				Expanded = IsMouseWithinBounds() && Focused;
			}
		}

		bool IsMouseWithinBounds()
		{
			Point mouseInClient = Parent.PointToClient(MousePosition);
			return Bounds.Contains(mouseInClient);
		}

		bool Expanded
		{
			set
			{
				if (fExpanded != value)
				{
					if (value)
					{
						ControlToChangeHeightOn.BringToFront();
						ControlDpiScalingHelper.SetHeight(ControlToChangeHeightOn, ControlToChangeHeightOn.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(300), false);
					}
					else
					{
						ControlDpiScalingHelper.SetHeight(ControlToChangeHeightOn, ControlToChangeHeightOn.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(300), false);
					}
					fExpanded = value;
				}
			}
		}
		bool fExpanded;

		Control ControlToChangeHeightOn
		{
			get
			{
				Control result = this;
				if (Dock == DockStyle.Fill)
				{
					result = Parent;
				}
				return result;
			}
		}

		protected new virtual Point MousePosition
		{
			get { return Control.MousePosition; }
		}

#endregion
	}
}

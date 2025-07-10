using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Layout
{
	internal class RowLayoutRow
	{
		public RowLayoutRow(RowLayout owner)
		{
			this.owner = owner;
		}

		public void AddControl(Control control)
		{
			control.VisibleChanged += new EventHandler(Control_VisibleChanged);
			control.ParentChanged += new EventHandler(Control_ParentChanged);
			control.Disposed += new EventHandler(Control_Disposed);
			controls.Add(control);
		}

		public void RemoveControl(Control control)
		{
			control.VisibleChanged -= new EventHandler(Control_VisibleChanged);
			control.Disposed -= new EventHandler(Control_Disposed);
			controls.Remove(control);
		}

		public ReadOnlyCollection<Control> Controls
		{
			get { return new ReadOnlyCollection<Control>(controls); }
		}

		public bool Visible
		{
			get
			{
				bool result = true;
				if (!IsDesigning)
				{
					result = false;
					foreach (Control control in Controls)
					{
						if (control.Visible)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public void EnsureHasLeftMostControl()
		{
			int leftMost = int.MaxValue;
			Control controlToMoveLeft = null;
			foreach (Control control in Controls)
			{
				if (control.Left < leftMost)
				{
					controlToMoveLeft = control;
					leftMost = control.Left;
				}
			}

			if (controlToMoveLeft != null)
			{
				ControlDpiScalingHelper.SetLeft(ref controlToMoveLeft, 0, true);
			}
		}

		#region Implementation

		readonly RowLayout owner;
		readonly List<Control> controls = new List<Control>();

		bool IsDesigning
		{
			get { return owner != null && owner.Container != null && owner.Container.Site != null && owner.Container.Site.DesignMode; }
		}

		void Control_VisibleChanged(object sender, EventArgs e)
		{
			if (owner != null && !IsDesigning && !owner.Container.IsPanelVisiblityChanging)
			{
				owner.DoLayout();
			}
		}

		void Control_ParentChanged(object sender, EventArgs e)
		{
			if (owner != null)
			{
				Control control = (Control)sender;
				if (control.Parent != owner.Container)
				{
					RemoveControl(control);
				}
			}
		}

		void Control_Disposed(object sender, EventArgs e)
		{
			RemoveControl((Control)sender);
		}

		#endregion
	}
}

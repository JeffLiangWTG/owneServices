using System;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	internal sealed class ControlAncestorChangedEvent
	{
		ControlAncestorChangedEvent(Control control)
		{
			this.Control = control;
		}

		public static ControlAncestorChangedEvent Get(Control control)
		{
			if (control == null)
			{
				throw new ArgumentNullException(nameof(control));
			}
			ControlAncestorChangedEvent result = control.GetUserData(ControlUserDataKey) as ControlAncestorChangedEvent;
			if (result == null)
			{
				result = new ControlAncestorChangedEvent(control);
				control.SetUserData(ControlUserDataKey, result);
			}
			return result;
		}
		static readonly int ControlUserDataKey = ControlExtensions.CreateUserDataKey();

		public Control Control { get; private set; }

		public event EventHandler AncestorChanged
		{
			add
			{
				if (ancestorChanged == null)
				{
					Control.ParentChanged += new EventHandler(Control_ParentChanged);
					Control.Disposed += new EventHandler(Control_Disposed);
					currentParent = Control.Parent == null || Control.Parent.IsDisposed ? null : ControlAncestorChangedEvent.Get(Control.Parent);
					if (currentParent != null)
					{
						currentParent.AncestorChanged += new EventHandler(ParentControl_AncestorChanged);
					}
				}
				ancestorChanged += value;
			}
			remove
			{
				ancestorChanged -= value;
				if (ancestorChanged == null)
				{
					Unhook();
				}
			}
		}
		EventHandler ancestorChanged;

		void Control_Disposed(object sender, EventArgs args)
		{
			Unhook();
		}

		void Unhook()
		{
			Control.ParentChanged -= new EventHandler(Control_ParentChanged);
			Control.Disposed -= new EventHandler(Control_Disposed);
			if (currentParent != null)
			{
				currentParent.AncestorChanged -= new EventHandler(ParentControl_AncestorChanged);
				currentParent = null;
			}
			if (!Control.IsDisposed)
			{
				Control.SetUserData(ControlUserDataKey, null);
			}
		}

		void OnAncestorChanged(EventArgs e)
		{
			if (ancestorChanged != null)
			{
				ancestorChanged(this, e);
			}
		}

		#region Implementation

		ControlAncestorChangedEvent currentParent;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "control")]
		void Control_ParentChanged(object sender, EventArgs e)
		{
			Control control = sender as Control;
			if (currentParent != null)
			{
				currentParent.AncestorChanged -= ParentControl_AncestorChanged;
			}
			currentParent = control.Parent == null ? null : ControlAncestorChangedEvent.Get(control.Parent);
			if (currentParent != null)
			{
				currentParent.AncestorChanged += ParentControl_AncestorChanged;
			}
			OnAncestorChanged(e);
		}

		void ParentControl_AncestorChanged(object sender, EventArgs e)
		{
			OnAncestorChanged(e);
		}

		#endregion
	}
}

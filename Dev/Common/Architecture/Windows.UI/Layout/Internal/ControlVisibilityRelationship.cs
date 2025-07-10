using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Layout
{
	internal sealed class ControlVisibilityRelationship : IDisposable
	{
		public ControlVisibilityRelationship(Control control, IVisibilityProvider visibleDependentOn)
		{
			Argument.NotNull(control, "control");
			Argument.NotNull(visibleDependentOn, "visibilityDependentOn");

			Control = control;
			VisibleDependentOn = visibleDependentOn;

			Control.VisibleChanged += Control_VisibleChanged;
			VisibleDependentOn.VisibleChanged += VisibleDependentOn_VisibleChanged;
			VisibleDependentOn_VisibleChanged(VisibleDependentOn, null);
		}

		public ControlVisibilityRelationship(Control control, IControlVisibilityProvider visibleDependentOn)
			: this(control, (IVisibilityProvider)visibleDependentOn)
		{
			if (control == visibleDependentOn.SourceControl)
			{
				throw new ArgumentException("You cannot make a control's visibility dependent on itself", nameof(visibleDependentOn));
			}
		}

		public Control Control { get; private set; }
		public IVisibilityProvider VisibleDependentOn { get; private set; }
		public void Dispose()
		{
			VisibleDependentOn.VisibleChanged -= VisibleDependentOn_VisibleChanged;
			Control.VisibleChanged -= Control_VisibleChanged;
		}

		#region Implementation

		void Control_VisibleChanged(object sender, EventArgs e)
		{
			DoRecursionProtected(ref doingControlVisibleChanged, () =>
			{
				Control.Visible = VisibleDependentOn.Visible;
			});
		}
		bool doingControlVisibleChanged;

		void VisibleDependentOn_VisibleChanged(object sender, EventArgs e)
		{
			DoRecursionProtected(ref doingVisibleDependentOnVisibleChanged, () =>
			{
				if ((Control.Site == null || !Control.Site.DesignMode) && (Control.Visible != VisibleDependentOn.Visible))
				{
					if (Control.FindForm() == null && Control.Parent != null) //suppress PerformLayout if we're not on a form yet
					{
						Control.Parent.SuspendLayout();
						Control.Visible = VisibleDependentOn.Visible;
						Control.Parent.ResumeLayout(false);
					}
					else
					{
						Control.Visible = VisibleDependentOn.Visible;
					}
				}
			});
		}
		bool doingVisibleDependentOnVisibleChanged;

		static void DoRecursionProtected(ref bool protectionFlag, Action action)
		{
			if (protectionFlag)
			{
				return;
			}

			try
			{
				protectionFlag = true;
				action.Invoke();
			}
			finally
			{
				protectionFlag = false;
			}
		}

		#endregion
	}
}

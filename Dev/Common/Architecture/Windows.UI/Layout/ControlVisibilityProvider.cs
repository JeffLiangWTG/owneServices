using System;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Windows.UI.Layout
{
	sealed class ControlVisibilityProvider : IControlVisibilityProvider
	{
		public ControlVisibilityProvider(Control sourceControl)
		{
			Argument.NotNull(sourceControl, "sourceControl");
			this.sourceControl = sourceControl;
		}

		readonly Control sourceControl;

		public bool Visible
		{
			get { return sourceControl.Visible; }
		}

		public event EventHandler VisibleChanged
		{
			add { sourceControl.VisibleChanged += value; }
			remove { sourceControl.VisibleChanged -= value; }
		}

		public Control SourceControl
		{
			get { return sourceControl; }
		}
	}
}

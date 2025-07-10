using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Microsoft.AspNetCore.Components.Rendering;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZUserControl
	{
		protected override bool SelectableByTabKey
		{
			get
			{
				if (FindFirstNonReadOnlyTabbableControl(this) != null)
				{
					return false;
				}
				return base.SelectableByTabKey && !this.GetReadOnly();
			}
		}

		Control FindFirstNonReadOnlyTabbableControl(Control container)
		{
			foreach (var control in container.Controls)
			{
				if (control.TabStop && !control.GetReadOnly())
				{
					return control;
				}
			}

			return null;
		}

		public override bool CaptureElementReference => true;
	}
}

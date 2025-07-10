using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CargoWise.Loader.Common
{
	public sealed class AutoValidatingCheckBox : CheckBox
	{
		public AutoValidatingCheckBox()
		{
		}

		protected override void OnCheckedChanged(EventArgs e)
		{
			base.OnCheckedChanged(e);
			OnValidating(new CancelEventArgs());
		}
	}
}


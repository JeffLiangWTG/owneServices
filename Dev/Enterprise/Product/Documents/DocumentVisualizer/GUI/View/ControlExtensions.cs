using System.Windows.Forms;

namespace Enterprise.DocumentVisualizer.GUI
{
	static class ControlExtensions
	{
		public static void CommitBindings(this Control control)
		{
			if (control != null)
			{
				foreach (Binding dataBinding in control.DataBindings)
				{
					dataBinding.WriteValue();
				}

				foreach (Control child in control.Controls)
				{
					CommitBindings(child);
				}
			}
		}

		public static void Remove(this Control control)
		{
			if (control != null && control.Parent != null)
			{
				control.Visible = false;
				control.Parent = null;
				control.Dispose();
			}
		}
	}
}

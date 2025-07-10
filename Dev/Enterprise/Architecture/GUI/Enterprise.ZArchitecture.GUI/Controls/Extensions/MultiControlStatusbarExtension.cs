using System.Collections.Generic;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	public class MultiControlStatusbarExtension : StatusbarExtension
	{
		public static MultiControlStatusbarExtension Create(params Control[] extraControls)
		{
			var result = new MultiControlStatusbarExtension();
			result.AddExtraControls(extraControls);
			return result;
		}

		public MultiControlStatusbarExtension() : base() { }

		internal MultiControlStatusbarExtension(Statusbar statusbar) : base(statusbar) { }

		public void AddExtraControls(IEnumerable<Control> controls)
		{
			foreach (var extraControl in controls)
			{
				if (!extraControls.Contains(extraControl))
				{
					extraControls.Add(extraControl);
					HookEvents(extraControl);
				}
			}
		}

		readonly List<Control> extraControls = new List<Control>();

		public override void Dispose()
		{
			foreach (var extraControl in extraControls)
			{
				UnhookEvents(extraControl);
			}
			extraControls.Clear();

			base.Dispose();
		}
	}
}

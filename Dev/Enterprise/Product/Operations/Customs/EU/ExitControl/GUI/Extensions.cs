using System.Windows.Forms;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public static class Extensions
	{
		public static IExitControlMainMenuSupporter GetExitControlMainMenuSupporter(this Control control)
		{
			while (control != null)
			{
				if (control is IExitControlMainMenuSupporter exitControlMainMenuSupporter)
				{
					return exitControlMainMenuSupporter;
				}
				control = control.Parent;
			}
			return null;
		}
	}
}

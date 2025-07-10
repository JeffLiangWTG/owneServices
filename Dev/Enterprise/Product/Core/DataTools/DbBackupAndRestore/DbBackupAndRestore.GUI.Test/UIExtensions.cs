using System.Windows.Forms;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTools.DbBackupAndRestore.Testing
{
	[CodeAlive("Used in DbBackupControlTest, but TestNoCode still complains")]
	public static class UIExtensions
	{
		public static T FindControlByName<T>(this Control parent, string name)
		{
			if (parent.Name == name && parent is T targetControl)
			{
				return targetControl;
			}

			foreach (Control childControl in parent.Controls)
			{
				var control = childControl.FindControlByName<T>(name);
				if (control != null)
				{
					return control;
				}
			}

			return default;
		}
	}
}

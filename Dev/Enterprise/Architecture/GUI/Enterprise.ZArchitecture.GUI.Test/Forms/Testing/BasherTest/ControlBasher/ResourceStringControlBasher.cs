using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ResourceStringControlBasher : IControlBasher
	{
		public static bool IsRequired(Control control)
		{
			return control.GetExtension<ILabelCaptionRenderer>() != null || control is IVariableLengthCaptionRenderer;
		}

		public void Bash(Control control, INotifications notifications)
		{
			MissingResourceStringChecker.Check(control, notifications);
			TruncatedRenderedCaptionChecker.Check(control, notifications);
		}
	}
}

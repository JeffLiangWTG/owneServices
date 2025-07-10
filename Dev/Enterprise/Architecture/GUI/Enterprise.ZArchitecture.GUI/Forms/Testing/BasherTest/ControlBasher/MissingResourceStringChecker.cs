using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public static class MissingResourceStringChecker
	{
		static readonly int ExcludeFromTestKey = ControlExtensions.CreateUserDataKey();

		[Conditional("DEBUG")]
		public static void ExcludeFromTest(Control control)
		{
			control.SetUserData(ExcludeFromTestKey, true);
		}

#if DEBUG

		internal static bool IsExcludedFromTest(Control control)
		{
			var data = control.GetUserData(ExcludeFromTestKey);
			return data != null && (bool)data;
		}

		internal static void Check(ZGridColumnStyle column, ZGrid grid, INotifications notifications)
		{
			if (column == null || !CaptionRenderingSupport.IsCaptionRenderingEnabled(grid))
			{
				return;
			}

			if (string.IsNullOrEmpty(column.HeaderText) ||
				column.HeaderTextWasDefaulted && string.IsNullOrEmpty(column.ResourceHeader.GetHeaderText(null, true)))
			{
				notifications.AddError("Column " + column.MappingName + " in " + ControlDescription.GetControlPath(grid) + " - is missing resource string");
			}
		}

		internal static void Check(Control control, INotifications notifications)
		{
			if (control == null || !CaptionRenderingSupport.IsCaptionRenderingEnabled(control))
			{
				return;
			}

			var renderer = control.GetExtension<ZLabelCaptionRenderer>();

			if (renderer != null && renderer.Visible)
			{
				if (string.IsNullOrEmpty(renderer.Caption))
				{
					if (control is IVariableLengthCaptionRenderer && !string.IsNullOrEmpty(control.Text))
					{
						return;
					}
					var button = control as Button;
					if (button != null && (button.Image ?? button.BackgroundImage) != null)
					{
						return;
					}

					foreach (Binding binding in renderer.DataBindings)
					{
						if (binding.PropertyName == "Caption")
						{
							return;
						}
					}

					var suppressed = TypeDescriptor.GetAttributes(control)[typeof(SuppressControlRequiresTextBasherAttribute)] != null;
					var excluded = IsExcludedFromTest(control);

					if (!suppressed && !excluded)
					{
						notifications.AddError(string.Format("Missing resource string caption for control '{0}')", ControlDescription.GetControlPath(control)));
						notifications.Add(BasherTestNotificationType.UniqueFooterMessage, "For more information, read the resource strings Wiki article at <a>https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx</a>");
						return;
					}
				}
			}

			return;
		}

#endif
	}
}

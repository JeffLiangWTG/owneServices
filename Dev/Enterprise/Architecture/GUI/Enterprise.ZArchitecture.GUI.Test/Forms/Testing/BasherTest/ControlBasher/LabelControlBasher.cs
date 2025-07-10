using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class LabelControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			CheckLabelTextNotCutOff((Label)control, notifications);
		}

		void CheckLabelTextNotCutOff(Label label, INotifications notifications)
		{
			if (!label.AutoSize)
			{
				using (var g = label.CreateGraphics())
				{
					var textSize = g.MeasureString(label.Text, label.Font, label.Size);
					if (label.Visible &&
						(label.Width < textSize.Width || label.Height < textSize.Height))
					{
						notifications.AddError(ControlDescription.GetControlPathAndLocation(label) + " - Text='" + label.Text.Replace("\n", "\\n").Replace("\r", "\\r") + "' Text Size=" + textSize.Width + "," + textSize.Height + " doesn't fit in Label Size=" + label.Width + "," + label.Height);
					}
				}
			}
		}
	}
}

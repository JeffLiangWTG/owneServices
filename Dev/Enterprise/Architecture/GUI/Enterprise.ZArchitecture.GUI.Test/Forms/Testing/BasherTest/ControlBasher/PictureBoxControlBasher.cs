using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class PictureBoxControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var pictureBox = (PictureBox)control;
			if (pictureBox.SizeMode == PictureBoxSizeMode.Normal && pictureBox.Image != null && !pictureBox.Size.Equals(pictureBox.Image.Size))
			{
				notifications.AddError(string.Format("{0} - pictureBox size ({1}) is different from image size ({2})", ControlDescription.GetControlPath(control), pictureBox.Size, pictureBox.Image.Size));
			}
		}
	}
}

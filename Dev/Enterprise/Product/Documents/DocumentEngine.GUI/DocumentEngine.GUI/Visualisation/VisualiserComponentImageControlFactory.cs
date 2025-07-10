using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	class VisualiserComponentImageControlFactory : VisualiserComponentControlFactory<VisualiserComponentImage, PictureBox>
	{
		protected override PictureBox CreateCore(VisualiserComponentImage component)
		{
			PictureBox renderedPictureBox = new PictureBoxWithErrorHandling();
			renderedPictureBox.Location = component.Location;
			renderedPictureBox.Size = component.Size;
			renderedPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
			renderedPictureBox.Image = component.Image;
			return renderedPictureBox;
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		internal class PictureBoxWithErrorHandling : PictureBox
		{
			public PictureBoxWithErrorHandling()
				: base()
			{
			}

			protected override void OnPaint(PaintEventArgs pe)
			{
				try
				{
					base.OnPaint(pe);
				}
				catch (OutOfMemoryException) //GDI+ error
				{
					Globals.Message.ShowWarning(Res.GetString("f02258aa-d150-49b5-8221-4adbbb69c651",
						"An image on the visualized report (Location ({0} X, {1} Y), Size ({2} Width, {3} Height)) was either corrupt or in a format not recognized by GDI+, and has been removed.",
						Location.X, Location.Y, Size.Width, Size.Height));
					this.Image = null;
					//replace with errored image?
				}
			}
		}
	}
}

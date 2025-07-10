using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	internal partial class WatermarkControl : RegistryBusinessObjectTemplateZUserControl
	{
		public WatermarkControl()
		{
			InitializeComponent();

			string filterName = Res.GetString("74f86e33-258b-43f6-8e46-92345569550e", "Image Files") + " ";
			ImageWatermarkImageSelectionControl.FileDialogFilter = filterName + "(*.PNG)|*.PNG";
			HelpPictureBox.Image = new WatermarkHelpPictureProvider().GetPreview();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ImageWatermarkImageSelectionControl.ReadOnly = readOnly;
		}
	}
}

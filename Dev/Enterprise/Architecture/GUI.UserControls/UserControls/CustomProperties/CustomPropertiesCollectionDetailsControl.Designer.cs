using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class CustomPropertiesCollectionDetailsControl
	{
		void InitializeComponent()
		{
			SuspendLayout();
			customPropertiesControl = GetCustomPropertiesControl();
			Controls.Add(customPropertiesControl);
			customPropertiesControl.Name = "customPropertiesControl";
			customPropertiesControl.Dock = DockStyle.Fill;
			CaptionRenderingEnabled = true;
			ResumeLayout(false);
		}

	}
}

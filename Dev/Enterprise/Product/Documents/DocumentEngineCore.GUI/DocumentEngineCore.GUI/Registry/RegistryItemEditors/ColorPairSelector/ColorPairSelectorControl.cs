using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class ColorPairSelectorControl : RegistryZUserControl
	{
		public ColorPairSelectorControl()
		{
			InitializeComponent();
		}

		internal ColorPairSelector ColorPair
		{
			get { return colorPair; }
			set
			{
				colorPair = value ?? new ColorPairSelector();
				RefreshPanelColors();
			}
		}
		ColorPairSelector colorPair;

		void PrimaryColorChangeButton_Click(object sender, EventArgs e)
		{
			Color currentColor = ColorPair.PrimaryColor;
			Color newColor = GetColorDialogResponse(currentColor);
			if (newColor != Color.Empty && newColor != currentColor)
			{
				ColorPair.PrimaryColor = newColor;
				RefreshPanelColors();
			}
		}

		void SecondaryColorChangeButton_Click(object sender, EventArgs e)
		{
			Color currentColor = ColorPair.SecondaryColor;
			Color newColor = GetColorDialogResponse(currentColor);
			if (newColor != Color.Empty && newColor != currentColor)
			{
				ColorPair.SecondaryColor = newColor;
				RefreshPanelColors();
			}
		}

		void RefreshPanelColors()
		{
			PreviewPanel.ForeColor = ColorPair.PrimaryColor;
			PreviewPanel.BackColor = ColorPair.SecondaryColor;
		}

#if DEBUG
		protected virtual
#endif
 Color GetColorDialogResponse(Color initialColor)
		{
			ColorDialog dialog = new ColorDialog();
			dialog.Color = initialColor;
			dialog.FullOpen = true;
			DialogResult result = dialog.ShowDialog();
			return result == DialogResult.OK ? dialog.Color : Color.Empty;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			PrimaryColorChangeButton.Enabled = !readOnly;
			SecondaryColorChangeButton.Enabled = !readOnly;
		}
	}
}

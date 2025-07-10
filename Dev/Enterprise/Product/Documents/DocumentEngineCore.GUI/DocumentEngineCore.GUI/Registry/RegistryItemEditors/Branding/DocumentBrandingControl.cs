using System;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class DocumentBrandingControl : ClientAndAgentBrandingControl
	{
		public DocumentBrandingControl()
		{
			InitializeComponent();
		}

		void UseGenericCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			SetUseGenericCheckBoxLabel();
		}

		void BrandEmailTextBox_TextChanged(object sender, EventArgs e)
		{
			SetUseGenericCheckBoxLabel();
		}

		void SetUseGenericCheckBoxLabel()
		{
			if (UseGenericCheckBox.Checked)
			{
				if (BrandEmailTextBox.Text != null && BrandEmailTextBox.Text.Contains("@"))
				{
					UseGenericCheckBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("ec7c5b3a-fb17-47d6-8842-c31860c52f81", "Untick this box to merge the user's email name with the above domain name. \r\n\r\nE.g. email_name") + BrandEmailTextBox.Text.Substring(BrandEmailTextBox.Text.IndexOf('@'));
				}
				else
				{
					UseGenericCheckBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("e54724ed-9f20-4f6b-9965-b38335592b32", "Untick this box to merge the user's email name with the above domain name. \r\n\r\nE.g. email_name@brand_domain.com");
				}
			}
			else
			{
				UseGenericCheckBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("42c796dc-e7e8-403e-aad5-ee9cd8fb0dc7", "Tick this box to use the above email address for all communications.");
			}
		}
	}
}

using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.MessageSending
{
	public partial class EnhancedValidationForm : ZChildForm
	{
		public EnhancedValidationForm(IEnhancedValidationEntryWrapper entryWrapper)
		{
			InitializeComponent();
			SuspendLayout();

			CommodityLine1Label.Visible = entryWrapper.CommodityLineCount >= 1;
			CommodityLine2Label.Visible = entryWrapper.CommodityLineCount >= 2;
			CommodityLine3Label.Visible = entryWrapper.CommodityLineCount >= 3;
			CommodityLine4Label.Visible = entryWrapper.CommodityLineCount >= 4;
			CommodityLine5Label.Visible = entryWrapper.CommodityLineCount >= 5;

			CommodityLine1Label.Text = entryWrapper.CommodityLine1;
			CommodityLine2Label.Text = entryWrapper.CommodityLine2;
			CommodityLine3Label.Text = entryWrapper.CommodityLine3;
			CommodityLine4Label.Text = entryWrapper.CommodityLine4;
			CommodityLine5Label.Text = entryWrapper.CommodityLine5;

			bool anyTaxLines = entryWrapper.TaxLineCount >= 1;
			TaxOverridesLabel.Visible = anyTaxLines;
			ManualTaxOverrideLabel.Visible = anyTaxLines;
			TaxLineLabel.Visible = anyTaxLines;

			TaxLine1Label.Visible = anyTaxLines;
			TaxLine2Label.Visible = entryWrapper.TaxLineCount >= 2;
			TaxLine3Label.Visible = entryWrapper.TaxLineCount >= 3;
			TaxLine4Label.Visible = entryWrapper.TaxLineCount >= 4;
			TaxLine5Label.Visible = entryWrapper.TaxLineCount >= 5;

			TaxLine1Label.Text = entryWrapper.TaxLine1;
			TaxLine2Label.Text = entryWrapper.TaxLine2;
			TaxLine3Label.Text = entryWrapper.TaxLine3;
			TaxLine4Label.Text = entryWrapper.TaxLine4;
			TaxLine5Label.Text = entryWrapper.TaxLine5;

			ManualTaxCheckBox.Visible = anyTaxLines;

			ResumeLayout();
		}

		void CheckBoxCheckedChanged(object sender, System.EventArgs e)
		{
			SubmitButton.Enabled = ClassificationCheckBox.Checked && CountryCheckBox.Checked && (ManualTaxCheckBox.Checked || !ManualTaxCheckBox.Visible);
		}
	}
}

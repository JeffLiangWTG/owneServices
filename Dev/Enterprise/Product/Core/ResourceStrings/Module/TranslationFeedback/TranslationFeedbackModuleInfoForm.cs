using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.Module
{
	[SuppressFormsLocalizedTest] // only displayed to English users
	public partial class TranslationFeedbackModuleInfoForm : ZChildForm
	{
		public TranslationFeedbackModuleInfoForm()
		{
			InitializeComponent();
		}

		void zLinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch("http://myaccount.cargowise.com/my-account/Documents/UpdateNotes/ediEnterpriseupdatenote20120405a.pdf");
		}

		void okButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

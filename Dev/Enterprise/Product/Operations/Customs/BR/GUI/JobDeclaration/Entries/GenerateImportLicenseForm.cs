using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079
using CargoWise.ComponentModel;

namespace Enterprise.Customs.BR.GUI
{
	public partial class GenerateImportLicenseForm : ZChildForm
	{
		public GenerateImportLicenseForm(GenerateImportLicenseObject declaration) : base(declaration)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		void OnOkButton_Click(object sender, EventArgs e)
		{
			var generator = DataSource as GenerateImportLicenseObject;
			generator.RunPreSaveValidation();
			if (generator.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				if (generator.GenerateImportLicense())
				{
					Globals.Message.Show(Res.GetString("4BCB88FD-EFD1-4F4C-9500-8F12472A55C7", "Generate Import License Completed!"));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("D07BF890-F1BF-4312-AA0B-4AFC3478D994", "Generate Import License Failed!"));
				}
				Close();
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

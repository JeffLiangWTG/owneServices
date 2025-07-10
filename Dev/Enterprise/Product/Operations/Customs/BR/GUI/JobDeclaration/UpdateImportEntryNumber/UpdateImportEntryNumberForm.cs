using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class UpdateImportEntryNumberForm : ZChildForm
	{
		public UpdateImportEntryNumberForm(UpdateImportEntryNumberObject declaration) : base(declaration)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		void OnOkButton_Click(object sender, EventArgs e)
		{
			var parent = DataSource as UpdateImportEntryNumberObject;

			if (parent != null)
			{
				parent.RunPreSaveValidation();
				if (parent.Notifications.HasErrors())
				{
					ShowErrorsDialog();
				}
				else
				{
					parent.UpdateEntryNumber();
					Close();
				}
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class NewReportConfigurationForm : ZChildForm
	{
		internal NewReportConfigurationForm(NewConfiguration newName)
			: base(newName)
		{
			if (newName.LinkedField != null)
			{
				LinkedFieldUserControl.SetFilter(newName.LinkedField);
			}
			else
			{
				LinkedFieldUserControl.Visible = false;
			}
			this.NewName = newName;
		}
#if DEBUG
		internal
#endif
 readonly NewConfiguration NewName;

		void zButtonOK_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}
	}
}

using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public sealed partial class ServiceInstallInfoForm : ZChildForm
	{
		public ServiceInstallInfoForm()
		{
		}

		public ServiceInstallInfoForm(ServiceInstallInfo info)
			: base(info)
		{
		}

		ServiceInstallInfo Info
		{
			get { return (ServiceInstallInfo)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (ValidateInfo())
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		#region Validation

		bool ValidateInfo()
		{
			var validationFailed = false;

			Info.RunPreSaveValidation();
			if (Info.HasErrors())
			{
				validationFailed = true;
				ShowErrorsDialog();
			}

			return !validationFailed;
		}

		#endregion
	}
}


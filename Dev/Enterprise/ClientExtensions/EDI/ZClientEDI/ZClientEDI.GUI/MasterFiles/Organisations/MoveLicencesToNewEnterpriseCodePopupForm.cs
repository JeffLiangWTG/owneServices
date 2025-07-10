using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class MoveLicencesToNewEnterpriseIDPopupForm : ZChildForm
	{
		public MoveLicencesToNewEnterpriseIDPopupForm()
		{
		}

		public MoveLicencesToNewEnterpriseIDPopupForm(MoveLicencesToNewEnterpriseIDBizO moveLicEntBizO)
			: base(moveLicEntBizO)
		{
		}

		protected new MoveLicencesToNewEnterpriseIDBizO BusinessEntity
		{
			get { return (MoveLicencesToNewEnterpriseIDBizO)base.BusinessEntity; }
		}

		void ButtonOk_Click(object sender, EventArgs e)
		{
			ButtonOk.Focus();
			LicenceEnterpriseIDFindBox.Focus();
			if (BusinessEntity.LicenceEnterpriseID != ZString.Empty)
			{
				if (ShowConfirmationDialog())
				{
					BusinessEntity.MoveLicencesToNewEnterpriseID();
					this.Close();
				}
			}
			else
			{
				Globals.Message.Show("Please enter existing enterprise ID or press Cancel button to close this window");
			}
		}

		protected virtual bool ShowConfirmationDialog()
		{
			DialogResult result = UserNotification.Instance.ShowConfirmation("You are about to move licences to new Enterprise ID. Are you sure you want to proceed?", "Move Licences Warning", "move", MessageBoxIcon.Warning);
			if (result == DialogResult.OK)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public class MoveLicencesToNewEnterpriseIDBizOCallbacks : IMoveLicencesToNewEnterpriseIDBizOCallbacks
	{
		public void NotifyFailure(string serverCode)
		{
			Globals.Message.Show("Licence Database with server code '" + serverCode + "' cannot be copied because target enterprise ID already contains Licence Database with same and all similar server codes. " + System.Environment.NewLine + "No Licences will be copied. " + System.Environment.NewLine + "Please call ediProd Administrator to resolve this problem.");
		}

		public void NotifyLicenceEnterpriseIDDoesNotExist(string id)
		{
			Globals.Message.Show("Enterprise ID '" + id + "' does not exist in the system");
		}

		public void NotifyTargetSameAsSource()
		{
			Globals.Message.Show("This enterprise ID is chosen as source to copy from - choose another enterprise ID to copy to");
		}

		public void NotifyError(string errorMessage)
		{
			Globals.Message.ShowError(errorMessage);
		}
	}
}

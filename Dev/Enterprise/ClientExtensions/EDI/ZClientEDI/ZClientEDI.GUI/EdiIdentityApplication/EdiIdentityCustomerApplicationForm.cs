using System;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IdentityApplication.GUI
{
	public partial class EdiIdentityCustomerApplicationForm : ZTemplateForm
	{
		public EdiIdentityCustomerApplicationForm(EdiIdentityApplication application)
			: base(application)
		{
			ControllerID = ClientControllerRegistration.EdiIdentityApplication;
			SetupActionMenuItemsEvent();
			SetupCertificateGridContextMenu();
			if (!Application.IDA_IsActive)
			{
				SetReadOnlyIncludingChildren();
			}
		}

		readonly ApplicationFormEventHandler formEventHandler = new ApplicationFormEventHandler();

		bool CanEditCertificate => EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed;

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		public EdiIdentityApplication Application
		{
			get { return (EdiIdentityApplication)base.BusinessEntity; }
		}

		void SetupActionMenuItemsEvent()
		{
			if (CanEditCertificate)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(Application.Certificates.Count > 0 ? formEventHandler.RenewCertificateText : formEventHandler.GenerateCertificateText, GenerateCertificate));
			}

			if (Application.IsInDatabase && !Application.IDA_IsRollback)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(formEventHandler.RollbackApplicationText, RollbackApplication));
			}
		}

		void SetupCertificateGridContextMenu()
		{
			if (CanEditCertificate)
			{
				CertificatesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(formEventHandler.RevokeCertificateText, RevokeCertificate));
			}
		}

		void GenerateCertificate(object sender, EventArgs e)
		{
			formEventHandler.GenerateCertificate(Application);
		}

		void RollbackApplication(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation(Res.GetString("A5BDF9AC-D550-43FB-A52A-778845E4B66E", "You are about to roll-back this application.\r\nAre you sure you want to proceed?"), Res.GetString("1F54B1A8-C71D-4ECA-B17D-B190BCAB59E2", "Warning"), "Yes", ZMessageBoxIcon.Warning) == ZDialogResult.OK)
			{
				Application.IDA_IsRollback = true;
			}
		}

		void RevokeCertificate(object sender, EventArgs e)
		{
			var certificates = CertificatesGrid.SelectedElements.Cast<EdiIdentityCertificate>().ToList();
			formEventHandler.RevokeCertificate(certificates, true);
		}

#if DEBUG
		public MenuItem RenewCertificateMenuItem => ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == formEventHandler.RenewCertificateText);
		public MenuItem GenerateCertificateMenuItem => ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == formEventHandler.GenerateCertificateText);
		public MenuItem RollbackApplicationMenuItem => ActionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == formEventHandler.RollbackApplicationText);
		public MenuItem RevokeCertificateMenuItem => CertificatesGrid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == formEventHandler.RevokeCertificateText);
		public ZGrid CertificatesGridForTest => CertificatesGrid;
#endif
	}
}

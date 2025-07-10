using System;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IdentityApplication.GUI
{
	public partial class EdiIdentityApplicationForm : ZTemplateForm
	{
#if DEBUG
		internal ZArchitecture.ZTextBox ApplicationNameTextboxForTest => ApplicationNameTextbox;
		internal ZArchitecture.ZTextBox TenantIdTextBoxForTest => TenantIdTextBox;
		internal ZArchitecture.ZTextBox ClientIdTextBoxForTest => ClientIdTextBox;
		internal ZDropEdit ApplicationTypeDropBoxForTest => ApplicationTypeDropBox;
		internal ZDropEdit ProductDropBoxForTest => ProductDropBox;
		internal ZCheckBox IsActiveCheckBoxForTest => IsActiveCheckBox;
		internal ZCheckBox IsRollbackCheckBoxForTest => IsRollbackCheckBox;
		internal ZArchitecture.ZGrid CertificateGridForTest => CertificatesGrid;
		internal ZArchitecture.ZGrid RedirectUrlGridForTest => RedirectUrlGrid;
		internal ZGroupBox PermissionGroupBoxForTest => PermissionGroupBox;
		internal ZArchitecture.ZGrid PermissionGridForTest => PermissionGrid;
#endif

		public EdiIdentityApplicationForm(EdiIdentityApplication application)
			: base(application)
		{
			SetupActionMenuItemsEvent();
			SetupCertificateGridContextMenu();
			SetupLayoutReadOnly();
			SetUpLicenceDatabaseTabPage();
		}

		void SetupActionMenuItemsEvent()
		{
			if (Application.IDA_LD.IsEmpty && CanEditCertificate)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(Application.Certificates.Count > 0 ? formEventHandler.RenewCertificateText : formEventHandler.GenerateCertificateText, GenerateCertificate));
			}

			if (Application.IsInDatabase && !Application.IDA_IsRollback && !Application.IDA_ClientID.IsEmpty)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(formEventHandler.RollbackApplicationText, RollbackApplicationEvent));
			}
		}

		void SetupCertificateGridContextMenu()
		{
			if (CanEditCertificate)
			{
				CertificatesGrid.ContextMenu.MenuItems.Add(new ZMenuItem(formEventHandler.RevokeCertificateText, RevokeCertificate));
			}
		}

		void SetupLayoutReadOnly()
		{
			if (!Application.IDA_IsActive)
			{
				SetReadOnlyIncludingChildren();
			}
			RedirectUrlGrid.ReadOnly = !Application.IDA_LD.IsEmpty;
		}

		void SetUpLicenceDatabaseTabPage()
		{
			if (Application.IDA_LD.IsEmpty)
			{
				LicenceDatabaseTabPage.TabVisible = false;
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

		void RollbackApplicationEvent(object sender, EventArgs e)
		{
			RollbackApplication();
		}

		internal void RollbackApplication()
		{
			if (Globals.Message.ShowConfirmation(Res.GetString("49723E59-0303-494A-828B-975D2E11DC97", "You are about to roll-back this application and remove the corresponding certificates and redirect URL in application from our Azure AD B2C server.\r\nAre you sure you want to proceed?"), Res.GetString("1F54B1A8-C71D-4ECA-B17D-B190BCAB59E2", "Warning"), "Yes", ZMessageBoxIcon.Warning) == ZDialogResult.OK)
			{
				Application.IDA_IsRollback = true;
			}
		}

		void GenerateCertificate(object sender, EventArgs e)
		{
			formEventHandler.GenerateCertificate(Application);
		}

		void RevokeCertificate(object sender, EventArgs e)
		{
			RevokeCertificate();
		}

		internal void RevokeCertificate()
		{
			var certificates = CertificatesGrid.SelectedElements.Cast<EdiIdentityCertificate>().ToList();
			formEventHandler.RevokeCertificate(certificates);
		}
	}
}

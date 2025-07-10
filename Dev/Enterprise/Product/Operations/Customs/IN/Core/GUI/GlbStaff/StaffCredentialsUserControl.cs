using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IN.GUI
{
	public partial class StaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public StaffCredentialsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ChooseCertificateButton.Click += ChooseCertificateButton_Click;
			ClearCertificateButton.Click += ClearCertificateButton_Click;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ChooseCertificateButton.Click -= ChooseCertificateButton_Click;
				ClearCertificateButton.Click -= ClearCertificateButton_Click;
			}
			base.Dispose(disposing);
		}

		void ChooseCertificateButton_Click(object sender, EventArgs e)
		{
			var certificatePassword = StaffWrapper.CertificatePassword;
			var libraryName = certificatePassword.LibraryName;
			if (libraryName.IsEmpty)
			{
				Globals.Message.ShowError(MandatoryValidation.YouHaveNotEnteredMessage(certificatePassword.GP_NameInfo.HumanReadableName));
				return;
			}

			var chosenCertificate = ChooseCertificate(libraryName);
			if (chosenCertificate is not null)
			{
				certificatePassword.GP_CertificateSerialNumber = chosenCertificate.SerialNumber;
			}
		}

		CryptokiCertificate ChooseCertificate(ZString libraryName)
		{
			var selector = new TokenCertificateSelector(libraryName);
			try
			{
				return selector.ChooseCertificate();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				return null;
			}
		}

		void ClearCertificateButton_Click(object sender, EventArgs e)
		{
			StaffWrapper.CertificatePassword.ClearDetails();
		}

		GlbStaffWrapper StaffWrapper => (GlbStaffWrapper)CurrentDataItem;
	}
}

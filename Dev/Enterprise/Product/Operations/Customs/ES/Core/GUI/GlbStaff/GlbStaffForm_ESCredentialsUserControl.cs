using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class GlbStaffForm_ESCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public GlbStaffForm_ESCredentialsUserControl()
		{
			InitializeComponent();
			SetUpESCertGrid();
		}

		void SetUpESCertGrid()
		{
			var changeCertStatusMenu = new ZMenuItem(ResString.GetMultilingualString("3594C418-F06F-4F23-89B2-4C0DA03DA4C4", "Activate/Deactivate"));
			changeCertStatusMenu.Click += ChangeCertStatus;
			ESCertGrid.ContextMenu.MenuItems.Add(changeCertStatusMenu);
		}

		void ChangeCertStatus(object sender, EventArgs ev)
		{
			var selectedElements = ESCertGrid.SelectedElements;
			if (selectedElements.Length != 1)
			{
				Globals.Message.Show(Res.GetString("B4FE63A0-7426-4E58-ADDC-2776F6E14032", "Please select a single row first"));
			}
			else
			{
				var certSelected = (GlbExternalPassword)selectedElements[0];
				var oldStatus = certSelected.GP_PasswordStatus;
				if (oldStatus != MasterFiles.Business.PasswordStatusList.Codes.Deactivated && oldStatus != MasterFiles.Business.PasswordStatusList.Codes.Valid)
				{
					Globals.Message.Show(Res.GetString("15206083-F53E-4B3B-9714-3134974106E4", "Please select active (VAL) or inactive (INA) certificate"));
				}
				else if (GetConfirmationTochangeCertStatus(oldStatus))
				{
					var factory = new BusinessObjectFactory();
					var newFactoryCert = factory.Load<GlbExternalPassword>(certSelected.PK);

					var newStatus = oldStatus == MasterFiles.Business.PasswordStatusList.Codes.Valid
															? MasterFiles.Business.PasswordStatusList.Codes.Deactivated
															: MasterFiles.Business.PasswordStatusList.Codes.Valid;
					newFactoryCert.GP_PasswordStatus = newStatus;

					try
					{
						factory.Save();
						Globals.Message.Show(Res.GetString("2FA14093-800C-4B11-836A-1BB55EF57ABB", "Status changed from {0} to {1}", oldStatus, newStatus));
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		bool GetConfirmationTochangeCertStatus(ZString status)
		{
			var message = status == MasterFiles.Business.PasswordStatusList.Codes.Valid
								? Res.GetString("FADD9C6D-3CE0-442E-9844-8C7BA6B355C5", "Are you sure you want to deactivate this certificate? If this certificate is deactivated it can no longer be eligible for any declaration.")
								: Res.GetString("9A5AFE28-189E-43DF-A471-5E992F5650BA", "Are you sure you want to activate this certificate?");
			var caption = status == MasterFiles.Business.PasswordStatusList.Codes.Valid
								? Res.GetString("ED6C85F0-3B7B-4A6C-816A-ECB05BDECF65", "Deactivate")
								: Res.GetString("2307D7AC-C8C6-400A-9240-C3B14EC96368", "Activate");
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.OK;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataChanged;
				CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataChanged;
			}
			base.Dispose(disposing);
		}

		#region Event Handlers

		void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
			(ESCertGrid.ListManager.GetCurrent() as GlbExternalPassword).GP_Certificate = loadedData;
		}

		void ESCertGrid_AfterBind(object sender, EventArgs e)
		{
			if (ESCertGrid.ListManager != null)
			{
				ESCertGrid.ListManager.CurrentChanged += ESCertGrid_CurrentChanged;
				ESCertGrid_CurrentChanged(ESCertGrid.ListManager, EventArgs.Empty);
			}
		}

		void ESCertGrid_CurrentChanged(object sender, EventArgs e)
		{
			try
			{
				CertificateLoaderUserControl.SetFileData((ESCertGrid.ListManager.GetCurrent() as GlbExternalPassword).GP_Certificate);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.SetFileData(ZBlob.Empty);
			}

			CheckCertLoaderVisibility();
		}

		void ESCertGrid_CurrentCellChanged(object sender, EventArgs e) => CheckCertLoaderVisibility();

		void ESCertGrid_Leave(object sender, EventArgs e)
		{
			try
			{
				CertificateLoaderUserControl.CertificatePassword = (ESCertGrid.ListManager.GetCurrent() as GlbExternalPassword).CurrentDecryptedCertificatePassphrase;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
			}
		}

		void CheckCertLoaderVisibility()
		{
			if (ESCertGrid.ListManager != null)
			{
				var isVisible = false;
				var hasAnyRows = ESCertGrid.ListManager.Count > 0;
				if (hasAnyRows)
				{
					var currentCell = ESCertGrid.ListManager.GetCurrent() as GlbExternalPassword;
					isVisible = (currentCell.GP_Name != ZString.Empty
						|| currentCell.GP_UserID != ZString.Empty
						|| currentCell.CurrentDecryptedCertificatePassphrase != ZString.Empty);
				}

				CertificateLoaderUserControl.Visible = isVisible;
			}
		}

		#endregion
	}
}

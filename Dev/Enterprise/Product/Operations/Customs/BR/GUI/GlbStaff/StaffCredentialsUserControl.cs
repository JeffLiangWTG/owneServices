using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class StaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public StaffCredentialsUserControl()
		{
			InitializeComponent();
			SetupNodesGridColumns();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataLoaded;
			CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataCleared;
			CertificateLoaderUserControl.OnDataView += OnCertificateDataView;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataLoaded;
				CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataCleared;
				CertificateLoaderUserControl.OnDataView -= OnCertificateDataView;
			}

			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var password = GlbStaffWrapper?.CCTPassword;
			if (password != null)
			{
				CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				CertificateLoaderUserControl.SetFileData(password.GP_Certificate);
			}
		}

		#region Event Handlers

		BRGlbStaffWrapper GlbStaffWrapper => DataSource as BRGlbStaffWrapper;

		void CertificateLoaderUserControl_DataLoaded(object sender, EventArgs e)
		{
			var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();

			var password = GlbStaffWrapper?.CCTPassword;
			if (password != null)
			{
				password.GP_Certificate = loadedData;
			}
		}

		void CertificateLoaderUserControl_DataCleared(object sender, EventArgs e)
		{
			var password = GlbStaffWrapper?.CCTPassword;
			if (password != null)
			{
				password.GP_Certificate = null;
				password.CurrentDecryptedCertificatePassphrase = ZString.Empty;
				password.GP_PasswordStatus = ZString.Empty;
			}
		}

		void OnCertificateDataView(object sender, EventArgs e)
		{
			try
			{
				var password = GlbStaffWrapper?.CCTPassword;
				if (password != null)
				{
					CertificateLoaderUserControl.CertificatePassword = password.CurrentDecryptedCertificatePassphrase;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CertificateLoaderUserControl.CertificatePassword = ZString.Empty;
			}
		}

		#endregion

		void SetupNodesGridColumns()
		{
			BRAccUserGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = GlbExternalPassword_BRS.Schema.GP_UserID,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CaptionResourceString = Res.GetData("C4FDD649-5F78-4023-8A4D-EFD7D4A9E8BE", "Event Id"),
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription
			});
			BRAccUserGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				ColumnName = GlbExternalPassword_BRS.Schema.StatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CaptionResourceString = Res.GetData("F9C82C76-B310-4032-9F18-4CC9667F5D57", "Status"),
				IsReadOnly = true,
			});
			BRAccUserGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo()
			{
				ColumnName = GlbExternalPassword_BRS.Schema.GP_MailBoxID,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CaptionResourceString = Res.GetData("38A6A18F-9279-4BB1-97FD-B991CF23CFAA", "Subscription Id"),
				IsReadOnly = true,
			});
		}
	}
}

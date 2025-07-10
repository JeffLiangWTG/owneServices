using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Certificates;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GlbStaffForm_ITCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
{
	public GlbStaffForm_ITCredentialsUserControl()
	{
		InitializeComponent();
		SetupNodesGridColumns();
	}

	GlbStaffWrapper StaffWrapper => (GlbStaffWrapper)CurrentDataItem;

	GlbExternalPassword CurrentExternalPassword => ITAccUserGrid.ListManager.GetCurrent() as GlbExternalPassword;

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		ToggleXadesCertificateStatus();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		CertificateLoaderUserControl.DataLoaded += CertificateLoaderUserControl_DataChanged;
		CertificateLoaderUserControl.DataCleared += CertificateLoaderUserControl_DataChanged;
		CertificatePasswordTextBox.Leave += CertPasswordTextBox_Leave;
		XadesCertificateChooseButton.Click += XadesCertificateChooseButton_Click;
		XadesCertificateInformationButton.Click += XadesCertificateInformationButton_Click;
		AddXadesCertificateButton.Click += AddXadesCertificateButton_Click;
		ClearXadesCertificateButton.Click += ClearXadesCertificateButton_Click;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CertificateLoaderUserControl.DataLoaded -= CertificateLoaderUserControl_DataChanged;
			CertificateLoaderUserControl.DataCleared -= CertificateLoaderUserControl_DataChanged;
			CertificatePasswordTextBox.Leave -= CertPasswordTextBox_Leave;
			XadesCertificateChooseButton.Click -= XadesCertificateChooseButton_Click;
			XadesCertificateInformationButton.Click -= XadesCertificateInformationButton_Click;
			AddXadesCertificateButton.Click -= AddXadesCertificateButton_Click;
			ClearXadesCertificateButton.Click -= ClearXadesCertificateButton_Click;
		}
		base.Dispose(disposing);
	}

	#region Event Handlers

	void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
	{
		UpdateExternalPasswordCertificateFileData();
	}

	void ITAccUserGrid_AfterBind(object sender, EventArgs e)
	{
		var gridListManager = ITAccUserGrid.ListManager;
		if (gridListManager != null)
		{
			gridListManager.CurrentChanged += ITAccUserGrid_CurrentChanged;
			ITAccUserGrid_CurrentChanged(gridListManager, EventArgs.Empty);
		}
	}

	void ITAccUserGrid_CurrentChanged(object sender, EventArgs e)
	{
		UpdateCertificateLoaderUserControlFileData();
		UpdateCertificateLoaderUserControlPassword();
		ToggleCertificateLoaderUserControlStatus();
	}

	void ITAccUserGrid_CurrentCellChanged(object sender, EventArgs e)
	{
		ToggleCertificateLoaderUserControlStatus();
	}

	void CertPasswordTextBox_Leave(object sender, EventArgs e)
	{
		UpdateCertificateLoaderUserControlPassword();
	}

	void UpdateExternalPasswordCertificateFileData()
	{
		var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
		var currentExternalPassword = CurrentExternalPassword;
		if (currentExternalPassword != null)
		{
			currentExternalPassword.GP_Certificate = loadedData;
		}
	}

	void UpdateCertificateLoaderUserControlFileData()
	{
		CertificateLoaderUserControl.SetFileData(CurrentExternalPassword?.GP_Certificate ?? ZBlob.Empty);
	}

	void UpdateCertificateLoaderUserControlPassword()
	{
		CertificateLoaderUserControl.CertificatePassword = CurrentExternalPassword?.CurrentDecryptedCertificatePassphrase ?? ZString.Empty;
	}

	void ToggleCertificateLoaderUserControlStatus()
	{
		CertificateLoaderUserControl.Enabled = CurrentExternalPassword != null;
	}

	void XadesCertificateChooseButton_Click(object sender, EventArgs e)
	{
		var cryptokiCertificate = StaffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
		var chipset = GetChipsetOrNotifyUserMustSpecifyIt(cryptokiCertificate);
		if (chipset.IsEmpty)
		{
			return;
		}

		var chosenCertificate = ChooseCertificate(chipset);
		if (chosenCertificate != null)
		{
			cryptokiCertificate?.PopulateCertificateRelatedFields(chosenCertificate);
		}
	}

	void XadesCertificateInformationButton_Click(object sender, EventArgs e)
	{
		var cryptokiCertificate = StaffWrapper.CryptokiCertificateCollection.GetCryptokiCertificate();
		var chipset = GetChipsetOrNotifyUserMustSpecifyIt(cryptokiCertificate);
		if (chipset.IsEmpty)
		{
			return;
		}

		try
		{
			GetCertificateSelector(chipset).ShowCertificateInfo(cryptokiCertificate.GP_CertificateSerialNumber);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			Globals.Message.ShowError(ex.Message);
		}
	}

	CryptokiCertificate ChooseCertificate(string chipset)
	{
		try
		{
			return GetCertificateSelector(chipset).ChooseCertificate();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			Globals.Message.ShowError(ex.Message);
			return null;
		}
	}

	ZString GetChipsetOrNotifyUserMustSpecifyIt(CryptokiExternalPassword cryptokiExternalPassword)
	{
		var chipset = cryptokiExternalPassword?.GP_Name ?? ZString.Empty;
		if (chipset.IsEmpty)
		{
			Globals.Message.ShowError(ValidationCaptions.CryptokiExternalPassword.YouMustSpecifyChipset);
		}
		return chipset;
	}

	void ClearXadesCertificateButton_Click(object sender, EventArgs e)
	{
		StaffWrapper.CryptokiCertificateCollection.RemoveAndDeleteAll();
		ToggleXadesCertificateStatus();
	}

	void AddXadesCertificateButton_Click(object sender, EventArgs e)
	{
		StaffWrapper.CryptokiCertificateCollection.AddNew();
		ToggleXadesCertificateStatus();
	}

	void ToggleXadesCertificateStatus()
	{
		var isCryptokiCertificateEntered = StaffWrapper?.CryptokiCertificateCollection?.GetCryptokiCertificate() != null;
		XadesCertificateChooseButton.Enabled = XadesCertificateInformationButton.Enabled = ClearXadesCertificateButton.Enabled = isCryptokiCertificateEntered;
		AddXadesCertificateButton.Enabled = !isCryptokiCertificateEntered;
	}

	protected virtual CertificateSelector GetCertificateSelector(string chipset)
		=> ExceptionFriendlyCertificateSelector.New(chipset);

	#endregion

	void SetupNodesGridColumns()
	{
		ITAccUserGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo()
		{
			ColumnName = GlbExternalPassword.Schema.GP_UserID,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			CaptionResourceString = Res.GetData("78CB669C-D714-46AD-BB41-2791FF20EAD8", "Node"),
			ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription
		});
		ITAccUserGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo()
		{
			ColumnName = GlbBrokerExternalPassword.Schema.AccountNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			CaptionResourceString = Res.GetData("E3FB9908-56A3-4EA7-9DB8-4C486D954E1F", "Account Number"),
			IsReadOnly = true,
		});
	}
}

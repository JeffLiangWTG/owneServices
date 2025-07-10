using System;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Registry.AccountsManagement;

public partial class AccountManagementUserControl : RegistryZUserControl
{
	public AccountManagementUserControl()
	{
		InitializeComponent();
		AddAccountsColumns();
		AddExciseNumbersColumns();
		AddAccountDetailsColumns();
	}

	void AddAccountsColumns()
	{
		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("5C9EF8FD-DA6A-46A9-B46E-9CD19388F3DB", "Account Number"),
			ColumnName = "AccountNumber",
			MaxLengthOverride = 15,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("6012F547-DDBD-499C-A484-1C61E111D868", "Password"),
			ColumnName = "AccountPassword",
			MaxLengthOverride = 15,
			PasswordChar = '*',
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
		});

		AccountsGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("3CDA0BA4-F102-43BD-AA1C-163A65707250", "Password Expiration Date"),
			ColumnName = "AccountPasswordExpirationDate",
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
		});

		AccountsGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("07614B4D-A9D5-48BD-A69B-1458A21CA275", "Status"),
			ColumnName = "AccountStatus",
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("EF69F067-8037-4092-9B65-08046A3127C4", "Status Desc."),
			ColumnName = "AccountStatusDisplay",
			IsReadOnly = true,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("BCBE6A67-1640-4605-93AA-60F61BFA7553", "Certificate"),
			ColumnName = "AccountCertificateState",
			IsReadOnly = true,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("5CBE84BE-7856-4250-8E8D-5FB8212A60D0", "Certificate Password"),
			ColumnName = "AccountCertificatePassword",
			PasswordChar = '*',
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
		});

		AccountsGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("C18A48B3-9829-42BC-9035-33D29D21B0C3", "Certificate Expiration Date"),
			ColumnName = "AccountCertificateExpirationDate",
			DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("31BA106C-94F0-4D3F-B915-089D00AC3653", "Certificate Status"),
			ColumnName = "AccountCertificateStatusDisplay",
			IsReadOnly = true,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("D662C23C-5D8F-47CF-9392-67852858368D", "Node"),
			ColumnName = "AccountNode",
			MaxLengthOverride = 4,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("CA6220DD-71F6-42A7-8594-108AFB7F985D", "Range Start"),
			ColumnName = "AccountRangeStart",
			TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
			MaxLengthOverride = 2,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
		});

		AccountsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("00BE96B6-5863-48E0-A7F3-C3E8C10E89F5", "Range End"),
			ColumnName = "AccountRangeEnd",
			TextAlign = System.Windows.Forms.HorizontalAlignment.Right,
			MaxLengthOverride = 2,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70)
		});
	}

	void AddExciseNumbersColumns()
	{
		ExciseNumbersGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Res.GetData("BD363331-D341-48FB-89CC-74CE31D0CA99", "Excise Number"),
			ColumnName = ExciseNumber.Schema.Number,
			MaxLengthOverride = ExciseNumber.Schema.NumberMaxLength,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
		});
	}

	void AddAccountDetailsColumns()
	{
		AccountDetailsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = AccountDetail.Schema.InternalCode,
			MaxLengthOverride = AccountDetail.Schema.InternalCodeMaxLength,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
		});

		AccountDetailsGrid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo
		{
			ColumnName = AccountDetail.Schema.DeclarantCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
		});

		AccountDetailsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = AccountDetail.Schema.AuthorizedUser,
			MaxLengthOverride = AccountDetail.Schema.AuthorizedUserMaxLength,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
		});
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
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
	{
		base.SetControlOrBusinessEntityReadOnly(readOnly);

		AccountsGrid.ReadOnly = readOnly;
	}

	void CertificateLoaderUserControl_DataChanged(object sender, EventArgs e)
	{
		var loadedData = (ZBlob)CertificateLoaderUserControl.FileDataAsBinary();
		var account = CurrentAccount;
		if (account != null)
		{
			account.AccountCertificate = loadedData;
		}
	}

	void AccountsGrid_AfterBind(object sender, EventArgs e)
	{
		if (AccountsGrid.ListManager != null)
		{
			AccountsGrid.ListManager.CurrentChanged += AccountsGrid_CurrentChanged;
			AccountsGrid_CurrentChanged(AccountsGrid.ListManager, EventArgs.Empty);
			AccountsGrid_CurrentCellChanged(AccountsGrid.ListManager, EventArgs.Empty);
		}
	}

	void AccountsGrid_CurrentChanged(object sender, EventArgs e)
	{
		CertificateLoaderUserControl.SetFileData(CurrentAccount?.AccountCertificate ?? ZBlob.Empty);
		CheckCertLoaderVisibility();
	}

	void AccountsGrid_CurrentCellChanged(object sender, EventArgs e)
	{
		UpdateCertificatePassword();
		CheckCertLoaderVisibility();
	}

	void AccountsGrid_Leave(object sender, EventArgs e)
	{
		UpdateCertificatePassword();
	}

	void UpdateCertificatePassword() => CertificateLoaderUserControl.CertificatePassword = CurrentAccount?.AccountCertificatePassword ?? ZString.Empty;

	void CheckCertLoaderVisibility()
	{
		if (AccountsGrid.ListManager != null)
		{
			var isVisible = false;
			var hasAnyRows = AccountsGrid.ListManager.Count > 0;
			if (hasAnyRows)
			{
				var currentCell = CurrentAccount;
				isVisible = (currentCell.AccountNumber != ZString.Empty
					|| currentCell.AccountPassword != ZString.Empty
					|| currentCell.AccountCertificate != ZBlob.Empty
					|| currentCell.AccountCertificatePassword != ZString.Empty);
			}

			CertificateLoaderUserControl.Visible = isVisible;
		}
	}

	Account CurrentAccount => AccountsGrid.ListManager.GetCurrent() as Account;
}

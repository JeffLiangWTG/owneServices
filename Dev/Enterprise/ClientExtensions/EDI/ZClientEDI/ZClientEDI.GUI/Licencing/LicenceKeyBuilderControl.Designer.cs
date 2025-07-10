


using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class LicenceKeyBuilderControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		ZCheckBox ReciprocalCheckBox;
		Enterprise.ZArchitecture.GUI.ZTabPage Licence3rdPartySoftware;
		Enterprise.ZArchitecture.GUI.ZPanel ActionButtonsPanel;
		Enterprise.ZArchitecture.GUI.ZPanel LicenceKeyActionPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox ExchangeRateGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox GSTGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox GSTRegisteredCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox GSTCashBasisCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox WithholdingRegisteredCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox WithholdingCashBasisCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox LicenceInfoGroupBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CurrencyCodeFindBox;
		Enterprise.ZArchitecture.ZTextBox CompanyCodeTextBox;
		Enterprise.ZArchitecture.ZLabel ExchangeRateTextLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton SendButton;
		Enterprise.ZArchitecture.GUI.ZTabPage LicencesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ConnectionsTabPage;
		Enterprise.ZArchitecture.GUI.ZRichTextBox ConnectionsRichTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox LicenceEnterpriseCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZButton ConnectButton;
		Enterprise.ZArchitecture.GUI.ZButton GenerateReopenPeriodKeyButton;

		ZPanel KeyBuilderPanel;
		ZTabPage InstallationTabPage;

		System.Windows.Forms.ContextMenu GenerateLicenceKeyMenu;
		Enterprise.ZArchitecture.GUI.ZMenuItem menuItem1;
		Enterprise.ZArchitecture.GUI.ZMenuItem menuItem2;
		protected Enterprise.ZArchitecture.GUI.ZButton AutoDeployLicenceButton;
		protected Enterprise.ZArchitecture.GUI.ZButton GenerateLicenceKeyButton;
		protected Enterprise.ZArchitecture.GUI.ZButton EmailCompanyButton;
		protected ZTabControl LicenceTabControl;
		protected ZTabPage TrainingSchedulesTabPage;
		private ZPanel TrainingSchedulesPanel;
		private ZTabPage SupportAndContractTabPage;
		private ZPanel ConnectionsPanel;
		private CargoWise.Windows.UI.KSplitter ConnectionsSplitter;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo35 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo36 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo38 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo39 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo40 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo41 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo42 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo43 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo44 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo45 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo46 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo47 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo48 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo49 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo50 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo51 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo52 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo53 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo54 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo55 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenceKeyBuilderControl));
			this.ExchangeRateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReciprocalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExchangeRateTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GSTGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WithholdingCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WithholdingRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSTCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSTRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LicenceEnterpriseCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LicenceInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LicenceEnterpriseIDFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CompanyCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LicenceTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.CompaniesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.editionCommentBox = new Enterprise.ZArchitecture.ZLabel();
			this.EditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productBox = new Enterprise.ZArchitecture.ZLabel();
			this.productLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ClientCompaniesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CompanyDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClientReciprocalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompanyCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientWebAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientWhtCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClientWhtRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClientGstCashBasisCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClientGstRegisteredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyRegNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyAddress1Box = new Enterprise.ZArchitecture.ZTextBox();
			this.LicencesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.licenceModulesControl = new Enterprise.Client.EDI.LicenceKeyBuilder.GUI.LicenceModulesControl();
			this.Licence3rdPartySoftware = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.Licence3rdPartyGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InstallationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.installationUserControl = new Enterprise.Client.EDI.LicenceKeyBuilder.GUI.InstallationControl();
			this.ProjectsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProjectsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TrainingSchedulesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TrainingSchedulesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TrainingCourseGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ConnectionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConnectionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConnectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConnectionsRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.ConnectionsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.ConnectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupportAndContractTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.supportContractUserControl = new Enterprise.Client.EDI.LicenceKeyBuilder.GUI.SupportContractControl();
			this.BillingInvoiceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillingInvoicingControl = new Enterprise.Client.EDI.Billing.GUI.BillingInvoicingControl();
			this.BillingPricesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillingPricesControl = new Enterprise.Client.EDI.Billing.GUI.BillingPricesControl();
			this.stlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.stlLicenceControl = new Enterprise.Client.EDI.Billing.GUI.StlLicenceControl();
			this.MaintenanceTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.licenceMaintenanceControl = new Enterprise.Client.EDI.Billing.GUI.LicenceMaintenanceControl();
			this.DiscountsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillingDiscountsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FeesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PremiumServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.clientPremiumServicesControl1 = new Enterprise.Client.EDI.Billing.GUI.ClientPremiumServicesControl();
			this.LicenceActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GenerateLoginTokenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenerateDiagnosticsLoginTokenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenerateLoginTokenWithOldEntCodeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowInactiveDatabasesCheckBox = new AlwaysActiveCheckBox();
			this.DatabasesModuleButtonGrid = new Enterprise.Client.EDI.LicenceKeyBuilder.GUI.ModuleButtonGridForLicencing();
			this.ActionButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.GenerateReopenPeriodKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LicenceKeyActionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.EmailCompanyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenerateLicenceKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AutoDeployLicenceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenerateLicenceKeyMenu = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.menuItem2 = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.KeyBuilderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DatabaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DbSplitter = new CargoWise.Windows.UI.KSplitContainer();
			this.InactiveCountBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InactiveCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExchangeRateGroupBox.SuspendLayout();
			this.GSTGroupBox.SuspendLayout();
			this.LicenceEnterpriseCodeFindBox.SuspendLayout();
			this.LicenceInfoGroupBox.SuspendLayout();
			this.LicenceEnterpriseIDFindBox.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.LicenceTabControl.SuspendLayout();
			this.CompaniesTabPage.SuspendLayout();
			this.EditionDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientCompaniesGrid)).BeginInit();
			this.ClientCompaniesGrid.SuspendLayout();
			this.CompanyDetailsGroupBox.SuspendLayout();
			this.LicencesTabPage.SuspendLayout();
			this.licenceModulesControl.SuspendLayout();
			this.Licence3rdPartySoftware.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Licence3rdPartyGrid)).BeginInit();
			this.Licence3rdPartyGrid.SuspendLayout();
			this.InstallationTabPage.SuspendLayout();
			this.installationUserControl.SuspendLayout();
			this.ProjectsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProjectsGrid)).BeginInit();
			this.ProjectsGrid.SuspendLayout();
			this.TrainingSchedulesTabPage.SuspendLayout();
			this.TrainingSchedulesPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TrainingCourseGrid)).BeginInit();
			this.TrainingCourseGrid.SuspendLayout();
			this.ConnectionsTabPage.SuspendLayout();
			this.ConnectionsPanel.SuspendLayout();
			this.ConnectionsRichTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConnectionsGrid)).BeginInit();
			this.ConnectionsGrid.SuspendLayout();
			this.SupportAndContractTabPage.SuspendLayout();
			this.supportContractUserControl.SuspendLayout();
			this.BillingInvoiceTabPage.SuspendLayout();
			this.BillingInvoicingControl.SuspendLayout();
			this.BillingPricesTabPage.SuspendLayout();
			this.BillingPricesControl.SuspendLayout();
			this.stlTabPage.SuspendLayout();
			this.stlLicenceControl.SuspendLayout();
			this.MaintenanceTabPage.SuspendLayout();
			this.licenceMaintenanceControl.SuspendLayout();
			this.DiscountsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillingDiscountsGrid)).BeginInit();
			this.BillingDiscountsGrid.SuspendLayout();
			this.FeesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).BeginInit();
			this.FeesGrid.SuspendLayout();
			this.PremiumServicesTabPage.SuspendLayout();
			this.clientPremiumServicesControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesModuleButtonGrid.InnerGrid)).BeginInit();
			this.DatabasesModuleButtonGrid.SuspendLayout();
			this.ActionButtonsPanel.SuspendLayout();
			this.LicenceKeyActionPanel.SuspendLayout();
			this.KeyBuilderPanel.SuspendLayout();
			this.DatabaseGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DbSplitter)).BeginInit();
			this.DbSplitter.Panel1.SuspendLayout();
			this.DbSplitter.Panel2.SuspendLayout();
			this.DbSplitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader);
			// 
			// ExchangeRateGroupBox
			// 
			this.ExchangeRateGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|5e646093-2363-4ca9-bb80-4b0f9e2961a2", "Exchange Rates");
			this.ExchangeRateGroupBox.Controls.Add(this.ReciprocalCheckBox);
			this.ExchangeRateGroupBox.Controls.Add(this.ExchangeRateTextLabel);
			this.ExchangeRateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 0, true);
			this.ExchangeRateGroupBox.Name = "ExchangeRateGroupBox";
			this.ExchangeRateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 64, true);
			this.ExchangeRateGroupBox.TabIndex = 1;
			this.ExchangeRateGroupBox.TabStop = false;
			// 
			// ReciprocalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReciprocalCheckBox, "LicCompany+LC_IsReciprocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_IsReciprocal)));
			this.ReciprocalCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|7d289ae6-3f6b-4043-9e52-407467210e01", "Are Exchange Rates Reciprocal");
			this.ReciprocalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReciprocalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.ReciprocalCheckBox.Name = "ReciprocalCheckBox";
			this.ReciprocalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 20, true);
			this.ReciprocalCheckBox.TabIndex = 4;
			this.ReciprocalCheckBox.UseVisualStyleBackColor = false;
			// 
			// ExchangeRateTextLabel
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateTextLabel, "LicCompany+ExchangeRateText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ExchangeRateText)));
			this.ExchangeRateTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ExchangeRateTextLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ExchangeRateTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.ExchangeRateTextLabel.Name = "ExchangeRateTextLabel";
			this.ExchangeRateTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.ExchangeRateTextLabel.TabIndex = 1;
			// 
			// GSTGroupBox
			// 
			this.GSTGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|195335cc-924b-4cbc-8b7b-239bd2284d82", "Tax");
			this.GSTGroupBox.Controls.Add(this.WithholdingCashBasisCheckBox);
			this.GSTGroupBox.Controls.Add(this.WithholdingRegisteredCheckBox);
			this.GSTGroupBox.Controls.Add(this.GSTCashBasisCheckBox);
			this.GSTGroupBox.Controls.Add(this.GSTRegisteredCheckBox);
			this.GSTGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(621, 0, true);
			this.GSTGroupBox.Name = "GSTGroupBox";
			this.GSTGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 64, true);
			this.GSTGroupBox.TabIndex = 2;
			this.GSTGroupBox.TabStop = false;
			// 
			// WithholdingCashBasisCheckBox
			// 
			this.WithholdingCashBasisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WithholdingCashBasisCheckBox, "LicCompany+LC_IsWHTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_IsWHTCashBasis)));
			this.WithholdingCashBasisCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|265dffc5-cb04-4ffb-93a2-0d3536056cf3", "Withholding Cash Basis");
			this.WithholdingCashBasisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WithholdingCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 43, true);
			this.WithholdingCashBasisCheckBox.Name = "WithholdingCashBasisCheckBox";
			this.WithholdingCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WithholdingCashBasisCheckBox.TabIndex = 3;
			// 
			// WithholdingRegisteredCheckBox
			// 
			this.WithholdingRegisteredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WithholdingRegisteredCheckBox, "LicCompany+LC_IsWHTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_IsWHTRegistered)));
			this.WithholdingRegisteredCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|7db4055a-8f3b-4844-94be-26953e50be46", "Withholding Registered");
			this.WithholdingRegisteredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WithholdingRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 19, true);
			this.WithholdingRegisteredCheckBox.Name = "WithholdingRegisteredCheckBox";
			this.WithholdingRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.WithholdingRegisteredCheckBox.TabIndex = 2;
			// 
			// GSTCashBasisCheckBox
			// 
			this.GSTCashBasisCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GSTCashBasisCheckBox, "LicCompany+LC_IsGSTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_IsGSTCashBasis)));
			this.GSTCashBasisCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("49c8d570-2aba-4144-a413-ae1d7fe6bfa2", "GST Cash Enabled", "When NOT TICKED all GST/VAT is posted and reported on an Accruals (Invoice) Basis.\r\nWhen TICKED, additional Cash Basis (Payment ) GST/VAT posting and reporting features will be enabled for the Licensed Company.");
			this.GSTCashBasisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSTCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 43, true);
			this.GSTCashBasisCheckBox.Name = "GSTCashBasisCheckBox";
			this.GSTCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.GSTCashBasisCheckBox.TabIndex = 1;
			// 
			// GSTRegisteredCheckBox
			// 
			this.GSTRegisteredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GSTRegisteredCheckBox, "LicCompany+LC_IsGSTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_IsGSTRegistered)));
			this.GSTRegisteredCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|ae9c3099-6410-4b5a-9ea2-dbee0f10476e", "GST Registered");
			this.GSTRegisteredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSTRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			this.GSTRegisteredCheckBox.Name = "GSTRegisteredCheckBox";
			this.GSTRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.GSTRegisteredCheckBox.TabIndex = 0;
			// 
			// LicenceEnterpriseCodeFindBox
			// 
			this.LicenceEnterpriseCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceEnterpriseCodeFindBox, "LicenceEnterpriseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicenceEnterpriseCode)));
			this.LicenceEnterpriseCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|a8e41f3c-03c9-4eb6-94be-2eefc1a26793", "Ent. Code");
			this.LicenceEnterpriseCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 16, true);
			this.LicenceEnterpriseCodeFindBox.Name = "LicenceEnterpriseCodeFindBox";
			this.LicenceEnterpriseCodeFindBox.PreBoundMaxLength = 3;
			this.LicenceEnterpriseCodeFindBox.ShouldResize = true;
			this.LicenceEnterpriseCodeFindBox.ShowDescriptionBox = false;
			this.LicenceEnterpriseCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.LicenceEnterpriseCodeFindBox.TabIndex = 1;
			// 
			// LicenceInfoGroupBox
			// 
			this.LicenceInfoGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|ae4a640c-15a7-4d06-b43c-5282c30a84d5", "License Info");
			this.LicenceInfoGroupBox.Controls.Add(this.LicenceEnterpriseIDFindBox);
			this.LicenceInfoGroupBox.Controls.Add(this.CompanyCodeTextBox);
			this.LicenceInfoGroupBox.Controls.Add(this.CurrencyCodeFindBox);
			this.LicenceInfoGroupBox.Controls.Add(this.LicenceEnterpriseCodeFindBox);
			this.LicenceInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicenceInfoGroupBox.Name = "LicenceInfoGroupBox";
			this.LicenceInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 64, true);
			this.LicenceInfoGroupBox.TabIndex = 0;
			this.LicenceInfoGroupBox.TabStop = false;
			// 
			// LicenceEnterpriseIDFindBox
			// 
			this.LicenceEnterpriseIDFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceEnterpriseIDFindBox, "LicenceEnterpriseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicenceEnterpriseID)));
			this.LicenceEnterpriseIDFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1f8d43ea-fe25-4b1b-8609-78bf4b0619ea", "Ent. ID");
			this.LicenceEnterpriseIDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 16, true);
			this.LicenceEnterpriseIDFindBox.Name = "LicenceEnterpriseIDFindBox";
			this.LicenceEnterpriseIDFindBox.PreBoundMaxLength = 6;
			this.LicenceEnterpriseIDFindBox.ShouldResize = false;
			this.LicenceEnterpriseIDFindBox.ShowDescriptionBox = false;
			this.LicenceEnterpriseIDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.LicenceEnterpriseIDFindBox.TabIndex = 0;
			// 
			// CompanyCodeTextBox
			// 
			this.CompanyCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CompanyCodeTextBox, "LicCompany+LC_CompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_CompanyCode)));
			this.CompanyCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a02c1eee-2edc-474e-aba5-76fe1e1f4998", "Company Code");
			this.CompanyCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 16, true);
			this.CompanyCodeTextBox.Name = "CompanyCodeTextBox";
			this.CompanyCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.CompanyCodeTextBox.TabIndex = 2;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.CurrencyCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "LicCompany+LC_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.LC_RX_NKCurrency)));
			this.CurrencyCodeFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|9f0668a2-31b6-4b44-a31e-34fce4c4fadf", "Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 38, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.CurrencyCodeFindBox.ShouldResize = true;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 3;
			// 
			// LicenceTabControl
			// 
			this.LicenceTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LicenceTabControl.Controls.Add(this.CompaniesTabPage);
			this.LicenceTabControl.Controls.Add(this.LicencesTabPage);
			this.LicenceTabControl.Controls.Add(this.Licence3rdPartySoftware);
			this.LicenceTabControl.Controls.Add(this.InstallationTabPage);
			this.LicenceTabControl.Controls.Add(this.ProjectsTabPage);
			this.LicenceTabControl.Controls.Add(this.TrainingSchedulesTabPage);
			this.LicenceTabControl.Controls.Add(this.ConnectionsTabPage);
			this.LicenceTabControl.Controls.Add(this.SupportAndContractTabPage);
			this.LicenceTabControl.Controls.Add(this.BillingInvoiceTabPage);
			this.LicenceTabControl.Controls.Add(this.BillingPricesTabPage);
			this.LicenceTabControl.Controls.Add(this.stlTabPage);
			this.LicenceTabControl.Controls.Add(this.MaintenanceTabPage);
			this.LicenceTabControl.Controls.Add(this.DiscountsTabPage);
			this.LicenceTabControl.Controls.Add(this.FeesTabPage);
			this.LicenceTabControl.Controls.Add(this.PremiumServicesTabPage);
			this.LicenceTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicenceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicenceTabControl.Name = "LicenceTabControl";
			this.LicenceTabControl.SelectedIndex = 0;
			this.LicenceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 341, true);
			this.LicenceTabControl.TabIndex = 0;
			// 
			// CompaniesTabPage
			// 
			this.CompaniesTabPage.Controls.Add(this.editionCommentBox);
			this.CompaniesTabPage.Controls.Add(this.EditionDropEdit);
			this.CompaniesTabPage.Controls.Add(this.productBox);
			this.CompaniesTabPage.Controls.Add(this.productLabel);
			this.CompaniesTabPage.Controls.Add(this.ClientCompaniesGrid);
			this.CompaniesTabPage.Controls.Add(this.CompanyDetailsGroupBox);
			this.CompaniesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CompaniesTabPage.Name = "CompaniesTabPage";
			this.CompaniesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.CompaniesTabPage.TabIndex = 1;
			this.CompaniesTabPage.Text = "Companies";
			// 
			// editionCommentBox
			// 
			this.editionCommentBox.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.editionCommentBox.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.editionCommentBox, false);
			this.editionCommentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 5, true);
			this.editionCommentBox.Name = "editionCommentBox";
			this.editionCommentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 13, true);
			this.editionCommentBox.TabIndex = 10;
			this.editionCommentBox.Text = "<Edition Comment>";
			// 
			// EditionDropEdit
			// 
			this.EditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EditionDropEdit, "LicCompany+DummyLicHeaderForBinding.Edition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding.Edition)));
			this.EditionDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0cb975de-fd98-4e47-aeda-6109fef75b1a", "Edition");
			this.EditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 2, true);
			this.EditionDropEdit.Name = "EditionDropEdit";
			this.EditionDropEdit.PreBoundMaxLength = 3;
			this.EditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.EditionDropEdit.TabIndex = 9;
			// 
			// productBox
			// 
			this.productBox.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.productBox.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.productBox, false);
			this.productBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 5, true);
			this.productBox.Name = "productBox";
			this.productBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 13, true);
			this.productBox.TabIndex = 8;
			this.productBox.Text = "<Product>";
			// 
			// productLabel
			// 
			this.productLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.productLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 5, true);
			this.productLabel.Name = "productLabel";
			this.productLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.productLabel.TabIndex = 7;
			this.productLabel.Text = "Product:";
			// 
			// ClientCompaniesGrid
			// 
			this.ClientCompaniesGrid.AllowNavigation = false;
			this.ClientCompaniesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientCompaniesGrid, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_RX_NKLocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_WebAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_BusinessRegNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_BusinessRegNo2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).DeactivateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LicenceEdition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).IsLiveAsText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).CreateTimeLocal)));
			this.ClientCompaniesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.ColumnName = "LCC_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.ColumnName = "LCC_Name";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Organization";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "LCC_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.ColumnName = "LCC_RN_NKCountryCode";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo4.Caption = "";
			zTextBoxColumnStyleInfo4.ColumnName = "LCC_RX_NKLocalCurrency";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "";
			zTextBoxColumnStyleInfo5.ColumnName = "LCC_Address1";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "";
			zTextBoxColumnStyleInfo6.ColumnName = "LCC_City";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.ColumnName = "LCC_PostCode";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "";
			zTextBoxColumnStyleInfo8.ColumnName = "LCC_State";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "";
			zTextBoxColumnStyleInfo9.ColumnName = "LCC_Phone";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "";
			zTextBoxColumnStyleInfo10.ColumnName = "LCC_WebAddress";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Reg #";
			zTextBoxColumnStyleInfo11.ColumnName = "LCC_BusinessRegNo";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "Reg #2";
			zTextBoxColumnStyleInfo12.ColumnName = "LCC_BusinessRegNo2";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Deactivated";
			zDateEditColumnStyleInfo1.ColumnName = "DeactivateTimeLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.Caption = "Edition";
			zTextBoxColumnStyleInfo13.ColumnName = "LicenceEdition";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.Caption = "Live";
			zTextBoxColumnStyleInfo14.ColumnName = "IsLiveAsText";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Created";
			zDateEditColumnStyleInfo2.ColumnName = "CreateTimeLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ClientCompaniesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ClientCompaniesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ClientCompaniesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ClientCompaniesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ClientCompaniesGrid.GridId = "28aa8b0c-86a7-4041-ba65-f93a86ce4b5e";
			this.ClientCompaniesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClientCompaniesGrid.LayoutKey = "CompaniesSharingDatabaseGrid";
			this.ClientCompaniesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
			this.ClientCompaniesGrid.Name = "ClientCompaniesGrid";
			this.ClientCompaniesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ClientCompaniesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 160, true);
			this.ClientCompaniesGrid.TabIndex = 5;
			// 
			// CompanyDetailsGroupBox
			// 
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientReciprocalCheckBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyCountryTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientPhoneTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientWebAddressTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientWhtCashBasisCheckBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientWhtRegisteredCheckBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientGstCashBasisCheckBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.ClientGstRegisteredCheckBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.zTextBox1);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyPostCodeTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyRegNoTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyStateTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyCityTextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyAddress2TextBox);
			this.CompanyDetailsGroupBox.Controls.Add(this.CompanyAddress1Box);
			this.CompanyDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CompanyDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 196, true);
			this.CompanyDetailsGroupBox.Name = "CompanyDetailsGroupBox";
			this.CompanyDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 128, true);
			this.CompanyDetailsGroupBox.TabIndex = 6;
			this.CompanyDetailsGroupBox.TabStop = false;
			this.CompanyDetailsGroupBox.Text = "Company Details";
			// 
			// ClientReciprocalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClientReciprocalCheckBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_IsReciprocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_IsReciprocal)));
			this.ClientReciprocalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientReciprocalCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ClientReciprocalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 105, true);
			this.ClientReciprocalCheckBox.Name = "ClientReciprocalCheckBox";
			this.ClientReciprocalCheckBox.ReadOnly = true;
			this.ClientReciprocalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ClientReciprocalCheckBox.TabIndex = 14;
			this.ClientReciprocalCheckBox.UseVisualStyleBackColor = true;
			// 
			// CompanyCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyCountryTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_RN_NKCountryCode)));
			this.CompanyCountryTextBox.CaptionResourceString = null;
			this.CompanyCountryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.CompanyCountryTextBox.Name = "CompanyCountryTextBox";
			this.CompanyCountryTextBox.ReadOnly = true;
			this.CompanyCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.CompanyCountryTextBox.TabIndex = 13;
			// 
			// ClientPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientPhoneTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Phone)));
			this.ClientPhoneTextBox.CaptionResourceString = null;
			this.ClientPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 18, true);
			this.ClientPhoneTextBox.Name = "ClientPhoneTextBox";
			this.ClientPhoneTextBox.ReadOnly = true;
			this.ClientPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.ClientPhoneTextBox.TabIndex = 5;
			// 
			// ClientWebAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientWebAddressTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_WebAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_WebAddress)));
			this.ClientWebAddressTextBox.CaptionResourceString = null;
			this.ClientWebAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientWebAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 39, true);
			this.ClientWebAddressTextBox.Name = "ClientWebAddressTextBox";
			this.ClientWebAddressTextBox.ReadOnly = true;
			this.ClientWebAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.ClientWebAddressTextBox.TabIndex = 6;
			// 
			// ClientWhtCashBasisCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClientWhtCashBasisCheckBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_IsWHTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_IsWHTCashBasis)));
			this.ClientWhtCashBasisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientWhtCashBasisCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ClientWhtCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 85, true);
			this.ClientWhtCashBasisCheckBox.Name = "ClientWhtCashBasisCheckBox";
			this.ClientWhtCashBasisCheckBox.ReadOnly = true;
			this.ClientWhtCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ClientWhtCashBasisCheckBox.TabIndex = 12;
			this.ClientWhtCashBasisCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClientWhtRegisteredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClientWhtRegisteredCheckBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_IsWHTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_IsWHTRegistered)));
			this.ClientWhtRegisteredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientWhtRegisteredCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ClientWhtRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 63, true);
			this.ClientWhtRegisteredCheckBox.Name = "ClientWhtRegisteredCheckBox";
			this.ClientWhtRegisteredCheckBox.ReadOnly = true;
			this.ClientWhtRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ClientWhtRegisteredCheckBox.TabIndex = 11;
			this.ClientWhtRegisteredCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClientGstCashBasisCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClientGstCashBasisCheckBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_IsGSTCashBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_IsGSTCashBasis)));
			this.ClientGstCashBasisCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientGstCashBasisCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ClientGstCashBasisCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 41, true);
			this.ClientGstCashBasisCheckBox.Name = "ClientGstCashBasisCheckBox";
			this.ClientGstCashBasisCheckBox.ReadOnly = true;
			this.ClientGstCashBasisCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ClientGstCashBasisCheckBox.TabIndex = 10;
			this.ClientGstCashBasisCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClientGstRegisteredCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClientGstRegisteredCheckBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_IsGSTRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_IsGSTRegistered)));
			this.ClientGstRegisteredCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClientGstRegisteredCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ClientGstRegisteredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 19, true);
			this.ClientGstRegisteredCheckBox.Name = "ClientGstRegisteredCheckBox";
			this.ClientGstRegisteredCheckBox.ReadOnly = true;
			this.ClientGstRegisteredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 16, true);
			this.ClientGstRegisteredCheckBox.TabIndex = 9;
			this.ClientGstRegisteredCheckBox.UseVisualStyleBackColor = true;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_BusinessRegNo2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_BusinessRegNo2)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 82, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.zTextBox1.TabIndex = 8;
			// 
			// CompanyPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyPostCodeTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_PostCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_PostCode)));
			this.CompanyPostCodeTextBox.CaptionResourceString = null;
			this.CompanyPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 83, true);
			this.CompanyPostCodeTextBox.Name = "CompanyPostCodeTextBox";
			this.CompanyPostCodeTextBox.ReadOnly = true;
			this.CompanyPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.CompanyPostCodeTextBox.TabIndex = 4;
			// 
			// CompanyRegNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyRegNoTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_BusinessRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_BusinessRegNo)));
			this.CompanyRegNoTextBox.CaptionResourceString = null;
			this.CompanyRegNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyRegNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 61, true);
			this.CompanyRegNoTextBox.Name = "CompanyRegNoTextBox";
			this.CompanyRegNoTextBox.ReadOnly = true;
			this.CompanyRegNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.CompanyRegNoTextBox.TabIndex = 7;
			// 
			// CompanyStateTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyStateTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_State)));
			this.CompanyStateTextBox.CaptionResourceString = null;
			this.CompanyStateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 83, true);
			this.CompanyStateTextBox.Name = "CompanyStateTextBox";
			this.CompanyStateTextBox.ReadOnly = true;
			this.CompanyStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.CompanyStateTextBox.TabIndex = 3;
			// 
			// CompanyCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyCityTextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_City)));
			this.CompanyCityTextBox.CaptionResourceString = null;
			this.CompanyCityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 62, true);
			this.CompanyCityTextBox.Name = "CompanyCityTextBox";
			this.CompanyCityTextBox.ReadOnly = true;
			this.CompanyCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.CompanyCityTextBox.TabIndex = 2;
			// 
			// CompanyAddress2TextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyAddress2TextBox, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Address2)));
			this.CompanyAddress2TextBox.CaptionResourceString = null;
			this.CompanyAddress2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 41, true);
			this.CompanyAddress2TextBox.Name = "CompanyAddress2TextBox";
			this.CompanyAddress2TextBox.ReadOnly = true;
			this.CompanyAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.CompanyAddress2TextBox.TabIndex = 1;
			// 
			// CompanyAddress1Box
			// 
			this.BindingSource.SetBindingMember(this.CompanyAddress1Box, "LicCompany+ActiveOrAllLicDatabases.ClientCompaniesExcludingDemo.LCC_Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.ClientCompany)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ClientCompaniesExcludingDemo)).SyncRoot)).LCC_Address1)));
			this.CompanyAddress1Box.CaptionResourceString = null;
			this.CompanyAddress1Box.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CompanyAddress1Box.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 19, true);
			this.CompanyAddress1Box.Name = "CompanyAddress1Box";
			this.CompanyAddress1Box.ReadOnly = true;
			this.CompanyAddress1Box.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.CompanyAddress1Box.TabIndex = 0;
			// 
			// LicencesTabPage
			// 
			this.LicencesTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|8dcf8224-1b67-4170-a4a9-0aa7356ade01", "Modules");
			this.LicencesTabPage.Controls.Add(this.licenceModulesControl);
			this.LicencesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LicencesTabPage.Name = "LicencesTabPage";
			this.LicencesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.LicencesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.LicencesTabPage.TabIndex = 0;
			// 
			// licenceModulesControl
			// 
			this.licenceModulesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.licenceModulesControl, "LicCompany+DummyLicHeaderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding)));
			this.licenceModulesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.licenceModulesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.licenceModulesControl.Name = "licenceModulesControl";
			this.licenceModulesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 314, true);
			this.licenceModulesControl.TabIndex = 26;
			// 
			// Licence3rdPartySoftware
			// 
			this.Licence3rdPartySoftware.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|554f0b62-4a4e-413c-b398-a85590348146", "3rd Party Software");
			this.Licence3rdPartySoftware.Controls.Add(this.Licence3rdPartyGrid);
			this.Licence3rdPartySoftware.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.Licence3rdPartySoftware.Name = "Licence3rdPartySoftware";
			this.Licence3rdPartySoftware.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Licence3rdPartySoftware.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.Licence3rdPartySoftware.TabIndex = 6;
			// 
			// Licence3rdPartyGrid
			// 
			this.Licence3rdPartyGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Licence3rdPartyGrid, "LicCompany+Licence3rdPartySoftware");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_OP_ProductSKU)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).PartDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_OSType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_LicenceIssued)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_LicenceCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_UpgradeAssuranceStartsOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.Licence3rdPartySoftware)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Licence3rdPartySoftware)).SyncRoot)).L3_UpgradeAssuranceEndsOn)));
			this.Licence3rdPartyGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "L3_OP_ProductSKU";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|76edce74-98af-4b98-af18-256904fc0987", "Product Description");
			zTextBoxColumnStyleInfo15.ColumnName = "PartDescription";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "L3_OH_Supplier";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "L3_OSType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "L3_LicenceType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.ColumnName = "L3_LicenceIssued";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "L3_LicenceCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.ColumnName = "L3_UpgradeAssuranceStartsOn";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDateEditColumnStyleInfo5.ColumnName = "L3_UpgradeAssuranceEndsOn";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.Licence3rdPartyGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.Licence3rdPartyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Licence3rdPartyGrid.GridId = "dfae7772-5adf-42c9-9e73-f0e05de1125c";
			this.Licence3rdPartyGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Licence3rdPartyGrid.LayoutKey = "Licence3rdPartyGrid";
			this.Licence3rdPartyGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Licence3rdPartyGrid.Name = "Licence3rdPartyGrid";
			this.Licence3rdPartyGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.Licence3rdPartyGrid.TabIndex = 0;
			// 
			// InstallationTabPage
			// 
			this.InstallationTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|12e2166a-7cf7-4da6-8ec7-36c30ec659f1", "Installation/Projects");
			this.InstallationTabPage.Controls.Add(this.installationUserControl);
			this.InstallationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InstallationTabPage.Name = "InstallationTabPage";
			this.InstallationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InstallationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.InstallationTabPage.TabIndex = 7;
			this.InstallationTabPage.Text = "Installation";
			// 
			// installationUserControl
			// 
			this.installationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.installationUserControl, "LicCompany+DummyLicHeaderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding)));
			this.installationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.installationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.installationUserControl.Name = "installationUserControl";
			this.installationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.installationUserControl.TabIndex = 0;
			// 
			// ProjectsTabPage
			// 
			this.ProjectsTabPage.Controls.Add(this.ProjectsGrid);
			this.ProjectsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProjectsTabPage.Name = "ProjectsTabPage";
			this.ProjectsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.ProjectsTabPage.TabIndex = 8;
			this.ProjectsTabPage.Text = "Projects";
			// 
			// ProjectsGrid
			// 
			this.ProjectsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProjectsGrid, "Projects");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_ProjectNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_Summary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).ContactPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_GS_NKProjectManager)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).WKP_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).PlannedInstall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).Projects)).SyncRoot)).InstallDate)));
			this.ProjectsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo16.Caption = "Project ID";
			zTextBoxColumnStyleInfo16.ColumnName = "WKP_ProjectNumber";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.Caption = "Type";
			zTextBoxColumnStyleInfo17.ColumnName = "WKP_Type";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo18.Caption = "Sub Type";
			zTextBoxColumnStyleInfo18.ColumnName = "WKP_SubType";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo19.Caption = "Module";
			zTextBoxColumnStyleInfo19.ColumnName = "WKP_Module";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo20.Caption = "Description";
			zTextBoxColumnStyleInfo20.ColumnName = "WKP_Summary";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo21.Caption = "Contact Phone";
			zTextBoxColumnStyleInfo21.ColumnName = "ContactPhone";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.Caption = "Manager";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "WKP_GS_NKProjectManager";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo22.Caption = "Status";
			zTextBoxColumnStyleInfo22.ColumnName = "WKP_Status";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo6.Caption = "Created";
			zDateEditColumnStyleInfo6.ColumnName = "WKP_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo7.Caption = "Planned Install";
			zDateEditColumnStyleInfo7.ColumnName = "PlannedInstall";
			zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo8.Caption = "Install Date";
			zDateEditColumnStyleInfo8.ColumnName = "InstallDate";
			zDateEditColumnStyleInfo8.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.ProjectsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ProjectsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.ProjectsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ProjectsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ProjectsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.ProjectsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProjectsGrid.GridId = "5e74b759-0046-4669-95b8-e63697318654";
			this.ProjectsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProjectsGrid.LayoutKey = "ProjectsGrid";
			this.ProjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProjectsGrid.Name = "ProjectsGrid";
			this.ProjectsGrid.ReadOnly = true;
			this.ProjectsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ProjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 324, true);
			this.ProjectsGrid.TabIndex = 18;
			this.ProjectsGrid.DoubleClick += new System.EventHandler(this.ProjectsGrid_DoubleClick);
			// 
			// TrainingSchedulesTabPage
			// 
			this.TrainingSchedulesTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|956e8ffe-287a-4b66-97d4-8e6b34411be5", "Training");
			this.TrainingSchedulesTabPage.Controls.Add(this.TrainingSchedulesPanel);
			this.TrainingSchedulesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TrainingSchedulesTabPage.Name = "TrainingSchedulesTabPage";
			this.TrainingSchedulesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.TrainingSchedulesTabPage.TabIndex = 9;
			// 
			// TrainingSchedulesPanel
			// 
			this.TrainingSchedulesPanel.Controls.Add(this.TrainingCourseGrid);
			this.TrainingSchedulesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TrainingSchedulesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TrainingSchedulesPanel.Name = "TrainingSchedulesPanel";
			this.TrainingSchedulesPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.TrainingSchedulesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 324, true);
			this.TrainingSchedulesPanel.TabIndex = 23;
			// 
			// ConnectionsTabPage
			// 
			this.ConnectionsTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|63074a4b-8ea2-46e8-986c-6a9af411d174", "Connections");
			this.ConnectionsTabPage.Controls.Add(this.ConnectionsPanel);
			this.ConnectionsTabPage.Controls.Add(this.ConnectionsSplitter);
			this.ConnectionsTabPage.Controls.Add(this.ConnectionsGrid);
			this.ConnectionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConnectionsTabPage.Name = "ConnectionsTabPage";
			this.ConnectionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.ConnectionsTabPage.TabIndex = 10;
			this.ConnectionsTabPage.TabVisible = false;
			// 
			// ConnectionsPanel
			// 
			this.ConnectionsPanel.Controls.Add(this.ConnectButton);
			this.ConnectionsPanel.Controls.Add(this.ConnectionsRichTextBox);
			this.ConnectionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConnectionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.ConnectionsPanel.Name = "ConnectionsPanel";
			this.ConnectionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 196, true);
			this.ConnectionsPanel.TabIndex = 5;
			// 
			// ConnectButton
			// 
			this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|401b7320-82c0-4900-a40c-e9f7b41d35a1", "Connect...");
			this.ConnectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(759, 0, true);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConnectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.ConnectButton.TabIndex = 3;
			this.ConnectButton.ToolTipCaption = null;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectToClient);
			// 
			// ConnectionsRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConnectionsRichTextBox, "LicCompany+ActiveOrAllLicDatabases.ConnectionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).ConnectionDetails)));
			this.ConnectionsRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConnectionsRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConnectionsRichTextBox.MaxLength = 10000000;
			this.ConnectionsRichTextBox.Name = "ConnectionsRichTextBox";
			this.ConnectionsRichTextBox.ParentZForm = null;
			this.ConnectionsRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 196, true);
			this.ConnectionsRichTextBox.TabIndex = 1;
			// 
			// ConnectionsSplitter
			// 
			this.ConnectionsSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConnectionsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 125, true);
			this.ConnectionsSplitter.Name = "ConnectionsSplitter";
			this.ConnectionsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 3, true);
			this.ConnectionsSplitter.TabIndex = 4;
			this.ConnectionsSplitter.TabStop = false;
			// 
			// ConnectionsGrid
			// 
			this.ConnectionsGrid.AllowNavigation = false;
			this.ConnectionsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.ConnectionsGrid, "LicCompany+ActiveOrAllLicDatabases.Connections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_ConnectionOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_RemoteAccessMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).RemoteAccessMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_RemoteAccessAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_RemoteAccessUserName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_RemoteAccessPassWord)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceConnection)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)).Connections)).SyncRoot)).LK_LC_Company)));
			this.ConnectionsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Order";
			zCalcEditColumnStyleInfo2.ColumnName = "LK_ConnectionOrder";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.Caption = "Method";
			zDropEditColumnStyleInfo3.ColumnName = "LK_RemoteAccessMethod";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo25.Caption = "Remote Access Method";
			zTextBoxColumnStyleInfo25.ColumnName = "RemoteAccessMethodDescription";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo26.Caption = "Remote Access Address";
			zTextBoxColumnStyleInfo26.ColumnName = "LK_RemoteAccessAddress";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo27.Caption = "Description";
			zTextBoxColumnStyleInfo27.ColumnName = "LK_Description";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo28.Caption = "User Name";
			zTextBoxColumnStyleInfo28.ColumnName = "LK_RemoteAccessUserName";
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo29.Caption = "Password";
			zTextBoxColumnStyleInfo29.ColumnName = "LK_RemoteAccessPassWord";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zGuidDropEditColumnStyleInfo1.Caption = "Company";
			zGuidDropEditColumnStyleInfo1.ColumnName = "LK_LC_Company";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ConnectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ConnectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ConnectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.ConnectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.ConnectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.ConnectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.ConnectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.ConnectionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.ConnectionsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConnectionsGrid.GridId = "c94a3186-b66c-4a1b-b0f0-221d6caac861";
			this.ConnectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConnectionsGrid.LayoutKey = "ConnectionsGrid";
			this.ConnectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConnectionsGrid.Name = "ConnectionsGrid";
			this.ConnectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 125, true);
			this.ConnectionsGrid.TabIndex = 0;
			this.ConnectionsGrid.DoubleClick += new System.EventHandler(this.ConnectToClientOnDoubleClick);
			// 
			// SupportAndContractTabPage
			// 
			this.SupportAndContractTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|a84769c5-8291-4eff-8862-54756553f443", "Support & Contract");
			this.SupportAndContractTabPage.Controls.Add(this.supportContractUserControl);
			this.SupportAndContractTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportAndContractTabPage.Name = "SupportAndContractTabPage";
			this.SupportAndContractTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.SupportAndContractTabPage.TabIndex = 11;
			// 
			// supportContractUserControl
			// 
			this.supportContractUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.supportContractUserControl, "LicCompany+DummyLicHeaderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding)));
			this.supportContractUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportContractUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supportContractUserControl.Name = "supportContractUserControl";
			this.supportContractUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.supportContractUserControl.TabIndex = 0;
			// 
			// BillingInvoiceTabPage
			// 
			this.BillingInvoiceTabPage.Controls.Add(this.BillingInvoicingControl);
			this.BillingInvoiceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillingInvoiceTabPage.Name = "BillingInvoiceTabPage";
			this.BillingInvoiceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillingInvoiceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.BillingInvoiceTabPage.TabIndex = 12;
			this.BillingInvoiceTabPage.Text = "Invoicing";
			// 
			// BillingInvoicingControl
			// 
			this.BillingInvoicingControl.AllowDrop = true;
			this.BillingInvoicingControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.BillingInvoicingControl, "LicCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompany)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany)));
			this.BillingInvoicingControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillingInvoicingControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillingInvoicingControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 350, true);
			this.BillingInvoicingControl.Name = "BillingInvoicingControl";
			this.BillingInvoicingControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 350, true);
			this.BillingInvoicingControl.TabIndex = 0;
			// 
			// BillingPricesTabPage
			// 
			this.BillingPricesTabPage.Controls.Add(this.BillingPricesControl);
			this.BillingPricesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillingPricesTabPage.Name = "BillingPricesTabPage";
			this.BillingPricesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillingPricesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.BillingPricesTabPage.TabIndex = 13;
			this.BillingPricesTabPage.Text = "Pricelists";
			// 
			// BillingPricesControl
			// 
			this.BillingPricesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillingPricesControl, "LicCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceCompany)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany)));
			this.BillingPricesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillingPricesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillingPricesControl.Name = "BillingPricesControl";
			this.BillingPricesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.BillingPricesControl.TabIndex = 0;
			// 
			// stlTabPage
			// 
			this.stlTabPage.Controls.Add(this.stlLicenceControl);
			this.stlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.stlTabPage.Name = "stlTabPage";
			this.stlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.stlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.stlTabPage.TabIndex = 14;
			this.stlTabPage.Text = "STL";
			// 
			// stlLicenceControl
			// 
			this.stlLicenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.stlLicenceControl, "LicCompany+DummyLicHeaderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding)));
			this.stlLicenceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stlLicenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.stlLicenceControl.Name = "stlLicenceControl";
			this.stlLicenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.stlLicenceControl.TabIndex = 5;
			// 
			// MaintenanceTabPage
			// 
			this.MaintenanceTabPage.Controls.Add(this.licenceMaintenanceControl);
			this.MaintenanceTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MaintenanceTabPage.Name = "MaintenanceTabPage";
			this.MaintenanceTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MaintenanceTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.MaintenanceTabPage.TabIndex = 15;
			this.MaintenanceTabPage.Text = "Maintenance";
			// 
			// licenceMaintenanceControl
			// 
			this.licenceMaintenanceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.licenceMaintenanceControl, "LicCompany+DummyLicHeaderForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding)));
			this.licenceMaintenanceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.licenceMaintenanceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.licenceMaintenanceControl.Name = "licenceMaintenanceControl";
			this.licenceMaintenanceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.licenceMaintenanceControl.TabIndex = 0;
			// 
			// DiscountsTabPage
			// 
			this.DiscountsTabPage.Controls.Add(this.BillingDiscountsGrid);
			this.DiscountsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DiscountsTabPage.Name = "DiscountsTabPage";
			this.DiscountsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.DiscountsTabPage.TabIndex = 16;
			this.DiscountsTabPage.Text = "Discounts";
			// 
			// BillingDiscountsGrid
			// 
			this.BillingDiscountsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BillingDiscountsGrid, "LicCompany+SelfBilling.BillingDiscounts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_SubCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_DiscountCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_ModuleCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_BreakAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_BreakUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Discount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Duration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceBillingDiscount)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.SelfBilling.BillingDiscounts)).SyncRoot)).L5_Comment)));
			this.BillingDiscountsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.Caption = "System";
			zDropEditColumnStyleInfo4.ColumnName = "L5_SystemCode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo5.Caption = "Category";
			zDropEditColumnStyleInfo5.ColumnName = "L5_SubCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo30.Caption = "Version";
			zTextBoxColumnStyleInfo30.ColumnName = "L5_DiscountCode";
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo6.Caption = "Type";
			zDropEditColumnStyleInfo6.ColumnName = "L5_Type";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo7.Caption = "Module Code";
			zDropEditColumnStyleInfo7.ColumnName = "L5_ModuleCode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Break/Min. Spend";
			zCalcEditColumnStyleInfo3.ColumnName = "L5_BreakAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Units";
			zCalcEditColumnStyleInfo4.ColumnName = "L5_Units";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo8.Caption = "Break Units";
			zDropEditColumnStyleInfo8.ColumnName = "L5_BreakUnits";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Discount %";
			zCalcEditColumnStyleInfo5.ColumnName = "L5_Discount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo10.Caption = "Start Date";
			zDateEditColumnStyleInfo10.ColumnName = "L5_StartDate";
			zDateEditColumnStyleInfo10.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo11.Caption = "End Date";
			zDateEditColumnStyleInfo11.ColumnName = "L5_EndDate";
			zDateEditColumnStyleInfo11.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo31.Caption = "Additional Description";
			zTextBoxColumnStyleInfo31.ColumnName = "L5_Description";
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Duration (Mths)";
			zCalcEditColumnStyleInfo6.ColumnName = "L5_Duration";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo1.Caption = "Comment";
			zMultiLineTextBoxColumnInfo1.ColumnName = "L5_Comment";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.BillingDiscountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.BillingDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.BillingDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.BillingDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.BillingDiscountsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.BillingDiscountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.BillingDiscountsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.BillingDiscountsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.BillingDiscountsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillingDiscountsGrid.GridId = "50663935-00ed-4503-b958-3f241c276eaa";
			this.BillingDiscountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BillingDiscountsGrid.LayoutKey = "BillingDiscountsGrid";
			this.BillingDiscountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillingDiscountsGrid.Name = "BillingDiscountsGrid";
			this.BillingDiscountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(887, 324, true);
			this.BillingDiscountsGrid.TabIndex = 27;
			// 
			// FeesTabPage
			// 
			this.FeesTabPage.Controls.Add(this.FeesGrid);
			this.FeesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FeesTabPage.Name = "FeesTabPage";
			this.FeesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.FeesTabPage.TabIndex = 17;
			this.FeesTabPage.Text = "Fees";
			// 
			// FeesGrid
			// 
			this.FeesGrid.AllowNavigation = false;
			this.FeesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FeesGrid, "LicCompany.Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_LD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_IsDiscountable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).DiscountChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_RenewalMonths)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_TaxDateCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientLicenceFee)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.Fees)).SyncRoot)).L8_Comment)));
			this.FeesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = "Order";
			zCalcEditColumnStyleInfo7.ColumnName = "L8_Order";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo9.Caption = "Type";
			zDropEditColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo9.ColumnName = "L8_Type";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo10.Caption = "Bill";
			zDropEditColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo10.ColumnName = "L8_SystemCode";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidDropEditColumnStyleInfo2.Caption = "Database";
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "L8_LD";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo2.Caption = "Description";
			zMultiLineTextBoxColumnInfo2.ColumnName = "L8_Description";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 350;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = "AmountDecimals";
			zCalcEditColumnStyleInfo8.Caption = "Amount";
			zCalcEditColumnStyleInfo8.ColumnName = "L8_Amount";
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.Caption = "Currency";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "L8_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo32.Caption = "Charge Code";
			zTextBoxColumnStyleInfo32.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo32.ColumnName = "L8_ChargeCode";
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "Can Discount?";
			zCheckBoxColumnStyleInfo1.ColumnName = "L8_IsDiscountable";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.ColumnName = "DiscountChargeCode";
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo12.Caption = "Start Date";
			zDateEditColumnStyleInfo12.ColumnName = "L8_StartDate";
			zDateEditColumnStyleInfo12.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo13.Caption = "End Date";
			zDateEditColumnStyleInfo13.ColumnName = "L8_EndDate";
			zDateEditColumnStyleInfo13.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.Caption = "Renewal Months";
			zCalcEditColumnStyleInfo9.ColumnName = "L8_RenewalMonths";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.Caption = "Tax Date";
			zDropEditColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo11.ColumnName = "L8_TaxDateCode";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zMultiLineTextBoxColumnInfo3.Caption = "Comment";
			zMultiLineTextBoxColumnInfo3.ColumnName = "L8_Comment";
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.FeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.FeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.FeesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.FeesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.FeesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.FeesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.FeesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.FeesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.FeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.FeesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.FeesGrid.GridId = "516a0bef-2540-4702-9f05-8fcde27a6f69";
			this.FeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesGrid.LayoutKey = "FeesGrid";
			this.FeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 31, true);
			this.FeesGrid.Name = "FeesGrid";
			this.FeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 287, true);
			this.FeesGrid.TabIndex = 3;
			// 
			// PremiumServicesTabPage
			// 
			this.PremiumServicesTabPage.Controls.Add(this.clientPremiumServicesControl1);
			this.PremiumServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PremiumServicesTabPage.Name = "PremiumServicesTabPage";
			this.PremiumServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PremiumServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 314, true);
			this.PremiumServicesTabPage.TabIndex = 18;
			this.PremiumServicesTabPage.Text = "Premium";
			// 
			// clientPremiumServicesControl1
			// 
			this.clientPremiumServicesControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientPremiumServicesControl1, "LicCompany+ActiveOrAllLicDatabases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)).SyncRoot)))));
			this.clientPremiumServicesControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.clientPremiumServicesControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.clientPremiumServicesControl1.Name = "clientPremiumServicesControl1";
			this.clientPremiumServicesControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 318, true);
			this.clientPremiumServicesControl1.TabIndex = 0;
			// 
			// LicenceActiveCheckBox
			// 
			this.LicenceActiveCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.LicenceActiveCheckBox, "LicCompany+DummyLicHeaderForBinding.LA_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.DummyLicHeaderForBinding.LA_IsActive)));
			this.LicenceActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LicenceActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 93, true);
			this.LicenceActiveCheckBox.Name = "LicenceActiveCheckBox";
			this.LicenceActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 15, true);
			this.LicenceActiveCheckBox.TabIndex = 16;
			this.LicenceActiveCheckBox.Text = "Licence Active on Database";
			this.LicenceActiveCheckBox.UseVisualStyleBackColor = true;
			this.LicenceActiveCheckBox.Click += new System.EventHandler(this.LicenceActiveCheckBox_Click);
			// 
			// GenerateLoginTokenButton
			// 
			this.GenerateLoginTokenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GenerateLoginTokenButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|0a42d9f7-b70a-4713-a74c-869c7144e39a", "Generate Login Token");
			this.GenerateLoginTokenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 88, true);
			this.GenerateLoginTokenButton.Name = "GenerateLoginTokenButton";
			this.GenerateLoginTokenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.GenerateLoginTokenButton.TabIndex = 17;
			this.GenerateLoginTokenButton.Visible = false;
			this.GenerateLoginTokenButton.Click += new System.EventHandler(this.GenerateLoginTokenButton_Click);
			// 
			// GenerateDiagnosticsLoginTokenButton
			// 
			this.GenerateDiagnosticsLoginTokenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GenerateDiagnosticsLoginTokenButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|0f9951b1-44db-4f05-a30d-72d8371bb420", "Generate Diagnostics Login Token");
			this.GenerateDiagnosticsLoginTokenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 88, true);
			this.GenerateDiagnosticsLoginTokenButton.Name = "GenerateDiagnosticsLoginTokenButton";
			this.GenerateDiagnosticsLoginTokenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 23, true);
			this.GenerateDiagnosticsLoginTokenButton.TabIndex = 18;
			this.GenerateDiagnosticsLoginTokenButton.Visible = false;
			this.GenerateDiagnosticsLoginTokenButton.Click += new System.EventHandler(this.GenerateDiagnosticsLoginTokenButton_Click);
			// 
			// GenerateLoginTokenWithOldEntCodeButton
			// 
			this.GenerateLoginTokenWithOldEntCodeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GenerateLoginTokenWithOldEntCodeButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|2EB6469C-3E58-43AF-9A62-437B058B72AC", "Generate Login Token With Old Ent Code");
			this.GenerateLoginTokenWithOldEntCodeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(710, 88, true);
			this.GenerateLoginTokenWithOldEntCodeButton.Name = "GenerateLoginTokenWithOldEntCodeButton";
			this.GenerateLoginTokenWithOldEntCodeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 23, true);
			this.GenerateLoginTokenWithOldEntCodeButton.TabIndex = 19;
			this.GenerateLoginTokenWithOldEntCodeButton.Visible = false;
			this.GenerateLoginTokenWithOldEntCodeButton.Click += new System.EventHandler(this.GenerateLoginTokenWithOldEntCodeButton_Click);
			// 
			// ShowInactiveDatabasesCheckBox
			// 
			this.ShowInactiveDatabasesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowInactiveDatabasesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowInactiveDatabasesCheckBox, "LicCompany+IncludeInactiveDatabases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.IncludeInactiveDatabases)));
			this.ShowInactiveDatabasesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowInactiveDatabasesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 90, true);
			this.ShowInactiveDatabasesCheckBox.Name = "ShowInactiveDatabasesCheckBox";
			this.ShowInactiveDatabasesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.ShowInactiveDatabasesCheckBox.TabIndex = 8;
			this.ShowInactiveDatabasesCheckBox.Text = "Show Inactive";
			this.ShowInactiveDatabasesCheckBox.UseVisualStyleBackColor = true;
			// 
			// DatabasesModuleButtonGrid
			// 
			this.DatabasesModuleButtonGrid.AllowDrop = true;
			this.DatabasesModuleButtonGrid.AttachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("2F901B52-A297-44D8-AC11-ABF6C21348D4", "Add");
			this.BindingSource.SetBindingMember(this.DatabasesModuleButtonGrid, "LicCompany+ActiveOrAllLicDatabases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.ActiveOrAllLicDatabases)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).AllDatabasesInLicenceEnterprise)));
			this.DatabasesModuleButtonGrid.BindToFindBoxList = "AllDatabasesInLicenceEnterprise";
			zCheckBoxColumnStyleInfo2.Caption = "Active";
			zCheckBoxColumnStyleInfo2.ColumnName = "LD_IsActive";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo34.Caption = "Product";
			zTextBoxColumnStyleInfo34.ColumnName = "LD_Product";
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo35.Caption = "Server Code";
			zTextBoxColumnStyleInfo35.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo36.Caption = "ID";
			zTextBoxColumnStyleInfo36.ColumnName = "DatabaseId";
			zTextBoxColumnStyleInfo36.IsReadOnly = true;
			zTextBoxColumnStyleInfo36.IsVisible = false;
			zTextBoxColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo37.Caption = "Model";
			zTextBoxColumnStyleInfo37.ColumnName = "BillingModel";
			zTextBoxColumnStyleInfo37.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo38.Caption = "Licence Type";
			zTextBoxColumnStyleInfo38.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo38.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo39.Caption = "Hosted Location";
			zTextBoxColumnStyleInfo39.ColumnName = "LD_HostedLocation";
			zTextBoxColumnStyleInfo39.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo40.Caption = "Release Ring";
			zTextBoxColumnStyleInfo40.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo40.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo41.Caption = "Release Ring Description";
			zTextBoxColumnStyleInfo41.ColumnName = "ReleaseRingDescription";
			zTextBoxColumnStyleInfo41.IsVisible = false;
			zTextBoxColumnStyleInfo41.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo42.Caption = "Server Security Mode";
			zTextBoxColumnStyleInfo42.ColumnName = "LD_DBServerSecurityMode";
			zTextBoxColumnStyleInfo42.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo43.Caption = "Email Address For Updates";
			zTextBoxColumnStyleInfo43.ColumnName = "LD_PublicEmailAddressForUpdate";
			zTextBoxColumnStyleInfo43.IsVisible = false;
			zTextBoxColumnStyleInfo43.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo44.Caption = "Upgrade Method";
			zTextBoxColumnStyleInfo44.ColumnName = "LD_AvailableUpgradeMethod";
			zTextBoxColumnStyleInfo44.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo14.Caption = "EXE Date";
			zDateEditColumnStyleInfo14.ColumnName = "CurrentVersion+HL_ExeVersionDate";
			zDateEditColumnStyleInfo14.IsVisible = false;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.Caption = "Current Version";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "LD_HL_CurrentRunningVersion";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo15.Caption = "Current Version\'s Exe Date";
			zDateEditColumnStyleInfo15.ColumnName = "CurrentVersionExeDate";
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo45.Caption = "Current Version\'s Release";
			zTextBoxColumnStyleInfo45.ColumnName = "CurrentVersionRelease";
			zTextBoxColumnStyleInfo45.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.Caption = "Sent Version";
			zGuidFindBoxColumnStyleInfo4.ColumnName = "LD_HL_CurrentSentVersion";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo16.Caption = "Sent Version\'s Exe Date";
			zDateEditColumnStyleInfo16.ColumnName = "SentVersionExeDate";
			zDateEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo46.Caption = "Sent Version\'s Release";
			zTextBoxColumnStyleInfo46.ColumnName = "SentVersionRelease";
			zTextBoxColumnStyleInfo46.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.Caption = "Tech Contact";
			zGuidFindBoxColumnStyleInfo5.ColumnName = "LD_OC_ContractInstallerOrInternalTechContact";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo6.Caption = "Admin Contact";
			zGuidFindBoxColumnStyleInfo6.ColumnName = "LD_OC_LicenseeAdminContact";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo47.Caption = "Reg. Server Name";
			zTextBoxColumnStyleInfo47.ColumnName = "LD_HostServerName";
			zTextBoxColumnStyleInfo47.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo48.Caption = "Reg. DB Instance Name";
			zTextBoxColumnStyleInfo48.ColumnName = "LD_HostDBInstance";
			zTextBoxColumnStyleInfo48.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo53.Caption = "Rpt. Server Name";
			zTextBoxColumnStyleInfo53.ColumnName = "LD_ReportedHostServerName";
			zTextBoxColumnStyleInfo53.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo54.Caption = "Rpt. DB Instance Name";
			zTextBoxColumnStyleInfo54.ColumnName = "LD_ReportedHostDBInstance";
			zTextBoxColumnStyleInfo54.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo55.Caption = "Feature Set Name";
			zTextBoxColumnStyleInfo55.ColumnName = "FeatureSet+FCS_ProductName";
			zTextBoxColumnStyleInfo55.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo49.Caption = "#";
			zTextBoxColumnStyleInfo49.ColumnName = "LD_DatabaseNumber";
			zTextBoxColumnStyleInfo49.IsReadOnly = true;
			zTextBoxColumnStyleInfo49.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo50.Caption = "Master Org";
			zTextBoxColumnStyleInfo50.ColumnName = "WebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo50.IsReadOnly = true;
			zTextBoxColumnStyleInfo50.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo51.Caption = "Tenant ID";
			zTextBoxColumnStyleInfo51.ColumnName = "LD_TenantID";
			zTextBoxColumnStyleInfo51.IsReadOnly = true;
			zTextBoxColumnStyleInfo51.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.Caption = "Token Authentication";
			zCheckBoxColumnStyleInfo3.ColumnName = "LD_TokenAuthenticationEnabled";
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo52.Caption = "System ID";
			zTextBoxColumnStyleInfo52.ColumnName = "SystemID";
			zTextBoxColumnStyleInfo52.IsReadOnly = true;
			zTextBoxColumnStyleInfo52.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo35);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo36);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo38);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo39);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo40);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo41);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo42);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo43);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo44);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo45);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo46);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo47);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo48);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo53);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo54);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo55);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo49);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo50);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo51);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.DatabasesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo52);
			this.DatabasesModuleButtonGrid.DetachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("579F1343-8E85-43EE-83EB-017B637BDD9D", "Remove");
			this.DatabasesModuleButtonGrid.DetachMessage = CargoWiseOne.ResourceStrings.Res.GetData("91A82C1C-0A34-4D0F-85F7-C59B32FFC505", "Are you sure you want to remove this Database from the client?");
			this.DatabasesModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabasesModuleButtonGrid.GridId = "e156351d-c080-4b49-9fed-e852fa45ee31";
			// 
			// 
			// 
			this.DatabasesModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DatabasesModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DatabasesModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DatabasesModuleButtonGrid.InnerGrid.GridId = null;
			this.DatabasesModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DatabasesModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DatabasesModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DatabasesModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DatabasesModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DatabasesModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 74, true);
			this.DatabasesModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DatabasesModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DatabasesModuleButtonGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 0, true);
			this.DatabasesModuleButtonGrid.Name = "DatabasesModuleButtonGrid";
			this.DatabasesModuleButtonGrid.NameOfAGridElement = CargoWiseOne.ResourceStrings.Res.GetData("F8E3708F-172F-46D4-953B-618FE361F3CE", "Database");
			this.DatabasesModuleButtonGrid.ParentControl = null;
			this.DatabasesModuleButtonGrid.ReadOnly = true;
			this.DatabasesModuleButtonGrid.ShowEditButton = false;
			this.DatabasesModuleButtonGrid.ShowNewButton = false;
			this.DatabasesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 112, true);
			this.DatabasesModuleButtonGrid.TabIndex = 0;
			// 
			// ActionButtonsPanel
			// 
			this.ActionButtonsPanel.Controls.Add(this.zLabel5);
			this.ActionButtonsPanel.Controls.Add(this.GenerateReopenPeriodKeyButton);
			this.ActionButtonsPanel.Controls.Add(this.LicenceKeyActionPanel);
			this.ActionButtonsPanel.Controls.Add(this.SendButton);
			this.ActionButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ActionButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 552, true);
			this.ActionButtonsPanel.Name = "ActionButtonsPanel";
			this.ActionButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 32, true);
			this.ActionButtonsPanel.TabIndex = 4;
			// 
			// zLabel5
			// 
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.zLabel5.TabIndex = 4;
			this.zLabel5.Text = "Package:";
			this.zLabel5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// GenerateReopenPeriodKeyButton
			// 
			this.GenerateReopenPeriodKeyButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|01f3ee84-db93-4812-9090-d83d10a155a3", "Generate Reopen Period Key");
			this.GenerateReopenPeriodKeyButton.Image = ((System.Drawing.Image)(resources.GetObject("GenerateReopenPeriodKeyButton.Image")));
			this.GenerateReopenPeriodKeyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.GenerateReopenPeriodKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 6, true);
			this.GenerateReopenPeriodKeyButton.Name = "GenerateReopenPeriodKeyButton";
			this.GenerateReopenPeriodKeyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GenerateReopenPeriodKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 23, true);
			this.GenerateReopenPeriodKeyButton.TabIndex = 2;
			this.GenerateReopenPeriodKeyButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.GenerateReopenPeriodKeyButton.ToolTipCaption = null;
			this.GenerateReopenPeriodKeyButton.Click += new System.EventHandler(this.GenerateReopenPeriodKeyButton_Click);
			// 
			// LicenceKeyActionPanel
			// 
			this.LicenceKeyActionPanel.Controls.Add(this.zLabel6);
			this.LicenceKeyActionPanel.Controls.Add(this.EmailCompanyButton);
			this.LicenceKeyActionPanel.Controls.Add(this.GenerateLicenceKeyButton);
			this.LicenceKeyActionPanel.Controls.Add(this.AutoDeployLicenceButton);
			this.LicenceKeyActionPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.LicenceKeyActionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(409, 0, true);
			this.LicenceKeyActionPanel.Name = "LicenceKeyActionPanel";
			this.LicenceKeyActionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 32, true);
			this.LicenceKeyActionPanel.TabIndex = 1;
			// 
			// zLabel6
			// 
			this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 8, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 18, true);
			this.zLabel6.TabIndex = 5;
			this.zLabel6.Text = "License Key:";
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EmailCompanyButton
			// 
			this.EmailCompanyButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|FEC3DBDE-FBC3-40A1-94AD-A126F9F9A296", "Email CW1 Company Import File");
			this.EmailCompanyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 6, true);
			this.EmailCompanyButton.Name = "EmailCompanyButton";
			this.EmailCompanyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.EmailCompanyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 23, true);
			this.EmailCompanyButton.TabIndex = 0;
			this.EmailCompanyButton.Text = "Email CW1 Company Import File";
			this.EmailCompanyButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EmailCompanyButton.ToolTipCaption = null;
			this.EmailCompanyButton.Click += new System.EventHandler(this.EmailCompanyButton_Click);
			// 
			// GenerateLicenceKeyButton
			// 
			this.GenerateLicenceKeyButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|b9a1dcd4-9945-4a3a-bd0f-3cc962cdf50f", "Save && Email License Key");
			this.GenerateLicenceKeyButton.Image = ((System.Drawing.Image)(resources.GetObject("GenerateLicenceKeyButton.Image")));
			this.GenerateLicenceKeyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.GenerateLicenceKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 6, true);
			this.GenerateLicenceKeyButton.Name = "GenerateLicenceKeyButton";
			this.GenerateLicenceKeyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.GenerateLicenceKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.GenerateLicenceKeyButton.TabIndex = 0;
			this.GenerateLicenceKeyButton.Text = "Save && Email";
			this.GenerateLicenceKeyButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.GenerateLicenceKeyButton.ToolTipCaption = null;
			this.GenerateLicenceKeyButton.Click += new System.EventHandler(this.GenerateLicenceKeyButton_Click);
			// 
			// AutoDeployLicenceButton
			// 
			this.AutoDeployLicenceButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|1b81300a-0d0c-433a-b1c9-64f8d3788f1b", "Update License Key Remotely");
			this.AutoDeployLicenceButton.Image = ((System.Drawing.Image)(resources.GetObject("AutoDeployLicenceButton.Image")));
			this.AutoDeployLicenceButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AutoDeployLicenceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 6, true);
			this.AutoDeployLicenceButton.Name = "AutoDeployLicenceButton";
			this.AutoDeployLicenceButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AutoDeployLicenceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 23, true);
			this.AutoDeployLicenceButton.TabIndex = 1;
			this.AutoDeployLicenceButton.Text = "Update Remotely";
			this.AutoDeployLicenceButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AutoDeployLicenceButton.ToolTipCaption = null;
			this.AutoDeployLicenceButton.Click += new System.EventHandler(this.AutoDeployLicenceButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|ca98b827-bdb3-43fd-883c-4635470def31", "Send Package All Databases");
			this.SendButton.Image = ((System.Drawing.Image)(resources.GetObject("SendButton.Image")));
			this.SendButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 6, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.SendButton.TabIndex = 0;
			this.SendButton.Text = "Send";
			this.SendButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GenerateLicenceKeyMenu
			// 
			this.GenerateLicenceKeyMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.menuItem1,
			this.menuItem2});
			// 
			// menuItem1
			// 
			this.menuItem1.Caption = null;
			this.menuItem1.CaptionResourceString = null;
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Save Licence Key";
			// 
			// menuItem2
			// 
			this.menuItem2.Caption = null;
			this.menuItem2.CaptionResourceString = null;
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "Auto-Deploy Licence Key";
			// 
			// KeyBuilderPanel
			// 
			this.KeyBuilderPanel.Controls.Add(this.DatabaseGroupBox);
			this.KeyBuilderPanel.Controls.Add(this.GSTGroupBox);
			this.KeyBuilderPanel.Controls.Add(this.ActionButtonsPanel);
			this.KeyBuilderPanel.Controls.Add(this.ExchangeRateGroupBox);
			this.KeyBuilderPanel.Controls.Add(this.LicenceInfoGroupBox);
			this.KeyBuilderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.KeyBuilderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.KeyBuilderPanel.Name = "KeyBuilderPanel";
			this.KeyBuilderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 584, true);
			this.KeyBuilderPanel.TabIndex = 0;
			// 
			// DatabaseGroupBox
			// 
			this.DatabaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DatabaseGroupBox.Controls.Add(this.DbSplitter);
			this.DatabaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.DatabaseGroupBox.Name = "DatabaseGroupBox";
			this.DatabaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 480, true);
			this.DatabaseGroupBox.TabIndex = 3;
			this.DatabaseGroupBox.TabStop = false;
			this.DatabaseGroupBox.Text = "Databases";
			// 
			// DbSplitter
			// 
			this.DbSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DbSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.DbSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DbSplitter.Name = "DbSplitter";
			this.DbSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DbSplitter.Panel1
			// 
			this.DbSplitter.Panel1.Controls.Add(this.ShowInactiveDatabasesCheckBox);
			this.DbSplitter.Panel1.Controls.Add(this.InactiveCountBox);
			this.DbSplitter.Panel1.Controls.Add(this.InactiveCountLabel);
			this.DbSplitter.Panel1.Controls.Add(this.LicenceActiveCheckBox);
			this.DbSplitter.Panel1.Controls.Add(this.GenerateLoginTokenButton);
			this.DbSplitter.Panel1.Controls.Add(this.GenerateDiagnosticsLoginTokenButton);
			this.DbSplitter.Panel1.Controls.Add(this.GenerateLoginTokenWithOldEntCodeButton);
			this.DbSplitter.Panel1.Controls.Add(this.DatabasesModuleButtonGrid);
			this.DbSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 461, true);
			this.DbSplitter.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(53);
			// 
			// DbSplitter.Panel2
			// 
			this.DbSplitter.Panel2.Controls.Add(this.LicenceTabControl);
			this.DbSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(112);
			this.DbSplitter.SplitterWidth = 8;
			this.DbSplitter.TabIndex = 1;
			// 
			// InactiveCountBox
			// 
			this.InactiveCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.InactiveCountBox, "LicCompany+InactiveDatabaseCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.MasterFiles.Business.EDIOrgHeader)(null)).LicCompany.InactiveDatabaseCount)));
			this.InactiveCountBox.CaptionResourceString = null;
			this.InactiveCountBox.DecimalPlaces = 0;
			this.InactiveCountBox.Decimals = 0;
			this.InactiveCountBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 91, true);
			this.InactiveCountBox.Name = "InactiveCountBox";
			this.InactiveCountBox.ReadOnly = true;
			this.InactiveCountBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 20, true);
			this.InactiveCountBox.TabIndex = 3;
			this.InactiveCountBox.Text = "0";
			this.InactiveCountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InactiveCountLabel
			// 
			this.InactiveCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.InactiveCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InactiveCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 93, true);
			this.InactiveCountLabel.Name = "InactiveCountLabel";
			this.InactiveCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 15, true);
			this.InactiveCountLabel.TabIndex = 2;
			this.InactiveCountLabel.Text = "# Inactive:";
			this.InactiveCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LicenceKeyBuilderControl
			// 
			this.Controls.Add(this.KeyBuilderPanel);
			this.IsModifyLicence = true;
			this.IsModifyLicence3rdPartySoftware = true;
			this.IsModifyLicenceConnectionDetails = true;
			this.IsModifyLicenceDatabaseConfiguration = true;
			this.IsModifyLicenceInstallationDetails = true;
			this.IsModifyLicenceLicenceKey = true;
			this.IsModifyLicenceSendUpgrade = true;
			this.IsModifyLicenceSendUpgradeToHigherRingDB = true;
			this.Name = "LicenceKeyBuilderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 608, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.KeyBuilderPanel, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExchangeRateGroupBox.ResumeLayout(false);
			this.ExchangeRateGroupBox.PerformLayout();
			this.GSTGroupBox.ResumeLayout(false);
			this.GSTGroupBox.PerformLayout();
			this.LicenceEnterpriseCodeFindBox.ResumeLayout(true);
			this.LicenceEnterpriseCodeFindBox.PerformLayout();
			this.LicenceInfoGroupBox.ResumeLayout(false);
			this.LicenceInfoGroupBox.PerformLayout();
			this.LicenceEnterpriseIDFindBox.ResumeLayout(true);
			this.LicenceEnterpriseIDFindBox.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.LicenceTabControl.ResumeLayout(false);
			this.LicenceTabControl.PerformLayout();
			this.CompaniesTabPage.ResumeLayout(false);
			this.CompaniesTabPage.PerformLayout();
			this.EditionDropEdit.ResumeLayout(true);
			this.EditionDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClientCompaniesGrid)).EndInit();
			this.ClientCompaniesGrid.ResumeLayout(false);
			this.ClientCompaniesGrid.PerformLayout();
			this.CompanyDetailsGroupBox.ResumeLayout(false);
			this.CompanyDetailsGroupBox.PerformLayout();
			this.LicencesTabPage.ResumeLayout(false);
			this.LicencesTabPage.PerformLayout();
			this.licenceModulesControl.ResumeLayout(true);
			this.licenceModulesControl.PerformLayout();
			this.Licence3rdPartySoftware.ResumeLayout(false);
			this.Licence3rdPartySoftware.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Licence3rdPartyGrid)).EndInit();
			this.Licence3rdPartyGrid.ResumeLayout(false);
			this.Licence3rdPartyGrid.PerformLayout();
			this.InstallationTabPage.ResumeLayout(false);
			this.InstallationTabPage.PerformLayout();
			this.installationUserControl.ResumeLayout(true);
			this.installationUserControl.PerformLayout();
			this.ProjectsTabPage.ResumeLayout(false);
			this.ProjectsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProjectsGrid)).EndInit();
			this.ProjectsGrid.ResumeLayout(false);
			this.ProjectsGrid.PerformLayout();
			this.TrainingSchedulesTabPage.ResumeLayout(false);
			this.TrainingSchedulesTabPage.PerformLayout();
			this.TrainingSchedulesPanel.ResumeLayout(false);
			this.TrainingSchedulesPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TrainingCourseGrid)).EndInit();
			this.TrainingCourseGrid.ResumeLayout(false);
			this.TrainingCourseGrid.PerformLayout();
			this.ConnectionsTabPage.ResumeLayout(false);
			this.ConnectionsTabPage.PerformLayout();
			this.ConnectionsPanel.ResumeLayout(false);
			this.ConnectionsPanel.PerformLayout();
			this.ConnectionsRichTextBox.ResumeLayout(true);
			this.ConnectionsRichTextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConnectionsGrid)).EndInit();
			this.ConnectionsGrid.ResumeLayout(false);
			this.ConnectionsGrid.PerformLayout();
			this.SupportAndContractTabPage.ResumeLayout(false);
			this.SupportAndContractTabPage.PerformLayout();
			this.supportContractUserControl.ResumeLayout(true);
			this.supportContractUserControl.PerformLayout();
			this.BillingInvoiceTabPage.ResumeLayout(false);
			this.BillingInvoiceTabPage.PerformLayout();
			this.BillingInvoicingControl.ResumeLayout(true);
			this.BillingInvoicingControl.PerformLayout();
			this.BillingPricesTabPage.ResumeLayout(false);
			this.BillingPricesTabPage.PerformLayout();
			this.BillingPricesControl.ResumeLayout(true);
			this.BillingPricesControl.PerformLayout();
			this.stlTabPage.ResumeLayout(false);
			this.stlTabPage.PerformLayout();
			this.stlLicenceControl.ResumeLayout(true);
			this.stlLicenceControl.PerformLayout();
			this.MaintenanceTabPage.ResumeLayout(false);
			this.MaintenanceTabPage.PerformLayout();
			this.licenceMaintenanceControl.ResumeLayout(true);
			this.licenceMaintenanceControl.PerformLayout();
			this.DiscountsTabPage.ResumeLayout(false);
			this.DiscountsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BillingDiscountsGrid)).EndInit();
			this.BillingDiscountsGrid.ResumeLayout(false);
			this.BillingDiscountsGrid.PerformLayout();
			this.FeesTabPage.ResumeLayout(false);
			this.FeesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).EndInit();
			this.FeesGrid.ResumeLayout(false);
			this.FeesGrid.PerformLayout();
			this.PremiumServicesTabPage.ResumeLayout(false);
			this.PremiumServicesTabPage.PerformLayout();
			this.clientPremiumServicesControl1.ResumeLayout(true);
			this.clientPremiumServicesControl1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabasesModuleButtonGrid.InnerGrid)).EndInit();
			this.DatabasesModuleButtonGrid.ResumeLayout(true);
			this.DatabasesModuleButtonGrid.PerformLayout();
			this.ActionButtonsPanel.ResumeLayout(false);
			this.ActionButtonsPanel.PerformLayout();
			this.LicenceKeyActionPanel.ResumeLayout(false);
			this.LicenceKeyActionPanel.PerformLayout();
			this.KeyBuilderPanel.ResumeLayout(false);
			this.KeyBuilderPanel.PerformLayout();
			this.DatabaseGroupBox.ResumeLayout(false);
			this.DatabaseGroupBox.PerformLayout();
			this.DbSplitter.Panel1.ResumeLayout(false);
			this.DbSplitter.Panel1.PerformLayout();
			this.DbSplitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DbSplitter)).EndInit();
			this.DbSplitter.ResumeLayout(false);
			this.DbSplitter.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private AlwaysActiveCheckBox ShowInactiveDatabasesCheckBox;
		private ZGrid Licence3rdPartyGrid;
		protected ZGrid ProjectsGrid;
		protected ZGrid TrainingCourseGrid;
		private ZGrid ConnectionsGrid;
		protected ModuleButtonGridForLicencing DatabasesModuleButtonGrid;
		private ZTabPage BillingInvoiceTabPage;
		private ZTabPage BillingPricesTabPage;
		private BillingInvoicingControl BillingInvoicingControl;
		public BillingPricesControl BillingPricesControl;
		private CargoWise.Windows.UI.KSplitContainer DbSplitter;
		protected ZTabPage ProjectsTabPage;
		private ZGroupBox DatabaseGroupBox;
		private ZLabel InactiveCountLabel;
		private ZCalcEdit InactiveCountBox;
		private ZCheckBox LicenceActiveCheckBox;
		private ZButton GenerateLoginTokenButton;
		private ZButton GenerateDiagnosticsLoginTokenButton;
		private ZButton GenerateLoginTokenWithOldEntCodeButton;
		private ZLabel zLabel5;
		private ZLabel zLabel6;
		private ZTabPage MaintenanceTabPage;
		private LicenceMaintenanceControl licenceMaintenanceControl;
		private ZTabPage DiscountsTabPage;
		private ZTabPage FeesTabPage;
		private ZGrid BillingDiscountsGrid;
		private ZGrid FeesGrid;
		private ZTabPage PremiumServicesTabPage;
		private ClientPremiumServicesControl clientPremiumServicesControl1;
		private LicenceModulesControl licenceModulesControl;
		private ZTabPage stlTabPage;
		private StlLicenceControl stlLicenceControl;
		private ZTabPage CompaniesTabPage;
		private ZGrid ClientCompaniesGrid;
		private ZGroupBox CompanyDetailsGroupBox;
		private ZTextBox CompanyAddress1Box;
		private ZTextBox CompanyStateTextBox;
		private ZTextBox CompanyCityTextBox;
		private ZTextBox CompanyAddress2TextBox;
		private ZTextBox CompanyPostCodeTextBox;
		private ZTextBox CompanyRegNoTextBox;
		private ZCheckBox ClientGstRegisteredCheckBox;
		private ZTextBox zTextBox1;
		private ZCheckBox ClientWhtCashBasisCheckBox;
		private ZCheckBox ClientWhtRegisteredCheckBox;
		private ZCheckBox ClientGstCashBasisCheckBox;
		private ZTextBox ClientWebAddressTextBox;
		private ZTextBox ClientPhoneTextBox;
		private ZTextBox CompanyCountryTextBox;
		public ZLabel productBox;
		private ZLabel productLabel;
		private ZDropEdit EditionDropEdit;
		public ZLabel editionCommentBox;
		private ZCheckBox ClientReciprocalCheckBox;
		private ZCodeFindBox LicenceEnterpriseIDFindBox;
		private SupportContractControl supportContractUserControl;
		private InstallationControl installationUserControl;
	}
}

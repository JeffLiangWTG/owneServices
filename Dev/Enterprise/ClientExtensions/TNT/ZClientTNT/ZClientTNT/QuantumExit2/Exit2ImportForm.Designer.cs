using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT
{
	public partial class Exit2ImportForm : ZChildForm
	{
		private Enterprise.ZArchitecture.GUI.ZButton MatchConsolButton;
		protected Enterprise.ZArchitecture.ZGrid EnterpriseConsolsGrid;
		private Enterprise.ZArchitecture.ZLabel QuantumMawbsLabel;
		private Enterprise.ZArchitecture.ZLabel EnterpriseConsolsLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZLabel Exit2FileNameLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton ProcessButton;
		private Enterprise.ZArchitecture.ZTextBox Search_VoyageFlightTextEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit Search_EtdDateTimeEdit;
		private Enterprise.ZArchitecture.ZTextBox Search_MasterBillNumTextBox;
		private Enterprise.ZArchitecture.ZLabel Search_VoyageFlightLabel;
		private Enterprise.ZArchitecture.ZLabel Search_EtdLabel;
		private Enterprise.ZArchitecture.ZLabel Search_MasterBillNumLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfLoadingFindBox;
		private Enterprise.ZArchitecture.ZLabel Search_PortOfLoadingLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfDischargeFindBox;
		private Enterprise.ZArchitecture.ZLabel Search_PortOfDischargeLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SearchGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton ConsolSearchButton;
		private Enterprise.ZArchitecture.GUI.ZButton UnmatchConsolButton;
		public Enterprise.ZArchitecture.ZGrid QuantumMawbsGrid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MatchConsolButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QuantumMawbsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EnterpriseConsolsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuantumMawbsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseConsolsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Exit2FileNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProcessButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SearchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConsolSearchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Search_VoyageFlightTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.Search_EtdDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Search_MasterBillNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Search_VoyageFlightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_EtdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_MasterBillNumLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_PortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Search_PortOfLoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_PortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Search_PortOfDischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnmatchConsolButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantumMawbsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseConsolsGrid)).BeginInit();
			this.SearchGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 26, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.TNT.Exit2ImportManager);
			// 
			// MatchConsolButton
			// 
			this.MatchConsolButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MatchConsolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 528, true);
			this.MatchConsolButton.Name = "MatchConsolButton";
			this.MatchConsolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.MatchConsolButton.TabIndex = 6;
			this.MatchConsolButton.Text = "&Match";
			this.MatchConsolButton.Click += new System.EventHandler(this.MatchConsolButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 528, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CloseButton.TabIndex = 9;
			this.CloseButton.Text = "&Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// QuantumMawbsGrid
			// 
			this.QuantumMawbsGrid.AllowNavigation = false;
			this.QuantumMawbsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QuantumMawbsGrid, "Exit2Mawbs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MasterBillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).DepartureDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).FlightNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).PortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).LinkedConsolUniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).RelatedShipmentsInFile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).ULDRequired)));
			this.QuantumMawbsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Master Bill";
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|f718895c-d107-4cbc-9d9d-30201ab63d80", "Master Bill");
			zTextBoxColumnStyleInfo1.ColumnName = "MasterBillNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.Caption = "ETD";
			zDateEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|f9436f94-8f9d-4de5-92b0-8e5b86f98946", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "DepartureDate";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Flight";
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|264910ca-192f-4589-a9f6-acb5950cfa19", "Flight");
			zTextBoxColumnStyleInfo2.ColumnName = "FlightNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Caption = "Load";
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|a4c3e299-50b4-49d6-a936-539b37d35ba1", "Load");
			zTextBoxColumnStyleInfo3.ColumnName = "PortOfLoading";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Caption = "Discharge";
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|c0660a9f-bc44-47a6-b9a7-972733e91dc5", "Discharge");
			zTextBoxColumnStyleInfo4.ColumnName = "PortOfDischarge";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Caption = "CargoWise One Consol";
			zTextBoxColumnStyleInfo5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|67ebfd26-9959-403c-9f41-8cde2a330fc5", "CargoWise One Consol");
			zTextBoxColumnStyleInfo5.ColumnName = "LinkedConsolUniqueConsignRef";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "NoOfHousebills";
			zCalcEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|de61155f-7fed-40e6-8c5b-858da93a650c", "No. of House Bills", "Number of House Bills.");
			zCalcEditColumnStyleInfo1.ColumnName = "RelatedShipmentsInFile";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.Caption = "ULD Req\'d";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("Exit2ImportForm|e99347d6-efc5-4134-a0f8-56cb8820ff8c", "ULD Req\'d");
			zCheckBoxColumnStyleInfo1.ColumnName = "ULDRequired";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			this.QuantumMawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.QuantumMawbsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.QuantumMawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.QuantumMawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.QuantumMawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.QuantumMawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.QuantumMawbsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.QuantumMawbsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.QuantumMawbsGrid.GridId = "d26b3677-e660-46f0-a4b8-ae6713528d20";
			this.QuantumMawbsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuantumMawbsGrid.LayoutKey = "QuantumMawbsGrid";
			this.QuantumMawbsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.QuantumMawbsGrid.Name = "QuantumMawbsGrid";
			this.QuantumMawbsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.QuantumMawbsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 216, true);
			this.QuantumMawbsGrid.TabIndex = 2;
			// 
			// EnterpriseConsolsGrid
			// 
			this.EnterpriseConsolsGrid.AllowNavigation = false;
			this.EnterpriseConsolsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EnterpriseConsolsGrid, "Exit2Mawbs.MatchingConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_MasterBillNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_JX_JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_JX_JV_VoyageFlight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_JX_JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.CommonConsol)(((System.Collections.IList)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).MatchingConsols)).SyncRoot)).JK_UniqueConsignRef)));
			this.EnterpriseConsolsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Master Bill";
			zTextBoxColumnStyleInfo6.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "ETD";
			zDateEditColumnStyleInfo2.ColumnName = "JK_JX_JA_E_DEP";
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "Flight";
			zTextBoxColumnStyleInfo7.ColumnName = "JK_JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.Caption = "Load";
			zTextBoxColumnStyleInfo8.ColumnName = "JK_JX_JA_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo8.IsMandatory = true;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.Caption = "Discharge";
			zTextBoxColumnStyleInfo9.ColumnName = "JK_JX_JB_RL_NKPortOfDischarge";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.Caption = "Consol ID";
			zTextBoxColumnStyleInfo10.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EnterpriseConsolsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.EnterpriseConsolsGrid.GridId = "067fabc4-d1fd-4a6d-8f92-25a8f2438053";
			this.EnterpriseConsolsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EnterpriseConsolsGrid.IsWholeRowSelectedOnClick = true;
			this.EnterpriseConsolsGrid.LayoutKey = "EnterpriseConsolsGrid";
			this.EnterpriseConsolsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 360, true);
			this.EnterpriseConsolsGrid.Name = "EnterpriseConsolsGrid";
			this.EnterpriseConsolsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EnterpriseConsolsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 160, true);
			this.EnterpriseConsolsGrid.TabIndex = 5;
			// 
			// QuantumMawbsLabel
			// 
			this.QuantumMawbsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.QuantumMawbsLabel.Name = "QuantumMawbsLabel";
			this.QuantumMawbsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.QuantumMawbsLabel.TabIndex = 0;
			this.QuantumMawbsLabel.Text = "Details in File";
			// 
			// EnterpriseConsolsLabel
			// 
			this.EnterpriseConsolsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EnterpriseConsolsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 255, true);
			this.EnterpriseConsolsLabel.Name = "EnterpriseConsolsLabel";
			this.EnterpriseConsolsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 15, true);
			this.EnterpriseConsolsLabel.TabIndex = 3;
			this.EnterpriseConsolsLabel.Text = "Existing Details in CargoWise One";
			// 
			// Exit2FileNameLabel
			// 
			this.BindingSource.SetBindingMember(this.Exit2FileNameLabel, "FileFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).FileFullName)));
			this.Exit2FileNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 3, true);
			this.Exit2FileNameLabel.Name = "Exit2FileNameLabel";
			this.Exit2FileNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 16, true);
			this.Exit2FileNameLabel.TabIndex = 1;
			this.Exit2FileNameLabel.Text = "FileName";
			// 
			// ProcessButton
			// 
			this.ProcessButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProcessButton.Enabled = false;
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 528, true);
			this.ProcessButton.Name = "ProcessButton";
			this.ProcessButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.ProcessButton.TabIndex = 8;
			this.ProcessButton.Text = "&Process && Close";
			this.ProcessButton.Click += new System.EventHandler(this.ProcessButton_Click);
			// 
			// SearchGroupBox
			// 
			this.SearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SearchGroupBox.Controls.Add(this.ConsolSearchButton);
			this.SearchGroupBox.Controls.Add(this.Search_VoyageFlightTextEdit);
			this.SearchGroupBox.Controls.Add(this.Search_EtdDateTimeEdit);
			this.SearchGroupBox.Controls.Add(this.Search_MasterBillNumTextBox);
			this.SearchGroupBox.Controls.Add(this.Search_VoyageFlightLabel);
			this.SearchGroupBox.Controls.Add(this.Search_EtdLabel);
			this.SearchGroupBox.Controls.Add(this.Search_MasterBillNumLabel);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfLoadingFindBox);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfLoadingLabel);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfDischargeFindBox);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfDischargeLabel);
			this.SearchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 276, true);
			this.SearchGroupBox.Name = "SearchGroupBox";
			this.SearchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 80, true);
			this.SearchGroupBox.TabIndex = 4;
			this.SearchGroupBox.TabStop = false;
			// 
			// ConsolSearchButton
			// 
			this.ConsolSearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolSearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(640, 13, true);
			this.ConsolSearchButton.Name = "ConsolSearchButton";
			this.ConsolSearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.ConsolSearchButton.TabIndex = 10;
			this.ConsolSearchButton.Text = "&Search";
			this.ConsolSearchButton.Click += new System.EventHandler(this.ConsolSearchButton_Click);
			// 
			// Search_VoyageFlightTextEdit
			// 
			this.BindingSource.SetBindingMember(this.Search_VoyageFlightTextEdit, "Exit2Mawbs.SearchFlightNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).SearchFlightNumber)));
			this.Search_VoyageFlightTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 14, true);
			this.Search_VoyageFlightTextEdit.Name = "Search_VoyageFlightTextEdit";
			this.Search_VoyageFlightTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Search_VoyageFlightTextEdit.TabIndex = 5;
			// 
			// Search_EtdDateTimeEdit
			// 
			this.Search_EtdDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.Search_EtdDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.Search_EtdDateTimeEdit, "Exit2Mawbs.SearchDepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).SearchDepartureDate)));
			this.Search_EtdDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 48, true);
			this.Search_EtdDateTimeEdit.Name = "Search_EtdDateTimeEdit";
			this.Search_EtdDateTimeEdit.TabIndex = 3;
			// 
			// Search_MasterBillNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.Search_MasterBillNumTextBox, "Exit2Mawbs.SearchMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).SearchMasterBill)));
			this.Search_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 14, true);
			this.Search_MasterBillNumTextBox.Name = "Search_MasterBillNumTextBox";
			this.Search_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Search_MasterBillNumTextBox.TabIndex = 1;
			// 
			// Search_VoyageFlightLabel
			// 
			this.Search_VoyageFlightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 16, true);
			this.Search_VoyageFlightLabel.Name = "Search_VoyageFlightLabel";
			this.Search_VoyageFlightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 16, true);
			this.Search_VoyageFlightLabel.TabIndex = 4;
			this.Search_VoyageFlightLabel.Text = "Flight:";
			// 
			// Search_EtdLabel
			// 
			this.Search_EtdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 50, true);
			this.Search_EtdLabel.Name = "Search_EtdLabel";
			this.Search_EtdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 16, true);
			this.Search_EtdLabel.TabIndex = 2;
			this.Search_EtdLabel.Text = "ETD:";
			// 
			// Search_MasterBillNumLabel
			// 
			this.Search_MasterBillNumLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.Search_MasterBillNumLabel.Name = "Search_MasterBillNumLabel";
			this.Search_MasterBillNumLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 16, true);
			this.Search_MasterBillNumLabel.TabIndex = 0;
			this.Search_MasterBillNumLabel.Text = "MasterBill:";
			// 
			// Search_PortOfLoadingFindBox
			// 
			this.BindingSource.SetBindingMember(this.Search_PortOfLoadingFindBox, "Exit2Mawbs.SearchPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).SearchPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfLoadingFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 14, true);
			this.Search_PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfLoadingFindBox.Name = "Search_PortOfLoadingFindBox";
			this.Search_PortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.Search_PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 21, true);
			this.Search_PortOfLoadingFindBox.TabIndex = 7;
			// 
			// Search_PortOfLoadingLabel
			// 
			this.Search_PortOfLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 16, true);
			this.Search_PortOfLoadingLabel.Name = "Search_PortOfLoadingLabel";
			this.Search_PortOfLoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 16, true);
			this.Search_PortOfLoadingLabel.TabIndex = 6;
			this.Search_PortOfLoadingLabel.Text = "Loading:";
			// 
			// Search_PortOfDischargeFindBox
			// 
			this.BindingSource.SetBindingMember(this.Search_PortOfDischargeFindBox, "Exit2Mawbs.SearchPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.QuantumMawb)(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).Exit2Mawbs)).SyncRoot)).SearchPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.Exit2ImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfDischargeFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 48, true);
			this.Search_PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfDischargeFindBox.Name = "Search_PortOfDischargeFindBox";
			this.Search_PortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.Search_PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.Search_PortOfDischargeFindBox.TabIndex = 9;
			// 
			// Search_PortOfDischargeLabel
			// 
			this.Search_PortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 50, true);
			this.Search_PortOfDischargeLabel.Name = "Search_PortOfDischargeLabel";
			this.Search_PortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 16, true);
			this.Search_PortOfDischargeLabel.TabIndex = 8;
			this.Search_PortOfDischargeLabel.Text = "Discharge:";
			// 
			// UnmatchConsolButton
			// 
			this.UnmatchConsolButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnmatchConsolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 528, true);
			this.UnmatchConsolButton.Name = "UnmatchConsolButton";
			this.UnmatchConsolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.UnmatchConsolButton.TabIndex = 7;
			this.UnmatchConsolButton.Text = "&Unmatch";
			this.UnmatchConsolButton.Click += new System.EventHandler(this.UnmatchConsolButton_Click);
			// 
			// Exit2ImportForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 589, true);
			this.Controls.Add(this.UnmatchConsolButton);
			this.Controls.Add(this.SearchGroupBox);
			this.Controls.Add(this.ProcessButton);
			this.Controls.Add(this.Exit2FileNameLabel);
			this.Controls.Add(this.EnterpriseConsolsLabel);
			this.Controls.Add(this.QuantumMawbsLabel);
			this.Controls.Add(this.EnterpriseConsolsGrid);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.MatchConsolButton);
			this.Controls.Add(this.QuantumMawbsGrid);
			this.DataSourceAssemblyName = "ZClientTNT";
			this.DataSourceType = typeof(Enterprise.Client.TNT.Exit2ImportManager);
			this.DataSourceTypeName = "Enterprise.Client.TNT.Exit2ImportManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 616, true);
			this.Name = "Exit2ImportForm";
			this.Text = "Quantum Exit2 Import";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Exit2ImportForm_Closing);
			this.Controls.SetChildIndex(this.QuantumMawbsGrid, 0);
			this.Controls.SetChildIndex(this.MatchConsolButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EnterpriseConsolsGrid, 0);
			this.Controls.SetChildIndex(this.QuantumMawbsLabel, 0);
			this.Controls.SetChildIndex(this.EnterpriseConsolsLabel, 0);
			this.Controls.SetChildIndex(this.Exit2FileNameLabel, 0);
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.SearchGroupBox, 0);
			this.Controls.SetChildIndex(this.UnmatchConsolButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantumMawbsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseConsolsGrid)).EndInit();
			this.SearchGroupBox.ResumeLayout(false);
			this.SearchGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

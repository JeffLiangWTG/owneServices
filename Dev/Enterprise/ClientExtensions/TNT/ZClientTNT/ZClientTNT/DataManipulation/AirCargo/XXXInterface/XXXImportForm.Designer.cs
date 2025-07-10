using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.TNT.AirCargo
{
	public partial class XXXImportForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel QuantumMawbsLabel;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.ZLabel INDFileNameLabel;
		Enterprise.ZArchitecture.ZTextBox Search_VoyageFlightTextEdit;
		Enterprise.ZArchitecture.ZMasterBillControl Search_MasterBillNumTextBox;
		Enterprise.ZArchitecture.ZLabel Search_VoyageFlightLabel;
		Enterprise.ZArchitecture.ZLabel Search_MasterBillNumLabel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfLoadingFindBox;
		Enterprise.ZArchitecture.ZLabel Search_PortOfLoadingLabel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfDischargeFindBox;
		Enterprise.ZArchitecture.ZLabel Search_PortOfDischargeLabel;
		Enterprise.ZArchitecture.GUI.ZGroupBox SearchGroupBox;
		public Enterprise.ZArchitecture.ZGrid MawbsGrid;
		Enterprise.ZArchitecture.ZGrid ExistingAirCargosGrid;
		Enterprise.ZArchitecture.GUI.ZDateEdit Search_EtaDateTimeEdit;
		Enterprise.ZArchitecture.ZLabel Search_EtaLabel;
		Enterprise.ZArchitecture.GUI.ZButton UpdateButton;
		Enterprise.ZArchitecture.GUI.ZButton AirCargoClearButton;
		Enterprise.ZArchitecture.GUI.ZButton AirCargoSearchButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MawbsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExistingAirCargosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuantumMawbsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.INDFileNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SearchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AirCargoClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AirCargoSearchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Search_VoyageFlightTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.Search_EtaDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Search_MasterBillNumTextBox = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.Search_VoyageFlightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_EtaLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_MasterBillNumLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_PortOfLoadingFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Search_PortOfLoadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Search_PortOfDischargeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Search_PortOfDischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MawbsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExistingAirCargosGrid)).BeginInit();
			this.SearchGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 525, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(312);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(313);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.XXXImportManager);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 496, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.Text = "&Close";
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MawbsGrid
			// 
			this.MawbsGrid.AllowNavigation = false;
			this.MawbsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MawbsGrid, "AirCargos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).Origin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).Destination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).Consignor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).PortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).LinkedHouseBillRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).MatchedMasterBill)));
			this.MawbsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "HouseBill";
			zTextBoxColumnStyleInfo1.ColumnName = "HouseBill";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = "Origin";
			zTextBoxColumnStyleInfo2.ColumnName = "Origin";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.Caption = "Destination";
			zTextBoxColumnStyleInfo3.ColumnName = "Destination";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.Caption = "Consignee";
			zTextBoxColumnStyleInfo4.ColumnName = "Consignee";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = "Consignor";
			zTextBoxColumnStyleInfo5.ColumnName = "Consignor";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Package";
			zCalcEditColumnStyleInfo1.ColumnName = "PackageCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "MasterBill";
			zTextBoxColumnStyleInfo6.ColumnName = "MasterBill";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.Caption = "Flight No";
			zTextBoxColumnStyleInfo7.ColumnName = "FlightNo";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.Caption = "Arrival Date";
			zDateEditColumnStyleInfo1.ColumnName = "ArrivalDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.Caption = "Load Port";
			zTextBoxColumnStyleInfo8.ColumnName = "PortOfLoading";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.Caption = "Discharge Port";
			zTextBoxColumnStyleInfo9.ColumnName = "PortOfDischarge";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo10.Caption = "HouseBill Ref";
			zTextBoxColumnStyleInfo10.ColumnName = "LinkedHouseBillRef";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.Caption = "Matched MasterBill";
			zTextBoxColumnStyleInfo11.ColumnName = "MatchedMasterBill";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MawbsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MawbsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MawbsGrid.GridId = "0d8c4657-8ec9-4d79-b575-e1f546cca4dd";
			this.MawbsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MawbsGrid.IsWholeRowSelectedOnClick = true;
			this.MawbsGrid.LayoutKey = "MawbsGrid";
			this.MawbsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.MawbsGrid.Name = "MawbsGrid";
			this.MawbsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MawbsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 236, true);
			this.MawbsGrid.TabIndex = 2;
			// 
			// ExistingAirCargosGrid
			// 
			this.ExistingAirCargosGrid.AllowNavigation = false;
			this.ExistingAirCargosGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExistingAirCargosGrid, "MatchingMasterbills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)).SyncRoot)).CM_MAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)).SyncRoot)).CM_FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)).SyncRoot)).CM_ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)).SyncRoot)).CM_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).MatchingMasterbills)).SyncRoot)).CM_RL_NKDischargePort)));
			this.ExistingAirCargosGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.Caption = "MasterBill";
			zTextBoxColumnStyleInfo12.ColumnName = "CM_MAWB";
			zTextBoxColumnStyleInfo12.IsMandatory = true;
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.Caption = "Flight No";
			zTextBoxColumnStyleInfo13.ColumnName = "CM_FlightNo";
			zTextBoxColumnStyleInfo13.IsMandatory = true;
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Caption = "Arrival Date";
			zDateEditColumnStyleInfo2.ColumnName = "CM_ArrivalDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Caption = "Load Port";
			zTextBoxColumnStyleInfo14.ColumnName = "CM_RL_NKLoadPort";
			zTextBoxColumnStyleInfo14.IsMandatory = true;
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Caption = "Discharge Port";
			zTextBoxColumnStyleInfo15.ColumnName = "CM_RL_NKDischargePort";
			zTextBoxColumnStyleInfo15.IsMandatory = true;
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ExistingAirCargosGrid.GridId = "46db97cf-96e2-49c6-bcb4-91953320523c";
			this.ExistingAirCargosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExistingAirCargosGrid.IsWholeRowSelectedOnClick = true;
			this.ExistingAirCargosGrid.LayoutKey = "ExistingAirCargosGrid";
			this.ExistingAirCargosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 340, true);
			this.ExistingAirCargosGrid.Name = "ExistingAirCargosGrid";
			this.ExistingAirCargosGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ExistingAirCargosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 148, true);
			this.ExistingAirCargosGrid.TabIndex = 4;
			// 
			// QuantumMawbsLabel
			// 
			this.QuantumMawbsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.QuantumMawbsLabel.Name = "QuantumMawbsLabel";
			this.QuantumMawbsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 21, true);
			this.QuantumMawbsLabel.TabIndex = 0;
			this.QuantumMawbsLabel.Text = "Details in File";
			// 
			// INDFileNameLabel
			// 
			this.INDFileNameLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.INDFileNameLabel, "FileFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).FileFullName)));
			this.INDFileNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 4, true);
			this.INDFileNameLabel.Name = "INDFileNameLabel";
			this.INDFileNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.INDFileNameLabel.TabIndex = 1;
			this.INDFileNameLabel.Text = "FileName";
			// 
			// SearchGroupBox
			// 
			this.SearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SearchGroupBox.Controls.Add(this.AirCargoClearButton);
			this.SearchGroupBox.Controls.Add(this.AirCargoSearchButton);
			this.SearchGroupBox.Controls.Add(this.Search_VoyageFlightTextEdit);
			this.SearchGroupBox.Controls.Add(this.Search_EtaDateTimeEdit);
			this.SearchGroupBox.Controls.Add(this.Search_MasterBillNumTextBox);
			this.SearchGroupBox.Controls.Add(this.Search_VoyageFlightLabel);
			this.SearchGroupBox.Controls.Add(this.Search_EtaLabel);
			this.SearchGroupBox.Controls.Add(this.Search_MasterBillNumLabel);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfLoadingFindBox);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfLoadingLabel);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfDischargeFindBox);
			this.SearchGroupBox.Controls.Add(this.Search_PortOfDischargeLabel);
			this.SearchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 258, true);
			this.SearchGroupBox.Name = "SearchGroupBox";
			this.SearchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 74, true);
			this.SearchGroupBox.TabIndex = 3;
			this.SearchGroupBox.TabStop = false;
			// 
			// AirCargoClearButton
			// 
			this.AirCargoClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AirCargoClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 15, true);
			this.AirCargoClearButton.Name = "AirCargoClearButton";
			this.AirCargoClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.AirCargoClearButton.TabIndex = 5;
			this.AirCargoClearButton.Text = "Cl&ear";
			this.AirCargoClearButton.Click += new System.EventHandler(this.AirCargoClearButton_Click);
			// 
			// AirCargoSearchButton
			// 
			this.AirCargoSearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AirCargoSearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 45, true);
			this.AirCargoSearchButton.Name = "AirCargoSearchButton";
			this.AirCargoSearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.AirCargoSearchButton.TabIndex = 6;
			this.AirCargoSearchButton.Text = "&Search";
			this.AirCargoSearchButton.Click += new System.EventHandler(this.AirCargoSearchButton_Click);
			// 
			// Search_VoyageFlightTextEdit
			// 
			this.BindingSource.SetBindingMember(this.Search_VoyageFlightTextEdit, "AirCargos.SearchFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).SearchFlightNo)));
			this.Search_VoyageFlightTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 13, true);
			this.Search_VoyageFlightTextEdit.Name = "Search_VoyageFlightTextEdit";
			this.Search_VoyageFlightTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Search_VoyageFlightTextEdit.TabIndex = 2;
			// 
			// Search_EtaDateTimeEdit
			// 
			this.Search_EtaDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.Search_EtaDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.Search_EtaDateTimeEdit, "AirCargos.SearchArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).SearchArrivalDate)));
			this.Search_EtaDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 45, true);
			this.Search_EtaDateTimeEdit.Name = "Search_EtaDateTimeEdit";
			this.Search_EtaDateTimeEdit.TabIndex = 1;
			// 
			// Search_MasterBillNumTextBox
			// 
			this.Search_MasterBillNumTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.Search_MasterBillNumTextBox, "AirCargos.SearchMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).SearchMasterBill)));
			this.Search_MasterBillNumTextBox.FormattedMasterBill = "";
			this.Search_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 13, true);
			this.Search_MasterBillNumTextBox.Name = "Search_MasterBillNumTextBox";
			this.Search_MasterBillNumTextBox.ReadOnly = false;
			this.Search_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Search_MasterBillNumTextBox.TabIndex = 0;
			// 
			// Search_VoyageFlightLabel
			// 
			this.Search_VoyageFlightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 15, true);
			this.Search_VoyageFlightLabel.Name = "Search_VoyageFlightLabel";
			this.Search_VoyageFlightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 15, true);
			this.Search_VoyageFlightLabel.TabIndex = 2;
			this.Search_VoyageFlightLabel.Text = "Flight:";
			// 
			// Search_EtaLabel
			// 
			this.Search_EtaLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 46, true);
			this.Search_EtaLabel.Name = "Search_EtaLabel";
			this.Search_EtaLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 15, true);
			this.Search_EtaLabel.TabIndex = 4;
			this.Search_EtaLabel.Text = "ETA:";
			// 
			// Search_MasterBillNumLabel
			// 
			this.Search_MasterBillNumLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.Search_MasterBillNumLabel.Name = "Search_MasterBillNumLabel";
			this.Search_MasterBillNumLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 15, true);
			this.Search_MasterBillNumLabel.TabIndex = 0;
			this.Search_MasterBillNumLabel.Text = "MasterBill:";
			// 
			// Search_PortOfLoadingFindBox
			// 
			this.BindingSource.SetBindingMember(this.Search_PortOfLoadingFindBox, "AirCargos.SearchPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).SearchPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfLoadingFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 13, true);
			this.Search_PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfLoadingFindBox.Name = "Search_PortOfLoadingFindBox";
			this.Search_PortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.Search_PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.Search_PortOfLoadingFindBox.TabIndex = 3;
			// 
			// Search_PortOfLoadingLabel
			// 
			this.Search_PortOfLoadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 15, true);
			this.Search_PortOfLoadingLabel.Name = "Search_PortOfLoadingLabel";
			this.Search_PortOfLoadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 15, true);
			this.Search_PortOfLoadingLabel.TabIndex = 6;
			this.Search_PortOfLoadingLabel.Text = "Loading:";
			// 
			// Search_PortOfDischargeFindBox
			// 
			this.BindingSource.SetBindingMember(this.Search_PortOfDischargeFindBox, "AirCargos.SearchPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.XXXAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).AirCargos)).SyncRoot)).SearchPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.XXXImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfDischargeFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 45, true);
			this.Search_PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfDischargeFindBox.Name = "Search_PortOfDischargeFindBox";
			this.Search_PortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.Search_PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.Search_PortOfDischargeFindBox.TabIndex = 4;
			// 
			// Search_PortOfDischargeLabel
			// 
			this.Search_PortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 46, true);
			this.Search_PortOfDischargeLabel.Name = "Search_PortOfDischargeLabel";
			this.Search_PortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.Search_PortOfDischargeLabel.TabIndex = 8;
			this.Search_PortOfDischargeLabel.Text = "Discharge:";
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 496, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.UpdateButton.TabIndex = 5;
			this.UpdateButton.Text = "&Update";
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// XXXImportForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 549, true);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.SearchGroupBox);
			this.Controls.Add(this.INDFileNameLabel);
			this.Controls.Add(this.QuantumMawbsLabel);
			this.Controls.Add(this.ExistingAirCargosGrid);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.MawbsGrid);
			this.DataSourceAssemblyName = "ZClientTNT";
			this.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.XXXImportManager);
			this.DataSourceTypeName = "Enterprise.Client.TNT.AirCargo.XXXImportManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 479, true);
			this.Name = "XXXImportForm";
			this.Text = "Quantum XXX Import";
			this.Closed += new System.EventHandler(this.XXXImportForm_Closed);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.XXXImportForm_Closing);
			this.Controls.SetChildIndex(this.MawbsGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ExistingAirCargosGrid, 0);
			this.Controls.SetChildIndex(this.QuantumMawbsLabel, 0);
			this.Controls.SetChildIndex(this.INDFileNameLabel, 0);
			this.Controls.SetChildIndex(this.SearchGroupBox, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MawbsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ExistingAirCargosGrid)).EndInit();
			this.SearchGroupBox.ResumeLayout(false);
			this.SearchGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
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

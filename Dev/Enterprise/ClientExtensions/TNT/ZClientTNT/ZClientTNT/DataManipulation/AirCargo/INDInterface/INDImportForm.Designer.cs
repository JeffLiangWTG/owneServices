using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.AirCargo
{
	public partial class INDImportForm : ZChildForm
	{
		private Enterprise.ZArchitecture.ZLabel QuantumMawbsLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZLabel INDFileNameLabel;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateButton;
		private Enterprise.ZArchitecture.ZTextBox Search_VoyageFlightTextEdit;
		private Enterprise.ZArchitecture.ZMasterBillControl Search_MasterBillNumTextBox;
		private Enterprise.ZArchitecture.ZLabel Search_VoyageFlightLabel;
		private Enterprise.ZArchitecture.ZLabel Search_MasterBillNumLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfLoadingFindBox;
		private Enterprise.ZArchitecture.ZLabel Search_PortOfLoadingLabel;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox Search_PortOfDischargeFindBox;
		private Enterprise.ZArchitecture.ZLabel Search_PortOfDischargeLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SearchGroupBox;
		public Enterprise.ZArchitecture.ZGrid MawbsGrid;
		private Enterprise.ZArchitecture.ZGrid ExistingAirCargosGrid;
		private Enterprise.ZArchitecture.GUI.ZDateEdit Search_EtaDateTimeEdit;
		private Enterprise.ZArchitecture.ZLabel Search_EtaLabel;
		private Enterprise.ZArchitecture.GUI.ZButton AddButton;
		private Enterprise.ZArchitecture.GUI.ZButton UpdateIQDownButton;
		private Enterprise.ZArchitecture.GUI.ZButton AirCargoSearchButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MawbsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExistingAirCargosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.QuantumMawbsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.INDFileNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SearchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UpdateIQDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
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
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.IQDownImportManager);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(552, 496, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.CloseButton.TabIndex = 7;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).PortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).PortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).NoOfHouseBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).LinkedMasterBillRef)));
			this.MawbsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Master Bill";
			zTextBoxColumnStyleInfo1.ColumnName = "MasterBill";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Flight No";
			zTextBoxColumnStyleInfo2.ColumnName = "FlightNo";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.Caption = "Arrival Date";
			zDateEditColumnStyleInfo1.ColumnName = "ArrivalDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo3.Caption = "Load";
			zTextBoxColumnStyleInfo3.ColumnName = "PortOfLoading";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo4.Caption = "Discharge";
			zTextBoxColumnStyleInfo4.ColumnName = "PortOfDischarge";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "No Of House Bills";
			zCalcEditColumnStyleInfo1.ColumnName = "NoOfHouseBills";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Linked Air Cargo Ref";
			zTextBoxColumnStyleInfo5.ColumnName = "LinkedMasterBillRef";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MawbsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MawbsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.MawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MawbsGrid.GridId = "ab76cf89-22d5-401f-a14f-21ee4e705c1f";
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
			this.BindingSource.SetBindingMember(this.ExistingAirCargosGrid, "AirCargos.MatchingAirCargos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)).SyncRoot)).CM_MAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)).SyncRoot)).CM_FlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)).SyncRoot)).CM_ArrivalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)).SyncRoot)).CM_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusMAWB)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).MatchingAirCargos)).SyncRoot)).CM_RL_NKDischargePort)));
			this.ExistingAirCargosGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Master Bill";
			zTextBoxColumnStyleInfo6.ColumnName = "CM_MAWB";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "Flight No";
			zTextBoxColumnStyleInfo7.ColumnName = "CM_FlightNo";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "Arrival Date";
			zDateEditColumnStyleInfo2.ColumnName = "CM_ArrivalDate";
			zDateEditColumnStyleInfo2.IsMandatory = true;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.Caption = "Load";
			zTextBoxColumnStyleInfo8.ColumnName = "CM_RL_NKLoadPort";
			zTextBoxColumnStyleInfo8.IsMandatory = true;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Caption = "Discharge";
			zTextBoxColumnStyleInfo9.ColumnName = "CM_RL_NKDischargePort";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ExistingAirCargosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ExistingAirCargosGrid.GridId = "a2add34b-febe-41f0-bc9b-f96f7e17860f";
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).FileFullName)));
			this.INDFileNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 4, true);
			this.INDFileNameLabel.Name = "INDFileNameLabel";
			this.INDFileNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.INDFileNameLabel.TabIndex = 1;
			this.INDFileNameLabel.Text = "FileName";
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 496, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.UpdateButton.TabIndex = 5;
			this.UpdateButton.Text = "&Update";
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// SearchGroupBox
			// 
			this.SearchGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SearchGroupBox.Controls.Add(this.UpdateIQDownButton);
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
			// UpdateIQDownButton
			// 
			this.UpdateIQDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateIQDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 13, true);
			this.UpdateIQDownButton.Name = "UpdateIQDownButton";
			this.UpdateIQDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.UpdateIQDownButton.TabIndex = 10;
			this.UpdateIQDownButton.Text = "Update &IQDown";
			this.UpdateIQDownButton.Click += new System.EventHandler(this.UpdateIQDownButton_Click);
			// 
			// AirCargoSearchButton
			// 
			this.AirCargoSearchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AirCargoSearchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 45, true);
			this.AirCargoSearchButton.Name = "AirCargoSearchButton";
			this.AirCargoSearchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.AirCargoSearchButton.TabIndex = 11;
			this.AirCargoSearchButton.Text = "&Search";
			this.AirCargoSearchButton.Click += new System.EventHandler(this.AirCargoSearchButton_Click);
			// 
			// Search_VoyageFlightTextEdit
			// 
			this.BindingSource.SetBindingMember(this.Search_VoyageFlightTextEdit, "AirCargos.SearchFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).SearchFlightNo)));
			this.Search_VoyageFlightTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 13, true);
			this.Search_VoyageFlightTextEdit.Name = "Search_VoyageFlightTextEdit";
			this.Search_VoyageFlightTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.Search_VoyageFlightTextEdit.TabIndex = 3;
			// 
			// Search_EtaDateTimeEdit
			// 
			this.Search_EtaDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.Search_EtaDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.Search_EtaDateTimeEdit, "AirCargos.SearchArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).SearchArrivalDate)));
			this.Search_EtaDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 45, true);
			this.Search_EtaDateTimeEdit.Name = "Search_EtaDateTimeEdit";
			this.Search_EtaDateTimeEdit.TabIndex = 5;
			// 
			// Search_MasterBillNumTextBox
			// 
			this.Search_MasterBillNumTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.Search_MasterBillNumTextBox, "AirCargos.SearchMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).SearchMasterBill)));
			this.Search_MasterBillNumTextBox.FormattedMasterBill = "";
			this.Search_MasterBillNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 13, true);
			this.Search_MasterBillNumTextBox.Name = "Search_MasterBillNumTextBox";
			this.Search_MasterBillNumTextBox.ReadOnly = false;
			this.Search_MasterBillNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Search_MasterBillNumTextBox.TabIndex = 1;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).SearchPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfLoadingFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfLoadingFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 13, true);
			this.Search_PortOfLoadingFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfLoadingFindBox.Name = "Search_PortOfLoadingFindBox";
			this.Search_PortOfLoadingFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfLoadingFindBox.ShowDescriptionBox = false;
			this.Search_PortOfLoadingFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.Search_PortOfLoadingFindBox.TabIndex = 7;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.TNT.AirCargo.IQDownAirCargo)(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).AirCargos)).SyncRoot)).SearchPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.TNT.AirCargo.IQDownImportManager)(null)).RefUNLOCO_List)));
			this.Search_PortOfDischargeFindBox.BindToList = "RefUNLOCO_List";
			this.Search_PortOfDischargeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 45, true);
			this.Search_PortOfDischargeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Search_PortOfDischargeFindBox.Name = "Search_PortOfDischargeFindBox";
			this.Search_PortOfDischargeFindBox.PreBoundMaxLength = 5;
			this.Search_PortOfDischargeFindBox.ShowDescriptionBox = false;
			this.Search_PortOfDischargeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.Search_PortOfDischargeFindBox.TabIndex = 9;
			// 
			// Search_PortOfDischargeLabel
			// 
			this.Search_PortOfDischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 46, true);
			this.Search_PortOfDischargeLabel.Name = "Search_PortOfDischargeLabel";
			this.Search_PortOfDischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 15, true);
			this.Search_PortOfDischargeLabel.TabIndex = 8;
			this.Search_PortOfDischargeLabel.Text = "Discharge:";
			// 
			// AddButton
			// 
			this.AddButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 496, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 21, true);
			this.AddButton.TabIndex = 6;
			this.AddButton.Text = "&Add";
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// INDImportForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 549, true);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.SearchGroupBox);
			this.Controls.Add(this.UpdateButton);
			this.Controls.Add(this.INDFileNameLabel);
			this.Controls.Add(this.QuantumMawbsLabel);
			this.Controls.Add(this.ExistingAirCargosGrid);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.MawbsGrid);
			this.DataSourceAssemblyName = "ZClientTNT";
			this.DataSourceType = typeof(Enterprise.Client.TNT.AirCargo.IQDownImportManager);
			this.DataSourceTypeName = "Enterprise.Client.TNT.AirCargo.IQDownImportManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 479, true);
			this.Name = "INDImportForm";
			this.Text = "Quantum IND Import";
			this.Closed += new System.EventHandler(this.INDImportForm_Closed);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.INDImportForm_Closing);
			this.Controls.SetChildIndex(this.MawbsGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ExistingAirCargosGrid, 0);
			this.Controls.SetChildIndex(this.QuantumMawbsLabel, 0);
			this.Controls.SetChildIndex(this.INDFileNameLabel, 0);
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.SearchGroupBox, 0);
			this.Controls.SetChildIndex(this.AddButton, 0);
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

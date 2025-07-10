namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class NonPersistentSplitCollectionUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FcsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GenralButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FRDButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveSplitsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FlightDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FlightArrivalDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SendFlightInfoTooCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoadFrdButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.totalPicesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitsGrid)).BeginInit();
			this.FlightDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData);
			// 
			// SplitsGrid
			// 
			this.SplitsGrid.AllowNavigation = false;
			this.SplitsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SplitsGrid, "SplitLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).SplitNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).NumberOfPieces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).NumberOfPiecesReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).HandlingDetail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).WarehouseLocationID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitLine)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SplitLines)).SyncRoot)).CustomsActionCode)));
			this.SplitsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9cf46880-a5e7-4442-91e9-7582fea3a5d2", "Split Reference Number");
			zTextBoxColumnStyleInfo1.ColumnName = "SplitNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("748c2504-1f03-408b-90aa-8c9a050f8b96", "NPX");
			zCalcEditColumnStyleInfo1.ColumnName = "NumberOfPieces";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("395a1000-3299-4d3c-a8fe-28c14a52b439", "NPR");
			zCalcEditColumnStyleInfo2.ColumnName = "NumberOfPiecesReceived";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("69e1ed59-1fe9-4744-aa6a-73784b5346fd", "Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "Weight";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3ea9275a-9be8-4f16-ad6c-fec4d8901c5e", "Weight Unit");
			zTextBoxColumnStyleInfo2.ColumnName = "WeightUQ";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f004e131-a320-4779-bba7-dd127036af11", "Marks & Numbers (Handling Detail)");
			zTextBoxColumnStyleInfo3.ColumnName = "HandlingDetail";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("924fe890-89c4-4be2-8011-c1785d0f88ec", "SSL");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WarehouseLocationID";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigLocation;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("58171173-47ad-41d3-a323-f2975aa5db8c", "CAC", "Action", "Customs Action Code");
			zTextBoxColumnStyleInfo4.ColumnName = "CustomsActionCode";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.SplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SplitsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SplitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SplitsGrid.CopySelectedRowsAllowed = true;
			this.SplitsGrid.GridId = "65d7b8b1-6ddf-4453-86b1-d7446c13a839";
			this.SplitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SplitsGrid.LayoutKey = "zGrid1";
			this.SplitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.SplitsGrid.Name = "SplitsGrid";
			this.SplitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 184, true);
			this.SplitsGrid.TabIndex = 0;
			// 
			// FcsButton
			// 
			this.FcsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FcsButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1223541f-b3f2-41c4-9579-7ae1f6dce8e0", "Send FCS");
			this.FcsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 308, true);
			this.FcsButton.Name = "FcsButton";
			this.FcsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.FcsButton.TabIndex = 21;
			this.FcsButton.UseVisualStyleBackColor = true;
			this.FcsButton.Click += new System.EventHandler(this.FcsButton_Click);
			// 
			// GenralButton
			// 
			this.GenralButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.GenralButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7715b551-6952-4cd4-b5fc-cd4e93303bd1", "GENRAL message");
			this.GenralButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 308, true);
			this.GenralButton.Name = "GenralButton";
			this.GenralButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.GenralButton.TabIndex = 32;
			this.GenralButton.UseVisualStyleBackColor = true;
			this.GenralButton.Click += new System.EventHandler(this.GenralButton_Click);
			// 
			// FRDButton
			// 
			this.FRDButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FRDButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("149a4340-19cf-42d5-add1-f9291052153d", "Send FRD");
			this.FRDButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 308, true);
			this.FRDButton.Name = "FRDButton";
			this.FRDButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.FRDButton.TabIndex = 22;
			this.FRDButton.UseVisualStyleBackColor = true;
			this.FRDButton.Click += new System.EventHandler(this.FRDButton_Click);
			// 
			// RemoveSplitsButton
			// 
			this.RemoveSplitsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RemoveSplitsButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("a2fc262b-18e9-4afa-8232-47b0c927229b", "Remove All Splits");
			this.RemoveSplitsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 308, true);
			this.RemoveSplitsButton.Name = "RemoveSplitsButton";
			this.RemoveSplitsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 23, true);
			this.RemoveSplitsButton.TabIndex = 50;
			this.RemoveSplitsButton.UseVisualStyleBackColor = true;
			this.RemoveSplitsButton.Click += new System.EventHandler(this.RemoveSplitsButton_Click);
			// 
			// FlightDetailsPanel
			// 
			this.FlightDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FlightDetailsPanel.Controls.Add(this.totalPicesTextBox);
			this.FlightDetailsPanel.Controls.Add(this.FlightArrivalDateDateEdit);
			this.FlightDetailsPanel.Controls.Add(this.SendFlightInfoTooCheckBox);
			this.FlightDetailsPanel.Controls.Add(this.FlightNumberTextBox);
			this.FlightDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 197, true);
			this.FlightDetailsPanel.Name = "FlightDetailsPanel";
			this.FlightDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 92, true);
			this.FlightDetailsPanel.TabIndex = 1;
			// 
			// FlightArrivalDateDateEdit
			// 
			this.FlightArrivalDateDateEdit.AllowDrop = true;
			this.FlightArrivalDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.FlightArrivalDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FlightArrivalDateDateEdit, "FlightArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).FlightArrivalDate)));
			this.FlightArrivalDateDateEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NonPersistentSplitCollectionUserControl|41c5e1d6-358e-4aa6-a892-fa1d92f1c14b", "Flight Arrival Date");
			this.FlightArrivalDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 62, true);
			this.FlightArrivalDateDateEdit.Name = "FlightArrivalDateDateEdit";
			this.FlightArrivalDateDateEdit.TabIndex = 3;
			// 
			// SendFlightInfoTooCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendFlightInfoTooCheckBox, "SendFlightInfoToo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).SendFlightInfoToo)));
			this.SendFlightInfoTooCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NonPersistentSplitCollectionUserControl|0f608568-3f45-4898-910e-8fa8623fd870", "Allocate NPR using flight data", "Tick this box once splits ALREADY exist at the shed, to indicate to the shed that you are advising which pieces go with which splits. When ticked, you cannot add or remove splits.");
			this.SendFlightInfoTooCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SendFlightInfoTooCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SendFlightInfoTooCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 6, true);
			this.SendFlightInfoTooCheckBox.Name = "SendFlightInfoTooCheckBox";
			this.SendFlightInfoTooCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 24, true);
			this.SendFlightInfoTooCheckBox.TabIndex = 10;
			this.SendFlightInfoTooCheckBox.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.SendFlightInfoTooCheckBox.TabIndex = 0;
			this.SendFlightInfoTooCheckBox.UseVisualStyleBackColor = true;
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNumberTextBox, "FlightNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).FlightNumber)));
			this.FlightNumberTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NonPersistentSplitCollectionUserControl|aa38043f-7c55-4b16-99a0-03d5de953660", "Flight Number");
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 36, true);
			this.FlightNumberTextBox.Name = "FlightNumberTextBox";
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.FlightNumberTextBox.TabIndex = 2;
			// 
			// totalPicesTextBox
			// 
			this.BindingSource.SetBindingMember(this.totalPicesTextBox, "TotalPieces");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentSplitsAndFlightData)(null)).TotalPieces)));
			this.totalPicesTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9fb2b882-0079-4dfe-b702-3a77a3f7ebe2", "Pieces");
			this.totalPicesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 9, true);
			this.totalPicesTextBox.Name = "totalPicesTextBox";
			this.totalPicesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.totalPicesTextBox.TabIndex = 1;
			// 
			// LoadFrdButton
			// 
			this.LoadFrdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.LoadFrdButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("8efe9ada-c485-4431-890c-de77de370e41", "Load last FRD");
			this.LoadFrdButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 308, true);
			this.LoadFrdButton.Name = "LoadFrdButton";
			this.LoadFrdButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 23, true);
			this.LoadFrdButton.TabIndex = 31;
			this.LoadFrdButton.UseVisualStyleBackColor = true;
			this.LoadFrdButton.Click += new System.EventHandler(this.LoadFrdButton_Click);
			// 
			// NonPersistentSplitCollectionUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlightDetailsPanel);
			this.Controls.Add(this.RemoveSplitsButton);
			this.Controls.Add(this.FRDButton);
			this.Controls.Add(this.GenralButton);
			this.Controls.Add(this.FcsButton);
			this.Controls.Add(this.SplitsGrid);
			this.Controls.Add(this.LoadFrdButton);
			this.Name = "NonPersistentSplitCollectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 337, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitsGrid)).EndInit();
			this.FlightDetailsPanel.ResumeLayout(false);
			this.FlightDetailsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid SplitsGrid;
		private ZArchitecture.GUI.ZButton FcsButton;
		private ZArchitecture.GUI.ZButton GenralButton;
		private ZArchitecture.GUI.ZButton FRDButton;
		private ZArchitecture.GUI.ZButton RemoveSplitsButton;
		private ZArchitecture.GUI.ZPanel FlightDetailsPanel;
		private ZArchitecture.GUI.ZCheckBox SendFlightInfoTooCheckBox;
		private ZArchitecture.ZTextBox FlightNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit FlightArrivalDateDateEdit;
		private ZArchitecture.GUI.ZButton LoadFrdButton;
		private ZArchitecture.ZTextBox totalPicesTextBox;
	}
}

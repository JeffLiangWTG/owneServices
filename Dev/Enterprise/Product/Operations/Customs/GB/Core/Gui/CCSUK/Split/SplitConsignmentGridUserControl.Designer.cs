namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class SplitConsignmentGridUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.splitConsignmentModuleButtonGrid = new SplitConsignmentModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitConsignmentModuleButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb);
			// 
			// SplitConsignmentModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.splitConsignmentModuleButtonGrid, "Splits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).SplitReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).WeightCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).NumberOfPiecesExpected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).NumberOfPiecesReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).CustomsActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).LatestCustomsActionText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).HandlingInformationForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).ChiefDeclarationUCR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).AgentBadge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.SplitConsignment)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.ICcsukCusAwb)(null)).Splits)).SyncRoot)).TemporaryStorageEndDate)));

			this.splitConsignmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			splitConsignmentModuleButtonGrid.ModuleID = this.ModuleId;			
			zTextBoxColumnStyleInfo1.Caption = "Split #";
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "SplitReference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Weight";
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Weight";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zTextBoxColumnStyleInfo2.Caption = "Weight Unit";
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "WeightCode";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "NPX";
			zCalcEditColumnStyleInfo2.CaptionResourceString = null;
			zCalcEditColumnStyleInfo2.ColumnName = "NumberOfPiecesExpected";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "NPR";
			zCalcEditColumnStyleInfo3.CaptionResourceString = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NumberOfPiecesReceived";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.Caption = "CAC";
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "CustomsActionCode";
			zTextBoxColumnStyleInfo7.Caption = "Agent";
			zTextBoxColumnStyleInfo7.CaptionResourceString = null;
			zTextBoxColumnStyleInfo7.ColumnName = "AgentBadge";
			zTextBoxColumnStyleInfo8.Caption = "Presence";
			zTextBoxColumnStyleInfo8.CaptionResourceString = null;
			zTextBoxColumnStyleInfo8.ColumnName = "PresenceOnNetworkStatus";
			zTextBoxColumnStyleInfo9.Caption = "Temporary Storage End Date";
			zTextBoxColumnStyleInfo9.CaptionResourceString = null;
			zTextBoxColumnStyleInfo9.ColumnName = "TemporaryStorageEndDate";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);


			zTextBoxColumnStyleInfo4.Caption = "CAT";
			zTextBoxColumnStyleInfo4.CaptionResourceString = null;
			zTextBoxColumnStyleInfo4.ColumnName = "LatestCustomsActionText";
			zTextBoxColumnStyleInfo5.Caption = "Marks & Numbers (Handling Information)";
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.ColumnName = "HandlingInformationForBinding";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d491d7b5-4d7c-44df-9bfe-a100555f2e6f", "DUCR", "Chief Declaration UCR");
			zTextBoxColumnStyleInfo6.ColumnName = "ChiefDeclarationUCR";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.splitConsignmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.splitConsignmentModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.splitConsignmentModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitConsignmentModuleButtonGrid.GridId = "8c70ac00-bbc3-42f5-9bc5-0bfcaaee54aa";
			this.splitConsignmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.splitConsignmentModuleButtonGrid.InnerGrid.LayoutKey = "ccsukSplitsGrid";
			this.splitConsignmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitConsignmentModuleButtonGrid.Name = "zGrid1";
			this.splitConsignmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 150, true);
			this.splitConsignmentModuleButtonGrid.TabIndex = 0; 
			// 
			// SplitConsignmentGridUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.splitConsignmentModuleButtonGrid);
			this.Name = "SplitConsignmentGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.splitConsignmentModuleButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}


		#endregion

		internal SplitConsignmentModuleButtonGrid splitConsignmentModuleButtonGrid;
	}
}

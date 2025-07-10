namespace Enterprise.Customs.MX.GUI
{
	partial class ClearanceUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ClearanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClearenceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClearenceGrid)).BeginInit();
			this.ClearenceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Business.CusEntryInstruction);
			// 
			// ClearanceGroupBox
			// 
			this.ClearanceGroupBox.CaptionResourceString = Enterprise.Customs.MX.GUI.Res.GetData("9fd5d65f-d324-42ac-b0ba-35e6351180de", "Clearance");
			this.ClearanceGroupBox.Controls.Add(this.ClearenceGrid);
			this.ClearanceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClearanceGroupBox.Name = "ClearanceGroupBox";
			this.ClearanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 373, true);
			this.ClearanceGroupBox.TabIndex = 0;
			this.ClearanceGroupBox.TabStop = false;
			// 
			// ClearenceGrid
			// 
			this.ClearenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ClearenceGrid, "Clearances");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_CustomsOffice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Business.Clearance)(((System.Collections.IList)(((Enterprise.Customs.MX.Business.CusEntryInstruction)(null)).Clearances)).SyncRoot)).CSI_ReferenceNumber2)));
			this.ClearenceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_CustomsOffice";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_SubType";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "CSI_Tariff";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_UnitOfQuantity";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ClearenceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ClearenceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ClearenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ClearenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClearenceGrid.GridId = "09a9cbb8-3692-4f65-a648-b4b051effcd3";
			this.ClearenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ClearenceGrid.LayoutKey = "zGrid1";
			this.ClearenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ClearenceGrid.Name = "ClearenceGrid";
			this.ClearenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 356, true);
			this.ClearenceGrid.TabIndex = 0;
			// 
			// ClearanceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClearanceGroupBox);
			this.Name = "ClearanceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceGroupBox.ResumeLayout(false);
			this.ClearanceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ClearenceGrid)).EndInit();
			this.ClearenceGrid.ResumeLayout(false);
			this.ClearenceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ClearanceGroupBox;
		internal ZArchitecture.ZGrid ClearenceGrid;
	}
}

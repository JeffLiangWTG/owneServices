namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CusUnderbond_ISR
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.IsrsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IsrsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB);
			// 
			// IsrsGrid
			// 
			this.IsrsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IsrsGrid, "ISRs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).C4_SendersMessageReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).NewShedId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).AirportOrCountryOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).Shed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).NoPackagesExpected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).C4_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.InterShedRemoval)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusHAWB)(null)).ISRs)).SyncRoot)).SplitReferenceToWhichThisRemovalPertains)));
			this.IsrsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9c7e9ade-d186-4db6-95ad-6df99d0568cb", "Reference Number");
			zTextBoxColumnStyleInfo1.ColumnName = "C4_SendersMessageReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("73b3f0ad-c78f-45b4-b9bb-d2713c690d6b", "New Shed");
			zDropEditColumnStyleInfo1.ColumnName = "NewShedId";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7d475e97-cf94-4023-96ac-642206578bc3", "Airport of Destination");
			zDropEditColumnStyleInfo2.ColumnName = "AirportOrCountryOfDestination";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("b7bbfdbd-994e-4bd3-a2d6-4ed94a462403", "Current Shed");
			zTextBoxColumnStyleInfo2.ColumnName = "Shed";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2190741f-db4d-46b4-9ea4-8a8cd50c5287", "NPX");
			zCalcEditColumnStyleInfo1.ColumnName = "NoPackagesExpected";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c4346717-1e15-4070-8c9d-d807d6a038e3", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "C4_Status";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CusUnderbond.SplitReferenceToWhichThisRemovalPertains", "SRF", "Split", "Split Reference", "Split to which this request pertains");
			zDropEditColumnStyleInfo3.ColumnName = "SplitReferenceToWhichThisRemovalPertains";
			this.IsrsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IsrsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.IsrsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.IsrsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.IsrsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.IsrsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IsrsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.IsrsGrid.GridId = "5edc96df-f1c7-42e7-b2b4-b4fc98a79de0";
			this.IsrsGrid.CopySelectedRowsAllowed = true;
			this.IsrsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IsrsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IsrsGrid.LayoutKey = "BGB-CCSUK-ISR";
			this.IsrsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IsrsGrid.Name = "IsrsGrid";
			this.IsrsGrid.ParentRowsForeColor = System.Drawing.SystemColors.ActiveCaption;
			this.IsrsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 219, true);
			this.IsrsGrid.TabIndex = 1;
			// 
			// CusUnderbond_ISR
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IsrsGrid);
			this.Name = "CusUnderbond_ISR";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IsrsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid IsrsGrid;
	}
}

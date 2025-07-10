namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPShipsCompartmentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.ShipsCompartmentInspectionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipsCompartmentsInspectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipsCompartmentInspectionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipsCompartmentsInspectionsGrid)).BeginInit();
			this.ShipsCompartmentsInspectionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// ShipsCompartmentInspectionsGroupBox
			// 
			this.ShipsCompartmentInspectionsGroupBox.Controls.Add(this.ShipsCompartmentsInspectionsGrid);
			this.ShipsCompartmentInspectionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.ShipsCompartmentInspectionsGroupBox.Name = "ShipsCompartmentInspectionsGroupBox";
			this.ShipsCompartmentInspectionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 275, true);
			this.ShipsCompartmentInspectionsGroupBox.TabIndex = 1;
			this.ShipsCompartmentInspectionsGroupBox.TabStop = false;
			this.ShipsCompartmentInspectionsGroupBox.Text = "Ships Compartment Inspections";
			// 
			// ShipsCompartmentsInspectionsGrid
			// 
			this.ShipsCompartmentsInspectionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipsCompartmentsInspectionsGrid, "QuarantineExDocHeader+Compartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Compartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocShipsCompartment)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Compartments)).SyncRoot)).QC_Compartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocShipsCompartment)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Compartments)).SyncRoot)).QC_RL_NKInspectionPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocShipsCompartment)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Compartments)).SyncRoot)).Lookups.InspectionPorts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.QuarantineExDocShipsCompartment)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Compartments)).SyncRoot)).QC_InspectionDate)));
			this.ShipsCompartmentsInspectionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Compartments";
			zTextBoxColumnStyleInfo1.ColumnComparer = null;
			zTextBoxColumnStyleInfo1.ColumnName = "QC_Compartments";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+InspectionPorts";
			zCodeFindBoxColumnStyleInfo1.Caption = "Inspection Port";
			zCodeFindBoxColumnStyleInfo1.ColumnComparer = null;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "QC_RL_NKInspectionPort";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.Caption = "Inspection Date";
			zDateEditColumnStyleInfo1.ColumnComparer = null;
			zDateEditColumnStyleInfo1.ColumnName = "QC_InspectionDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ShipsCompartmentsInspectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipsCompartmentsInspectionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ShipsCompartmentsInspectionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ShipsCompartmentsInspectionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipsCompartmentsInspectionsGrid.GridId = "bd42140b-78a8-4091-9e0e-5a92d883a83a";
			this.ShipsCompartmentsInspectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipsCompartmentsInspectionsGrid.LayoutKey = "ShipsCompartmentsInspectionsGrid";
			this.ShipsCompartmentsInspectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ShipsCompartmentsInspectionsGrid.Name = "ShipsCompartmentsInspectionsGrid";
			this.ShipsCompartmentsInspectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 258, true);
			this.ShipsCompartmentsInspectionsGrid.TabIndex = 0;
			// 
			// RFPShipsCompartmentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShipsCompartmentInspectionsGroupBox);
			this.Name = "RFPShipsCompartmentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipsCompartmentInspectionsGroupBox.ResumeLayout(false);
			this.ShipsCompartmentInspectionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipsCompartmentsInspectionsGrid)).EndInit();
			this.ShipsCompartmentsInspectionsGrid.ResumeLayout(false);
			this.ShipsCompartmentsInspectionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ShipsCompartmentInspectionsGroupBox;
		private ZArchitecture.ZGrid ShipsCompartmentsInspectionsGrid;
	}
}

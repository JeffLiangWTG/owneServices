namespace Enterprise.Customs.FR.Module
{
	partial class CreditCODOperationalApplicatorControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CreditCODGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditCODGrid)).BeginInit();
			this.CreditCODGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator);
			// 
			// CreditCODGrid
			// 
			this.CreditCODGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CreditCODGrid, "FrCreditCODItemApplicators");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).CreditMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).CreditMethodDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).ReleasingEntryReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).PreviousEntryReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).PreviousEntryLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.CreditCODDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.FrCreditCODApplicator)(null)).FrCreditCODItemApplicators)).SyncRoot)).Currency)));
			this.CreditCODGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CreditMethod";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CreditMethodDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ReleasingEntryReference";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "PreviousEntryReference";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PreviousEntryLineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Amount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Currency";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CreditCODGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CreditCODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CreditCODGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CreditCODGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CreditCODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CreditCODGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CreditCODGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CreditCODGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditCODGrid.GridId = "e3eb783e-2c36-4f72-a4f6-6e1bd837ab3d";
			this.CreditCODGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditCODGrid.LayoutKey = "CreditCODGrid";
			this.CreditCODGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CreditCODGrid.Name = "CreditCODGrid";
			this.CreditCODGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 461, true);
			this.CreditCODGrid.TabIndex = 0;
			// 
			// CreditCODOperationalApplicatorControl
			// 
			this.Controls.Add(this.CreditCODGrid);
			this.Name = "CreditCODOperationalApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 461, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditCODGrid)).EndInit();
			this.CreditCODGrid.ResumeLayout(false);
			this.CreditCODGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZGrid CreditCODGrid;
		#endregion
	}
}

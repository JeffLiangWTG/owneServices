namespace Enterprise.Customs.KR.GUI
{
	partial class Import5FEMessageDutyTaxDetailsUserControl
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
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
      this.DutyTaxDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.DutyTaxDetailsGrid)).BeginInit();
      this.DutyTaxDetailsGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent);
      // 
      // DutyTaxDetailsGrid
      // 
      this.DutyTaxDetailsGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.DutyTaxDetailsGrid, "SendingObjectsCollection.AmendedDutyTaxItems");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)).SyncRoot)).DutyTaxType)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)).SyncRoot)).DutyTaxTypeDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)).SyncRoot)).BeforeAmount)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)).SyncRoot)).AfterAmount)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AmendedDutyTaxItems)).SyncRoot)).AmountDifference)));
      this.DutyTaxDetailsGrid.CaptionVisible = false;
      zTextBoxColumnStyleInfo1.ColumnName = "DutyTaxType";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
      zTextBoxColumnStyleInfo2.ColumnName = "DutyTaxTypeDescription";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
      zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo1.ColumnName = "BeforeAmount";
      zCalcEditColumnStyleInfo1.Decimals = 0;
      zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo2.ColumnName = "AfterAmount";
      zCalcEditColumnStyleInfo2.Decimals = 0;
      zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo3.ColumnName = "AmountDifference";
      zCalcEditColumnStyleInfo3.Decimals = 0;
      zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
      zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      this.DutyTaxDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.DutyTaxDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.DutyTaxDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
      this.DutyTaxDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
      this.DutyTaxDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
      this.DutyTaxDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DutyTaxDetailsGrid.GridId = "8e17f5ed-7fb5-45a9-b308-8c8428bdf023";
      this.DutyTaxDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.DutyTaxDetailsGrid.LayoutKey = "DutyTaxDetailsGrid";
      this.DutyTaxDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.DutyTaxDetailsGrid.Name = "DutyTaxDetailsGrid";
      this.DutyTaxDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
      this.DutyTaxDetailsGrid.TabIndex = 0;
      // 
      // Import5FEMessageDutyTaxDetailsUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.DutyTaxDetailsGrid);
      this.Name = "Import5FEMessageDutyTaxDetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.DutyTaxDetailsGrid)).EndInit();
      this.DutyTaxDetailsGrid.ResumeLayout(false);
      this.DutyTaxDetailsGrid.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid DutyTaxDetailsGrid;
	}
}

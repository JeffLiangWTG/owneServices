namespace Enterprise.Customs.KR.GUI
{
	partial class TaxExemptionOrSpecificUseDutyRateUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid)).BeginInit();
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusEntryHeader);
            // 
            // ApplyingTaxExemptionOrSpecificUseDutyRateGrid
            // 
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid, "SubsequentMessageDetails.GOVCBR5FNMessages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).EntryLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).MessageStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).MessageStatusDesc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).AcceptedDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).HSCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).DutyReduction)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5FNMessages)).SyncRoot)).PostClearanceYN)));
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
            zTextBoxColumnStyleInfo1.ColumnName = "MessageStatus";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
            zTextBoxColumnStyleInfo2.ColumnName = "MessageStatusDesc";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
            zDateEditColumnStyleInfo1.ColumnName = "AcceptedDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo3.ColumnName = "HSCode";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.ColumnName = "DutyReduction";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
            zTextBoxColumnStyleInfo5.ColumnName = "PostClearanceYN";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.GridId = "ffc6eebf-3bff-414b-92ef-86a62e708084";
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.LayoutKey = "ApplyingTaxExemptionOrSpecificUseDutyRateGrid";
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.Name = "ApplyingTaxExemptionOrSpecificUseDutyRateGrid";
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 191, true);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.TabIndex = 1;
            // 
            // TaxExemptionOrSpecificUseDutyRateUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid);
            this.Name = "TaxExemptionOrSpecificUseDutyRateUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 191, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid)).EndInit();
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.ResumeLayout(false);
            this.ApplyingTaxExemptionOrSpecificUseDutyRateGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ApplyingTaxExemptionOrSpecificUseDutyRateGrid;
	}
}

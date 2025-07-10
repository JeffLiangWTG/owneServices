namespace Enterprise.Customs.KR.GUI
{
	partial class ExemptionRequestOfPenaltyUserControl
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.Messages5UAGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Messages5UAGrid)).BeginInit();
            this.Messages5UAGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusEntryHeader);
            // 
            // Messages5UAGrid
            // 
            this.Messages5UAGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.Messages5UAGrid, "SubsequentMessageDetails.GOVCBR5UAMessages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).ApplicationReference)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageStatusDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.AcceptedDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.ReviewDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.ReviewResultDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.AmendmentDeclarationDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.AmendmentVersionNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.PenaltyTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.PenaltyExemptionReasonsCodeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5UAMessages)).SyncRoot)).MessageData5UA.PenaltyExemptionAmount)));
            this.Messages5UAGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "ApplicationReference";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zTextBoxColumnStyleInfo2.ColumnName = "MessageStatus";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo3.ColumnName = "MessageStatusDescription";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo1.ColumnName = "MessageData5UA+AcceptedDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zDateEditColumnStyleInfo2.ColumnName = "MessageData5UA+ReviewDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo4.ColumnName = "MessageData5UA+ReviewResultDescription";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zDateEditColumnStyleInfo3.ColumnName = "MessageData5UA+AmendmentDeclarationDate";
            zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "MessageData5UA+AmendmentVersionNo";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zTextBoxColumnStyleInfo5.ColumnName = "MessageData5UA+PenaltyTypeDescription";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo6.ColumnName = "MessageData5UA+PenaltyExemptionReasonsCodeDescription";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "MessageData5UA+PenaltyExemptionAmount";
            zCalcEditColumnStyleInfo2.Decimals = 0;
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.Messages5UAGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.Messages5UAGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.Messages5UAGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.Messages5UAGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.Messages5UAGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.Messages5UAGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.Messages5UAGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Messages5UAGrid.GridId = "7c675980-37aa-4a11-81c6-9458487c5ed6";
            this.Messages5UAGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.Messages5UAGrid.LayoutKey = "Messages5UAGrid";
            this.Messages5UAGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.Messages5UAGrid.Name = "Messages5UAGrid";
            this.Messages5UAGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 149, true);
            this.Messages5UAGrid.TabIndex = 0;
            // 
            // ExemptionRequestOfPenaltyUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.Messages5UAGrid);
            this.Name = "ExemptionRequestOfPenaltyUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 149, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Messages5UAGrid)).EndInit();
            this.Messages5UAGrid.ResumeLayout(false);
            this.Messages5UAGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid Messages5UAGrid;
	}
}

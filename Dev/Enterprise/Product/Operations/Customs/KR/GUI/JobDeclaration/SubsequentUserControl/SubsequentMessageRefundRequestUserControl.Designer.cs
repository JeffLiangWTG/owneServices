namespace Enterprise.Customs.KR.GUI
{
	partial class SubsequentMessageRefundRequestUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.SubsequentMessageRefundRequestGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SubsequentMessageRefundRequestGrid)).BeginInit();
            this.SubsequentMessageRefundRequestGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusEntryHeader);
            // 
            // SubsequentMessageRefundRequestGrid
            // 
            this.SubsequentMessageRefundRequestGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.SubsequentMessageRefundRequestGrid, "SubsequentMessageDetails.GOVCBR5ULMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.RefundDeclarationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.MessageStatus5UL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.MessageStatusDesc5UL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.ReviewResult)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.ReviewResultDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.AcceptedDate5UL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.CustomsDisbursementBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.RefundApprovalDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.RefundApprovalNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.ProvisionDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5ULMessages)).SyncRoot)).MessageData5UL.ProvisionNo)));
            this.SubsequentMessageRefundRequestGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "MessageData5UL+RefundDeclarationNumber";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zTextBoxColumnStyleInfo2.ColumnName = "MessageData5UL+MessageStatus5UL";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo3.ColumnName = "MessageData5UL+MessageStatusDesc5UL";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo4.ColumnName = "MessageData5UL+ReviewResult";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "MessageData5UL+ReviewResultDesc";
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo1.ColumnName = "MessageData5UL+AcceptedDate5UL";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "MessageData5UL+CustomsDisbursementBillNumber";
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
            zDateEditColumnStyleInfo2.ColumnName = "MessageData5UL+RefundApprovalDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo7.ColumnName = "MessageData5UL+RefundApprovalNumber";
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zDateEditColumnStyleInfo3.ColumnName = "MessageData5UL+ProvisionDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo8.ColumnName = "MessageData5UL+ProvisionNo";
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.SubsequentMessageRefundRequestGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.SubsequentMessageRefundRequestGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubsequentMessageRefundRequestGrid.GridId = "34f9eac5-eb0f-4e43-a636-926c30cac48b";
            this.SubsequentMessageRefundRequestGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.SubsequentMessageRefundRequestGrid.LayoutKey = "SubsequentMessageRefundRequestGrid";
            this.SubsequentMessageRefundRequestGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SubsequentMessageRefundRequestGrid.Name = "SubsequentMessageRefundRequestGrid";
            this.SubsequentMessageRefundRequestGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 381, true);
            this.SubsequentMessageRefundRequestGrid.TabIndex = 0;
            // 
            // SubsequentMessageRefundRequestUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.SubsequentMessageRefundRequestGrid);
            this.Name = "SubsequentMessageRefundRequestUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 381, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SubsequentMessageRefundRequestGrid)).EndInit();
            this.SubsequentMessageRefundRequestGrid.ResumeLayout(false);
            this.SubsequentMessageRefundRequestGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SubsequentMessageRefundRequestGrid;
	}
}

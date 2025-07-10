using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	partial class RequestToExtendReExportDateUserControl
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
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.RequestToExtendReExportDateGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RequestToExtendReExportDateGrid)).BeginInit();
            this.RequestToExtendReExportDateGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // RequestToExtendReExportDateGrid
            // 
            this.RequestToExtendReExportDateGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.RequestToExtendReExportDateGrid, "CustomsEntryHeaders.SubsequentMessageDetails.GOVCBRD72Messages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.SequenceNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.MessageStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.MessageStatusDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.AcceptedDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.DecisionDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.NoticeTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.BeforeReExportScheduledDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.AfterReExportScheduledDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).SubsequentMessageDetails.GOVCBRD72Messages)).SyncRoot)).GOVCBRD72Message.ReasonDescription)));
            this.RequestToExtendReExportDateGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "GOVCBRD72Message+SequenceNo";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.IsReadOnly = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zTextBoxColumnStyleInfo1.ColumnName = "GOVCBRD72Message+MessageStatus";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "GOVCBRD72Message+MessageStatusDescription";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo1.ColumnName = "GOVCBRD72Message+AcceptedDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsReadOnly = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "GOVCBRD72Message+DecisionDate";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "GOVCBRD72Message+NoticeTypeDescription";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo3.ColumnName = "GOVCBRD72Message+BeforeReExportScheduledDate";
            zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo3.IsReadOnly = true;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            zDateEditColumnStyleInfo4.ColumnName = "GOVCBRD72Message+AfterReExportScheduledDate";
            zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo4.IsReadOnly = true;
            zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            zTextBoxColumnStyleInfo4.ColumnName = "GOVCBRD72Message+ReasonDescription";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
            this.RequestToExtendReExportDateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.RequestToExtendReExportDateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RequestToExtendReExportDateGrid.GridId = "1e0f7e11-63e5-40f4-addb-440225a98787";
            this.RequestToExtendReExportDateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.RequestToExtendReExportDateGrid.LayoutKey = "RequestToExtendReExportDateGrid";
            this.RequestToExtendReExportDateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RequestToExtendReExportDateGrid.Name = "RequestToExtendReExportDateGrid";
            this.RequestToExtendReExportDateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1218, 187, true);
            this.RequestToExtendReExportDateGrid.TabIndex = 0;
            // 
            // RequestToExtendReExportDateUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.RequestToExtendReExportDateGrid);
            this.Name = "RequestToExtendReExportDateUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1218, 187, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RequestToExtendReExportDateGrid)).EndInit();
            this.RequestToExtendReExportDateGrid.ResumeLayout(false);
            this.RequestToExtendReExportDateGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid RequestToExtendReExportDateGrid;
	}
}

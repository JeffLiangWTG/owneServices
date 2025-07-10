namespace Enterprise.Customs.KR.GUI
{
	partial class AgreedRateForAllLinesUserControl
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
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
      this.AmendmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.AgreedRateForAllLinesAmendmentGrid = new Enterprise.ZArchitecture.ZGrid();
      this.AgreedRateForAllLinesPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.MessageStatusDropEdit.SuspendLayout();
      this.AcceptedDateEdit.SuspendLayout();
      this.AmendmentGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.AgreedRateForAllLinesAmendmentGrid)).BeginInit();
      this.AgreedRateForAllLinesAmendmentGrid.SuspendLayout();
      this.AgreedRateForAllLinesPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusEntryHeader);
      // 
      // MessageStatusDropEdit
      // 
      this.MessageStatusDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "SubsequentMessageDetails.MessageStatus5BA");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.MessageStatus5BA)));
      this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 3, true);
      this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
      this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
      this.MessageStatusDropEdit.TabIndex = 0;
      // 
      // AcceptedDateEdit
      // 
      this.AcceptedDateEdit.AllowDrop = true;
      this.AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
      this.BindingSource.SetBindingMember(this.AcceptedDateEdit, "SubsequentMessageDetails.AcceptedDate5BA");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.AcceptedDate5BA)));
      this.AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 3, true);
      this.AcceptedDateEdit.Name = "AcceptedDateEdit";
      this.AcceptedDateEdit.TabIndex = 1;
      // 
      // AmendmentGroupBox
      // 
      this.AmendmentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("7659a64e-b1e2-433e-88cb-fe1162a180a3", "Amendment");
      this.AmendmentGroupBox.Controls.Add(this.AgreedRateForAllLinesAmendmentGrid);
      this.AmendmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.AmendmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
      this.AmendmentGroupBox.Name = "AmendmentGroupBox";
      this.AmendmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 350, true);
      this.AmendmentGroupBox.TabIndex = 4;
      this.AmendmentGroupBox.TabStop = false;
      // 
      // AgreedRateForAllLinesAmendmentGrid
      // 
      this.AgreedRateForAllLinesAmendmentGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.AgreedRateForAllLinesAmendmentGrid, "SubsequentMessageDetails.GOVCBR5BBMessages");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).ApplicationReference)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).MessageStatus)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).MessageStatusDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).GOVCBR5BBMessage.AcceptedDate)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).GOVCBR5BBMessage.ReviewDate)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).GOVCBR5BBMessage.ReviewResultDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessageWrapper)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.GOVCBR5BBMessages)).SyncRoot)).GOVCBR5BBMessage.AmendReasonDescription)));
      this.AgreedRateForAllLinesAmendmentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ApplicationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
      zTextBoxColumnStyleInfo2.ColumnName = "MessageStatus";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zTextBoxColumnStyleInfo3.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
      zDateEditColumnStyleInfo1.ColumnName = "GOVCBR5BBMessage+AcceptedDate";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
      zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zDateEditColumnStyleInfo2.ColumnName = "GOVCBR5BBMessage+ReviewDate";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
      zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zTextBoxColumnStyleInfo4.ColumnName = "GOVCBR5BBMessage+ReviewResultDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zTextBoxColumnStyleInfo5.ColumnName = "GOVCBR5BBMessage+AmendReasonDescription";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
      this.AgreedRateForAllLinesAmendmentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
      this.AgreedRateForAllLinesAmendmentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.AgreedRateForAllLinesAmendmentGrid.GridId = "16b5a902-1411-46d9-86db-ed8a420c7b07";
      this.AgreedRateForAllLinesAmendmentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.AgreedRateForAllLinesAmendmentGrid.LayoutKey = "AgreedRateForAllLinesAmendmentGrid";
      this.AgreedRateForAllLinesAmendmentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.AgreedRateForAllLinesAmendmentGrid.Name = "AgreedRateForAllLinesAmendmentGrid";
      this.AgreedRateForAllLinesAmendmentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 333, true);
      this.AgreedRateForAllLinesAmendmentGrid.TabIndex = 0;
      // 
      // AgreedRateForAllLinesPanel
      // 
      this.AgreedRateForAllLinesPanel.Controls.Add(this.AcceptedDateEdit);
      this.AgreedRateForAllLinesPanel.Controls.Add(this.MessageStatusDropEdit);
      this.AgreedRateForAllLinesPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.AgreedRateForAllLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.AgreedRateForAllLinesPanel.Name = "AgreedRateForAllLinesPanel";
      this.AgreedRateForAllLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 32, true);
      this.AgreedRateForAllLinesPanel.TabIndex = 2;
      // 
      // AgreedRateForAllLinesUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.AmendmentGroupBox);
      this.Controls.Add(this.AgreedRateForAllLinesPanel);
      this.Name = "AgreedRateForAllLinesUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 382, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.MessageStatusDropEdit.ResumeLayout(true);
      this.MessageStatusDropEdit.PerformLayout();
      this.AcceptedDateEdit.ResumeLayout(true);
      this.AcceptedDateEdit.PerformLayout();
      this.AmendmentGroupBox.ResumeLayout(false);
      this.AmendmentGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.AgreedRateForAllLinesAmendmentGrid)).EndInit();
      this.AgreedRateForAllLinesAmendmentGrid.ResumeLayout(false);
      this.AgreedRateForAllLinesAmendmentGrid.PerformLayout();
      this.AgreedRateForAllLinesPanel.ResumeLayout(false);
      this.AgreedRateForAllLinesPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDateEdit;
		private ZArchitecture.GUI.ZGroupBox AmendmentGroupBox;
		private ZArchitecture.ZGrid AgreedRateForAllLinesAmendmentGrid;
		private ZArchitecture.GUI.ZPanel AgreedRateForAllLinesPanel;
	}
}

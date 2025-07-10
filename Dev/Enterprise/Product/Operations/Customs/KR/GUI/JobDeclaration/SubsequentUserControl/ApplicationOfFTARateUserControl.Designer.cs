namespace Enterprise.Customs.KR.GUI
{
	partial class ApplicationOfFTARateUserControl
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
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
      this.LawCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
      this.ApplicationOfFTARateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.AmendmentOfFTARateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.AmendmentOfApplicationOfFTARateGrid = new Enterprise.ZArchitecture.ZGrid();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.MessageStatusDropEdit.SuspendLayout();
      this.AcceptedDateEdit.SuspendLayout();
      this.ApplicationOfFTARateGroupBox.SuspendLayout();
      this.AmendmentOfFTARateGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.AmendmentOfApplicationOfFTARateGrid)).BeginInit();
      this.AmendmentOfApplicationOfFTARateGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusEntryHeader);
      // 
      // MessageStatusDropEdit
      // 
      this.MessageStatusDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "SubsequentMessageDetails.FTAMessageStatus");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAMessageStatus)));
      this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 17, true);
      this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
      this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
      this.MessageStatusDropEdit.TabIndex = 0;
      // 
      // AcceptedDateEdit
      // 
      this.AcceptedDateEdit.AllowDrop = true;
      this.AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
      this.BindingSource.SetBindingMember(this.AcceptedDateEdit, "SubsequentMessageDetails.FTAAcceptedDate");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAcceptedDate)));
      this.AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 44, true);
      this.AcceptedDateEdit.Name = "AcceptedDateEdit";
      this.AcceptedDateEdit.TabIndex = 1;
      // 
      // LawCodeTextBox
      // 
      this.BindingSource.SetBindingMember(this.LawCodeTextBox, "SubsequentMessageDetails.LawCodeDescription");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.LawCodeDescription)));
      this.LawCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 17, true);
      this.LawCodeTextBox.Name = "LawCodeTextBox";
      this.LawCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
      this.LawCodeTextBox.TabIndex = 2;
      // 
      // ApplicationOfFTARateGroupBox
      // 
      this.ApplicationOfFTARateGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1ac7c41e-1a66-466b-a75f-4ad486156e98", "Application of FTA Rate");
      this.ApplicationOfFTARateGroupBox.Controls.Add(this.MessageStatusDropEdit);
      this.ApplicationOfFTARateGroupBox.Controls.Add(this.AcceptedDateEdit);
      this.ApplicationOfFTARateGroupBox.Controls.Add(this.LawCodeTextBox);
      this.ApplicationOfFTARateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.ApplicationOfFTARateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.ApplicationOfFTARateGroupBox.Name = "ApplicationOfFTARateGroupBox";
      this.ApplicationOfFTARateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(671, 108, true);
      this.ApplicationOfFTARateGroupBox.TabIndex = 3;
      this.ApplicationOfFTARateGroupBox.TabStop = false;
      // 
      // AmendmentOfFTARateGroupBox
      // 
      this.AmendmentOfFTARateGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("f6aa4d76-17f6-4a33-8119-781af93ef7a0", "Amendment of Application of FTA Rate");
      this.AmendmentOfFTARateGroupBox.Controls.Add(this.AmendmentOfApplicationOfFTARateGrid);
      this.AmendmentOfFTARateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.AmendmentOfFTARateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
      this.AmendmentOfFTARateGroupBox.Name = "AmendmentOfFTARateGroupBox";
      this.AmendmentOfFTARateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(671, 233, true);
      this.AmendmentOfFTARateGroupBox.TabIndex = 4;
      this.AmendmentOfFTARateGroupBox.TabStop = false;
      // 
      // AmendmentOfApplicationOfFTARateGrid
      // 
      this.AmendmentOfApplicationOfFTARateGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.AmendmentOfApplicationOfFTARateGrid, "SubsequentMessageDetails.FTAAmendments");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).SequenceNo)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).MessageStatus)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).MessageStatusDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).AcceptedDate)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).DecisionDate)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).NoticeTypeDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).AmendmentReason)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FTAAmendmentDetails)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusEntryHeader)(null)).SubsequentMessageDetails.FTAAmendments)).SyncRoot)).AmendmentType)));
      this.AmendmentOfApplicationOfFTARateGrid.CaptionVisible = false;
      zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
      zCalcEditColumnStyleInfo1.ColumnName = "SequenceNo";
      zCalcEditColumnStyleInfo1.IsReadOnly = true;
      zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
      zTextBoxColumnStyleInfo1.ColumnName = "MessageStatus";
      zTextBoxColumnStyleInfo1.IsReadOnly = true;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zTextBoxColumnStyleInfo2.ColumnName = "MessageStatusDescription";
      zTextBoxColumnStyleInfo2.IsReadOnly = true;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zDateEditColumnStyleInfo1.ColumnName = "AcceptedDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
      zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
      zDateEditColumnStyleInfo2.ColumnName = "DecisionDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
      zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
      zTextBoxColumnStyleInfo3.ColumnName = "NoticeTypeDescription";
      zTextBoxColumnStyleInfo3.IsReadOnly = true;
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      zTextBoxColumnStyleInfo4.ColumnName = "AmendmentReason";
      zTextBoxColumnStyleInfo4.IsReadOnly = true;
      zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zTextBoxColumnStyleInfo5.ColumnName = "AmendmentType";
      zTextBoxColumnStyleInfo5.IsReadOnly = true;
      zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
      this.AmendmentOfApplicationOfFTARateGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
      this.AmendmentOfApplicationOfFTARateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.AmendmentOfApplicationOfFTARateGrid.GridId = "54543084-171e-4589-b510-390eae2b4e90";
      this.AmendmentOfApplicationOfFTARateGrid.LayoutKey = "AmendmentOfApplicationOfFTARateGrid";
      this.AmendmentOfApplicationOfFTARateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.AmendmentOfApplicationOfFTARateGrid.Name = "AmendmentOfApplicationOfFTARateGrid";
      this.AmendmentOfApplicationOfFTARateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 216, true);
      this.AmendmentOfApplicationOfFTARateGrid.TabIndex = 0;
      // 
      // ApplicationOfFTARateUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.AmendmentOfFTARateGroupBox);
      this.Controls.Add(this.ApplicationOfFTARateGroupBox);
      this.Name = "ApplicationOfFTARateUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(671, 341, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.MessageStatusDropEdit.ResumeLayout(true);
      this.MessageStatusDropEdit.PerformLayout();
      this.AcceptedDateEdit.ResumeLayout(true);
      this.AcceptedDateEdit.PerformLayout();
      this.ApplicationOfFTARateGroupBox.ResumeLayout(false);
      this.ApplicationOfFTARateGroupBox.PerformLayout();
      this.AmendmentOfFTARateGroupBox.ResumeLayout(false);
      this.AmendmentOfFTARateGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.AmendmentOfApplicationOfFTARateGrid)).EndInit();
      this.AmendmentOfApplicationOfFTARateGrid.ResumeLayout(false);
      this.AmendmentOfApplicationOfFTARateGrid.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDateEdit;
		private ZArchitecture.ZTextBox LawCodeTextBox;
		private ZArchitecture.GUI.ZGroupBox ApplicationOfFTARateGroupBox;
		private ZArchitecture.GUI.ZGroupBox AmendmentOfFTARateGroupBox;
		private ZArchitecture.ZGrid AmendmentOfApplicationOfFTARateGrid;
	}
}

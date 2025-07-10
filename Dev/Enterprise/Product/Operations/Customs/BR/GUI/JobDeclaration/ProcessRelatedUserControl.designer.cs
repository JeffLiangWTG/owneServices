namespace Enterprise.Customs.BR.GUI
{
	partial class ProcessRelatedUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReferenceNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateEditIssued = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TextBoxInfo = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.DropEditType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReferenceNumbersGroupBox.SuspendLayout();
			this.DateEditIssued.SuspendLayout();
			this.DropEditType.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).BeginInit();
			this.NumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ReferenceNumbersGroupBox
			// 
			this.ReferenceNumbersGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("dab88654-6f28-475c-b133-91796062ec07", "Process Related Numbers");
			this.ReferenceNumbersGroupBox.Controls.Add(this.DateEditIssued);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxInfo);
			this.ReferenceNumbersGroupBox.Controls.Add(this.TextBoxNumber);
			this.ReferenceNumbersGroupBox.Controls.Add(this.DropEditType);
			this.ReferenceNumbersGroupBox.Controls.Add(this.NumbersGrid);
			this.ReferenceNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbersGroupBox.Name = "ReferenceNumbersGroupBox";
			this.ReferenceNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 284, true);
			this.ReferenceNumbersGroupBox.TabIndex = 1;
			this.ReferenceNumbersGroupBox.TabStop = false;
			// 
			// DateEditIssued
			// 
			this.DateEditIssued.AllowDrop = true;
			this.DateEditIssued.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DateEditIssued.AutoCompleteMonthThreshold = 1;
			this.DateEditIssued.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEditIssued, "ProcessRelatedNumbers.CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_IssueDate)));
			this.DateEditIssued.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6ec726ec-9350-4581-856c-46cd0b15a546", "Issued");
			this.DateEditIssued.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 258, true);
			this.DateEditIssued.Name = "DateEditIssued";
			this.DateEditIssued.TabIndex = 4;
			// 
			// TextBoxInfo
			// 
			this.TextBoxInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TextBoxInfo, "ProcessRelatedNumbers.CE_EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryLineReference)));
			this.TextBoxInfo.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("cc651817-b0b2-4084-a2c9-43bc55bda2ee", "Info");
			this.TextBoxInfo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 236, true);
			this.TextBoxInfo.Name = "TextBoxInfo";
			this.TextBoxInfo.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.TextBoxInfo.TabIndex = 3;
			// 
			// TextBoxNumber
			// 
			this.TextBoxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.TextBoxNumber, "ProcessRelatedNumbers.CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryNum)));
			this.TextBoxNumber.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("94194497-e334-481f-b711-fbfccd2200b0", "Number");
			this.TextBoxNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 214, true);
			this.TextBoxNumber.Name = "TextBoxNumber";
			this.TextBoxNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.TextBoxNumber.TabIndex = 2;
			// 
			// DropEditType
			// 
			this.DropEditType.AllowDrop = true;
			this.DropEditType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.DropEditType, "ProcessRelatedNumbers.CE_EntryType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryType)));
			this.DropEditType.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0fe055af-1dd6-4978-b21d-596e854a4ef7", "Type");
			this.DropEditType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 193, true);
			this.DropEditType.Name = "DropEditType";
			this.DropEditType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.DropEditType.TabIndex = 1;
			// 
			// NumbersGrid
			// 
			this.NumbersGrid.AllowNavigation = false;
			this.NumbersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NumbersGrid, "ProcessRelatedNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_EntryLineReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).CE_IssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Common.CusEntryNumber)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ProcessRelatedNumbers)).SyncRoot)).AdditionalReferenceNumberTypeDescription)));
			this.NumbersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("75bab0f4-27a9-4e0c-a8e6-2b665c55e649", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CE_EntryType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("91f6987b-0b0f-4188-96ca-917f23ae66c9", "Number");
			zTextBoxColumnStyleInfo1.ColumnName = "CE_EntryNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("728a6409-a975-4d92-b970-f3f0ad929b7d", "Info");
			zTextBoxColumnStyleInfo2.ColumnName = "CE_EntryLineReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("3c88465b-9490-4e4f-8844-ebc49435a9c3", "Issued Date");
			zDateEditColumnStyleInfo1.ColumnName = "CE_IssueDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("AE8059D8-1368-4D7B-AE23-C645A61A1527", "Description");
			zTextBoxColumnStyleInfo3.ColumnName = "AdditionalReferenceNumberTypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo3.IsVisible = false;
			this.NumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NumbersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NumbersGrid.GridId = "bb30e775-b325-4d67-9888-abd533d6b12c";
			this.NumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NumbersGrid.LayoutKey = "NumbersGrid";
			this.NumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NumbersGrid.Name = "NumbersGrid";
			this.NumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 171, true);
			this.NumbersGrid.TabIndex = 0;
			// 
			// ProcessRelatedUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumbersGroupBox);
			this.Name = "ProcessRelatedUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 284, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReferenceNumbersGroupBox.ResumeLayout(false);
			this.ReferenceNumbersGroupBox.PerformLayout();
			this.DateEditIssued.ResumeLayout(true);
			this.DateEditIssued.PerformLayout();
			this.DropEditType.ResumeLayout(true);
			this.DropEditType.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumbersGrid)).EndInit();
			this.NumbersGrid.ResumeLayout(false);
			this.NumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ReferenceNumbersGroupBox;
		private ZArchitecture.GUI.ZDateEdit DateEditIssued;
		private ZArchitecture.ZTextBox TextBoxInfo;
		private ZArchitecture.ZTextBox TextBoxNumber;
		private ZArchitecture.GUI.ZDropEdit DropEditType;
		public ZArchitecture.ZGrid NumbersGrid;
	}
}

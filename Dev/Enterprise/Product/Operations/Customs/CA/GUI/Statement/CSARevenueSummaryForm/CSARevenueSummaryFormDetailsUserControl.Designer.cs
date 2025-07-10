
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class CSARevenueSummaryFormDetailsUserControl
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
			this.ImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BusinessNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PeriodEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatementDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VFDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSFStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSFSubmissionDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RSFAcceptedDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CalculateRSFButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PeriodMonthEdit = new Enterprise.Customs.Module.MonthEdit();
			this.PeriodYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterGuidFindBox.SuspendLayout();
			this.BusinessNumberTextBox.SuspendLayout();
			this.PeriodStartDateEdit.SuspendLayout();
			this.PeriodEndDateEdit.SuspendLayout();
			this.StatementDateEdit.SuspendLayout();
			this.VFDTextBox.SuspendLayout();
			this.StatementNumberTextBox.SuspendLayout();
			this.RSFStatusTextBox.SuspendLayout();
			this.RSFSubmissionDateTextBox.SuspendLayout();
			this.RSFAcceptedDateTextBox.SuspendLayout();
			this.CalculateRSFButton.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusStatementHeader);
			// 
			// ImporterGuidFindBox
			// 
			this.ImporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterGuidFindBox, "B2_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_OH_Importer)));
			this.ImporterGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("84cbb931-e439-44a2-9824-bcbf7da13afa", "Importer");
			this.ImporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 15, true);
			this.ImporterGuidFindBox.Name = "ImporterGuidFindBox";
			this.ImporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 20, true);
			this.ImporterGuidFindBox.TabIndex = 1;
			// 
			// BusinessNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BusinessNumberTextBox, "B2_ImporterCustomsID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_ImporterCustomsID)));
			this.BusinessNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B2687AA2-ECE5-4E90-B949-25182BBE5D59", "Business Number");
			this.BusinessNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 41, true);
			this.BusinessNumberTextBox.Name = "BusinessNumberTextBox";
			this.BusinessNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.BusinessNumberTextBox.TabIndex = 2;
			// 
			// PeriodStartDateEdit
			// 
			this.BindingSource.SetBindingMember(this.PeriodStartDateEdit, "B2_PeriodStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PeriodStartDate)));
			this.PeriodStartDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DFC1F30B-6751-4F74-BE76-7CB7D3E3F0DC", "Period Start");
			this.PeriodStartDateEdit.AllowDrop = true;
			this.PeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodStartDateEdit.AutoCompleteYear = true;
			this.PeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 93, true);
			this.PeriodStartDateEdit.Name = "PeriodStartDateEdit";
			this.PeriodStartDateEdit.TabIndex = 5;
			// 
			// PeriodEndDateEdit
			// 
			this.BindingSource.SetBindingMember(this.PeriodEndDateEdit, "B2_PeriodEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PeriodEndDate)));
			this.PeriodEndDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B150791E-C82C-4A5C-8668-02B5F99AE5CB", "Period End");
			this.PeriodEndDateEdit.AllowDrop = true;
			this.PeriodEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodEndDateEdit.AutoCompleteYear = true;
			this.PeriodEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 93, true);
			this.PeriodEndDateEdit.Name = "PeriodEndDateEdit";
			this.PeriodEndDateEdit.TabIndex = 6;
			// 
			// StatementDateEdit
			// 
			this.BindingSource.SetBindingMember(this.StatementDateEdit, "B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PrintDate)));
			this.StatementDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C3BD4FBC-ABDF-4669-8D38-C1C37FB8B164", "Statement Date");
			this.StatementDateEdit.AllowDrop = true;
			this.StatementDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementDateEdit.AutoCompleteYear = true;
			this.StatementDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 41, true);
			this.StatementDateEdit.Name = "StatementDateEdit";
			this.StatementDateEdit.TabIndex = 9;
			// 
			// VFDTextBox
			// 
			this.VFDTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8FF73AAF-AF0A-4497-AF78-60D6BD292BC9", "VFD");
			this.VFDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 119, true);
			this.VFDTextBox.Name = "VFDTextBox";
			this.VFDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.VFDTextBox.TabIndex = 7;
			this.VFDTextBox.ReadOnly = true;
			// 
			// StatementNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementNumberTextBox, "B2_StatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementNumber)));
			this.StatementNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6353F675-74D5-4C0C-9BAD-11E8AAC89368", "Statement Number");
			this.StatementNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 15, true);
			this.StatementNumberTextBox.Name = "StatementNumberTextBox";
			this.StatementNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.StatementNumberTextBox.TabIndex = 8;
			// 
			// RSFStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSFStatusTextBox, "B2_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_Status)));
			this.RSFStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("22A0135C-27B4-43BD-AE16-7B76AC4E54F2", "RSF Status");
			this.RSFStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 67, true);
			this.RSFStatusTextBox.Name = "RSFStatusTextBox";
			this.RSFStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.RSFStatusTextBox.TabIndex = 10;
			// 
			// RSFSubmissionDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSFSubmissionDateTextBox, "B2_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_DueDate)));
			this.RSFSubmissionDateTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5747DB35-632A-4EFB-8725-17A07CC47AD4", "RSF Submission Date");
			this.RSFSubmissionDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 93, true);
			this.RSFSubmissionDateTextBox.Name = "RSFSubmissionDateTextBox";
			this.RSFSubmissionDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.RSFSubmissionDateTextBox.TabIndex = 11;
			this.RSFSubmissionDateTextBox.ReadOnly = true;
			// 
			// RSFAcceptedDateTextBox
			// 
			this.BindingSource.SetBindingMember(this.RSFAcceptedDateTextBox, "B2_ProcessDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_ProcessDate)));
			this.RSFAcceptedDateTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("54D74D66-FAF4-4C0B-8683-064E9E53442E", "RSF Accepted Date");
			this.RSFAcceptedDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 119, true);
			this.RSFAcceptedDateTextBox.Name = "RSFAcceptedDateTextBox";
			this.RSFAcceptedDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.RSFAcceptedDateTextBox.TabIndex = 12;
			this.RSFAcceptedDateTextBox.ReadOnly = true;
			//
			// CalculateRSFButton
			//
			this.CalculateRSFButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 143, true);
			this.CalculateRSFButton.Name = "CalculateRSFButton";
			this.CalculateRSFButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CalculateRSFButton.TabIndex = 12;
			this.CalculateRSFButton.Text = Res.GetString("F8695E31-BFF0-429C-8F7C-06732B22E069", "Calculate RSF");
			this.CalculateRSFButton.Click += CalculateRSFButton_Clicked;
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodMonthEdit, "PeriodMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).PeriodMonth)));
			this.PeriodMonthEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("880b4882-b2a4-4714-9324-eb10f1acb487", "Period");
			this.PeriodMonthEdit.DecimalPlaces = 2;
			this.PeriodMonthEdit.IsCalculatorEnabled = false;
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 67, true);
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PeriodMonthEdit.ShowGroupSeparators = false;
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.PeriodMonthEdit.TabIndex = 3;
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodYearEdit, "PeriodYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).PeriodYear)));
			this.PeriodYearEdit.CaptionResourceString = null;
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodYearEdit, false);
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 67, true);
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 20, true);
			this.PeriodYearEdit.TabIndex = 4;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.AutoSize = true;
			this.PeriodLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 71, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.PeriodLabel.TabIndex = 15;
			this.PeriodLabel.Text = "/";
			// 
			// CSARevenueSummaryFormDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodMonthEdit);
			this.Controls.Add(this.PeriodYearEdit);
			this.Controls.Add(this.PeriodLabel);
			this.Controls.Add(this.ImporterGuidFindBox);
			this.Controls.Add(this.BusinessNumberTextBox);
			this.Controls.Add(this.PeriodStartDateEdit);
			this.Controls.Add(this.PeriodEndDateEdit);
			this.Controls.Add(this.StatementDateEdit);
			this.Controls.Add(this.VFDTextBox);
			this.Controls.Add(this.StatementNumberTextBox);
			this.Controls.Add(this.RSFStatusTextBox);
			this.Controls.Add(this.RSFSubmissionDateTextBox);
			this.Controls.Add(this.RSFAcceptedDateTextBox);
			this.Controls.Add(this.CalculateRSFButton);
			this.Name = "CSARevenueSummaryFormDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterGuidFindBox.ResumeLayout(true);
			this.ImporterGuidFindBox.PerformLayout();
			this.BusinessNumberTextBox.ResumeLayout(false);
			this.BusinessNumberTextBox.PerformLayout();
			this.PeriodStartDateEdit.ResumeLayout(true);
			this.PeriodStartDateEdit.PerformLayout();
			this.PeriodEndDateEdit.ResumeLayout(true);
			this.PeriodEndDateEdit.PerformLayout();
			this.StatementDateEdit.ResumeLayout(true);
			this.StatementDateEdit.PerformLayout();
			this.VFDTextBox.ResumeLayout(false);
			this.VFDTextBox.PerformLayout();
			this.StatementNumberTextBox.ResumeLayout(false);
			this.StatementNumberTextBox.PerformLayout();
			this.RSFStatusTextBox.ResumeLayout(false);
			this.RSFStatusTextBox.PerformLayout();
			this.RSFSubmissionDateTextBox.ResumeLayout(false);
			this.RSFSubmissionDateTextBox.PerformLayout();
			this.RSFAcceptedDateTextBox.ResumeLayout(false);
			this.RSFAcceptedDateTextBox.PerformLayout();
			this.CalculateRSFButton.ResumeLayout(false);
			this.CalculateRSFButton.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox BusinessNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PeriodStartDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PeriodEndDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit StatementDateEdit;
		private Enterprise.ZArchitecture.ZTextBox VFDTextBox;
		private Enterprise.ZArchitecture.ZTextBox StatementNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox RSFStatusTextBox;
		private Enterprise.ZArchitecture.ZTextBox RSFSubmissionDateTextBox;
		private Enterprise.ZArchitecture.ZTextBox RSFAcceptedDateTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton CalculateRSFButton;
		private Customs.Module.MonthEdit PeriodMonthEdit;
		private ZArchitecture.GUI.ZYearEdit PeriodYearEdit;
		private ZArchitecture.ZLabel PeriodLabel;
	}
}

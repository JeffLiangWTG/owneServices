
namespace Enterprise.Customs.ES.GUI
{
	partial class UpdateCSVClearanceForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.CSVClearance = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearanceDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SecondaryCSVNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.ThirdCSVNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceDate.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CsvCodeInfo);
			// 
			// CSVClearance
			// 
			this.BindingSource.SetBindingMember(this.CSVClearance, "CsvCodeFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CsvCodeInfo)(null)).CsvCodeFromUser)));
			this.CSVClearance.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("D3028CDF-8065-47BC-B614-7FF912999539", "CSV Clearance");
			this.CSVClearance.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 58, true);
			this.CSVClearance.Name = "CSVClearance";
			this.CSVClearance.ShouldEscapeAllSpecialCharacters = false;
			this.CSVClearance.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.CSVClearance.TabIndex = 2;
			// 
			// ClearanceDate
			// 
			this.BindingSource.SetBindingMember(this.ClearanceDate, "ClearanceDateFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CsvCodeInfo)(null)).ClearanceDateFromUser)));
			this.ClearanceDate.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("5C88D9AD-E3EF-462B-90E1-6A82A24625E7", "Clearance Date");
			this.ClearanceDate.AllowDrop = true;
			this.ClearanceDate.AutoCompleteMonthThreshold = 1;
			this.ClearanceDate.AutoCompleteYear = true;
			this.ClearanceDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClearanceDate, true);
			this.ClearanceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 84, true);
			this.ClearanceDate.Name = "ClearanceDate";
			this.ClearanceDate.TabIndex = 3;
			// 
			// SecondaryCSVNumber
			// 
			this.BindingSource.SetBindingMember(this.SecondaryCSVNumber, "SecondaryCsvCodeFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CsvCodeInfo)(null)).SecondaryCsvCodeFromUser)));
			this.SecondaryCSVNumber.CaptionResourceString = null;
			this.SecondaryCSVNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 110, true);
			this.SecondaryCSVNumber.Name = "SecondaryCSVNumber";
			this.SecondaryCSVNumber.ShouldEscapeAllSpecialCharacters = false;
			this.SecondaryCSVNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.SecondaryCSVNumber.TabIndex = 4;
			// 
			// ThirdCSVNumber
			// 
			this.BindingSource.SetBindingMember(this.ThirdCSVNumber, "ThirdCsvCodeFromUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CsvCodeInfo)(null)).ThirdCsvCodeFromUser)));
			this.ThirdCSVNumber.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("36F99546-C547-4875-86D4-5881D2192489", "CSV Exit Certificate");
			this.ThirdCSVNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 136, true);
			this.ThirdCSVNumber.Name = "ThirdCSVNumber";
			this.ThirdCSVNumber.ShouldEscapeAllSpecialCharacters = false;
			this.ThirdCSVNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ThirdCSVNumber.TabIndex = 5;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("23E13A67-E603-4923-9011-C3C7458FC355", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 161, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("5422DB49-CEED-45C6-87E3-7BB5EC3340F6", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.IsCaptionOverridden = false;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 161, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 7;
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageLabel.TabIndex = 0;
			// 
			// UpdateCSVClearanceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 214, true);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.CSVClearance);
			this.Controls.Add(this.ClearanceDate);
			this.Controls.Add(this.SecondaryCSVNumber);
			this.Controls.Add(this.ThirdCSVNumber);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.DataSourceType = typeof(Enterprise.Customs.ES.Business.CsvCodeInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "UpdateCSVClearanceForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Update CSV Clearance";
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.SecondaryCSVNumber, 0);
			this.Controls.SetChildIndex(this.CSVClearance, 0);
			this.Controls.SetChildIndex(this.ClearanceDate, 0);
			this.Controls.SetChildIndex(this.ThirdCSVNumber, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceDate.ResumeLayout(true);
			this.ClearanceDate.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZLabel MessageLabel;
		protected ZArchitecture.ZTextBox CSVClearance;
		protected ZArchitecture.GUI.ZDateEdit ClearanceDate;
		protected ZArchitecture.ZTextBox SecondaryCSVNumber;
		protected ZArchitecture.ZTextBox ThirdCSVNumber;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton Cancel_Button;
	}
}

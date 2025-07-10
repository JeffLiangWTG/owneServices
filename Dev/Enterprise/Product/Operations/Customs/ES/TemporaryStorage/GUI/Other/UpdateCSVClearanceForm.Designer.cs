
namespace Enterprise.Customs.ES.TemporaryStorage.GUI;
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
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 241, true);
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.TemporaryStorage.Business.CsvCodeInfo);
		// 
		// CSVClearance
		// 
		this.BindingSource.SetBindingMember(this.CSVClearance, "CsvCodeFromUser");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.TemporaryStorage.Business.CsvCodeInfo)(null)).CsvCodeFromUser)));
		this.CSVClearance.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 58, true);
		this.CSVClearance.Name = "CSVClearance";
		this.CSVClearance.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
		this.CSVClearance.TabIndex = 2;
		// 
		// ClearanceDate
		// 
		this.ClearanceDate.AllowDrop = true;
		this.ClearanceDate.AutoCompleteMonthThreshold = 1;
		this.BindingSource.SetBindingMember(this.ClearanceDate, "ClearanceDateFromUser");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.TemporaryStorage.Business.CsvCodeInfo)(null)).ClearanceDateFromUser)));
		this.ClearanceDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
		this.ClearanceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 84, true);
		this.ClearanceDate.Name = "ClearanceDate";
		this.ClearanceDate.TabIndex = 3;
		// 
		// OKButton
		// 
		this.OKButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("9b1eb768-175b-4d30-b06f-a8d036be7867", "&OK");
		this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 161, true);
		this.OKButton.Name = "OKButton";
		this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
		this.OKButton.TabIndex = 6;
		this.OKButton.ToolTipCaption = null;
		this.OKButton.UseVisualStyleBackColor = true;
		// 
		// Cancel_Button
		// 
		this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("8a121ea1-9d88-44d1-aea2-54135ac4dfa6", "&Cancel");
		this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 161, true);
		this.Cancel_Button.Name = "Cancel_Button";
		this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
		this.Cancel_Button.TabIndex = 7;
		this.Cancel_Button.ToolTipCaption = null;
		this.Cancel_Button.UseVisualStyleBackColor = true;
		// 
		// MessageLabel
		// 
		this.MessageLabel.AutoSize = true;
		this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
		this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
		this.MessageLabel.Name = "MessageLabel";
		this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 27, true);
		this.MessageLabel.TabIndex = 0;
		// 
		// UpdateCSVClearanceFormMaxLength
		// 
		this.CaptionRenderingEnabled = true;
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 265, true);
		this.Controls.Add(this.MessageLabel);
		this.Controls.Add(this.ClearanceDate);
		this.Controls.Add(this.CSVClearance);
		this.Controls.Add(this.OKButton);
		this.Controls.Add(this.Cancel_Button);
		this.DataSourceType = typeof(Enterprise.Customs.ES.TemporaryStorage.Business.CsvCodeInfo);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.Name = "UpdateCSVClearanceForm";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		//this.Text = "Update CSV Clearance";
		this.Controls.SetChildIndex(this.Cancel_Button, 0);
		this.Controls.SetChildIndex(this.OKButton, 0);
		this.Controls.SetChildIndex(this.CSVClearance, 0);
		this.Controls.SetChildIndex(this.ClearanceDate, 0);
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
	private ZArchitecture.GUI.ZButton OKButton;
	private ZArchitecture.GUI.ZButton Cancel_Button;
}

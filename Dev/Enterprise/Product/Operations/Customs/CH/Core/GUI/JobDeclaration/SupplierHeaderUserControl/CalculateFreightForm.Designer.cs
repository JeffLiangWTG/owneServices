
namespace Enterprise.Customs.CH.GUI;

partial class CalculateFreightForm
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
    protected override void InitializeComponent()
    {
        this.TotalAmountCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
        this.PercentageToCHBoarderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.PercentageToFinalDestinationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.AmountToCHBorderCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.AmountToFinalDestinationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
        this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.QuitButton = new Enterprise.ZArchitecture.GUI.ZButton();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.TotalAmountCurrencyControl.SuspendLayout();
        this.SuspendLayout();
        // 
        // MainStatusBar
        // 
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
        // 
        // BindingSource
        // 
        this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.CalculateFreightBizObj);
        // 
        // TotalAmountCurrencyControl
        // 
        this.TotalAmountCurrencyControl.AllowDrop = true;
        this.TotalAmountCurrencyControl.BindToAmount = "TotalAmount";
        this.TotalAmountCurrencyControl.BindToUnit = "Currency";
        this.TotalAmountCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
        this.TotalAmountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 8, true);
        this.TotalAmountCurrencyControl.Name = "TotalAmountCurrencyControl";
        this.TotalAmountCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
        this.TotalAmountCurrencyControl.TabIndex = 0;
        // 
        // PercentageToCHBoarderCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.PercentageToCHBoarderCalcEdit, "PercentageToCHBoarder");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CalculateFreightBizObj)(null)).PercentageToCHBoarder)));
        this.PercentageToCHBoarderCalcEdit.CaptionResourceString = null;
        this.PercentageToCHBoarderCalcEdit.DecimalPlaces = 2;
        this.PercentageToCHBoarderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 34, true);
        this.PercentageToCHBoarderCalcEdit.Name = "PercentageToCHBoarderCalcEdit";
        this.PercentageToCHBoarderCalcEdit.ShouldEscapeAllSpecialCharacters = false;
        this.PercentageToCHBoarderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
        this.PercentageToCHBoarderCalcEdit.TabIndex = 1;
        this.PercentageToCHBoarderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // PercentageToFinalDestinationCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.PercentageToFinalDestinationCalcEdit, "PercentageToFinalDestination");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CalculateFreightBizObj)(null)).PercentageToFinalDestination)));
        this.PercentageToFinalDestinationCalcEdit.CaptionResourceString = null;
        this.PercentageToFinalDestinationCalcEdit.DecimalPlaces = 2;
        this.PercentageToFinalDestinationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 60, true);
        this.PercentageToFinalDestinationCalcEdit.Name = "PercentageToFinalDestinationCalcEdit";
        this.PercentageToFinalDestinationCalcEdit.ShouldEscapeAllSpecialCharacters = false;
        this.PercentageToFinalDestinationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
        this.PercentageToFinalDestinationCalcEdit.TabIndex = 2;
        this.PercentageToFinalDestinationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // AmountToCHBorderCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.AmountToCHBorderCalcEdit, "AmountToCHBorder");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CalculateFreightBizObj)(null)).AmountToCHBorder)));
        this.AmountToCHBorderCalcEdit.CaptionResourceString = null;
        this.AmountToCHBorderCalcEdit.DecimalPlaces = 2;
        this.AmountToCHBorderCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 86, true);
        this.AmountToCHBorderCalcEdit.Name = "AmountToCHBorderCalcEdit";
        this.AmountToCHBorderCalcEdit.ShouldEscapeAllSpecialCharacters = false;
        this.AmountToCHBorderCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
        this.AmountToCHBorderCalcEdit.TabIndex = 3;
        this.AmountToCHBorderCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // AmountToFinalDestinationCalcEdit
        // 
        this.BindingSource.SetBindingMember(this.AmountToFinalDestinationCalcEdit, "AmountToFinalDestination");
        // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
        CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.CalculateFreightBizObj)(null)).AmountToFinalDestination)));
        this.AmountToFinalDestinationCalcEdit.CaptionResourceString = null;
        this.AmountToFinalDestinationCalcEdit.DecimalPlaces = 2;
        this.AmountToFinalDestinationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(161, 112, true);
        this.AmountToFinalDestinationCalcEdit.Name = "AmountToFinalDestinationCalcEdit";
        this.AmountToFinalDestinationCalcEdit.ShouldEscapeAllSpecialCharacters = false;
        this.AmountToFinalDestinationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
        this.AmountToFinalDestinationCalcEdit.TabIndex = 4;
        this.AmountToFinalDestinationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        // 
        // OKButton
        // 
        this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.OKButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("0a41d865-0b20-4a1f-8ef0-a635455ceaa0", "&OK");
        this.OKButton.IsCaptionOverridden = false;
        this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 148, true);
        this.OKButton.Name = "OKButton";
        this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
        this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.OKButton.TabIndex = 5;
        this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        this.OKButton.ToolTipCaption = null;
        this.OKButton.UseVisualStyleBackColor = true;
        this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
        // 
        // QuitButton
        // 
        this.QuitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.QuitButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("df359eee-6c4b-4e23-a1f4-a89d3cbaa27d", "&Cancel");
        this.QuitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.QuitButton.IsCaptionOverridden = false;
        this.QuitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 148, true);
        this.QuitButton.Name = "cancelButton";
        this.QuitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
        this.QuitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.QuitButton.TabIndex = 6;
        this.QuitButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        this.QuitButton.ToolTipCaption = null;
        this.QuitButton.UseVisualStyleBackColor = true;
        this.QuitButton.Click += new System.EventHandler(this.cancelButton_Click);
        // 
        // CalculateFreightForm
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.CancelButton = this.QuitButton;
        this.CaptionRenderingEnabled = true;
        this.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("004ae633-1d68-45ee-9e65-a38ca5529da6", "Calculate Freight");
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 201, true);
        this.Controls.Add(this.QuitButton);
        this.Controls.Add(this.OKButton);
        this.Controls.Add(this.AmountToFinalDestinationCalcEdit);
        this.Controls.Add(this.AmountToCHBorderCalcEdit);
        this.Controls.Add(this.PercentageToFinalDestinationCalcEdit);
        this.Controls.Add(this.PercentageToCHBoarderCalcEdit);
        this.Controls.Add(this.TotalAmountCurrencyControl);
        this.DataSourceType = typeof(Enterprise.Customs.CH.Business.CalculateFreightBizObj);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.Name = "CalculateFreightForm";
        this.Controls.SetChildIndex(this.TotalAmountCurrencyControl, 0);
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        this.Controls.SetChildIndex(this.PercentageToCHBoarderCalcEdit, 0);
        this.Controls.SetChildIndex(this.PercentageToFinalDestinationCalcEdit, 0);
        this.Controls.SetChildIndex(this.AmountToCHBorderCalcEdit, 0);
        this.Controls.SetChildIndex(this.AmountToFinalDestinationCalcEdit, 0);
        this.Controls.SetChildIndex(this.OKButton, 0);
        this.Controls.SetChildIndex(this.QuitButton, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.TotalAmountCurrencyControl.ResumeLayout(true);
        this.TotalAmountCurrencyControl.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl TotalAmountCurrencyControl;
    internal ZArchitecture.ZCalcEdit PercentageToFinalDestinationCalcEdit;
    internal ZArchitecture.ZCalcEdit AmountToCHBorderCalcEdit;
    internal ZArchitecture.ZCalcEdit AmountToFinalDestinationCalcEdit;
    internal ZArchitecture.ZCalcEdit PercentageToCHBoarderCalcEdit;
    internal ZArchitecture.GUI.ZButton OKButton;
    internal ZArchitecture.GUI.ZButton QuitButton;
}

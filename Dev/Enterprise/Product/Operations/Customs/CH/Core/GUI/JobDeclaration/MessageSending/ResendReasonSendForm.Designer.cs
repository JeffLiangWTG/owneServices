namespace Enterprise.Customs.CH.GUI;

partial class ResendReasonSendForm
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
        this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
        this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.ZCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
        this.WarningMessageLabel = new Enterprise.ZArchitecture.ZLabel();
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
        this.SuspendLayout();
        // 
        // MainStatusBar
        // 
        this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
        this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 25, true);
        // 
        // ReasonTextBox
        // 
        this.ReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ReasonTextBox.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("7739381C-A4BF-403D-9A40-7DAB33ED928C", "Reason");
        this.ReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
        this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 117, true);
        this.ReasonTextBox.Multiline = true;
        this.ReasonTextBox.Name = "ReasonTextBox";
        this.ReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 54, true);
        this.ReasonTextBox.TabIndex = 1;
        // 
        // OKButton
        // 
        this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.OKButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("CD0080A3-7C84-4059-A344-3AFB5E6385FD", "OK");
        this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 177, true);
        this.OKButton.Name = "OKButton";
        this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.OKButton.TabIndex = 2;
        this.OKButton.ToolTipCaption = null;
        this.OKButton.UseVisualStyleBackColor = true;
        this.OKButton.Click += new System.EventHandler(this.SendButton_Click);
        // 
        // ZCancelButton
        // 
        this.ZCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ZCancelButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("A187F391-B1E6-4F07-B095-AB64FA7F393B", "Cancel");
        this.ZCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.ZCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(597, 177, true);
        this.ZCancelButton.Name = "ZCancelButton";
        this.ZCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
        this.ZCancelButton.TabIndex = 3;
        this.ZCancelButton.ToolTipCaption = null;
        this.ZCancelButton.UseVisualStyleBackColor = true;
        // 
        // WarningMessageLabel
        // 
        this.WarningMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.WarningMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
        this.WarningMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 13, true);
        this.WarningMessageLabel.Name = "WarningMessageLabel";
        this.WarningMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 100, true);
        this.WarningMessageLabel.TabIndex = 4;
        // 
        // ConfirmSendForm
        // 
        this.CaptionRenderingEnabled = true;
        this.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("721E92D7-FBD8-4D15-AF30-C6E66EFC46DE", "Warning - Continue submitting?");
        this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 231, true);
        this.Controls.Add(this.WarningMessageLabel);
        this.Controls.Add(this.ReasonTextBox);
        this.Controls.Add(this.ZCancelButton);
        this.Controls.Add(this.OKButton);
        this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
        this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
        this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 270, true);
        this.MinimizeBox = false;
        this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 270, true);
        this.Name = "PassarConfirmSendForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Controls.SetChildIndex(this.OKButton, 0);
        this.Controls.SetChildIndex(this.ZCancelButton, 0);
        this.Controls.SetChildIndex(this.ReasonTextBox, 0);
        this.Controls.SetChildIndex(this.MainStatusBar, 0);
        this.Controls.SetChildIndex(this.WarningMessageLabel, 0);
        ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    internal Enterprise.ZArchitecture.ZTextBox ReasonTextBox;
    internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
    internal Enterprise.ZArchitecture.GUI.ZButton ZCancelButton;
    internal ZArchitecture.ZLabel WarningMessageLabel;
}

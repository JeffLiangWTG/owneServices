namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

partial class DsdtSdFormatUserControl
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
		this.components = new System.ComponentModel.Container();
		this.DsdtSdFormatNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.GoToUrlButton = new Enterprise.ZArchitecture.GUI.ZButton();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.DsdtSdFormatNumberTextBox.SuspendLayout();
		this.GoToUrlButton.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
		// 
		// DsdtSdFormatNumberTextBox
		//
		this.DsdtSdFormatNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
| System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.DsdtSdFormatNumberTextBox, "DsdtMrnNumberSdFormat");
		this.DsdtSdFormatNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("97BB8F1A-A4ED-429D-891F-BA8F0C1B8767", "DSDT (SD Format)");
		this.DsdtSdFormatNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.DsdtSdFormatNumberTextBox.Name = "DsdtSdFormatNumberTextBox";
		this.DsdtSdFormatNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
		this.DsdtSdFormatNumberTextBox.TabIndex = 2;
		this.DsdtSdFormatNumberTextBox.TabStop = false;
		// 
		// GoToUrlButton
		// 
		this.GoToUrlButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
		this.GoToUrlButton.Dock = System.Windows.Forms.DockStyle.Right;
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoToUrlButton, false);
		this.GoToUrlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 0, true);
		this.GoToUrlButton.Name = "GoToUrlButton";
		this.GoToUrlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
		this.GoToUrlButton.TabIndex = 27;
		this.GoToUrlButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.GoToUrlButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
		this.GoToUrlButton.ToolTipCaption = null;
		this.GoToUrlButton.UseVisualStyleBackColor = true;
		this.GoToUrlButton.Click += new System.EventHandler(this.GoToUrlButton_Click);
		// 
		// DsdtSdFormatUserControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.DsdtSdFormatNumberTextBox);
		this.Controls.Add(this.GoToUrlButton);
		this.Name = "DsdtSdFormatUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.DsdtSdFormatNumberTextBox.ResumeLayout(false);
		this.DsdtSdFormatNumberTextBox.PerformLayout();
		this.GoToUrlButton.ResumeLayout(false);
		this.GoToUrlButton.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	internal ZArchitecture.ZTextBox DsdtSdFormatNumberTextBox;
	internal Enterprise.ZArchitecture.GUI.ZButton GoToUrlButton;
}

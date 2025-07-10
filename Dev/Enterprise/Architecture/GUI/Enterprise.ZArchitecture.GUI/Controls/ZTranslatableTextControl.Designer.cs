using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture
{
	public partial class ZTranslatableTextControl
	{

		ZDropButtonOnly zDropButton;
		ZLabel zLanguageText;
		ZTextBox zTextBox;

		void InitializeComponent()
		{
			this.zTextBox = new ZTextBox();
			this.zDropButton = new ZDropButtonOnly();
			this.zLanguageText = new ZLabel();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			((ISupportInitialize)(this.zDropButton)).BeginInit();
			this.SuspendLayout();
			// 
			// zTextBox
			// 
			this.zTextBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox, false);
			this.zTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTextBox.Name = "zTextBox";
			this.zTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zTextBox.TabIndex = 0;
			// 
			// zDropButton
			// 
			this.zDropButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.zDropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 1, true);
			this.zDropButton.Name = "zDropButton";
			this.zDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.zDropButton.TabIndex = 2;
			this.zDropButton.UseVisualStyleBackColor = true;
			this.zDropButton.Click += new EventHandler(this.zDropButton_Click);
			// 
			// zLanguageText
			// 
			this.zLanguageText.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.zLanguageText.BackColor = System.Drawing.SystemColors.Window;
			this.zLanguageText.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.zLanguageText.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("c6e39c2e-2f29-4003-bb95-c404adb4b276", "ENG");
			this.zLanguageText.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.zLanguageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 1, true);
			this.zLanguageText.Name = "zLanguageText";
			this.zLanguageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 19, true);
			this.zLanguageText.TabIndex = 3;
			// 
			// ZTranslatableTextControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zDropButton);
			this.Controls.Add(this.zTextBox);
			this.Controls.Add(this.zLanguageText);
			this.Name = "ZTranslatableTextControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			((ISupportInitialize)(this.zDropButton)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

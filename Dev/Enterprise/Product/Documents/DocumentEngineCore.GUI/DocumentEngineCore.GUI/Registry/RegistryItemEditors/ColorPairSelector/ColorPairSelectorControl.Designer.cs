namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class ColorPairSelectorControl
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
			this.PrimaryColorChangeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviewPanel = new Enterprise.ZArchitecture.ZLabel();
			this.SecondaryColorChangeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.PreviewPanel.SuspendLayout();
			this.SuspendLayout();

			//
			// GroupBox
			//
			this.GroupBox.BackColor = System.Drawing.Color.Transparent;
			this.GroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ColorPairSelectorControl|0f0a6870-1621-4760-9288-bd6c949a5a37", "Selected Color Preview");
			this.GroupBox.Controls.Add(this.PreviewPanel);
			this.GroupBox.Controls.Add(this.PrimaryColorChangeButton);
			this.GroupBox.Controls.Add(this.SecondaryColorChangeButton);
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 174, true);
			this.GroupBox.TabIndex = 0;
			this.GroupBox.TabStop = false;
			//
			// PreviewPanel
			//
			this.PreviewPanel.BackColor = System.Drawing.Color.DimGray;
			this.PreviewPanel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ColorPairSelectorControl|8ab0a281-f9f5-4724-8a7f-efc576c1a943", "SAMPLE TEXT");
			this.PreviewPanel.ForeColor = System.Drawing.Color.Silver;
			this.PreviewPanel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PreviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PreviewPanel.Name = "PreviewPanel";
			this.PreviewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 109, true);
			this.PreviewPanel.TabIndex = 0;
			//
			// PrimaryColorChangeButton
			//
			this.PrimaryColorChangeButton.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ColorPairSelectorControl|a6255c9a-753e-40eb-abc8-8209fb1ad39f", "Change Font Color");
			this.PrimaryColorChangeButton.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PrimaryColorChangeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 125, true);
			this.PrimaryColorChangeButton.Name = "PrimaryColorChangeButton";
			this.PrimaryColorChangeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 23, true);
			this.PrimaryColorChangeButton.TabIndex = 1;
			this.PrimaryColorChangeButton.UseVisualStyleBackColor = true;
			this.PrimaryColorChangeButton.Click += new System.EventHandler(this.PrimaryColorChangeButton_Click);
			//
			// SecondaryColorChangeButton
			//
			this.SecondaryColorChangeButton.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ColorPairSelectorControl|5f1c48f9-5cfa-45db-a50f-ff0062b7410f1", "Change Background Color");
			this.SecondaryColorChangeButton.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SecondaryColorChangeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 148, true);
			this.SecondaryColorChangeButton.Name = "SecondaryColorChangeButton";
			this.SecondaryColorChangeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 23, true);
			this.SecondaryColorChangeButton.TabIndex = 2;
			this.SecondaryColorChangeButton.UseVisualStyleBackColor = true;
			this.SecondaryColorChangeButton.Click += new System.EventHandler(this.SecondaryColorChangeButton_Click);
			//
			// ColorPairSelectorControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "ColorPairSelectorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 186, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.PreviewPanel.ResumeLayout(false);
			this.PreviewPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox GroupBox;
		internal Enterprise.ZArchitecture.GUI.ZButton PrimaryColorChangeButton;
		internal Enterprise.ZArchitecture.GUI.ZButton SecondaryColorChangeButton;
		internal Enterprise.ZArchitecture.ZLabel PreviewPanel;
	}
}

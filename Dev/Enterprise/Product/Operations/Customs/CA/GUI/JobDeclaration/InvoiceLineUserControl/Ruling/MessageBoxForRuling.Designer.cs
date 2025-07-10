namespace Enterprise.Customs.CA.GUI
{
	partial class MessageBoxForRuling
	{
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
		private void InitializeComponent()
		{
			this.WithImporterRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.WithoutImporterRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// Button1
			//
			this.Button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 74, true);
			this.Button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			//
			// Button2
			//
			this.Button2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 74, true);
			this.Button2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23);
			//
			// TextBox
			//
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 8, true);
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 28);
			// 
			// WithImporterRadioButton
			// 
			this.WithImporterRadioButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2DC5BB4F-790B-4106-AC8F-497DBEB3ED85", "Add as Organization specific");
			this.WithImporterRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WithImporterRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 41, true);
			this.WithImporterRadioButton.Name = "WithImporterRadioButton";
			this.WithImporterRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17);
			this.WithImporterRadioButton.TabIndex = 0;
			this.WithImporterRadioButton.UseVisualStyleBackColor = true;
			this.WithImporterRadioButton.Checked = true;
			// 
			// WithoutImporterRadioButton
			// 
			this.WithoutImporterRadioButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24B1AFF5-D994-469E-9EF4-5DBF5A5AC784", "Add for all Organizations");
			this.WithoutImporterRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WithoutImporterRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 41, true);
			this.WithoutImporterRadioButton.Name = "WithoutImporterRadioButton";
			this.WithoutImporterRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17);
			this.WithoutImporterRadioButton.TabIndex = 1;
			this.WithoutImporterRadioButton.UseVisualStyleBackColor = true;
			// 
			// ZMessageBoxWithCheckbox
			// 
			this.Controls.Add(this.WithImporterRadioButton);
			this.Controls.Add(this.WithoutImporterRadioButton);
			this.Name = "ZMessageBoxWithCheckbox";
			this.Controls.SetChildIndex(this.WithImporterRadioButton, 0);
			this.Controls.SetChildIndex(this.WithoutImporterRadioButton, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.PictureBox, 0);
			this.Controls.SetChildIndex(this.Button1, 0);
			this.Controls.SetChildIndex(this.Button2, 0);
			this.Controls.SetChildIndex(this.Button3, 0);
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 140);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.ZArchitecture.GUI.ZRadioButton WithImporterRadioButton;
		public Enterprise.ZArchitecture.GUI.ZRadioButton WithoutImporterRadioButton;

		#endregion
	}
}

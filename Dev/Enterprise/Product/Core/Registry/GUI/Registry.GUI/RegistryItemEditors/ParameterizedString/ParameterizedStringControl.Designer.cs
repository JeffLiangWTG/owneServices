namespace Enterprise.Registry.GUI
{
	partial class ParameterizedStringControl
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
			this.valueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.informationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// valueTextBox
			// 
			this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.valueTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.valueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.valueTextBox.Name = "valueTextBox";
			this.valueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.valueTextBox.TabIndex = 0;
			// 
			// informationTextBox
			// 
			this.informationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.informationTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.informationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.informationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.informationTextBox.Multiline = true;
			this.informationTextBox.Name = "informationTextBox";
			this.informationTextBox.ReadOnly = true;
			this.informationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 106, true);
			this.informationTextBox.TabIndex = 1;
			// 
			// ParameterizedStringControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.informationTextBox);
			this.Controls.Add(this.valueTextBox);
			this.Name = "ParameterizedStringControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox valueTextBox;
		internal ZArchitecture.ZTextBox informationTextBox;
	}
}

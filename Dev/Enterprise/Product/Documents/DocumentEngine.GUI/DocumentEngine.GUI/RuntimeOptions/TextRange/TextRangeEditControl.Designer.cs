using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.TextRange
{
	partial class TextRangeEditControl
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
			this.FromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FromTextBox
			// 
			this.FromTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("62ec5521-6d01-49ea-8fb0-005edfd91642", "From");
			this.FromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 3, true);
			this.FromTextBox.Name = "FromTextBox";
			this.FromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FromTextBox.TabIndex = 2;
			this.FromTextBox.Text = "OTEXTBOX1";
			// 
			// ToTextBox
			// 
			this.ToTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("8ece27d0-15d7-4d06-9183-41f936f43284", "To");
			this.ToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(222, 4, true);
			this.ToTextBox.Name = "ToTextBox";
			this.ToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ToTextBox.TabIndex = 4;
			this.ToTextBox.Text = "OTEXTBOX1";
			// 
			// TextRangeEditControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToTextBox);
			this.Controls.Add(this.FromTextBox);
			this.Name = "TextRangeEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		public Enterprise.ZArchitecture.ZTextBox FromTextBox;
		public Enterprise.ZArchitecture.ZTextBox ToTextBox;
		#endregion
	}
}

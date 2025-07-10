using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class UnknownUserControl : RuntimeOptionUserControl
	{
		ZLabel ErrorLabel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ErrorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ErrorLabel
			// 
			this.ErrorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.ErrorLabel.Name = "ErrorLabel";
			this.ErrorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 23, true);
			this.ErrorLabel.TabIndex = 0;
			this.ErrorLabel.Text = "Error";
			// 
			// UnknownUserControl
			// 
			this.Controls.Add(this.ErrorLabel);
			this.Name = "UnknownUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

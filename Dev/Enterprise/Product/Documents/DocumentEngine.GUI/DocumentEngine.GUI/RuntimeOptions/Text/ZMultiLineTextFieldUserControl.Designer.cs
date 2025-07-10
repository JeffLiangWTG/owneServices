namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class ZMultiLineTextFieldUserControl : TextFieldUserControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FieldTextBox
			// 
			this.FieldTextBox.AcceptsReturn = true;
			this.FieldTextBox.Multiline = true;
			this.FieldTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 72, true);
			// 
			// ZMultiLineTextFieldUserControl
			// 
			this.Name = "ZMultiLineTextFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 80, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
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

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class TextFieldUserControl : RuntimeOptionUserControl
	{
		protected internal Enterprise.ZArchitecture.ZTextBox FieldTextBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FieldTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FieldTextBox
			// 
			this.BindingSource.SetBindingMember(this.FieldTextBox, "Value");
			this.FieldTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("TextFieldUserControl|aa75d0f0-52fa-468c-b119-4d1a8eb6e6e5", "Label");
			this.FieldTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 4, true);
			this.FieldTextBox.Name = "FieldTextBox";
			this.FieldTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.FieldTextBox.TabIndex = 1;
			// 
			// TextFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldTextBox);
			this.Name = "TextFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
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

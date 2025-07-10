namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class LookupFieldUserControl : RuntimeOptionUserControl
	{
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox FieldFindBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.FieldFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FieldFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// FieldFindBox
			// 
			this.FieldFindBox.AllowDrop = true;
			this.FieldFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 4, true);
			this.FieldFindBox.Name = "FieldFindBox";
			this.FieldFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 21, true);
			this.FieldFindBox.TabIndex = 1;
			// 
			// LookupFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldFindBox);
			this.Name = "LookupFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FieldFindBox.ResumeLayout(true);
			this.FieldFindBox.PerformLayout();
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

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class CodeLookupFieldUserControl : RuntimeOptionUserControl
	{
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FieldFindBox;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.FieldFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FieldFindBox
			// 
			this.FieldFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 4, true);
			this.FieldFindBox.Name = "FieldFindBox";
			this.FieldFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 21, true);
			this.FieldFindBox.TabIndex = 1;
			// 
			// CodeLookupFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldFindBox);
			this.Name = "CodeLookupFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 27, true);
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

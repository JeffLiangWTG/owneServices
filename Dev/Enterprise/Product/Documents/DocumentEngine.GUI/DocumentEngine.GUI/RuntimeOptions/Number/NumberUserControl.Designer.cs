namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class NumberUserControl : RuntimeOptionUserControl
	{
		internal Enterprise.ZArchitecture.ZCalcEdit CalcEdit;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CalcEdit, "ZValue");
			this.CalcEdit.Decimals = 0;
			this.CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 2, true);
			this.CalcEdit.Name = "CalcEdit";
			this.CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CalcEdit.TabIndex = 10;
			this.CalcEdit.Text = "0";
			this.CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CalcEdit);
			this.Name = "NumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 27, true);
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

namespace Enterprise.Registry.GUI
{
	partial class ColorSelectorFindBox
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
			this.CurrentColor = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CurrentColor
			// 
			this.CurrentColor.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.CurrentColor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 4, true);
			this.CurrentColor.Name = "CurrentColor";
			this.CurrentColor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.CurrentColor.TabIndex = 2;
			// 
			// ColorSelectorFindBox
			// 
			this.Controls.Add(this.CurrentColor);
			this.Name = "ColorSelectorFindBox";
			this.Controls.SetChildIndex(this.CodeBox, 0);
			this.Controls.SetChildIndex(this.PopupButton, 0);
			this.Controls.SetChildIndex(this.CurrentColor, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel CurrentColor;
	}
}

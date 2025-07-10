using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ColourLegendElement
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
			this.TextPart = new CargoWise.Windows.UI.KLabel();
			this.ColourPart = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TextPart
			// 
			this.TextPart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TextPart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 6, true);
			this.TextPart.Name = "TextPart";
			this.TextPart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 18, true);
			this.TextPart.TabIndex = 0;
			this.TextPart.Text = "TextPart";
			this.TextPart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ColourPart
			// 
			this.ColourPart.Dock = System.Windows.Forms.DockStyle.Left;
			this.ColourPart.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.ColourPart.Name = "ColourPart";
			this.ColourPart.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.ColourPart.TabIndex = 1;
			// 
			// ColourLegendElement
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TextPart);
			this.Controls.Add(this.ColourPart);
			this.Name = "ColourLegendElement";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 30, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KLabel TextPart;
		private CargoWise.Windows.UI.KPanel ColourPart;
	}
}

namespace Enterprise.DocumentScanning.GUI
{
	partial class PageSelectorControl
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
			this.PageNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.TotalPagesLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PageNumericUpDown)).BeginInit();
			this.PageNumericUpDown.SuspendLayout();
			this.SuspendLayout();
			// 
			// PageNumericUpDown
			// 
			this.PageNumericUpDown.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("PageSelectorControl|3e2fe871-f958-486e-92aa-df78a0afeb72", "Page:");
			this.PageNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PageNumericUpDown.IsNeverReadOnly = true;
			this.PageNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 0, true);
			this.PageNumericUpDown.Name = "PageNumericUpDown";
			this.PageNumericUpDown.ReverseUpDownButtons = true;
			this.PageNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.PageNumericUpDown.TabIndex = 1;
			this.PageNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.PageNumericUpDown.ValueChanged += new System.EventHandler(this.PageNumericUpDown_ValueChanged);
			// 
			// TotalPagesLabel
			// 
			this.TotalPagesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalPagesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalPagesLabel, false);
			this.TotalPagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 2, true);
			this.TotalPagesLabel.Name = "TotalPagesLabel";
			this.TotalPagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 15, true);
			this.TotalPagesLabel.TabIndex = 3;
			// 
			// PageSelectorControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TotalPagesLabel);
			this.Controls.Add(this.PageNumericUpDown);
			this.Name = "PageSelectorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PageNumericUpDown)).EndInit();
			this.PageNumericUpDown.ResumeLayout(false);
			this.PageNumericUpDown.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZNumericUpDown PageNumericUpDown;
		private ZArchitecture.ZLabel TotalPagesLabel;
	}
}

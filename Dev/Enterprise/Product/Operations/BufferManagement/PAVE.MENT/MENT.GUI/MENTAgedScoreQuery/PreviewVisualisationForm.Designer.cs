namespace Enterprise.PAVE.MENT.GUI
{
	partial class PreviewVisualisationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
#if !WINZOR
		Enterprise.ZArchitecture.GUI.OxyplotView plotView;
		MENTChartWindowsControl chartControl;

#endif

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

			if (disposing)
			{
#if !WINZOR
				if (plotView != null)
				{
					plotView.Dispose();
				}

				if (chartControl != null)
				{
					chartControl.Dispose();
				}
#endif
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PreviewVisualisationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.PAVE.MENT.GUI.Res.GetData("44940b69-d1b5-4c41-9eb8-b8978f941971", "Visualization Preview");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 238, true);
			this.Name = "PreviewVisualisationForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}

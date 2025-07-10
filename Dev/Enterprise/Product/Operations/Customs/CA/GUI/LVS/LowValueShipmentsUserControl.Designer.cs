namespace Enterprise.Customs.CA.GUI
{
	partial class LowValueShipmentsUserControl
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
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.AutoScroll = true;
			this.DeclarationTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1270, 710, true);
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa704885-1024-409c-9fa2-36cc3a7abc31", "Low Value Shipments");
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("97ccfc02-109b-4a5e-87d8-b85f05f8da24", "Messages");
			// 
			// LowValueShipmentsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "LowValueShipmentsUserControl";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}

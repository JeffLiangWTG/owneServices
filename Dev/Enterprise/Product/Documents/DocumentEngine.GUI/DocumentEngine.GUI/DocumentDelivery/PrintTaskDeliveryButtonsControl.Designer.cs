namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class PrintTaskDeliveryButtonsControl
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
			this.DeliverButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.VisualiseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// DeliverButton
			// 
			this.DeliverButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryButtonsControl|ec99c4b2-faf3-492c-9a12-f0320fda5805", "&Deliver");
			this.DeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.DeliverButton.Name = "DeliverButton";
			this.DeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.DeliverButton.TabIndex = 0;
			this.DeliverButton.Click += new System.EventHandler(this.DeliverButton_Click);
			// 
			// VisualiseButton
			// 
			this.VisualiseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryButtonsControl|9bd312bb-7333-4f8e-acc1-e63345acb04e", "&Modify");
			this.VisualiseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 1, true);
			this.VisualiseButton.Name = "VisualiseButton";
			this.VisualiseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.VisualiseButton.TabIndex = 1;
			this.VisualiseButton.Click += new System.EventHandler(this.VisualiseButton_Click);
			// 
			// PreviewButton
			// 
			this.PreviewButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryButtonsControl|2c72da7c-0f32-40be-b1a8-315a7c58af0d", "Pre&view");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 1, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.PreviewButton.TabIndex = 2;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryButtonsControl|bf7faed6-cdec-44c9-a2ec-f0d83dff1ae2", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 1, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 3;
			// 
			// PrintTaskDeliveryButtonsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliverButton);
			this.Controls.Add(this.VisualiseButton);
			this.Controls.Add(this.PreviewButton);
			this.Controls.Add(this.CloseButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 25, true);
			this.Name = "PrintTaskDeliveryButtonsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton DeliverButton;
		protected Enterprise.ZArchitecture.GUI.ZButton VisualiseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton PreviewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}

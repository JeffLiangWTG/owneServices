namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class IncidentEmailTemplatePairRegistryControl
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
		void InitializeComponent()
		{
			this.customerServiceIncidentEmailTemplatesControl = new Enterprise.Client.EDI.Registry.GUI.IncidentEmailTemplatePairControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.IncidentEmailTemplatePair);
			// 
			// notificationEmailTemplateControl1
			// 
			this.BindingSource.SetBindingMember(this.customerServiceIncidentEmailTemplatesControl, ".");
			this.customerServiceIncidentEmailTemplatesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customerServiceIncidentEmailTemplatesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customerServiceIncidentEmailTemplatesControl.Name = "notificationEmailTemplateControl1";
			this.customerServiceIncidentEmailTemplatesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 395, true);
			this.customerServiceIncidentEmailTemplatesControl.TabIndex = 0;
			// 
			// CustomerServiceIncidentEmailTemplatesRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.customerServiceIncidentEmailTemplatesControl);
			this.Name = "CustomerServiceIncidentEmailTemplatesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 395, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private IncidentEmailTemplatePairControl customerServiceIncidentEmailTemplatesControl;
	}
}

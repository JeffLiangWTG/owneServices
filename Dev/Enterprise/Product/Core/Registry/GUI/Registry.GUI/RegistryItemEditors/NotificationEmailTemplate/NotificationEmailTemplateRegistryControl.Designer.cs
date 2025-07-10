namespace Enterprise.Registry.GUI
{
	partial class NotificationEmailTemplateRegistryControl
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
			this.notificationEmailTemplateControl1 = new Enterprise.Registry.GUI.NotificationEmailTemplateControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.NotificationEmailTemplate);
			// 
			// notificationEmailTemplateControl1
			// 
			this.BindingSource.SetBindingMember(this.notificationEmailTemplateControl1, ".");
			this.notificationEmailTemplateControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.notificationEmailTemplateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.notificationEmailTemplateControl1.Name = "notificationEmailTemplateControl1";
			this.notificationEmailTemplateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 395, true);
			this.notificationEmailTemplateControl1.TabIndex = 0;
			// 
			// NotificationEmailTemplateRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.notificationEmailTemplateControl1);
			this.Name = "NotificationEmailTemplateRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 395, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private NotificationEmailTemplateControl notificationEmailTemplateControl1;
	}
}

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class SeaCargoHouseBillCustomFieldsUserControl
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
			this.CustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.NothingSetupMessageLabelText = "To make use of this tab, please setup customized fields captions and hints in...";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 135, true);
			this.CustomFieldsControl.TabIndex = 1;
			// 
			// AsycudaBillCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomFieldsControl);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "SeaCargoHouseBillCustomFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomFieldsControl.ResumeLayout(true);
			this.CustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsControl;

		#endregion
	}
}

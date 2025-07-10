using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentManagementGroupCustomFieldsControl : ZUserControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.customFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.processTemplateCustomFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			customFieldsGroupBox.SuspendLayout();
			processTemplateCustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ProcessManagement.Business.WorkItem);
			// 
			// CustomFieldsGroupBox
			// 
			this.customFieldsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("D906B2B9-C160-4D73-8DE7-C830B69AD2B1", "Custom Fields");
			this.customFieldsGroupBox.Controls.Add(this.processTemplateCustomFieldsControl);
			this.customFieldsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customFieldsGroupBox.Name = "customFieldsGroupBox";
			this.customFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 296, true);
			this.customFieldsGroupBox.TabIndex = 0;
			this.customFieldsGroupBox.TabStop = false;
			// 
			// processTemplateCustomFieldsControl
			// 
			this.processTemplateCustomFieldsControl.AllowDrop = true;
			this.processTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.processTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.processTemplateCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.processTemplateCustomFieldsControl.Name = "processTemplateCustomFieldsControl";
			this.processTemplateCustomFieldsControl.NothingSetupMessageLabelText = "";
			this.processTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 280, true);
			this.processTemplateCustomFieldsControl.TabIndex = 0;
			// 
			// IncidentManagementGroupCustomFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.customFieldsGroupBox);
			this.Name = "IncidentManagementGroupCustomFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 296, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			customFieldsGroupBox.ResumeLayout(false);
			customFieldsGroupBox.PerformLayout();
			processTemplateCustomFieldsControl.ResumeLayout(false);
			processTemplateCustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox customFieldsGroupBox;
		ZArchitecture.GUI.ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl;
	}
}

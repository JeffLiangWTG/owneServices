using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class OrganisationConsignorPlugInUserControl
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
			this.CustomsDefaultsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zCheckBox14UseIndirectRepresentationForExporter = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsDefaultsPanel.SuspendLayout();
			this.zCheckBox14UseIndirectRepresentationForExporter.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = GetDataSourceType();
			// 
			// EUPanel
			// 
			this.CustomsDefaultsPanel.Controls.Add(this.zCheckBox14UseIndirectRepresentationForExporter);
			this.CustomsDefaultsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDefaultsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsDefaultsPanel.Name = "CustomsDefaultsPanel";
			this.CustomsDefaultsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomsDefaultsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 160, true);
			this.CustomsDefaultsPanel.TabIndex = 1;
			this.CustomsDefaultsPanel.Text = "Customs Defaults";
			// 
			// zCheckBoxRepresentationType
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox14UseIndirectRepresentationForExporter, "ZO_Box14UseIndirectRepresentationForExporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EUOrgImpAddInfo)(null)).ZO_Box14UseIndirectRepresentationForExporter)));
			this.zCheckBox14UseIndirectRepresentationForExporter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 20, true);
			this.zCheckBox14UseIndirectRepresentationForExporter.Name = "zCheckBox14UseIndirectRepresentationForExporter";
			this.zCheckBox14UseIndirectRepresentationForExporter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.zCheckBox14UseIndirectRepresentationForExporter.TabIndex = 1;
			// 
			// OrganisationConsigneePlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsDefaultsPanel);
			this.Name = "OrganisationConsigneePlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsDefaultsPanel.ResumeLayout(false);
			this.CustomsDefaultsPanel.PerformLayout();
			this.zCheckBox14UseIndirectRepresentationForExporter.ResumeLayout(true);
			this.zCheckBox14UseIndirectRepresentationForExporter.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel CustomsDefaultsPanel;
		private ZCheckBox zCheckBox14UseIndirectRepresentationForExporter;
	}
}

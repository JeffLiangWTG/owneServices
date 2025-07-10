
namespace Enterprise.Customs.GB.GUI.Registry.CDSUCRAutomation
{
	partial class CDSUCRAutomationControl
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
            this.comboBoxCDSAutomationSettings = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.comboBoxCDSAutomationSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.CDSDUCRAutomationSettings);
            // 
            // comboBoxCDSAutomationSettings
            // 
            this.comboBoxCDSAutomationSettings.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.comboBoxCDSAutomationSettings, "CDSDUCRAutomation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Registry.CDSDUCRAutomationSettings)(null)).CDSDUCRAutomation)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.comboBoxCDSAutomationSettings, false);
            this.comboBoxCDSAutomationSettings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 25, true);
            this.comboBoxCDSAutomationSettings.Name = "comboBoxCDSAutomationSettings";
            this.comboBoxCDSAutomationSettings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
            this.comboBoxCDSAutomationSettings.TabIndex = 1;
            // 
            // CDSUCRAutomationControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.comboBoxCDSAutomationSettings);
            this.Name = "CDSUCRAutomationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 92, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.comboBoxCDSAutomationSettings.ResumeLayout(true);
            this.comboBoxCDSAutomationSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}


		#endregion
		public Enterprise.ZArchitecture.GUI.ZDropEdit comboBoxCDSAutomationSettings;
	}
}

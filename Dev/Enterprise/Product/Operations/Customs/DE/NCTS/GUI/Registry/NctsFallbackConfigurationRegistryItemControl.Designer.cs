namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class NctsFallbackConfigurationRegistryItemControl
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
			this.fallbackSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.customsIncidentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.startDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.fallbackSettingsGroupBox.SuspendLayout();
			this.startDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsFallbackConfiguration);
			// 
			// fallbackSettingsGroupBox
			// 
			this.fallbackSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.fallbackSettingsGroupBox.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("ae89a431-bb86-4b35-b9d7-2673fbd0b29f", "Fallback settings");
			this.fallbackSettingsGroupBox.Controls.Add(this.customsIncidentNumberTextBox);
			this.fallbackSettingsGroupBox.Controls.Add(this.startDateEdit);
			this.fallbackSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.fallbackSettingsGroupBox.Name = "fallbackSettingsGroupBox";
			this.fallbackSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 81, true);
			this.fallbackSettingsGroupBox.TabIndex = 0;
			this.fallbackSettingsGroupBox.TabStop = false;
			// 
			// customsIncidentNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.customsIncidentNumberTextBox, "CustomsIncidentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.NctsFallbackConfiguration)(null)).CustomsIncidentNumber)));
			this.customsIncidentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 45, true);
			this.customsIncidentNumberTextBox.Name = "customsIncidentNumberTextBox";
			this.customsIncidentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.customsIncidentNumberTextBox.TabIndex = 1;
			// 
			// startDateEdit
			// 
			this.startDateEdit.AllowDrop = true;
			this.startDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.startDateEdit, "Start");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.NctsFallbackConfiguration)(null)).Start)));
			this.startDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.startDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 19, true);
			this.startDateEdit.Name = "startDateEdit";
			this.startDateEdit.TabIndex = 0;
			// 
			// NctsFallbackConfigurationRegistryItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.fallbackSettingsGroupBox);
			this.Name = "NctsFallbackConfigurationRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 87, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.fallbackSettingsGroupBox.ResumeLayout(false);
			this.fallbackSettingsGroupBox.PerformLayout();
			this.startDateEdit.ResumeLayout(true);
			this.startDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox fallbackSettingsGroupBox;
		private ZArchitecture.GUI.ZDateEdit startDateEdit;
		private ZArchitecture.ZTextBox customsIncidentNumberTextBox;
	}
}

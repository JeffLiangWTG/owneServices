using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class ColumnConfigurationFieldUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			this.comboSettings = new CargoWise.Windows.UI.KComboBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("8D5AEBF9-4990-4B8A-9C58-3D2E6A163BC4", "Configuration");
			this.label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 13, true);
			this.label1.TabIndex = 1;
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboSettings
			// 
			this.comboSettings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSettings.FormattingEnabled = true;
			this.comboSettings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 3, true);
			this.comboSettings.MaxDropDownItems = 50;
			this.comboSettings.Name = "comboSettings";
			this.comboSettings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 21, true);
			this.comboSettings.TabIndex = 2;
			// 
			// ColumnConfigurationFieldUserControl
			// 
			this.Controls.Add(this.comboSettings);
			this.Controls.Add(this.label1);
			this.Name = "ColumnConfigurationFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel label1;
		internal CargoWise.Windows.UI.KComboBox comboSettings;
	}
}

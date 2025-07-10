using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ServiceUrlControl : RegistryZUserControl
	{
		internal ZTextBox textBoxUrl;
		internal ZButton buttonTestConnection;

		void InitializeComponent()
		{
			this.textBoxUrl = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonTestConnection = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Types.ZString);
			// 
			// textBoxUrl
			// 
			this.textBoxUrl.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxUrl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.textBoxUrl.Name = "textBoxUrl";
			this.textBoxUrl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.textBoxUrl.TabIndex = 0;
			// 
			// buttonTestConnection
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.buttonTestConnection, false);
			this.buttonTestConnection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 3, true);
			this.buttonTestConnection.Name = "buttonTestConnection";
			this.buttonTestConnection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 21, true);
			this.buttonTestConnection.TabIndex = 1;
			this.buttonTestConnection.Text = "Test";
			this.buttonTestConnection.Click += new System.EventHandler(this.buttonTestConnection_Click);
			// 
			// ServiceUrlControl
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.textBoxUrl, false);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.textBoxUrl);
			this.Controls.Add(this.buttonTestConnection);
			this.Name = "ServiceUrlControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 48, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

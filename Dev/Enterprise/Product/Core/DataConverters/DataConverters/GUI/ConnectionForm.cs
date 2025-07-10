using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataConverters.GUI
{
	public class ConnectionForm : ZChildForm
	{
		public ConnectionForm(DummyImporter bizO) : base(bizO)
		{
			InitializeComponent();
			this.BizO = bizO;
		}

		#region Windows generated code

		ZArchitecture.ZTextBox ServerTextBox;
		ZArchitecture.ZTextBox UserTextBox;
		ZArchitecture.ZTextBox PasswordTextBox;
		ZButton SetStringButton;
		ZArchitecture.ZTextBox ConnectionTextBox;
		ZButton OKButton;
		ZArchitecture.ZTextBox DataLocationTextBox;

		new void InitializeComponent()
		{
			this.ServerTextBox = new ZArchitecture.ZTextBox();
			this.UserTextBox = new ZArchitecture.ZTextBox();
			this.PasswordTextBox = new ZArchitecture.ZTextBox();
			this.SetStringButton = new ZButton();
			this.ConnectionTextBox = new ZArchitecture.ZTextBox();
			this.OKButton = new ZButton();
			this.DataLocationTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(348);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(349);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DummyImporter);
			// 
			// ServerTextBox
			// 
			this.BindingSource.SetBindingMember(this.ServerTextBox, "ServerName");
			this.ServerTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|b86ece84-0578-4097-b50e-ba7a5db8b043", "Server Name");
			this.ServerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 33, true);
			this.ServerTextBox.Name = "ServerTextBox";
			this.ServerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.ServerTextBox.TabIndex = 1;
			// 
			// UserTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserTextBox, "UserId");
			this.UserTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|dd65c5cc-5e8a-4946-9246-d26d104a65ec", "User ID");
			this.UserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 58, true);
			this.UserTextBox.Name = "UserTextBox";
			this.UserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.UserTextBox.TabIndex = 2;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
			this.PasswordTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|8dd329ce-6756-40a0-94f0-7c2b778ad645", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 83, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PasswordTextBox.TabIndex = 3;
			// 
			// SetStringButton
			// 
			this.SetStringButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|df9fc432-05c3-477b-a54f-e290bed84f64", "Set Connection String");
			this.SetStringButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 82, true);
			this.SetStringButton.Name = "SetStringButton";
			this.SetStringButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 21, true);
			this.SetStringButton.TabIndex = 4;
			this.SetStringButton.Click += new System.EventHandler(this.SetStringButton_Click);
			// 
			// ConnectionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConnectionTextBox, "ConnectionText");
			this.ConnectionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConnectionTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|da6d2202-fab4-4b70-85b6-b24702d68096", "Connection String");
			this.ConnectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 108, true);
			this.ConnectionTextBox.Name = "ConnectionTextBox";
			this.ConnectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 20, true);
			this.ConnectionTextBox.TabIndex = 5;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|071d2a7c-bd4c-4f27-a5d3-58222362963c", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(627, 142, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 6;
			// 
			// DataLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.DataLocationTextBox, "DataLocation");
			this.DataLocationTextBox.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|df606f79-52b3-413e-aa53-9c94e78a605e", "Data Location");
			this.DataLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			this.DataLocationTextBox.Name = "DataLocationTextBox";
			this.DataLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 20, true);
			this.DataLocationTextBox.TabIndex = 0;
			// 
			// ConnectionForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 198, true);
			this.CaptionResourceString = Enterprise.DataConverters.Res.GetData("ConnectionForm|e8d11362-46cf-4ca7-95b3-0cc6f334da67", "Connection String");
			this.Controls.Add(this.DataLocationTextBox);
			this.Controls.Add(this.ConnectionTextBox);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.SetStringButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.UserTextBox);
			this.Controls.Add(this.ServerTextBox);
			this.DataSourceAssemblyName = "Enterprise.DataConverters";
			this.DataSourceType = typeof(DummyImporter);
			this.DataSourceTypeName = "Enterprise.DataConverters.DummyImporter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 215, true);
			this.Name = "ConnectionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ServerTextBox, 0);
			this.Controls.SetChildIndex(this.UserTextBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.SetStringButton, 0);
			this.Controls.SetChildIndex(this.PasswordTextBox, 0);
			this.Controls.SetChildIndex(this.ConnectionTextBox, 0);
			this.Controls.SetChildIndex(this.DataLocationTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		void SetStringButton_Click(object sender, System.EventArgs e)
		{
			BizO.SetConnectionText();
		}

		readonly DummyImporter BizO;
	}
}

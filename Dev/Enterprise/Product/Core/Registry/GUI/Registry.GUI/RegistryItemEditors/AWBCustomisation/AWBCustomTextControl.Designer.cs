using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AWBCustomTextControl : ZUserControl
	{
		internal Enterprise.ZArchitecture.ZTextBox TextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ViewButton;

		void InitializeComponent()
		{
			this.TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TextBox
			// 
			this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TextBox.Name = "TextBox";
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.TextBox.TabIndex = 0;
			// 
			// ViewButton
			// 
			this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBCustomTextControl|a7d6f4cb-eeac-40ec-94b2-cb12c72676c1", "Insert Field", "Displays a list of all available fields that can be included on the Air Waybill.\r\nYou can choose a field and it will automatically be inserted into the registry item value.");
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 0, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 23, true);
			this.ViewButton.TabIndex = 1;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// AWBCustomTextControl
			// 
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.ViewButton);
			this.Name = "AWBCustomTextControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 24, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}

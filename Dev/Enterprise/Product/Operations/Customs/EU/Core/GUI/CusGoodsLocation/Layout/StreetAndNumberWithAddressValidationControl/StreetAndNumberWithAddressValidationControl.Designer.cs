namespace Enterprise.Customs.EU.GUI
{
	public sealed partial class StreetAndNumberWithAddressValidationControl
	{
		void InitializeComponent()
		{
            this.StreetAndNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusGoodsLocation);
            // 
            // StreetAndNumberTextBox
            // 
            this.StreetAndNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top
				| System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.StreetAndNumberTextBox, "Address.E2_Address1AndE2_Address2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusGoodsLocation)(null)).Address.E2_Address1AndE2_Address2)));
            this.StreetAndNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.StreetAndNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.StreetAndNumberTextBox.Name = "StreetAndNumberTextBox";
            this.StreetAndNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.StreetAndNumberTextBox.TabIndex = 1;
            // 
            // ValidateAddressButton
            // 
            this.ValidateAddressButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top
				| System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right)));
            this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 0, true);
            this.ValidateAddressButton.Name = "ValidateAddressButton";
            this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 19, true);
            this.ValidateAddressButton.TabIndex = 2;
            this.ValidateAddressButton.ToolTipCaption = null;
            // 
            // StreetAndNumberWithAddressValidationControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.StreetAndNumberTextBox);
            this.Controls.Add(this.ValidateAddressButton);
            this.Name = "StreetAndNumberWithAddressValidationControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 19, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		internal ZArchitecture.ZTextBox StreetAndNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ValidateAddressButton;
	}
}

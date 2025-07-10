namespace Enterprise.Customs.DE.GUI
{
	partial class DgSubstanceUserControl
	{
		private void InitializeComponent()
		{
			this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DgSubstanceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine);
			// 
			// MoreButton
			// 
			this.MoreButton.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("40291303-ca85-4b75-adbb-31da515fb733", "More..");
			this.MoreButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 0, true);
			this.MoreButton.Name = "MoreButton";
			this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.MoreButton.TabIndex = 1;
			this.MoreButton.ToolTipCaption = null;
			this.MoreButton.Click += new System.EventHandler(this.MoreButton_Click);
			// 
			// DgSubstanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DgSubstanceTextBox, "DgSubstance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(null)).DgSubstance)));
			this.DgSubstanceTextBox.CaptionResourceString = null;
			this.DgSubstanceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DgSubstanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DgSubstanceTextBox.Name = "DgSubstanceTextBox";
			this.DgSubstanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.DgSubstanceTextBox.TabIndex = 0;
			// 
			// DgSubstanceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DgSubstanceTextBox);
			this.Controls.Add(this.MoreButton);
			this.Name = "DgSubstanceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZButton MoreButton;
		internal ZArchitecture.ZTextBox DgSubstanceTextBox;
	}
}

namespace Enterprise.Customs.CH.GUI;

partial class UNDGCodesUserControl
{
	private void InitializeComponent()
	{
		this.MoreButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.UNDGCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
		// 
		// MoreButton
		// 
		this.MoreButton.CaptionResourceString = Enterprise.Customs.CH.GUI.Res.GetData("7d058101-6220-4079-a60b-af05309e8655", "More…");
		this.MoreButton.Dock = System.Windows.Forms.DockStyle.Right;
		this.MoreButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 0, true);
		this.MoreButton.Name = "MoreButton";
		this.MoreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
		this.MoreButton.TabIndex = 1;
		this.MoreButton.ToolTipCaption = null;
		this.MoreButton.Click += new System.EventHandler(this.MoreButton_Click);
		// 
		// UNDGCodesTextBox
		// 
		this.BindingSource.SetBindingMember(this.UNDGCodesTextBox, "UNDGCodes");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).UNDGCodes)));
		this.UNDGCodesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
		this.UNDGCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.UNDGCodesTextBox.Name = "UNDGCodesTextBox";
		this.UNDGCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 17, true);
		this.UNDGCodesTextBox.TabIndex = 0;
		// 
		// UNDGCodesUserControl
		// 
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.UNDGCodesTextBox);
		this.Controls.Add(this.MoreButton);
		this.Name = "UNDGCodesUserControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	internal Enterprise.ZArchitecture.GUI.ZButton MoreButton;
	internal ZArchitecture.ZTextBox UNDGCodesTextBox;
}

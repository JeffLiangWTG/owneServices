namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class AdditionalSupplementaryCodesUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalSupplementaryCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc);
			// 
			// AdditionalSupplementaryCodesTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesTextBox, "BY_Supplements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).BY_Supplements)));
			this.AdditionalSupplementaryCodesTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalSupplementaryCodesTextBox, false);
			this.AdditionalSupplementaryCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalSupplementaryCodesTextBox.Name = "AdditionalSupplementaryCodesTextBox";
			this.AdditionalSupplementaryCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 15, true);
			this.AdditionalSupplementaryCodesTextBox.TabIndex = 0;
			// 
			// AdditionalSupplementaryCodesEditButton
			// 
			this.AdditionalSupplementaryCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("AdditionalSupplementaryCodesEditButton|F801BF99-6F32-454F-B0E6-8247749C1197", "Additional codes...");
			this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 0, true);
			this.AdditionalSupplementaryCodesEditButton.Name = "AdditionalSupplementaryCodesEditButton";
			this.AdditionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.AdditionalSupplementaryCodesEditButton.TabIndex = 1;
			this.AdditionalSupplementaryCodesEditButton.ToolTipCaption = null;
			this.AdditionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalSupplementaryCodesEditButton);
			this.Controls.Add(this.AdditionalSupplementaryCodesTextBox);
			this.Name = "AdditionalSupplementaryCodesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox AdditionalSupplementaryCodesTextBox;
		internal ZArchitecture.GUI.ZButton AdditionalSupplementaryCodesEditButton;
	}
}

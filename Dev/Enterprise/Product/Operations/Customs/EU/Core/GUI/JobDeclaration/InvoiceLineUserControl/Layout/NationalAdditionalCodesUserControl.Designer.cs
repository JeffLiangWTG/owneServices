namespace Enterprise.Customs.EU.GUI
{
	partial class NationalAdditionalCodesUserControl
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
			this.JI_NationalAdditionalCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NationalAdditionalCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// JI_NationalAdditionalCodesTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_NationalAdditionalCodesTextBox, "JI_NationalAdditionalCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).JI_NationalAdditionalCodes)));
			this.JI_NationalAdditionalCodesTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JI_NationalAdditionalCodesTextBox, false);
			this.JI_NationalAdditionalCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JI_NationalAdditionalCodesTextBox.Name = "JI_NationalAdditionalCodesTextBox";
			this.JI_NationalAdditionalCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 15, true);
			this.JI_NationalAdditionalCodesTextBox.TabIndex = 0;
			// 
			// NationalAdditionalCodesEditButton
			// 
			this.NationalAdditionalCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("89EEBA57-69DC-49DF-982D-D565D08A16AF", "More..");
			this.NationalAdditionalCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 0, true);
			this.NationalAdditionalCodesEditButton.Name = "NationalAdditionalCodesEditButton";
			this.NationalAdditionalCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.NationalAdditionalCodesEditButton.TabIndex = 1;
			this.NationalAdditionalCodesEditButton.ToolTipCaption = null;
			this.NationalAdditionalCodesEditButton.Click += new System.EventHandler(this.NationalAdditionalCodesEditButton_OnClick);
			// 
			// NationalAdditionalCodesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NationalAdditionalCodesEditButton);
			this.Controls.Add(this.JI_NationalAdditionalCodesTextBox);
			this.Name = "NationalAdditionalCodesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox JI_NationalAdditionalCodesTextBox;
		private ZArchitecture.GUI.ZButton NationalAdditionalCodesEditButton;
	}
}

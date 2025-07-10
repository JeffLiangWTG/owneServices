namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class SendAccessCodeUserControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendAccessCodeUserControl));
            this.GuaranteeNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.OfficeOfGuaranteeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.NewMainAccessCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.OfficeOfGuaranteeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel);
            // 
            // GuaranteeNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.GuaranteeNumberTextBox, "GuaranteeNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel)(null)).GuaranteeNumber)));
            this.GuaranteeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 11, true);
            this.GuaranteeNumberTextBox.Name = "GuaranteeNumberTextBox";
            this.GuaranteeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.GuaranteeNumberTextBox.TabIndex = 0;
            // 
            // OfficeOfGuaranteeFindBox
            // 
            this.OfficeOfGuaranteeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OfficeOfGuaranteeFindBox, "OfficeOfGuarantee");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel)(null)).OfficeOfGuarantee)));
            this.OfficeOfGuaranteeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 37, true);
            this.OfficeOfGuaranteeFindBox.Name = "OfficeOfGuaranteeFindBox";
            this.OfficeOfGuaranteeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OfficeOfGuaranteeFindBox.ParentType = null;
            this.OfficeOfGuaranteeFindBox.PreBoundMaxLength = 8;
            this.OfficeOfGuaranteeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.OfficeOfGuaranteeFindBox.TabIndex = 1;
            // 
            // NewMainAccessCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.NewMainAccessCodeTextBox, "NewMainAccessCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.SendAccessCodeViewModel)(null)).NewMainAccessCode)));
            this.NewMainAccessCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.NewMainAccessCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 63, true);
            this.NewMainAccessCodeTextBox.Name = "NewMainAccessCodeTextBox";
            this.NewMainAccessCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.NewMainAccessCodeTextBox.TabIndex = 2;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("845acd91-fba9-45b9-a95d-37064fc8b420", "Cancel");
            this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(323, 111, true);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.ToolTipCaption = null;
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // SendButton
            // 
            this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendButton.CaptionResourceString = Enterprise.Customs.DE.NCTS.GUI.Res.GetData("dd35116d-9202-4429-bf63-4dbbeaefe946", "Send");
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 111, true);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.SendButton.TabIndex = 3;
            this.SendButton.ToolTipCaption = null;
            this.SendButton.UseVisualStyleBackColor = true;
            // 
            // SendAccessCodeUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.NewMainAccessCodeTextBox);
            this.Controls.Add(this.OfficeOfGuaranteeFindBox);
            this.Controls.Add(this.GuaranteeNumberTextBox);
            this.Name = "SendAccessCodeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 137, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.OfficeOfGuaranteeFindBox.ResumeLayout(true);
            this.OfficeOfGuaranteeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox GuaranteeNumberTextBox;
		private ZArchitecture.GUI.ZCodeFindBox OfficeOfGuaranteeFindBox;
		private ZArchitecture.ZTextBox NewMainAccessCodeTextBox;
		internal ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton SendButton;
	}
}

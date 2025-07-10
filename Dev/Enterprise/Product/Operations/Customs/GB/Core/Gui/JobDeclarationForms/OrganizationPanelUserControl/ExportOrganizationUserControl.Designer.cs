namespace Enterprise.Customs.GB.GUI
{
	partial class ExportOrganizationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.SupervisingOfficePanel = new CargoWise.Windows.UI.KPanel();
			this.SupervisingOfficeDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.SpoffSetterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConsignorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupervisingOfficePanel.SuspendLayout();
			this.SupervisingOfficeDocAddress.SuspendLayout();
			this.ConsignorAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 90, true);
			this.SellerAddressControl.TabIndex = 3;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 112, true);
			this.ManufacturerAddressControl.TabIndex = 4;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 68, true);
			this.RepresentativeAddressControl.TabIndex = 2;
			// 
			// DeclarantOfficeAddressControl
			// 
			this.DeclarantOfficeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 46, true);
			this.DeclarantOfficeAddressControl.TabIndex = 1;
			// 
			// SupervisingOfficePanel
			// 
			this.SupervisingOfficePanel.Controls.Add(this.SupervisingOfficeDocAddress);
			this.SupervisingOfficePanel.Controls.Add(this.SpoffSetterButton);
			this.SupervisingOfficePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SupervisingOfficePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupervisingOfficePanel.Name = "SupervisingOfficePanel";
			this.SupervisingOfficePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 44, true);
			this.SupervisingOfficePanel.TabIndex = 0;
			// 
			// SupervisingOfficeDocAddress
			// 
			this.SupervisingOfficeDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupervisingOfficeDocAddress, "SupervisingOfficeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).SupervisingOfficeDocAddress)));
			this.SupervisingOfficeDocAddress.BindToOrganisations = "Lookups+SupervisingOfficeCollection";
			this.SupervisingOfficeDocAddress.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("BD952D86-ECBE-4CF9-A7DB-1853A4A9B521", "[44] Supervising Office");
			this.SupervisingOfficeDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.SupervisingOfficeDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 0, true);
			this.SupervisingOfficeDocAddress.Name = "SupervisingOfficeDocAddress";
			this.SupervisingOfficeDocAddress.ReadOnly = false;
			this.SupervisingOfficeDocAddress.SingleLineNoGroupBoxPanelWidth = 320;
			this.SupervisingOfficeDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.SupervisingOfficeDocAddress.TabIndex = 0;
			this.SupervisingOfficeDocAddress.ValidationJustForced = false;
			// 
			// SpoffSetterButton
			// 
			this.SpoffSetterButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("E5A43434-6659-46F2-AE25-629BB27CA7CF", "Set SPOFF from Declarant");
			this.SpoffSetterButton.IsCaptionOverridden = false;
			this.SpoffSetterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 22, true);
			this.SpoffSetterButton.Name = "SpoffSetterButton";
			this.SpoffSetterButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SpoffSetterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.SpoffSetterButton.TabIndex = 1;
			this.SpoffSetterButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SpoffSetterButton.ToolTipCaption = null;
			this.SpoffSetterButton.UseVisualStyleBackColor = true;
			this.SpoffSetterButton.Click += new System.EventHandler(this.SpoffSetterButton_Click);
			// 
			// ConsignorAddressControl
			// 
			this.ConsignorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorAddressControl, "JE_OA_ShipperAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OA_ShipperAddress)));
			this.ConsignorAddressControl.BindToOrgList = "Lookups+SellerList";
			this.ConsignorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 135, true);
			this.ConsignorAddressControl.Name = "ConsignorAddressControl";
			this.ConsignorAddressControl.ShowAddress = false;
			this.ConsignorAddressControl.ShowOrganisationName = true;
			this.ConsignorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ConsignorAddressControl.TabIndex = 5;
			// 
			// ExportOrganizationUserControl
			// 
			this.Controls.Add(this.ConsignorAddressControl);
			this.Controls.Add(this.SupervisingOfficePanel);
			this.Name = "ExportOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 163, true);
			this.Controls.SetChildIndex(this.SupervisingOfficePanel, 0);
			this.Controls.SetChildIndex(this.ConsignorAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupervisingOfficePanel.ResumeLayout(false);
			this.SupervisingOfficePanel.PerformLayout();
			this.SupervisingOfficeDocAddress.ResumeLayout(true);
			this.SupervisingOfficeDocAddress.PerformLayout();
			this.ConsignorAddressControl.ResumeLayout(true);
			this.ConsignorAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton SpoffSetterButton;
		private CargoWise.Windows.UI.KPanel SupervisingOfficePanel;
		private Enterprise.MasterFiles.GUI.ZDocAddressControl SupervisingOfficeDocAddress;
		private Enterprise.ZArchitecture.GUI.ZAddressControl ConsignorAddressControl;
	}
}

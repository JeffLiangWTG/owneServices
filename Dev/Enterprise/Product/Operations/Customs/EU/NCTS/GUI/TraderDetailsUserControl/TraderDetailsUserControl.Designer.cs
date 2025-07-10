namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class TraderDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PrincipalDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.RepresentativeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.FromWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrincipalDocAddressControl.SuspendLayout();
			this.ConsignorDocAddressControl.SuspendLayout();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.RepresentativeDocAddressControl.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// PrincipalDocAddressControl
			// 
			this.PrincipalDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrincipalDocAddressControl, "Principal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Principal)));
			this.PrincipalDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.PrincipalDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("43DF34FA-5075-446A-86EF-D2F71CF7F16F", "Principal");
			this.PrincipalDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PrincipalDocAddressControl, false);
			this.PrincipalDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PrincipalDocAddressControl.Name = "PrincipalDocAddressControl";
			this.PrincipalDocAddressControl.ReadOnly = false;
			this.PrincipalDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.PrincipalDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 117, true);
			this.PrincipalDocAddressControl.TabIndex = 0;
			this.PrincipalDocAddressControl.ValidationJustForced = false;
			// 
			// ConsignorDocAddressControl
			// 
			this.ConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignorDocAddressControl, "Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Consignor)));
			this.ConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.ConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("5B021476-DB74-408D-B368-B8395A4596C5", "Consignor");
			this.ConsignorDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsignorDocAddressControl, false);
			this.ConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.ConsignorDocAddressControl.Name = "ConsignorDocAddressControl";
			this.ConsignorDocAddressControl.ReadOnly = false;
			this.ConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 117, true);
			this.ConsignorDocAddressControl.TabIndex = 1;
			this.ConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Consignee)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("A1375B0E-E82C-4651-B3BD-5B8AF8312BEE", "Consignee");
			this.ConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConsigneeDocAddressControl, false);
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 19, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 117, true);
			this.ConsigneeDocAddressControl.TabIndex = 2;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// RepresentativeDocAddressControl
			// 
			this.RepresentativeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeDocAddressControl, "MovementHeader.Representative");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.Representative)));
			this.RepresentativeDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.RepresentativeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1E6D62AD-FCF1-4740-B16E-E545B618A3FD", "Representative");
			this.RepresentativeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithContactTab;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RepresentativeDocAddressControl, false);
			this.RepresentativeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 217, true);
			this.RepresentativeDocAddressControl.Name = "RepresentativeDocAddressControl";
			this.RepresentativeDocAddressControl.ReadOnly = false;
			this.RepresentativeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.RepresentativeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 117, true);
			this.RepresentativeDocAddressControl.TabIndex = 3;
			this.RepresentativeDocAddressControl.ValidationJustForced = false;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b716bd58-6a5b-4dff-ac75-00c0a5b9e18f", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 458, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 83, true);
			this.FromWarehouseGroupBox.TabIndex = 15;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "MovementHeader.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e5efbc63-13fb-4407-92b4-f0340fc4956f", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 58, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FromWarehouseCodeTextBox.TabIndex = 1;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "MovementHeader.BM_OA_WarehouseAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_OA_WarehouseAddress)));
			this.FromWarehouseAddressControl.BindToOrgList = "MovementHeader.Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.StackControls = true;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 22, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// TraderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FromWarehouseGroupBox);
			this.Controls.Add(this.PrincipalDocAddressControl);
			this.Controls.Add(this.ConsignorDocAddressControl);
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Controls.Add(this.RepresentativeDocAddressControl);
			this.Name = "TraderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 721, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrincipalDocAddressControl.ResumeLayout(true);
			this.PrincipalDocAddressControl.PerformLayout();
			this.ConsignorDocAddressControl.ResumeLayout(true);
			this.ConsignorDocAddressControl.PerformLayout();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.RepresentativeDocAddressControl.ResumeLayout(true);
			this.RepresentativeDocAddressControl.PerformLayout();
			this.FromWarehouseGroupBox.ResumeLayout(false);
			this.FromWarehouseGroupBox.PerformLayout();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterFiles.GUI.ZDocAddressControl PrincipalDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsignorDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl ConsigneeDocAddressControl;
		internal MasterFiles.GUI.ZDocAddressControl RepresentativeDocAddressControl;
		internal ZArchitecture.GUI.ZGroupBox FromWarehouseGroupBox;
		internal ZArchitecture.GUI.ZAddressControl FromWarehouseAddressControl;
		internal ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
	}
}

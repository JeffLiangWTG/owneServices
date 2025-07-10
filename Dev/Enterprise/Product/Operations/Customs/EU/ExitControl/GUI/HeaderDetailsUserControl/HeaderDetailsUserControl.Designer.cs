using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class HeaderDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExporterOrgAddressControl = new MasterFiles.GUI.ZOrganisationControl();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CarrierAddressWithContactControl = new MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExporterOrgAddressControl.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.CarrierAddressWithContactControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.ExitControl.Business.CusExitHeader);
			// 
			// ExporterOrgAddressControl
			// 
			this.ExporterOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterOrgAddressControl, "CXH_OH_Exporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CXH_OH_Exporter)));
			this.ExporterOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("FE5C70F6-9B97-4D59-A8E6-2A403812310C", "Exporter/Client");
			this.ExporterOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 85, true);
			this.ExporterOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ExporterOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ExporterOrgAddressControl.Name = "ExporterOrgAddressControl";
			this.ExporterOrgAddressControl.PopupCaption = "";
			this.ExporterOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.ExporterOrgAddressControl.TabIndex = 3;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "CXH_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CXH_GB_Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("9dfdeb45-923b-4469-bad1-e6ede56a213c", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 59, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BranchGuidFindBox.TabIndex = 1;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "CXH_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CXH_GS_NKCustomsAgent)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("64E16DD7-79C2-4721-B648-8BFEC458012B", "Broker");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 46, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.ShouldResize = false;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.BrokerCodeFindBox.TabIndex = 2;
			// 
			// CarrierAddressWithContactControl
			// 
			this.CarrierAddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAddressWithContactControl, "CarrierZAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ZAddressWithContact)(((Enterprise.Customs.EU.ExitControl.Business.CusExitHeader)(null)).CarrierZAddressWithContact)));
			this.CarrierAddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 6, true);
			this.CarrierAddressWithContactControl.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("9B9B6356-8A8E-4F98-8CBB-F74332357AF7", "Carrier");
			this.CarrierAddressWithContactControl.ContactInfoTabVisible = true;
			this.CarrierAddressWithContactControl.InvoiceContactTabCaption = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("DDFCECAA-CEE4-4C88-9467-BF91E764936A", "Carrier Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CarrierAddressWithContactControl, false);
			this.CarrierAddressWithContactControl.Name = "CarrierAddressWithContactControl";
			this.CarrierAddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.CarrierAddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.CarrierAddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.CarrierAddressWithContactControl.OnlyStopOnDebtor = false;
			this.CarrierAddressWithContactControl.TabIndex = 4;
			// 
			// HeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BranchGuidFindBox);
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.ExporterOrgAddressControl);
			this.Controls.Add(this.CarrierAddressWithContactControl);
			this.Name = "HeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExporterOrgAddressControl.ResumeLayout(true);
			this.ExporterOrgAddressControl.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.CarrierAddressWithContactControl.ResumeLayout(true);
			this.CarrierAddressWithContactControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterFiles.GUI.ZOrganisationControl ExporterOrgAddressControl;
		internal ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl CarrierAddressWithContactControl;
	}
}

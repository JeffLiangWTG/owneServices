namespace Enterprise.Customs.IE.GUI
{
	partial class TemporaryStorageUserControl
	{
		private void InitializeComponent()
		{
			this.ManifestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsOfficeofLodgementCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsOfficeOfFirstEntryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RepresentativeStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BorderTransportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BorderTransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManifestTypeDropEdit.SuspendLayout();
			this.CustomsOfficeofLodgementCodeFindBox.SuspendLayout();
			this.CustomsOfficeOfFirstEntryCodeFindBox.SuspendLayout();
			this.RepresentativeStatusDropEdit.SuspendLayout();
			this.BorderTransportTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// ManifestTypeDropEdit
			// 
			this.ManifestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestTypeDropEdit, "AMA_ManifestType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_ManifestType)));
			this.ManifestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 26, true);
			this.ManifestTypeDropEdit.Name = "ManifestTypeDropEdit";
			this.ManifestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 16, true);
			this.ManifestTypeDropEdit.TabIndex = 38;
			// 
			// CustomsOfficeofLodgementCodeFindBox
			// 
			this.CustomsOfficeofLodgementCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeofLodgementCodeFindBox, "CustomsOfficeOfLodgement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).CustomsOfficeOfLodgement)));
			this.CustomsOfficeofLodgementCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 276, true);
			this.CustomsOfficeofLodgementCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeofLodgementCodeFindBox.Name = "CustomsOfficeofLodgementCodeFindBox";
			this.CustomsOfficeofLodgementCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeofLodgementCodeFindBox.ParentType = null;
			this.CustomsOfficeofLodgementCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 16, true);
			this.CustomsOfficeofLodgementCodeFindBox.TabIndex = 0;
			// 
			// CustomsOfficeOfFirstEntryCodeFindBox
			// 
			this.CustomsOfficeOfFirstEntryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeOfFirstEntryCodeFindBox, "CustomsOfficeOfFirstEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).CustomsOfficeOfFirstEntry)));
			this.CustomsOfficeOfFirstEntryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 299, true);
			this.CustomsOfficeOfFirstEntryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeOfFirstEntryCodeFindBox.Name = "CustomsOfficeOfFirstEntryCodeFindBox";
			this.CustomsOfficeOfFirstEntryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeOfFirstEntryCodeFindBox.ParentType = null;
			this.CustomsOfficeOfFirstEntryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 16, true);
			this.CustomsOfficeOfFirstEntryCodeFindBox.TabIndex = 39;
			// 
			// RepresentativeStatusDropEdit
			// 
			this.RepresentativeStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeStatusDropEdit, "AMA_AgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_AgentType)));
			this.RepresentativeStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 56, true);
			this.RepresentativeStatusDropEdit.Name = "RepresentativeStatusDropEdit";
			this.RepresentativeStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 16, true);
			this.RepresentativeStatusDropEdit.TabIndex = 40;
			// 
			// BorderTransportTypeDropEdit
			// 
			this.BorderTransportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportTypeDropEdit, "AMA_TransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_TransportMeans)));
			this.BorderTransportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 96, true);
			this.BorderTransportTypeDropEdit.Name = "BorderTransportTypeDropEdit";
			this.BorderTransportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 16, true);
			this.BorderTransportTypeDropEdit.TabIndex = 41;
			// 
			// BorderTransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.BorderTransportIDTextBox, "AMA_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_AgentType)));
			this.BorderTransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 136, true);
			this.BorderTransportIDTextBox.Name = "BorderTransportIDTextBox";
			this.BorderTransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 18, true);
			this.BorderTransportIDTextBox.TabIndex = 42;
			// 
			// TemporaryStorageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficeOfFirstEntryCodeFindBox);
			this.Controls.Add(this.ManifestTypeDropEdit);
			this.Controls.Add(this.CustomsOfficeofLodgementCodeFindBox);
			this.Controls.Add(this.RepresentativeStatusDropEdit);
			this.Controls.Add(this.BorderTransportTypeDropEdit);
			this.Controls.Add(this.BorderTransportIDTextBox);
			this.Name = "TemporaryStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 567, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestTypeDropEdit.ResumeLayout(true);
			this.ManifestTypeDropEdit.PerformLayout();
			this.CustomsOfficeofLodgementCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeofLodgementCodeFindBox.PerformLayout();
			this.CustomsOfficeOfFirstEntryCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeOfFirstEntryCodeFindBox.PerformLayout();
			this.RepresentativeStatusDropEdit.ResumeLayout(true);
			this.RepresentativeStatusDropEdit.PerformLayout();
			this.BorderTransportTypeDropEdit.ResumeLayout(true);
			this.BorderTransportTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit ManifestTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsOfficeofLodgementCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeOfFirstEntryCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit RepresentativeStatusDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit BorderTransportTypeDropEdit;
		internal ZArchitecture.ZTextBox BorderTransportIDTextBox;
	}
}

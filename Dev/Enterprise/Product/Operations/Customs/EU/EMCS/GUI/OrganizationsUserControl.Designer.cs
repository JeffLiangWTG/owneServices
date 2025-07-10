namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class OrganizationsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CarrierAgentDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.TransporterDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DispatchWarehouseDocAddressControl = new Enterprise.Customs.EU.EMCS.GUI.EMCSDispatchWarehouseDocAddressControl();
			this.DestinationWarehouseDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.OrganizationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierAgentDocAddressControl.SuspendLayout();
			this.TransporterDocAddressControl.SuspendLayout();
			this.DispatchWarehouseDocAddressControl.SuspendLayout();
			this.DestinationWarehouseDocAddressControl.SuspendLayout();
			this.OrganizationsGroupBox.SuspendLayout();
			this.TransportGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// CarrierAgentDocAddressControl
			// 
			this.CarrierAgentDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAgentDocAddressControl, "CarrierAgentDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).CarrierAgentDocumentaryAddress)));
			this.CarrierAgentDocAddressControl.BindToOrganisations = "Lookups+CarrierAgentList";
			this.CarrierAgentDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("811e588a-3a4a-439c-a8be-b44f363a76ee", "Carrier Agent");
			this.CarrierAgentDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 15, true);
			this.CarrierAgentDocAddressControl.Name = "CarrierAgentDocAddressControl";
			this.CarrierAgentDocAddressControl.ReadOnly = false;
			this.CarrierAgentDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.CarrierAgentDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.CarrierAgentDocAddressControl.TabIndex = 0;
			this.CarrierAgentDocAddressControl.ValidationJustForced = false;
			// 
			// TransporterDocAddressControl
			// 
			this.TransporterDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransporterDocAddressControl, "TransporterDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).TransporterDocumentaryAddress)));
			this.TransporterDocAddressControl.BindToOrganisations = "Lookups+TransporterList";
			this.TransporterDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("488dfb20-5edd-43b4-add3-c5c111008d54", "Transporter");
			this.TransporterDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 15, true);
			this.TransporterDocAddressControl.Name = "TransporterDocAddressControl";
			this.TransporterDocAddressControl.ReadOnly = false;
			this.TransporterDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.TransporterDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.TransporterDocAddressControl.TabIndex = 1;
			this.TransporterDocAddressControl.ValidationJustForced = false;
			// 
			// DispatchWarehouseDocAddressControl
			// 
			this.DispatchWarehouseDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DispatchWarehouseDocAddressControl, ".");
			this.DispatchWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.DispatchWarehouseDocAddressControl.Name = "DispatchWarehouseDocAddressControl";
			this.DispatchWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 205, true);
			this.DispatchWarehouseDocAddressControl.TabIndex = 0;
			// 
			// DestinationWarehouseDocAddressControl
			// 
			this.DestinationWarehouseDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationWarehouseDocAddressControl, "DestinationWarehouseDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).DestinationWarehouseDocumentaryAddress)));
			this.DestinationWarehouseDocAddressControl.BindToOrganisations = "Lookups+DestinationWarehouseList";
			this.DestinationWarehouseDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("0431bdb4-ab80-49af-8ab8-d196788131b1", "Destination Warehouse");
			this.DestinationWarehouseDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 10, true);
			this.DestinationWarehouseDocAddressControl.Name = "DestinationWarehouseDocAddressControl";
			this.DestinationWarehouseDocAddressControl.ReadOnly = false;
			this.DestinationWarehouseDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DestinationWarehouseDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DestinationWarehouseDocAddressControl.TabIndex = 1;
			this.DestinationWarehouseDocAddressControl.ValidationJustForced = false;
			// 
			// OrganizationsGroupBox
			// 
			this.OrganizationsGroupBox.Controls.Add(this.DispatchWarehouseDocAddressControl);
			this.OrganizationsGroupBox.Controls.Add(this.DestinationWarehouseDocAddressControl);
			this.OrganizationsGroupBox.Controls.Add(this.TransportGroupBox);
			this.OrganizationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OrganizationsGroupBox, false);
			this.OrganizationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganizationsGroupBox.Name = "OrganizationsGroupBox";
			this.OrganizationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 430, true);
			this.OrganizationsGroupBox.TabIndex = 0;
			this.OrganizationsGroupBox.TabStop = false;
			// 
			// TransportGroupBox
			// 
			this.TransportGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("b7fc684d-98a9-469f-96f5-5aae785dd0bf", "Transport");
			this.TransportGroupBox.Controls.Add(this.CarrierAgentDocAddressControl);
			this.TransportGroupBox.Controls.Add(this.TransporterDocAddressControl);
			this.TransportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 230, true);
			this.TransportGroupBox.Name = "TransportGroupBox";
			this.TransportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 200, true);
			this.TransportGroupBox.TabIndex = 2;
			this.TransportGroupBox.TabStop = false;
			// 
			// OrganizationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrganizationsGroupBox);
			this.Name = "OrganizationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierAgentDocAddressControl.ResumeLayout(true);
			this.CarrierAgentDocAddressControl.PerformLayout();
			this.TransporterDocAddressControl.ResumeLayout(true);
			this.TransporterDocAddressControl.PerformLayout();
			this.DispatchWarehouseDocAddressControl.ResumeLayout(true);
			this.DispatchWarehouseDocAddressControl.PerformLayout();
			this.DestinationWarehouseDocAddressControl.ResumeLayout(true);
			this.DestinationWarehouseDocAddressControl.PerformLayout();
			this.OrganizationsGroupBox.ResumeLayout(false);
			this.OrganizationsGroupBox.PerformLayout();
			this.TransportGroupBox.ResumeLayout(false);
			this.TransportGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MasterFiles.GUI.ZDocAddressControl CarrierAgentDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl TransporterDocAddressControl;
		private Enterprise.Customs.EU.EMCS.GUI.EMCSDispatchWarehouseDocAddressControl DispatchWarehouseDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl DestinationWarehouseDocAddressControl;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OrganizationsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TransportGroupBox;
	}
}

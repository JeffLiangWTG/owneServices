namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class TransportAndPackagingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CarrierDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.TransportMethodOfPaymentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierDocAddressControl.SuspendLayout();
			this.TransportMethodOfPaymentDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// CarrierDocAddressControl
			// 
			this.CarrierDocAddressControl.AddressValidationProcessCmdKey = null;
			this.CarrierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierDocAddressControl, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).Carrier)));
			this.CarrierDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.CarrierDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E0A103E1-F52B-4D2C-B1F2-3ECE833B167A", "Carrier");
			this.CarrierDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.CarrierDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 29, true);
			this.CarrierDocAddressControl.Name = "CarrierDocAddressControl";
			this.CarrierDocAddressControl.ReadOnly = false;
			this.CarrierDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.CarrierDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.CarrierDocAddressControl.TabIndex = 1;
			this.CarrierDocAddressControl.ValidationJustForced = false;
			// 
			// TransportMethodOfPaymentDropEdit
			// 
			this.TransportMethodOfPaymentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportMethodOfPaymentDropEdit, "BM_MethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_MethodOfPayment)));
			this.TransportMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 3, true);
			this.TransportMethodOfPaymentDropEdit.Name = "TransportMethodOfPaymentDropEdit";
			this.TransportMethodOfPaymentDropEdit.PreBoundMaxLength = 1;
			this.TransportMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.TransportMethodOfPaymentDropEdit.TabIndex = 2;
			// 
			// TransportAndPackagingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportMethodOfPaymentDropEdit);
			this.Controls.Add(this.CarrierDocAddressControl);
			this.Name = "TransportAndPackagingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CarrierDocAddressControl.ResumeLayout(true);
			this.CarrierDocAddressControl.PerformLayout();
			this.TransportMethodOfPaymentDropEdit.ResumeLayout(true);
			this.TransportMethodOfPaymentDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal MasterFiles.GUI.ZDocAddressControl CarrierDocAddressControl;
		internal ZArchitecture.GUI.ZDropEdit TransportMethodOfPaymentDropEdit;
	}
}

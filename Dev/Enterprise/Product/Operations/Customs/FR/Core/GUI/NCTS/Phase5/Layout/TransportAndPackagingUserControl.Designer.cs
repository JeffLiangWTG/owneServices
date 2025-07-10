using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS
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
			this.PortOfPresentationCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.ChargePaymentOrDestinationIDDropEdit= new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CarrierDocAddressControl.SuspendLayout();
			this.ChargePaymentOrDestinationIDDropEdit.SuspendLayout();
			this.TransportMethodOfPaymentDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NctsDepartureMovementHeader);
			// 
			// CarrierDocAddressControl
			// 
			this.CarrierDocAddressControl.AddressValidationProcessCmdKey = null;
			this.CarrierDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierDocAddressControl, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((NctsDepartureMovementHeader)(null)).Carrier)));
			this.CarrierDocAddressControl.BindToOrganisations = "Lookups.Organisations";
			this.CarrierDocAddressControl.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("62A6F36E-969F-452B-9C03-96B03AD207A0", "Carrier");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NctsDepartureMovementHeader)(null)).BM_MethodOfPayment)));
			this.TransportMethodOfPaymentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 3, true);
			this.TransportMethodOfPaymentDropEdit.Name = "TransportMethodOfPaymentDropEdit";
			this.TransportMethodOfPaymentDropEdit.PreBoundMaxLength = 1;
			this.TransportMethodOfPaymentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.TransportMethodOfPaymentDropEdit.TabIndex = 2;
			// 
			// PortOfPresentationCodeFindBox
			// 
			this.PortOfPresentationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfPresentationCodeFindBox, "BM_RL_NKPortOfPresentation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NctsDepartureMovementHeader)(null)).BM_RL_NKPortOfPresentation)));
			this.PortOfPresentationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 55, true);
			this.PortOfPresentationCodeFindBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("F5AAC474-EF08-405D-9875-C016225B17F0", "Port of Dispatch");
			this.PortOfPresentationCodeFindBox.Name = "PortOfPresentationCodeFindBox";
			this.PortOfPresentationCodeFindBox.PreBoundMaxLength = 1;
			this.PortOfPresentationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PortOfPresentationCodeFindBox.TabIndex = 3;
			// 
			// 
			// ChargePaymentOrDestinationIDDropEdit
			// 
			this.ChargePaymentOrDestinationIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargePaymentOrDestinationIDDropEdit, "ChargePaymentOrDestinationID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NctsDepartureMovementHeader)(null)).ChargePaymentOrDestinationID)));
			this.ChargePaymentOrDestinationIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 81, true);
			this.ChargePaymentOrDestinationIDDropEdit.Name = "ChargePaymentOrDestinationIDDropEdit";
			this.ChargePaymentOrDestinationIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ChargePaymentOrDestinationIDDropEdit.TabIndex = 4;
			// 
			// TransportAndPackagingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportMethodOfPaymentDropEdit);
			this.Controls.Add(this.CarrierDocAddressControl);
			this.Controls.Add(this.PortOfPresentationCodeFindBox);
			this.Controls.Add(this.ChargePaymentOrDestinationIDDropEdit);
			this.Name = "TransportAndPackagingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ChargePaymentOrDestinationIDDropEdit.ResumeLayout(true);
			this.ChargePaymentOrDestinationIDDropEdit.PerformLayout();
			this.PortOfPresentationCodeFindBox.ResumeLayout(true);
			this.PortOfPresentationCodeFindBox.PerformLayout();
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
		internal ZArchitecture.GUI.ZCodeFindBox PortOfPresentationCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit ChargePaymentOrDestinationIDDropEdit;
	}
}

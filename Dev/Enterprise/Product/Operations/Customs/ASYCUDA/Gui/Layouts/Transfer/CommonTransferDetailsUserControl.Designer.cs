namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class CommonTransferDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DestinationPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransferTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CarrierAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CarrierIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OnwardCarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DestinationWarehouseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DestinationPortCodeFindBox.SuspendLayout();
			this.TransferTypeDropEdit.SuspendLayout();
			this.CarrierAddressControl.SuspendLayout();
			this.OnwardCarrierCodeFindBox.SuspendLayout();
			this.DestinationWarehouseAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader);
			// 
			// DestinationPortCodeFindBox
			// 
			this.DestinationPortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationPortCodeFindBox, "ATF_RL_NKDestinationPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_RL_NKDestinationPortCode)));
			this.DestinationPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 27, true);
			this.DestinationPortCodeFindBox.Name = "DestinationPortCodeFindBox";
			this.DestinationPortCodeFindBox.PreBoundMaxLength = 2;
			this.DestinationPortCodeFindBox.ShouldResize = true;
			this.DestinationPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.DestinationPortCodeFindBox.TabIndex = 1;
			// 
			// TransferTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TransferTypeDropEdit, "ATF_TransferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_TransferType)));
			this.TransferTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 27, true);
			this.TransferTypeDropEdit.Name = "TransferTypeDropEdit";
			this.TransferTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.TransferTypeDropEdit.TabIndex = 2;
			// 
			// CarrierAddressControl
			// 
			this.CarrierAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierAddressControl, "ATF_OA_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_OA_Carrier)));
			this.CarrierAddressControl.BindToOrgList = "Lookups+ShippingProviders";
			this.CarrierAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 36, true);
			this.CarrierAddressControl.Name = "CarrierAddressControl";
			this.CarrierAddressControl.PopupCaption = "";
			this.CarrierAddressControl.ReadOnly = false;
			this.CarrierAddressControl.ShowAddress = false;
			this.CarrierAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CarrierAddressControl.TabIndex = 3;
			// 
			// CarrierIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.CarrierIDTextBox, "ATF_CarrierID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_CarrierID)));
			this.CarrierIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 64, true);
			this.CarrierIDTextBox.Name = "CarrierIDTextBox";
			this.CarrierIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.CarrierIDTextBox.TabIndex = 4;
			// 
			// OnwardCarrierCodeFindBox
			// 
			this.OnwardCarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OnwardCarrierCodeFindBox, "ATF_OnwardCarrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_OnwardCarrier)));
			this.OnwardCarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 90, true);
			this.OnwardCarrierCodeFindBox.Name = "OnwardCarrierCodeFindBox";
			this.OnwardCarrierCodeFindBox.PreBoundMaxLength = 4;
			this.OnwardCarrierCodeFindBox.ShowDescriptionBox = true;
			this.OnwardCarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.OnwardCarrierCodeFindBox.TabIndex = 5;
			// 
			// DestinationWarehouseAddressControl
			// 
			this.DestinationWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationWarehouseAddressControl, "ATF_OA_DestinationWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_OA_DestinationWarehouse)));
			this.DestinationWarehouseAddressControl.BindToOrgList = "Lookups+BondedWarehouseCollection";
			this.DestinationWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 36, true);
			this.DestinationWarehouseAddressControl.Name = "DestinationWarehouseAddressControl";
			this.DestinationWarehouseAddressControl.PopupCaption = "";
			this.DestinationWarehouseAddressControl.ReadOnly = false;
			this.DestinationWarehouseAddressControl.ShowAddress = false;
			this.DestinationWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DestinationWarehouseAddressControl.TabIndex = 3;
			// 
			// DestinationWarehouseIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DestinationWarehouseIDTextBox, "ATF_DestinationWarehouseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(null)).ATF_DestinationWarehouseID)));
			this.DestinationWarehouseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 64, true);
			this.DestinationWarehouseIDTextBox.Name = "DestinationWarehouseIDTextBox";
			this.DestinationWarehouseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.DestinationWarehouseIDTextBox.TabIndex = 4;
			// 
			// CommonPackedItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.Controls.Add(this.DestinationPortCodeFindBox);
			this.Controls.Add(this.TransferTypeDropEdit);
			this.Controls.Add(this.CarrierAddressControl);
			this.Controls.Add(this.CarrierIDTextBox);
			this.Controls.Add(this.OnwardCarrierCodeFindBox);
			this.Controls.Add(this.DestinationWarehouseAddressControl);
			this.Controls.Add(this.DestinationWarehouseIDTextBox);
			this.Name = "CommonTransferDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DestinationPortCodeFindBox.ResumeLayout(true);
			this.DestinationPortCodeFindBox.PerformLayout();
			this.TransferTypeDropEdit.ResumeLayout(true);
			this.TransferTypeDropEdit.PerformLayout();
			this.CarrierAddressControl.ResumeLayout(true);
			this.CarrierAddressControl.PerformLayout();
			this.OnwardCarrierCodeFindBox.ResumeLayout(true);
			this.OnwardCarrierCodeFindBox.PerformLayout();
			this.DestinationWarehouseAddressControl.ResumeLayout(true);
			this.DestinationWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DestinationPortCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransferTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl CarrierAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox CarrierIDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OnwardCarrierCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DestinationWarehouseAddressControl;
		internal Enterprise.ZArchitecture.ZTextBox DestinationWarehouseIDTextBox;
	}
}

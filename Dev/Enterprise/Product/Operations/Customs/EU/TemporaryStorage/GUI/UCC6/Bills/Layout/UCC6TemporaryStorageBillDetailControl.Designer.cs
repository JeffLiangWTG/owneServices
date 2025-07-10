namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageBillDetailControl
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
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.billNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.uCRNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.consignorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.consigneeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.notifyPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl = new Enterprise.Customs.EU.TemporaryStorage.GUI.UCC6TemporaryStorageGrossWeightWithUnitUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.typeDropEdit.SuspendLayout();
			this.consignorAddressControl.SuspendLayout();
			this.consigneeAddressControl.SuspendLayout();
			this.notifyPartyAddressControl.SuspendLayout();
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.typeDropEdit, "Bills.TypeOfBillDocument");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).TypeOfBillDocument)));
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 107, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.PreBoundMaxLength = 4;
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.typeDropEdit.TabIndex = 1;
			// 
			// billNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.billNumberTextBox, "Bills.ABL_BillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_BillNumber)));
			this.billNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 142, true);
			this.billNumberTextBox.Name = "billNumberTextBox";
			this.billNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.billNumberTextBox.TabIndex = 2;
			// 
			// uCRNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.uCRNumberTextBox, "Bills.ABL_UCRNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_UCRNumber)));
			this.uCRNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 180, true);
			this.uCRNumberTextBox.Name = "uCRNumberTextBox";
			this.uCRNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.uCRNumberTextBox.TabIndex = 3;
			// 
			// consignorAddressControl
			// 
			this.consignorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consignorAddressControl, "Bills.ABL_OA_Shipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_Shipper)));
			this.consignorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 243, true);
			this.consignorAddressControl.Name = "consignorAddressControl";
			this.consignorAddressControl.PopupCaption = "";
			this.consignorAddressControl.ShowAddress = false;
			this.consignorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.consignorAddressControl.TabIndex = 4;
			// 
			// consigneeAddressControl
			// 
			this.consigneeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.consigneeAddressControl, "Bills.ABL_OA_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_Consignee)));
			this.consigneeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 279, true);
			this.consigneeAddressControl.Name = "consigneeAddressControl";
			this.consigneeAddressControl.PopupCaption = "";
			this.consigneeAddressControl.ShowAddress = false;
			this.consigneeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.consigneeAddressControl.TabIndex = 5;
			// 
			// notifyPartyAddressControl
			// 
			this.notifyPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.notifyPartyAddressControl, "Bills.ABL_OA_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_OA_NotifyParty)));
			this.notifyPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 323, true);
			this.notifyPartyAddressControl.Name = "notifyPartyAddressControl";
			this.notifyPartyAddressControl.PopupCaption = "";
			this.notifyPartyAddressControl.ShowAddress = false;
			this.notifyPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.notifyPartyAddressControl.TabIndex = 6;
			// 
			// uCC6TemporaryStorageGrossWeightWithUnitUserControl
			// 
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.uCC6TemporaryStorageGrossWeightWithUnitUserControl, ".");
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 211, true);
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.Name = "uCC6TemporaryStorageGrossWeightWithUnitUserControl";
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.TabIndex = 7;
			// 
			// UCC6TemporaryStorageBillDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.uCC6TemporaryStorageGrossWeightWithUnitUserControl);
			this.Controls.Add(this.notifyPartyAddressControl);
			this.Controls.Add(this.consigneeAddressControl);
			this.Controls.Add(this.consignorAddressControl);
			this.Controls.Add(this.uCRNumberTextBox);
			this.Controls.Add(this.billNumberTextBox);
			this.Controls.Add(this.typeDropEdit);
			this.Name = "UCC6TemporaryStorageBillDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 426, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.consignorAddressControl.ResumeLayout(true);
			this.consignorAddressControl.PerformLayout();
			this.consigneeAddressControl.ResumeLayout(true);
			this.consigneeAddressControl.PerformLayout();
			this.notifyPartyAddressControl.ResumeLayout(true);
			this.notifyPartyAddressControl.PerformLayout();
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.ResumeLayout(true);
			this.uCC6TemporaryStorageGrossWeightWithUnitUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit typeDropEdit;
		internal ZArchitecture.ZTextBox billNumberTextBox;
		internal ZArchitecture.ZTextBox uCRNumberTextBox;
		internal ZArchitecture.GUI.ZAddressControl consignorAddressControl;
		internal ZArchitecture.GUI.ZAddressControl consigneeAddressControl;
		internal ZArchitecture.GUI.ZAddressControl notifyPartyAddressControl;
		internal UCC6TemporaryStorageGrossWeightWithUnitUserControl uCC6TemporaryStorageGrossWeightWithUnitUserControl;
	}
}

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ItemSecurityTabUserControl
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
			this.ItemSecurityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UNDangerousGoodsGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ItemSecurityConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ItemSecurityConsignorDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CommercialReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportChargesMoPDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemSecurityGroupBox.SuspendLayout();
			this.UNDangerousGoodsGuidFindBox.SuspendLayout();
			this.ItemSecurityConsigneeDocAddressControl.SuspendLayout();
			this.ItemSecurityConsignorDocAddressControl.SuspendLayout();
			this.TransportChargesMoPDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// ItemSecurityGroupBox
			// 
			this.ItemSecurityGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("75DF38DA-A3D2-12D4-F065-2F694D791379", "Item Security Details");
			this.ItemSecurityGroupBox.Controls.Add(this.UNDangerousGoodsGuidFindBox);
			this.ItemSecurityGroupBox.Controls.Add(this.ItemSecurityConsigneeDocAddressControl);
			this.ItemSecurityGroupBox.Controls.Add(this.ItemSecurityConsignorDocAddressControl);
			this.ItemSecurityGroupBox.Controls.Add(this.CommercialReferenceNumberTextBox);
			this.ItemSecurityGroupBox.Controls.Add(this.TransportChargesMoPDropEdit);
			this.ItemSecurityGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemSecurityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemSecurityGroupBox.Name = "ItemSecurityGroupBox";
			this.ItemSecurityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 292, true);
			this.ItemSecurityGroupBox.TabIndex = 0;
			this.ItemSecurityGroupBox.TabStop = false;
			// 
			// UNDangerousGoodsGuidFindBox
			// 
			this.UNDangerousGoodsGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDangerousGoodsGuidFindBox, "UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.UNDangerousGoodsGuidFindBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("14aa933e-0e08-473f-bdd2-7c7dd10b6622", "Dangerous Goods Substance");
			this.UNDangerousGoodsGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 87, true);
			this.UNDangerousGoodsGuidFindBox.Name = "UNDangerousGoodsGuidFindBox";
			this.UNDangerousGoodsGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.UNDangerousGoodsGuidFindBox.TabIndex = 2;
			// 
			// ItemSecurityConsigneeDocAddressControl
			// 
			this.ItemSecurityConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ItemSecurityConsigneeDocAddressControl, "SecurityConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).SecurityConsignee)));
			this.ItemSecurityConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ItemSecurityConsigneeDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("017fb41c-8aa9-4166-a1ea-a0c8e70e6571", "Security Consignee");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ItemSecurityConsigneeDocAddressControl, false);
			this.ItemSecurityConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 19, true);
			this.ItemSecurityConsigneeDocAddressControl.Name = "ItemSecurityConsigneeDocAddressControl";
			this.ItemSecurityConsigneeDocAddressControl.ReadOnly = false;
			this.ItemSecurityConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ItemSecurityConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ItemSecurityConsigneeDocAddressControl.TabIndex = 4;
			this.ItemSecurityConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// ItemSecurityConsignorDocAddressControl
			// 
			this.ItemSecurityConsignorDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ItemSecurityConsignorDocAddressControl, "SecurityConsignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).SecurityConsignor)));
			this.ItemSecurityConsignorDocAddressControl.BindToOrganisations = "Lookups.Consignors";
			this.ItemSecurityConsignorDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0700c85c-517f-4645-bae4-dba5f9a2df20", "Security Consignor");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ItemSecurityConsignorDocAddressControl, false);
			this.ItemSecurityConsignorDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(449, 19, true);
			this.ItemSecurityConsignorDocAddressControl.Name = "ItemSecurityConsignorDocAddressControl";
			this.ItemSecurityConsignorDocAddressControl.ReadOnly = false;
			this.ItemSecurityConsignorDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ItemSecurityConsignorDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ItemSecurityConsignorDocAddressControl.TabIndex = 3;
			this.ItemSecurityConsignorDocAddressControl.ValidationJustForced = false;
			// 
			// CommercialReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommercialReferenceNumberTextBox, "BY_CommercialReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).BY_CommercialReferenceNumber)));
			this.CommercialReferenceNumberTextBox.CaptionResourceString = null;
			this.CommercialReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommercialReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 35, true);
			this.CommercialReferenceNumberTextBox.Name = "CommercialReferenceNumberTextBox";
			this.CommercialReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.CommercialReferenceNumberTextBox.TabIndex = 0;
			// 
			// TransportChargesMoPDropEdit
			// 
			this.TransportChargesMoPDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportChargesMoPDropEdit, "BY_TransportChargesMethodOfPayment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).BY_TransportChargesMethodOfPayment)));
			this.TransportChargesMoPDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 61, true);
			this.TransportChargesMoPDropEdit.Name = "TransportChargesMoPDropEdit";
			this.TransportChargesMoPDropEdit.ShouldResizeByMaxLength = true;
			this.TransportChargesMoPDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.TransportChargesMoPDropEdit.TabIndex = 1;
			// 
			// ItemSecurityTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemSecurityGroupBox);
			this.Name = "ItemSecurityTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 292, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemSecurityGroupBox.ResumeLayout(false);
			this.ItemSecurityGroupBox.PerformLayout();
			this.UNDangerousGoodsGuidFindBox.ResumeLayout(true);
			this.UNDangerousGoodsGuidFindBox.PerformLayout();
			this.ItemSecurityConsigneeDocAddressControl.ResumeLayout(true);
			this.ItemSecurityConsigneeDocAddressControl.PerformLayout();
			this.ItemSecurityConsignorDocAddressControl.ResumeLayout(true);
			this.ItemSecurityConsignorDocAddressControl.PerformLayout();
			this.TransportChargesMoPDropEdit.ResumeLayout(true);
			this.TransportChargesMoPDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ItemSecurityGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox UNDangerousGoodsGuidFindBox;
		private MasterFiles.GUI.ZDocAddressControl ItemSecurityConsigneeDocAddressControl;
		private MasterFiles.GUI.ZDocAddressControl ItemSecurityConsignorDocAddressControl;
		private ZArchitecture.ZTextBox CommercialReferenceNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit TransportChargesMoPDropEdit;
	}
}

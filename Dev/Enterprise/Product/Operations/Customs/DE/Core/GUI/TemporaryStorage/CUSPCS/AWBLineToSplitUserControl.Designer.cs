namespace Enterprise.Customs.DE.GUI
{
	partial class AWBLineToSplitUserControl
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
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.BranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OwnerReferenceTypeDropEdit.SuspendLayout();
			this.CustodianAddressControl.SuspendLayout();
			this.BranchDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine);
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "Dec.STH_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).Dec.STH_MessageStatus)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("740fe911-6195-4aeb-920a-8cd9ab83ebf4", "Customs Status");
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 96, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.MessageStatusTextBox.TabIndex = 6;
			// 
			// OwnerReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OwnerReferenceNumberTextBox, "FormattedOwnerReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).FormattedOwnerReferenceNumber)));
			this.OwnerReferenceNumberTextBox.CaptionResourceString = null;
			this.OwnerReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 30, true);
			this.OwnerReferenceNumberTextBox.Name = "OwnerReferenceNumberTextBox";
			this.OwnerReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceNumberTextBox.TabIndex = 2;
			// 
			// EORITextBox
			// 
			this.BindingSource.SetBindingMember(this.EORITextBox, "TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).TSL_CustodianIdentifier)));
			this.EORITextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8901351d-c0a1-4e19-9916-a13cebf4832c", "Custodian EORI / Branch");
			this.EORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 74, true);
			this.EORITextBox.Name = "EORITextBox";
			this.EORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.EORITextBox.TabIndex = 4;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ef8d6dc4-7d87-4cbb-aa63-78cdf0c3daec", "Owner Reference Type");
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 8, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 1;
			// 
			// CustodianAddressControl
			// 
			this.CustodianAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianAddressControl, "TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).TSL_OA_Custodian)));
			this.CustodianAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2e7c5be9-d7ec-407b-94ad-31e00b356b5d", "Custodian");
			this.CustodianAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 52, true);
			this.CustodianAddressControl.Name = "CustodianAddressControl";
			this.CustodianAddressControl.PopupCaption = "";
			this.CustodianAddressControl.ReadOnly = false;
			this.CustodianAddressControl.ShowAddress = false;
			this.CustodianAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianAddressControl.TabIndex = 3;
			// 
			// BranchDropEdit
			// 
			this.BranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchDropEdit, "TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(null)).TSL_CustodianIdentifierBranchNo)));
			this.BranchDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4d057ee7-83c9-4847-bca6-1e6ea2267677", "Branch");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BranchDropEdit, false);
			this.BranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 74, true);
			this.BranchDropEdit.Name = "BranchDropEdit";
			this.BranchDropEdit.PreBoundMaxLength = 4;
			this.BranchDropEdit.ShouldResizeByMaxLength = true;
			this.BranchDropEdit.ShowDescriptionBox = false;
			this.BranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.BranchDropEdit.TabIndex = 5;
			// 
			// AWBLineToSplitUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustodianAddressControl);
			this.Controls.Add(this.BranchDropEdit);
			this.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Controls.Add(this.OwnerReferenceNumberTextBox);
			this.Controls.Add(this.EORITextBox);
			this.Name = "AWBLineToSplitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 122, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.CustodianAddressControl.ResumeLayout(true);
			this.CustodianAddressControl.PerformLayout();
			this.BranchDropEdit.ResumeLayout(true);
			this.BranchDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZArchitecture.ZTextBox OwnerReferenceNumberTextBox;
		private ZArchitecture.ZTextBox EORITextBox;
		private ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeDropEdit;
		private ZArchitecture.GUI.ZAddressControl CustodianAddressControl;
		private ZArchitecture.GUI.ZDropEdit BranchDropEdit;
	}
}

namespace Enterprise.Customs.DE.GUI
{
	partial class CHGOFFAWBDeclarationUserControl
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
			this.AWBLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisposalEntitledTraderBranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisposalEntitledTraderEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DisposalEntitledTraderAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CustodianBranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustodianUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AWBLineGroupBox.SuspendLayout();
			this.DisposalEntitledTraderBranchDropEdit.SuspendLayout();
			this.DisposalEntitledTraderAddressControl.SuspendLayout();
			this.CustodianBranchDropEdit.SuspendLayout();
			this.OwnerReferenceTypeDropEdit.SuspendLayout();
			this.CustodianUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// AWBLineGroupBox
			// 
			this.AWBLineGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b63daf8b-cd32-4329-aec3-89448b75e4d3", "Line Details");
			this.AWBLineGroupBox.Controls.Add(this.DisposalEntitledTraderBranchDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.DisposalEntitledTraderEORITextBox);
			this.AWBLineGroupBox.Controls.Add(this.DisposalEntitledTraderAddressControl);
			this.AWBLineGroupBox.Controls.Add(this.CustodianBranchDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.CustodianEORITextBox);
			this.AWBLineGroupBox.Controls.Add(this.CustodianUserControl);
			this.AWBLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBLineGroupBox.Name = "AWBLineGroupBox";
			this.AWBLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 120, true);
			this.AWBLineGroupBox.TabIndex = 1;
			this.AWBLineGroupBox.TabStop = false;
			// 
			// DisposalEntitledTraderBranchDropEdit
			// 
			this.DisposalEntitledTraderBranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderBranchDropEdit, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_GoodsOwnerIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifierBranchNo)));
			this.DisposalEntitledTraderBranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 83, true);
			this.DisposalEntitledTraderBranchDropEdit.Name = "DisposalEntitledTraderBranchDropEdit";
			this.DisposalEntitledTraderBranchDropEdit.ShouldResizeByMaxLength = true;
			this.DisposalEntitledTraderBranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderBranchDropEdit.TabIndex = 8;
			// 
			// DisposalEntitledTraderEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderEORITextBox, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_GoodsOwnerIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifier)));
			this.DisposalEntitledTraderEORITextBox.CaptionResourceString = null;
			this.DisposalEntitledTraderEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 61, true);
			this.DisposalEntitledTraderEORITextBox.Name = "DisposalEntitledTraderEORITextBox";
			this.DisposalEntitledTraderEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderEORITextBox.TabIndex = 7;
			// 
			// DisposalEntitledTraderAddressControl
			// 
			this.DisposalEntitledTraderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderAddressControl, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_OA_GoodsOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner)));
			this.DisposalEntitledTraderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(645, 39, true);
			this.DisposalEntitledTraderAddressControl.Name = "DisposalEntitledTraderAddressControl";
			this.DisposalEntitledTraderAddressControl.PopupCaption = "";
			this.DisposalEntitledTraderAddressControl.ReadOnly = false;
			this.DisposalEntitledTraderAddressControl.ShowAddress = false;
			this.DisposalEntitledTraderAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderAddressControl.TabIndex = 6;
			// 
			// CustodianBranchDropEdit
			// 
			this.CustodianBranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianBranchDropEdit, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			this.CustodianBranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 83, true);
			this.CustodianBranchDropEdit.Name = "CustodianBranchDropEdit";
			this.CustodianBranchDropEdit.ShouldResizeByMaxLength = true;
			this.CustodianBranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianBranchDropEdit.TabIndex = 5;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 17, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 1;
			// 
			// CustodianEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.CustodianEORITextBox, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			this.CustodianEORITextBox.CaptionResourceString = null;
			this.CustodianEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 61, true);
			this.CustodianEORITextBox.Name = "CustodianEORITextBox";
			this.CustodianEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianEORITextBox.TabIndex = 4;
			// 
			// CustodianUserControl
			// 
			this.CustodianUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianUserControl, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			this.CustodianUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 39, true);
			this.CustodianUserControl.Name = "CustodianUserControl";
			this.CustodianUserControl.PopupCaption = "";
			this.CustodianUserControl.ReadOnly = false;
			this.CustodianUserControl.ShowAddress = false;
			this.CustodianUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianUserControl.TabIndex = 3;
			// 
			// CHGOFFAWBDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AWBLineGroupBox);
			this.Name = "CHGOFFAWBDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AWBLineGroupBox.ResumeLayout(false);
			this.AWBLineGroupBox.PerformLayout();
			this.DisposalEntitledTraderBranchDropEdit.ResumeLayout(true);
			this.DisposalEntitledTraderBranchDropEdit.PerformLayout();
			this.DisposalEntitledTraderAddressControl.ResumeLayout(true);
			this.DisposalEntitledTraderAddressControl.PerformLayout();
			this.CustodianBranchDropEdit.ResumeLayout(true);
			this.CustodianBranchDropEdit.PerformLayout();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.CustodianUserControl.ResumeLayout(true);
			this.CustodianUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AWBLineGroupBox;
		private ZArchitecture.GUI.ZDropEdit CustodianBranchDropEdit;
		private ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeDropEdit;
		private ZArchitecture.ZTextBox CustodianEORITextBox;
		private ZArchitecture.GUI.ZAddressControl CustodianUserControl;
		private ZArchitecture.GUI.ZAddressControl DisposalEntitledTraderAddressControl;
		private ZArchitecture.GUI.ZDropEdit DisposalEntitledTraderBranchDropEdit;
		private ZArchitecture.ZTextBox DisposalEntitledTraderEORITextBox;
	}
}

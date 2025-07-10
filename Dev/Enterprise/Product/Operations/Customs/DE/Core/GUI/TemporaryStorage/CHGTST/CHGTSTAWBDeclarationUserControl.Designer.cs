namespace Enterprise.Customs.DE.GUI
{
	partial class CHGTSTAWBDeclarationUserControl
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
			this.CustodianBranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NewLocationOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnerReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustodianEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustodianUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AWBLineGroupBox.SuspendLayout();
			this.CustodianBranchDropEdit.SuspendLayout();
			this.NewLocationOfGoodsDropEdit.SuspendLayout();
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
			this.AWBLineGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("bdec3e9e-3fdf-42ae-99a8-e0a8642876dd", "Line Details");
			this.AWBLineGroupBox.Controls.Add(this.CustodianBranchDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.NewLocationOfGoodsDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.OwnerReferenceTypeDropEdit);
			this.AWBLineGroupBox.Controls.Add(this.CustodianEORITextBox);
			this.AWBLineGroupBox.Controls.Add(this.CustodianUserControl);
			this.AWBLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBLineGroupBox.Name = "AWBLineGroupBox";
			this.AWBLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 152, true);
			this.AWBLineGroupBox.TabIndex = 0;
			this.AWBLineGroupBox.TabStop = false;
			// 
			// CustodianBranchDropEdit
			// 
			this.CustodianBranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianBranchDropEdit, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_CustodianIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			this.CustodianBranchDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2520c100-f09d-4feb-b921-1f8905f432c9", "Custodian Branch");
			this.CustodianBranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 83, true);
			this.CustodianBranchDropEdit.Name = "CustodianBranchDropEdit";
			this.CustodianBranchDropEdit.ShouldResizeByMaxLength = true;
			this.CustodianBranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianBranchDropEdit.TabIndex = 5;
			// 
			// NewLocationOfGoodsDropEdit
			// 
			this.NewLocationOfGoodsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewLocationOfGoodsDropEdit, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			this.NewLocationOfGoodsDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8cd8f7db-b354-493f-b1c1-a93ed47b897e", "New Goods Location");
			this.NewLocationOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 106, true);
			this.NewLocationOfGoodsDropEdit.Name = "NewLocationOfGoodsDropEdit";
			this.NewLocationOfGoodsDropEdit.ShouldResizeByMaxLength = true;
			this.NewLocationOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.NewLocationOfGoodsDropEdit.TabIndex = 6;
			// 
			// OwnerReferenceTypeDropEdit
			// 
			this.OwnerReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerReferenceTypeDropEdit, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_OwnerReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			this.OwnerReferenceTypeDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8acf2777-2bd0-49b0-9992-3d0264cf8199", "Owner Reference Type");
			this.OwnerReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 17, true);
			this.OwnerReferenceTypeDropEdit.Name = "OwnerReferenceTypeDropEdit";
			this.OwnerReferenceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OwnerReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OwnerReferenceTypeDropEdit.TabIndex = 1;
			// 
			// CustodianEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.CustodianEORITextBox, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_CustodianIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			this.CustodianEORITextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("2378aea0-8c8b-4f4c-af63-d409ddab9e81", "Custodian EORI");
			this.CustodianEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 61, true);
			this.CustodianEORITextBox.Name = "CustodianEORITextBox";
			this.CustodianEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianEORITextBox.TabIndex = 4;
			// 
			// CustodianUserControl
			// 
			this.CustodianUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustodianUserControl, "CHGTSTCusTempStorageDecs.CusTempStorageLines.TSL_OA_Custodian");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGTSTCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGTSTCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			this.CustodianUserControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("062a58c3-0613-47a6-8229-666e62a41b3a", "Custodian");
			this.CustodianUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 39, true);
			this.CustodianUserControl.Name = "CustodianUserControl";
			this.CustodianUserControl.PopupCaption = "";
			this.CustodianUserControl.ReadOnly = false;
			this.CustodianUserControl.ShowAddress = false;
			this.CustodianUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CustodianUserControl.TabIndex = 3;
			// 
			// CHGTSTAWBDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AWBLineGroupBox);
			this.Name = "CHGTSTAWBDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AWBLineGroupBox.ResumeLayout(false);
			this.AWBLineGroupBox.PerformLayout();
			this.CustodianBranchDropEdit.ResumeLayout(true);
			this.CustodianBranchDropEdit.PerformLayout();
			this.NewLocationOfGoodsDropEdit.ResumeLayout(true);
			this.NewLocationOfGoodsDropEdit.PerformLayout();
			this.OwnerReferenceTypeDropEdit.ResumeLayout(true);
			this.OwnerReferenceTypeDropEdit.PerformLayout();
			this.CustodianUserControl.ResumeLayout(true);
			this.CustodianUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AWBLineGroupBox;
		private ZArchitecture.ZTextBox CustodianEORITextBox;
		private ZArchitecture.GUI.ZAddressControl CustodianUserControl;
		private ZArchitecture.GUI.ZDropEdit NewLocationOfGoodsDropEdit;
		private ZArchitecture.GUI.ZDropEdit OwnerReferenceTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CustodianBranchDropEdit;
	}
}

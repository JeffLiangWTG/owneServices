namespace Enterprise.Customs.DE.GUI
{
	partial class CHGOFFREGDeclarationUserControl
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
			this.REGLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DisposalEntitledTraderBranchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DisposalEntitledTraderEORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DisposalEntitledTraderAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REGLineGroupBox.SuspendLayout();
			this.DisposalEntitledTraderBranchDropEdit.SuspendLayout();
			this.DisposalEntitledTraderAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// REGLineGroupBox
			// 
			this.REGLineGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("b63daf8b-cd32-4329-aec3-89448b75e4d3", "Line Details");
			this.REGLineGroupBox.Controls.Add(this.DisposalEntitledTraderBranchDropEdit);
			this.REGLineGroupBox.Controls.Add(this.DisposalEntitledTraderEORITextBox);
			this.REGLineGroupBox.Controls.Add(this.DisposalEntitledTraderAddressControl);
			this.REGLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REGLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REGLineGroupBox.Name = "REGLineGroupBox";
			this.REGLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 96, true);
			this.REGLineGroupBox.TabIndex = 1;
			this.REGLineGroupBox.TabStop = false;
			// 
			// DisposalEntitledTraderBranchDropEdit
			// 
			this.DisposalEntitledTraderBranchDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderBranchDropEdit, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_GoodsOwnerIdentifierBranchNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifierBranchNo)));
			this.DisposalEntitledTraderBranchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 61, true);
			this.DisposalEntitledTraderBranchDropEdit.Name = "DisposalEntitledTraderBranchDropEdit";
			this.DisposalEntitledTraderBranchDropEdit.ShouldResizeByMaxLength = true;
			this.DisposalEntitledTraderBranchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderBranchDropEdit.TabIndex = 4;
			// 
			// DisposalEntitledTraderEORITextBox
			// 
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderEORITextBox, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_GoodsOwnerIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifier)));
			this.DisposalEntitledTraderEORITextBox.CaptionResourceString = null;
			this.DisposalEntitledTraderEORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 39, true);
			this.DisposalEntitledTraderEORITextBox.Name = "DisposalEntitledTraderEORITextBox";
			this.DisposalEntitledTraderEORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderEORITextBox.TabIndex = 3;
			// 
			// DisposalEntitledTraderAddressControl
			// 
			this.DisposalEntitledTraderAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DisposalEntitledTraderAddressControl, "CHGOFFCusTempStorageDecs.CusTempStorageLines.TSL_OA_GoodsOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CHGOFFCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CHGOFFCusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner)));
			this.DisposalEntitledTraderAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 16, true);
			this.DisposalEntitledTraderAddressControl.Name = "DisposalEntitledTraderAddressControl";
			this.DisposalEntitledTraderAddressControl.PopupCaption = "";
			this.DisposalEntitledTraderAddressControl.ReadOnly = false;
			this.DisposalEntitledTraderAddressControl.ShowAddress = false;
			this.DisposalEntitledTraderAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DisposalEntitledTraderAddressControl.TabIndex = 2;
			// 
			// CHGOFFREGDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.REGLineGroupBox);
			this.Name = "CHGOFFREGDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REGLineGroupBox.ResumeLayout(false);
			this.REGLineGroupBox.PerformLayout();
			this.DisposalEntitledTraderBranchDropEdit.ResumeLayout(true);
			this.DisposalEntitledTraderBranchDropEdit.PerformLayout();
			this.DisposalEntitledTraderAddressControl.ResumeLayout(true);
			this.DisposalEntitledTraderAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox REGLineGroupBox;
		private ZArchitecture.GUI.ZAddressControl DisposalEntitledTraderAddressControl;
		private ZArchitecture.GUI.ZDropEdit DisposalEntitledTraderBranchDropEdit;
		private ZArchitecture.ZTextBox DisposalEntitledTraderEORITextBox;
	}
}

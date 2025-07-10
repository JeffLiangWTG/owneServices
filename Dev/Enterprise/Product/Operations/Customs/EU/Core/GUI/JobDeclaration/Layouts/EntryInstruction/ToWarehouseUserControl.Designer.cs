using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class ToWarehouseUserControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseCodeTextBox.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction);
			//
			// ToWarehouseCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("5b087474-e0c2-404d-af1f-67de0c168cbc", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(409, 14, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			//
			// ToWarehouseAddressControl
			//
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 25, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			//
			// ToWarehouseUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToWarehouseCodeTextBox);
			this.Controls.Add(this.ToWarehouseAddressControl);
			this.Name = "EntryInstructionBasicDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 50, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToWarehouseCodeTextBox.ResumeLayout(true);
			this.ToWarehouseCodeTextBox.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZAddressControl ToWarehouseAddressControl;
		internal ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
	}
}

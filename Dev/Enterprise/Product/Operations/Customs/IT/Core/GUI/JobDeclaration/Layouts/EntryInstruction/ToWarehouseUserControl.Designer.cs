using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI
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
			this.ToWarehouseTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction);
			// 
			// ToWarehouseTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseTypeTextBox, "ZG_ToWarehouseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_ToWarehouseType)));
			this.ToWarehouseTypeTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("8a97b738-a87c-41d6-b9f9-048dd1c36bd6", "Type");
			this.ToWarehouseTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 0, true);
			this.ToWarehouseTypeTextBox.Name = "ToWarehouseTypeTextBox";
			this.ToWarehouseTypeTextBox.ReadOnly = true;
			this.ToWarehouseTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.ToWarehouseTypeTextBox.TabIndex = 1;
			// 
			// ToWarehouseIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseIDTextBox, "ZG_ToWarehouseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_ToWarehouseID)));
			this.ToWarehouseIDTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("e1f4cc61-691d-4aac-bf2f-00e64e26874b", "ID");
			this.ToWarehouseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.ToWarehouseIDTextBox.Name = "ToWarehouseIDTextBox";
			this.ToWarehouseIDTextBox.ReadOnly = true;
			this.ToWarehouseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ToWarehouseIDTextBox.TabIndex = 2;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToWarehouseAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 1, 3, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			// 
			// ToWarehouseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToWarehouseTypeTextBox);
			this.Controls.Add(this.ToWarehouseIDTextBox);
			this.Controls.Add(this.ToWarehouseAddressControl);
			this.Name = "ToWarehouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZAddressControl ToWarehouseAddressControl;
		internal ZTextBox ToWarehouseIDTextBox;
		internal ZTextBox ToWarehouseTypeTextBox;
	}
}

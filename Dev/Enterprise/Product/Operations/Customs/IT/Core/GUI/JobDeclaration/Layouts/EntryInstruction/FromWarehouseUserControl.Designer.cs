using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI
{
	partial class FromWarehouseUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FromWarehouseTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction);
			// 
			// FromWarehouseTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseTypeTextBox, "ZG_FromWarehouseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_FromWarehouseType)));
			this.FromWarehouseTypeTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("b3a68ae3-d8e7-4a31-ae01-ab79021b1ab7", "Type");
			this.FromWarehouseTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 0, true);
			this.FromWarehouseTypeTextBox.Name = "FromWarehouseTypeTextBox";
			this.FromWarehouseTypeTextBox.ReadOnly = true;
			this.FromWarehouseTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.FromWarehouseTypeTextBox.TabIndex = 1;
			// 
			// FromWarehouseIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseIDTextBox, "ZG_FromWarehouseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_FromWarehouseID)));
			this.FromWarehouseIDTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("63f4db1c-f984-4abb-82b8-5136b7878461", "ID");
			this.FromWarehouseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 0, true);
			this.FromWarehouseIDTextBox.Name = "FromWarehouseIDTextBox";
			this.FromWarehouseIDTextBox.ReadOnly = true;
			this.FromWarehouseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.FromWarehouseIDTextBox.TabIndex = 2;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FromWarehouseAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 1, 3, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// FromWarehouseUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FromWarehouseTypeTextBox);
			this.Controls.Add(this.FromWarehouseIDTextBox);
			this.Controls.Add(this.FromWarehouseAddressControl);
			this.Name = "FromWarehouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZAddressControl FromWarehouseAddressControl;
		internal ZTextBox FromWarehouseIDTextBox;
		internal ZTextBox FromWarehouseTypeTextBox;
	}
}

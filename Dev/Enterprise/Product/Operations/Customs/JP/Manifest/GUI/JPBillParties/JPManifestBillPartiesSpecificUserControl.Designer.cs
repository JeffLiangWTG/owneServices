using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPManifestBillPartiesSpecificUserControl
	{
		void InitializeComponent()
		{
			this.ConsigneeRegNoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConsigneeRegTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConsigneeRegNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShipperRegNoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ShipperRegTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShipperRegNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyRegNoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotifyPartyRegTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NotifyPartyRegNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeRegNoPanel.SuspendLayout();
			this.ConsigneeRegTypeDropEdit.SuspendLayout();
			this.ShipperRegNoPanel.SuspendLayout();
			this.ShipperRegTypeDropEdit.SuspendLayout();
			this.NotifyPartyRegNoPanel.SuspendLayout();
			this.NotifyPartyRegTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaBill);
			// 
			// ConsigneeRegNoPanel
			// 
			this.ConsigneeRegNoPanel.Controls.Add(this.ConsigneeRegTypeDropEdit);
			this.ConsigneeRegNoPanel.Controls.Add(this.ConsigneeRegNumTextBox);
			this.ConsigneeRegNoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsigneeRegNoPanel.Name = "ConsigneeRegNoPanel";
			this.ConsigneeRegNoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.ConsigneeRegNoPanel.TabIndex = 0;
			// 
			// ConsigneeRegTypeDropEdit
			// 
			this.ConsigneeRegTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeRegTypeDropEdit, "ABL_ConsigneeRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeRegNoType)));
			this.ConsigneeRegTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsigneeRegTypeDropEdit.Name = "ConsigneeRegTypeDropEdit";
			this.ConsigneeRegTypeDropEdit.PreBoundMaxLength = 3;
			this.ConsigneeRegTypeDropEdit.ShowDescriptionBox = false;
			this.ConsigneeRegTypeDropEdit.TabIndex = 0;
			// 
			// ConsigneeRegNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeRegNumTextBox, "ABL_ConsigneeRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_ConsigneeRegNo)));
			this.ConsigneeRegNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.ConsigneeRegNumTextBox.Name = "ConsigneeRegNumTextBox";
			this.ConsigneeRegNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 15, true);
			this.ConsigneeRegNumTextBox.TabIndex = 1;
			// 
			// ShipperRegNoPanel
			// 
			this.ShipperRegNoPanel.Controls.Add(this.ShipperRegTypeDropEdit);
			this.ShipperRegNoPanel.Controls.Add(this.ShipperRegNumTextBox);
			this.ShipperRegNoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipperRegNoPanel.Name = "ShipperRegNoPanel";
			this.ShipperRegNoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.ShipperRegNoPanel.TabIndex = 0;
			// 
			// ShipperRegTypeDropEdit
			// 
			this.ShipperRegTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperRegTypeDropEdit, "ABL_ShipperRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_ShipperRegNoType)));
			this.ShipperRegTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipperRegTypeDropEdit.Name = "ShipperRegTypeDropEdit";
			this.ShipperRegTypeDropEdit.PreBoundMaxLength = 3;
			this.ShipperRegTypeDropEdit.ShowDescriptionBox = false;
			this.ShipperRegTypeDropEdit.TabIndex = 0;
			// 
			// ShipperRegNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipperRegNumTextBox, "ABL_ShipperRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_ShipperRegNo)));
			this.ShipperRegNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.ShipperRegNumTextBox.Name = "ShipperRegNumTextBox";
			this.ShipperRegNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 15, true);
			this.ShipperRegNumTextBox.TabIndex = 1;
			// 
			// NotifyPartyRegNoPanel
			// 
			this.NotifyPartyRegNoPanel.Controls.Add(this.NotifyPartyRegTypeDropEdit);
			this.NotifyPartyRegNoPanel.Controls.Add(this.NotifyPartyRegNumTextBox);
			this.NotifyPartyRegNoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotifyPartyRegNoPanel.Name = "NotifyPartyRegNoPanel";
			this.NotifyPartyRegNoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.NotifyPartyRegNoPanel.TabIndex = 0;
			// 
			// NotifyPartyRegTypeDropEdit
			// 
			this.NotifyPartyRegTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyRegTypeDropEdit, "ABL_NotifyPartyRegNoType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_NotifyPartyRegNoType)));
			this.NotifyPartyRegTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotifyPartyRegTypeDropEdit.Name = "NotifyPartyRegTypeDropEdit";
			this.NotifyPartyRegTypeDropEdit.PreBoundMaxLength = 3;
			this.NotifyPartyRegTypeDropEdit.ShowDescriptionBox = false;
			this.NotifyPartyRegTypeDropEdit.TabIndex = 0;
			// 
			// NotifyPartyRegNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyRegNumTextBox, "ABL_NotifyPartyRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaBill)(null)).ABL_NotifyPartyRegNo)));
			this.NotifyPartyRegNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.NotifyPartyRegNumTextBox.Name = "NotifyPartyRegNumTextBox";
			this.NotifyPartyRegNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 15, true);
			this.NotifyPartyRegNumTextBox.TabIndex = 1;
			// 
			// JPManifestBillPartiesSpecificUserControl
			// 
			this.Controls.Add(this.ConsigneeRegNoPanel);
			this.Controls.Add(this.ShipperRegNoPanel);
			this.Controls.Add(this.NotifyPartyRegNoPanel);
			this.Name = "JPManifestBillPartiesSpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeRegNoPanel.ResumeLayout(false);
			this.ConsigneeRegNoPanel.PerformLayout();
			this.ConsigneeRegTypeDropEdit.ResumeLayout(true);
			this.ConsigneeRegTypeDropEdit.PerformLayout();
			this.ShipperRegNoPanel.ResumeLayout(false);
			this.ShipperRegNoPanel.PerformLayout();
			this.ShipperRegTypeDropEdit.ResumeLayout(true);
			this.ShipperRegTypeDropEdit.PerformLayout();
			this.NotifyPartyRegNoPanel.ResumeLayout(false);
			this.NotifyPartyRegNoPanel.PerformLayout();
			this.NotifyPartyRegTypeDropEdit.ResumeLayout(true);
			this.NotifyPartyRegTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZPanel ConsigneeRegNoPanel;
		ZDropEdit ConsigneeRegTypeDropEdit;
		ZTextBox ConsigneeRegNumTextBox;
		ZPanel ShipperRegNoPanel;
		ZDropEdit ShipperRegTypeDropEdit;
		ZTextBox ShipperRegNumTextBox;
		ZPanel NotifyPartyRegNoPanel;
		ZDropEdit NotifyPartyRegTypeDropEdit;
		ZTextBox NotifyPartyRegNumTextBox;
	}
}

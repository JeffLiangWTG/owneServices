namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class SeaCargoHouseUserControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OceanBillDetailsUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.OceanBillDetailsUserControl();
			this.HouseBillDetailsUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.HouseBillDetailsUserControl();
			this.HouseBillPartiesUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.HouseBillPartiesUserControl();
			this.UnderbondAndPackingUserControlForCMR = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoUnderbondAndPackingStandAloneUserControl();
			this.HouseBillCustomFieldsControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoHouseBillCustomFieldsUserControl();
			this.HouseBillTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OceanBillDetailsUserControl.SuspendLayout();
			this.HouseBillDetailsUserControl.SuspendLayout();
			this.HouseBillPartiesUserControl.SuspendLayout();
			this.UnderbondAndPackingUserControlForCMR.SuspendLayout();
			this.HouseBillCustomFieldsControl.SuspendLayout();
			this.HouseBillTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// OceanBillDetailsUserControl
			// 
			this.OceanBillDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OceanBillDetailsUserControl, "OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill)));
			this.OceanBillDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.OceanBillDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OceanBillDetailsUserControl.Name = "OceanBillDetailsUserControl";
			this.OceanBillDetailsUserControl.ReadOnly = true;
			this.OceanBillDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 108, true);
			this.OceanBillDetailsUserControl.TabIndex = 0;
			this.OceanBillDetailsUserControl.TabStop = false;
			// 
			// HouseBillDetailsUserControl
			// 
			this.HouseBillDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillDetailsUserControl, ".");
			this.HouseBillDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillDetailsUserControl.Name = "HouseBillDetailsUserControl";
			this.HouseBillDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 312, true);
			this.HouseBillDetailsUserControl.TabIndex = 0;
			this.HouseBillDetailsUserControl.TabStop = false;
			// 
			// HouseBillPartiesUserControl
			// 
			this.HouseBillPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillPartiesUserControl, ".");
			this.HouseBillPartiesUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.HouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 0, true);
			this.HouseBillPartiesUserControl.Name = "HouseBillPartiesUserControl";
			this.HouseBillPartiesUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.HouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 312, true);
			this.HouseBillPartiesUserControl.TabIndex = 1;
			// 
			// HouseBillCustomFieldsControl
			// 
			this.HouseBillCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillCustomFieldsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)))));
			this.HouseBillCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HouseBillCustomFieldsControl.Name = "HouseBillCustomFieldsControl";
			this.HouseBillCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 312, true);
			this.HouseBillCustomFieldsControl.TabIndex = 1;
			// 
			// HouseBillTabControl
			// 
			this.HouseBillTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HouseBillTabControl.Controls.Add(this.DetailsTabPage);
			this.HouseBillTabControl.Controls.Add(this.PackingTabPage);
			this.HouseBillTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.HouseBillTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
			this.HouseBillTabControl.Name = "HouseBillTabControl";
			this.HouseBillTabControl.SelectedIndex = 0;
			this.HouseBillTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 339, true);
			this.HouseBillTabControl.TabIndex = 1;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.HouseBillDetailsUserControl);
			this.DetailsTabPage.Controls.Add(this.HouseBillPartiesUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 312, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.Text = "Details";
			// 
			// PackingTabPage
			// 
			this.PackingTabPage.Controls.Add(this.UnderbondAndPackingUserControlForCMR);
			this.PackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackingTabPage.Name = "PackingTabPage";
			this.PackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 312, true);
			this.PackingTabPage.TabIndex = 1;
			this.PackingTabPage.Text = "Packing";
			// 
			// UnderbondAndPackingUserControlForCMR
			// 
			this.BindingSource.SetBindingMember(this.UnderbondAndPackingUserControlForCMR, ".");
			this.UnderbondAndPackingUserControlForCMR.AllowDrop = true;
			this.UnderbondAndPackingUserControlForCMR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondAndPackingUserControlForCMR.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondAndPackingUserControlForCMR.Name = "UnderbondAndPackingUserControlForCMR";
			this.UnderbondAndPackingUserControlForCMR.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 303, true);
			this.UnderbondAndPackingUserControlForCMR.TabIndex = 3;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("3DBDEB66-1C0D-4D95-B141-4CC0843F7656", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.HouseBillCustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 312, true);
			this.CustomFieldsTabPage.TabIndex = 2;
			this.CustomFieldsTabPage.Text = "Custom Fields";
			// 
			// SeaCargoHouseUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.HouseBillTabControl);
			this.Controls.Add(this.OceanBillDetailsUserControl);
			this.Name = "SeaCargoHouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 447, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OceanBillDetailsUserControl.ResumeLayout(true);
			this.OceanBillDetailsUserControl.PerformLayout();
			this.HouseBillDetailsUserControl.ResumeLayout(true);
			this.HouseBillDetailsUserControl.PerformLayout();
			this.HouseBillPartiesUserControl.ResumeLayout(true);
			this.HouseBillPartiesUserControl.PerformLayout();
			this.UnderbondAndPackingUserControlForCMR.ResumeLayout(true);
			this.UnderbondAndPackingUserControlForCMR.PerformLayout();
			this.HouseBillCustomFieldsControl.ResumeLayout(true);
			this.HouseBillCustomFieldsControl.PerformLayout();
			this.HouseBillTabControl.ResumeLayout(false);
			this.HouseBillTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		OceanBillDetailsUserControl OceanBillDetailsUserControl;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl HouseBillTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage PackingTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		HouseBillDetailsUserControl HouseBillDetailsUserControl;
		HouseBillPartiesUserControl HouseBillPartiesUserControl;
		SeaCargoUnderbondAndPackingStandAloneUserControl UnderbondAndPackingUserControlForCMR;
		SeaCargoHouseBillCustomFieldsUserControl HouseBillCustomFieldsControl;
		System.ComponentModel.IContainer components;
	}
}

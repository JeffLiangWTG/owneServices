using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class OrgRequiredFieldsControl : ZUserControl
	{
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireAddressCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequirePhoneNumberCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireEmailCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireWebCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireFaxNumberCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireFaxEmailOrWebCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireARContactCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireCityCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequirePhoneOrBusinessNoCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireBusinessNoCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RequireBranchCheckEdit;
		private Enterprise.ZArchitecture.ZLabel NoteLabel;

		private void InitializeComponent()
		{
			this.RequireAddressCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireBranchCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireCityCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequirePhoneNumberCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequirePhoneOrBusinessNoCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireBusinessNoCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireEmailCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireWebCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireFaxNumberCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireFaxEmailOrWebCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RequireARContactCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NoteLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// RequireAddressCheckEdit
			// 
			this.RequireAddressCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireAddressCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireAddressCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RequireAddressCheckEdit.Name = "RequireAddressCheckEdit";
			this.RequireAddressCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.RequireAddressCheckEdit.TabIndex = 1;
			this.RequireAddressCheckEdit.Text = Res.GetString("c50e61cb-de9c-463e-ab91-46177fc19fc4", "Require Address 2:");
			// 
			// RequireBranchCheckEdit
			// 
			this.RequireBranchCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireBranchCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireBranchCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.RequireBranchCheckEdit.Name = "RequireBranchCheckEdit";
			this.RequireBranchCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.RequireBranchCheckEdit.TabIndex = 1;
			this.RequireBranchCheckEdit.Text = Res.GetString("41ec7176-4673-47db-9734-67b193514973", "Require Branch:");
			// 
			// RequireCityCheckEdit
			// 
			this.RequireCityCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireCityCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireCityCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.RequireCityCheckEdit.Name = "RequireCityCheckEdit";
			this.RequireCityCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.RequireCityCheckEdit.TabIndex = 1;
			this.RequireCityCheckEdit.Text = Res.GetString("53bf7e7d-f673-45b4-b3e4-c270eb2e3025", "Require City:");
			// 
			// RequirePhoneNumberCheckEdit
			// 
			this.RequirePhoneNumberCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequirePhoneNumberCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequirePhoneNumberCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.RequirePhoneNumberCheckEdit.Name = "RequirePhoneNumberCheckEdit";
			this.RequirePhoneNumberCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.RequirePhoneNumberCheckEdit.TabIndex = 1;
			this.RequirePhoneNumberCheckEdit.Text = Res.GetString("fdeb15e1-28e0-4d38-bb4e-7245dfb62f9c", "Require Phone Number:");
			// 
			// RequirePhoneOrBusinessNoCheckEdit
			// 
			this.RequirePhoneOrBusinessNoCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequirePhoneOrBusinessNoCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequirePhoneOrBusinessNoCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.RequirePhoneOrBusinessNoCheckEdit.Name = "RequirePhoneOrBusinessNoCheckEdit";
			this.RequirePhoneOrBusinessNoCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 48, true);
			this.RequirePhoneOrBusinessNoCheckEdit.TabIndex = 1;
			// 
			// RequireBusinessNoCheckEdit
			// 
			this.RequireBusinessNoCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireBusinessNoCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireBusinessNoCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.RequireBusinessNoCheckEdit.Name = "RequireBusinessNoCheckEdit";
			this.RequireBusinessNoCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.RequireBusinessNoCheckEdit.TabIndex = 1;
			// 
			// RequireEmailCheckEdit
			// 
			this.RequireEmailCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireEmailCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireEmailCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 32, true);
			this.RequireEmailCheckEdit.Name = "RequireEmailCheckEdit";
			this.RequireEmailCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.RequireEmailCheckEdit.TabIndex = 1;
			this.RequireEmailCheckEdit.Text = Res.GetString("7d4ccf01-3596-42cd-92a8-ae54d307b56a", "Require Email Address:");
			// 
			// RequireWebCheckEdit
			// 
			this.RequireWebCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireWebCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireWebCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 64, true);
			this.RequireWebCheckEdit.Name = "RequireWebCheckEdit";
			this.RequireWebCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.RequireWebCheckEdit.TabIndex = 1;
			this.RequireWebCheckEdit.Text = Res.GetString("3e86457a-f823-404c-b508-dd31ae7b4acb", "Require Web Address:");
			// 
			// RequireFaxNumberCheckEdit
			// 
			this.RequireFaxNumberCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireFaxNumberCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireFaxNumberCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 0, true);
			this.RequireFaxNumberCheckEdit.Name = "RequireFaxNumberCheckEdit";
			this.RequireFaxNumberCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 24, true);
			this.RequireFaxNumberCheckEdit.TabIndex = 1;
			this.RequireFaxNumberCheckEdit.Text = Res.GetString("a2c4f7d8-f265-47f6-a1da-997a99e3cde5", "Require Fax Number:");
			// 
			// RequireFaxEmailOrWebCheckEdit
			// 
			this.RequireFaxEmailOrWebCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireFaxEmailOrWebCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireFaxEmailOrWebCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 91, true);
			this.RequireFaxEmailOrWebCheckEdit.Name = "RequireFaxEmailOrWebCheckEdit";
			this.RequireFaxEmailOrWebCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 34, true);
			this.RequireFaxEmailOrWebCheckEdit.TabIndex = 1;
			this.RequireFaxEmailOrWebCheckEdit.Text = Res.GetString("24718b5b-9dc9-49bb-82f0-765fc4478088", "Require Fax, Email or Web:");
			// 
			// RequireARContactCheckEdit
			//
			this.RequireARContactCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RequireARContactCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RequireARContactCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 123, true);
			this.RequireARContactCheckEdit.Name = "RequireARContactCheckEdit";
			this.RequireARContactCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 34, true);
			this.RequireARContactCheckEdit.TabIndex = 1;
			this.RequireARContactCheckEdit.Text = Res.GetString("4AAD9F5A-0EBF-45EC-AD3B-4AF2699C6462", "Require A/R Contact:");
			// 
			// NoteLabel
			// 
			this.NoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 196, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 36, true);
			this.NoteLabel.TabIndex = 2;
			this.NoteLabel.Text = Res.GetString("ede055d6-bbee-4f5e-b984-fef260a18584", "* These fields will only be required for organizations in your home country/region.");
			// 
			// OrgRequiredFieldsControl
			// 
			this.Controls.Add(this.NoteLabel);
			this.Controls.Add(this.RequireAddressCheckEdit);
			this.Controls.Add(this.RequireBranchCheckEdit);
			this.Controls.Add(this.RequireCityCheckEdit);
			this.Controls.Add(this.RequirePhoneNumberCheckEdit);
			this.Controls.Add(this.RequirePhoneOrBusinessNoCheckEdit);
			this.Controls.Add(this.RequireBusinessNoCheckEdit);
			this.Controls.Add(this.RequireEmailCheckEdit);
			this.Controls.Add(this.RequireWebCheckEdit);
			this.Controls.Add(this.RequireFaxNumberCheckEdit);
			this.Controls.Add(this.RequireFaxEmailOrWebCheckEdit);
			this.Controls.Add(this.RequireARContactCheckEdit);
			this.Name = "OrgRequiredFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
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
	}
}

namespace Enterprise.Customs.EU.GUI.Registry
{
	partial class NctsDefaultConsignorConsigneeRegistryItemUserControl
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
			this.LeaveBlankCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ValueFromCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsignorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Registry.NctsDefaultConsignorConsignee);
			// 
			// LeaveBlankCheckBox
			// 
			this.BindingSource.SetBindingMember(this.LeaveBlankCheckBox, "LeaveBlank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Registry.NctsDefaultConsignorConsignee)(null)).LeaveBlank)));
			this.LeaveBlankCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LeaveBlankCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 12, true);
			this.LeaveBlankCheckBox.Name = "LeaveBlankCheckBox";
			this.LeaveBlankCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 24, true);
			this.LeaveBlankCheckBox.TabIndex = 0;
			// 
			// ValueFromCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ValueFromCheckBox, "ValueFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Registry.NctsDefaultConsignorConsignee)(null)).ValueFrom)));
			this.ValueFromCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ValueFromCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 42, true);
			this.ValueFromCheckBox.Name = "ValueFromCheckBox";
			this.ValueFromCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 24, true);
			this.ValueFromCheckBox.CheckedChanged += new System.EventHandler(this.ValueFromCheckBox_CheckedChanged);
			this.ValueFromCheckBox.TabIndex = 1;
			// 
			// ConsignorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ConsignorCheckBox, "Consignor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Registry.NctsDefaultConsignorConsignee)(null)).Consignor)));
			this.ConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 72, true);
			this.ConsignorCheckBox.Name = "ConsignorCheckBox";
			this.ConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 24, true);
			this.ConsignorCheckBox.Visible = this.ValueFromCheckBox.Checked;
			this.ConsignorCheckBox.TabIndex = 2;
			// 
			// LeaveBlankCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeCheckBox, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Registry.NctsDefaultConsignorConsignee)(null)).Consignee)));
			this.ConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 102, true);
			this.ConsigneeCheckBox.Name = "ConsigneeCheckBox";
			this.ConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 24, true);
			this.ConsigneeCheckBox.Visible = this.ValueFromCheckBox.Checked;
			this.ConsigneeCheckBox.TabIndex = 3;
			// 
			// NctsDefaultConsigneeConsignorRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LeaveBlankCheckBox);
			this.Controls.Add(this.ValueFromCheckBox);
			this.Controls.Add(this.ConsigneeCheckBox);
			this.Controls.Add(this.ConsignorCheckBox);
			this.Name = "NctsDefaultPrincipalRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(323, 132, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZCheckBox LeaveBlankCheckBox;
		ZArchitecture.GUI.ZCheckBox ValueFromCheckBox;
		ZArchitecture.GUI.ZCheckBox ConsigneeCheckBox;
		ZArchitecture.GUI.ZCheckBox ConsignorCheckBox;
	}
}

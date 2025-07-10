
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class StandAloneFsrEnquiryFormForNew
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

 
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BadgeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zMasterBillControl1 = new Enterprise.ZArchitecture.ZMasterBillControl();
			this.DatabaseCode = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 186, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 24, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew);
			// 
			// CreateButton
			//
			this.CreateButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3d5d02ea-fd9c-467c-8bbe-86e12b2b5cec", "Inquire");
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 156, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CreateButton.TabIndex = 7;
			this.CreateButton.UseVisualStyleBackColor = true;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// BadgeDropEdit
			// 
			this.BadgeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BadgeDropEdit, "PIMA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).PIMA)));
			this.BadgeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 119, true);
			this.BadgeDropEdit.Name = "BadgeDropEdit";
			this.BadgeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BadgeDropEdit.TabIndex = 6;
			// 
			// CancelButton
			//
			this.CancelButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("1a60b060-5402-4f95-827b-56746a261da6", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 156, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelButton.TabIndex = 8;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "HAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).HAWB)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 38, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.zTextBox2.TabIndex = 1;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "SRF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).SRF)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 67, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.zTextBox3.TabIndex = 2;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "Airport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).Airport)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 93, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.zTextBox4.TabIndex = 4;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "Shed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).Shed)));
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 93, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.zTextBox5.TabIndex = 5;
			// 
			// zMasterBillControl1
			// 
			this.zMasterBillControl1.AllowAlphaInMAWP = true;
			this.zMasterBillControl1.AllowDrop = true;
			this.zMasterBillControl1.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zMasterBillControl1, "MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).MAWB)));
			this.zMasterBillControl1.FormattedMasterBill = "";
			this.zMasterBillControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 10, true);
			this.zMasterBillControl1.Name = "zMasterBillControl1";
			this.zMasterBillControl1.ReadOnly = false;
			this.zMasterBillControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zMasterBillControl1.TabIndex = 0;
			// 
			// DatabaseCode
			// 
			this.DatabaseCode.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatabaseCode, "DatabaseToQuery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew)(null)).DatabaseToQuery)));
			this.DatabaseCode.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("653286ca-5009-446c-8b64-dc1c3c1dc437", "DB", "Database", "Which CCS-UK database to query");
			this.DatabaseCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 67, true);
			this.DatabaseCode.Name = "DatabaseCode";
			this.DatabaseCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DatabaseCode.TabIndex = 3;
			// 
			// StandAloneFsrEnquiryFormForNew
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d1b21066-37ce-4c27-9cb5-ff1391e55bc4", "Community Database Enquiry (FSR)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 210, true);
			this.Controls.Add(this.DatabaseCode);
			this.Controls.Add(this.zMasterBillControl1);
			this.Controls.Add(this.zTextBox5);
			this.Controls.Add(this.zTextBox4);
			this.Controls.Add(this.zTextBox3);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.BadgeDropEdit);
			this.Controls.Add(this.CreateButton);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "StandAloneFsrEnquiryFormForNew";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CreateButton, 0);
			this.Controls.SetChildIndex(this.BadgeDropEdit, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zTextBox3, 0);
			this.Controls.SetChildIndex(this.zTextBox4, 0);
			this.Controls.SetChildIndex(this.zTextBox5, 0);
			this.Controls.SetChildIndex(this.zMasterBillControl1, 0);
			this.Controls.SetChildIndex(this.DatabaseCode, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion 


		public ZArchitecture.GUI.ZButton CreateButton;
		private ZArchitecture.GUI.ZDropEdit BadgeDropEdit;
		new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZTextBox zTextBox3;
		private ZArchitecture.ZTextBox zTextBox4;
		private ZArchitecture.ZTextBox zTextBox5;
		private ZArchitecture.ZMasterBillControl zMasterBillControl1;
		private ZArchitecture.GUI.ZDropEdit DatabaseCode;

	}
}

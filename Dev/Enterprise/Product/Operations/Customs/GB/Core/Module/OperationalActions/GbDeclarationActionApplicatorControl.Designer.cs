using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public partial class GbDeclarationActionApplicatorControl : ZUserControl
	{
		ZArchitecture.ZTextBox txtMawp;
		ZArchitecture.ZTextBox txtMawn;
		ZArchitecture.ZTextBox txtAirport;
		ZCheckBox chkDisassociate;
		ZCheckBox chkAssociate;
		ZCheckBox chkClose;
		ZGroupBox zGroupBox1;
		ZGroupBox zGroupBox2;
		ZGroupBox grpAutomatic;
		ZGroupBox grpManual;
		ZRadioButton rdoManual;
		ZRadioButton rdoAutomatic;
		ZArchitecture.ZTextBox txtMucrManual;
		ZArchitecture.ZTextBox txtShed;

		void InitializeComponent()
		{
			this.txtMawp = new ZArchitecture.ZTextBox();
			this.txtMawn = new ZArchitecture.ZTextBox();
			this.txtAirport = new ZArchitecture.ZTextBox();
			this.txtShed = new ZArchitecture.ZTextBox();
			this.chkDisassociate = new ZCheckBox();
			this.chkAssociate = new ZCheckBox();
			this.chkClose = new ZCheckBox();
			this.zGroupBox1 = new ZGroupBox();
			this.rdoManual = new ZRadioButton();
			this.rdoAutomatic = new ZRadioButton();
			this.grpManual = new ZGroupBox();
			this.txtMucrManual = new ZArchitecture.ZTextBox();
			this.grpAutomatic = new ZGroupBox();
			this.zGroupBox2 = new ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.grpManual.SuspendLayout();
			this.grpAutomatic.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GbDeclarationActionMethodApplicator);
			// 
			// txtMawp
			// 
			this.BindingSource.SetBindingMember(this.txtMawp, "Mawp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).Mawp);
			this.txtMawp.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("GbDeclarationActionApplicatorControl|ea5ce1a2-0996-4f45-9100-13911796a561", "MAWP", "MAWP", "MAWB prefix", "MAWB prefix, e.g. 125");
			this.txtMawp.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 19, true);
			this.txtMawp.Name = "txtMawp";
			this.txtMawp.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.txtMawp.TabIndex = 0;
			// 
			// txtMawn
			// 
			this.BindingSource.SetBindingMember(this.txtMawn, "Mawn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).Mawn);
			this.txtMawn.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("GbDeclarationActionApplicatorControl|5e87cfbb-7516-4808-8fb7-1505566b873f", "MAWN", "MAWB number", "MAWB number", "MAWB number, e.g. 12345678");
			this.txtMawn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 19, true);
			this.txtMawn.Name = "txtMawn";
			this.txtMawn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.txtMawn.TabIndex = 1;
			// 
			// txtAirport
			// 
			this.BindingSource.SetBindingMember(this.txtAirport, "Airport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).Airport);
			this.txtAirport.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("GbDeclarationActionApplicatorControl|403a5118-fa48-458a-a935-479500f7e120", "Airport", "Airport", "Airport (5 char)", "Airport code, 5 characters, e.g. GBLHR");
			this.txtAirport.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 44, true);
			this.txtAirport.Name = "txtAirport";
			this.txtAirport.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
			this.txtAirport.TabIndex = 3;
			// 
			// txtShed
			// 
			this.BindingSource.SetBindingMember(this.txtShed, "Shed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).Shed);
			this.txtShed.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("GbDeclarationActionApplicatorControl|4899c4a5-5d87-4c3f-b263-a958b8956f73", "Shed", "Shed", "Shed code", "Shed code, e.g. BAC");
			this.txtShed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 44, true);
			this.txtShed.Name = "txtShed";
			this.txtShed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.txtShed.TabIndex = 2;
			// 
			// chkDisassociate
			// 
			this.BindingSource.SetBindingMember(this.chkDisassociate, "ActionDisassociate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).ActionDisassociate);
			this.chkDisassociate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkDisassociate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.chkDisassociate.Name = "chkDisassociate";
			this.chkDisassociate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 23, true);
			this.chkDisassociate.TabIndex = 0;
			this.chkDisassociate.Text = "Disassociate declaration from existing MUCR";
			this.chkDisassociate.UseVisualStyleBackColor = true;
			// 
			// chkAssociate
			// 
			this.BindingSource.SetBindingMember(this.chkAssociate, "ActionAssociate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).ActionAssociate);
			this.chkAssociate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkAssociate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 43, true);
			this.chkAssociate.Name = "chkAssociate";
			this.chkAssociate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.chkAssociate.TabIndex = 1;
			this.chkAssociate.Text = "Associate declaration to new MUCR";
			this.chkAssociate.UseVisualStyleBackColor = true;
			// 
			// chkClose
			// 
			this.BindingSource.SetBindingMember(this.chkClose, "ActionClose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).ActionClose);
			this.chkClose.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.chkClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 64, true);
			this.chkClose.Name = "chkClose";
			this.chkClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.chkClose.TabIndex = 2;
			this.chkClose.Text = "Close MUCR using first declaration";
			this.chkClose.UseVisualStyleBackColor = true;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.rdoManual);
			this.zGroupBox1.Controls.Add(this.rdoAutomatic);
			this.zGroupBox1.Controls.Add(this.grpManual);
			this.zGroupBox1.Controls.Add(this.grpAutomatic);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 184, true);
			this.zGroupBox1.TabIndex = 7;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Update MUCR using badge options and these new data...";
			// 
			// rdoManual
			// 
			this.rdoManual.AutoCheck = false;
			this.rdoManual.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rdoManual.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 20, true);
			this.rdoManual.Name = "rdoManual";
			this.rdoManual.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.rdoManual.TabIndex = 1;
			this.rdoManual.Text = "Manual override";
			this.rdoManual.UseVisualStyleBackColor = true;
			this.rdoManual.CheckedChanged += new EventHandler(this.rdoManual_CheckedChanged);
			// 
			// rdoAutomatic
			// 
			this.rdoAutomatic.AutoCheck = false;
			this.rdoAutomatic.Checked = true;
			this.rdoAutomatic.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.rdoAutomatic.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 20, true);
			this.rdoAutomatic.Name = "rdoAutomatic";
			this.rdoAutomatic.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 24, true);
			this.rdoAutomatic.TabIndex = 0;
			this.rdoAutomatic.TabStop = true;
			this.rdoAutomatic.Text = "Automatic generation";
			this.rdoAutomatic.UseVisualStyleBackColor = true;
			this.rdoAutomatic.CheckedChanged += new EventHandler(this.rdoAutomatic_CheckedChanged);
			// 
			// grpManual
			// 
			this.grpManual.Controls.Add(this.txtMucrManual);
			this.grpManual.Enabled = false;
			this.grpManual.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 125, true);
			this.grpManual.Name = "grpManual";
			this.grpManual.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 53, true);
			this.grpManual.TabIndex = 10;
			this.grpManual.TabStop = false;
			this.grpManual.Text = "Manual MUCR entry";
			// 
			// txtMucrManual
			// 
			this.BindingSource.SetBindingMember(this.txtMucrManual, "MucrManual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationActionMethodApplicator)(null)).MucrManual);
			this.txtMucrManual.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("GbDeclarationActionApplicatorControl|f1e9e7b1-43a2-4547-9bf7-2d1a4728a4fb", "MUCR", "MUCR", "MUCR (verbatim)", "MUCR as should be applied directly, without attention to badge generation style");
			this.txtMucrManual.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 18, true);
			this.txtMucrManual.Name = "txtMucrManual";
			this.txtMucrManual.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 20, true);
			this.txtMucrManual.TabIndex = 0;
			// 
			// grpAutomatic
			// 
			this.grpAutomatic.Controls.Add(this.txtMawp);
			this.grpAutomatic.Controls.Add(this.txtMawn);
			this.grpAutomatic.Controls.Add(this.txtShed);
			this.grpAutomatic.Controls.Add(this.txtAirport);
			this.grpAutomatic.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 49, true);
			this.grpAutomatic.Name = "grpAutomatic";
			this.grpAutomatic.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 70, true);
			this.grpAutomatic.TabIndex = 9;
			this.grpAutomatic.TabStop = false;
			this.grpAutomatic.Text = "Automatic MUCR generation using badge options";
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.chkClose);
			this.zGroupBox2.Controls.Add(this.chkDisassociate);
			this.zGroupBox2.Controls.Add(this.chkAssociate);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 190, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 90, true);
			this.zGroupBox2.TabIndex = 8;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Send EAC messages to CHIEF";
			// 
			// GbDeclarationActionApplicatorControl
			// 
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Name = "GbDeclarationActionApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.grpManual.ResumeLayout(false);
			this.grpManual.PerformLayout();
			this.grpAutomatic.ResumeLayout(false);
			this.grpAutomatic.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}

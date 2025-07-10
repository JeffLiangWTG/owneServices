using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class SystemToSystemCertificateControl
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

		
		protected internal ZTextBox zTextBox1;
		protected internal ZTextBox zTextBox2;
		protected internal ZTextBox zTextBox3;
		protected internal ZDateEdit validFromDateTimeEdit;
		protected internal ZDateEdit validToDateTimeEdit;
		protected internal ZButton resetButton;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.validFromDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.validToDateTimeEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.resetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SystemToSystemTrustInfo);
			
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "TenantId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).TenantId)));
			this.zTextBox1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A5DA6B18-7C15-4977-9F9D-A836EBC6F8AD", "Tenant ID");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "ClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).ClientId)));
			this.zTextBox2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("B4565E5B-68B6-40B9-BA95-E0BE680A9E5E", "Client ID");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 55, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ReadOnly = true;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.zTextBox2.TabIndex = 2;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "CertificateThumbprint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).CertificateThumbprint)));
			this.zTextBox3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("40760EC1-2C38-4892-B253-17AED0A654AD", "Certificate Thumbprint");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 90, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.ReadOnly = true;
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.zTextBox3.TabIndex = 3;
			// 
			// validFromDateTimeEdit
			// 
			this.validFromDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.validFromDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validFromDateTimeEdit, "CertificateValidFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).CertificateValidFrom)));
			this.validFromDateTimeEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2800795B-C6DB-4B52-BAFB-C4635DE7BE68", "Certificate Valid From");
			this.validFromDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.validFromDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 125, true);
			this.validFromDateTimeEdit.Name = "validFromDateTimeEdit";
			this.validFromDateTimeEdit.ReadOnly = true;
			this.validFromDateTimeEdit.TabIndex = 4;
			// 
			// validToDateTimeEdit
			// 
			this.validToDateTimeEdit.AutoCompleteMonthThreshold = 1;
			this.validToDateTimeEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validToDateTimeEdit, "CertificateValidTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).CertificateValidTo)));
			this.validToDateTimeEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2EC92985-E927-403B-AF78-4AF372754F31", "Certificate Valid To");
			this.validToDateTimeEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.validToDateTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 160, true);
			this.validToDateTimeEdit.Name = "validToDateTimeEdit";
			this.validToDateTimeEdit.ReadOnly = true;
			this.validToDateTimeEdit.TabIndex = 5;
			//
			// resetButton
			//
			this.resetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 195, true);
			this.resetButton.Name = "resetButton";
			this.resetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.resetButton.TabIndex = 6;
			this.resetButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3627E4DE-4E34-4A39-946D-139379A3F63D", "Reset");
			this.resetButton.Click += new System.EventHandler(this.ResetButton_Click);
			// 
			// SystemToSystemCertificateControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.zTextBox3);
			this.Controls.Add(this.validFromDateTimeEdit);
			this.Controls.Add(this.validToDateTimeEdit);
			this.Controls.Add(this.resetButton);
			this.Name = "SystemToSystemCertificateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}

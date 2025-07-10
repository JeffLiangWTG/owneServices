using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ExcludedFullyDigitalizedElectronicInvoiceDataControl
	{
		private void InitializeComponent()
		{
			this.BuyerAddressCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BuyerPhoneNumberCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BuyerBankAccountCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ExcludedFullyDigitalizedElectronicInvoiceData);
			// 
			// BuyerAddressCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.BuyerAddressCheckEdit, "BuyerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ExcludedFullyDigitalizedElectronicInvoiceData)(null)).BuyerAddress)));
			this.BuyerAddressCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 20, true);
			this.BuyerAddressCheckEdit.Name = "BuyerAddressCheckEdit";
			this.BuyerAddressCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.BuyerAddressCheckEdit.TabIndex = 1;
			this.BuyerAddressCheckEdit.Text = Res.GetString("7F90138F-2912-48D3-A363-B44BC63D38A7", "Buyer's Address");
			// 
			// BuyerPhoneNumberCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.BuyerPhoneNumberCheckEdit, "BuyerPhoneNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ExcludedFullyDigitalizedElectronicInvoiceData)(null)).BuyerPhoneNumber)));
			this.BuyerPhoneNumberCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 47, true);
			this.BuyerPhoneNumberCheckEdit.Name = "BuyerPhoneNumberCheckEdit";
			this.BuyerPhoneNumberCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.BuyerPhoneNumberCheckEdit.TabIndex = 2;
			this.BuyerPhoneNumberCheckEdit.Text = Res.GetString("D33B2A5D-C41D-40FD-9BD0-6B63A637F40B", "Buyer's Phone Number");
			// 
			// BuyerBankAccountCheckEdit
			// 
			this.BindingSource.SetBindingMember(this.BuyerBankAccountCheckEdit, "BuyerBankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Registry.Business.ExcludedFullyDigitalizedElectronicInvoiceData)(null)).BuyerBankAccount)));
			this.BuyerBankAccountCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 74, true);
			this.BuyerBankAccountCheckEdit.Name = "BuyerBankAccountCheckEdit";
			this.BuyerBankAccountCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 24, true);
			this.BuyerBankAccountCheckEdit.TabIndex = 3;
			this.BuyerBankAccountCheckEdit.Text = Res.GetString("3CFB33C4-D094-447E-AB86-AFAC6371F7B1", "Buyer's Bank Account");
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.zGroupBox1.Controls.Add(this.BuyerAddressCheckEdit);
			this.zGroupBox1.Controls.Add(this.BuyerPhoneNumberCheckEdit);
			this.zGroupBox1.Controls.Add(this.BuyerBankAccountCheckEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 125, true);
			this.zGroupBox1.TabIndex = 4;
			this.zGroupBox1.TabStop = false;
			// 
			// ExcludedFullyDigitalizedElectronicInvoiceDataControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.zGroupBox1);
			this.zGroupBox1.Controls.Add(this.BuyerAddressCheckEdit);
			this.zGroupBox1.Controls.Add(this.BuyerPhoneNumberCheckEdit);
			this.zGroupBox1.Controls.Add(this.BuyerBankAccountCheckEdit);
			this.Name = "ExcludedFullyDigitalizedElectronicInvoiceDataControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 196, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

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

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox BuyerAddressCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox BuyerPhoneNumberCheckEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox BuyerBankAccountCheckEdit;
	}
}

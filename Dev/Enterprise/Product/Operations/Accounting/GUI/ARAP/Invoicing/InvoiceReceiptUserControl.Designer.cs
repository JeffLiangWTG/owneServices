using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceReceiptUserControl
	{


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReceiptPaymentDetailsGroupbox = new ZGroupBox();
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox = new ZTextBox();
			this.ReceiptPaymentAH_DrawerBranchTextbox = new ZTextBox();
			this.ReceiptPaymentAH_DrawerBankTextbox = new ZTextBox();
			this.ReceiptPaymentAH_ChequeDrawerTextbox = new ZTextBox();
			this.ReceiptPaymentAH_ABFindbox = new ZGuidFindBox();
			this.ReceiptPaymentAH_ReceiptTypeDropEdit = new ZDropEdit();
			this.ReceiptPaymentAH_InvoiceDateEdit = new ZDateEdit();
			this.ReceiptPaymentAH_PostDateEdit = new ZDateEdit();
			this.ReceiptPaymentAH_DescTextbox = new ZTextBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReceiptPaymentDetailsGroupbox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ARAP.Invoicing.ARInvoice);
			// 
			// ReceiptPaymentDetailsGroupbox
			// 
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_PostDateEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_InvoiceDateEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ChequeOrReferenceTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_DrawerBranchTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_DrawerBankTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ChequeDrawerTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ABFindbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ReceiptTypeDropEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_DescTextbox);
			this.ReceiptPaymentDetailsGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptPaymentDetailsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiptPaymentDetailsGroupbox.Name = "ReceiptPaymentDetailsGroupbox";
			this.ReceiptPaymentDetailsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 124, true);
			this.ReceiptPaymentDetailsGroupbox.TabIndex = 8;
			this.ReceiptPaymentDetailsGroupbox.TabStop = false;
			// 
			// ReceiptPaymentAH_ChequeOrReferenceTextbox
			// 
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ChequeOrReferenceTextbox, "ReceiptPaymentAH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_ChequeOrReference)));
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 67, true);
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Name = "ReceiptPaymentAH_ChequeOrReferenceTextbox";
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.TabIndex = 2;
			// 
			// ReceiptPaymentAH_DrawerBranchTextbox
			// 
			this.ReceiptPaymentAH_DrawerBranchTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_DrawerBranchTextbox, "ReceiptPaymentAH_DrawerBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_DrawerBranch)));
			this.ReceiptPaymentAH_DrawerBranchTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|4d510d02-ee32-4693-853b-f371db828937", "Branch", "Drawer Branch", "");
			this.ReceiptPaymentAH_DrawerBranchTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 91, true);
			this.ReceiptPaymentAH_DrawerBranchTextbox.Name = "ReceiptPaymentAH_DrawerBranchTextbox";
			this.ReceiptPaymentAH_DrawerBranchTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ReceiptPaymentAH_DrawerBranchTextbox.TabIndex = 8;
			// 
			// ReceiptPaymentAH_DrawerBankTextbox
			// 
			this.ReceiptPaymentAH_DrawerBankTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_DrawerBankTextbox, "ReceiptPaymentAH_DrawerBank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_DrawerBank)));
			this.ReceiptPaymentAH_DrawerBankTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|a723d0d0-6f7b-4dc6-ac77-ff951858a149", "Bank", "Drawer Bank", "");
			this.ReceiptPaymentAH_DrawerBankTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 67, true);
			this.ReceiptPaymentAH_DrawerBankTextbox.Name = "ReceiptPaymentAH_DrawerBankTextbox";
			this.ReceiptPaymentAH_DrawerBankTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ReceiptPaymentAH_DrawerBankTextbox.TabIndex = 7;
			// 
			// ReceiptPaymentAH_ChequeDrawerTextbox
			// 
			this.ReceiptPaymentAH_ChequeDrawerTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ChequeDrawerTextbox, "ReceiptPaymentAH_ChequeDrawer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((string)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_ChequeDrawer)));
			this.ReceiptPaymentAH_ChequeDrawerTextbox.CaptionResourceString =
				Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|c5610d53-fa87-48ab-8709-b48bc0c0178c", "Drawer",
					"Check Drawer", "");
			this.ReceiptPaymentAH_ChequeDrawerTextbox.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 43, true);
			this.ReceiptPaymentAH_ChequeDrawerTextbox.Name = "ReceiptPaymentAH_ChequeDrawerTextbox";
			this.ReceiptPaymentAH_ChequeDrawerTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ReceiptPaymentAH_ChequeDrawerTextbox.TabIndex = 6;
			// 
			// ReceiptPaymentAH_DescTextbox
			// 
			this.ReceiptPaymentAH_DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_DescTextbox, "ReceiptPaymentAH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_Desc)));
			this.ReceiptPaymentAH_DescTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|02FBDAF7-3FDC-4C86-95C5-86D420BE6F4E", "Description");
			this.ReceiptPaymentAH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 19, true);
			this.ReceiptPaymentAH_DescTextbox.Name = "ReceiptPaymentAH_DescTextbox";
			this.ReceiptPaymentAH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ReceiptPaymentAH_DescTextbox.TabIndex = 5;
			// 
			// ReceiptPaymentAH_ABFindbox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ABFindbox, "ReceiptPaymentAH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.ARInvoice)(null)).BankAccountLookup)));
			this.ReceiptPaymentAH_ABFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|46f28b1a-f723-46f0-97da-d3832f990f78", "Bank", "Bank Account", "");
			this.ReceiptPaymentAH_ABFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 43, true);
			this.ReceiptPaymentAH_ABFindbox.Name = "ReceiptPaymentAH_ABFindbox";
			this.ReceiptPaymentAH_ABFindbox.PopupCaption = null;
			this.ReceiptPaymentAH_ABFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ReceiptPaymentAH_ABFindbox.TabIndex = 1;
			// 
			// ReceiptPaymentAH_ReceiptTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ReceiptTypeDropEdit, "ReceiptPaymentAH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptMethods)));
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|968970ec-4ae3-4dd1-83e0-95c46ba62e38", "Receipt Type", "Receipt Type", "");
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Name = "ReceiptPaymentAH_ReceiptTypeDropEdit";
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.PreBoundMaxLength = 3;
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.TabIndex = 0;
			// 
			// ReceiptPaymentAH_InvoiceDateEdit
			// 
			this.ReceiptPaymentAH_InvoiceDateEdit.AllowDrop = true;
			this.ReceiptPaymentAH_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceiptPaymentAH_InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_InvoiceDateEdit, "ReceiptPaymentAH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_InvoiceDate)));
			this.ReceiptPaymentAH_InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|b504d594-5468-48ae-80f5-af1f0e9242f5", "Receipt Date");
			this.ReceiptPaymentAH_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 93, true);
			this.ReceiptPaymentAH_InvoiceDateEdit.Name = "ReceiptPaymentAH_InvoiceDateEdit";
			this.ReceiptPaymentAH_InvoiceDateEdit.TabIndex = 3;
			// 
			// ReceiptPaymentAH_PostDateEdit
			// 
			this.ReceiptPaymentAH_PostDateEdit.AllowDrop = true;
			this.ReceiptPaymentAH_PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceiptPaymentAH_PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_PostDateEdit, "ReceiptPaymentAH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.ARAP.Invoicing.ARInvoice)(null)).ReceiptPaymentAH_PostDate)));
			this.ReceiptPaymentAH_PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoiceReceiptUserControl|04f1ee3b-c6b0-4511-ba1b-102afb53d35c", "Post Date");
			this.ReceiptPaymentAH_PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 93, true);
			this.ReceiptPaymentAH_PostDateEdit.Name = "ReceiptPaymentAH_PostDateEdit";
			this.ReceiptPaymentAH_PostDateEdit.TabIndex = 4;
			// 
			// InvoiceReceiptUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReceiptPaymentDetailsGroupbox);
			this.Name = "InvoiceReceiptUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 124, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReceiptPaymentDetailsGroupbox.ResumeLayout(false);
			this.ReceiptPaymentDetailsGroupbox.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
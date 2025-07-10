using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoicePaymentUserControl
	{


		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReceiptPaymentDetailsGroupbox = new ZGroupBox();
			this.AddressWithContactControl = new ARAP.PaymentAddressWithContactControl();
			this.ReceiptPaymentAH_PostDateEdit = new ZDateEdit();
			this.ReceiptPaymentAH_InvoiceDateEdit = new ZDateEdit();
			this.CardSecurityCodeTextBox = new ZTextBox();
			this.AutoPrintZLabel = new ZLabel();
			this.AutoAllocateZLabel = new ZLabel();
			this.ClearHotChequeDetailsButton = new ZButton();
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit = new ZCalcEdit();
			this.ChequeBookGuidFindBox = new ZGuidFindBox();
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox = new ZTextBox();
			this.ReceiptPaymentAH_DescTextbox = new ZTextBox();
			this.ReceiptPaymentAH_ABFindbox = new ZGuidFindBox();
			this.ReceiptPaymentAH_ReceiptTypeDropEdit = new ZDropEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReceiptPaymentDetailsGroupbox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.ReceiptPaymentAH_PostDateEdit.SuspendLayout();
			this.ReceiptPaymentAH_InvoiceDateEdit.SuspendLayout();
			this.ChequeBookGuidFindBox.SuspendLayout();
			this.ReceiptPaymentAH_ABFindbox.SuspendLayout();
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(APInvoice);
			// 
			// ReceiptPaymentDetailsGroupbox
			// 
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.AddressWithContactControl);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_PostDateEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_InvoiceDateEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.CardSecurityCodeTextBox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.AutoPrintZLabel);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.AutoAllocateZLabel);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ClearHotChequeDetailsButton);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_OSTotalAmountCalcEdit);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ChequeBookGuidFindBox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ChequeOrReferenceTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_DescTextbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ABFindbox);
			this.ReceiptPaymentDetailsGroupbox.Controls.Add(this.ReceiptPaymentAH_ReceiptTypeDropEdit);
			this.ReceiptPaymentDetailsGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReceiptPaymentDetailsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceiptPaymentDetailsGroupbox.Name = "ReceiptPaymentDetailsGroupbox";
			this.ReceiptPaymentDetailsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 168, true);
			this.ReceiptPaymentDetailsGroupbox.TabIndex = 10;
			this.ReceiptPaymentDetailsGroupbox.TabStop = false;
			// 
			// AddressWithContactControl
			// 
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "PaymentOrganisationAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.ZAddressWithContact)(((APInvoice)(null)).PaymentOrganisationAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = null;
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddressWithContactControl, false);
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = true;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 132, true);
			this.AddressWithContactControl.TabIndex = 0;
			// 
			// ReceiptPaymentAH_PostDateEdit
			// 
			this.ReceiptPaymentAH_PostDateEdit.AllowDrop = true;
			this.ReceiptPaymentAH_PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceiptPaymentAH_PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_PostDateEdit, "ReceiptPaymentAH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APInvoice)(null)).ReceiptPaymentAH_PostDate)));
			this.ReceiptPaymentAH_PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|8036aa90-cf92-4c9d-9381-7d854053e19b", "Post Date");
			this.ReceiptPaymentAH_PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 93, true);
			this.ReceiptPaymentAH_PostDateEdit.Name = "ReceiptPaymentAH_PostDateEdit";
			this.ReceiptPaymentAH_PostDateEdit.TabIndex = 5;
			// 
			// ReceiptPaymentAH_InvoiceDateEdit
			// 
			this.ReceiptPaymentAH_InvoiceDateEdit.AllowDrop = true;
			this.ReceiptPaymentAH_InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceiptPaymentAH_InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_InvoiceDateEdit, "ReceiptPaymentAH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APInvoice)(null)).ReceiptPaymentAH_InvoiceDate)));
			this.ReceiptPaymentAH_InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|40a44d90-849a-4ba2-98a6-f1548411de7c", "Payment Date");
			this.ReceiptPaymentAH_InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 93, true);
			this.ReceiptPaymentAH_InvoiceDateEdit.Name = "ReceiptPaymentAH_InvoiceDateEdit";
			this.ReceiptPaymentAH_InvoiceDateEdit.TabIndex = 4;
			// 
			// CardSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CardSecurityCodeTextBox, "ReceiptPaymentCardSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((APInvoice)(null)).ReceiptPaymentCardSecurityCode)));
			this.CardSecurityCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|b7d887b8-125b-4507-a137-139592c00319", "Security Code");
			this.CardSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 93, true);
			this.CardSecurityCodeTextBox.Name = "CardSecurityCodeTextBox";
			this.CardSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.CardSecurityCodeTextBox.TabIndex = 8;
			// 
			// AutoPrintZLabel
			// 
			this.AutoPrintZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintZLabel, "Calc_ChequeIsAutoPrintedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((APInvoice)(null)).Calc_ChequeIsAutoPrintedLabel)));
			this.AutoPrintZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|af9fe73b-d7c4-4a38-92d7-347c3defcbdd", "Auto Print");
			this.AutoPrintZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoPrintZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 75, true);
			this.AutoPrintZLabel.Name = "AutoPrintZLabel";
			this.AutoPrintZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 13, true);
			this.AutoPrintZLabel.TabIndex = 29;
			// 
			// AutoAllocateZLabel
			// 
			this.AutoAllocateZLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoAllocateZLabel, "Calc_ChequeNumberIsAutoAllocatedLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((APInvoice)(null)).Calc_ChequeNumberIsAutoAllocatedLabel)));
			this.AutoAllocateZLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|d17ec253-caf8-46ee-9ae4-a601b792852e", "Auto Allocate");
			this.AutoAllocateZLabel.ForeColor = System.Drawing.Color.Red;
			this.AutoAllocateZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 53, true);
			this.AutoAllocateZLabel.Name = "AutoAllocateZLabel";
			this.AutoAllocateZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.AutoAllocateZLabel.TabIndex = 28;
			// 
			// ClearHotChequeDetailsButton
			// 
			this.ClearHotChequeDetailsButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|efdb03d6-c777-4977-9bbb-23c6841b559e", "Clear Hot Check Details");
			this.ClearHotChequeDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(810, 64, true);
			this.ClearHotChequeDetailsButton.Name = "ClearHotChequeDetailsButton";
			this.ClearHotChequeDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 23, true);
			this.ClearHotChequeDetailsButton.TabIndex = 9;
			this.ClearHotChequeDetailsButton.UseVisualStyleBackColor = true;
			this.ClearHotChequeDetailsButton.Click += new EventHandler(this.ClearImportedHotChequeButton_Click);
			// 
			// ReceiptPaymentAH_OSTotalAmountCalcEdit
			// 
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_OSTotalAmountCalcEdit, "ReceiptPaymentAH_OSTotalAmount_ReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((APInvoice)(null)).ReceiptPaymentAH_OSTotalAmount_ReadOnly)));
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|5a5391af-dfcd-4ce3-821b-2461f0323b14", "Hot Check Amount", "Hot Check Amount", "");
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.DecimalPlaces = 2;
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(869, 43, true);
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.Name = "ReceiptPaymentAH_OSTotalAmountCalcEdit";
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.TabIndex = 7;
			this.ReceiptPaymentAH_OSTotalAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ChequeBookGuidFindBox
			// 
			this.ChequeBookGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChequeBookGuidFindBox, "ReceiptPaymentAK_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((APInvoice)(null)).ReceiptPaymentAK_AB)));
			this.ChequeBookGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|f960fab2-358a-4d92-8378-0574efe28173", "Check Book", "Check Book", "");
			this.ChequeBookGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 67, true);
			this.ChequeBookGuidFindBox.Name = "ChequeBookGuidFindBox";
			this.ChequeBookGuidFindBox.PopupCaption = null;
			this.ChequeBookGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
			this.ChequeBookGuidFindBox.TabIndex = 3;
			// 
			// ReceiptPaymentAH_ChequeOrReferenceTextbox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ChequeOrReferenceTextbox, "ReceiptPaymentAH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((APInvoice)(null)).ReceiptPaymentAH_ChequeOrReference)));
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 43, true);
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Name = "ReceiptPaymentAH_ChequeOrReferenceTextbox";
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.ReceiptPaymentAH_ChequeOrReferenceTextbox.TabIndex = 6;
			// 
			// ReceiptPaymentAH_DescTextbox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_DescTextbox, "ReceiptPaymentAH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((APInvoice)(null)).ReceiptPaymentAH_Desc)));
			this.ReceiptPaymentAH_DescTextbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|B2E2FD20-1B31-478B-8397-DE7244C2F224", "Description");
			this.ReceiptPaymentAH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 19, true);
			this.ReceiptPaymentAH_DescTextbox.Name = "ReceiptPaymentAH_DescTextbox";
			this.ReceiptPaymentAH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 20, true);
			this.ReceiptPaymentAH_DescTextbox.TabIndex = 5;
			// 
			// ReceiptPaymentAH_ABFindbox
			// 
			this.ReceiptPaymentAH_ABFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ABFindbox, "ReceiptPaymentAH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((APInvoice)(null)).ReceiptPaymentAH_AB)));
			this.ReceiptPaymentAH_ABFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|799f2f89-cacd-4077-a101-dd8938400651", "Bank", "Bank Account", "");
			this.ReceiptPaymentAH_ABFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 43, true);
			this.ReceiptPaymentAH_ABFindbox.Name = "ReceiptPaymentAH_ABFindbox";
			this.ReceiptPaymentAH_ABFindbox.PopupCaption = null;
			this.ReceiptPaymentAH_ABFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
			this.ReceiptPaymentAH_ABFindbox.TabIndex = 2;
			// 
			// ReceiptPaymentAH_ReceiptTypeDropEdit
			// 
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptPaymentAH_ReceiptTypeDropEdit, "ReceiptPaymentAH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((APInvoice)(null)).ReceiptPaymentAH_ReceiptType)));
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePaymentUserControl|56df9752-4362-4a9a-a71a-9dd31f4eba18", "Payment Type");
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 19, true);
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Name = "ReceiptPaymentAH_ReceiptTypeDropEdit";
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.PreBoundMaxLength = 3;
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.TabIndex = 1;
			// 
			// InvoicePaymentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReceiptPaymentDetailsGroupbox);
			this.Name = "InvoicePaymentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 168, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReceiptPaymentDetailsGroupbox.ResumeLayout(false);
			this.ReceiptPaymentDetailsGroupbox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.ReceiptPaymentAH_PostDateEdit.ResumeLayout(true);
			this.ReceiptPaymentAH_PostDateEdit.PerformLayout();
			this.ReceiptPaymentAH_InvoiceDateEdit.ResumeLayout(true);
			this.ReceiptPaymentAH_InvoiceDateEdit.PerformLayout();
			this.ChequeBookGuidFindBox.ResumeLayout(true);
			this.ChequeBookGuidFindBox.PerformLayout();
			this.ReceiptPaymentAH_ABFindbox.ResumeLayout(true);
			this.ReceiptPaymentAH_ABFindbox.PerformLayout();
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.ResumeLayout(true);
			this.ReceiptPaymentAH_ReceiptTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
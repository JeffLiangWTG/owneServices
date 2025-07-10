using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using System.ComponentModel;
namespace Enterprise.Accounting.GUI.PayableOrder
{
	partial class AccPayableOrderUserControl : ZUserControl
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

		#region Order

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AccPayableOrderHeader Order
		{
			get { return (AccPayableOrderHeader)CurrentDataItem; }
		}

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OrderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GSTInclusiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APH_RXBoundCurrency = new Enterprise.ZArchitecture.GUI.ZExchangeRateControl();
			this.APH_GoodsReceivedStatusBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APH_FollowupDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_ExpectedDeliveryBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_ReadyForDeliveryBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_GoodsDescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_InvoiceDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_BookingConfDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_DueDateBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.APH_TypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APH_InvoiceNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_BookingConfRefBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.APH_OrderNumberSplitBoundTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.APH_DispositionBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APH_StageBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.APH_OrderNumberBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupplierDocumentaryDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.RightTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.OrderSplitsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OrderSplitsButtonGrid = new Enterprise.Accounting.GUI.PayableOrder.OrderSplitsButtonGrid();
			this.OrderLinesTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.orderLinesControl = new Enterprise.Accounting.GUI.PayableOrder.AccPayableOrderLinesControl();
			this.BottomTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ProductSummaryTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.orderLinesTotalByProduct = new Enterprise.Accounting.GUI.PayableOrder.AccPayableOrderLinesTotalByProductControl();
			this.OrderStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DispositionExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddressWithContactControl = new Enterprise.MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrderDetailsGroupBox.SuspendLayout();
			this.APH_RXBoundCurrency.SuspendLayout();
			this.APH_GoodsReceivedStatusBoundDropEdit.SuspendLayout();
			this.APH_FollowupDateBoundDateEdit.SuspendLayout();
			this.APH_ExpectedDeliveryBoundDateEdit.SuspendLayout();
			this.APH_ReadyForDeliveryBoundDateEdit.SuspendLayout();
			this.APH_InvoiceDateBoundDateEdit.SuspendLayout();
			this.APH_BookingConfDateBoundDateEdit.SuspendLayout();
			this.APH_DueDateBoundDateEdit.SuspendLayout();
			this.APH_TypeBoundDropEdit.SuspendLayout();
			this.APH_DispositionBoundDropEdit.SuspendLayout();
			this.APH_StageBoundDropEdit.SuspendLayout();
			this.SupplierDocumentaryDocAddressControl.SuspendLayout();
			this.RightTabControl.SuspendLayout();
			this.OrderSplitsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).BeginInit();			
			this.OrderSplitsButtonGrid.SuspendLayout();
			this.OrderLinesTab.SuspendLayout();
			this.orderLinesControl.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			this.ProductSummaryTab.SuspendLayout();
			this.orderLinesTotalByProduct.SuspendLayout();
			this.OrderStatusGroupBox.SuspendLayout();
			this.AddressWithContactControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader);
			// 
			// OrderDetailsGroupBox
			// 
			this.OrderDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7580d42e-9d15-4207-9951-f493696ef566", "Details");
			this.OrderDetailsGroupBox.Controls.Add(this.GSTInclusiveCheckBox);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_RXBoundCurrency);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_GoodsReceivedStatusBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_FollowupDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_ExpectedDeliveryBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_ReadyForDeliveryBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_GoodsDescriptionBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_InvoiceDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_BookingConfDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_DueDateBoundDateEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_TypeBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_InvoiceNumberBoundTextBox);
			this.OrderDetailsGroupBox.Controls.Add(this.APH_BookingConfRefBoundTextBox);
			this.OrderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 111, true);
			this.OrderDetailsGroupBox.Name = "OrderDetailsGroupBox";
			this.OrderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 235, true);
			this.OrderDetailsGroupBox.TabIndex = 5;
			this.OrderDetailsGroupBox.TabStop = false;
			// 
			// GSTInclusiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GSTInclusiveCheckBox, "APH_GSTInclusive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_GSTInclusive)));
			this.GSTInclusiveCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b9996d6a-ad0b-4734-9736-33e7e1dd6ea8", "Tax Inclusive Amounts");
			this.GSTInclusiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSTInclusiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 214, true);
			this.GSTInclusiveCheckBox.Name = "GSTInclusiveCheckBox";
			this.GSTInclusiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 17, true);
			this.GSTInclusiveCheckBox.TabIndex = 13;
			this.GSTInclusiveCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// APH_RXBoundCurrency
			// 
			this.APH_RXBoundCurrency.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APH_RXBoundCurrency, "APH_Calc_Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZExchangeRate)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Calc_Currency)));
			this.APH_RXBoundCurrency.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 187, true);
			this.APH_RXBoundCurrency.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_RXBoundCurrency.Name = "APH_RXBoundCurrency";
			this.APH_RXBoundCurrency.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.APH_RXBoundCurrency.TabIndex = 11;
			// 
			// APH_GoodsReceivedStatusBoundDropEdit
			// 
			this.APH_GoodsReceivedStatusBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APH_GoodsReceivedStatusBoundDropEdit, "APH_GoodsReceivedStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_GoodsReceivedStatus)));
			this.APH_GoodsReceivedStatusBoundDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7053b986-106f-43f7-a8ea-c8446f1fddf6", "Receival Status");
			this.APH_GoodsReceivedStatusBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 137, true);
			this.APH_GoodsReceivedStatusBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_GoodsReceivedStatusBoundDropEdit.Name = "APH_GoodsReceivedStatusBoundDropEdit";
			this.APH_GoodsReceivedStatusBoundDropEdit.PreBoundMaxLength = 3;
			this.APH_GoodsReceivedStatusBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.APH_GoodsReceivedStatusBoundDropEdit.TabIndex = 17;
			// 
			// APH_FollowupDateBoundDateEdit
			// 
			this.APH_FollowupDateBoundDateEdit.AllowDrop = true;
			this.APH_FollowupDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_FollowupDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_FollowupDateBoundDateEdit, "APH_FollowupDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_FollowupDate)));
			this.APH_FollowupDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0ffeac0e-71a9-40e4-9c56-10c59e223a0c", "Follow up");
			this.APH_FollowupDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 113, true);
			this.APH_FollowupDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_FollowupDateBoundDateEdit.Name = "APH_FollowupDateBoundDateEdit";
			this.APH_FollowupDateBoundDateEdit.TabIndex = 16;
			// 
			// APH_ExpectedDeliveryBoundDateEdit
			// 
			this.APH_ExpectedDeliveryBoundDateEdit.AllowDrop = true;
			this.APH_ExpectedDeliveryBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_ExpectedDeliveryBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_ExpectedDeliveryBoundDateEdit, "APH_ExpectedDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_ExpectedDelivery)));
			this.APH_ExpectedDeliveryBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a349c9a1-4f47-466f-82e7-78d519dfbee8", "Expected DLV");
			this.APH_ExpectedDeliveryBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 88, true);
			this.APH_ExpectedDeliveryBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_ExpectedDeliveryBoundDateEdit.Name = "APH_ExpectedDeliveryBoundDateEdit";
			this.APH_ExpectedDeliveryBoundDateEdit.TabIndex = 15;
			// 
			// APH_ReadyForDeliveryBoundDateEdit
			// 
			this.APH_ReadyForDeliveryBoundDateEdit.AllowDrop = true;
			this.APH_ReadyForDeliveryBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_ReadyForDeliveryBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_ReadyForDeliveryBoundDateEdit, "APH_ReadyForDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_ReadyForDelivery)));
			this.APH_ReadyForDeliveryBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("dbb219b2-0fec-4a18-a6b3-eb19bc44691a", "Ready for DLV");
			this.APH_ReadyForDeliveryBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 88, true);
			this.APH_ReadyForDeliveryBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_ReadyForDeliveryBoundDateEdit.Name = "APH_ReadyForDeliveryBoundDateEdit";
			this.APH_ReadyForDeliveryBoundDateEdit.TabIndex = 14;
			// 
			// APH_GoodsDescriptionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_GoodsDescriptionBoundTextBox, "APH_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_GoodsDescription)));
			this.APH_GoodsDescriptionBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f9000f53-0ee3-48f9-b7aa-4688c8642cc3", "Description");
			this.APH_GoodsDescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.APH_GoodsDescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 39, true);
			this.APH_GoodsDescriptionBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_GoodsDescriptionBoundTextBox.Name = "APH_GoodsDescriptionBoundTextBox";
			this.APH_GoodsDescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 18, true);
			this.APH_GoodsDescriptionBoundTextBox.TabIndex = 6;
			// 
			// APH_InvoiceDateBoundDateEdit
			// 
			this.APH_InvoiceDateBoundDateEdit.AllowDrop = true;
			this.APH_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_InvoiceDateBoundDateEdit, "APH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_InvoiceDate)));
			this.APH_InvoiceDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9d76d6b6-7f1a-415b-a5e3-a31dcb6e38c1", "Date");
			this.APH_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 162, true);
			this.APH_InvoiceDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_InvoiceDateBoundDateEdit.Name = "APH_InvoiceDateBoundDateEdit";
			this.APH_InvoiceDateBoundDateEdit.TabIndex = 10;
			// 
			// APH_BookingConfDateBoundDateEdit
			// 
			this.APH_BookingConfDateBoundDateEdit.AllowDrop = true;
			this.APH_BookingConfDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_BookingConfDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_BookingConfDateBoundDateEdit, "APH_BookingConfDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_BookingConfDate)));
			this.APH_BookingConfDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4eff699d-0118-45cf-9c86-4fd937cd0af1", "Date");
			this.APH_BookingConfDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 63, true);
			this.APH_BookingConfDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_BookingConfDateBoundDateEdit.Name = "APH_BookingConfDateBoundDateEdit";
			this.APH_BookingConfDateBoundDateEdit.TabIndex = 8;
			// 
			// APH_DueDateBoundDateEdit
			// 
			this.APH_DueDateBoundDateEdit.AllowDrop = true;
			this.APH_DueDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.APH_DueDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.APH_DueDateBoundDateEdit, "APH_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_DueDate)));
			this.APH_DueDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 187, true);
			this.APH_DueDateBoundDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_DueDateBoundDateEdit.Name = "APH_DueDateBoundDateEdit";
			this.APH_DueDateBoundDateEdit.TabIndex = 12;
			// 
			// APH_TypeBoundDropEdit
			// 
			this.APH_TypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APH_TypeBoundDropEdit, "APH_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Type)));
			this.APH_TypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 14, true);
			this.APH_TypeBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_TypeBoundDropEdit.Name = "APH_TypeBoundDropEdit";
			this.APH_TypeBoundDropEdit.PreBoundMaxLength = 3;
			this.APH_TypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.APH_TypeBoundDropEdit.TabIndex = 5;
			// 
			// APH_InvoiceNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_InvoiceNumberBoundTextBox, "APH_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_InvoiceNumber)));
			this.APH_InvoiceNumberBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("275386b5-8a44-4a32-b5ae-07da935116b2", "Invoice No");
			this.APH_InvoiceNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 162, true);
			this.APH_InvoiceNumberBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_InvoiceNumberBoundTextBox.Name = "APH_InvoiceNumberBoundTextBox";
			this.APH_InvoiceNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.APH_InvoiceNumberBoundTextBox.TabIndex = 9;
			// 
			// APH_BookingConfRefBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_BookingConfRefBoundTextBox, "APH_BookingConfRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_BookingConfRef)));
			this.APH_BookingConfRefBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b161bb33-64d8-4d8f-ae13-ed1ef49f92f8", "Confirmation No.");
			this.APH_BookingConfRefBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 63, true);
			this.APH_BookingConfRefBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_BookingConfRefBoundTextBox.Name = "APH_BookingConfRefBoundTextBox";
			this.APH_BookingConfRefBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 18, true);
			this.APH_BookingConfRefBoundTextBox.TabIndex = 7;
			// 
			// APH_OrderNumberSplitBoundTextBox
			// 
			this.APH_OrderNumberSplitBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.APH_OrderNumberSplitBoundTextBox, "APH_OrderNumberSplit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_OrderNumberSplit)));
			this.APH_OrderNumberSplitBoundTextBox.DecimalPlaces = 0;
			this.APH_OrderNumberSplitBoundTextBox.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.APH_OrderNumberSplitBoundTextBox, false);
			this.APH_OrderNumberSplitBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 17, true);
			this.APH_OrderNumberSplitBoundTextBox.Name = "APH_OrderNumberSplitBoundTextBox";
			this.APH_OrderNumberSplitBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.APH_OrderNumberSplitBoundTextBox.TabIndex = 29;
			this.APH_OrderNumberSplitBoundTextBox.Text = "0";
			this.APH_OrderNumberSplitBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// APH_DispositionBoundDropEdit
			// 
			this.APH_DispositionBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APH_DispositionBoundDropEdit, "APH_Disposition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Disposition)));
			this.APH_DispositionBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 68, true);
			this.APH_DispositionBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_DispositionBoundDropEdit.Name = "APH_DispositionBoundDropEdit";
			this.APH_DispositionBoundDropEdit.PreBoundMaxLength = 3;
			this.APH_DispositionBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.APH_DispositionBoundDropEdit.TabIndex = 4;
			// 
			// APH_StageBoundDropEdit
			// 
			this.APH_StageBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.APH_StageBoundDropEdit, "APH_Stage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_Stage)));
			this.APH_StageBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 42, true);
			this.APH_StageBoundDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_StageBoundDropEdit.Name = "APH_StageBoundDropEdit";
			this.APH_StageBoundDropEdit.PreBoundMaxLength = 3;
			this.APH_StageBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.APH_StageBoundDropEdit.TabIndex = 3;
			// 
			// APH_OrderNumberBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.APH_OrderNumberBoundTextBox, "APH_OrderNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).APH_OrderNumber)));
			this.APH_OrderNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 17, true);
			this.APH_OrderNumberBoundTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.APH_OrderNumberBoundTextBox.Name = "APH_OrderNumberBoundTextBox";
			this.APH_OrderNumberBoundTextBox.ReadOnly = true;
			this.APH_OrderNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 18, true);
			this.APH_OrderNumberBoundTextBox.TabIndex = 2;
			// 
			// SupplierDocumentaryDocAddressControl
			// 
			this.SupplierDocumentaryDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierDocumentaryDocAddressControl, "SupplierDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).SupplierDocumentaryAddress)));
			this.SupplierDocumentaryDocAddressControl.BindToOrganisations = "Lookups.Supplier_OrgList";
			this.SupplierDocumentaryDocAddressControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("27de2029-8965-4bf0-92f0-9558685144a9", "Supplier");
			this.SupplierDocumentaryDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 164, true);
			this.SupplierDocumentaryDocAddressControl.Name = "SupplierDocumentaryDocAddressControl";
			this.SupplierDocumentaryDocAddressControl.SingleLineNoGroupBoxPanelWidth = 370;
			this.SupplierDocumentaryDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.SupplierDocumentaryDocAddressControl.TabIndex = 1;
			// 
			// RightTabControl
			// 
			this.RightTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RightTabControl.Controls.Add(this.OrderSplitsTabPage);
			this.RightTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(628, 6, true);
			this.RightTabControl.Name = "RightTabControl";
			this.RightTabControl.SelectedIndex = 0;
			this.RightTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 314, true);
			this.RightTabControl.TabIndex = 18;
			// 
			// OrderSplitsTabPage
			// 
			this.OrderSplitsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("35d4e9f8-089d-46ce-b400-6c8fe55559e5", "Order Splits");
			this.OrderSplitsTabPage.Controls.Add(this.OrderSplitsButtonGrid);
			this.OrderSplitsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.OrderSplitsTabPage.Name = "OrderSplitsTabPage";
			this.OrderSplitsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OrderSplitsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 290, true);
			this.OrderSplitsTabPage.TabIndex = 1;
			this.OrderSplitsTabPage.UseVisualStyleBackColor = true;
			// 
			// OrderSplitsButtonGrid
			// 
			this.OrderSplitsButtonGrid.AllowDrop = true;
			this.OrderSplitsButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
			this.BindingSource.SetBindingMember(this.OrderSplitsButtonGrid, "OrderSplitSiblings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderSplitSiblings)));
			this.OrderSplitsButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderSplitsButtonGrid.GridId = "B204F9FC-E401-49E3-84E4-F7B102984586";
			// 
			// 
			// 
			this.OrderSplitsButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrderSplitsButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OrderSplitsButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrderSplitsButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.OrderSplitsButtonGrid.InnerGrid.GridId = null;
			this.OrderSplitsButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderSplitsButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OrderSplitsButtonGrid.InnerGrid.Name = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.ReadOnly = true;
			this.OrderSplitsButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 254, true);
			this.OrderSplitsButtonGrid.InnerGrid.TabIndex = 0;
			this.OrderSplitsButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrderSplitsButtonGrid.Name = "OrderSplitsButtonGrid";
			this.OrderSplitsButtonGrid.NameOfAGridElement = Enterprise.Accounting.GUI.Res.GetData("19c8f1ab-d140-44c7-89f3-1880ce4386c2", "Order Line");
			this.OrderSplitsButtonGrid.Order = null;
			this.OrderSplitsButtonGrid.ReadOnly = true;
			this.OrderSplitsButtonGrid.ShowAttachButton = false;
			this.OrderSplitsButtonGrid.ShowDetachButton = false;
			this.OrderSplitsButtonGrid.ShowEditButton = false;
			this.OrderSplitsButtonGrid.ShowNewButton = false;
			this.OrderSplitsButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 284, true);
			this.OrderSplitsButtonGrid.TabIndex = 18;
			// 
			// OrderLinesTab
			// 
			this.OrderLinesTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PayableOrdersUserControl|195cb104-edf1-420c-92f5-6ee5d2319eb5", "Order Lines");
			this.OrderLinesTab.Controls.Add(this.orderLinesControl);
			this.OrderLinesTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.OrderLinesTab.Name = "OrderLinesTab";
			this.OrderLinesTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 91, true);
			this.OrderLinesTab.TabIndex = 0;
			// 
			// orderLinesControl
			// 
			this.orderLinesControl.AllowDrop = true;
			this.orderLinesControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));			
			this.orderLinesControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.orderLinesControl, ".");
			this.orderLinesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.orderLinesControl.Name = "orderLinesControl";
			this.orderLinesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(982, 89, true);
			this.orderLinesControl.TabIndex = 19;
			// 
			// BottomTabControl
			// 
			this.BottomTabControl.Controls.Add(this.OrderLinesTab);
			this.BottomTabControl.Controls.Add(this.ProductSummaryTab);
			this.BottomTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 24, true);
			this.BottomTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 347, true);
			this.BottomTabControl.Name = "BottomTabControl";
			this.BottomTabControl.SelectedIndex = 0;
			this.BottomTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 117, true);
			this.BottomTabControl.TabIndex = 20;
			this.BottomTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.BottomTabControl_Selecting);
			// 
			// ProductSummaryTab
			// 
			this.ProductSummaryTab.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1dff113c-236a-4739-a240-d0a2ff7cea48", "Product Quantity Summary");
			this.ProductSummaryTab.Controls.Add(this.orderLinesTotalByProduct);
			this.ProductSummaryTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.ProductSummaryTab.Name = "ProductSummaryTab";
			this.ProductSummaryTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ProductSummaryTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 91, true);
			this.ProductSummaryTab.TabIndex = 1;
			// 
			// orderLinesTotalByProduct
			// 
			this.orderLinesTotalByProduct.AllowDrop = true;
			this.orderLinesTotalByProduct.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.orderLinesTotalByProduct.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.orderLinesTotalByProduct, ".");
			this.orderLinesTotalByProduct.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.orderLinesTotalByProduct.Name = "orderLinesTotalByProduct";
			this.orderLinesTotalByProduct.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 86, true);
			this.orderLinesTotalByProduct.TabIndex = 0;
			// 
			// OrderStatusGroupBox
			// 
			this.OrderStatusGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0e9924a6-101b-489f-ad32-6254a56f7804", "Status");
			this.OrderStatusGroupBox.Controls.Add(this.DispositionExplainButton);
			this.OrderStatusGroupBox.Controls.Add(this.APH_OrderNumberBoundTextBox);
			this.OrderStatusGroupBox.Controls.Add(this.APH_OrderNumberSplitBoundTextBox);
			this.OrderStatusGroupBox.Controls.Add(this.APH_StageBoundDropEdit);
			this.OrderStatusGroupBox.Controls.Add(this.APH_DispositionBoundDropEdit);
			this.OrderStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 6, true);
			this.OrderStatusGroupBox.Name = "OrderStatusGroupBox";
			this.OrderStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 99, true);
			this.OrderStatusGroupBox.TabIndex = 2;
			this.OrderStatusGroupBox.TabStop = false;
			// 
			// DispositionExplainButton
			// 
			this.DispositionExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 67, true);
			this.DispositionExplainButton.Name = "DispositionExplainButton";
			this.DispositionExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.DispositionExplainButton.TabIndex = 30;
			this.DispositionExplainButton.Text = "...";
			this.DispositionExplainButton.Click += new System.EventHandler(this.DispositionExplainButton_Click);
			// 
			// AddressWithContactControl
			// 
			this.AddressWithContactControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressWithContactControl, "OrderedByAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ZAddressWithContact)(((Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeader)(null)).OrderedByAddressWithContact)));
			this.AddressWithContactControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("76d62ef4-4d27-4633-a6e9-2e5e937cf571", "Ordered By");
			this.AddressWithContactControl.ContactInfoTabVisible = true;
			this.AddressWithContactControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.AddressWithContactControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.Name = "AddressWithContactControl";
			this.AddressWithContactControl.OnlyStopOnDebtor = false;
			this.AddressWithContactControl.PopupCaption = "";
			this.AddressWithContactControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AddressWithContactControl.TabIndex = 21;
			// 
			// AccPayableOrderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressWithContactControl);
			this.Controls.Add(this.OrderStatusGroupBox);
			this.Controls.Add(this.RightTabControl);
			this.Controls.Add(this.SupplierDocumentaryDocAddressControl);
			this.Controls.Add(this.OrderDetailsGroupBox);
			this.Controls.Add(this.BottomTabControl);
			this.Name = "AccPayableOrderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrderDetailsGroupBox.ResumeLayout(false);
			this.OrderDetailsGroupBox.PerformLayout();
			this.APH_RXBoundCurrency.ResumeLayout(true);
			this.APH_RXBoundCurrency.PerformLayout();
			this.APH_GoodsReceivedStatusBoundDropEdit.ResumeLayout(true);
			this.APH_GoodsReceivedStatusBoundDropEdit.PerformLayout();
			this.APH_FollowupDateBoundDateEdit.ResumeLayout(true);
			this.APH_FollowupDateBoundDateEdit.PerformLayout();
			this.APH_ExpectedDeliveryBoundDateEdit.ResumeLayout(true);
			this.APH_ExpectedDeliveryBoundDateEdit.PerformLayout();
			this.APH_ReadyForDeliveryBoundDateEdit.ResumeLayout(true);
			this.APH_ReadyForDeliveryBoundDateEdit.PerformLayout();
			this.APH_InvoiceDateBoundDateEdit.ResumeLayout(true);
			this.APH_InvoiceDateBoundDateEdit.PerformLayout();
			this.APH_BookingConfDateBoundDateEdit.ResumeLayout(true);
			this.APH_BookingConfDateBoundDateEdit.PerformLayout();
			this.APH_DueDateBoundDateEdit.ResumeLayout(true);
			this.APH_DueDateBoundDateEdit.PerformLayout();
			this.APH_TypeBoundDropEdit.ResumeLayout(true);
			this.APH_TypeBoundDropEdit.PerformLayout();
			this.APH_DispositionBoundDropEdit.ResumeLayout(true);
			this.APH_DispositionBoundDropEdit.PerformLayout();
			this.APH_StageBoundDropEdit.ResumeLayout(true);
			this.APH_StageBoundDropEdit.PerformLayout();
			this.SupplierDocumentaryDocAddressControl.ResumeLayout(true);
			this.SupplierDocumentaryDocAddressControl.PerformLayout();
			this.RightTabControl.ResumeLayout(false);
			this.RightTabControl.PerformLayout();
			this.OrderSplitsTabPage.ResumeLayout(false);
			this.OrderSplitsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).EndInit();			
			this.OrderSplitsButtonGrid.ResumeLayout(true);
			this.OrderSplitsButtonGrid.PerformLayout();
			this.OrderLinesTab.ResumeLayout(false);
			this.OrderLinesTab.PerformLayout();
			this.orderLinesControl.ResumeLayout(true);
			this.orderLinesControl.PerformLayout();
			this.BottomTabControl.ResumeLayout(false);
			this.BottomTabControl.PerformLayout();
			this.ProductSummaryTab.ResumeLayout(false);
			this.ProductSummaryTab.PerformLayout();
			this.orderLinesTotalByProduct.ResumeLayout(true);
			this.orderLinesTotalByProduct.PerformLayout();
			this.OrderStatusGroupBox.ResumeLayout(false);
			this.OrderStatusGroupBox.PerformLayout();
			this.AddressWithContactControl.ResumeLayout(true);
			this.AddressWithContactControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZGroupBox OrderDetailsGroupBox;
		private ZArchitecture.ZTextBox APH_OrderNumberBoundTextBox;
		private ZArchitecture.ZTextBox APH_BookingConfRefBoundTextBox;
		private ZArchitecture.ZTextBox APH_InvoiceNumberBoundTextBox;
		private ZDropEdit APH_DispositionBoundDropEdit;
		private ZDropEdit APH_StageBoundDropEdit;
		private ZDropEdit APH_TypeBoundDropEdit;
		private ZDateEdit APH_InvoiceDateBoundDateEdit;
		private ZDateEdit APH_BookingConfDateBoundDateEdit;
		private ZDateEdit APH_DueDateBoundDateEdit;
		private ZArchitecture.ZTextBox APH_GoodsDescriptionBoundTextBox;
		private ZDateEdit APH_FollowupDateBoundDateEdit;
		private ZDateEdit APH_ExpectedDeliveryBoundDateEdit;
		private ZDateEdit APH_ReadyForDeliveryBoundDateEdit;
		private ZDropEdit APH_GoodsReceivedStatusBoundDropEdit;
		internal ZDocAddressControl SupplierDocumentaryDocAddressControl;
		protected ZTabControl RightTabControl;
		private ZTabPage OrderSplitsTabPage;
		protected ZTabPage OrderLinesTab;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl BottomTabControl;
		protected OrderSplitsButtonGrid OrderSplitsButtonGrid;
		private AccPayableOrderLinesControl orderLinesControl;
		protected ZExchangeRateControl APH_RXBoundCurrency;
		private ZArchitecture.ZCalcEdit APH_OrderNumberSplitBoundTextBox;
		public ZCheckBox GSTInclusiveCheckBox;
		protected ZGroupBox OrderStatusGroupBox;
		private ZTabPage ProductSummaryTab;
		private AccPayableOrderLinesTotalByProductControl orderLinesTotalByProduct;
		private ZButton DispositionExplainButton;
		public MasterFiles.GUI.Organisation.UserControls.Address.ZAddressWithContactControl AddressWithContactControl;
		
	}
}

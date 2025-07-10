using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class LVSSubHeadersUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.SubHeaderAllDetailsUserControl = new Enterprise.Customs.CA.GUI.LVSSubHeaderAllDetailsUserControl();
			this.LVSSubHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LVSSubHeadersGrid = new Enterprise.Customs.CA.GUI.LVSSubHeadersGrid();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubHeaderAllDetailsUserControl.SuspendLayout();
			this.LVSSubHeadersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LVSSubHeadersGrid)).BeginInit();
			this.LVSSubHeadersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// SubHeaderAllDetailsUserControl
			// 
			this.SubHeaderAllDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubHeaderAllDetailsUserControl, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)))));
			this.SubHeaderAllDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubHeaderAllDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubHeaderAllDetailsUserControl.Name = "SubHeaderAllDetailsUserControl";
			this.SubHeaderAllDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 202, true);
			this.SubHeaderAllDetailsUserControl.TabIndex = 0;
			// 
			// LVSSubHeadersGroupBox
			// 
			this.LVSSubHeadersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|181fe3ec-675c-4d1f-9955-77a96c1f3f67", "LVS Shipments");
			this.LVSSubHeadersGroupBox.Controls.Add(this.LVSSubHeadersGrid);
			this.LVSSubHeadersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSubHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LVSSubHeadersGroupBox.Name = "LVSSubHeadersGroupBox";
			this.LVSSubHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 53, true);
			this.LVSSubHeadersGroupBox.TabIndex = 0;
			this.LVSSubHeadersGroupBox.TabStop = false;
			// 
			// LVSSubHeadersGrid
			// 
			this.LVSSubHeadersGrid.AllowDrop = true;
			this.LVSSubHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LVSSubHeadersGrid, "Invoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Buyer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RX_NKInvoice_Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceCurrExRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).InvoiceLineTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_BalanceString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_ValuationDateOverride)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RN_NKDefaultOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RW_NKOriginState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_RN_NKExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_USStateOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_TreatmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_PortOfClearance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_TimeLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_TimeLimitCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_ValueForDutyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_OtherReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_FOBAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_FOBCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_Calc_TNI)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_RX_Calc_TNICurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_LVSLastPrintDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).JZ_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).CA_CarrierCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).SupplierDocumentaryAddress.E2_OA_Address)));
			this.LVSSubHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|24cfa501-5290-487e-bd9c-e82c335e27a5", "LVS ID/Inv. #");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JZ_InvoiceNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = "LVS ID/Inv. #";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JZ_OH_Buyer";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Importer";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zOrganisationFindBoxColumnStyleInfo3.BindToList = "Lookups.SupplierList";
			zOrganisationFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|22db1051-a9ce-44bb-a6ca-79560f422159", "Vendor");
			zOrganisationFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("0752670e-9559-4a99-981a-6bbe17acdcdd", "Vendor");
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "SupplierDocumentaryAddress+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo3.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo3.ToolTip = "Vendor";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JZ_InvoiceAmount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|c7d08bcd-e5b6-4741-92b1-f70934e33144", "Invoice Amount");
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ToolTip = "Invoice Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|dba33938-6f17-4ca5-b13e-7ac2e1aae84d", "Curr", "Invoice Currency", "The total amount of the invoice and its currency.");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JZ_RX_NKInvoice_Currency";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|c7d08bcd-e5b6-4741-92b1-f70934e33144", "Invoice Amount");
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Invoice Currency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JZ_InvoiceCurrExRate";
			zCalcEditColumnStyleInfo2.ToolTip = "Exchange Rate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|ffebe3e6-d68c-4d39-ab92-522f364972ab", "Line Total", "Expected Line Total", "Expected Invoice Line Total", "Expected Invoice Line Total. This is the Invoice Total less charges that are not included in the invoice line amounts.");
			zCalcEditColumnStyleInfo3.ColumnName = "InvoiceLineTotal";
			zCalcEditColumnStyleInfo3.ToolTip = "Invoice Line Total";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|0bb38ac9-6204-444a-aa83-7fb137bb6034", "Balance", "The balance to be entered in the invoice lines for this commercial invoice. It is the difference between the Invoice Total Amount entered for this commercial invoice and the total of all related invoice lines entered for that commercial invoice.");
			zTextBoxColumnStyleInfo2.ColumnName = "JZ_Calc_BalanceString";
			zTextBoxColumnStyleInfo2.ToolTip = "Balance";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|fb139e92-af16-495a-851f-e8f32850d547", "Ship. Date", "Direct Ship. Date", "Direct Shipment Date", "The Date of Direct Shipment of this invoice. Defaults to the Date of Export (ATD) on the declaration tab.");
			zDateEditColumnStyleInfo1.ColumnName = "JZ_ValuationDateOverride";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.ToolTip = "Direct Shipment Date";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|d79ab38a-af91-430e-8d4d-d18515802ad8", "ORG", "Origin", "Country/Region Of Origin", "The country/region of origin of the goods.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JZ_RN_NKDefaultOrigin";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|03ad15ee-649e-4308-8bb3-7f96820cdcee", "Country/Region Of Origin");
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Country/Region of Origin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|2245c68d-1173-4834-a18c-f5f253d444b4", "ST", "State", "US State of Origin", "The US state or origin when the Country/Region of Origin is US.");
			zDropEditColumnStyleInfo1.ColumnName = "JZ_RW_NKOriginState";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|03ad15ee-649e-4308-8bb3-7f96820cdcee", "Country/Region Of Origin");
			zDropEditColumnStyleInfo1.ToolTip = "US State of Origin";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CA_RN_NKExport";
			zCodeFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|fa1d064b-39bc-4385-9a18-5ada2b6e6240", "Country/Region Of Export");
			zCodeFindBoxColumnStyleInfo3.ToolTip = "Country/Region of Export";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|6208b46c-1d9d-41b1-91a4-80798739c432", "ST", "State", "US State of Export", "The US state code if the Country/Region of Export is US.");
			zDropEditColumnStyleInfo2.ColumnName = "CA_USStateOfExport";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|fa1d064b-39bc-4385-9a18-5ada2b6e6240", "Country/Region Of Export");
			zDropEditColumnStyleInfo2.ToolTip = "US State of Export";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo3.ColumnName = "CA_TreatmentCode";
			zDropEditColumnStyleInfo3.ToolTip = "Tariff Treatment Code";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo4.ColumnName = "JZ_IncoTerm";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.ToolTip = "Incoterm";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CA_PortOfClearance";
			zCodeFindBoxColumnStyleInfo4.ToolTip = "Port Of Clearance";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CA_TimeLimit";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|c3156210-de95-4821-baa3-89f076057067", "Time Limit");
			zCalcEditColumnStyleInfo4.ToolTip = "Time Limit";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo5.ColumnName = "CA_TimeLimitCode";
			zDropEditColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|c3156210-de95-4821-baa3-89f076057067", "Time Limit");
			zDropEditColumnStyleInfo5.ToolTip = "Time Limit Unit";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CA_OtherReference";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.ToolTip = "Other Reference";
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JZ_Calc_FOBAmount";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|ae360920-c275-4245-a840-221537c416df", "FOB");
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.ToolTip = "FOB Amount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JZ_Calc_FOBCurrency";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|ae360920-c275-4245-a840-221537c416df", "FOB");
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.ToolTip = "FOB Currency";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JZ_Calc_TNI";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|5bad05b2-62f9-460a-9f41-6cc7a8693ab7", "Overseas Freight & Insurance");
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.ToolTip = "Overseas Freight & Insurance";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "JZ_RX_Calc_TNICurrency";
			zGuidFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|5bad05b2-62f9-460a-9f41-6cc7a8693ab7", "Overseas Freight & Insurance");
			zGuidFindBoxColumnStyleInfo3.IsVisible = false;
			zGuidFindBoxColumnStyleInfo3.ToolTip = "Overseas Freight & Insurance Currency";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|676CFCF3-F0EE-444D-8ED2-0146F4AEC313", "Date Last Printed");
			zDateEditColumnStyleInfo2.ColumnName = "CA_LVSLastPrintDate";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|5B9885C9-6B00-47EF-89DE-49534334A443", "Invoice Date");
			zDateEditColumnStyleInfo3.ColumnName = "JZ_InvoiceDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.ToolTip = "Invoice Date";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|85734B5B-605F-47A7-BBE6-413A744AC316", "Release Date");
			zDateEditColumnStyleInfo4.ColumnName = "JobDeclaration.JE_EntryAuthorisationDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.ToolTip = "Release Date";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSSubHeadersUserControl|77CA3D6A-4DD6-4575-B6AE-365687B77C33", "Carrier Code");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "CA_CarrierCode";
			zCodeFindBoxColumnStyleInfo5.ToolTip = "Carrier Code";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8e961820-c60d-4d74-b8f3-5e1f05698421", "Vendor Address");
			zAddressDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("0752670e-9559-4a99-981a-6bbe17acdcdd", "Vendor");
			zAddressDropEditColumnStyleInfo1.ColumnName = "SupplierDocumentaryAddress+E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.LVSSubHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.LVSSubHeadersGrid.CopySelectedRowsAllowed = true;
			this.LVSSubHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LVSSubHeadersGrid.GridId = "8cedf60b-b67d-4b0d-ae8f-cab4ee371c35";
			this.LVSSubHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LVSSubHeadersGrid.LayoutKey = "LVSSubHeadersGrid";
			this.LVSSubHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LVSSubHeadersGrid.Name = "LVSSubHeadersGrid";
			this.LVSSubHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 34, true);
			this.LVSSubHeadersGrid.TabIndex = 0;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.LVSSubHeadersGroupBox);
			this.MainSplitContainer.Panel1MinSize = 51;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.SubHeaderAllDetailsUserControl);
			this.MainSplitContainer.Panel2MinSize = 202;
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 232, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			this.MainSplitContainer.SplitterWidth = 2;
			this.MainSplitContainer.TabIndex = 3;
			// 
			// LVSSubHeadersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "LVSSubHeadersUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 232, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubHeaderAllDetailsUserControl.ResumeLayout(true);
			this.SubHeaderAllDetailsUserControl.PerformLayout();
			this.LVSSubHeadersGroupBox.ResumeLayout(false);
			this.LVSSubHeadersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LVSSubHeadersGrid)).EndInit();
			this.LVSSubHeadersGrid.ResumeLayout(false);
			this.LVSSubHeadersGrid.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private LVSSubHeaderAllDetailsUserControl SubHeaderAllDetailsUserControl;
		private ZArchitecture.GUI.ZGroupBox LVSSubHeadersGroupBox;
		public Enterprise.Customs.CA.GUI.LVSSubHeadersGrid LVSSubHeadersGrid;
		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
	}
}

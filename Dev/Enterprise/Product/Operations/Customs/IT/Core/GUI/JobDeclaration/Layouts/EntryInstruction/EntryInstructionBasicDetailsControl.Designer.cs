using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using CargoWise.Windows.UI;
using System.ComponentModel;
using System.Windows.Forms;

namespace Enterprise.Customs.IT.GUI
{
	partial class EntryInstructionBasicDetailsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PreviousInvoiceAmountBoundCurrencyUserControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.PreviousInvoiceCurrencyExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FinancialAndBankingDataLine1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FinancialAndBankingDataLine2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BoxElectronicDocumentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseDeclarationOfIntentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClearanceByEntryLineCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SimplifiedDecAcceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValuationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IncotermTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TempProcLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ParticipantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OtherCustomsInformationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssessmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FinancialAndBankingDataLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PreviousInvoiceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PresentationOfGoodsDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.FromWarehouseUserControl = new Enterprise.Customs.IT.GUI.FromWarehouseUserControl();
			this.ToWarehouseUserControl = new Enterprise.Customs.IT.GUI.ToWarehouseUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreviousInvoiceAmountBoundCurrencyUserControl.SuspendLayout();
			this.SimplifiedDecAcceptanceDateEdit.SuspendLayout();
			this.TempProcLimitDateEdit.SuspendLayout();
			this.ProcedureCodeDropEdit.SuspendLayout();
			this.ParticipantTypeDropEdit.SuspendLayout();
			this.AssessmentDateEdit.SuspendLayout();
			this.PresentationOfGoodsDateEdit.SuspendLayout();
			this.FromWarehouseUserControl.SuspendLayout();
			this.ToWarehouseUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction);
			// 
			// PreviousInvoiceAmountBoundCurrencyUserControl
			// 
			this.PreviousInvoiceAmountBoundCurrencyUserControl.AllowDrop = true;
			this.PreviousInvoiceAmountBoundCurrencyUserControl.BindToAmount = "ZG_PreviousInvoiceAmount";
			this.PreviousInvoiceAmountBoundCurrencyUserControl.BindToList = "AddInfoLookups.CurrencyList";
			this.PreviousInvoiceAmountBoundCurrencyUserControl.BindToUnit = "ZG_PreviousInvoiceCurrency";
			this.PreviousInvoiceAmountBoundCurrencyUserControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PreviousInvoiceAmountBoundCurrencyUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 578, true);
			this.PreviousInvoiceAmountBoundCurrencyUserControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.PreviousInvoiceAmountBoundCurrencyUserControl.Name = "PreviousInvoiceAmountBoundCurrencyUserControl";
			this.PreviousInvoiceAmountBoundCurrencyUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.PreviousInvoiceAmountBoundCurrencyUserControl.TabIndex = 0;
			// 
			// PreviousInvoiceCurrencyExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousInvoiceCurrencyExRateCalcEdit, "PreviousInvoiceCurrencyExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).PreviousInvoiceCurrencyExRate)));
			this.PreviousInvoiceCurrencyExRateCalcEdit.CaptionResourceString = null;
			this.PreviousInvoiceCurrencyExRateCalcEdit.DecimalPlaces = 2;
			this.PreviousInvoiceCurrencyExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 330, true);
			this.PreviousInvoiceCurrencyExRateCalcEdit.Name = "PreviousInvoiceCurrencyExRateCalcEdit";
			this.PreviousInvoiceCurrencyExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PreviousInvoiceCurrencyExRateCalcEdit.TabIndex = 1;
			this.PreviousInvoiceCurrencyExRateCalcEdit.Text = "0.000000";
			this.PreviousInvoiceCurrencyExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FinancialAndBankingDataLine1TextBox
			// 
			this.BindingSource.SetBindingMember(this.FinancialAndBankingDataLine1TextBox, "FinancialAndBankingDataLine1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).FinancialAndBankingDataLine1)));
			this.FinancialAndBankingDataLine1TextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("6E07BC2B-D7B1-459D-8BAD-08362A084B72", "Line 1");
			this.FinancialAndBankingDataLine1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 422, true);
			this.FinancialAndBankingDataLine1TextBox.Name = "FinancialAndBankingDataLine1TextBox";
			this.FinancialAndBankingDataLine1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 20, true);
			this.FinancialAndBankingDataLine1TextBox.TabIndex = 0;
			// 
			// FinancialAndBankingDataLine2TextBox
			// 
			this.BindingSource.SetBindingMember(this.FinancialAndBankingDataLine2TextBox, "FinancialAndBankingDataLine2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).FinancialAndBankingDataLine2)));
			this.FinancialAndBankingDataLine2TextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("69584B0E-9529-4CF0-912A-BC31A6E0F02D", "Line 2");
			this.FinancialAndBankingDataLine2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 448, true);
			this.FinancialAndBankingDataLine2TextBox.Name = "FinancialAndBankingDataLine2TextBox";
			this.FinancialAndBankingDataLine2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 20, true);
			this.FinancialAndBankingDataLine2TextBox.TabIndex = 1;
			// 
			// BoxElectronicDocumentsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BoxElectronicDocumentsCheckBox, "ElectronicDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ElectronicDocuments)));
			this.BoxElectronicDocumentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 373, true);
			this.BoxElectronicDocumentsCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 0, 0, 0, true);
			this.BoxElectronicDocumentsCheckBox.Name = "BoxElectronicDocumentsCheckBox";
			this.BoxElectronicDocumentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.BoxElectronicDocumentsCheckBox.TabIndex = 0;
			this.BoxElectronicDocumentsCheckBox.UseVisualStyleBackColor = true;
			// 
			// UseDeclarationOfIntentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseDeclarationOfIntentCheckBox, "ZG_UseDeclarationOfIntent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_UseDeclarationOfIntent)));
			this.UseDeclarationOfIntentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 353, true);
			this.UseDeclarationOfIntentCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 0, 0, 0, true);
			this.UseDeclarationOfIntentCheckBox.Name = "UseDeclarationOfIntentCheckBox";
			this.UseDeclarationOfIntentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.UseDeclarationOfIntentCheckBox.TabIndex = 1;
			this.UseDeclarationOfIntentCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClearanceByEntryLineCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ClearanceByEntryLineCheckBox, "ClearanceByEntryLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ClearanceByEntryLine)));
			this.ClearanceByEntryLineCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 277, true);
			this.ClearanceByEntryLineCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, 0, 0, 0, true);
			this.ClearanceByEntryLineCheckBox.Name = "ClearanceByEntryLineCheckBox";
			this.ClearanceByEntryLineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			this.ClearanceByEntryLineCheckBox.TabIndex = 2;
			// 
			// SimplifiedDecAcceptanceDateEdit
			// 
			this.SimplifiedDecAcceptanceDateEdit.AllowDrop = true;
			this.SimplifiedDecAcceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SimplifiedDecAcceptanceDateEdit, "ZG_SimplifiedDecAcceptanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_SimplifiedDecAcceptanceDate)));
			this.SimplifiedDecAcceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 500, true);
			this.SimplifiedDecAcceptanceDateEdit.Name = "SimplifiedDecAcceptanceDateEdit";
			this.SimplifiedDecAcceptanceDateEdit.TabIndex = 7;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).Currency)));
			this.CurrencyTextBox.CaptionResourceString = null;
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 202, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.CurrencyTextBox.TabIndex = 6;
			// 
			// ValuationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValuationCodeTextBox, "ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ValuationCode)));
			this.ValuationCodeTextBox.CaptionResourceString = null;
			this.ValuationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 228, true);
			this.ValuationCodeTextBox.Name = "ValuationCodeTextBox";
			this.ValuationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.ValuationCodeTextBox.TabIndex = 5;
			// 
			// IncotermTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncotermTextBox, "Incoterm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).Incoterm)));
			this.IncotermTextBox.CaptionResourceString = null;
			this.IncotermTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 254, true);
			this.IncotermTextBox.Name = "IncotermTextBox";
			this.IncotermTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.IncotermTextBox.TabIndex = 4;
			// 
			// TempProcLimitDateEdit
			// 
			this.TempProcLimitDateEdit.AllowDrop = true;
			this.TempProcLimitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TempProcLimitDateEdit, "ZG_TempProcLimitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_TempProcLimitDate)));
			this.TempProcLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 474, true);
			this.TempProcLimitDateEdit.Name = "TempProcLimitDateEdit";
			this.TempProcLimitDateEdit.TabIndex = 3;
			// 
			// ProcedureCodeDropEdit
			// 
			this.ProcedureCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureCodeDropEdit, "CEI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).CEI_Procedure)));
			this.ProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 396, true);
			this.ProcedureCodeDropEdit.Name = "ProcedureCodeDropEdit";
			this.ProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ProcedureCodeDropEdit.TabIndex = 2;
			// 
			// ParticipantTypeDropEdit
			// 
			this.ParticipantTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParticipantTypeDropEdit, "ZG_ParticipantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_ParticipantType)));
			this.ParticipantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 552, true);
			this.ParticipantTypeDropEdit.Name = "ParticipantTypeDropEdit";
			this.ParticipantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ParticipantTypeDropEdit.TabIndex = 0;
			// 
			// OtherCustomsInformationLabel
			// 
			this.OtherCustomsInformationLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("29FDBA27-A8C9-47D2-8E5C-91CF8171EC36", "Other Customs Information");
			this.OtherCustomsInformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OtherCustomsInformationLabel.IsFontBold = true;
			this.OtherCustomsInformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 169, true);
			this.OtherCustomsInformationLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.OtherCustomsInformationLabel.Name = "OtherCustomsInformationLabel";
			this.OtherCustomsInformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OtherCustomsInformationLabel.TabIndex = 0;
			// 
			// ToWarehouseLabel
			// 
			this.ToWarehouseLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("78AED231-4738-4400-9F32-5D8F3F376DAC", "[49] To Warehouse");
			this.ToWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ToWarehouseLabel.IsFontBold = true;
			this.ToWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 151, true);
			this.ToWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.ToWarehouseLabel.Name = "ToWarehouseLabel";
			this.ToWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ToWarehouseLabel.TabIndex = 0;
			// 
			// FromWarehouseLabel
			// 
			this.FromWarehouseLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("86dc6757-1fac-40eb-8f7a-749b99f09924", "[49] From Warehouse");
			this.FromWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FromWarehouseLabel.IsFontBold = true;
			this.FromWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 130, true);
			this.FromWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.FromWarehouseLabel.Name = "FromWarehouseLabel";
			this.FromWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.FromWarehouseLabel.TabIndex = 0;
			// 
			// AssessmentDateEdit
			// 
			this.AssessmentDateEdit.AllowDrop = true;
			this.AssessmentDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AssessmentDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.AssessmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 304, true);
			this.AssessmentDateEdit.Name = "AssessmentDateEdit";
			this.AssessmentDateEdit.TabIndex = 1;
			// 
			// FinancialAndBankingDataLabel
			// 
			this.FinancialAndBankingDataLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("61E84D0F-38E8-478D-8D69-05C10546BE7F", "[28] Financial and Banking data");
			this.FinancialAndBankingDataLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FinancialAndBankingDataLabel.IsFontBold = true;
			this.FinancialAndBankingDataLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 110, true);
			this.FinancialAndBankingDataLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.FinancialAndBankingDataLabel.Name = "FinancialAndBankingDataLabel";
			this.FinancialAndBankingDataLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.FinancialAndBankingDataLabel.TabIndex = 0;
			// 
			// PreviousInvoiceLabel
			// 
			this.PreviousInvoiceLabel.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("335520CA-830C-4482-A267-364C18DB996C", "[44.3] Previous Invoice");
			this.PreviousInvoiceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PreviousInvoiceLabel.IsFontBold = true;
			this.PreviousInvoiceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 91, true);
			this.PreviousInvoiceLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.PreviousInvoiceLabel.Name = "PreviousInvoiceLabel";
			this.PreviousInvoiceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.PreviousInvoiceLabel.TabIndex = 0;
			// 
			// PresentationOfGoodsDateEdit
			// 
			this.PresentationOfGoodsDateEdit.AllowDrop = true;
			this.PresentationOfGoodsDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationOfGoodsDateEdit, "ZG_PresentationStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.CusEntryInstruction)(null)).ZG_PresentationStartDate)));
			this.PresentationOfGoodsDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationOfGoodsDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 526, true);
			this.PresentationOfGoodsDateEdit.Name = "PresentationOfGoodsDateEdit";
			this.PresentationOfGoodsDateEdit.TabIndex = 8;
			// 
			// FromWarehouseUserControl
			// 
			this.FromWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseUserControl, ".");
			this.FromWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 67, true);
			this.FromWarehouseUserControl.Name = "FromWarehouseUserControl";
			this.FromWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 21, true);
			this.FromWarehouseUserControl.TabIndex = 0;
			// 
			// ToWarehouseUserControl
			// 
			this.ToWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseUserControl, ".");
			this.ToWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 37, true);
			this.ToWarehouseUserControl.Name = "ToWarehouseUserControl";
			this.ToWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 24, true);
			this.ToWarehouseUserControl.TabIndex = 0;
			// 
			// EntryInstructionBasicDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ToWarehouseUserControl);
			this.Controls.Add(this.FromWarehouseUserControl);
			this.Controls.Add(this.PreviousInvoiceAmountBoundCurrencyUserControl);
			this.Controls.Add(this.PreviousInvoiceCurrencyExRateCalcEdit);
			this.Controls.Add(this.FinancialAndBankingDataLine1TextBox);
			this.Controls.Add(this.FinancialAndBankingDataLine2TextBox);
			this.Controls.Add(this.BoxElectronicDocumentsCheckBox);
			this.Controls.Add(this.UseDeclarationOfIntentCheckBox);
			this.Controls.Add(this.ClearanceByEntryLineCheckBox);
			this.Controls.Add(this.SimplifiedDecAcceptanceDateEdit);
			this.Controls.Add(this.CurrencyTextBox);
			this.Controls.Add(this.ValuationCodeTextBox);
			this.Controls.Add(this.IncotermTextBox);
			this.Controls.Add(this.TempProcLimitDateEdit);
			this.Controls.Add(this.ProcedureCodeDropEdit);
			this.Controls.Add(this.ParticipantTypeDropEdit);
			this.Controls.Add(this.OtherCustomsInformationLabel);
			this.Controls.Add(this.ToWarehouseLabel);
			this.Controls.Add(this.FromWarehouseLabel);
			this.Controls.Add(this.AssessmentDateEdit);
			this.Controls.Add(this.FinancialAndBankingDataLabel);
			this.Controls.Add(this.PreviousInvoiceLabel);
			this.Controls.Add(this.PresentationOfGoodsDateEdit);
			this.Name = "EntryInstructionBasicDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 607, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousInvoiceAmountBoundCurrencyUserControl.ResumeLayout(true);
			this.PreviousInvoiceAmountBoundCurrencyUserControl.PerformLayout();
			this.SimplifiedDecAcceptanceDateEdit.ResumeLayout(true);
			this.SimplifiedDecAcceptanceDateEdit.PerformLayout();
			this.TempProcLimitDateEdit.ResumeLayout(true);
			this.TempProcLimitDateEdit.PerformLayout();
			this.ProcedureCodeDropEdit.ResumeLayout(true);
			this.ProcedureCodeDropEdit.PerformLayout();
			this.ParticipantTypeDropEdit.ResumeLayout(true);
			this.ParticipantTypeDropEdit.PerformLayout();
			this.AssessmentDateEdit.ResumeLayout(true);
			this.AssessmentDateEdit.PerformLayout();
			this.PresentationOfGoodsDateEdit.ResumeLayout(true);
			this.PresentationOfGoodsDateEdit.PerformLayout();
			this.FromWarehouseUserControl.ResumeLayout(true);
			this.FromWarehouseUserControl.PerformLayout();
			this.ToWarehouseUserControl.ResumeLayout(true);
			this.ToWarehouseUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit ProcedureCodeDropEdit;
		internal ZDateEdit TempProcLimitDateEdit;
		internal ZTextBox IncotermTextBox;
		internal ZTextBox ValuationCodeTextBox;
		internal ZTextBox CurrencyTextBox;
		internal ZCheckBox BoxElectronicDocumentsCheckBox;
		internal ZTextBox FinancialAndBankingDataLine1TextBox;
		internal ZTextBox FinancialAndBankingDataLine2TextBox;
		internal ZCheckBox UseDeclarationOfIntentCheckBox;
		internal ZDropEdit ParticipantTypeDropEdit;
		internal ConvertToLocalCurrencyControl PreviousInvoiceAmountBoundCurrencyUserControl;
		internal ZCalcEdit PreviousInvoiceCurrencyExRateCalcEdit;
		internal ZCheckBox ClearanceByEntryLineCheckBox;
		internal ZDateEdit SimplifiedDecAcceptanceDateEdit;
		internal ZLabel OtherCustomsInformationLabel;
		internal ZLabel ToWarehouseLabel;
		internal ZLabel FromWarehouseLabel;
		internal ZDateEdit AssessmentDateEdit;
		internal ZLabel FinancialAndBankingDataLabel;
		internal ZLabel PreviousInvoiceLabel;
		internal ZDateTimeOffsetEdit PresentationOfGoodsDateEdit;
		internal FromWarehouseUserControl FromWarehouseUserControl;
		internal ToWarehouseUserControl ToWarehouseUserControl;
	}
}

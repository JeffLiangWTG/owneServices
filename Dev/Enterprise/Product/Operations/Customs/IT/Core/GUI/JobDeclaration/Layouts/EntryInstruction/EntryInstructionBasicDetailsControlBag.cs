using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryInstructionBasicDetailsControlBag : ControlBag
{
	protected override Control CreateTemplate() => new EntryInstructionBasicDetailsControl();

	public static EntryInstructionBasicDetailsControlBag Instance => instance ?? (instance = new EntryInstructionBasicDetailsControlBag());

	[ThreadStatic]
	static EntryInstructionBasicDetailsControlBag instance;

	EntryInstructionBasicDetailsControlBag()
	{
		ProcedureCodeDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ProcedureCodeDropEdit));
		TempProcLimitDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.TempProcLimitDateEdit));
		SimplifiedDecAcceptanceDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.SimplifiedDecAcceptanceDateEdit));
		ParticipantTypeDropEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ParticipantTypeDropEdit));
		AssessmentDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.AssessmentDateEdit));
		IncotermTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.IncotermTextBox));
		ValuationCodeTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ValuationCodeTextBox));
		CurrencyTextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.CurrencyTextBox));
		BoxElectronicDocumentsCheckBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.BoxElectronicDocumentsCheckBox));
		UseDeclarationOfIntentCheckBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.UseDeclarationOfIntentCheckBox));
		ClearanceByEntryLineCheckBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ClearanceByEntryLineCheckBox));
		OtherCustomsInformationLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.OtherCustomsInformationLabel));
		ToWarehouseLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseLabel));
		FromWarehouseLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseLabel));
		FinancialAndBankingDataLine1TextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FinancialAndBankingDataLine1TextBox));
		FinancialAndBankingDataLine2TextBox = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FinancialAndBankingDataLine2TextBox));
		PreviousInvoiceAmountBoundCurrencyUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.PreviousInvoiceAmountBoundCurrencyUserControl));
		PreviousInvoiceCurrencyExRateCalcEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.PreviousInvoiceCurrencyExRateCalcEdit));
		FinancialAndBankingDataLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FinancialAndBankingDataLabel));
		PreviousInvoiceLabel = RegisterControl(nameof(EntryInstructionBasicDetailsControl.PreviousInvoiceLabel));
		PresentationOfGoodsDateEdit = RegisterControl(nameof(EntryInstructionBasicDetailsControl.PresentationOfGoodsDateEdit));
		FromWarehouseUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.FromWarehouseUserControl));
		ToWarehouseUserControl = RegisterControl(nameof(EntryInstructionBasicDetailsControl.ToWarehouseUserControl));
	}

	public ControlReference ProcedureCodeDropEdit { get; }

	public ControlReference TempProcLimitDateEdit { get; }

	public ControlReference SimplifiedDecAcceptanceDateEdit { get; }

	public ControlReference ParticipantTypeDropEdit { get; }

	public ControlReference AssessmentDateEdit { get; }

	public ControlReference IncotermTextBox { get; }

	public ControlReference ValuationCodeTextBox { get; }

	public ControlReference CurrencyTextBox { get; }

	public ControlReference BoxElectronicDocumentsCheckBox { get; }

	public ControlReference UseDeclarationOfIntentCheckBox { get; }

	public ControlReference ClearanceByEntryLineCheckBox { get; }

	public ControlReference OtherCustomsInformationLabel { get; }

	public ControlReference ToWarehouseLabel { get; }

	public ControlReference FromWarehouseLabel { get; }

	public ControlReference FinancialAndBankingDataLine1TextBox { get; }

	public ControlReference FinancialAndBankingDataLine2TextBox { get; }

	public ControlReference PreviousInvoiceAmountBoundCurrencyUserControl { get; }

	public ControlReference PreviousInvoiceCurrencyExRateCalcEdit { get; }

	public ControlReference FinancialAndBankingDataLabel { get; }

	public ControlReference PreviousInvoiceLabel { get; }

	public ControlReference PresentationOfGoodsDateEdit { get; }

	public ControlReference FromWarehouseUserControl { get; }

	public ControlReference ToWarehouseUserControl { get; }
}

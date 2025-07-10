using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryInstructionBasicDetailsControlBag))]
sealed class EntryInstructionBasicDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => EntryInstructionBasicDetailsControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ProcedureCodeDropEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.TempProcLimitDateEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.SimplifiedDecAcceptanceDateEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ParticipantTypeDropEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseLabel));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseLabel));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.AssessmentDateEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.IncotermTextBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ValuationCodeTextBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.CurrencyTextBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.BoxElectronicDocumentsCheckBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.UseDeclarationOfIntentCheckBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ClearanceByEntryLineCheckBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.FinancialAndBankingDataLine1TextBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.FinancialAndBankingDataLine2TextBox));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.PreviousInvoiceAmountBoundCurrencyUserControl));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.PreviousInvoiceCurrencyExRateCalcEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.FinancialAndBankingDataLabel));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.OtherCustomsInformationLabel));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.PreviousInvoiceLabel));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.PresentationOfGoodsDateEdit));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseUserControl));
			yield return (nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseUserControl));
		}
	}
}

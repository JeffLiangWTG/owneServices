using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsControlBag))]
sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineDetailsControlBag.NetDutyCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.CustomNetWeightCalcDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.TareSupplementUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.CalculatedGrossMassCalcDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.VATCodeUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.VATValueConfirmationCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.PermitObligationDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.NonCustomsLawObligationDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.StorageTypeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.GrossMassConfirmationCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.NetMassConfirmationCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.AdditionalUnitConfirmationCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.StatisticalValueConfirmationCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.ConfirmationCodesSeparatorUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.NonCommercialGoodsCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.GoodsReturnedCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.DutyRateUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.RefundTypeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.RefundReferenceNumberTextBox);
			yield return nameof(InvoiceLineDetailsControlBag.RefundGoodsItemNumberIntEdit);
			yield return nameof(InvoiceLineDetailsControlBag.RefundReasonTextBox);
			yield return nameof(InvoiceLineDetailsControlBag.CusCodeFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.RateFormulaDescriptionTextBox);
			yield return nameof(InvoiceLineDetailsControlBag.RateOverrideCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.OverriddenRateCalcEdit);
			yield return nameof(InvoiceLineDetailsControlBag.UNDGCodesUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
}

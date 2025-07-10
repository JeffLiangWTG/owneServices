using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportAmendmentEntryDetailsLayoutItemsControlBag))]
	sealed class ImportAmendmentEntryDetailsLayoutItemsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.VersionNoCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.AmendmentTypeTextBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.AmendmentTypeDescriptionTextBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.ReasonCodeDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.AmendmentReasonTextBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.FaultPartyDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.FaultReasonTextBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyPaymentReasonCodeFindBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.TotalAmendedCountItemCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.TotalAmendedCountDutyTaxCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.BeforeTotalDutyTaxCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.AfterTotalDutyTaxCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.DutyTaxDifferenceCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.BeforeCustomsValueCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.AfterCustomsValueCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.CustomsValueDifferenceCalcEdit);

				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.DTYPenaltyTypeDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.DTYPenaltyReducedYNDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.DomesticTaxPenaltyTypeDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyExemptReqDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyExemptReasonCodeDropEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyExemptReasonMultiLineTextBox);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyExemptSequenceCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.PenaltyExemptAmountCalcEdit);
				yield return nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.RefundRequestYNDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance;
	}
}

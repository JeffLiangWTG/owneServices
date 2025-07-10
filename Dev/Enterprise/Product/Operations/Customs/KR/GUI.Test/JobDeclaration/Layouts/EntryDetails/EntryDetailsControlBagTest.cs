using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EntryDetailsControlBag))]
	sealed class EntryDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryDetailsControlBag.EntryNumberTextBox);
				yield return nameof(EntryDetailsControlBag.MessageTypeDropEdit);
				yield return nameof(EntryDetailsControlBag.MessageStatusDropEdit);
				yield return nameof(EntryDetailsControlBag.EntryStatusDropEdit);
				yield return nameof(EntryDetailsControlBag.EntrySubmittedDateEdit);
				yield return nameof(EntryDetailsControlBag.AcceptedDateEdit);
				yield return nameof(EntryDetailsControlBag.ClearedDateEdit);
				yield return nameof(EntryDetailsControlBag.IncotermTextBox);
				yield return nameof(EntryDetailsControlBag.TotalInvoiceAmountUserControl);
				yield return nameof(EntryDetailsControlBag.TotalCustomsValueKRWCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalCustomsValueUSDCalcEdit);
				yield return nameof(EntryDetailsControlBag.FreightCalcEdit);
				yield return nameof(EntryDetailsControlBag.InsuranceCalcEdit);
				yield return nameof(EntryDetailsControlBag.AdditionalAmountCalcEdit);
				yield return nameof(EntryDetailsControlBag.DeductedAmountCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalValueForVATCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalVATExemptionValueCalcEdit);
				yield return nameof(EntryDetailsControlBag.CustomsDisbursementBillGroupBox);
				yield return nameof(EntryDetailsControlBag.TotalDutyAmountCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalSpecialConsumptionTaxCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalTransportationTaxCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalLiquorTaxCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalEducationTaxCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalAgricultureTaxCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalVATCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalPayableAmountCalcEdit);
				yield return nameof(EntryDetailsControlBag.PenaltyForLateDeclarationCalcEdit);
				yield return nameof(EntryDetailsControlBag.PenaltyForMissedDeclarationCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalGrossWeightInKGCalcEdit);
				yield return nameof(EntryDetailsControlBag.TotalPackagesDropEdit);
				yield return nameof(EntryDetailsControlBag.CustomerOfficerTextBox);
				yield return nameof(EntryDetailsControlBag.CustomsRemarkLongTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryDetailsControlBag.Instance;
	}
}

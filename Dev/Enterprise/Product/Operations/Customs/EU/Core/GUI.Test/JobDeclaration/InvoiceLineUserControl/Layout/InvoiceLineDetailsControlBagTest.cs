using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.FormattedProcedureCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.UnformattedProcedureCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.DestinationCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.DispatchCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.DestinationUsingZZRefCusCodeListCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode1DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode2DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.AdditionalSupplementaryCodesUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.CountryOfSupplyCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.AdditionalProcedureCodesUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.PreferenceCodeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.QuotaDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.SecondQuotaDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.AdditionalSupplementaryCodesAndGDMUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.CusNumberCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.QuotaWithCheckLinkUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.ValuationAdjustmentPercentageCalcEdit);
				yield return nameof(InvoiceLineDetailsControlBag.TransactionNatureDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.UsedGoodsCodeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ReturnToOriginCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.SecondaryTreatedProductCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.ReturningGoodsReasonCodeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ReturningGoodsReasonDetailTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionPackCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionThreadCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionProductionYearCalcEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionDeferredInstallmentTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionEcologicalCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.InwardProcessingLicenseLineNumberTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.ProcessingDescriptionLongTextControl);
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryCode1AndGDMUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.EntryExitPurposeCodeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.EntryExitPurposeDetailTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.BorderTradeStateCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExportUnionAdditionalTariffCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.ExcessStockCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.CommercialPaymentCodeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ValuationCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.NationalAdditionalCode1DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.NationalAdditionalCode2DropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.NationalAdditionalCodesUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.RegionOfDestinationDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.PriceTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}

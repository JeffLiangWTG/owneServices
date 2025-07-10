using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(ExportInvoiceDetailsLayoutProvider))]
sealed class ExportInvoiceDetailsLayoutProviderTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;
			var inBag = CommercialInvoiceDetailsControlBag.Instance;
			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto),
				(commonBag.GroupInvoiceDropEdit, ControlWidthClass.Auto),
				(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto),
				(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto),
				(commonBag.IncoTermDropEdit, ControlWidthClass.Auto),
				(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto),
				(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto),
				(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto),
				(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto),
				(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto),
				(commonBag.ExporterAddressControl, ControlWidthClass.Auto),
				(inBag.ExporterContractNumberTextBox, ControlWidthClass.Auto),
				(commonBag.PaymentMethodDropEdit, ControlWidthClass.Auto),
				(inBag.PaymentDaysCalcEdit, ControlWidthClass.Auto),
				(inBag.AuthorizedEconomicOperatorGuidFindBox, ControlWidthClass.Auto),
				(inBag.AuthorizedEconomicOperatorCountryTextBox, ControlWidthClass.Medium),
				(inBag.AuthorizedEconomicOperatorCodeTextBox, ControlWidthClass.Auto),
				(inBag.AuthorizedEconomicOperatorRoleTextBox, ControlWidthClass.Medium),
				(inBag.IGSTPaymentStatusDropEdit, ControlWidthClass.Auto),
			};
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
}

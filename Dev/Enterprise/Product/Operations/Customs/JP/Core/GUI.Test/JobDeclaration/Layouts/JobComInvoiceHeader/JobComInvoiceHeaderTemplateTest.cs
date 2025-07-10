using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(JobComInvoiceHeaderTemplate))]
	sealed class JobComInvoiceHeaderTemplateTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new JobComInvoiceHeaderTemplate())
			{
				CombineAssertions(() =>
				{
					var grossWeightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("GrossWeightCalcDropEdit");
					AssertNotNull(grossWeightCalcDropEdit);

					var netWeightCalcDropEdit = control.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit");
					AssertNotNull(netWeightCalcDropEdit);

					var invoiceAmountConvertToLocalCurrencyControl = control.FindSingle<ConvertToLocalCurrencyControl>("InvoiceAmountConvertToLocalCurrencyControl");
					AssertNotNull(invoiceAmountConvertToLocalCurrencyControl);

					var commercialInvoiceDetailsIncoTermsUserControl = control.FindSingle<CommercialInvoiceDetailsIncoTermsUserControl>("CommercialInvoiceDetailsIncoTermsUserControl");
					AssertNotNull(invoiceAmountConvertToLocalCurrencyControl);
				});
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ImportMethodFourUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobComInvoiceHeader), control.BindingSource.DataSourceType);
			var salesOfHighestQuantityAmountCalcFindBox = control.FindSingle<ZCalcFindBox>("SalesOfHighestQuantityAmountCalcFindBox");
			var deductionCostCustomsReferenceNumberTextBox = control.FindSingle<ZTextBox>("DeductionCostCustomsReferenceNumberTextBox");
			var salesOfHighestQuantityExchangeRateCalcEdit = control.FindSingle<ZCalcEdit>("SalesOfHighestQuantityExchangeRateCalcEdit");
			var salesOfHighestQuantityAmountKRWCalcEdit = control.FindSingle<ZCalcEdit>("SalesOfHighestQuantityAmountKRWCalcEdit");
			var deductionCostConsignmentSalesFeeCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostConsignmentSalesFeeCalcEdit");
			var deductionCostGeneralCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostGeneralCostCalcEdit");
			var deductionCostCostRateCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostCostRateCalcEdit");
			var deductionCostTransportationCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostTransportationCostCalcEdit");
			var deductionCosInsuranceCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCosInsuranceCalcEdit");
			var deductionCostUnloadCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostUnloadCostCalcEdit");
			var deductionCostOtherTransportationCostsCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostOtherTransportationCostsCalcEdit");
			var deductionCostAdditionalCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostAdditionalCostCalcEdit");
			var deductionCosTaxCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCosTaxCalcEdit");
			var deductionCostTotalDeductionAmountCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostTotalDeductionAmountCalcEdit");
			var deductionCostCostRateCodeDropEdit = control.FindSingle<ZDropEdit>("DeductionCostCostRateCodeDropEdit");

			AssertEquals("DeductionCostAmount", salesOfHighestQuantityAmountCalcFindBox.BindToAmount);
			AssertEquals("ReplacementCurrency", salesOfHighestQuantityAmountCalcFindBox.BindToUnit);
			AssertEquals("CustomsReferenceNumber", deductionCostCustomsReferenceNumberTextBox.BindTo);
			AssertEquals("JZ_InvoiceCurrExRate", salesOfHighestQuantityExchangeRateCalcEdit.BindTo);
			AssertEquals("ReplacementAmountKRW", salesOfHighestQuantityAmountKRWCalcEdit.BindTo);
			AssertEquals("ConsignmentSalesFee", deductionCostConsignmentSalesFeeCalcEdit.BindTo);
			AssertEquals("GeneralCost", deductionCostGeneralCostCalcEdit.BindTo);
			AssertEquals("JZ_DeductionRate", deductionCostCostRateCalcEdit.BindTo);
			AssertEquals("DeductionTransportationCost", deductionCostTransportationCostCalcEdit.BindTo);
			AssertEquals("DeductionInsurance", deductionCosInsuranceCalcEdit.BindTo);
			AssertEquals("DeductionUnloadCost", deductionCostUnloadCostCalcEdit.BindTo);
			AssertEquals("OtherTransportationCosts", deductionCostOtherTransportationCostsCalcEdit.BindTo);
			AssertEquals("AdditionalCost", deductionCostAdditionalCostCalcEdit.BindTo);
			AssertEquals("Tax", deductionCosTaxCalcEdit.BindTo);
			AssertEquals("DeductionCostTotalDeductionAmount", deductionCostTotalDeductionAmountCalcEdit.BindTo);
			AssertEquals("JZ_DeductionType", deductionCostCostRateCodeDropEdit.BindTo);

			Assert(!salesOfHighestQuantityAmountCalcFindBox.ReadOnly);
			Assert(!deductionCostCustomsReferenceNumberTextBox.ReadOnly);
			Assert(!salesOfHighestQuantityExchangeRateCalcEdit.ReadOnly);
			Assert(!salesOfHighestQuantityAmountKRWCalcEdit.ReadOnly);
			Assert(!deductionCostConsignmentSalesFeeCalcEdit.ReadOnly);
			Assert(!deductionCostGeneralCostCalcEdit.ReadOnly);
			Assert(!deductionCostCostRateCalcEdit.ReadOnly);
			Assert(!deductionCostTransportationCostCalcEdit.ReadOnly);
			Assert(!deductionCosInsuranceCalcEdit.ReadOnly);
			Assert(!deductionCostUnloadCostCalcEdit.ReadOnly);
			Assert(!deductionCostOtherTransportationCostsCalcEdit.ReadOnly);
			Assert(!deductionCostAdditionalCostCalcEdit.ReadOnly);
			Assert(!deductionCosTaxCalcEdit.ReadOnly);
			Assert(!deductionCostTotalDeductionAmountCalcEdit.ReadOnly);
			Assert(!deductionCostCostRateCodeDropEdit.ReadOnly);

			control.BindToMessageSendingObject();
			salesOfHighestQuantityAmountCalcFindBox = control.FindSingle<ZCalcFindBox>("SalesOfHighestQuantityAmountCalcFindBox");
			deductionCostCustomsReferenceNumberTextBox = control.FindSingle<ZTextBox>("DeductionCostCustomsReferenceNumberTextBox");
			salesOfHighestQuantityExchangeRateCalcEdit = control.FindSingle<ZCalcEdit>("SalesOfHighestQuantityExchangeRateCalcEdit");
			salesOfHighestQuantityAmountKRWCalcEdit = control.FindSingle<ZCalcEdit>("SalesOfHighestQuantityAmountKRWCalcEdit");
			deductionCostConsignmentSalesFeeCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostConsignmentSalesFeeCalcEdit");
			deductionCostGeneralCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostGeneralCostCalcEdit");
			deductionCostCostRateCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostCostRateCalcEdit");
			deductionCostTransportationCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostTransportationCostCalcEdit");
			deductionCosInsuranceCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCosInsuranceCalcEdit");
			deductionCostUnloadCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostUnloadCostCalcEdit");
			deductionCostOtherTransportationCostsCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostOtherTransportationCostsCalcEdit");
			deductionCostAdditionalCostCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostAdditionalCostCalcEdit");
			deductionCosTaxCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCosTaxCalcEdit");
			deductionCostTotalDeductionAmountCalcEdit = control.FindSingle<ZCalcEdit>("DeductionCostTotalDeductionAmountCalcEdit");
			deductionCostCostRateCodeDropEdit = control.FindSingle<ZDropEdit>("DeductionCostCostRateCodeDropEdit");

			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.DeductionCostAmount", salesOfHighestQuantityAmountCalcFindBox.BindToAmount);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.ReplacementCurrency", salesOfHighestQuantityAmountCalcFindBox.BindToUnit);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.CustomsReferenceNumber", deductionCostCustomsReferenceNumberTextBox.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_InvoiceCurrExRate", salesOfHighestQuantityExchangeRateCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.ReplacementAmountKRW", salesOfHighestQuantityAmountKRWCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.ConsignmentSalesFee", deductionCostConsignmentSalesFeeCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.GeneralCost", deductionCostGeneralCostCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_DeductionRate", deductionCostCostRateCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.DeductionTransportationCost", deductionCostTransportationCostCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.DeductionInsurance", deductionCosInsuranceCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.DeductionUnloadCost", deductionCostUnloadCostCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.OtherTransportationCosts", deductionCostOtherTransportationCostsCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.AdditionalCost", deductionCostAdditionalCostCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.Tax", deductionCosTaxCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.DeductionCostTotalDeductionAmount", deductionCostTotalDeductionAmountCalcEdit.BindTo);
			AssertEquals("SendingObjectsCollection.InvoiceHeader.JZ_DeductionType", deductionCostCostRateCodeDropEdit.BindTo);

			Assert(!salesOfHighestQuantityAmountCalcFindBox.ReadOnly);
			Assert(deductionCostCustomsReferenceNumberTextBox.ReadOnly);
			Assert(!salesOfHighestQuantityExchangeRateCalcEdit.ReadOnly);
			Assert(!salesOfHighestQuantityAmountKRWCalcEdit.ReadOnly);
			Assert(!deductionCostConsignmentSalesFeeCalcEdit.ReadOnly);
			Assert(!deductionCostGeneralCostCalcEdit.ReadOnly);
			Assert(deductionCostCostRateCalcEdit.ReadOnly);
			Assert(!deductionCostTransportationCostCalcEdit.ReadOnly);
			Assert(!deductionCosInsuranceCalcEdit.ReadOnly);
			Assert(!deductionCostUnloadCostCalcEdit.ReadOnly);
			Assert(!deductionCostOtherTransportationCostsCalcEdit.ReadOnly);
			Assert(!deductionCostAdditionalCostCalcEdit.ReadOnly);
			Assert(!deductionCosTaxCalcEdit.ReadOnly);
			Assert(!deductionCostTotalDeductionAmountCalcEdit.ReadOnly);
			Assert(deductionCostCostRateCodeDropEdit.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ImportMethodFourUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ImportMethodFourUserControl control;
	}
}

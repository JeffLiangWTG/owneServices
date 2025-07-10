using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ImportMethodTwoToThreeUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
		{
			AssertEquals(typeof(Business.JobComInvoiceHeader), control.BindingSource.DataSourceType);
			AssertEquals("ReplacementAmountCalcFindBox Binding", "ReplaceAmount", control.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").BindToAmount);
			AssertEquals("ReplacementAmountCalcFindBox Binding", "ReplacementCurrency", control.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").BindToUnit);
			AssertEquals("ReplacementExchangeRateCalcEdit Binding", "JZ_InvoiceCurrExRate", control.FindSingle<ZCalcEdit>("ReplacementExchangeRateCalcEdit").BindTo);
			AssertEquals("ReplacementAmountKRWCalcEdit Binding", "ReplacementAmountKRW", control.FindSingle<ZCalcEdit>("ReplacementAmountKRWCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentQuantityDiscountCalcEdit Binding", "AdditionalAdjustmentQuantityDiscount", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentQuantityDiscountCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentCommercialAmountCalcEdit Binding", "AdditionalAdjustmentCommercialAmount", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentCommercialAmountCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentTransportationCostCalcEdit Binding", "AdditionalAdjustmentTransportationCost", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentTransportationCostCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentShippingPortCostCalcEdit Binding", "AdditionalAdjustmentShippingPortCost", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentShippingPortCostCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentInsuranceCalcEdit Binding", "AdditionalAdjustmentInsurance", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentInsuranceCalcEdit").BindTo);
			AssertEquals("TotalAdditionalAdjustmentAmountCalcEdit Binding", "TotalAdditionalAdjustmentAmount", control.FindSingle<ZCalcEdit>("TotalAdditionalAdjustmentAmountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentQuantityDiscountCalcEdit Binding", "DeductionAdjustmentQuantityDiscount", control.FindSingle<ZCalcEdit>("DeductionAdjustmentQuantityDiscountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentCommercialAmountCalcEdit Binding", "DeductionAdjustmentCommercialAmount", control.FindSingle<ZCalcEdit>("DeductionAdjustmentCommercialAmountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentTransportationCostCalcEdit Binding", "DeductionAdjustmentTransportationCost", control.FindSingle<ZCalcEdit>("DeductionAdjustmentTransportationCostCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentShippingPortCostCalcEdit Binding", "DeductionAdjustmentShippingPortCost", control.FindSingle<ZCalcEdit>("DeductionAdjustmentShippingPortCostCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentInsuranceCalcEdit Binding", "DeductionAdjustmentInsurance", control.FindSingle<ZCalcEdit>("DeductionAdjustmentInsuranceCalcEdit").BindTo);
			AssertEquals("TotalDeductionAdjustmentAmountCalcEdit Binding", "TotalDeductionAdjustmentAmount", control.FindSingle<ZCalcEdit>("TotalDeductionAdjustmentAmountCalcEdit").BindTo);

			control.BindToMessageSendingObject();
			AssertEquals(typeof(Business.ValuationDeclarationMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("ReplacementAmountCalcFindBox Binding", "SendingObjectsCollection.InvoiceHeader.ReplaceAmount", control.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").BindToAmount);
			AssertEquals("ReplacementAmountCalcFindBox Binding", "SendingObjectsCollection.InvoiceHeader.ReplacementCurrency", control.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").BindToUnit);
			AssertEquals("ReplacementExchangeRateCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.JZ_InvoiceCurrExRate", control.FindSingle<ZCalcEdit>("ReplacementExchangeRateCalcEdit").BindTo);
			AssertEquals("ReplacementAmountKRWCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.ReplacementAmountKRW", control.FindSingle<ZCalcEdit>("ReplacementAmountKRWCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentQuantityDiscountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalAdjustmentQuantityDiscount", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentQuantityDiscountCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentCommercialAmountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalAdjustmentCommercialAmount", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentCommercialAmountCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentTransportationCostCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalAdjustmentTransportationCost", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentTransportationCostCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentShippingPortCostCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalAdjustmentShippingPortCost", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentShippingPortCostCalcEdit").BindTo);
			AssertEquals("AdditionalAdjustmentInsuranceCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.AdditionalAdjustmentInsurance", control.FindSingle<ZCalcEdit>("AdditionalAdjustmentInsuranceCalcEdit").BindTo);
			AssertEquals("TotalAdditionalAdjustmentAmountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.TotalAdditionalAdjustmentAmount", control.FindSingle<ZCalcEdit>("TotalAdditionalAdjustmentAmountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentQuantityDiscountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.DeductionAdjustmentQuantityDiscount", control.FindSingle<ZCalcEdit>("DeductionAdjustmentQuantityDiscountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentCommercialAmountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.DeductionAdjustmentCommercialAmount", control.FindSingle<ZCalcEdit>("DeductionAdjustmentCommercialAmountCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentTransportationCostCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.DeductionAdjustmentTransportationCost", control.FindSingle<ZCalcEdit>("DeductionAdjustmentTransportationCostCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentShippingPortCostCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.DeductionAdjustmentShippingPortCost", control.FindSingle<ZCalcEdit>("DeductionAdjustmentShippingPortCostCalcEdit").BindTo);
			AssertEquals("DeductionAdjustmentInsuranceCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.DeductionAdjustmentInsurance", control.FindSingle<ZCalcEdit>("DeductionAdjustmentInsuranceCalcEdit").BindTo);
			AssertEquals("TotalDeductionAdjustmentAmountCalcEdit Binding", "SendingObjectsCollection.InvoiceHeader.TotalDeductionAdjustmentAmount", control.FindSingle<ZCalcEdit>("TotalDeductionAdjustmentAmountCalcEdit").BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ImportMethodTwoToThreeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ImportMethodTwoToThreeUserControl control;
	}
}

using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportValuationDeclarationUserControlTest : TestCaseWithFactory
	{
		public void TestValuationDeclarationTabControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			CreateSendingObjectData(ZString.Empty);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodOne);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodTwo);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodThree);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodFourA);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodFourB);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodFive);
			CreateSendingObjectData(ValuationCodeList.Codes.MethodSix);
			CreateSendingObjectData("XX");

			using (var form = new MessageSendingActionFormForTest(new ValuationDeclarationMessageSendingObjectParent(declaration), new Import934MessageSendingFormBuilder()))
			{
				form.Show();

				var sendingObjectsGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertEquals(9, sendingObjectsGrid.ListManager.Count);
				var userControl = form.FindSingle<ImportValuationDeclarationUserControl>("ImportValuationDeclarationUserControl");
				sendingObjectsGrid.CurrentRowIndex = 8;
				userControl.SetDataBinding(userControl.DataSourceForBinding, userControl.DataSourceTypeName);

				sendingObjectsGrid.CurrentRowIndex = 0;
				AssertTabPages(userControl, "QuestionTabPage", "PriceTabPage");
				sendingObjectsGrid.CurrentRowIndex = 1;
				AssertTabPages(userControl, "QuestionTabPage", "PriceTabPage");
				sendingObjectsGrid.CurrentRowIndex = 2;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodTwoToThreeTabPage");
				sendingObjectsGrid.CurrentRowIndex = 3;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodTwoToThreeTabPage");
				sendingObjectsGrid.CurrentRowIndex = 4;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodFourTabPage");
				sendingObjectsGrid.CurrentRowIndex = 5;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodFourTabPage");
				sendingObjectsGrid.CurrentRowIndex = 6;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodFiveToSixTabPage");
				sendingObjectsGrid.CurrentRowIndex = 7;
				AssertTabPages(userControl, "MethodTwoToSixTabPage", "MethodFiveToSixTabPage");
				sendingObjectsGrid.CurrentRowIndex = 8;
				var valuationDeclarationTabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
				AssertEquals(2, valuationDeclarationTabControl.TabPages.Count);
				AssertEquals("DetailsTabPage", valuationDeclarationTabControl.TabPages[0].Name);
				AssertEquals("MethodTwoToSixTabPage", valuationDeclarationTabControl.TabPages[1].Name);
			}

			void AssertTabPages(ZUserControl userControl, string tabPage1, string tabPage2)
			{
				var tabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
				AssertEquals(3, tabControl.TabPages.Count);
				AssertEquals("DetailsTabPage", tabControl.TabPages[0].Name);
				AssertEquals(tabPage1, tabControl.TabPages[1].Name);
				AssertEquals(tabPage2, tabControl.TabPages[2].Name);
			}

			void CreateSendingObjectData(string valuationCode)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_ValuationCode = valuationCode;
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		public void TestValuationTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new MessageSendingActionFormForTest(new ValuationDeclarationMessageSendingObjectParent(declaration), new Import934MessageSendingFormBuilder()))
			{
				var userControl = form.FindSingle<ImportValuationDeclarationUserControl>("ImportValuationDeclarationUserControl");

				var dynamicDocumentLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DynamicDocumentLayoutPanel");
				AssertNotNull(dynamicDocumentLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToSixControlBag.ExpectedCustomsValueCalcEdit)));
				AssertNotNull(dynamicDocumentLayoutPanel.FindSingle<ZTextBox>(nameof(MethodTwoToSixControlBag.SupportingDocument1TextBox)));
				AssertNotNull(dynamicDocumentLayoutPanel.FindSingle<ZTextBox>(nameof(MethodTwoToSixControlBag.SupportingDocument2TextBox)));
				var dynamicItemUseCodeLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DynamicItemUseCodeLayoutPanel");
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.SampleItemCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.AdvertisingUseCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.UseOfDefectiveRepairCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.ReplacementItemCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.GiftOrFreeDonationCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.ForProductionAndManufactureCheckBox)));
				AssertNotNull(dynamicItemUseCodeLayoutPanel.FindSingle<ZTextBox>(nameof(MethodTwoToSixControlBag.ItemUseCodeOtherReasonTextBox)));
				var dynamicGoodsPricingBasisLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DynamicGoodsPricingBasisLayoutPanel");
				AssertNotNull(dynamicGoodsPricingBasisLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.PerformancePriceOfPaidTransactionCheckBox)));
				AssertNotNull(dynamicGoodsPricingBasisLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.PriceListCheckBox)));
				AssertNotNull(dynamicGoodsPricingBasisLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.ManufacturingCostCheckBox)));
				AssertNotNull(dynamicGoodsPricingBasisLayoutPanel.FindSingle<ZCheckBox>(nameof(MethodTwoToSixControlBag.InvoiceCheckBox)));
				AssertNotNull(dynamicGoodsPricingBasisLayoutPanel.FindSingle<ZTextBox>(nameof(MethodTwoToSixControlBag.GoodsPricingBasisOtherReasonTextBox)));

				var replacementDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("ReplacementDynamicLayoutPanel");
				AssertNotNull(replacementDynamicLayoutPanel.FindSingle<ZCalcFindBox>(nameof(MethodTwoToThreeControlBag.ReplacementAmountCalcFindBox)));
				AssertNotNull(replacementDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.ReplacementAmountKRWCalcEdit)));
				AssertNotNull(replacementDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.ReplacementExchangeRateCalcEdit)));
				var additionalAdjustmentDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("AdditionalAdjustmentDynamicLayoutPanel");
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentQuantityDiscountCalcEdit)));
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentTransportationCostCalcEdit)));
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentInsuranceCalcEdit)));
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentCommercialAmountCalcEdit)));
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentShippingPortCostCalcEdit)));
				AssertNotNull(additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.TotalAdditionalAdjustmentAmountCalcEdit)));
				var deductionAdjustmentDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DeductionAdjustmentDynamicLayoutPanel");
				AssertNotNull(deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.DeductionAdjustmentQuantityDiscountCalcEdit)));
				AssertNotNull(deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.DeductionAdjustmentTransportationCostCalcEdit)));
				AssertNotNull(deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.DeductionAdjustmentInsuranceCalcEdit)));
				AssertNotNull(deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.DeductionAdjustmentShippingPortCostCalcEdit)));
				AssertNotNull(deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodTwoToThreeControlBag.TotalDeductionAdjustmentAmountCalcEdit)));

				var salesOfHighestQuantityDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("SalesOfHighestQuantityDynamicLayoutPanel");
				AssertNotNull(salesOfHighestQuantityDynamicLayoutPanel.FindSingle<ZCalcFindBox>(nameof(MethodFourControlBag.SalesOfHighestQuantityAmountCalcFindBox)));
				AssertNotNull(salesOfHighestQuantityDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.SalesOfHighestQuantityAmountKRWCalcEdit)));
				AssertNotNull(salesOfHighestQuantityDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.SalesOfHighestQuantityExchangeRateCalcEdit)));
				var deductionCostDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DeductionCostDynamicLayoutPanel");
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(MethodFourControlBag.DeductionCostCustomsReferenceNumberTextBox)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostGeneralCostCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZDropEdit>(nameof(MethodFourControlBag.DeductionCostCostRateCodeDropEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCosInsuranceCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostOtherTransportationCostsCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCosTaxCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostConsignmentSalesFeeCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostCostRateCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostTransportationCostCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostUnloadCostCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostAdditionalCostCalcEdit)));
				AssertNotNull(deductionCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFourControlBag.DeductionCostTotalDeductionAmountCalcEdit)));

				var amountAgreedUponWithCustomsDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("AmountAgreedUponWithCustomsDynamicLayoutPanel");
				AssertNotNull(amountAgreedUponWithCustomsDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(MethodFiveToSixControlBag.AmountAgreedUponWithCustomsKRWCalcEdit)));
				var additionalCostDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("AdditionalCostDynamicLayoutPanel");
				AssertNotNull(additionalCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFiveToSixControlBag.AdditionalCostFreightToArrivalPortCalcEdit)));
				AssertNotNull(additionalCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFiveToSixControlBag.AdditionalCostInsuranceCalcEdit)));
				AssertNotNull(additionalCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFiveToSixControlBag.AdditionalCostFreightToDeparturePortCalcEdit)));
				AssertNotNull(additionalCostDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(MethodFiveToSixControlBag.AdditionalCostTotalAdditionalAmountCalcEdit)));

				var question5Panel = userControl.FindSingle<DynamicLayoutPanel>("Question5Panel");
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5ALongLabel)));
				AssertNotNull(question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5ADropEdit)));
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5BLongLabel)));
				AssertNotNull(question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5BDropEdit)));
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5CLabel)));
				AssertNotNull(question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5CDropEdit)));
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5DLabel)));
				AssertNotNull(question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5DDropEdit)));
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5ELabel)));
				AssertNotNull(question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5EDropEdit)));
				AssertNotNull(question5Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question5ETextLabel)));
				AssertNotNull(question5Panel.FindSingle<ZTextBox>(nameof(Question5To7ControlBag.Question5ETextBox)));

				var question6Panel = userControl.FindSingle<DynamicLayoutPanel>("Question6Panel");
				AssertNotNull(question6Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question6ALabel)));
				AssertNotNull(question6Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question6ADropEdit)));
				AssertNotNull(question6Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question6BLabel)));
				AssertNotNull(question6Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question6BDropEdit)));

				var question7Panel = userControl.FindSingle<DynamicLayoutPanel>("Question7Panel");
				AssertNotNull(question7Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question7ALabel)));
				AssertNotNull(question7Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question7ADropEdit)));
				AssertNotNull(question7Panel.FindSingle<ZLabel>(nameof(Question5To7ControlBag.Question7BLabel)));
				AssertNotNull(question7Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question7BDropEdit)));

				var basisForCalculationPanel = userControl.FindSingle<DynamicLayoutPanel>("BasisForCalculationPanel");
				AssertNotNull(basisForCalculationPanel.FindSingle<ConvertToLocalCurrencyControl>(nameof(PriceControlBag.PaymentAmountConvertToLocalCurrencyControl)));
				AssertNotNull(basisForCalculationPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ExchangeRateCalcEdit)));
				AssertNotNull(basisForCalculationPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.IndirectAmountCalcEdit)));
				AssertNotNull(basisForCalculationPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.PaymentAmountCalcEdit)));

				var additionalCostsPanel = userControl.FindSingle<DynamicLayoutPanel>("AdditionalCostsPanel");
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.PurchaseCostCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.BrokerageFeeCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ContainerPackagingCostCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.GoodsCostCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ProductToolCostsCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.CommodityUsageCostsCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ProductDevCostsCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.RoyaltyCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ProfitAmountCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.ExcludingTransportationCostsCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.FreightCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.UnloadCostCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.InsuranceCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.TransportationCostCalcEdit)));
				AssertNotNull(additionalCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.TotalAdditionalAmountCalcEdit)));

				var deductionCostsPanel = userControl.FindSingle<DynamicLayoutPanel>("DeductionCostsPanel");
				AssertNotNull(deductionCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.LocalTransportationCostCalcEdit)));
				AssertNotNull(deductionCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.TechnicalCostCalcEdit)));
				AssertNotNull(deductionCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.OtherCostsCalcEdit)));
				AssertNotNull(deductionCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.DiscountAmountCalcEdit)));
				AssertNotNull(deductionCostsPanel.FindSingle<ZCalcEdit>(nameof(PriceControlBag.TotalDeductionAmountCalcEdit)));
			}
		}

		public void TestQuestionTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new MessageSendingActionFormForTest(new ValuationDeclarationMessageSendingObjectParent(declaration), new Import934MessageSendingFormBuilder()))
			{
				var userControl = form.FindSingle<ImportValuationDeclarationUserControl>("ImportValuationDeclarationUserControl");
				var question5Panel = userControl.FindSingle<DynamicLayoutPanel>("Question5Panel");
				AssertEquals(true, question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5ADropEdit)).ReadOnly);
				AssertEquals(true, question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5BDropEdit)).ReadOnly);
				AssertEquals(true, question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5CDropEdit)).ReadOnly);
				AssertEquals(true, question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5DDropEdit)).ReadOnly);
				AssertEquals(true, question5Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question5EDropEdit)).ReadOnly);
				AssertEquals(true, question5Panel.FindSingle<ZTextBox>(nameof(Question5To7ControlBag.Question5ETextBox)).ReadOnly);

				var question6Panel = userControl.FindSingle<DynamicLayoutPanel>("Question6Panel");
				AssertEquals(true, question6Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question6ADropEdit)).ReadOnly);
				AssertEquals(true, question6Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question6BDropEdit)).ReadOnly);

				var question7Panel = userControl.FindSingle<DynamicLayoutPanel>("Question7Panel");
				AssertEquals(true, question7Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question7ADropEdit)).ReadOnly);
				AssertEquals(true, question7Panel.FindSingle<ZDropEdit>(nameof(Question5To7ControlBag.Question7BDropEdit)).ReadOnly);
			}
		}

		class MessageSendingActionFormForTest : MessageSendingActionForm
		{
			public MessageSendingActionFormForTest(IJobDeclarationMessageSendingObjectParent declarationWrapper, MessageSendingFormBuilder builder) : base(declarationWrapper, builder) { }
			public bool CheckIsOKToSend_Exposed() => base.CheckIsOKToSend();
		}
	}
}

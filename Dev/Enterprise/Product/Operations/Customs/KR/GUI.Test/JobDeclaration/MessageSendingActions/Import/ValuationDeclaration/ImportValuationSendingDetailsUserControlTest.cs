using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class ImportValuationSendingDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsTabPage()
		{
			using (var userControl = new ImportValuationSendingDetailsUserControl())
			{
				var detailsDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("DetailsDynamicLayoutPanel");
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(DetailsAndProvisionalPriceControlBag.InvoiceNoTextBox)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZDateEdit>(nameof(DetailsAndProvisionalPriceControlBag.InvoiceDateEdit)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(DetailsAndProvisionalPriceControlBag.PurchaseOrderNoTextBox)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZDateEdit>(nameof(DetailsAndProvisionalPriceControlBag.PurchaseOrderDateEdit)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(DetailsAndProvisionalPriceControlBag.ContractNoTextBox)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZDateEdit>(nameof(DetailsAndProvisionalPriceControlBag.ContractDateEdit)));
				AssertNotNull(detailsDynamicLayoutPanel.FindSingle<ZCalcEdit>(nameof(DetailsAndProvisionalPriceControlBag.TotalCustomsValueCalcEdit)));

				var detailsAndProvisionalPriceLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("ProvisionalPriceDynamicLayoutPanel");
				AssertNotNull(detailsAndProvisionalPriceLayoutPanel.FindSingle<ZDropEdit>(nameof(DetailsAndProvisionalPriceControlBag.ProvisionalPricingDropEdit)));
				AssertNotNull(detailsAndProvisionalPriceLayoutPanel.FindSingle<ZCalcEdit>(nameof(DetailsAndProvisionalPriceControlBag.ProvisionalAdditionRateCalcEdit)));
				AssertNotNull(detailsAndProvisionalPriceLayoutPanel.FindSingle<ZCalcEdit>(nameof(DetailsAndProvisionalPriceControlBag.ProvisionalAdditionAmountCalcEdit)));
				AssertNotNull(detailsAndProvisionalPriceLayoutPanel.FindSingle<ZDateEdit>(nameof(DetailsAndProvisionalPriceControlBag.EstimatedDateOfFinalPriceDateEdit)));
				AssertNotNull(detailsAndProvisionalPriceLayoutPanel.FindSingle<ZDateEdit>(nameof(DetailsAndProvisionalPriceControlBag.ContractExpirationDateEdit)));

				var provisionalPricingReasonsDynamicLayoutPanel = userControl.FindSingle<DynamicLayoutPanel>("ProvisionalPricingReasonsDynamicLayoutPanel");
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason101CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason102CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason103CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason104CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason105CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason106CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason107CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason108CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason109CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason110CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason111CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason112CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason113CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason114CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason115CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason116CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason117CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZCheckBox>(nameof(ProvisionalPricingReasonsControlBag.ProvisionalPricingReason120CheckBox)));
				AssertNotNull(provisionalPricingReasonsDynamicLayoutPanel.FindSingle<ZTextBox>(nameof(ProvisionalPricingReasonsControlBag.OtherReasonTextBox)));
			}
		}
	}
}

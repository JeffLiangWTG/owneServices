using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class PriceControlBag : ControlBag
	{
		PriceControlBag(BindingContext bindingContext)
		{
			PaymentAmountConvertToLocalCurrencyControl = RegisterControl(nameof(PriceUserControl.PaymentAmountConvertToLocalCurrencyControl));
			ExchangeRateCalcEdit = RegisterControl(nameof(PriceUserControl.ExchangeRateCalcEdit));
			IndirectAmountCalcEdit = RegisterControl(nameof(PriceUserControl.IndirectAmountCalcEdit));
			PaymentAmountCalcEdit = RegisterControl(nameof(PriceUserControl.PaymentAmountCalcEdit));

			PurchaseCostCalcEdit = RegisterControl(nameof(PriceUserControl.PurchaseCostCalcEdit));
			BrokerageFeeCalcEdit = RegisterControl(nameof(PriceUserControl.BrokerageFeeCalcEdit));
			ContainerPackagingCostCalcEdit = RegisterControl(nameof(PriceUserControl.ContainerPackagingCostCalcEdit));
			GoodsCostCalcEdit = RegisterControl(nameof(PriceUserControl.GoodsCostCalcEdit));
			ProductToolCostsCalcEdit = RegisterControl(nameof(PriceUserControl.ProductToolCostsCalcEdit));
			CommodityUsageCostsCalcEdit = RegisterControl(nameof(PriceUserControl.CommodityUsageCostsCalcEdit));
			ProductDevCostsCalcEdit = RegisterControl(nameof(PriceUserControl.ProductDevCostsCalcEdit));
			RoyaltyCalcEdit = RegisterControl(nameof(PriceUserControl.RoyaltyCalcEdit));
			ProfitAmountCalcEdit = RegisterControl(nameof(PriceUserControl.ProfitAmountCalcEdit));
			ExcludingTransportationCostsCalcEdit = RegisterControl(nameof(PriceUserControl.ExcludingTransportationCostsCalcEdit));
			FreightCalcEdit = RegisterControl(nameof(PriceUserControl.FreightCalcEdit));
			UnloadCostCalcEdit = RegisterControl(nameof(PriceUserControl.UnloadCostCalcEdit));
			InsuranceCalcEdit = RegisterControl(nameof(PriceUserControl.InsuranceCalcEdit));
			TransportationCostCalcEdit = RegisterControl(nameof(PriceUserControl.TransportationCostCalcEdit));
			TotalAdditionalAmountCalcEdit = RegisterControl(nameof(PriceUserControl.TotalAdditionalAmountCalcEdit));

			LocalTransportationCostCalcEdit = RegisterControl(nameof(PriceUserControl.LocalTransportationCostCalcEdit));
			TechnicalCostCalcEdit = RegisterControl(nameof(PriceUserControl.TechnicalCostCalcEdit));
			OtherCostsCalcEdit = RegisterControl(nameof(PriceUserControl.OtherCostsCalcEdit));
			DiscountAmountCalcEdit = RegisterControl(nameof(PriceUserControl.DiscountAmountCalcEdit));
			TotalDeductionAmountCalcEdit = RegisterControl(nameof(PriceUserControl.TotalDeductionAmountCalcEdit));
			this.bindingContext = bindingContext;
		}
		readonly BindingContext bindingContext;

		public static PriceControlBag InstanceForDeclaration => instanceForDeclaration ?? (instanceForDeclaration = new PriceControlBag(BindingContext.InvoiceHeader));
		[ThreadStatic]
		static PriceControlBag instanceForDeclaration;

		public static PriceControlBag InstanceForMessageSendingObject => instanceForMessageSendingObject ?? (instanceForMessageSendingObject = new PriceControlBag(BindingContext.MessageSending));
		[ThreadStatic]
		static PriceControlBag instanceForMessageSendingObject;

		protected override Control CreateTemplate()
		{
			var result = new PriceUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSendingObject();
			}
			return result;
		}

		public ControlReference PaymentAmountConvertToLocalCurrencyControl { get; }
		public ControlReference ExchangeRateCalcEdit { get; }
		public ControlReference IndirectAmountCalcEdit { get; }
		public ControlReference PaymentAmountCalcEdit { get; }

		public ControlReference PurchaseCostCalcEdit { get; }
		public ControlReference BrokerageFeeCalcEdit { get; }
		public ControlReference ContainerPackagingCostCalcEdit { get; }
		public ControlReference GoodsCostCalcEdit { get; }
		public ControlReference ProductToolCostsCalcEdit { get; }
		public ControlReference CommodityUsageCostsCalcEdit { get; }
		public ControlReference ProductDevCostsCalcEdit { get; }
		public ControlReference RoyaltyCalcEdit { get; }
		public ControlReference ProfitAmountCalcEdit { get; }
		public ControlReference ExcludingTransportationCostsCalcEdit { get; }
		public ControlReference FreightCalcEdit { get; }
		public ControlReference UnloadCostCalcEdit { get; }
		public ControlReference InsuranceCalcEdit { get; }
		public ControlReference TransportationCostCalcEdit { get; }
		public ControlReference TotalAdditionalAmountCalcEdit { get; }

		public ControlReference LocalTransportationCostCalcEdit { get; }
		public ControlReference TechnicalCostCalcEdit { get; }
		public ControlReference OtherCostsCalcEdit { get; }
		public ControlReference DiscountAmountCalcEdit { get; }
		public ControlReference TotalDeductionAmountCalcEdit { get; }
	}
}

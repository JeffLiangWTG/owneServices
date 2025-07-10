using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(PriceControlBag))]
	sealed class PriceControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PriceControlBag.PaymentAmountConvertToLocalCurrencyControl);
				yield return nameof(PriceControlBag.ExchangeRateCalcEdit);
				yield return nameof(PriceControlBag.IndirectAmountCalcEdit);
				yield return nameof(PriceControlBag.PaymentAmountCalcEdit);

				yield return nameof(PriceControlBag.PurchaseCostCalcEdit);
				yield return nameof(PriceControlBag.BrokerageFeeCalcEdit);
				yield return nameof(PriceControlBag.ContainerPackagingCostCalcEdit);
				yield return nameof(PriceControlBag.GoodsCostCalcEdit);
				yield return nameof(PriceControlBag.ProductToolCostsCalcEdit);
				yield return nameof(PriceControlBag.CommodityUsageCostsCalcEdit);
				yield return nameof(PriceControlBag.ProductDevCostsCalcEdit);
				yield return nameof(PriceControlBag.RoyaltyCalcEdit);
				yield return nameof(PriceControlBag.ProfitAmountCalcEdit);
				yield return nameof(PriceControlBag.ExcludingTransportationCostsCalcEdit);
				yield return nameof(PriceControlBag.FreightCalcEdit);
				yield return nameof(PriceControlBag.UnloadCostCalcEdit);
				yield return nameof(PriceControlBag.InsuranceCalcEdit);
				yield return nameof(PriceControlBag.TransportationCostCalcEdit);
				yield return nameof(PriceControlBag.TotalAdditionalAmountCalcEdit);

				yield return nameof(PriceControlBag.LocalTransportationCostCalcEdit);
				yield return nameof(PriceControlBag.TechnicalCostCalcEdit);
				yield return nameof(PriceControlBag.OtherCostsCalcEdit);
				yield return nameof(PriceControlBag.DiscountAmountCalcEdit);
				yield return nameof(PriceControlBag.TotalDeductionAmountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PriceControlBag.InstanceForDeclaration;
	}
}

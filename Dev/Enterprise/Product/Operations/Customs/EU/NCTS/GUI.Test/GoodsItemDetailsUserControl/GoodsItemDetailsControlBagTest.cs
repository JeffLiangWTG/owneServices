using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemDetailsControlBag))]
	sealed class GoodsItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GoodsItemDetailsControlBag.ItemNumberTextBox);
				yield return nameof(GoodsItemDetailsControlBag.DeclarationGoodsItemNumberTextBox);
				yield return nameof(GoodsItemDetailsControlBag.DescriptionOfGoodsTextBox);
				yield return nameof(GoodsItemDetailsControlBag.GrossWeightCalcDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.NetWeightCalcDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CommodityCodeTariffFindBox);
				yield return nameof(GoodsItemDetailsControlBag.DeclarationTypeDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CountryOfDispatchDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CountryOfOriginDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CountryOfDestinationDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.ConsigneeDocAddressControl);
				yield return nameof(GoodsItemDetailsControlBag.CommercialReferenceNumberTextBox);
				yield return nameof(GoodsItemDetailsControlBag.TransportChargesMethodOfPaymentDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.UNDangerousGoodsUserControl);
				yield return nameof(GoodsItemDetailsControlBag.CusC4NumberCodeFindBox);
				yield return nameof(GoodsItemDetailsControlBag.CustomsQuantityDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CustomsThirdQuantityDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CustomsFourthQuantityDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.SupplementaryUnitsCalcDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.CustomsValueCalcDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.TaxOrFeeDropEdit);
				yield return nameof(GoodsItemDetailsControlBag.AdditionalSupplementaryCodesUserControl);
				yield return nameof(GoodsItemDetailsControlBag.FeesUserControl);
				yield return nameof(GoodsItemDetailsControlBag.LinePriceCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GoodsItemDetailsControlBag.Instance;
	}
}

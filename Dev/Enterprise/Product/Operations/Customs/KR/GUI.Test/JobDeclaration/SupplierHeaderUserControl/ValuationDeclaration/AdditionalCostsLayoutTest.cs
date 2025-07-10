using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(AdditionalCostsLayout))]
	sealed class AdditionalCostsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new AdditionalCostsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(7, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];

			AssertEquals(6, row1.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.PurchaseCostCalcEdit), row1.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.BrokerageFeeCalcEdit), row1.Parts[3].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.ContainerPackagingCostCalcEdit), row1.Parts[5].Name);

			AssertEquals(6, row2.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.GoodsCostCalcEdit), row2.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.ProductToolCostsCalcEdit), row2.Parts[3].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.CommodityUsageCostsCalcEdit), row2.Parts[5].Name);

			AssertEquals(6, row3.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.ProductDevCostsCalcEdit), row3.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.RoyaltyCalcEdit), row3.Parts[3].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.ProfitAmountCalcEdit), row3.Parts[5].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.ExcludingTransportationCostsCalcEdit), row4.Parts[1].Name);

			AssertEquals(6, row5.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.FreightCalcEdit), row5.Parts[1].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.UnloadCostCalcEdit), row5.Parts[3].Name);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.InsuranceCalcEdit), row5.Parts[5].Name);

			AssertEquals(2, row6.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.TransportationCostCalcEdit), row6.Parts[1].Name);

			AssertEquals(2, row7.Parts.Count);
			AssertEquals(nameof(PriceControlBag.InstanceForDeclaration.TotalAdditionalAmountCalcEdit), row7.Parts[1].Name);
		}
	}
}

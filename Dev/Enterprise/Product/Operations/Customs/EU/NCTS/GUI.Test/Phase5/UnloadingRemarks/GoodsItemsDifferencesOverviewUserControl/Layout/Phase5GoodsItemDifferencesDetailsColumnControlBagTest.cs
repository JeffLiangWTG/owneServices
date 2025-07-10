using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsColumnControlBag))]
	sealed class Phase5GoodsItemDifferencesDetailsColumnControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredValueLabel);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredCommodityCodeCodeFindBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredCusCodeCodeFindBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredDescriptionTextBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredGrossWeightDropEdit);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.DeclaredNetWeightDropEdit);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedValueLabel);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedCommodityCodeCodeFindBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedCusCodeCodeFindBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedDescriptionTextBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedGrossWeightDropEdit);
				yield return nameof(Phase5GoodsItemDifferencesDetailsColumnControlBag.UnloadedNetWeightDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5GoodsItemDifferencesDetailsColumnControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDifferencesDetailsColumnControlBag))]
class GoodsItemDifferencesDetailsColumnControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(GoodsItemDifferencesDetailsColumnControlBag.DeclaredCommodityCodeCodeFindBox);
			yield return nameof(GoodsItemDifferencesDetailsColumnControlBag.UnloadedCommodityCodeCodeFindBox);
			yield return nameof(GoodsItemDifferencesDetailsColumnControlBag.UnloadingRemarkCodeDropEdit);
			yield return nameof(GoodsItemDifferencesDetailsColumnControlBag.UnloadingRemarkTextTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => GoodsItemDifferencesDetailsColumnControlBag.Instance;
}

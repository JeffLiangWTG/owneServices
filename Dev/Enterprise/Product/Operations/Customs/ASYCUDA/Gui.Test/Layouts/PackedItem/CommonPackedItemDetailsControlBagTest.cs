using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonPackedItemDetailsControlBag))]
	sealed class CommonPackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonPackedItemDetailsControlBag.TariffFindBox);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomsQtyCalcDropEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.GoodsOriginCodeFindBox);
				yield return nameof(CommonPackedItemDetailsControlBag.GoodsDescriptionTextBox);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomsValueCalcEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.TaxAmountCalcEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.DutyAmountCalcEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.MessageStatusTextBox);
				yield return nameof(CommonPackedItemDetailsControlBag.PackStatusTextBox);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomEntriesSeparatorUserControl);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomEntriesGrid);
				yield return nameof(CommonPackedItemDetailsControlBag.BrandTextBox);
				yield return nameof(CommonPackedItemDetailsControlBag.ModelTextBox);
				yield return nameof(CommonPackedItemDetailsControlBag.GrossWeightCalcDropEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.NetWeightCalcDropEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomsQty2CalcDropEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.CustomsQty3CalcDropEdit);
				yield return nameof(CommonPackedItemDetailsControlBag.GoodsValueLocalCurrencyControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonPackedItemDetailsControlBag.Instance;
	}
}

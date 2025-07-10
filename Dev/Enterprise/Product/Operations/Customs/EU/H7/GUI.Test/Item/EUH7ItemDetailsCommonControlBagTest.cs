using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemDetailsCommonControlBag))]
	sealed class EUH7ItemDetailsCommonControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUH7ItemDetailsCommonControlBag.TariffFindBox);
				yield return nameof(EUH7ItemDetailsCommonControlBag.GrossWeightCalcDropEdit);
				yield return nameof(EUH7ItemDetailsCommonControlBag.NetWeightCalcDropEdit);
				yield return nameof(EUH7ItemDetailsCommonControlBag.IntrinsicValueConvertToLocalCurrencyControl);
				yield return nameof(EUH7ItemDetailsCommonControlBag.GoodsOriginCodeFindBox);
				yield return nameof(EUH7ItemDetailsCommonControlBag.SupplementaryDropEdit);
				yield return nameof(EUH7ItemDetailsCommonControlBag.GoodsDescriptionTextBox);
				yield return nameof(EUH7ItemDetailsCommonControlBag.CustomEntriesSeparatorUserControl);
				yield return nameof(EUH7ItemDetailsCommonControlBag.CustomEntriesGrid);
				yield return nameof(EUH7ItemDetailsCommonControlBag.QuantityCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUH7ItemDetailsCommonControlBag.Instance;
	}
}

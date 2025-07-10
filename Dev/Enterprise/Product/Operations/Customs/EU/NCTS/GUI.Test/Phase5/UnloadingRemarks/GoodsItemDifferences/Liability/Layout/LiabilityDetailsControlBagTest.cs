using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(LiabilityDetailsControlBag))]
	sealed class LiabilityDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(LiabilityDetailsControlBag.CountryOfOriginDropEdit);
				yield return nameof(LiabilityDetailsControlBag.CommodityCodeTariffFindBox);
				yield return nameof(LiabilityDetailsControlBag.SupplementaryUnitsCalcDropEdit);
				yield return nameof(LiabilityDetailsControlBag.CustomsThirdQuantityDropEdit);
				yield return nameof(LiabilityDetailsControlBag.CustomsFourthQuantityDropEdit);
				yield return nameof(LiabilityDetailsControlBag.CustomsValueCalcDropEdit);
				yield return nameof(LiabilityDetailsControlBag.AdditionalSupplementaryCodesUserControl);
				yield return nameof(LiabilityDetailsControlBag.FeesUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => LiabilityDetailsControlBag.Instance;
	}
}

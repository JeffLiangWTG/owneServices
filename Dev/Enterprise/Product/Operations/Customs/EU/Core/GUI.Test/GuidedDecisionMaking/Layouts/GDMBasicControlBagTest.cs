using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GDMBasicControlBag))]
	sealed class GDMBasicControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GDMBasicControlBag.EffectiveDateDateEdit);
				yield return nameof(GDMBasicControlBag.TariffCodeFindBox);
				yield return nameof(GDMBasicControlBag.CountryOfOriginDropEdit);
				yield return nameof(GDMBasicControlBag.CountryOfDestinationDropEdit);
				yield return nameof(GDMBasicControlBag.PreferenceDropEdit);
				yield return nameof(GDMBasicControlBag.QuotaOrderNumberDropEdit);
				yield return nameof(GDMBasicControlBag.CustomsFirstQuantityCalcEdit);
				yield return nameof(GDMBasicControlBag.CustomsSecondQuantityCalcDropEdit);
				yield return nameof(GDMBasicControlBag.CustomsThirdQuantityCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GDMBasicControlBag.Instance;
	}
}


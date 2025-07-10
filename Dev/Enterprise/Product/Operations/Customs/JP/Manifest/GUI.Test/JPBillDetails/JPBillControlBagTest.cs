using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPBillControlBag))]
	sealed class JPBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(JPBillControlBag.FinalDestinationUserControl);
				yield return nameof(JPBillControlBag.GoodsLocationCodeFindBox);
				yield return nameof(JPBillControlBag.SpecialCargoCodeFindBox);
				yield return nameof(JPBillControlBag.CargoTypeDropEdit);
				yield return nameof(JPBillControlBag.TariffFindBox);
				yield return nameof(JPBillControlBag.RepresentativeHSCodeFindBox);
				yield return nameof(JPBillControlBag.GoodsOriginCodeFindBox);
				yield return nameof(JPBillControlBag.CustomsWeightCalcDropEdit);
				yield return nameof(JPBillControlBag.CustomsNetWeightCalcDropEdit);
				yield return nameof(JPBillControlBag.CustomsVolumeCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => JPBillControlBag.Instance;
	}
}

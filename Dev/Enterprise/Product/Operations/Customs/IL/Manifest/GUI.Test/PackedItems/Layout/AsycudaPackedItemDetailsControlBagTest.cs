using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaPackedItemDetailsControlBag))]
	public class AsycudaPackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AsycudaPackedItemDetailsControl.SeqCalcEdit);
				yield return nameof(AsycudaPackedItemDetailsControl.TariffCodeFindBox);
				yield return nameof(AsycudaPackedItemDetailsControl.GoodsDescriptionTextBox);
				yield return nameof(AsycudaPackedItemDetailsControl.GrossWeightCalcEdit);
				yield return nameof(AsycudaPackedItemDetailsControl.UQDropEdit);
				yield return nameof(AsycudaPackedItemDetailsControl.PackStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AsycudaPackedItemDetailsControlBag.Instance;
	}
}

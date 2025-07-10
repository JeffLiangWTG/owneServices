using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(VehicleDetailsControlBag))]
sealed class VehicleDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(VehicleDetailsControlBag.VinTextBox);
			yield return nameof(VehicleDetailsControlBag.BrandTextBox);
			yield return nameof(VehicleDetailsControlBag.ModelTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => new VehicleDetailsControlBag();
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeControlBag))]
	sealed class ShipmentTypeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ShipmentTypeControlBag.EntryStyleDropEdit);
				yield return nameof(ShipmentTypeControlBag.CTStatusIDDropEdit);
				yield return nameof(ShipmentTypeControlBag.SpecificCircumstanceDropEdit);
				yield return nameof(ShipmentTypeControlBag.IsHighValueOvrdCheckBox);
				yield return nameof(ShipmentTypeControlBag.BorderTransportMeansDropEdit);
				yield return nameof(ShipmentTypeControlBag.IsSecurityDeclarationCheckBox);
				yield return nameof(ShipmentTypeControlBag.SecurityDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ShipmentTypeControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.GUI.Testing
{
	[TestedType(typeof(PBNManifestControlBag))]
	class PBNManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PBNManifestControlBag.IsEmptyVehicleCheckBox);
				yield return nameof(PBNManifestControlBag.CarrierCodeDropEdit);
				yield return nameof(PBNManifestControlBag.CustomsReferencesGroupBox);
				yield return nameof(PBNManifestControlBag.TransitReferencesGroupBox);
				yield return nameof(PBNManifestControlBag.PersonsGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PBNManifestControlBag.Instance;
	}
}

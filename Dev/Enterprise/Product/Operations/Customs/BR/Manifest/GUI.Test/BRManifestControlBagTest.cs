using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(BRManifestControlBag))]
	sealed class BRManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(BRManifestControlBag.CustomsOwnNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => BRManifestControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	[TestedType(typeof(H7ManifestControlBag))]
	sealed class H7ManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(H7ManifestControlBag.CSPDropEdit);
				yield return nameof(H7ManifestControlBag.SupervisingOfficeAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => H7ManifestControlBag.Instance;
	}
}

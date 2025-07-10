using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(MXManifestControlBag))]
	class MXManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MXManifestControlBag.LastForeignPortCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MXManifestControlBag.Instance;
	}
}

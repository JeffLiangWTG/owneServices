using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLManifestControlBag))]
	sealed class CLManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CLManifestControlBag.IsTrampCheckBox);
				yield return nameof(CLManifestControlBag.TranshipmentTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CLManifestControlBag.Instance;
	}
}

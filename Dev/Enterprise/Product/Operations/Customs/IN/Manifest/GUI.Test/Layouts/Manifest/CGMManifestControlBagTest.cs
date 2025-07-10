using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMManifestControlBag))]
sealed class CGMManifestControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CGMManifestControlBag.ImportGeneralManifestNumberTextBox);
			yield return nameof(CGMManifestControlBag.ImportGeneralManifestDateEdit);
			yield return nameof(CGMManifestControlBag.GrossWeightCalcDropEdit);
			yield return nameof(CGMManifestControlBag.ManifestQtyCalcDropEdit);
			yield return nameof(CGMManifestControlBag.MessageAndCustomsStatusWithOverrideUserControl);
			yield return nameof(CGMManifestControlBag.ActionDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CGMManifestControlBag.Instance;
}

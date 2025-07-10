using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.GUI.Testing
{
	[TestedType(typeof(EUManifestControlBag))]
	sealed class EUManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUManifestControlBag.SpecificCircumstanceIndicatorDropEdit);
				yield return nameof(EUManifestControlBag.MethodOfPaymentDropEdit);
				yield return nameof(EUManifestControlBag.SpecialMentionsDropEdit);
				yield return nameof(EUManifestControlBag.ETAatFirstCustomsOfficeDateEdit);
				yield return nameof(EUManifestControlBag.ATAatFirstCustomsOfficeDateEdit);
				yield return nameof(EUManifestControlBag.EUCustomsOfficesUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUManifestControlBag.Instance;
	}
}

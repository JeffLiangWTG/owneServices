using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ManifestControlBag))]
	sealed class EUH7ManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUH7ManifestControlBag.CustomsOfficeLabel);
				yield return nameof(EUH7ManifestControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(EUH7ManifestControlBag.PresentationOfficeCodeFindBox);
				yield return nameof(EUH7ManifestControlBag.SubmitTypeDropEdit);
				yield return nameof(EUH7ManifestControlBag.PresenterAddressControl);
				yield return nameof(EUH7ManifestControlBag.ConsolidatedStatusSeparatorUserControl);
				yield return nameof(EUH7ManifestControlBag.ConsolidatedCustomsStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUH7ManifestControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPManifestControlBag))]
	sealed class JPManifestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(JPManifestControlBag.PortOfLoadingUserControl);
				yield return nameof(JPManifestControlBag.PortOfDischargeUserControl);
				yield return nameof(JPManifestControlBag.ConsolidatorUserControl);
				yield return nameof(JPManifestControlBag.CustomsAgentCodeFindBox);
				yield return nameof(JPManifestControlBag.CustomsAgentCredentialGuidDropEdit);
				yield return nameof(JPManifestControlBag.IsSubConsolidationCheckBox);
				yield return nameof(JPManifestControlBag.IsCoLoadedCheckBox);
				yield return nameof(JPManifestControlBag.InputReferenceTextBox);
				yield return nameof(JPManifestControlBag.BookingNumberTextBox);
				yield return nameof(JPManifestControlBag.ViaLocationCodeFindBox);
				yield return nameof(JPManifestControlBag.MoveInDestinationCodeFindBox);
				yield return nameof(JPManifestControlBag.MasterBillCustomsStatusDropEdit);
				yield return nameof(JPManifestControlBag.MasterBillMessageStatusDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => JPManifestControlBag.Instance;
	}
}

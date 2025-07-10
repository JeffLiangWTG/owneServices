using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentDetailsControlBag))]
	sealed class SupportingDocumentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SupportingDocumentDetailsControl.typeDropEdit);
				yield return nameof(SupportingDocumentDetailsControl.referenceNumberTextBox);
				yield return nameof(SupportingDocumentDetailsControl.eDocGuidDropEditGuidDropEdit);
				yield return nameof(SupportingDocumentDetailsControl.statusTextBox);
				yield return nameof(SupportingDocumentDetailsControl.additionalDescriptionTextBox);
				yield return nameof(SupportingDocumentDetailsControl.customsDocIDTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SupportingDocumentDetailsControlBag.Instance;
	}
}

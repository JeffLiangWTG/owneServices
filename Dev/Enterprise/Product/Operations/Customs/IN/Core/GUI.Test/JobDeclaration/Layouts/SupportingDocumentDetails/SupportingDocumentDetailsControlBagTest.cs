using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SupportingDocumentDetailsControlBag))]
sealed class SupportingDocumentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SupportingDocumentDetailsControlBag.ImageReferenceNumberTextBox);
			yield return nameof(SupportingDocumentDetailsControlBag.DocumentTypeCodeFindBox);
			yield return nameof(SupportingDocumentDetailsControlBag.IssuingPartyGroupBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => SupportingDocumentDetailsControlBag.Instance;
}

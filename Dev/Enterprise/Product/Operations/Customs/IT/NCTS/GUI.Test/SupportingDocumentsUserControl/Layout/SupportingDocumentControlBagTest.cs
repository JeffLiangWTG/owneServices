using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(SupportingDocumentControlBag))]
sealed class SupportingDocumentControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SupportingDocumentUserControl.YearOfIssueTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => SupportingDocumentControlBag.Instance;
}

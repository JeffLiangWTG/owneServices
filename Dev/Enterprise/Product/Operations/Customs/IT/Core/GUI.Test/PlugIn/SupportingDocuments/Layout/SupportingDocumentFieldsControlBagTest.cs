using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(SupportingDocumentFieldsControlBag))]
sealed class SupportingDocumentFieldsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SupportingDocumentFieldsControlBag.YearOfIssueTextBox);
			yield return nameof(SupportingDocumentFieldsControlBag.IssuingAuthorityTextBox);
			yield return nameof(SupportingDocumentFieldsControlBag.CountryCodeCodeFindBox);
			yield return nameof(SupportingDocumentFieldsControlBag.AvailabilityDropEdit);
			yield return nameof(SupportingDocumentFieldsControlBag.LineNoCalcEdit);
			yield return nameof(SupportingDocumentFieldsControlBag.ValueCalcFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => SupportingDocumentFieldsControlBag.Instance;
}

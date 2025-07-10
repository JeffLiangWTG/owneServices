using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentControlBag))]
	sealed class SupportingDocumentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SupportingDocumentUserControl.TypeCodeFindBox);
				yield return nameof(SupportingDocumentUserControl.ReferenceNumberTextBox);
				yield return nameof(SupportingDocumentUserControl.ComplementTextBox);
				yield return nameof(SupportingDocumentUserControl.ItemNumberCalcEdit);
				yield return nameof(SupportingDocumentUserControl.LineNoCalcEdit);
				yield return nameof(SupportingDocumentUserControl.StatusLabel);
				yield return nameof(SupportingDocumentUserControl.CountryCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SupportingDocumentControlBag.Instance;
	}
}

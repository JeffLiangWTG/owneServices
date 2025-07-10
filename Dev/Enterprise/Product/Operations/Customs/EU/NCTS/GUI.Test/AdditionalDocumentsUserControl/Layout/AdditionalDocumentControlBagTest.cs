using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(AdditionalDocumentControlBag))]
	sealed class AdditionalDocumentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AdditionalDocumentControlBag.KindDropEdit);
				yield return nameof(AdditionalDocumentControlBag.TypeCodeFindBox);
				yield return nameof(AdditionalDocumentControlBag.ReferenceNumberTextBox);
				yield return nameof(AdditionalDocumentControlBag.DescriptionTextBox);
				yield return nameof(AdditionalDocumentControlBag.DescriptionMultilineTextBox);
				yield return nameof(AdditionalDocumentControlBag.LineNoCalcEdit);
				yield return nameof(AdditionalDocumentControlBag.StatusLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AdditionalDocumentControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentControlBag))]
	sealed class PreviousDocumentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PreviousDocumentControlBag.TypeCodeFindBox);
				yield return nameof(PreviousDocumentControlBag.ReferenceNumberTextBox);
				yield return nameof(PreviousDocumentControlBag.ItemNumberCalcEdit);
				yield return nameof(PreviousDocumentControlBag.NumOfPackagesDropEdit);
				yield return nameof(PreviousDocumentControlBag.QuantityDropEdit);
				yield return nameof(PreviousDocumentControlBag.ComplementTextBox);
				yield return nameof(PreviousDocumentControlBag.LineNoCalcEdit);
				yield return nameof(PreviousDocumentControlBag.StatusTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PreviousDocumentControlBag.Instance;
	}
}


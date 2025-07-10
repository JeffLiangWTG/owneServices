using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCopyDocumentControlBag))]

	sealed class InvoiceLineCopyDocumentControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineCopyDocumentControlBag.InvoiceNumberDropEditGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineCopyDocumentControlBag.Instance;
	}
}

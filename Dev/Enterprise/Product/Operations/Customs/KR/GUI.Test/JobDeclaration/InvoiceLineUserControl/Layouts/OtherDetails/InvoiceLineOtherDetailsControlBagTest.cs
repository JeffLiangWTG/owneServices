using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineOtherDetailsControlBag))]
	sealed class InvoiceLineOtherDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineOtherDetailsControlBag.ApprovalNoTextBox);
				yield return nameof(InvoiceLineOtherDetailsControlBag.SteelExportEffectiveDateUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineOtherDetailsControlBag.Instance;
	}
}

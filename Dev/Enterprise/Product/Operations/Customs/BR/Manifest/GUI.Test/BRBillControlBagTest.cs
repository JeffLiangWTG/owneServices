using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(BRBillControlBag))]
	sealed class BRBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(BRBillControlBag.DocumentTypeDropEdit);
				yield return nameof(BRBillControlBag.ToOrderCheckBox);
				yield return nameof(BRBillControlBag.BLServiceCheckBox);
				yield return nameof(BRBillControlBag.FRTModeDropEdit);
				yield return nameof(BRBillControlBag.SellerCountryCodeFindBox);
				yield return nameof(BRBillControlBag.CEMercanteTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => BRBillControlBag.Instance;
	}
}

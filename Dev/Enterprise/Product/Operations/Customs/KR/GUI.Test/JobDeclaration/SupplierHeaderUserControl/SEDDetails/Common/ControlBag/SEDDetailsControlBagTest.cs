using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SEDDetailsControlBag))]
	sealed class SEDDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SEDDetailsControlBag.GoodsOriginCodeFindBox);
				yield return nameof(SEDDetailsControlBag.COODeterminationRuleDropEdit);
				yield return nameof(SEDDetailsControlBag.COOIssueStatusDropEdit);
				yield return nameof(SEDDetailsControlBag.COOLabelLocationDropEdit);
				yield return nameof(SEDDetailsControlBag.ManufacturerIPCCodeFindBox);
				yield return nameof(SEDDetailsControlBag.ManufacturerUnipassIDTextBox);
				yield return nameof(SEDDetailsControlBag.ManufacturerAddressControl);
				yield return nameof(SEDDetailsControlBag.ManufacturerGuidFindBox);
				yield return nameof(SEDDetailsControlBag.BuyerIDTextBox);
				yield return nameof(SEDDetailsControlBag.BuyerGuidFindBox);
				yield return nameof(SEDDetailsControlBag.SupplierGuidFindBox);
				yield return nameof(SEDDetailsControlBag.SupplierUnipassIDTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SEDDetailsControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LEXSEDDetailsControlBag))]
	sealed class LEXSEDDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(LEXSEDDetailsControlBag.Instance.CustomsOfficeCodeFindBox);
				yield return nameof(LEXSEDDetailsControlBag.Instance.CustomsDivisionCodeFindBox);
				yield return nameof(LEXSEDDetailsControlBag.Instance.SubLocationOfGoodsTextBox);
				yield return nameof(LEXSEDDetailsControlBag.Instance.BondedAreaCodeFindBox);
				yield return nameof(LEXSEDDetailsControlBag.Instance.CrewCountCalcEdit);
				yield return nameof(LEXSEDDetailsControlBag.Instance.BlanketDeclarationDropEdit);
				yield return nameof(LEXSEDDetailsControlBag.Instance.DeclarationDateEdit);
				yield return nameof(LEXSEDDetailsControlBag.Instance.GridUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => LEXSEDDetailsControlBag.Instance;
	}
}

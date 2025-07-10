using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(DepartureDetailsControlBag))]
	sealed class DepartureDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DepartureDetailsUserControl.DepartureGoodsLocationCodeFindBox);
				yield return nameof(DepartureDetailsUserControl.TNNDocumentTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DepartureDetailsControlBag.Instance;
	}
}

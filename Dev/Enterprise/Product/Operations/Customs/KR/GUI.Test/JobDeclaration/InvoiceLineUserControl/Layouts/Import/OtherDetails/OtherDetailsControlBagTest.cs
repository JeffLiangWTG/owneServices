using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(OtherDetailsControlBag))]
	sealed class OtherDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OtherDetailsControlBag.Instance.ProductDropEdit);
				yield return nameof(OtherDetailsControlBag.Instance.LineNoCalcEdit);
				yield return nameof(OtherDetailsControlBag.Instance.Agency1CodeFindBox);
				yield return nameof(OtherDetailsControlBag.Instance.Agency2CodeFindBox);
				yield return nameof(OtherDetailsControlBag.Instance.Agency3CodeFindBox);
				yield return nameof(OtherDetailsControlBag.Instance.InspectionDropEdit);
				yield return nameof(OtherDetailsControlBag.Instance.DeliveryCompanyDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => OtherDetailsControlBag.Instance;
	}
}

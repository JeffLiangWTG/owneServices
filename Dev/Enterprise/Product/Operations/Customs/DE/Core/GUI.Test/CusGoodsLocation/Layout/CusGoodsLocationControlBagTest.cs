using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationControlBag))]
	class CusGoodsLocationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusGoodsLocationControlBag.LoadingPlaceTextBox);
				yield return nameof(CusGoodsLocationControlBag.AdditionalIdentifierDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;
	}
}

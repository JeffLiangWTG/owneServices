using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationControlBag))]
	public class CusGoodsLocationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusGoodsLocationControlBag.ESAuthorizationCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;
	}
}

using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationControlBag))]
	sealed class CusGoodsLocationControlBagTest : ControlBagAbstractTest
	{
		protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusGoodsLocationControlBag.OrganisationFindBox);
				yield return nameof(CusGoodsLocationControlBag.AuthorizationCodeFindBox);
				yield return nameof(CusGoodsLocationControlBag.AdditionalIdentifierDropEdit);
			}
		}
	}
}

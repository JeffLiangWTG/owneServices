using System;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(GoodsLocationFormLayoutProvider))]
	sealed class GoodsLocationFormLayoutProviderTest : EU.GUI.Testing.GoodsLocationFormLayoutProviderAbstractTest<GoodsLocationFormLayoutProvider>
	{
		protected override Type ExpectedGoodsLocationLayout => typeof(CusGoodsLocationLayout);
	}
}

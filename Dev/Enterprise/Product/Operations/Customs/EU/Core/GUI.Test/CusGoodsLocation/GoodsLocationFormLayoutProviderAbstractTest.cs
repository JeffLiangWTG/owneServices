using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestsSubclassesOf(typeof(IGoodsLocationFormLayoutProvider))]
	public abstract class GoodsLocationFormLayoutProviderAbstractTest<TLayoutProvider> : TestCaseWithFactory
		where TLayoutProvider : IGoodsLocationFormLayoutProvider, new()
	{
		public void TestGetGoodsLocationLayout()
		{
			AssertEquals("GetGoodsLocationLayout", ExpectedGoodsLocationLayout, GetGoodsLocationFormLayoutProviderForTesting()?.GetGoodsLocationLayout()?.GetType());
		}

		protected abstract Type ExpectedGoodsLocationLayout { get; }

		protected IGoodsLocationFormLayoutProvider GetGoodsLocationFormLayoutProviderForTesting() => new TLayoutProvider();

		protected override void SetUp()
		{
			base.SetUp();
			provider = GetGoodsLocationFormLayoutProviderForTesting();
		}

		protected IGoodsLocationFormLayoutProvider provider;
	}
}

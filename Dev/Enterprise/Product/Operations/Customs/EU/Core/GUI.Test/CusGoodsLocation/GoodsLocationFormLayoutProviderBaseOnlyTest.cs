using System;
using CargoWise.Application;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GoodsLocationFormLayoutProvider))]
	sealed class GoodsLocationFormLayoutProviderBaseOnlyTest : GoodsLocationFormLayoutProviderAbstractTest<GoodsLocationFormLayoutProvider>
	{
		public void TestGetLayoutProvider_WhenInputIsInvalidOrEmpty()
		{
			var goodsLocationProviderMock = new Mock<ICusGoodsLocationProvider>();

			CombineAssertions(() =>
			{
				var provider = GoodsLocationFormLayoutProvider.GetLayoutProvider(null);
				AssertNull("Provider for NULL", provider);

				provider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<GoodsLocationFormLayoutProvider>("Provider for empty CusGoodsLocationProvider", provider);

				goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("XXX");
				provider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<GoodsLocationFormLayoutProvider>("Provider for invalid key", provider);
			});
		}

		public void TestGetLayoutProvider()
		{
			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(new GoodsLocationFormLayoutProviderForTest());

			var goodsLocationFormLayoutProviderKeyObjectHandleDictionary = new KeyObjectHandleDictionaryObject
			{
				{ "XXX", objectHandleMock.Object }
			};

			ObjectFactory.Substitute("GoodsLocationFormLayoutProviders", goodsLocationFormLayoutProviderKeyObjectHandleDictionary);

			var goodsLocationProviderMock = new Mock<ICusGoodsLocationProvider>();
			goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("XXX");

			var provider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
			AssertType<GoodsLocationFormLayoutProviderForTest>("Provider for valid key", provider);
		}

		public void TestGetLayoutProvider_WhenEuProviderExists()
		{
			var ieObjectHandleMock = new Mock<ObjectHandle>();
			ieObjectHandleMock.Setup(x => x.GetObject()).Returns(new IEGoodsLocationFormLayoutProviderForTest());

			var euObjectHandleMock = new Mock<ObjectHandle>();
			euObjectHandleMock.Setup(x => x.GetObject()).Returns(new EUGoodsLocationFormLayoutProviderForTest());

			var defaultObjectHandleMock = new Mock<ObjectHandle>();
			defaultObjectHandleMock.Setup(x => x.GetObject()).Returns(new GoodsLocationFormLayoutProviderForTest());

			var layoutProviderDictionary = new KeyObjectHandleDictionaryObject
			{
				{ "EUH7D", euObjectHandleMock.Object },
				{ "IEH7D", ieObjectHandleMock.Object },
				{ "Default", defaultObjectHandleMock.Object }
			};

			ObjectFactory.Substitute("GoodsLocationFormLayoutProviders", layoutProviderDictionary);

			var goodsLocationProviderMock = new Mock<ICusGoodsLocationProvider>();

			CombineAssertions(() =>
			{
				goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("IEH7D");
				var layoutProvider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<IEGoodsLocationFormLayoutProviderForTest>("Provider for valid key", layoutProvider);

				goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("GBH7D");
				layoutProvider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<EUGoodsLocationFormLayoutProviderForTest>("Provider for invalid GB key but EU key exists", layoutProvider);

				goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("ESH7D");
				layoutProvider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<EUGoodsLocationFormLayoutProviderForTest>("Provider for invalid ES key but EU key exists", layoutProvider);

				goodsLocationProviderMock.Setup(x => x.ProviderKey).Returns("IENCTS");
				layoutProvider = GoodsLocationFormLayoutProvider.GetLayoutProvider(goodsLocationProviderMock.Object);
				AssertType<GoodsLocationFormLayoutProviderForTest>("Provider for invalid key and EU key does not exists", layoutProvider);
			});
		}

		protected override Type ExpectedGoodsLocationLayout => typeof(CusGoodsLocationLayout);

		sealed class GoodsLocationFormLayoutProviderForTest : IGoodsLocationFormLayoutProvider
		{
			public IPanelLayoutProvider GetGoodsLocationLayout() => null;
		}

		sealed class EUGoodsLocationFormLayoutProviderForTest : IGoodsLocationFormLayoutProvider
		{
			public IPanelLayoutProvider GetGoodsLocationLayout() => null;
		}

		sealed class IEGoodsLocationFormLayoutProviderForTest : IGoodsLocationFormLayoutProvider
		{
			public IPanelLayoutProvider GetGoodsLocationLayout() => null;
		}
	}
}

using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CargoDescValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsCargoDescValidationDecider
	{
		public CargoDescValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var goodsItemsConfiguration = new Mock<GoodsItemsConfiguration> { CallBase = true };
			_ = goodsItemsConfiguration
				.Protected()
				.Setup<INctsCargoDescValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsCommonCargoDesc>())
				.Returns(validationDecider);

			_ = configuration
				.Protected()
				.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
				.Returns(goodsItemsConfiguration.Object);
		}
	}
}

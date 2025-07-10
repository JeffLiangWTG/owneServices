using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsAdditionalInfoValidationDecider
	{
		public NctsAdditionalInfoValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var goodsItemConfiguration = new Mock<GoodsItemsConfiguration> { CallBase = true };
			goodsItemConfiguration
				.Protected()
				.Setup<INctsAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);

			configuration
				.Protected()
				.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
				.Returns(goodsItemConfiguration.Object);
		}
	}
}

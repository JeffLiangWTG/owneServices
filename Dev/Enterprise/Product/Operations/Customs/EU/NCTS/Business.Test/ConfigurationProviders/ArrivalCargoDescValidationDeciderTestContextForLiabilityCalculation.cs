using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class ArrivalCargoDescValidationDeciderTestContextForLiabilityCalculation : NctsValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>
	{
		public ArrivalCargoDescValidationDeciderTestContextForLiabilityCalculation(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, INctsArrivalCargoDescPhase5ValidationDecider validationDecider)
		{
			var goodsItemsConfiguration = new Mock<GoodsItemsConfiguration> { CallBase = true };
			_ = goodsItemsConfiguration
				.Protected()
				.Setup<INctsCargoDescValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsArrivalCargoDesc>())
				.Returns(validationDecider);

			_ = goodsItemsConfiguration
				.Protected()
				.Setup<ZBool>("IsLiabilityCalculationForArrivalSupportedCore")
				.Returns(true);

			_ = configuration
				.Protected()
				.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
				.Returns(goodsItemsConfiguration.Object);
		}
	}
}

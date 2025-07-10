using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public sealed class NctsPreviousDocumentValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsPreviousDocumentValidationDecider
	{
		public NctsPreviousDocumentValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
		{
			var nctsPreviousDocumentConfiguration = new Mock<NctsPreviousDocumentConfiguration> { CallBase = true };
			nctsPreviousDocumentConfiguration
				.Protected()
				.Setup<INctsPreviousDocumentValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsPreviousDocument>())
				.Returns(validationDecider);

			var goodsItemsConfiguration = new Mock<GoodsItemsConfiguration>();
			goodsItemsConfiguration
				.Protected()
				.Setup<NctsPreviousDocumentConfiguration>("GetNewNctsPreviousDocumentConfiguration")
				.Returns(nctsPreviousDocumentConfiguration.Object);

			configuration
				.Protected()
				.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
				.Returns(goodsItemsConfiguration.Object);
		}
	}
}

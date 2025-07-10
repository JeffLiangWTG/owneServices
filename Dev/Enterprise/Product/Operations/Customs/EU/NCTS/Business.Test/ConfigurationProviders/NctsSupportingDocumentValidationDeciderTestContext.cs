using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public sealed class NctsSupportingDocumentValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
		where T : class, INctsSupportingDocumentValidationDecider
{
	public NctsSupportingDocumentValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
			: base(factory, ruleDeciderInterfaceTypes)
	{
	}

	protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
	{
		var nctsSupportingDocumentConfiguration = new Mock<NctsSupportingDocumentConfiguration> { CallBase = true };
		nctsSupportingDocumentConfiguration
			.Protected()
			.Setup<INctsSupportingDocumentValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsSupportingDocument>())
			.Returns(validationDecider);

		var goodsItemsConfiguration = new Mock<GoodsItemsConfiguration>();
		goodsItemsConfiguration
			.Protected()
			.Setup<NctsSupportingDocumentConfiguration>("GetNewNctsSupportingDocumentConfiguration")
			.Returns(nctsSupportingDocumentConfiguration.Object);

		configuration
			.Protected()
			.Setup<GoodsItemsConfiguration>("GetNewGoodsItemsConfiguration")
			.Returns(goodsItemsConfiguration.Object);
	}
}

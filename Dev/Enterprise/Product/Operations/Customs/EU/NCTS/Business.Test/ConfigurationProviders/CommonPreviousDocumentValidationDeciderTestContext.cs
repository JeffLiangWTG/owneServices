using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public class CommonPreviousDocumentValidationDeciderTestContext<T>(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	: NctsValidationDeciderTestContext<T>(factory, ruleDeciderInterfaceTypes)
	where T : class, ICommonPreviousDocumentValidationDecider
{
	protected override void SetupConfiguration(Mock<NctsConfiguration> configurationMock, T validationDecider)
	{
		var commonPreviousDocumentConfigurationMock = new Mock<CommonPreviousDocumentConfiguration> { CallBase = true };
		commonPreviousDocumentConfigurationMock
			.Protected()
			.Setup<ICommonPreviousDocumentValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CommonPreviousDocument>())
			.Returns(validationDecider);

		configurationMock
			.Protected()
			.Setup<CommonPreviousDocumentConfiguration>("GetNewCommonPreviousDocumentConfiguration")
			.Returns(commonPreviousDocumentConfigurationMock.Object);
	}
}

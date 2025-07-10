using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusTransportMeansValidationDeciderTestContext<T>(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		: NctsValidationDeciderTestContext<T>(factory, ruleDeciderInterfaceTypes)
		where T : class, ICusTransportMeansValidationDecider
	{
		protected override void SetupConfiguration(Mock<NctsConfiguration> configurationMock, T validationDecider)
		{
			var cusTransportMeansConfiguration = new Mock<CusTransportMeansConfiguration> { CallBase = true };
			cusTransportMeansConfiguration
				.Protected()
				.Setup<ICusTransportMeansValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
				.Returns(validationDecider);

			configurationMock
				.Protected()
				.Setup<CusTransportMeansConfiguration>("GetNewCusTransportMeansConfiguration")
				.Returns(cusTransportMeansConfiguration.Object);
		}
	}
}

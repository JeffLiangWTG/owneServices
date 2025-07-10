using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsContainerValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T> where T : class, INctsContainerValidationDecider
{
	public NctsContainerValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes) : base(factory, ruleDeciderInterfaceTypes)
	{
		InitializeConfiguration();
	}

	protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
	{
		var nctsContainerConfiguration = new Mock<NctsContainerConfiguration> { CallBase = true };
		nctsContainerConfiguration
			.Protected()
			.Setup<INctsContainerValidationDecider>("GetHeaderValidationDeciderCore", ItExpr.IsAny<NctsHeader>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<NctsContainerConfiguration>("GetNewNctsContainerConfiguration")
			.Returns(nctsContainerConfiguration.Object);
	}
}

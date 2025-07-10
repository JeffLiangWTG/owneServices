using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class EnRouteIncidentValidationDeciderTestContext<T> : NctsValidationDeciderTestContext<T>
	where T : class, IEnRouteIncidentValidationDecider
{
	public EnRouteIncidentValidationDeciderTestContext(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
		: base(factory, ruleDeciderInterfaceTypes)
	{
	}

	protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, T validationDecider)
	{
		var enRouteIncidentConfiguration = new Mock<EnRouteIncidentConfiguration> { CallBase = true };
		enRouteIncidentConfiguration
			.Protected()
			.Setup<IEnRouteIncidentValidationDecider>("GetValidationDeciderCore")
			.Returns(validationDecider);

		configuration
			.Protected()
			.Setup<EnRouteIncidentConfiguration>("GetNewEnRouteIncidentConfiguration")
			.Returns(enRouteIncidentConfiguration.Object);
	}
}

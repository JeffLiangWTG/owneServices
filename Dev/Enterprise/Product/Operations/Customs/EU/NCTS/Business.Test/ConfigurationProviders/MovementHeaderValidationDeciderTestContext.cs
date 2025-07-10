using System;
using CargoWise.EntityFramework;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public sealed class MovementHeaderValidationDeciderTestContext<TValidationDecider>(BusinessObjectFactory factory, params Type[] ruleDeciderInterfaceTypes)
	: NctsValidationDeciderTestContext<TValidationDecider>(factory, ruleDeciderInterfaceTypes)
	where TValidationDecider : class, INctsMovementHeaderValidationDecider
{
	protected override void SetupConfiguration(Mock<NctsConfiguration> configuration, TValidationDecider validationDecider)
	{
		var movementHeaderConfiguration = new Mock<MovementHeaderConfiguration> { CallBase = true };
		movementHeaderConfiguration
			.Protected()
			.Setup<INctsMovementHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<NctsCommonMovementHeader>())
			.Returns(validationDecider);

		configuration
			.Protected()
			.Setup<MovementHeaderConfiguration>("GetNewMovementHeaderConfiguration")
			.Returns(movementHeaderConfiguration.Object);
	}
}

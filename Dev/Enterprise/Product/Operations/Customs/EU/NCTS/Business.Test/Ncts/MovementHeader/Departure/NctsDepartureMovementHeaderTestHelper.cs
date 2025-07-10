using System;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public static class NctsDepartureMovementHeaderTestHelper
{
	public static MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase4ValidationDecider>
		CreateDeparturePhase4ValidationTestContext(this NctsDepartureMovementHeader movementHeader, params Type[] ruleDeciderInterfaceTypes)
	{
		Assert("NCTS Movement Header must not be in Phase4 to create a Phase4 test context", movementHeader.IsPhase4);
		return movementHeader.CreateDepartureValidationTestContext<INctsDepartureMovementHeaderPhase4ValidationDecider>(ruleDeciderInterfaceTypes);
	}

	public static MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>
		CreateDeparturePhase5ValidationTestContext(this NctsDepartureMovementHeader movementHeader, params Type[] ruleDeciderInterfaceTypes)
	{
		Assert("NCTS Movement Header must not be in Phase5 to create a Phase5 test context", movementHeader.IsPhase5);
		return movementHeader.CreateDepartureValidationTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(ruleDeciderInterfaceTypes);
	}

	public static MovementHeaderValidationDeciderTestContext<TNctsDepartureMovementHeaderValidationDecider>
		CreateDepartureValidationTestContext<TNctsDepartureMovementHeaderValidationDecider>(this NctsDepartureMovementHeader movementHeader, params Type[] ruleDeciderInterfaceTypes)
		where TNctsDepartureMovementHeaderValidationDecider : class, INctsDepartureMovementHeaderValidationDecider
	{
		var result = new MovementHeaderValidationDeciderTestContext<TNctsDepartureMovementHeaderValidationDecider>(movementHeader.Factory, ruleDeciderInterfaceTypes);
		result.AddAutoCacheResetObject(movementHeader);
		if (movementHeader.Header is NctsHeader nctsHeader)
		{
			result.AddAutoCacheResetObject(nctsHeader);
		}
		return result;
	}

	public static NctsConfigurationTestContext CreateConfigurationTestContext(this NctsDepartureMovementHeader movementHeader)
	{
		var result = new NctsConfigurationTestContext(movementHeader.Factory);
		result.AddAutoCacheResetObject(movementHeader);
		if (movementHeader.Header is NctsHeader nctsHeader)
		{
			result.AddAutoCacheResetObject(nctsHeader);
		}
		return result;
	}
}

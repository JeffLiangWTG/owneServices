using System;

namespace Enterprise.Customs.EU.NCTS.Business;

public static class NctsCommonMovementHeaderValidationExtensions
{
	public static bool IsDeparturePhase5RuleActive(this NctsDepartureMovementHeader movementHeader, Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool> isActive)
		=> movementHeader.IsRuleActive(isActive);

	public static bool IsRuleActive<TValidationDecider>(this NctsCommonMovementHeader movementHeader, Func<TValidationDecider, bool> isActive)
		where TValidationDecider : class, INctsMovementHeaderValidationDecider
		=> movementHeader?.ValidationDecider is TValidationDecider validationDecider && isActive(validationDecider);
}


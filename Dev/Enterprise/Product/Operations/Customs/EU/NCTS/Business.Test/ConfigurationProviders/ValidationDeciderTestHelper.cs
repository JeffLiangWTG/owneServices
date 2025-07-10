using System;
using System.Linq.Expressions;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	static class ValidationDeciderTestHelper
	{
		public static MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> InactiveForDepartureMovementHeaderPhase5(BusinessObjectFactory factory, Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule)
		{
			return CreateForDepartureMovementHeaderPhase5(factory, rule, false);
		}

		public static MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> ActiveForDepartureMovementHeaderPhase5(BusinessObjectFactory factory, Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule)
		{
			return CreateForDepartureMovementHeaderPhase5(factory, rule, true);
		}

		public static MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider> CreateForDepartureMovementHeaderPhase5(BusinessObjectFactory factory, Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule, bool active)
		{
			return CreateForMovementHeader(factory, (rule, active));
		}

		static MovementHeaderValidationDeciderTestContext<T> CreateForMovementHeader<T>(BusinessObjectFactory factory, params (Expression<Func<T, bool>> ruleProperty, bool value)[] rules)
			where T : class, INctsMovementHeaderValidationDecider
		{
			var result = new MovementHeaderValidationDeciderTestContext<T>(factory);
			rules.ForEach(x =>
			{
				if (x.value)
				{
					result.EnableRule(x.ruleProperty);
				}
				else
				{
					result.DisableRule(x.ruleProperty);
				}
			});
			return result;
		}
	}
}

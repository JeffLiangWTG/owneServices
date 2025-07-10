using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native.Common.Behaviours
{
	static class BehaviourApplicator
	{
		const string DateRangeUpdate = "DateRangeUpdate";
		const string RateEntryDateRangeUpdate = "RateEntryDateRangeUpdate";
		const string OrgRateTariffLevelDateRangeUpdate = "OrgRateTariffLevelDateRangeUpdate";

		internal static void ApplyBehaviour(
			List<IEntity> entities,
			IRowRepository rowRepository,
			AncillaryImportServices sessionServices,
			ERConverter converter,
			IEntityContext context)
		{
			foreach (var behaviourGroup in entities.GroupBy(x => x.Definition.Behaviour))
			{
				IEntityBehaviour behaviour = null;
				if (behaviourGroup.Key == DateRangeUpdate)
				{
					behaviour = new DateRangeUpdateBehaviour(sessionServices);
				}
				else if (behaviourGroup.Key == RateEntryDateRangeUpdate)
				{
					behaviour = new RateEntryDateRangeUpdateBehaviour(sessionServices);
				}
				else if (behaviourGroup.Key == OrgRateTariffLevelDateRangeUpdate)
				{
					behaviour = new OrgRateTariffLevelDateRangeUpdateBehaviour(sessionServices);
				}

				if (behaviour != null)
				{
					var applicableEntities = behaviourGroup.Where(x => behaviour.CanBeAppliedToAction(x.Action));
					if (applicableEntities.Any())
					{
						behaviour.Apply(new BehaviourContext(applicableEntities, rowRepository, converter, context,
							behaviourGroup.Where(x => x.Action == EntityAction.DELETE)));
					}
				}
			}
		}
	}
}

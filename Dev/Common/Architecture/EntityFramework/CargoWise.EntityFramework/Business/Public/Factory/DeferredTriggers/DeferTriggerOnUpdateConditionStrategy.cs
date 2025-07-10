using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public abstract class DeferTriggerOnUpdateConditionStrategy : IDeferTriggerConditionStrategy
	{
		protected DeferTriggerOnUpdateConditionStrategy()
		{
		}

		TriggerRunType IDeferTriggerConditionStrategy.RunType => TriggerRunType.InsertOrUpdate;

		bool IDeferTriggerConditionStrategy.ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var hasRelevantColumnChanges = businessEntity.IsInDatabase && ColumnsThatRequireTriggerDeferralWhenChanged.Any(c => DoesColumnHaveChanges(businessEntity, c));
			var shouldDeferDueToInsert = ShouldInsertsBeDeferred && !businessEntity.IsInDatabase;

			return (hasRelevantColumnChanges || shouldDeferDueToInsert) && ShouldDeferTrigger(businessEntity);
		}

		bool DoesColumnHaveChanges(BusinessObject businessEntity, SchemaColumn column)
		{
			var propertyInfo = businessEntity.ZPropertyInfoHash[column.Name];
			return propertyInfo.IsPersistent && propertyInfo.HasChanges;
		}

		protected virtual bool ShouldDeferTrigger(BusinessObject businessEntity) => true;
		protected virtual bool ShouldInsertsBeDeferred => true;

		protected abstract IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged { get; }
	}
}

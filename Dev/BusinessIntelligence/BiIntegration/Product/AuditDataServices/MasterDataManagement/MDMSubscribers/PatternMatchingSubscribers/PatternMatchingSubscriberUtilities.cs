using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class PatternMatchingSubscriberUtilities<T> : IPatternMatchingSubscriberUtilities where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		public bool CreatePatternMatchingBusinessObject(BusinessObjectFactory factory, ZGuid masterId, ZInt hashedValue, ZString parentTableCode, SchemaGuidColumn parentIdColumn, ZGuid parentId, ZString countryCode)
		{
			var storedPatternMatchingObjects = factory.Load<T>(new ZQuery(parentIdColumn, parentId));

			if (storedPatternMatchingObjects != null && storedPatternMatchingObjects.Length == 0)
			{
				var patternMatchingObject = factory.New<T>();

				SetMasterId(patternMatchingObject, masterId);
				patternMatchingObject.HashedValue = hashedValue;
				patternMatchingObject.ParentTableCode = parentTableCode;
				patternMatchingObject.ParentId = parentId;
				patternMatchingObject.PatternMatchingCountryCode = countryCode;
				patternMatchingObject.IsActive = true;

				return true;
			}

			return false;
		}

		public bool UpdatePatternMatchingBusinessObject(BusinessObjectFactory factory, ZGuid parentId, SchemaGuidColumn parentIdColumn, SchemaIntColumn hashedValueColumn, ZInt hashedValue, ZInt originalHashedValue, ZGuid masterId, ZString parentTableCode, ZString countryCode)
		{
			var shouldSave = false;
			var updatePattrenQuery = new ZQuery(parentIdColumn, parentId);
			updatePattrenQuery.AddToFilter(JoinCondition.And, hashedValueColumn, originalHashedValue);
			var patternMatchingObjects = factory.Load<T>(updatePattrenQuery);

			if (patternMatchingObjects.Length == 0)
			{
				shouldSave |= CreatePatternMatchingBusinessObject(factory, masterId, hashedValue, parentTableCode, parentIdColumn, parentId, countryCode);
			}
			else
			{
				foreach (var pattern in patternMatchingObjects)
				{
					SetMasterId(pattern, masterId);
					pattern.HashedValue = hashedValue;
					shouldSave = true;
				}
			}

			return shouldSave;
		}

		public bool UpdatePatternMatchingCountryCode(BusinessObjectFactory factory, SchemaGuidColumn parentIdColumn, SchemaGuidColumn masterIDColumn, ZGuid parentId, BusinessObject master, ZString newCountryCode)
		{
			return master != null
					? UpdateRelatedPatternMatchingBusinessObjects(factory, masterIDColumn, master.PK, newCountryCode)
					: UpdateRelatedPatternMatchingBusinessObjects(factory, parentIdColumn, parentId, newCountryCode);
		}

		public bool DeletePatternMatchingBusinessObject(BusinessObjectFactory factory, ZGuid parentId, SchemaGuidColumn parentIdColumn, SchemaIntColumn hashedValueColumn, ZInt hashedValue)
		{
			var deletePattrenQuery = new ZQuery(parentIdColumn, parentId);
			deletePattrenQuery.AddToFilter(JoinCondition.And, hashedValueColumn, hashedValue);

			var patternMatchingObject = factory.Load<T>(deletePattrenQuery);

			if (patternMatchingObject != null && patternMatchingObject.Length > 0)
			{
				foreach (var pattern in patternMatchingObject)
				{
					pattern.Delete();
				}

				return true;
			}

			return false;
		}

		static bool UpdateRelatedPatternMatchingBusinessObjects(BusinessObjectFactory factory, SchemaGuidColumn column, ZGuid pk, ZString newCountryCode)
		{
			var shouldSave = false;
			var bizOs = factory.Load<T>(new ZQuery(column, pk));

			foreach (var bizo in bizOs)
			{
				if (bizo.PatternMatchingCountryCode != newCountryCode)
				{
					bizo.PatternMatchingCountryCode = newCountryCode;
					shouldSave = true;
				}
			}

			return shouldSave;
		}

		protected abstract void SetMasterId(T patternMatchingObjects, ZGuid id);

		public abstract SchemaGuidColumn PatternMatchingAddressMasterIdColumn { get; }

		public abstract SchemaGuidColumn PatternMatchingDomainMasterIdColumn { get; }

		public abstract SchemaGuidColumn PatternMatchingEmailMasterIdColumn { get; }

		public abstract SchemaGuidColumn PatternMatchingNameMasterIdColumn { get; }

		public abstract SchemaGuidColumn PatternMatchingPhoneMasterIdColumn { get; }

		public abstract SchemaGuidColumn PatternMatchingRegCodeMasterIdColumn { get; }
	}
}

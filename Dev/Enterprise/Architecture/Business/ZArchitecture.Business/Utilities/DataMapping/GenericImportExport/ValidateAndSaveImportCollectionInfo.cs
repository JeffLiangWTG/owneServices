using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ValidateAndSaveImportCollectionInfo<T> : ImportCollectionInfoImpl where T : BusinessObject
	{
		public ValidateAndSaveImportCollectionInfo()
			: base(new ActiveBusinessObjectCollection<T>(new BusinessObjectFactory(), new AnythingIsFine()))
		{
			ValidateAndSave = true;
		}

		public void Add(string propertyName, ZCharacterCasing casing = ZCharacterCasing.Normal)
		{
			Add(new ImportPropertyInfoImpl<T>(propertyName) { CharacterCasing = casing });
		}

		class AnythingIsFine : CollectionRelationship
		{
			public AnythingIsFine()
				: base(typeof(T))
			{ }

			internal HashSet<ZGuid> collectionPKs = new HashSet<ZGuid>();

			protected override bool MatchesRelationshipFilterCore(BusinessObject businessObject, bool ignoreActiveFilter, bool fetchOnlyFromLocalCache)
			{
				return collectionPKs.Contains(businessObject.PK) && base.MatchesRelationshipFilterCore(businessObject, ignoreActiveFilter, fetchOnlyFromLocalCache);
			}

			protected override bool SupportsAddToRelationshipCore()
			{
				return true;
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				collectionPKs.Add(businessObject.PK);
			}

			protected override void RemoveFromRelationship(BusinessObject businessObject)
			{
				collectionPKs.Remove(businessObject.PK);
			}
		}
	}
}
